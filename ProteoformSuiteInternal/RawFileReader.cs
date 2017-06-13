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
    public class RawFileReader
    {
        //GET MS SCANS
        public static Dictionary<Tuple<string, double>, MsScan> get_ms_scans(Dictionary<Tuple<string, double>, MsScan> ms_scans, string filename, string raw_file_path)
        {
            ThermoStaticData myMsDataFile = ThermoStaticData.LoadAllStaticData(raw_file_path);
            Parallel.ForEach(myMsDataFile, spectrum =>
            {
                MsScan scan = new MsScan(spectrum.MsnOrder, spectrum.OneBasedScanNumber, filename, spectrum.RetentionTime, Convert.ToDouble(spectrum.InjectionTime), spectrum.TotalIonCurrent, spectrum.MassSpectrum.GetNoises(), spectrum.MassSpectrum.XArray, spectrum.MassSpectrum.YArray);
                if (scan.ms_order == 2) scan.precursor_mz = (spectrum as ThermoScanWithPrecursor).IsolationMz;
                lock (ms_scans)
                {
                    ms_scans.Add((new Tuple<string, double>(filename, scan.retention_time)), scan);
                }
            });
            //set charge, mz, intensity, find MS1 numbers
            Parallel.ForEach(SaveState.lollipop.td_hits_calibration.Where(f => f.filename == filename).ToList(), hit =>
            {
                double mz = (myMsDataFile.GetOneBasedScan(hit.ms2ScanNumber) as ThermoScanWithPrecursor).IsolationMz;
                hit.charge = Convert.ToInt16(Math.Round(hit.reported_mass / (double)mz, 0)); //m / (m/z)  round to get charge 
                hit.mz = hit.reported_mass.ToMz(hit.charge);
                hit.ms1Scan = ms_scans[new Tuple<string, double> (filename, myMsDataFile.Where(s => hit.ms2ScanNumber > s.OneBasedScanNumber && s.MsnOrder == 1).OrderByDescending(s => s.OneBasedScanNumber).First().RetentionTime)];
            });
            return ms_scans;
        }

        public static void check_fragmented_experimentals(List<InputFile> files)
        {
            Dictionary<Tuple<string, double>, MsScan> ms_scans = new Dictionary<Tuple<string, double>, MsScan>();
            foreach (InputFile file in files)
                ms_scans = get_ms_scans(ms_scans, file.filename, file.complete_path);
            Parallel.ForEach(SaveState.lollipop.target_proteoform_community.experimental_proteoforms, e =>
            {
                foreach (Component comp in e.aggregated_components)
                {
                    if (!e.fragmented)
                    {

                        foreach (ChargeState cs in comp.charge_states)
                        {
                            //if isolation m/z is less than 1.5 m/z away and rt is within 5 minutes, consider fragmented
                            if (ms_scans.Values.Count(s => s.ms_order == 2 && Math.Abs(cs.mz_centroid - s.precursor_mz) <= 1.5 && Math.Abs(s.retention_time - Convert.ToDouble(comp.rt_range.Split('-')[0])) < 5) > 0)
                            {
                                e.fragmented = true;
                                break;
                            }
                        }
                    }
                }
            });
        }
    }
}
