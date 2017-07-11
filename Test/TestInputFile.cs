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
                new InputFile("fake.txt", Purpose.Calibration),
                new InputFile("fake.txt", Purpose.Calibration),
                new InputFile("fake.txt", Purpose.Quantification),
                new InputFile("fake.txt", Purpose.Quantification),
            };
            Assert.AreEqual(3, Sweet.lollipop.get_files(files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Count());
        }

        [Test]
        public void enter_directory_of_files()
        {
            string[] folder = new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory") };
            List<string> extension = new List<string> { ".xlsx" };
            List<Purpose> purpose = new List<Purpose> { Purpose.Identification };
            List<InputFile> destination = new List<InputFile>();
            Sweet.lollipop.enter_input_files(folder, extension, purpose, destination, false);
            Assert.AreEqual(2, destination.Count);
            Assert.True(destination.All(f => f.extension == extension[0]));
            Assert.True(destination.All(f => f.purpose == purpose[0]));
        }

        [Test]
        public void enter_directory_of_files_updated_with_presets()
        {
            string[] folder = new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory") };
            List<string> extension = new List<string> { ".xlsx" };
            List<Purpose> purpose = new List<Purpose> { Purpose.Identification };
            List<InputFile> destination = new List<InputFile>();
            InputFile mock = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory", "one.xlsx"), Purpose.Identification);
            Sweet.change_file(mock, mock.lt_condition, nameof(mock.lt_condition), mock.lt_condition, "Normal");
            Sweet.change_file(mock, mock.hv_condition, nameof(mock.hv_condition), mock.hv_condition, "Stress");
            Sweet.change_file(mock, mock.biological_replicate, nameof(mock.biological_replicate), mock.biological_replicate.ToString(), "2");
            Sweet.lollipop.enter_input_files(folder, extension, purpose, destination, true);
            Assert.AreEqual(2, destination.Count);
            Assert.True(destination.All(f => f.extension == extension[0]));
            Assert.True(destination.All(f => f.purpose == purpose[0]));
            Assert.True(destination.Where(f => f.filename.StartsWith("one")).All(f => f.lt_condition == "Normal"));
            Assert.True(destination.Where(f => f.filename.StartsWith("one")).All(f => f.hv_condition == "Stress"));
            Assert.True(destination.Where(f => f.filename.StartsWith("one")).All(f => f.biological_replicate == 2));
        }

        [Test]
        public void add_file_from_presets()
        {
            List<InputFile> destination = new List<InputFile>();
            InputFile mock = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_directory", "one.xlsx"), Purpose.Identification);
            Sweet.add_file_action(mock);
            Sweet.change_file(mock, mock.lt_condition, nameof(mock.lt_condition), mock.lt_condition, "Normal");
            Sweet.change_file(mock, mock.hv_condition, nameof(mock.hv_condition), mock.hv_condition, "Stress");
            Sweet.change_file(mock, mock.biological_replicate, nameof(mock.biological_replicate), mock.biological_replicate.ToString(), "2");
            Sweet.add_files_from_presets(destination);
            Assert.AreEqual(1, destination.Count);
            Assert.True(destination[0].lt_condition == "Normal");
            Assert.True(destination[0].hv_condition == "Stress");
            Assert.True(destination[0].biological_replicate == 2);
        }

        [Test]
        public void matching_calibration_one_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            Assert.False(c.matchingCalibrationFile);
            Assert.False(i.matchingCalibrationFile);
            c.filename = "hello";
            i.filename = "hello";
            Sweet.lollipop.input_files = new List<InputFile> { c, i };
            Sweet.lollipop.match_calibration_files();
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
            Sweet.lollipop.input_files = new List<InputFile> { c, i, j };
            Assert.AreNotEqual("", Sweet.lollipop.match_calibration_files());
        }

        [Test]
        public void matching_calibration_no_match()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            InputFile i = new InputFile("fake.txt", Purpose.Identification);
            c.filename = "hello";
            i.filename = "hey";
            Sweet.lollipop.input_files = new List<InputFile> { c, i };
            Sweet.lollipop.match_calibration_files();
            Assert.False(c.matchingCalibrationFile);
            Assert.False(i.matchingCalibrationFile);
        }

        [Test]
        public void matching_calibration_none_possible()
        {
            InputFile c = new InputFile("fake.txt", Purpose.Calibration);
            Sweet.lollipop.input_files = new List<InputFile> { c };
            Assert.AreNotEqual("", Sweet.lollipop.match_calibration_files());
        }
    }
}
