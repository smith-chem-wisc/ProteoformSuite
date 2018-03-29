using Chemistry;
using ClosedXML.Excel;
using IO.MzML;
using IO.Thermo;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Calibration
    {
        //CALIBRATION WITH TD HITS
        private IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile;

        private List<TopDownHit> all_topdown_hits;
        private List<TopDownHit> high_scoring_topdown_hits;
        private InputFile raw_file;

        public bool Run_TdMzCal(InputFile raw_file, List<TopDownHit> topdown_hits)
        {
            all_topdown_hits = topdown_hits;
            high_scoring_topdown_hits = all_topdown_hits.Where(h => h.score >= 40).ToList();
            this.raw_file = raw_file;

            if (high_scoring_topdown_hits.Count < 5)
            {
                return false;
            }

            var trainingPointCounts = new List<int>();

            myMsDataFile = Path.GetExtension(raw_file.complete_path) == ".raw" ?
                ThermoStaticData.LoadAllStaticData(raw_file.complete_path) :
                null;
            if (myMsDataFile == null) { myMsDataFile = Mzml.LoadAllStaticData(raw_file.complete_path); }
            if (myMsDataFile == null) { return false; }
            DataPointAquisitionResults dataPointAcquisitionResult = null;

            //need to reset m/z in case same td hits used for multiple calibration raw files...
            Parallel.ForEach(all_topdown_hits, h => h.mz = h.reported_mass.ToMz(h.charge));
            int round = 0;
            int linearCalibrationRound = 1;
            while (true)
            {
                dataPointAcquisitionResult = GetDataPoints();
                // go until same # training points as previous round
                if (linearCalibrationRound >= 2 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[linearCalibrationRound - 2])
                {
                    break;
                }
                round++;
                trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
                if (dataPointAcquisitionResult.Ms1List.Count < 5)
                {
                    return false;
                }

                CalibrateLinear(dataPointAcquisitionResult.Ms1List.OrderBy(p => p.Label).ToList(), round);
                linearCalibrationRound++;
            }

            trainingPointCounts = new List<int>();
            int forestCalibrationRound = 1;
            while (true)
            {
                CalibrationFunction calibrationFunction = CalibrateRF(dataPointAcquisitionResult.Ms1List.OrderBy(p => p.Label).ToList(), round);
                dataPointAcquisitionResult = GetDataPoints();
                if (forestCalibrationRound >= 2 && dataPointAcquisitionResult.Ms1List.Count <= trainingPointCounts[forestCalibrationRound - 2])
                {
                    break;
                }
                round++;
                trainingPointCounts.Add(dataPointAcquisitionResult.Ms1List.Count);
                if (dataPointAcquisitionResult.Ms1List.Count < 5)
                {
                    return false;
                }
                forestCalibrationRound++;
            }
            if (Sweet.lollipop.calibrate_raw_files)
            {
                MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(myMsDataFile, raw_file.directory + "\\" + raw_file.filename + "_calibrated.mzML", false);
            }
            return true;
        }

        private CalibrationFunction CalibrateRF(List<LabeledMs1DataPoint> res, int round)
        {
            Random decoy_rng = Sweet.lollipop.calibration_use_random_seed ? new Random(round + Sweet.lollipop.calibration_random_seed) : new Random(); //new random generator for each round of calibration
            var shuffledMs1TrainingPoints = res.OrderBy(item => decoy_rng.Next()).ToList();
            var trainList1 = shuffledMs1TrainingPoints.Take((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();
            var testList1 = shuffledMs1TrainingPoints.Skip((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.GetMSE(testList1);
            if (trainList1.Count > 0)
            {
                var ms1regressorRF = new RandomForestCalibrationFunction(40, 10, new[] { true, true });
                ms1regressorRF.Train(trainList1);
                var MS1mse = ms1regressorRF.GetMSE(testList1);
                if (MS1mse < bestMS1MSE)
                {
                    bestMS1MSE = MS1mse;
                    bestMS1predictor = ms1regressorRF;
                }
            }
            CalibrationFunction bestCf = bestMS1predictor;

            CalibrateHitsAndComponents(bestCf);
            return bestCf;
        }

        public void CalibrateHitsAndComponents(CalibrationFunction bestCf)
        {
            foreach (TopDownHit hit in all_topdown_hits)
            {
                hit.mz = hit.mz - bestCf.Predict(new double[] { hit.mz, hit.ms1_retention_time });
            }
            foreach (Component c in Sweet.lollipop.calibration_components.Where(h => h.input_file.lt_condition == raw_file.lt_condition && h.input_file.biological_replicate == raw_file.biological_replicate && h.input_file.fraction == raw_file.fraction && h.input_file.technical_replicate == raw_file.technical_replicate))
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
                Func<Spectra.IPeak, double> theFunc = x => x.X - bestCf.Predict(new double[] { x.X, a.RetentionTime });
                a.MassSpectrum.ReplaceXbyApplyingFunction(theFunc);
            }
        }

        private void CalibrateLinear(List<LabeledMs1DataPoint> res, int round)
        {
            Random decoy_rng = Sweet.lollipop.calibration_use_random_seed ? new Random(round + Sweet.lollipop.calibration_random_seed) : new Random(); //new random generator for each round of
            var shuffledMs1TrainingPoints = res.OrderBy(item => decoy_rng.Next()).ToList();
            var trainList1 = shuffledMs1TrainingPoints.Take((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();
            var testList1 = shuffledMs1TrainingPoints.Skip((int)(shuffledMs1TrainingPoints.Count * 0.75)).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.GetMSE(testList1);
            {
                var ms1regressor = new ConstantCalibrationFunction();
                ms1regressor.Train(trainList1);
                double MS1mse = ms1regressor.GetMSE(testList1);
                if (MS1mse < bestMS1MSE)
                {
                    bestMS1MSE = MS1mse;
                    bestMS1predictor = ms1regressor;
                }
            }

            var transforms = new List<TransformFunction>
            {
                new TransformFunction(b => new double[] { b[0] }, 1),
                new TransformFunction(b => new double[] { b[1] }, 1),
                new TransformFunction(b => new double[] { b[0], b[1] }, 2)
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
            CalibrateHitsAndComponents(bestMS1predictor);
        }

        private DataPointAquisitionResults GetDataPoints()
        {
            DataPointAquisitionResults res = new DataPointAquisitionResults()
            {
                Ms1List = new List<LabeledMs1DataPoint>()
            };

            // Set of peaks, identified by m/z and retention time. If a peak is in here, it means it has been a part of an accepted identification, and should be rejected
            var peaksAddedFromMS1HashSet = new HashSet<Tuple<double, int>>();
            foreach (TopDownHit identification in high_scoring_topdown_hits.OrderByDescending(h => h.score).ThenBy(h => h.pscore).ThenBy(h => h.reported_mass))
            {
                List<int> scanNumbers = new List<int>() { identification.ms2ScanNumber };
                int proteinCharge = identification.charge;

                Component matching_component = null;
                if (identification.filename != raw_file.filename) //if calibrating across files find component with matching mass and retention time
                {
                    //look around theoretical mass of topdown hit identified proteoforms - 10 ppm and 5 minutes same br, tr, fraction, condition (same file!)
                    //if neucode labled, look for the light component mass (loaded in...)
                    List<Component> potential_matches = Sweet.lollipop.calibration_components.
                        Where(c => c.input_file.lt_condition == raw_file.lt_condition
                        && (Sweet.lollipop.neucode_labeled || c.input_file.biological_replicate == raw_file.biological_replicate)
                        && c.input_file.fraction == raw_file.fraction
                        && c.input_file.technical_replicate == raw_file.technical_replicate).ToList();
                    if (potential_matches.Count > 0)
                    {
                        matching_component = potential_matches.Where(c =>
                           Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - identification.theoretical_mass) * 1e6 / c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) < 10
                           && Math.Abs(c.rt_apex - identification.ms1_retention_time) < 5.0).OrderBy(c => Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - identification.theoretical_mass)).FirstOrDefault();
                    }
                    else
                    {
                        matching_component = null;
                    }

                    if (matching_component == null)
                    {
                        continue;
                    }
                    scanNumbers.Clear();
                    //get scan numbers using retention time (if raw file is spliced, scan numbers change)
                    double rt = Convert.ToDouble(matching_component.rt_range.Split('-')[0]);
                    while (Math.Round(rt, 2) <= Math.Round(Convert.ToDouble(matching_component.rt_range.Split('-')[1]), 2))
                    {
                        int scanNumber = myMsDataFile.GetClosestOneBasedSpectrumNumber(rt);
                        scanNumbers.Add(scanNumber);
                        rt = myMsDataFile.GetOneBasedScan(scanNumber + 1).RetentionTime;
                    }
                    proteinCharge = matching_component.charge_states.OrderByDescending(c => c.intensity).First().charge_count;
                }
                else if (identification.technical_replicate != raw_file.technical_replicate)
                {
                    continue;
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

                List<int> scansAdded = new List<int>();
                foreach (int scanNumber in scanNumbers)
                {
                    res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, scanNumber, -1, scansAdded, peaksAddedFromMS1HashSet, proteinCharge, identification));
                    res.Ms1List.AddRange(SearchMS1Spectra(masses, intensities, scanNumber, 1, scansAdded, peaksAddedFromMS1HashSet, proteinCharge, identification));
                }
            }

            return res;
        }

        private IEnumerable<LabeledMs1DataPoint> SearchMS1Spectra(double[] originalMasses, double[] originalIntensities, int ms2spectrumIndex, int direction, List<int> scansAdded, HashSet<Tuple<double, int>> peaksAddedHashSet, int proteinCharge, TopDownHit identification)
        {
            int theIndex = direction == 1 ? ms2spectrumIndex : ms2spectrumIndex - 1;
            bool addedAscan = true;
            int highestKnownChargeForThisPeptide = proteinCharge;
            while (theIndex >= 1 && theIndex <= myMsDataFile.NumSpectra && addedAscan)
            {
                if (scansAdded.Contains(theIndex))
                {
                    theIndex += direction;
                    continue;
                }
                scansAdded.Add(theIndex);
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
                {
                    break;
                }

                //look in both charge state directions for proteins -- likely to have multiple charge states.
                for (int i = -1; i <= 1; i += 2)
                {
                    bool startingToAddCharges = false;
                    bool continueAddingCharges = false;
                    int chargeToLookAt = i == 1 ? proteinCharge : (proteinCharge - 1);
                    do
                    {
                        //if m/z too big or small and going in correct direction, increase or decrease charge state. otherwise break.
                        if (originalMasses[0].ToMz(chargeToLookAt) > scanWindowRange.Maximum)
                        {
                            if (i == 1)
                            {
                                chargeToLookAt++;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (originalMasses[0].ToMz(chargeToLookAt) < scanWindowRange.Minimum)
                        {
                            if (i == -1)
                            {
                                chargeToLookAt--;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        var trainingPointsToAverage = new List<LabeledMs1DataPoint>();
                        foreach (double a in originalMasses)
                        {
                            double theMZ = a.ToMz(chargeToLookAt);
                            if (Sweet.lollipop.neucode_labeled)
                            {
                                theMZ = Sweet.lollipop.get_neucode_mass(theMZ.ToMass(chargeToLookAt), identification.sequence.Count(s => s == 'K')).ToMz(chargeToLookAt);
                            }

                            //10 ppm
                            double mass_tolerance = theMZ / 1e6 * 10;
                            var npwr = fullMS1spectrum.NumPeaksWithinRange(theMZ - mass_tolerance, theMZ + mass_tolerance);
                            if (npwr == 0)
                            {
                                break;
                            }
                            if (npwr > 1)
                            {
                                continue;
                            }

                            var closestPeak = fullMS1spectrum.GetClosestPeakXvalue(theMZ);
                            var closestPeakMZ = closestPeak;

                            var theTuple = closestPeakMZ != null ? Tuple.Create((double)closestPeakMZ, ms1ScanNumber) : null;
                            if (theTuple == null)
                            {
                                continue;
                            }
                            if (!peaksAddedHashSet.Contains(theTuple))
                            {
                                peaksAddedHashSet.Add(theTuple);
                                highestKnownChargeForThisPeptide = Math.Max(highestKnownChargeForThisPeptide, chargeToLookAt);
                                trainingPointsToAverage.Add(new LabeledMs1DataPoint((double)closestPeakMZ, double.NaN, (double)closestPeakMZ - theMZ, null));
                            }
                            else
                            {
                                break;
                            }
                        }
                        // If started adding and suddnely stopped, go to next one, no need to look at higher charges
                        if (trainingPointsToAverage.Count == 0 && startingToAddCharges == true)
                        {
                            break;
                        }
                        if ((trainingPointsToAverage.Count == 0 || (trainingPointsToAverage.Count == 1 && originalIntensities[0] < 0.65)) && (proteinCharge <= chargeToLookAt))
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
        public static void calibrate_td_hits_file(InputFile file)
        {
            //Copy file to new worksheet
            string old_absolute_path = file.complete_path;
            string new_absolute_path = file.directory + "\\" + file.filename + "_calibrated" + file.extension;

            //create copy of excel file
            byte[] byteArray = File.ReadAllBytes(old_absolute_path);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, byteArray.Length);
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
            foreach (IXLRow row in rows_to_delete)
            {
                row.Delete(); //can't parallelize
            }
            workbook.Save();
        }

        //READ AND WRITE NEW CALIBRATED RAW EXPERIMENTAL COMPONENTS FILE
        public static void calibrate_components_in_xlsx(InputFile file)
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
                        if (Sweet.lollipop.file_mz_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(row.Cell(3).GetDouble() / row.Cell(2).GetDouble(), 0), Math.Round(row.Cell(5).GetDouble(), 2)), out value))
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