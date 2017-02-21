using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;

namespace Test
{
    [TestFixture]
    class TestInputFile
    {
        [Test]
        public void matching_calibration_one_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            Assert.False(c.matchingCalibrationFile);
            Assert.False(i.matchingCalibrationFile);
            c.filename = "hello";
            i.filename = "hello";
            Lollipop.input_files = new List<InputFile> { c, i };
            Lollipop.match_calibration_files();
            Assert.True(c.matchingCalibrationFile);
            Assert.True(i.matchingCalibrationFile);
        }

        [Test]
        public void matching_calibration_two_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            InputFile j = new InputFile("fake.txt", Purpose.Identification);
            c.filename = "hello";
            i.filename = "hello";
            j.filename = "hello";
            Lollipop.input_files = new List<InputFile> { c, i, j };
            Assert.AreNotEqual("", Lollipop.match_calibration_files());
        }

        [Test]
        public void matching_calibration_no_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            c.filename = "hello";
            i.filename = "hey";
            Lollipop.input_files = new List<InputFile> { c, i };
            Lollipop.match_calibration_files();
            Assert.False(c.matchingCalibrationFile);
            Assert.False(i.matchingCalibrationFile);
        }

        [Test]
        public void matching_calibration_none_possible()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            Lollipop.input_files = new List<InputFile> { c };
            Assert.AreNotEqual("", Lollipop.match_calibration_files());
        }
    }
}
