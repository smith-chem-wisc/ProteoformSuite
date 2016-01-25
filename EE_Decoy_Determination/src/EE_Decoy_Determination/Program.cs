using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EE_Decoy_Determination
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<double, int> EE_values = new Dictionary<double, int>(); // mass is key and lysine count is value
            EE_values = Get_EE_Values();

        }

        public static bool ValidateFilePath(string path, bool RequireDirectory, bool IncludeFileName, bool RequireFileName = false)
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
                validFile = ValidateFilePath(filePath, true, true, true);// filepath, bool RequireDirectory, bool IncludeFileName, bool RequireFileName = false
            }
            return filePath;
        }

        static Dictionary<double, int> Get_EE_Values()
        {
            Dictionary<double, int> EE_values = new Dictionary<double, int>();
            string EE_file_path = null;
            bool validPath = false;

            while (validPath == false)
            {
                EE_file_path = GetValidEE_FilePath();

                string line1;

                using (StreamReader test_reader = new StreamReader(EE_file_path))
                {
                    line1 = test_reader.ReadLine();
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


            using (StreamReader reader = new StreamReader(EE_file_path))
            {
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] lineElements;
                    lineElements = line.Split('\t');
                    double mass = double.Parse(lineElements[0]);
                    int lysCt = Int32.Parse(lineElements[1]);

                    EE_values.Add(mass, lysCt);

                }//End of file reading and input of data into memory
                 //reader.Close();
            }
            return EE_values;
        }
    }
}
