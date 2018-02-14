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
        public void testpromex()
        {
            Sweet.lollipop = new Lollipop();
            bool success = false;

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);

            //string input = @"C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw";
            //string output = @"C:\Users\j0lte\Desktop\ProMex_Test_Results";
            Sweet.lollipop.promex_deconvolute();

            if(File.Exists(@"C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\bin\Debug\05-26-17_B7A_yeast_td_fract5_rep1_MS1_ms1ft.csv") == true)
            {
                success = true;
            }

            Assert.IsTrue(success);
            //Goal: ProMex.exe -i C:\Users\j0lte\Documents\GitClones\ProteoformSuite\Test\05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw -o C:\Users\j0lte\Desktop\ProMex_Test_Results -minCharge 2 -maxCharge 60 -minMass 3000 -maxMass 50000 -score n -csv n -maxThreads 0
        }

        [Test]
        public void testfileconversion()
        {
            Sweet.lollipop = new Lollipop();
            string filelocation = @"C:\Users\j0lte\Desktop\ProMex_Test_Results\05-26-17_B7A_yeast_td_fract5_rep1_MS1.ms1ft";

            Sweet.lollipop.convertxml(filelocation);
        }

    }
}
