using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Thermo;
using Chemistry;
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

        //CALIBRATION WITH TD HITS
        private int numMs1MassChargeCombinationsConsidered;
        private int numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks;
        private IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile;

        public bool Run_TdMzCal(InputFile file, List<TopDownHit> identifications, bool td_file, int bio_rep, int fraction, int tech_rep)
        {

            if (identifications.Count < 5) return false;

            var trainingPointCounts = new List<int>();

            myMsDataFile = ThermoStaticData.LoadAllStaticData(file.complete_path);
            DataPointAquisitionResults dataPointAcquisitionResult = null;
            Parallel.ForEach(identifications, h => h.mz = h.reported_mass.ToMz(h.charge)); //need to reset m/z in case same td hits used for multiple calibration raw files... 
            for (int linearCalibrationRound = 1; ; linearCalibrationRound++)
            {
                dataPointAcquisitionResult = GetDataPoints(identifications, td_file, bio_rep, fraction, tech_rep);
                // go until same # training points as previous round
                if (linearCalibrationRound >= 2 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[linearCalibrationRound - 2])
                    break;
                trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
                CalibrateLinear(td_file, bio_rep, fraction, tech_rep, dataPointAcquisitionResult.Ms1List);
            }

            trainingPointCounts = new List<int>();
            for (int forestCalibrationRound = 1; ; forestCalibrationRound++)
            {
                CalibrationFunction calibrationFunction = CalibrateRF(dataPointAcquisitionResult, td_file, bio_rep, fraction, tech_rep);
                dataPointAcquisitionResult = GetDataPoints(identifications, td_file, bio_rep, fraction, tech_rep);
                if (forestCalibrationRound >= 2 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[forestCalibrationRound - 2])
                    break;
                trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
            }

            //calibrate topdown hits if this is topdown file....
            foreach (TopDownHit hit in Sweet.lollipop.td_hits_calibration.Where(h => h.filename == file.filename))
            {
                Tuple<string, int, double> key = new Tuple<string, int, double>(hit.filename, hit.ms2ScanNumber, hit.reported_mass);
                if (!Sweet.lollipop.td_hit_correction.ContainsKey(key)) lock (Sweet.lollipop.td_hit_correction) Sweet.lollipop.td_hit_correction.Add(key, hit.mz.ToMass(hit.charge));
            }
            return true;
        }

        private CalibrationFunction CalibrateRF(DataPointAquisitionResults res, bool td_file, int bio_rep, int fraction, int tech_rep)
        {
            var rnd = new Random();

            var shuffledMs1TrainingPoints = res.Ms1List.OrderBy(item => rnd.Next()).ToList();

            var trainList1 = shuffledMs1TrainingPoints.Take((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();
            var testList1 = shuffledMs1TrainingPoints.Skip((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.GetMSE(testList1);
            if (trainList1.Count > 0)
            {
                var ms1regressorRF = new RandomForestCalibrationFunction(40, 10, new bool[] { true, true });
                ms1regressorRF.Train(trainList1);
                var MS1mse = ms1regressorRF.GetMSE(testList1);
                if (MS1mse < bestMS1MSE)
                {
                    bestMS1MSE = MS1mse;
                    bestMS1predictor = ms1regressorRF;
                }
            }
            CalibrationFunction bestCf = bestMS1predictor;

            CalibrateHitsAndComponents(bestCf, td_file, bio_rep, fraction, tech_rep);
            return bestCf;
        }


        public void CalibrateHitsAndComponents(CalibrationFunction bestCf, bool td_file, int bio_rep, int fraction, int tech_rep)
        {
            foreach (TopDownHit hit in Sweet.lollipop.td_hits_calibration.Where(h => h.biological_replicate == bio_rep && h.fraction == fraction && (!td_file || h.technical_replicate == tech_rep)))
            {
                hit.mz = hit.mz - bestCf.Predict(new double[] { hit.mz, hit.retention_time });
            }
            foreach (Component c in Sweet.lollipop.calibration_components.Where(h => h.input_file.topdown_file == td_file && h.input_file.biological_replicate == bio_rep && h.input_file.technical_replicate == tech_rep && h.input_file.fraction == fraction))
            {
                foreach (ChargeState cs in c.charge_states)
                {
                    int scanNumber = myMsDataFile.GetClosestOneBasedSpectrumNumber(c.rt_apex);
                    var scan = myMsDataFile.GetOneBasedScan(scanNumber);
                    bool ms1Scan = scan.MsnOrder == 1;
                    while (!ms1Scan)
                    {
                        scanNumber--;
                        scan = myMsDataFile.GetOneBasedScan(scanNumber);
                        ms1Scan = scan.MsnOrder == 1;
                    }
                    cs.mz_centroid = cs.mz_centroid - bestCf.Predict(new double[] { cs.mz_centroid, scan.RetentionTime });
                }
            }
            foreach (var a in myMsDataFile.Where(s => s.MsnOrder == 1))
            {
                Func<IMzPeak, double> theFunc = x => x.Mz - bestCf.Predict(new double[] { x.Mz, a.RetentionTime });
                a.TransformByApplyingFunctionToSpectra(theFunc);
            }
        }

        private void CalibrateLinear(bool td_file, int bio_rep, int fraction, int tech_rep, List<LabeledMs1DataPoint> res)
        {
            var rnd = new Random();

            var shuffledMs1TrainingPoints = res.OrderBy(item => rnd.Next()).ToList();

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
            CalibrateHitsAndComponents(bestMS1predictor, td_file, bio_rep, fraction, tech_rep);
        }

        private DataPointAquisitionResults GetDataPoints(List<TopDownHit> identifications, bool td_file, int bio_rep, int fraction, int tech_rep)
        {
            DataPointAquisitionResults res = new DataPointAquisitionResults()
            {
                Ms1List = new List<LabeledMs1DataPoint>()
            };
            
            // Set of peaks, identified by m/z and retention time. If a peak is in here, it means it has been a part of an accepted identification, and should be rejected
            var peaksAddedFromMS1HashSet = new HashSet<Tuple<double, int>>();
            foreach (TopDownHit identification in identifications)
            {
                List<int> scanNumbers = new List<int>() { identification.ms2ScanNumber };
                int proteinCharge = identification.charge;

                Component matching_component = null;
                if (!td_file) //if calibrating across files find component with matching mass and retention time
                {
                    //look around theoretical mass of topdown hit identified proteoforms - 10 ppm and 5 minutes            
                     double hit_mass = (Sweet.lollipop.neucode_labeled ? (identification.theoretical_mass - (identification.sequence.Count(s => s == 'K') * 128.094963) + (identification.sequence.Count(s => s == 'K') * 136.109162)) : identification.mz.ToMass(identification.charge));
                     matching_component = Sweet.lollipop.calibration_components.Where(c => c.input_file.biological_replicate == bio_rep && c.input_file.fraction == fraction
                && Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - hit_mass ) * 1e6 / c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) < 10
                && Math.Abs(c.rt_apex- identification.retention_time) < 5.0 ).OrderBy(c => Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - hit_mass)).FirstOrDefault();
                    if (matching_component == null) continue;
                    ChargeState cs = matching_component.charge_states.OrderByDescending(c => c.intensity).FirstOrDefault();
                    scanNumbers.Clear();
                    for (int i = Convert.ToInt16(matching_component.scan_range.Split('-')[0]); i <= Convert.ToInt16(matching_component.scan_range.Split('-')[1]); i++)
                    {
                        scanNumbers.Add(i);
                    }
                    proteinCharge = matching_component.charge_states.OrderByDescending(c => c.intensity).First().charge_count;
                }

                var SequenceWithChemicalFormulas = identification.GetSequenceWithChemicalFormula();
                if (SequenceWithChemicalFormulas == null)
                {
                    continue;
                }
                Proteomics.Peptide coolPeptide = new Proteomics.Peptide(SequenceWithChemicalFormulas);

                // Calculate isotopic distribution of the full peptide

                var dist = IsotopicDistribution.GetDistribution(coolPeptide.GetChemicalFormula(), 0.1, 0.001);

                double[] masses = dist.Masses.ToArray();
                double[] intensities = dist.Intensities.ToArray();

                Array.Sort(intensities, masses, Comparer<double>.Create((x, y) => y.CompareTo(x)));

                numMs1MassChargeCombinationsConsidered = 0;
                numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks = 0;

                foreach (int scanNumber in scanNumbers)
                {
                    res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, scanNumber, -1, peaksAddedFromMS1HashSet, proteinCharge, identification));
                    res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, scanNumber, 1, peaksAddedFromMS1HashSet, proteinCharge, identification));
                    res.numMs1MassChargeCombinationsConsidered += numMs1MassChargeCombinationsConsidered;
                    res.numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks += numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks;
                }
            }
            return res;
        }

        private IEnumerable<LabeledMs1DataPoint> SearchMS1Spectra(double[] originalMasses, double[] originalIntensities, int ms2spectrumIndex, int direction, HashSet<Tuple<double, int>> peaksAddedHashSet, int peptideCharge, TopDownHit identification)
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
                int ms1ScanNumber = fullMS1scan.OneBasedScanNumber;
                var scanWindowRange = fullMS1scan.ScanWindowRange;
                var fullMS1spectrum = fullMS1scan.MassSpectrum;
                if (fullMS1spectrum.Size == 0)
                    break;

                for (int i = -1; i <= 1; i += 2)
                {
                    bool startingToAddCharges = false;
                    bool continueAddingCharges = false;
                    int chargeToLookAt = direction > 0 ? peptideCharge : (peptideCharge - 1);
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
                            if (Sweet.lollipop.neucode_labeled)
                            {
                                theMZ = (theMZ.ToMass(chargeToLookAt) - (identification.sequence.Count(s => s == 'K') * 128.094963) + ((identification.sequence.Count(s => s == 'K') * 136.109162))).ToMz(chargeToLookAt);
                            }

                            //10 ppm
                            double mass_tolerance = theMZ / 1e6 * 10;
                            var npwr = fullMS1spectrum.NumPeaksWithinRange(theMZ - mass_tolerance, theMZ + mass_tolerance);
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

                            var theTuple = Tuple.Create(closestPeakMZ, ms1ScanNumber);
                            if (!peaksAddedHashSet.Contains(theTuple))
                            {
                                peaksAddedHashSet.Add(theTuple);
                                highestKnownChargeForThisPeptide = Math.Max(highestKnownChargeForThisPeptide, chargeToLookAt);
                                trainingPointsToAverage.Add(new LabeledMs1DataPoint(closestPeakMZ, double.NaN, closestPeakMZ - theMZ, null));
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
                    double corrected_mass;
                    if (Sweet.lollipop.td_hit_correction.TryGetValue(new Tuple<string, int, double>(row.Cell(15).Value.ToString().Split('.')[0], Convert.ToInt16(row.Cell(18).GetDouble()), row.Cell(17).GetDouble()), out corrected_mass))
                    {
                        row.Cell(17).SetValue(corrected_mass);
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
                        double value;
                        if (Sweet.lollipop.file_mz_correction.TryGetValue(new Tuple<string, double>(file.filename, Math.Round(row.Cell(3).GetDouble(), 0)), out value))
                        {
                            row.Cell(4).SetValue(value);
                        }
                    }
                }
            });
            workbook.Save();

        }
    }
}

