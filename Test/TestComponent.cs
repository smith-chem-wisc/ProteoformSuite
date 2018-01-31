using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    class TestComponent
    {
        Component testComponent1 = new Component();

        [Test]
        public void testComponentCreationFromFile()
        {
            List<string> component1Values = new List<string> { "2", "1000.001", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile1 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent1 = new Component(component1Values, componentInputFile1);
            Assert.AreEqual(componentInputFile1.UniqueId + "_2", createdComponent1.id);
            Assert.AreEqual(1000.001d, createdComponent1.reported_monoisotopic_mass);
            Assert.AreEqual(99.9d, createdComponent1.intensity_reported);
            Assert.AreEqual(5, createdComponent1.num_charge_states);
            Assert.AreEqual(6, createdComponent1.num_detected_intervals);
            Assert.AreEqual(0.02d, createdComponent1.reported_delta_mass);
            Assert.AreEqual(88.8d, createdComponent1.relative_abundance);
            Assert.AreEqual(0.888, createdComponent1.fract_abundance);
            Assert.AreEqual("10-15", createdComponent1.scan_range);
            Assert.AreEqual("11.1-12.2", createdComponent1.rt_range);
            Assert.AreEqual(10.55d, createdComponent1.rt_apex);
            Assert.AreEqual(99.9d, createdComponent1.intensity_sum);
            Assert.AreEqual(true, createdComponent1.accepted);
            Assert.AreEqual(0, createdComponent1.charge_states.Count());
        }

        [Test]
        public void testComponentCreationFromChargeStates()
        {
            List<string> component2Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile2 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent2 = new Component(component2Values, componentInputFile2);

            ChargeState cs1 = new ChargeState(10, 200d, 100d);//(int charge_count, double intensity, double mz_centroid, double mz_correction)
            createdComponent2.charge_states.Add(cs1);
            createdComponent2.calculate_properties();
            Assert.AreEqual(componentInputFile2.UniqueId + "_2", createdComponent2.id);
            Assert.AreEqual(989.9d, createdComponent2.reported_monoisotopic_mass);
            Assert.AreEqual(99.9d, createdComponent2.intensity_reported);
            Assert.AreEqual(200d, createdComponent2.intensity_sum);
            Assert.AreEqual(1, createdComponent2.num_charge_states);
            Assert.AreEqual(6, createdComponent2.num_detected_intervals);
            Assert.AreEqual(0.02d, createdComponent2.reported_delta_mass);
            Assert.AreEqual(88.8d, createdComponent2.relative_abundance);
            Assert.AreEqual(0.888, createdComponent2.fract_abundance);
            Assert.AreEqual("10-15", createdComponent2.scan_range);
            Assert.AreEqual("11.1-12.2", createdComponent2.rt_range);
            Assert.AreEqual(10.55d, createdComponent2.rt_apex);
            Assert.AreEqual(200d, createdComponent2.intensity_sum);
            Assert.AreEqual(true, createdComponent2.accepted);
            Assert.AreEqual(1, createdComponent2.charge_states.Count());
        }

        [Test]
        public void testComponentMergeSameMass()
        {
            List<string> component3Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile3 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent3 = new Component(component3Values, componentInputFile3);
            ChargeState cs1 = new ChargeState(10, 200d, 100d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent3.charge_states.Add(cs1);

            List<string> component4Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            Component createdComponent4 = new Component(component3Values, componentInputFile3);
            ChargeState cs2 = new ChargeState(11, 300d, 91.0006615d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent4.charge_states.Add(cs1);
            createdComponent4.charge_states.Add(cs2);

            createdComponent3.mergeTheseComponents(createdComponent4);

            Assert.AreEqual(componentInputFile3.UniqueId + "_2", createdComponent3.id);
            Assert.AreEqual(989.9d, createdComponent3.reported_monoisotopic_mass);
            Assert.AreEqual(99.9d, createdComponent3.intensity_reported);
            Assert.AreEqual(700d, createdComponent3.intensity_sum);
            Assert.AreEqual(2, createdComponent3.num_charge_states);
            Assert.AreEqual(true, createdComponent3.accepted);
            Assert.AreEqual(2, createdComponent3.charge_states.Count());
        }

        [Test]
        public void testComponentMergeDifferentMassLarger()
        {
            List<string> component3Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile3 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent3 = new Component(component3Values, componentInputFile3);
            ChargeState cs1 = new ChargeState(10, 200d, 100d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent3.charge_states.Add(cs1);

            List<string> component4Values = new List<string> { "2", "990.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            Component createdComponent4 = new Component(component3Values, componentInputFile3);
            ChargeState cs2 = new ChargeState(10, 300d, 100.10023d);//(int charge_count, double intensity, double mz_centroid)
            ChargeState cs3 = new ChargeState(11, 500d, 91.09177968d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent4.charge_states.Add(cs2);
            createdComponent4.charge_states.Add(cs3);

            createdComponent3.mergeTheseComponents(createdComponent4);

            Assert.AreEqual(1000d, createdComponent3.intensity_sum);
            Assert.AreEqual(Math.Round(989.92723551500012d, 5),Math.Round(createdComponent3.weighted_monoisotopic_mass,5));
            Assert.AreEqual(2, createdComponent3.num_charge_states);
            Assert.AreEqual(true, createdComponent3.accepted);
            Assert.AreEqual(2, createdComponent3.charge_states.Count());
        }

        [Test]
        public void testComponentMergeDifferentMassSmaller()
        {
            List<string> component3Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile3 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent3 = new Component(component3Values, componentInputFile3);
            ChargeState cs1 = new ChargeState(10, 200d, 100d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent3.charge_states.Add(cs1);

            List<string> component4Values = new List<string> { "2", "990.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            Component createdComponent4 = new Component(component3Values, componentInputFile3);
            ChargeState cs2 = new ChargeState(10, 300d, 99.89977d);//(int charge_count, double intensity, double mz_centroid)
            ChargeState cs3 = new ChargeState(11, 500d, 90.90954331d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent4.charge_states.Add(cs2);
            createdComponent4.charge_states.Add(cs3);

            createdComponent3.mergeTheseComponents(createdComponent4);

            Assert.AreEqual(1000d, createdComponent3.intensity_sum);
            Assert.AreEqual(Math.Round(989.92723551500012d, 5), Math.Round(createdComponent3.weighted_monoisotopic_mass, 5));
            Assert.AreEqual(2, createdComponent3.num_charge_states);
            Assert.AreEqual(true, createdComponent3.accepted);
            Assert.AreEqual(2, createdComponent3.charge_states.Count());
        }

        [Test]
        public void testComponentMergeDifferentChargeState()
        {
            List<string> component3Values = new List<string> { "2", "989.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            InputFile componentInputFile3 = new InputFile("fake.txt", Purpose.Identification);
            Component createdComponent3 = new Component(component3Values, componentInputFile3);
            ChargeState cs1 = new ChargeState(10, 200d, 100d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent3.charge_states.Add(cs1);

            List<string> component4Values = new List<string> { "2", "990.9", "99.9", "5", "6", "0.02", "88.8", "0.888", "10-15", "11.1-12.2", "10.55" };
            Component createdComponent4 = new Component(component3Values, componentInputFile3);
            ChargeState cs2 = new ChargeState(11, 500d, 91.09177968d);//(int charge_count, double intensity, double mz_centroid)
            createdComponent4.charge_states.Add(cs2);

            createdComponent3.mergeTheseComponents(createdComponent4);

            Assert.AreEqual(700d, createdComponent3.intensity_sum);
            Assert.AreEqual(Math.Round(989.92723551500012d, 5), Math.Round(createdComponent3.weighted_monoisotopic_mass, 5));
            Assert.AreEqual(2, createdComponent3.num_charge_states);
            Assert.AreEqual(true, createdComponent3.accepted);
            Assert.AreEqual(2, createdComponent3.charge_states.Count());
        }

        [Test]
        public void testSetValuesWithExistingChargeStates()
        {
            ChargeState chargeState1 = new ChargeState(10, 1, 100);
            testComponent1.charge_states.Add(chargeState1);
            testComponent1.calculate_properties();
            Assert.AreEqual(1, testComponent1.num_charge_states);
            Assert.That(() => testComponent1.num_charge_states = 2, Throws.TypeOf<ArgumentException>()
                    .With.Property("Message")
                    .EqualTo("Charge state data exists that can't be overwritten with input"));
            Assert.That(() => testComponent1.intensity_sum = 2d, Throws.TypeOf<ArgumentException>()
                    .With.Property("Message")
                    .EqualTo("Charge state data exists that can't be overwritten with intensity input"));
            Assert.That(() => testComponent1.weighted_monoisotopic_mass = 2d, Throws.TypeOf<ArgumentException>()
                    .With.Property("Message")
                    .EqualTo("Charge state data exists that can't be overwritten with mass input"));
        }
    }

    [TestFixture]
    class TestChargeState
    {
        ChargeState testChargeStateNoCorrection = new ChargeState(10, 1, 100);
        ChargeState testChargeStateList = new ChargeState(new List<string> { Convert.ToString(10), Convert.ToString(1), Convert.ToString(100), Convert.ToString(10000) });
        ChargeState testChargeStateWithCorrection = new ChargeState(10, 1, 100.1);
        ChargeState mergeChargeState1 = new ChargeState(10, 1, 100);
        ChargeState mergeChargeState2 = new ChargeState(10, 1, 100.1);

        [Test]
        public void testUncorrected()
        {
            Assert.AreEqual(10, testChargeStateNoCorrection.charge_count);
            Assert.AreEqual(1d, testChargeStateNoCorrection.intensity);
            Assert.AreEqual(100d, testChargeStateNoCorrection.mz_centroid);
            Assert.AreEqual(Math.Round(989.9272355,5), Math.Round(testChargeStateNoCorrection.calculated_mass),5);
        }

        [Test]
        public void testListUncorrected()
        {
            Assert.AreEqual(10, testChargeStateList.charge_count);
            Assert.AreEqual(1d / 10, testChargeStateList.intensity); //charge state normalized
            Assert.AreEqual(100d, testChargeStateList.mz_centroid);
            Assert.AreEqual(Math.Round(989.92723555, 5), Math.Round(testChargeStateList.calculated_mass), 5);
        }

        [Test]
        public void testCorrected()
        {
            Assert.AreEqual(10, testChargeStateWithCorrection.charge_count);
            Assert.AreEqual(1d, testChargeStateWithCorrection.intensity);
            Assert.AreEqual(100.1d, testChargeStateWithCorrection.mz_centroid);
            Assert.AreEqual(Math.Round(990.9272355, 5), Math.Round(testChargeStateWithCorrection.calculated_mass, 5));
        }

        [Test]
        public void testMerge()
        {
            mergeChargeState1.mergeTheseChargeStates(mergeChargeState2);
            Assert.AreEqual(10, mergeChargeState1.charge_count);
            Assert.AreEqual(2d, mergeChargeState1.intensity);
            Assert.AreEqual(Math.Round(990.4272355, 5), Math.Round(mergeChargeState1.calculated_mass, 5));
            Assert.AreEqual(100.05d, Math.Round(mergeChargeState1.mz_centroid,2));
        }

        [Test]
        public void testToString()
        {
            Assert.AreEqual("10\t1", testChargeStateNoCorrection.ToString());
        }
    }
}
