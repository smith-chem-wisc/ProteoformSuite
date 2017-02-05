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
using MathNet.Numerics;


namespace ProteoformSuiteInternal
{
    //based on stefank's program mzcal in MetaMorpheus
    public class TdMzCal
    {
        //parameters
        public static double fineResolution = 0.1;

        public static void get_signal_to_noise(Component comp)
        {
            UsefulProteomicsDatabases.Loaders.LoadElements("elements.dat");

            MsScan scan = Lollipop.Ms_scans.Where(s => s.scan_number == Convert.ToInt16(comp.scan_range.Split('-')[0])).First();
            foreach (ChargeState cs in comp.charge_states)
            {
                double monoisotopic_averagine = 111.0543053;
                double average_averagine = 111.1234625;
                double average_mass = cs.mz_centroid.ToMass(cs.charge_count) / monoisotopic_averagine * average_averagine;
                int units_over = Convert.ToInt16(average_mass - cs.mz_centroid.ToMass(cs.charge_count));
                double mz_average = cs.mz_centroid + (units_over * Lollipop.MONOISOTOPIC_UNIT_MASS / cs.charge_count);
                int index_peak = scan.peak_x.Select((x, i) => new { Index = i, Distance = Math.Abs(mz_average - x) }).OrderBy(x => x.Distance).First().Index;
                cs.signal_to_noise = scan.peak_y[index_peak] / scan.noises[index_peak]; 
            }
            comp.max_signal_to_noise = comp.charge_states.Max(cs => cs.signal_to_noise);
        }


        //runner
        public static Func<double[], double> Run_TdMzCal(string filename, string raw_file_path, List<TopDownHit> identifications)
        {
            var myMsDataFile = new ThermoRawFile(raw_file_path);
            myMsDataFile.Open();

            foreach (IMsDataScan<ThermoSpectrum> spectrum in myMsDataFile)
            {
                MsScan scan = new ProteoformSuiteInternal.MsScan(spectrum.MsnOrder, spectrum.OneBasedScanNumber, filename, spectrum.RetentionTime, spectrum.InjectionTime, spectrum.TotalIonCurrent, spectrum.MassSpectrum.xArray, spectrum.MassSpectrum.yArray, spectrum.MassSpectrum.GetNoises());
                Lollipop.Ms_scans.Add(scan);
            }

            //set charge, mz, intensity, find MS1 numbers
            foreach (TopDownHit hit in identifications)
            {
                hit.ms_scan = Lollipop.Ms_scans.Where(s => s.filename == hit.filename && s.scan_number == hit.scan).ToList().First();
                //add intensity and charge info for precursor
                double intensity;
                // myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessChargeStateGuess(out protein_charge); didn't work 
                myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessMonoisotopicIntensity(out intensity);
                double mz;
                myMsDataFile.GetOneBasedScan(hit.scan).TryGetSelectedIonGuessMonoisotopicMZ(out mz);
                hit.charge = Convert.ToInt16(Math.Round(hit.reported_mass / mz, 0)); //m / (m/z)  round to get charge 
                hit.mz = hit.reported_mass.ToMassToChargeRatio(hit.charge);
                hit.intensity = intensity;
            }

            Func<double[], double> calibration_function = null;
            if (Lollipop.calibrate_td_results && identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList().Count >= 5)
            {
                List<TopDownHit> identifications_tight_mass = identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList();
                //filter out 5% outliers
                List<double> mass_errors = identifications_tight_mass.Select(h => (h.theoretical_mass - h.reported_mass) - Math.Round(h.theoretical_mass - h.reported_mass, 0)).ToList().OrderBy(m => m).ToList();
                double percent_in_window = 1;
                int start = 0; //start index
                int count = identifications_tight_mass.Count; //end index
                while (percent_in_window > .95)  //decrease window until ~95% of points in it
                {
                    List<double> mass_errors_start = mass_errors.GetRange(start + 1, count - 1);
                    double start_range = mass_errors_start.Max() - mass_errors_start.Min();
                    List<double> mass_errors_end = mass_errors.GetRange(start, count - 1);
                    double end_range = mass_errors_end.Max() - mass_errors_end.Min();
                    if (start_range < end_range) start++; //if window smaller from removing first delta m, keep this window
                    count--; //either way count fewer
                    percent_in_window = (double)mass_errors.GetRange(start, count).ToList().Count / mass_errors.Count;
                }
                List<TopDownHit> identifications_to_use = identifications_tight_mass.OrderBy(h => ((h.theoretical_mass - h.reported_mass) - Math.Round(h.theoretical_mass - h.reported_mass, 0))).ToList().GetRange(start, count).ToList();

                List<LabeledDataPoint> pointList = new List<LabeledDataPoint>();
                    pointList = GetDataPoints(myMsDataFile, identifications_to_use);
                    //linear fit, mz correction as a function of retention time and m/z value
                    double[][] variables = new double[pointList.Count()][];
                    int k = 0;
                    foreach (LabeledDataPoint p in pointList)
                    {
                        variables[k] = p.inputs;
                        k++;
                    }

                    var mz_errors = pointList.Select(p => p.output).ToArray();
                    var functions = new Func<double[], double>[3];
                    functions[0] = a => 1;
                    for (int i = 0; i < 2; i++)
                    {
                        int j = i;
                        functions[j + 1] = a => a[j];
                    }
                    calibration_function = Fit.LinearMultiDimFunc(variables, mz_errors, functions);
                    Lollipop.hits = identifications.Count;
                    Lollipop.tight_mass_hits = identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList().Count;
                    Lollipop.training_points = pointList.Count;
                
                myMsDataFile.Close();
            }
            return calibration_function;
        }

        //Training points extractor
        public static List<LabeledDataPoint> GetDataPoints(ThermoRawFile myMsDataFile, List<TopDownHit> identifications)
        {
            List<LabeledDataPoint> trainingPointsToReturn = new List<LabeledDataPoint>();
            UsefulProteomicsDatabases.Loaders.LoadElements("elements.dat");

            foreach (TopDownHit hit in identifications)
            {
                double[] inputs = new double[2] { hit.mz, hit.retention_time};
                var a = new LabeledDataPoint(inputs, (hit.reported_mass - hit.theoretical_mass - Math.Round(hit.reported_mass - hit.theoretical_mass, 0)) / hit.charge);
                trainingPointsToReturn.Add(a);
            }
            return trainingPointsToReturn;
        }
    }
}

