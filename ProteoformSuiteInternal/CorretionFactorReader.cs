using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class CorretionFactorReader
    {
        private List<Correction> raw_correctionFactors_in_file = new List<Correction>();

        public List<Correction> read_components_from_txt(string filename)
        {
            this.raw_correctionFactors_in_file.Clear();
            try
            {
                string line;

                // Read the file and display it line by line.
                using (StreamReader file = new StreamReader(@filename))
                {
                    string fn = file.ReadLine();
                    while ((line = file.ReadLine()) != null)
                    {
                        Correction correction = new Correction();
                        char[] delimiters = new char[] { '\t' };
                        string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                        correction.file_origin = fn;
                        correction.scan_number = Convert.ToInt32(parts[0].ToString());

                        //two corrections can be available for each scan. The correction in column 3 is preferred
                        //if column three is NaN, then column 2 is selected.
                        //if column 2 is also NaN, then the correction for the scan will be interpolated from adjacent scans

                        try
                        {
                            correction.correction = Convert.ToDouble(parts[2].ToString());
                        }
                        catch
                        {
                            try
                            {
                                correction.correction = Convert.ToDouble(parts[1].ToString());
                            }
                            catch
                            {
                                correction.correction = Double.NaN;
                            }                           
                        }
                        raw_correctionFactors_in_file.Add(correction);
                    }

                    file.Close();
                }
            }
            catch (IOException ex) { throw new IOException(ex.Message); }

            return raw_correctionFactors_in_file;
        }      
    }
}
