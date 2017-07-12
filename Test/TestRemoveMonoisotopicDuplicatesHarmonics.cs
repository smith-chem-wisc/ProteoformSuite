using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    class TestRemoveMonoisotopicDuplicatesHarmonics
    {

        ComponentReader cr = new ComponentReader();
        Lollipop L = new Lollipop();
        List<Component> cList = new List<Component>();

        [Test]
        public void CompressMissedMonoisotopics()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleMissedMonoisotopicsList =
                        Enumerable.Range(-3, 7).Select(x =>
                        1000d + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS).ToList();

            int counter = 0;
            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;
                if (counter == 3)
                {
                    ChargeState cs1 = new ChargeState(10, 100d, (mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs1);
                }
                else
                {
                    ChargeState cs1 = new ChargeState(10, 50d, (mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs1);
                }

                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(1, compressed.Count);
            Assert.AreEqual(400d, compressed.FirstOrDefault().intensity_sum);

            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass + 1000d;

                if (counter == 10)
                {
                    ChargeState cs1 = new ChargeState(10, 100d, (c.weighted_monoisotopic_mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs1);
                }
                else
                {
                    ChargeState cs1 = new ChargeState(10, 50d, (c.weighted_monoisotopic_mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs1);
                }

                c.scan_range = "3-4";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            compressed = cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList);

            Assert.AreEqual(2, compressed.Count);
        }

        [Test]
        public void CompressHarmonics()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleHarmonicList = // 2 missed on the top means up to 4 missed monos on the 2nd harmonic and 6 missed monos on the 3rd harmonic
                        Enumerable.Range(-4, 9).Select(x => (1000d + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 2d).Concat(
                            Enumerable.Range(-6, 13).Select(x => (1000d + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 3d)).ToList();

            possibleHarmonicList.Add(1000d);

            int counter = 0;
            foreach (double mass in possibleHarmonicList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;

                if (mass == 1000d)
                {
                    ChargeState cs1 = new ChargeState(10, 1000d, (c.weighted_monoisotopic_mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs1);
                }
                else
                {
                    if (mass == 1000d / 2d || mass == 1000d / 3d)
                    {
                        ChargeState cs1 = new ChargeState(10, 5d, (c.weighted_monoisotopic_mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs1);
                    }
                    else
                    {
                        ChargeState cs1 = new ChargeState(10, 1d, (c.weighted_monoisotopic_mass + 10d * Lollipop.PROTON_MASS) / 10d);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs1);
                    }
                }

                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(1, compressed.Count);

        }

        [Test]
        public void CompressHarmonicsHighNumberChargeStates()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleMissedMonoisotopicsList = new List<double> { 1000d, 500d };

            int counter = 0;
            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;

                for (int j = 1; j < 6; j++)
                {
                    ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs);
                }
                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(2, compressed.Count);
        }

        [Test]
        public void CompressHarmonicsEqualNumberChargeStates()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleMissedMonoisotopicsList = new List<double> { 1000d, 500d };

            int counter = 0;
            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;

                for (int j = 1; j < 4; j++)
                {
                    ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                    c.charge_states.Add(cs);
                }
                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(1, compressed.Count);
            Assert.AreEqual(1000, Convert.ToInt32(compressed.FirstOrDefault().weighted_monoisotopic_mass));
        }

        [Test]
        public void CompressHarmonicsUnequalNumberChargeStates_HighMassHighChargeStateCount()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleMissedMonoisotopicsList = new List<double> { 1000d, 500d };

            int counter = 0;
            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;

                if (mass == 1000d)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs);
                    }
                }
                else
                {
                    for (int j = 1; j < 3; j++)
                    {
                        ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs);
                    }
                }
                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(1, compressed.Count);
            Assert.AreEqual(1000, Convert.ToInt32(compressed.FirstOrDefault().weighted_monoisotopic_mass));
        }


        [Test]
        public void CompressHarmonicsUnequalNumberChargeStates_LowMassHighChargeStateCount()
        {
            cList.Clear();
            L.neucode_labeled = false;

            List<double> possibleMissedMonoisotopicsList = new List<double> { 1000d, 500d };

            int counter = 0;
            foreach (double mass in possibleMissedMonoisotopicsList)
            {
                Component c = new Component();
                InputFile i = new InputFile("path", Purpose.Identification);
                c.input_file = i;
                c.id = counter.ToString();
                c.weighted_monoisotopic_mass = mass;

                if (mass == 500d)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs);
                    }
                }
                else
                {
                    for (int j = 1; j < 3; j++)
                    {
                        ChargeState cs = new ChargeState(j, 100d, (mass + j * Lollipop.PROTON_MASS) / j);//(int charge_count, double intensity, double mz_centroid)
                        c.charge_states.Add(cs);
                    }
                }
                c.scan_range = "1-2";
                c.calculate_properties();
                cList.Add(c);
                counter++;
            }

            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(1, compressed.Count);
            Assert.AreEqual(500, Convert.ToInt32(compressed.FirstOrDefault().weighted_monoisotopic_mass));
        }

        [Test]
        public void neuCodeHarmonics()
        {
            cList.Clear();
            L.neucode_labeled = true;

            Component c1 = new Component();
            c1.input_file = new InputFile("path", Purpose.Identification);
            c1.id = 1.ToString();
            ChargeState csOne = new ChargeState(10, 200d, 100.8068165d);//(int charge_count, double intensity, double mz_centroid) ** Because of NeuCode correction, this mass is stepped down by 2.0046 Da to make sure it's a neucode pair
            c1.charge_states.Add(csOne);
            c1.scan_range = "1-2";
            c1.calculate_properties();
            cList.Add(c1);

            Component c2 = new Component();
            c2.input_file = new InputFile("path", Purpose.Identification);
            c2.id = 2.ToString();
            ChargeState csTwo = new ChargeState(10, 100d, 101.057698);//(int charge_count, double intensity, double mz_centroid)
            c2.charge_states.Add(csTwo);
            c2.scan_range = "1-2";
            c2.calculate_properties();
            cList.Add(c2);

            Component c3 = new Component();
            c3.input_file = new InputFile("path", Purpose.Identification);
            c3.id = 3.ToString();
            ChargeState csThree = new ChargeState(10, 50d, 201.1081195d);//(int charge_count, double intensity, double mz_centroid)
            c3.charge_states.Add(csThree);
            c3.scan_range = "1-2";
            c3.calculate_properties();
            cList.Add(c3);


            List<Component> compressed = new List<Component>(cr.remove_monoisotopic_duplicates_harmonics_from_same_scan(cList));

            Assert.AreEqual(2, compressed.Count);
        }

    }
}
