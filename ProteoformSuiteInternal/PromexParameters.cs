using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ProteoformSuiteInternal
{
    public class PromexParameters
    {
        [Option(Required = true, HelpText = "Location of Promex executable")]
        public string ExePath { get; set; }

        [Option("i", Required = true, HelpText = "Input file or input folder; supports .pbf, .mzML, and several vendor formats (see documentation)")]
        public string InputPath { get; set; }

        [Option("minCharge", HelpText = "Minimum charge state")] //Min = 1, Max = 60)]
        public int MinSearchCharge { get; set; }  

        [Option("maxCharge", HelpText = "Maximum charge state")]// Min = 1, Max = 60)]
        public int MaxSearchCharge { get; set; }

        [Option("minMass", HelpText = "Minimum mass in Da")] //Min = 600, Max = 100000)]
        public double MinSearchMass { get; set; }

        [Option("maxMass", HelpText = "Maximum mass in Da")] // Min = 600, Max = 100000)]
        public double MaxSearchMass { get; set; }

        [Option("score", HelpText = "Output extended scoring information")]
        public bool ScoreReport { get; set; }

        [Option("csv", HelpText = "Also write feature data to a CSV file")]
        public bool CsvOutput { get; set; }

        [Option("maxThreads", HelpText = "Max number of threads to use (Default: 0 (automatically determine the number of threads to use))" )]  //, Min = 0)]
        public int MaxThreads { get; set; }

        //\ProMex.exe -i file.mzml -minCharge 2 -maxCharge 60 -minMass 3000 -maxMass 50000 -score n -csv n -maxThreads 0
    }
}
