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
        public static Dictionary<Tuple<string, double>, MsScan> get_ms_scans( Dictionary<Tuple<string, double>, MsScan> ms_scans, InputFile file)
        {
            ThermoStaticData myMsDataFile = ThermoStaticData.LoadAllStaticData(file.complete_path);
            Parallel.ForEach(myMsDataFile, spectrum =>
            {
                MsScan scan = new MsScan(spectrum.MsnOrder, spectrum.OneBasedScanNumber, file.filename, spectrum.RetentionTime, Convert.ToDouble(spectrum.InjectionTime), spectrum.TotalIonCurrent, spectrum.MassSpectrum.GetNoises(), spectrum.MassSpectrum.XArray, spectrum.MassSpectrum.YArray);
                if (scan.ms_order == 2) scan.precursor_mz = (spectrum as ThermoScanWithPrecursor).IsolationMz;
                lock (ms_scans)
                {
                    ms_scans.Add((new Tuple<string, double>(file.filename, scan.scan_number)), scan);
                }
            });
            return ms_scans;
        }


            public static bool check_fragmented_experimentals(List<InputFile> files)
        {
            Dictionary<Tuple<string, double>, MsScan> ms_scans = new Dictionary<Tuple<string, double>, MsScan>();
            foreach (InputFile file in files)
                ms_scans = get_ms_scans( ms_scans, file);
           foreach(ExperimentalProteoform e in Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0))
            {
                foreach (Component comp in e.aggregated_components)
                {
                    foreach (ChargeState cs in comp.charge_states)
                    {
                        if (!e.fragmented)
                        {
                            bool in_range = true;
                            double scan_num = Convert.ToDouble(comp.scan_range.Split('-')[0]);
                            do
                            {
                                MsScan scan;
                                lock (ms_scans)
                                {
                                    string filename = comp.input_file.filename.Contains("_calibrated") ? comp.input_file.filename.Substring(0, comp.input_file.filename.IndexOf("_calibrated")) : comp.input_file.filename;
                                    if (!ms_scans.TryGetValue(new Tuple<string, double>(filename, scan_num), out scan)) return false;
                                }

                                if (scan.ms_order == 2 && Math.Abs(cs.mz_centroid - scan.precursor_mz) <= 10)
                                {
                                    e.fragmented = true;
                                    break;
                                }
                                else
                                {
                                    if (scan.ms_order == 1 && scan.scan_number > Convert.ToDouble(comp.scan_range.Split('-')[1])) in_range = false;
                                }
                                scan_num++;
                            } while (in_range);
                        }
                    }
                }
            }
            return true;
        }
    }
}
