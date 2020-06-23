using Chemistry;
using ClosedXML.Excel;
using IO.MzML;
using IO.Thermo;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using SharpLearning.Containers.Matrices;
using SharpLearning.CrossValidation.TrainingTestSplitters;
using SharpLearning.Metrics.Regression;
using SharpLearning.Optimization;
using SharpLearning.RandomForest.Learners;
using SharpLearning.RandomForest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlashLFQ;

namespace ProteoformSuiteInternal
{
    public class Calibration
    {
        //CALIBRATION WITH TD HITS
        private MsDataFile myMsDataFile;

        private List<SpectrumMatch> all_topdown_hits;
        private List<SpectrumMatch> high_scoring_topdown_hits;
        private InputFile raw_file;

        public bool Run_TdMzCal(InputFile raw_file, List<SpectrumMatch> topdown_hits)
        {
            all_topdown_hits = topdown_hits.Where(h => h.score > 0).ToList();
            //need to reset m/z in case same td hits used for multiple calibration raw files...
            Parallel.ForEach(all_topdown_hits, h => h.mz = h.reported_mass.ToMz(h.charge));

            high_scoring_topdown_hits = all_topdown_hits.Where(h => h.score >= 40).ToList();
            this.raw_file = raw_file;

            if (high_scoring_topdown_hits.Count < 5)
            {
                return false;
            }

            myMsDataFile = Path.GetExtension(raw_file.complete_path) == ".raw" ?
                ThermoStaticData.LoadAllStaticData(raw_file.complete_path) :
                null;
            if (myMsDataFile == null) { myMsDataFile = Mzml.LoadAllStaticData(raw_file.complete_path); }
            if (myMsDataFile == null) { return false; }

            DataPointAquisitionResults dataPointAcquisitionResult = GetDataPoints();

            if (dataPointAcquisitionResult.Ms1List.Count < 10) return false;

            if (Sweet.lollipop.mass_calibration)
            {
                var myMs1DataPoints = new List<(double[] xValues, double yValue)>();

                for (int i = 0; i < dataPointAcquisitionResult.Ms1List.Count; i++)
                {
                    //x values
                    var explanatoryVariables = new double[4];
                    explanatoryVariables[0] = dataPointAcquisitionResult.Ms1List[i].mz;
                    explanatoryVariables[1] = dataPointAcquisitionResult.Ms1List[i].retentionTime;
                    explanatoryVariables[2] = dataPointAcquisitionResult.Ms1List[i].logTotalIonCurrent;
                    explanatoryVariables[3] = dataPointAcquisitionResult.Ms1List[i].logInjectionTime;

                    //yvalue
                    double mzError = dataPointAcquisitionResult.Ms1List[i].massError;

                    myMs1DataPoints.Add((explanatoryVariables, mzError));
                }

                var ms1Model = GetRandomForestModel(myMs1DataPoints);

                CalibrateHitsAndComponents(ms1Model);
                if (Sweet.lollipop.calibrate_raw_files)
                {
                    MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(myMsDataFile,
                        raw_file.directory + "\\" + raw_file.filename + "_calibrated.mzML", false);
                }
            }

            if (Sweet.lollipop.retention_time_calibration)
            {
                var myMs1DataPoints = new List<(double[] xValues, double yValue)>();
                List<SpectrumMatch> firstElutingTopDownHit = new List<SpectrumMatch>();
                List<string> PFRs = high_scoring_topdown_hits.Select(h => h.pfr_accession).Distinct().ToList();
                foreach (var PFR in PFRs)
                {
                    var firstHitWithPFR = high_scoring_topdown_hits
                        .Where(h => h.pfr_accession == PFR).OrderBy(h => h.ms2_retention_time).First();
                    firstElutingTopDownHit.Add(firstHitWithPFR);
                }

                for (int i = 0; i < dataPointAcquisitionResult.Ms1List.Count; i++)
                {
                    if (firstElutingTopDownHit.Contains(dataPointAcquisitionResult.Ms1List[i].identification))
                    {
                        //x values
                        var explanatoryVariables = new double[1];
                        explanatoryVariables[0] = dataPointAcquisitionResult.Ms1List[i].retentionTime;

                        //yvalue
                        double RTError = dataPointAcquisitionResult.Ms1List[i].RTError;

                        myMs1DataPoints.Add((explanatoryVariables, RTError));
                    }
                }

                if (myMs1DataPoints.Count < 10)
                {
                    return false;
                }

                var ms1Model = GetRandomForestModel(myMs1DataPoints);

                foreach (Component c in Sweet.lollipop.calibration_components.Where(h => h.input_file.lt_condition == raw_file.lt_condition && h.input_file.biological_replicate == raw_file.biological_replicate && h.input_file.fraction == raw_file.fraction && h.input_file.technical_replicate == raw_file.technical_replicate))
                {
                    c.rt_apex = c.rt_apex - ms1Model.Predict(new double[] { c.rt_apex });
                }
            }
            return true;
        }

        public void RetentionTimeCalibrateTopDownHits(List<SpectrumMatch> topdown_hits)

        {
            all_topdown_hits = topdown_hits.Where(h => h.score > 0).ToList();
            high_scoring_topdown_hits = all_topdown_hits.Where(h => h.score >= 40).ToList();
            List<string> filenames_orderbydescendingUniquePFRs = high_scoring_topdown_hits.Select(h => h.filename).Distinct().OrderByDescending(f => high_scoring_topdown_hits.Where(h => h.filename == f).Select(h => h.pfr_accession).Distinct().Count()).ThenByDescending(f => high_scoring_topdown_hits.Count(h => h.filename == f)).ToList();
            if (filenames_orderbydescendingUniquePFRs.Count == 1) return; //only if more than 1 file.

            //calibrate everything to file with most unique hits.
            List<SpectrumMatch> calibrated_RT_topdown_hits =
                high_scoring_topdown_hits.Where(h => h.filename == filenames_orderbydescendingUniquePFRs.First()).ToList();

            for (int i = 1; i < filenames_orderbydescendingUniquePFRs.Count; i++)
            {
                var myMs1DataPoints = new List<(double[] xValue, double yValue)>();
                List<string> PFRs = high_scoring_topdown_hits.Where(h => h.filename == filenames_orderbydescendingUniquePFRs[i]).Select(h => h.pfr_accession).Distinct().ToList();
                foreach (var PFR in PFRs)
                {
                    double minRTNewFile = high_scoring_topdown_hits
                        .Where(h => h.filename == filenames_orderbydescendingUniquePFRs[i] && h.pfr_accession == PFR)
                        .Min(h => h.ms2_retention_time);
                    var calibratedRTs = calibrated_RT_topdown_hits
                        .Where(h => h.pfr_accession == PFR);
                    if (calibratedRTs.Count() == 0) continue;

                    var calibratedminRT = calibratedRTs.Min(h => h.calibrated_retention_time);

                    myMs1DataPoints.Add((new double[] { minRTNewFile}, minRTNewFile - calibratedminRT));
                }
                var ms1Model = GetRandomForestModel(myMs1DataPoints);
                foreach (SpectrumMatch hit in all_topdown_hits.Where(h => h.filename == filenames_orderbydescendingUniquePFRs[i]))
                {
                    hit.calibrated_retention_time = hit.ms2_retention_time - ms1Model.Predict(new double[] { hit.ms2_retention_time });
                    if (hit.score > 40)
                    {
                        calibrated_RT_topdown_hits.Add(hit); //use for subsequent rounds
                    }
                }
            }
        }

        public void CalibrateHitsAndComponents(RegressionForestModel bestCf)
        {
            foreach (SpectrumMatch hit in all_topdown_hits)
            {
                hit.mz = hit.mz - bestCf.Predict(new double[] { hit.mz, hit.ms1_scan.RetentionTime, Math.Log(hit.ms1_scan.TotalIonCurrent), hit.ms1_scan.InjectionTime.HasValue ? Math.Log(hit.ms1_scan.InjectionTime.Value) : double.NaN });
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

                    cs.mz_centroid = cs.mz_centroid - bestCf.Predict(new double[] { cs.mz_centroid, scan.RetentionTime, Math.Log(scan.TotalIonCurrent), scan.InjectionTime.HasValue ? Math.Log(scan.InjectionTime.Value) : double.NaN });
                }
            }
            foreach (var a in myMsDataFile.GetAllScansList().Where(s => s.MsnOrder == 1))
            {
                Func<MzPeak, double> theFunc = x => x.Mz - bestCf.Predict(new double[] { x.Mz, a.RetentionTime, Math.Log(a.TotalIonCurrent), a.InjectionTime.HasValue ? Math.Log(a.InjectionTime.Value) : double.NaN });
                a.MassSpectrum.ReplaceXbyApplyingFunction(theFunc);
            }
        }

        private DataPointAquisitionResults GetDataPoints()
        {
            DataPointAquisitionResults res = new DataPointAquisitionResults()
            {
                Ms1List = new List<LabeledMs1DataPoint>()
            };

            // Set of peaks, identified by m/z and retention time. If a peak is in here, it means it has been a part of an accepted identification, and should be rejected
            var peaksAddedFromMS1HashSet = new HashSet<Tuple<double, int>>();
            foreach (SpectrumMatch identification in high_scoring_topdown_hits.OrderByDescending(h => h.score).ThenBy(h => h.qValue).ThenBy(h => h.reported_mass))
            {
                int scanNum = myMsDataFile.GetClosestOneBasedSpectrumNumber(identification.ms2_retention_time);

                List<int> scanNumbers = new List<int>() { scanNum };
                int proteinCharge = identification.charge;

                Component matching_component = null;
                if (identification.filename != raw_file.filename) //if calibrating across files find component with matching mass and retention time
                {
                    //NOTE: only looking at components from same raw file... looking for components corresponding to td hits from any files w/ same br, fraction, condition however.
                    //look around theoretical mass of topdown hit identified proteoforms - 10 ppm and 5 minutes same br, tr, fraction, condition (same file!)
                    //if neucode labled, look for the light component mass (loaded in...)
                    List<Component> potential_matches = Sweet.lollipop.calibration_components.
                        Where(c => c.input_file.lt_condition == raw_file.lt_condition
                        && c.input_file.biological_replicate == raw_file.biological_replicate
                        && c.input_file.fraction == raw_file.fraction
                        && c.input_file.technical_replicate == raw_file.technical_replicate).ToList();
                    if (potential_matches.Count > 0)
                    {
                        matching_component = potential_matches.Where(c =>
                           Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - identification.theoretical_mass) * 1e6 / c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) < Sweet.lollipop.cali_mass_tolerance
                           && Math.Abs(c.rt_apex - identification.ms1_scan.RetentionTime) < Sweet.lollipop.cali_rt_tolerance).OrderBy(c => Math.Abs(c.charge_states.OrderByDescending(s => s.intensity).First().mz_centroid.ToMass(c.charge_states.OrderByDescending(s => s.intensity).First().charge_count) - identification.theoretical_mass)).FirstOrDefault();
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
                    double rt = matching_component.min_rt;
                    while (Math.Round(rt, 2) <= Math.Round(matching_component.max_rt, 2))
                    {
                        int scanNumber = myMsDataFile.GetClosestOneBasedSpectrumNumber(rt);
                        scanNumbers.Add(scanNumber);
                        rt = myMsDataFile.GetOneBasedScan(scanNumber + 1).RetentionTime;
                    }
                    proteinCharge = matching_component.charge_states.OrderByDescending(c => c.intensity).First().charge_count;
                    if (matching_component.charge_states.Count == 1)
                    {
                        proteinCharge = identification.charge;
                    }
                }


                var formula = identification.GetChemicalFormula();
                if (formula == null)
                {
                    continue;
                }

                // Calculate isotopic distribution of the full peptide
                var dist = IsotopicDistribution.GetDistribution(formula, 0.1, 0.001);

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

        private IEnumerable<LabeledMs1DataPoint> SearchMS1Spectra(double[] originalMasses, double[] originalIntensities, int ms2spectrumIndex, int direction, List<int> scansAdded, HashSet<Tuple<double, int>> peaksAddedHashSet, int proteinCharge, SpectrumMatch identification)
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

                            double mass_tolerance = theMZ / 1e6 * Sweet.lollipop.cali_mass_tolerance;
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
                                trainingPointsToAverage.Add(new LabeledMs1DataPoint((double)closestPeakMZ, double.NaN, double.NaN, double.NaN, (double)closestPeakMZ - theMZ, (double)fullMS1scan.RetentionTime - identification.calibrated_retention_time, null));
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
                                Math.Log(fullMS1scan.TotalIonCurrent),
                                fullMS1scan.InjectionTime.HasValue ? Math.Log(fullMS1scan.InjectionTime.Value) : double.NaN,
                                trainingPointsToAverage.Select(b => b.massError).Median(),
                                trainingPointsToAverage.OrderBy(b => b.retentionTime).First().RTError,
                                identification);
                        }
                        chargeToLookAt += i;
                    } while (continueAddingCharges);
                }
                theIndex += direction;
            }
        }

        private RegressionForestModel GetRandomForestModel(List<(double[] xValues, double yValue)> myInputs)
        {
            // create a machine learner
            var learner = new RegressionRandomForestLearner();
            var metric = new MeanSquaredErrorRegressionMetric();

            var splitter = new RandomTrainingTestIndexSplitter<double>(trainingPercentage: 0.70);

            // put x values into a matrix and y values into a 1D array
            var myXValueMatrix = new F64Matrix(myInputs.Count, myInputs.First().xValues.Length);
            for (int i = 0; i < myInputs.Count; i++)
                for (int j = 0; j < myInputs.First().xValues.Length; j++)
                    myXValueMatrix[i, j] = myInputs[i].xValues[j];

            var myYValues = myInputs.Select(p => p.yValue).ToArray();

            // split data into training set and test set
            var splitData = splitter.SplitSet(myXValueMatrix, myYValues);
            var trainingSetX = splitData.TrainingSet.Observations;
            var trainingSetY = splitData.TrainingSet.Targets;

            // learn an initial model
            var myModel = learner.Learn(trainingSetX, trainingSetY);

            // parameter ranges for the optimizer
            var parameters = new ParameterBounds[]
            {
                new ParameterBounds(min: 100, max: 150, transform: Transform.Linear),
                new ParameterBounds(min: 1, max: 5, transform: Transform.Linear),
                new ParameterBounds(min: 500, max: 2000, transform: Transform.Linear),
                new ParameterBounds(min: 0, max: 2, transform: Transform.Linear),
                new ParameterBounds(min: 1e-06, max: 1e-05, transform: Transform.Logarithmic),
                new ParameterBounds(min: 0.7, max: 1.5, transform: Transform.Linear)
            };

            var validationSplit = new RandomTrainingTestIndexSplitter<double>(trainingPercentage: 0.70)
                .SplitSet(myXValueMatrix, myYValues);

            // define minimization metric
            Func<double[], OptimizerResult> minimize = p =>
            {
                // create the candidate learner using the current optimization parameters
                var candidateLearner = new RegressionRandomForestLearner(
                    trees: (int)p[0],
                    minimumSplitSize: (int)p[1],
                    maximumTreeDepth: (int)p[2],
                    featuresPrSplit: (int)p[3],
                    minimumInformationGain: p[4],
                    subSampleRatio: p[5],
                    runParallel: false);

                var candidateModel = candidateLearner.Learn(validationSplit.TrainingSet.Observations,
                validationSplit.TrainingSet.Targets);

                var validationPredictions = candidateModel.Predict(validationSplit.TestSet.Observations);
                var candidateError = metric.Error(validationSplit.TestSet.Targets, validationPredictions);

                return new OptimizerResult(p, candidateError);
            };

            // create optimizer
            var optimizer = new RandomSearchOptimizer(parameters, iterations: 30, runParallel: true);

            // find best parameters
            var result = optimizer.OptimizeBest(minimize);
            var best = result.ParameterSet;

            // create the final learner using the best parameters
            // (parameters that resulted in the model with the least error)
            learner = new RegressionRandomForestLearner(
                    trees: (int)best[0],
                    minimumSplitSize: (int)best[1],
                    maximumTreeDepth: (int)best[2],
                    featuresPrSplit: (int)best[3],
                    minimumInformationGain: best[4],
                    subSampleRatio: best[5],
                    runParallel: true);

            // learn final model with optimized parameters
            myModel = learner.Learn(trainingSetX, trainingSetY);

            // all done
            return myModel;
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
                    double corrected_RT;

                    if (Sweet.lollipop.retention_time_calibration)
                    {
                        if (Sweet.lollipop.td_hit_RT_correction.TryGetValue(
                            new Tuple<string, double, double>(row.Cell(15).Value.ToString().Split('.')[0],
                                row.Cell(18).GetDouble(), row.Cell(17).GetDouble()), out corrected_RT))
                        {
                            row.Cell(19).SetValue(corrected_RT);
                        }
                        //if hit's file not calibrated (not enough calibration points, remove from list
                        else
                        {
                            lock (rows_to_delete) rows_to_delete.Add(row);
                        }
                    }

                    if (Sweet.lollipop.mass_calibration)
                    {
                        if (Sweet.lollipop.td_hit_mz_correction.TryGetValue(
                            new Tuple<string, double, double>(row.Cell(15).Value.ToString().Split('.')[0],
                                row.Cell(18).GetDouble(), row.Cell(17).GetDouble()), out corrected_mass))
                        {
                            row.Cell(17).SetValue(corrected_mass);
                        }
                        //if hit's file not calibrated (not enough calibration points, remove from list
                        else
                        {
                            lock (rows_to_delete) rows_to_delete.Add(row);
                        }
                    }

                }
            });
            foreach (IXLRow row in rows_to_delete.Distinct())
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

            if (file.extension == ".xlsx")
            {
                //create copy of excel file
                byte[] byteArray = File.ReadAllBytes(old_absolute_path);
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(byteArray, 0, (int)byteArray.Length);
                    File.WriteAllBytes(new_absolute_path, stream.ToArray());
                }
                var workbook = new XLWorkbook(new_absolute_path);
                var worksheet = workbook.Worksheets.Worksheet(1);
                Parallel.ForEach(worksheet.Rows(2, worksheet.Rows().Count()), row =>
                {
                    if (row.Cell(1).Value.ToString().Length == 0)
                    {
                        if (Regex.IsMatch(row.Cell(2).Value.ToString(), @"^\d+$"))
                        {
                            double corrected_mass;
                            //CHECK WITH CHARGE NORMALIZED INTENSITY!!
                            if (Sweet.lollipop.component_mz_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(row.Cell(3).GetDouble() / row.Cell(2).GetDouble(), 0), Math.Round(row.Cell(5).GetDouble(), 2)), out corrected_mass))
                            {
                                row.Cell(4).SetValue(corrected_mass);
                            }

                        }
                    }
                    else
                    {
                        double corrected_RT;
                        if (Sweet.lollipop.component_RT_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(row.Cell(3).GetDouble(), 0), Math.Round(row.Cell(2).GetDouble(), 2)), out corrected_RT))
                        {
                            row.Cell(11).SetValue(corrected_RT);
                        }
                    }
                });
                workbook.Save();
            }
            else if (file.extension == ".tsv")
            {
                string[] old = File.ReadAllLines(old_absolute_path);
                List<string> new_file = new List<string>();
                new_file.Add(old[0]);
                for (int i = 1; i < old.Length; i++)
                {
                    string[] row = old[i].Split('\t');
                    double mass;
                    double intensity;
                    if (row.Length == 16 && Double.TryParse(row[2], out mass) && Double.TryParse(row[9], out intensity))
                    {
                        double value;
                        if (Sweet.lollipop.component_mz_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(intensity, 0), Math.Round(mass, 2)), out value))
                        {
                            row[2] = value.ToString();
                            new_file.Add(string.Join("\t", row));
                        }
                        if (Sweet.lollipop.component_RT_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(intensity, 0), Math.Round(mass, 2)), out value))
                        {
                            row[8] = (value*60).ToString();
                            new_file.Add(string.Join("\t", row));
                        }
                    }
                    else if (row.Length == 3 && Double.TryParse(row[0], out mass) &&
                             Double.TryParse(row[1], out intensity))
                    {
                        double value;
                        if (Sweet.lollipop.component_mz_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(intensity, 0), Math.Round(mass, 2)), out value))
                        {
                            row[0] = value.ToString();
                            new_file.Add(string.Join("\t", row));
                        }
                        if (Sweet.lollipop.component_RT_correction.TryGetValue(new Tuple<string, double, double>(file.filename, Math.Round(intensity, 0), Math.Round(mass, 2)), out value))
                        {
                            row[2] = value.ToString();
                            new_file.Add(string.Join("\t", row));
                        }
                    }
                }
                File.WriteAllLines(new_absolute_path, new_file);
            }
        }
    }
}