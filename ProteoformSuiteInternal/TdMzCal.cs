using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Thermo;
using Spectra;
using Chemistry;
using MathNet.Numerics.Statistics;
using Proteomics;
using MassSpectrometry;
using System.IO;


namespace ProteoformSuiteInternal
{
    //based on stefank's program mzcal in MetaMorpheus
    public class TdMzCal
    {
        //parameters
        public static double fineResolution = 0.1;
        public static string[] filenames = new string[18];
        public static int[] training_points = new int[18];
        public static int[] hits_total = new int[18];
        public static int[] hits_tight_mass = new int[18];
        public static int i = 0;
        //runner
        public static CalibrationFunction Run_TdMzCal(string filename, string raw_file_path, List<TopDownHit> identifications)
        {
            var myMsDataFile = new ThermoRawFile(raw_file_path);
            myMsDataFile.Open();

            foreach (IMsDataScan<ThermoSpectrum> spectrum in myMsDataFile)
            {
                MsScan scan = new ProteoformSuiteInternal.MsScan(spectrum.MsnOrder, spectrum.OneBasedScanNumber, filename, spectrum.RetentionTime, spectrum.InjectionTime, spectrum.TotalIonCurrent);
                Lollipop.Ms_scans.Add(scan);
            }

            foreach (TopDownHit hit in identifications)
            {
                hit.ms_scan = Lollipop.Ms_scans.Where(s => s.filename == hit.filename && s.scan_number == hit.scan).ToList().First();
                //add intensity and charge info for precursor
                double intensity;
                // myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessChargeStateGuess(out protein_charge); didn't work 
                myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessIntensity(out intensity);
                double mz;
                myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessMonoisotopicMZ(out mz);
                hit.charge = Convert.ToInt16(Math.Round(hit.reported_mass / mz, 0)); //m / (m/z)  round to get charge 
                hit.mz = hit.reported_mass.ToMassToChargeRatio(hit.charge);
                hit.intensity = intensity;
            }


            if (Lollipop.calibrate_td_results && identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList().Count > 0)
            {
                List<LabeledDataPoint> pointList;
                pointList = GetDataPoints(myMsDataFile, identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList());
                if (pointList.Count == 0) return null;
                CalibrationFunction calibration_function = Calibrate(pointList, myMsDataFile, filename);
                myMsDataFile.Close();

                filenames[i] = filename;
                training_points[i] = pointList.Count;
                hits_total[i] = identifications.Count;
                hits_tight_mass[i] = identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList().Count;
                i++;
                return calibration_function;
            }
            else return null;
        }

        //Training points extractor
        public static List<LabeledDataPoint> GetDataPoints(ThermoRawFile myMsDataFile, List<TopDownHit> identifications)
        {
            //final trianing point list to return
            List<LabeledDataPoint> trainingPointsToReturn = new List<LabeledDataPoint>();
            UsefulProteomicsDatabases.Loaders.LoadElements("elements.dat");

            //set of peaks id'd by m/z and RT. Peaks in this list have been part of accepted id's and should be rejected
            HashSet<Tuple<double, double>> peaksAddedFromMS1HashSet = new HashSet<Tuple<double, double>>();
            int numIdentifications = identifications.Count;
                foreach (TopDownHit hit in identifications)
                {
                    Peptide peptideBuilder = new Peptide(hit.sequence);

                    foreach (Ptm ptm in hit.ptm_list)
                    {
                        peptideBuilder.AddModification(new ChemicalFormulaModification(new ChemicalFormulaModification(ptm.modification.chemical_formula_line, ptm.modification.description).ThisChemicalFormula.Formula), ptm.position);
                    }

                    IsotopicDistribution dist = new IsotopicDistribution(peptideBuilder.GetChemicalFormula(), fineResolution, .001);

                    

                    double[] masses = new double[dist.Masses.Count];
                    double[] intensities = new double[dist.Intensities.Count];
                    for (int i = 0; i < dist.Masses.Count; i++)
                    {
                        masses[i] = dist.Masses[i];
                        intensities[i] = dist.Intensities[i];
                    }

                    Array.Sort(intensities, masses, Comparer<double>.Create((x, y) => y.CompareTo(x)));
                    trainingPointsToReturn.AddRange(SearchMS1Spectra(myMsDataFile, masses, intensities, hit.scan, -1, peaksAddedFromMS1HashSet, hit.charge, hit.filename));
                    trainingPointsToReturn.AddRange(SearchMS1Spectra(myMsDataFile, masses, intensities, hit.scan, 1, peaksAddedFromMS1HashSet, hit.charge, hit.filename));
                
            }
                return trainingPointsToReturn;
        }

        private static List<LabeledDataPoint> SearchMS1Spectra(IMsDataFile<IMzSpectrum<MzPeak>> myMsDataFile, double[] originalMasses, double[] originalIntensities, int ms2spectrumIndex, int direction, HashSet<Tuple<double, double>> peaksAddeddHashSet, int protein_charge, string filename)
        {

            List<LabeledDataPoint> myCandidatePoints = new List<LabeledDataPoint>();
            var theIndex = -1;
            if (direction == 1) theIndex = ms2spectrumIndex;
            else theIndex = ms2spectrumIndex - 1;

            bool addedAscan = true;
            int highestKnownChargeForThisProtein = protein_charge;
            while (theIndex >= 1 && theIndex <= myMsDataFile.NumSpectra && addedAscan == true)
            {
                int countForThisScan = 0;
                if (myMsDataFile.GetOneBasedScan(theIndex).MsnOrder > 1)
                {
                    theIndex += direction;
                    continue;
                }
                addedAscan = false;
                List<LabeledDataPoint> myCandidatePointsForThisMS1scan = new List<LabeledDataPoint>();
                var fullMS1scan = myMsDataFile.GetOneBasedScan(theIndex);
                double ms1RetentionTime = fullMS1scan.RetentionTime;
                var scanWindowRange = fullMS1scan.ScanWindowRange;
                var fullMS1spectrum = fullMS1scan.MassSpectrum;
                if (fullMS1spectrum.Count == 0) break;

                bool startingToAddCharges = false;
                int chargeToLookAt = 5; //minimum deconvolution charge

                //TODO: ask stefan why charge of precursor is max charge looked at... for peptides makes sense maybe not for proteins?
                //think of criteria for charges to look at --> or just look at 5 - 30... 
                do
                {
                    if (originalMasses[0].ToMassToChargeRatio(chargeToLookAt) > scanWindowRange.Maximum)
                    {
                        chargeToLookAt++;
                        continue;
                    }
                    if (originalMasses[0].ToMassToChargeRatio(chargeToLookAt) < scanWindowRange.Minimum)
                        break;
                    List<TrainingPoint> trainingPointsToAverage = new List<TrainingPoint>();
                    foreach (double a in originalMasses)
                    {
                        double theMZ = a.ToMassToChargeRatio(chargeToLookAt);

                        var npwr = fullMS1spectrum.NumPeaksWithinRange(theMZ - Lollipop.td_mass_tolerance.ToMassToChargeRatio(chargeToLookAt), theMZ + Lollipop.td_mass_tolerance.ToMassToChargeRatio(chargeToLookAt));
                        if (npwr == 0) break;
                        //  if (npwr > 1) continue;
                        var closestPeak = fullMS1spectrum.GetClosestPeak(theMZ);
                        var closestPeakMZ = closestPeak.MZ;
                        var theTuple = Tuple.Create(closestPeakMZ, ms1RetentionTime);
                        if (!peaksAddeddHashSet.Contains(theTuple))
                        {
                            peaksAddeddHashSet.Add(theTuple);
                            highestKnownChargeForThisProtein = Math.Max(highestKnownChargeForThisProtein, chargeToLookAt);
                            trainingPointsToAverage.Add(new TrainingPoint(new DataPoint(closestPeakMZ, double.NaN, 1, closestPeak.Intensity, double.NaN, double.NaN, filename), closestPeakMZ - theMZ));
                        }
                        else break;
                    }
                    if (trainingPointsToAverage.Count == 0 && startingToAddCharges == true) break; //started adding and suddenly stopped, don't need to look at higher charges
                    if ((trainingPointsToAverage.Count == 0 || (trainingPointsToAverage.Count == 1 && originalIntensities[0] < 0.65)) && (protein_charge <= chargeToLookAt)) break; //didn't find charge, no need to look at higher charges
                    //if (trainingPointsToAverage.Count == 1 && originalIntensities[0] < 0.65) { } //not adding bc intensity too low
                    else if (trainingPointsToAverage.Count < Math.Min(3, originalIntensities.Count()))
                    {
                        //not adding bc count of training points to average is too low
                    }
                    else
                    {
                        addedAscan = true;
                        startingToAddCharges = true;
                        countForThisScan += 1;
                        double[] inputs = new double[6] {1,  trainingPointsToAverage.Select(b => b.dp.mz).Average(), fullMS1scan.RetentionTime, trainingPointsToAverage.Select(b => b.dp.intensity).Average(), fullMS1scan.TotalIonCurrent, fullMS1scan.InjectionTime };
                        var a = new LabeledDataPoint(inputs, trainingPointsToAverage.Select(b => b.l).Median());
                        myCandidatePointsForThisMS1scan.Add(a);
                    }

                    chargeToLookAt++;
                } while (chargeToLookAt <= highestKnownChargeForThisProtein + 1); 

                myCandidatePoints.AddRange(myCandidatePointsForThisMS1scan);

                theIndex += direction;

            }
                return myCandidatePoints;
        }

        //Calibration
        private static CalibrationFunction Calibrate(List<LabeledDataPoint> trainingPoints, ThermoRawFile myMsDataFile, string filename)
        {
            var rnd = new Random(1);
            var shuffledTrainingPoints = trainingPoints.OrderBy(item => rnd.Next()).ToArray();

            var trainList = shuffledTrainingPoints.Take(trainingPoints.Count * 3 / 4).ToList();
            var testList = shuffledTrainingPoints.Skip(trainingPoints.Count * 3 / 4).ToList();

            CalibrationFunction bestMS1predictor = new IdentityCalibrationFunction();
            double bestMS1MSE = bestMS1predictor.getMSE(testList);

            CalibrationFunction ms1regressor = new ConstantCalibrationFunction();
            ms1regressor.Train(trainList);
            double MS1mse = ms1regressor.getMSE(testList);

            if (MS1mse < bestMS1MSE)
            {
                bestMS1MSE = MS1mse;
                bestMS1predictor = ms1regressor;
            }

            List<TransformFunction> transforms = new List<TransformFunction>();

            transforms.Add(new TransformFunction(b => new double[1] { b[1] }, 1, "TFFFF"));
            transforms.Add(new TransformFunction(b => new double[1] { b[2] }, 1, "FTFFF"));
            transforms.Add(new TransformFunction(b => new double[1] { Math.Log(b[3]) }, 1, "FFTFF"));
            transforms.Add(new TransformFunction(b => new double[1] { Math.Log(b[4]) }, 1, "FFFTF"));
            transforms.Add(new TransformFunction(b => new double[1] { Math.Log(b[5]) }, 1, "FFFFT"));

            transforms.Add(new TransformFunction(b => new double[2] { b[1], b[2] }, 2, "TTFFF"));
            transforms.Add(new TransformFunction(b => new double[2] { b[1], Math.Log(b[3]) }, 2, "TFTFF"));
            transforms.Add(new TransformFunction(b => new double[2] { b[1], Math.Log(b[4]) }, 2, "TFFTF"));
            transforms.Add(new TransformFunction(b => new double[2] { b[1], Math.Log(b[5]) }, 2, "TFFFT"));
            transforms.Add(new TransformFunction(b => new double[2] { b[2], Math.Log(b[3]) }, 2, "FTTFF"));
            transforms.Add(new TransformFunction(b => new double[2] { b[2], Math.Log(b[4]) }, 2, "FTFTF"));
            transforms.Add(new TransformFunction(b => new double[2] { b[2], Math.Log(b[5]) }, 2, "FTFFT"));
            transforms.Add(new TransformFunction(b => new double[2] { Math.Log(b[3]), Math.Log(b[4]) }, 2, "FFTTF"));
            transforms.Add(new TransformFunction(b => new double[2] { Math.Log(b[3]), Math.Log(b[5]) }, 2, "FFTFT"));
            transforms.Add(new TransformFunction(b => new double[2] { Math.Log(b[4]), Math.Log(b[5]) }, 2, "FFFTT"));

            transforms.Add(new TransformFunction(b => new double[3] { b[1], b[2], Math.Log(b[3]) }, 3, "TTTFF"));
            transforms.Add(new TransformFunction(b => new double[3] { b[1], b[2], Math.Log(b[4]) }, 3, "TTFTF"));
            transforms.Add(new TransformFunction(b => new double[3] { b[1], b[2], Math.Log(b[5]) }, 3, "TTFFT"));
            transforms.Add(new TransformFunction(b => new double[3] { b[1], Math.Log(b[3]), Math.Log(b[4]) }, 3, "TFTTF"));
            transforms.Add(new TransformFunction(b => new double[3] { b[1], Math.Log(b[3]), Math.Log(b[5]) }, 3, "TFTFT"));
            transforms.Add(new TransformFunction(b => new double[3] { b[1], Math.Log(b[4]), Math.Log(b[5]) }, 3, "TFFTT"));
            transforms.Add(new TransformFunction(b => new double[3] { b[2], Math.Log(b[3]), Math.Log(b[4]) }, 3, "FTTTF"));
            transforms.Add(new TransformFunction(b => new double[3] { b[2], Math.Log(b[3]), Math.Log(b[5]) }, 3, "FTTFT"));
            transforms.Add(new TransformFunction(b => new double[3] { b[2], Math.Log(b[4]), Math.Log(b[5]) }, 3, "FTFTT"));
            transforms.Add(new TransformFunction(b => new double[3] { Math.Log(b[3]), Math.Log(b[4]), Math.Log(b[5]) }, 3, "FFTTT"));

            transforms.Add(new TransformFunction(b => new double[4] { b[1], b[2], Math.Log(b[3]), Math.Log(b[4]) }, 4, "TTTTF"));
            transforms.Add(new TransformFunction(b => new double[4] { b[1], b[2], Math.Log(b[3]), Math.Log(b[5]) }, 4, "TTTFT"));
            transforms.Add(new TransformFunction(b => new double[4] { b[1], b[2], Math.Log(b[4]), Math.Log(b[5]) }, 4, "TTFTT"));
            transforms.Add(new TransformFunction(b => new double[4] { b[1], Math.Log(b[3]), Math.Log(b[4]), Math.Log(b[5]) }, 4, "TFTTT"));
            transforms.Add(new TransformFunction(b => new double[4] { b[2], Math.Log(b[3]), Math.Log(b[4]), Math.Log(b[5]) }, 4, "FTTTT"));

            transforms.Add(new TransformFunction(b => new double[5] { b[1], b[2], Math.Log(b[3]), Math.Log(b[4]), Math.Log(b[5]) }, 5, "TTTTT"));

            try
            {
                foreach (var transform in transforms)
                {
                    ms1regressor = new LinearCalibrationFunctionMathNet(transform);
                    ms1regressor.Train(trainList);
                    MS1mse = ms1regressor.getMSE(testList);
                    if (MS1mse < bestMS1MSE)
                    {
                        bestMS1MSE = MS1mse;
                        bestMS1predictor = ms1regressor;
                    }

                }

                foreach(var transform in transforms)
                {
                    ms1regressor = new QuadraticCalibrationFunctionMathNet(transform);
                    ms1regressor.Train(trainList);
                    MS1mse = ms1regressor.getMSE(testList);
                    if (MS1mse < bestMS1MSE)
                    {
                        bestMS1MSE = MS1mse;
                        bestMS1predictor = ms1regressor;
                    }
                }
            }
            catch { }
            CalibrationFunction bestCf = bestMS1predictor;
            //CalibrateSpectra(bestCf, myMsDataFile);
            return bestCf;
        }
    }
}

