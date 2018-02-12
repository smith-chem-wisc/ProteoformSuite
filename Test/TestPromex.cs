using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.IO;
using Proteomics;
using System.Linq;
using System;
using Chemistry;
using PRISM;
using CommandLine;

namespace Test
{
    [TestFixture]

    class TestPromex
    {
        [Test]
        public void Test_Promex()
        {
            Sweet.lollipop = new Lollipop();

            bool success = false;

            string input = @"C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw";
            string output = @"C:\Users\j0lte\Desktop\ProMex_Test_Results";
            Lollipop.runcommand("ProMex.exe", "-i " + input + " -o " + output + " -minCharge 2 -maxCharge 60 -minMass 3000 -maxMass 50000 -score n -csv n -maxThreads 0");

            if(File.Exists(@"C:\Users\j0lte\Desktop\ProMex_Test_Results\05-26-17_B7A_yeast_td_fract5_rep1_MS1.ms1ft") == true)
            {
                success = true;
            }

            Assert.IsTrue(success);
            //Goal: ProMex.exe -i C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw -o C:\Users\j0lte\Desktop\ProMex_Test_Results -minCharge 2 -maxCharge 60 -minMass 3000 -maxMass 50000 -score n -csv n -maxThreads 0







            //Sweet.lollipop = new Lollipop();

            //string[] parameters = new string[9];

            //parameters[0] = @"C:\Users\j0lte\Desktop\Informed_Proteomics\ProMex\bin\Debug\ProMex.exe";
            //parameters[1] = @"C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw";
            //parameters[2] = "2";
            //parameters[3] = "60";
            //parameters[4] = "3000";
            //parameters[5] = "50000";
            //parameters[6] = "n";
            //parameters[7] = "n";
            //parameters[8] = "0";

            ////args = new string[] {"-i", @"QC_Shew_Intact_26Sep14_Bane_C2Column3.pbf", "-minMass", "3000", "-maxMass", "30000"};
            //bool truth = Sweet.lollipop.Promex(parameters);
            ////ProMex.exe -i MyDataset.pbf -minCharge 2 -maxCharge 60 -minMass 3000 -maxMass 50000 -score n -csv n -maxThreads 0

            //Assert.IsTrue(truth);

            ////String input = "";
            ////String output = "";
            ////int minCharge = 2;
            ////int maxCharge = 60;
            ////int minMass = 3000;
            ////int maxMass = 50000;
            ////String score = "n";
            ////int maxThreads = 0;
            ////String csv = "n";
        }

    }
}
