using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Thermo;
using Chemistry;
using Proteomics;
using System.IO;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using MassSpectrometry;
using MathNet.Numerics.Statistics;

namespace ProteoformSuiteInternal
{
    public class Calibration
    {
        //parameters
        public double fineResolution = 0.1;

        //RAW LOCK MASS 
        public void raw_lock_mass(string filename, string raw_file_path)
        {
            var tol = .01;
            var Compound1 = new Peptide("NNNNN");
            var regularMZ = IsotopicDistribution.GetDistribution(Compound1.GetChemicalFormula(), 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();
            var withAmmoniaLoss = Compound1.GetChemicalFormula();
            withAmmoniaLoss.Add(ChemicalFormula.ParseFormula("N-1H-2"));
            var withAmmoniaLossMZ = IsotopicDistribution.GetDistribution(withAmmoniaLoss, 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();
            var deamidated = Compound1.GetChemicalFormula();
            deamidated.Add(ChemicalFormula.ParseFormula("H-1N-1O"));
            var deamidatedMZ = IsotopicDistribution.GetDistribution(deamidated, 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();

            List<List<double>> allDistributions = new List<List<double>>() { regularMZ, withAmmoniaLossMZ, deamidatedMZ };
            using (ThermoDynamicData myMsDataFile = ThermoDynamicData.InitiateDynamicConnection(raw_file_path))
            {
                foreach (var scan in myMsDataFile)
                {
                    if (scan.MsnOrder == 1)
                    {
                        double bestIntensity = 0;
                        double monoError = double.NaN;
                        foreach (var dist in allDistributions)
                        {
                            ThermoMzPeak monoisotopicPeak = null;
                            try { monoisotopicPeak = scan.MassSpectrum.Extract(dist[0] - tol, dist[0] + tol).OrderByDescending(p => p.Y).First(); }
                            catch { }
                            if (monoisotopicPeak != null && bestIntensity < monoisotopicPeak.Intensity)
                            {
                                bestIntensity = monoisotopicPeak.Intensity;
                                monoError = monoisotopicPeak.Mz - dist[0];
                            }
                        }
                        MsScan get_scan = SaveState.lollipop.Ms_scans.Where(s => s.filename == filename && s.scan_number == scan.OneBasedScanNumber).ToList().First();
                        get_scan.lock_mass_shift = monoError;
                    }
                }
            }
        }


        //CALIBRATION WITH TD HITS
        private int numMs1MassChargeCombinationsConsidered;
        private int numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks;
        private IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile;
        private double mass_tolerance;

        public bool Run_TdMzCal(InputFile file, List<TopDownHit> identifications, bool td_file, double mass_tolerance)
        {
            var trainingPointCounts = new List<int>();

            myMsDataFile = ThermoStaticData.LoadAllStaticData(file.complete_path);
            this.mass_tolerance = mass_tolerance;
            DataPointAquisitionResults dataPointAcquisitionResult = null;
            for (int linearCalibrationRound = 1; linearCalibrationRound < 4 ; linearCalibrationRound++)
            {
                dataPointAcquisitionResult = GetDataPoints(identifications);
                //if (linearCalibrationRound >= 3 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[linearCalibrationRound - 2])
                //    break;

                trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
              //  if (dataPointAcquisitionResult.Ms1List.Count == 0) return false;
                CalibrationFunction calibrationFunction = CalibrateLinear(file.filename, dataPointAcquisitionResult);
            }

            //trainingPointCounts = new List<int>();
            //for (int forestCalibrationRound = 1; ; forestCalibrationRound++)
            //{
            //    CalibrationFunction calibrationFunction = CalibrateRF(dataPointAcquisitionResult, file.filename);
            //    dataPointAcquisitionResult = GetDataPoints(identifications);
            //    if (forestCalibrationRound >= 2 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[forestCalibrationRound - 2])
            //        break;
            //    trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
            //}
            return true;
        }

        private CalibrationFunction CalibrateRF(DataPointAquisitionResults res, string filename)
        {
            var rnd = new Random();

            var shuffledMs1TrainingPoints = res.Ms1List.OrderBy(item => rnd.Next()).ToList();

            var trainList1 = shuffledMs1TrainingPoints.Take((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();
            var testList1 = shuffledMs1TrainingPoints.Skip((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.GetMSE(testList1);
            List<bool[]> boolStuffms1 = new List<bool[]>
            {
                new bool[] {true, true, false, false},
                new bool[] {true, true, true, true},
            };
            if (trainList1.Count > 0)
                foreach (var boolStuff in boolStuffms1)
                {
                    var ms1regressorRF = new RandomForestCalibrationFunction(40, 10, boolStuff);
                    ms1regressorRF.Train(trainList1);
                    var MS1mse = ms1regressorRF.GetMSE(testList1);
                    if (MS1mse < bestMS1MSE)
                    {
                        bestMS1MSE = MS1mse;
                        bestMS1predictor = ms1regressorRF;
                    }
                }

            CalibrationFunction bestCf = bestMS1predictor;

            CalibrateHitsAndComponents(bestCf, filename);
            return bestCf;
        }


        public void CalibrateHitsAndComponents(CalibrationFunction bestCf, string filename)
        {
            Parallel.ForEach(SaveState.lollipop.td_hits_calibration.Where(h => h.filename == filename), hit =>
             {
                 hit.mz = hit.mz - bestCf.Predict(new double[] { hit.mz, hit.retention_time, hit.ms1Scan.TIC, hit.ms1Scan.injection_time });
                 hit.corrected_mass = hit.mz * hit.charge - hit.charge * Lollipop.PROTON_MASS;
             });
            Parallel.ForEach(SaveState.lollipop.calibration_components.Where(h => h.input_file.filename == filename), c =>
            {
                MsScan ms1 = SaveState.lollipop.Ms_scans.Where(s => s.filename == filename && s.scan_number == Convert.ToInt16(c.scan_range.Split('-')[0])).First();
                foreach (ChargeState cs in c.charge_states)
                {
                    cs.mz_centroid = cs.mz_centroid - bestCf.Predict(new double[] { cs.mz_centroid, Convert.ToDouble(c.rt_range.Split('-')[0]), ms1.TIC, ms1.injection_time });
                }
            });
            foreach (var a in myMsDataFile.Where(s => s.MsnOrder == 1))
            {
                Func<IMzPeak, double> theFunc = x => x.Mz - bestCf.Predict(new double[] { x.Mz, a.RetentionTime, a.TotalIonCurrent, a.InjectionTime ?? double.NaN });
                a.TransformByApplyingFunctionToSpectra(theFunc);
            }
        }

        private CalibrationFunction CalibrateLinear(string filename, DataPointAquisitionResults res)
        {
            var rnd = new Random();

            var shuffledMs1TrainingPoints = res.Ms1List.OrderBy(item => rnd.Next()).ToList();

            var trainList1 = shuffledMs1TrainingPoints.Take((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();
            var testList1 = shuffledMs1TrainingPoints.Skip((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.GetMSE(testList1);

            {
                var ms1regressor = new ConstantCalibrationFunction();
                var ms2regressor = new ConstantCalibrationFunction();
                ms1regressor.Train(trainList1);
                double MS1mse = ms1regressor.GetMSE(testList1);
                if (MS1mse < bestMS1MSE)
                {
                    bestMS1MSE = MS1mse;
                    bestMS1predictor = ms1regressor;
                }
            }

            // NOT b[2]. It is intensity!!!
            var transforms = new List<TransformFunction>
            {
                new TransformFunction(b => new double[] { b[0] }, 1),
                new TransformFunction(b => new double[] { b[1] }, 1),
                new TransformFunction(b => new double[] { Math.Log(b[2]) }, 1),
                new TransformFunction(b => new double[] { Math.Log(b[3]) }, 1),

                new TransformFunction(b => new double[] { b[0], b[1] }, 2),
                new TransformFunction(b => new double[] { b[0], Math.Log(b[2]) }, 2),
                new TransformFunction(b => new double[] { b[0], Math.Log(b[3]) }, 2),
                new TransformFunction(b => new double[] { b[1], Math.Log(b[2]) }, 2),
                new TransformFunction(b => new double[] { b[1], Math.Log(b[3]) }, 2),
                new TransformFunction(b => new double[] { Math.Log(b[2]), Math.Log(b[3]) }, 2),

                new TransformFunction(b => new double[] { b[0], b[1], Math.Log(b[2]) }, 3),
                new TransformFunction(b => new double[] { b[0], b[1], Math.Log(b[3]) }, 3),
                new TransformFunction(b => new double[] { b[0], Math.Log(b[2]), Math.Log(b[3]) }, 3),
                new TransformFunction(b => new double[] { b[1], Math.Log(b[2]), Math.Log(b[3]) }, 3),

                new TransformFunction(b => new double[] { b[0], b[1], Math.Log(b[2]), Math.Log(b[3]) }, 4),
            };
            foreach (var transform in transforms)
            {
                try
                {
                    var ms1regressorLinear = new LinearCalibrationFunctionMathNet(transform);
                    ms1regressorLinear.Train(trainList1);
                    var MS1mse = ms1regressorLinear.GetMSE(testList1);
                    if (MS1mse < bestMS1MSE)
                    {
                        bestMS1MSE = MS1mse;
                        bestMS1predictor = ms1regressorLinear;
                    }
                }
                catch
                {
                }
            }
            CalibrateHitsAndComponents(bestMS1predictor, filename);
            return bestMS1predictor;
        }

        private DataPointAquisitionResults GetDataPoints(List<TopDownHit> identifications)
        {
            DataPointAquisitionResults res = new DataPointAquisitionResults()
            {
                Ms1List = new List<LabeledMs1DataPoint>()
            };

            // Set of peaks, identified by m/z and retention time. If a peak is in here, it means it has been a part of an accepted identification, and should be rejected
            var peaksAddedFromMS1HashSet = new HashSet<Tuple<double, double>>();
            foreach(TopDownHit identification in identifications)
            {
                int ms2scanNumber = identification.ms2ScanNumber;
                int proteinCharge = identification.charge;

                var SequenceWithChemicalFormulas = identification.GetSequenceWithChemicalFormula();
                if (SequenceWithChemicalFormulas == null) continue;
                Proteomics.Peptide coolPeptide = new Proteomics.Peptide(SequenceWithChemicalFormulas);

                // Calculate isotopic distribution of the full peptide

                var dist = IsotopicDistribution.GetDistribution(coolPeptide.GetChemicalFormula(), 0.1, 0.001);

                double[] masses = dist.Masses.ToArray();
                double[] intensities = dist.Intensities.ToArray();

                Array.Sort(intensities, masses, Comparer<double>.Create((x, y) => y.CompareTo(x)));

                numMs1MassChargeCombinationsConsidered = 0;
                numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks = 0;

                res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, ms2scanNumber, -1, peaksAddedFromMS1HashSet, identification.charge, identification));
                res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, ms2scanNumber, 1, peaksAddedFromMS1HashSet, identification.charge, identification));
                res.numMs1MassChargeCombinationsConsidered += numMs1MassChargeCombinationsConsidered;
                res.numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks += numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks;
            }
           return res;
        }

        private IEnumerable<LabeledMs1DataPoint> SearchMS1Spectra(double[] originalMasses, double[] originalIntensities, int ms2spectrumIndex, int direction, HashSet<Tuple<double, double>> peaksAddedHashSet, int peptideCharge, TopDownHit identification)
        {
            var theIndex = -1;
            if (direction == 1)
                theIndex = ms2spectrumIndex;
            else
                theIndex = ms2spectrumIndex - 1;

            bool addedAscan = true;

            int highestKnownChargeForThisPeptide = peptideCharge;
            while (theIndex >= 1 && theIndex <= myMsDataFile.NumSpectra && addedAscan)
            {
                int countForThisScan = 0;
                if (myMsDataFile.GetOneBasedScan(theIndex).MsnOrder > 1)
                {
                    theIndex += direction;
                    continue;
                }
                addedAscan = false;
                var fullMS1scan = myMsDataFile.GetOneBasedScan(theIndex);
                double ms1RetentionTime = fullMS1scan.RetentionTime;
                var scanWindowRange = fullMS1scan.ScanWindowRange;
                var fullMS1spectrum = fullMS1scan.MassSpectrum;
                if (fullMS1spectrum.Size == 0)
                    break;

                for (int i = -1; i <= 1; i += 2)
                {
                    bool startingToAddCharges = false;
                    bool continueAddingCharges = false;
                    int chargeToLookAt = direction > 0 ? identification.charge : (identification.charge - 1);
                    do
                    {
                        if (originalMasses[0].ToMz(chargeToLookAt) > scanWindowRange.Maximum)
                        {
                            chargeToLookAt++;
                            continue;
                        }
                        if (originalMasses[0].ToMz(chargeToLookAt) < scanWindowRange.Minimum)
                            break;
                        var trainingPointsToAverage = new List<LabeledMs1DataPoint>();
                        foreach (double a in originalMasses)
                        {
                            double theMZ = a.ToMz(chargeToLookAt);

                            var npwr = fullMS1spectrum.NumPeaksWithinRange(theMZ - (mass_tolerance / identification.charge), theMZ + (mass_tolerance / identification.charge));
                            if (npwr == 0)
                            {
                                break;
                            }
                            numMs1MassChargeCombinationsConsidered++;
                            if (npwr > 1)
                            {
                                numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks++;
                                continue;
                            }

                            var closestPeak = fullMS1spectrum.GetClosestPeak(theMZ);
                            var closestPeakMZ = closestPeak.Mz;

                            var theTuple = Tuple.Create(closestPeakMZ, ms1RetentionTime);
                            if (!peaksAddedHashSet.Contains(theTuple))
                            {
                                peaksAddedHashSet.Add(theTuple);
                                highestKnownChargeForThisPeptide = Math.Max(highestKnownChargeForThisPeptide, chargeToLookAt);
                                trainingPointsToAverage.Add(new LabeledMs1DataPoint(closestPeakMZ, double.NaN, double.NaN, double.NaN, closestPeakMZ - theMZ, null));
                            }
                            else
                                break;
                        }
                        // If started adding and suddnely stopped, go to next one, no need to look at higher charges
                        if (trainingPointsToAverage.Count == 0 && startingToAddCharges == true)
                        {
                            break;
                        }
                        if ((trainingPointsToAverage.Count == 0 || (trainingPointsToAverage.Count == 1 && originalIntensities[0] < 0.65)) && (peptideCharge <= chargeToLookAt))
                        {
                            break;
                        }
                        if (trainingPointsToAverage.Count < Math.Min(5, originalIntensities.Count()))
                        {
                        }
                        else
                        {
                            continueAddingCharges = true;
                            addedAscan = true;
                            startingToAddCharges = true;
                            countForThisScan++;
                            yield return new LabeledMs1DataPoint(trainingPointsToAverage.Select(b => b.mz).Average(),
                                                                 fullMS1scan.RetentionTime,
                                                                 fullMS1scan.TotalIonCurrent,
                                                                 fullMS1scan.InjectionTime,
                                                                 trainingPointsToAverage.Select(b => b.Label).Median(),
                                                                 identification);
                        }
                        chargeToLookAt += i;
                    } while (continueAddingCharges);
                }
                theIndex += direction;
            }
        }

            //READ AND WRITE NEW CALIBRATED TD HITS FILE 
            public void calibrate_td_hits_file(InputFile file)
        {
            //Copy file to new worksheet
            string old_absolute_path = file.complete_path;
            string new_absolute_path = file.directory + "\\" + file.filename + "_calibrated" + file.extension;

            //create copy of excel file
            byte[] byteArray = File.ReadAllBytes(old_absolute_path);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, (int)byteArray.Length);
                File.WriteAllBytes(new_absolute_path, stream.ToArray());
            }
            // Get Data in Sheet1 of Excel file
            var workbook = new XLWorkbook(new_absolute_path);
            var worksheet = workbook.Worksheets.Worksheet(1);
            List<IXLRow> rows_to_delete = new List<IXLRow>();
            Parallel.ForEach(worksheet.Rows(), row =>
            {
                if (row.RowNumber() != 1)
                {
                    if (SaveState.lollipop.td_calibration_functions.ContainsKey(row.Cell(15).Value.ToString().Split('.')[0]))
                    {
                        row.Cell(17).SetValue(SaveState.lollipop.td_hit_correction[new Tuple<string, int, double>(row.Cell(15).Value.ToString().Split('.')[0], Convert.ToInt16(row.Cell(18).GetDouble()), row.Cell(17).GetDouble())]);
                    }
                    //if hit's file not calibrated (not enough calibration points, remove from list
                    else
                    {
                        lock (rows_to_delete) rows_to_delete.Add(row);
                    }
                }
            });
            foreach (IXLRow row in rows_to_delete) row.Delete(); //can't parallelize
            workbook.Save();
        }

        //READ AND WRITE NEW CALIBRATED RAW EXPERIMENTAL COMPONENTS FILE
        public void calibrate_components_in_xlsx(InputFile file)
        {
            if ((!SaveState.lollipop.calibrate_td_results && !SaveState.lollipop.calibrate_intact_with_td_ids) || SaveState.lollipop.td_calibration_functions.ContainsKey(file.filename))
            {
                //Copy file to new worksheet
                string old_absolute_path = file.complete_path;
                string new_absolute_path = file.directory + "\\" + file.filename + "_calibrated" + file.extension;

                //create copy of excel file
                byte[] byteArray = File.ReadAllBytes(old_absolute_path);
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(byteArray, 0, (int)byteArray.Length);
                    File.WriteAllBytes(new_absolute_path, stream.ToArray());
                }
                var workbook = new XLWorkbook(new_absolute_path);
                var worksheet = workbook.Worksheets.Worksheet(1);
                Parallel.ForEach(worksheet.Rows(), row =>
                {
                    if (row.Cell(1).Value.ToString().Length == 0)
                    {
                        if (Regex.IsMatch(row.Cell(2).Value.ToString(), @"^\d+$"))
                        {
                            Tuple<double, double, int, int> value;
                            if (SaveState.lollipop.file_mz_correction.TryGetValue(new Tuple<string, double>(file.filename, Math.Round(row.Cell(3).GetDouble(), 0)), out value))
                                row.Cell(4).SetValue(value.Item1);
                            row.Cell(6).SetValue(value.Item2);
                            row.Cell(7).SetValue(value.Item3);
                            row.Cell(8).SetValue(value.Item4);
                        }
                        else
                        {
                            row.Cell(6).SetValue("Signal to Noise");
                            row.Cell(7).SetValue("Isotopic peaks left of averagine");
                            row.Cell(8).SetValue("Isotopic peaks right of averageine");
                        }
                    }
                });
                workbook.Save();
            }
        }

        public double get_correction_factor(string filename, string scan_range)
        {
            if (SaveState.lollipop.correctionFactors == null) return 0D;
            int[] scans = new int[2] { 0, 0 };
            try
            {
                scans = Array.ConvertAll<string, int>(scan_range.Split('-').ToArray(), int.Parse);
            }
            catch
            { }

            if (scans[0] <= 0 || scans[1] <= 0) return 0D;

            IEnumerable<double> allCorrectionFactors =
                (from s in SaveState.lollipop.correctionFactors
                 where s.file_name == filename
                 where s.scan_number >= scans[0]
                 where s.scan_number <= scans[1]
                 select s.correction).ToList();

            if (allCorrectionFactors.Count() <= 0) return 0D;
            return allCorrectionFactors.Average();
        }
    }
}

