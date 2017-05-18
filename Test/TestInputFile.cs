using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Assert.AreEqual(3, SaveState.lollipop.get_files(files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Count());
        }

        [Test]
        public void enter_directory_of_files()
        {
            string[] folder = new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory") };
            List<string> extension = new List<string> { ".xlsx" };
            List<Purpose> purpose = new List<Purpose> { Purpose.Identification };
            List<InputFile> destination = new List<InputFile>();
            SaveState.lollipop.enter_input_files(folder, extension, purpose, destination);
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
            SaveState.lollipop.input_files = new List<InputFile> { c, i };
            SaveState.lollipop.match_calibration_files();
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
            SaveState.lollipop.input_files = new List<InputFile> { c, i, j };
            Assert.AreNotEqual("", SaveState.lollipop.match_calibration_files());
        }

        [Test]
        public void matching_calibration_no_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.RawFile);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            c.filename = "hello";
            i.filename = "hey";
            SaveState.lollipop.input_files = new List<InputFile> { c, i };
            SaveState.lollipop.match_calibration_files();
            Assert.False(c.matchingCalibrationFile);
            Assert.False(i.matchingCalibrationFile);
        }

        [Test]
        public void matching_calibration_none_possible()
        {
            SaveState.lollipop.calibrate_td_results = true;
            InputFile c = new InputFile("fake.xlsx", Purpose.CalibrationIdentification);
            InputFile r = new InputFile("other.raw", Purpose.RawFile);
            SaveState.lollipop.input_files = new List<InputFile> { c, r};
            Assert.AreNotEqual("", SaveState.lollipop.match_calibration_files());
        }
    }
}
