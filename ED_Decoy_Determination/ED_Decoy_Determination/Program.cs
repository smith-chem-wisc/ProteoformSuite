using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamreadertest
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<double, List<int>> EE_values = new Dictionary<double, List<int>>(); // mass is key and lysine count is value
            string validFileName = GetValidEE_FilePath();
            string outFile = outFileName(validFileName);
            EE_values = Get_EE_Values(validFileName);
            List<double> ED_Values = new List<double>();
            ED_Values = GetEDValues(EE_values);
            WriteEDValuesToOutput(outFile, ED_Values);


        }

        static void WriteEDValuesToOutput(string path, List<double> ED_Values)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (double massDifference in ED_Values)
                {
                    sw.WriteLine(massDifference);
                }

            }
        }

        static List<double> GetEDValues(Dictionary<double, List<int>> EE_values)
        {
            List<double> ED_Values = new List<double>();
            double inMass;
            double outMass;

            foreach (KeyValuePair<double, List<int>> inEntry in EE_values)
            {
                inMass = inEntry.Key;
                foreach (int inK in inEntry.Value)
                {
                    foreach (KeyValuePair<double, List<int>> outEntry in EE_values)
                    {
                        outMass = outEntry.Key;
                        foreach (int outK in outEntry.Value)
                        {
                            if (outMass > inMass && (outK - inK) > 3 && (outMass - inMass) < 500)
                            {
                                ED_Values.Add(outMass - inMass);
                            }

                        }
                    }

                }
            }


            return ED_Values;
        }

        static string outFileName(string path)
        {
            string outFile = "";
            outFile = path.Replace(".txt", "_ED_Decoys.txt");
            Console.WriteLine("Output: " + outFile);
            Console.Read();
            return outFile;
        }

        static bool checkFileContent(string path)
        {
            bool validPath = false;

            while (validPath == false)
            {

                string line1;

                using (StreamReader test_reader = new StreamReader(path))
                {
                    line1 = test_reader.ReadLine();
                    //Console.WriteLine("line 1 " + line1);
                    //Console.Read();
                }


                try
                {
                    string[] values = line1.Split('\t');
                    double mass;
                    int k_count;
                    if (double.TryParse(values[0], out mass))
                    {
                        if (int.TryParse(values[1], out k_count))
                        {
                            validPath = true;
                            //Console.WriteLine("first mass: " + mass + " first K: " + k_count);
                            //Console.Read();
                        }
                        else { validPath = false; }
                    }
                    else
                    {
                        Console.WriteLine("Data format is invalid");
                        validPath = false;
                    }
                }
                catch
                {
                    validPath = false;
                }
            }


            return true;
        }

        public static bool ValidateFilePath(string path, bool RequireDirectory, bool IncludeFileName, bool checkContents, bool RequireFileName = false)
        {
            if (string.IsNullOrEmpty(path)) { return false; }
            string root = null; ;
            string directory = null;
            string filename = null;
            try
            {
                //throw ArgumentException   - The path parameter contains invalid characters, is empty, or contains only white spaces.
                root = Path.GetPathRoot(path);
                //throw ArgumentException   - path contains one or more of the invalid characters defined in GetInvalidPathChars.
                //    -or- String.Empty was passed to path.
                directory = Path.GetDirectoryName(path);
                //path contains one or more of the invalid characters defined in GetInvalidPathChars
                if (IncludeFileName) { filename = Path.GetFileName(path); }
            }
            catch (ArgumentException)
            {
                return false;
            }
            //null if path is null, or an empty string if path does not contain root directory information
            if (String.IsNullOrEmpty(root)) { return false; }
            //null if path denotes a root directory or is null. Returns String.Empty if path does not contain directory information
            if (String.IsNullOrEmpty(directory)) { return false; }
            if (RequireFileName)
            {
                //f the last character of path is a directory or volume separator character, this method returns String.Empty
                if (String.IsNullOrEmpty(filename)) { return false; }
                //check for illegal chars in filename
                if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { return false; }
            }
            if (Path.GetExtension(path) != ".txt") { return false; } //check that file extension if of type .txt
            if (checkContents) { if (!checkFileContent(path)) { return false; } }
            return true;
        }

        static string GetValidEE_FilePath()
        {
            bool validFile = false;
            string filePath = "";
            while (validFile == false)
            {
                Console.WriteLine("Enter Valid EE DataFile with Path: ");
                filePath = Console.ReadLine();
                validFile = ValidateFilePath(filePath, true, true, true, true);// filepath, bool RequireDirectory, bool IncludeFileName, bool checkcontents bool RequireFileName = false
                //validFile = true;
            }

            return filePath;
        }

        static Dictionary<double, List<int>> Get_EE_Values(string path)
        {


            Dictionary<double, List<int>> EE_values = new Dictionary<double, List<int>>();

            using (StreamReader reader = new StreamReader(path))
            {
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    Console.WriteLine(line);
                    string[] lineElements;
                    lineElements = line.Split('\t');
                    double mass = double.Parse(lineElements[0]);
                    int lysCt = Int32.Parse(lineElements[1]);

                    if (EE_values.ContainsKey(mass))
                    {
                        List<int> kValues = EE_values[mass];
                        kValues.Add(lysCt);
                        EE_values[mass] = kValues;
                    }
                    else
                    {
                        List<int> kValues = new List<int>();
                        kValues.Add(lysCt);
                        EE_values.Add(mass, kValues);
                    }

                    Console.WriteLine("mass: " + mass + " K: " + lysCt);
                    //Console.Read();

                }//End of file reading and input of data into memory
                 //reader.Close();
            }
            return EE_values;
        }

    }
}

