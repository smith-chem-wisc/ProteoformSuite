using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System.IO;

namespace Test
{
    [TestFixture]
    class TestInputFile
    {
        [Test]
        public void test_get_input_files_with_multiple_purposes()
        {
            List<InputFile> files = new List<InputFile> {
                new InputFile("fake.txt", Purpose.Identification),
                new InputFile("fake.txt", Purpose.Identification),
                new InputFile("fake.txt", Purpose.PtmList),
                new InputFile("fake.txt", Purpose.PtmList),
                new InputFile("fake.txt", Purpose.ProteinDatabase),
                new InputFile("fake.txt", Purpose.RawFile),
                new InputFile("fake.txt", Purpose.RawFile),
                new InputFile("fake.txt", Purpose.Quantification),
                new InputFile("fake.txt", Purpose.Quantification),
            };
            Assert.AreEqual(3, Lollipop.get_files(files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Count());
        }

        [Test]
        public void enter_directory_of_files()
        {
            string[] folder = new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory") };
            List<string> extension = new List<string> { ".xlsx" };
            List<Purpose> purpose = new List<Purpose> { Purpose.Identification };
            List<InputFile> destination = new List<InputFile>();
            Lollipop.enter_input_files(folder, extension, purpose, destination);
            Assert.AreEqual(2, destination.Count);
            Assert.True(destination.All(f => f.extension == extension[0]));
            Assert.True(destination.All(f => f.purpose == purpose[0]));
        }

        [Test]
        public void matching_calibration_one_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.RawFile);
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
            InputFile c = new InputFile("fake.txt", Purpose.RawFile);
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
            InputFile c = new InputFile("fake.txt", Purpose.RawFile);
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
            InputFile c = new InputFile("fake.txt", Purpose.RawFile);
            Lollipop.input_files = new List<InputFile> { c };
            Assert.AreNotEqual("", Lollipop.match_calibration_files());
        }
    }
}
