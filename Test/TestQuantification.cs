using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;

namespace Test
{
    [TestFixture]
    class TestQuantification
    {
        //class global variables here
        public static double starter_rt = 50.0;
        public static int starter_lysine_count = 3;
        List<Component> empty_quant_components_list = new List<Component>();


        //class methods here
        public static List<Component> generate_neucode_components(double mass, double lightIntensity, double heavyIntensity, int lysineCount)
        {
            List<Component> components = new List<Component>();
            InputFile inFile = new ProteoformSuiteInternal.InputFile("somepath", Labeling.NeuCode, Purpose.Identification);

            for (int i = 0; i < 2; i++)
            {
                Component light = new Component();
                Component heavy = new Component();
                light.input_file = inFile;
                heavy.input_file = inFile;
                light.input_file.lt_condition = "light";
                heavy.input_file.hv_condition = "heavy";
                light.input_file.purpose = Purpose.Identification;
                heavy.input_file.purpose = Purpose.Identification;
                light.id = DateTime.Now.Ticks.ToString();
                heavy.id = (DateTime.Now.Ticks + 1).ToString();
                //light.id = 1.ToString();
                //heavy.id = 2.ToString();
                light.weighted_monoisotopic_mass = (mass);
                heavy.weighted_monoisotopic_mass = (mass + lysineCount * Lollipop.NEUCODE_LYSINE_MASS_SHIFT);
                light.intensity_sum_olcs = lightIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
                heavy.intensity_sum_olcs = heavyIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
                light.rt_apex = starter_rt;
                heavy.rt_apex = starter_rt;
                light.accepted = true;
                heavy.accepted = true;
                ChargeState light_charge_state = new ChargeState(1, light.intensity_sum_olcs, light.weighted_monoisotopic_mass, 1.00727645D);
                ChargeState heavy_charge_state = new ChargeState(1, heavy.intensity_sum_olcs, heavy.weighted_monoisotopic_mass, 1.00727645D);
                light.charge_states = new List<ChargeState> { light_charge_state };
                heavy.charge_states = new List<ChargeState> { heavy_charge_state };
                NeuCodePair n = new NeuCodePair(light, heavy);
                n.lysine_count = lysineCount;
                components.Add(n);
            }
            return components;
        }

        public static List<Component> generate_neucode_quantitative_components(double mass, double lightIntensity, double heavyIntensity, int biorep, int lysineCount)
        {
            List<Component> components = new List<Component>();
            InputFile inFile = new InputFile("somepath", Labeling.NeuCode, Purpose.Quantification);
            inFile.biological_replicate = biorep;

            Component light = new Component();
            Component heavy = new Component();
            light.input_file = inFile;
            heavy.input_file = inFile;
            light.input_file.lt_condition = "light";
            heavy.input_file.hv_condition = "heavy";
            light.id = DateTime.Now.Ticks.ToString();
            heavy.id = (DateTime.Now.Ticks + 1).ToString();
            //light.id = 1.ToString();
            //heavy.id = 2.ToString();
            light.weighted_monoisotopic_mass = (mass);
            heavy.weighted_monoisotopic_mass = (mass + lysineCount * Lollipop.NEUCODE_LYSINE_MASS_SHIFT);
            light.intensity_sum  = lightIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
            heavy.intensity_sum = heavyIntensity;
            light.rt_apex = starter_rt;
            heavy.rt_apex = starter_rt;
            light.accepted = true;
            heavy.accepted = true;
            components.Add(light);
            components.Add(heavy);
            return components;
        }

        [Test]
        public void proteoformQuantificationStatistics()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;

            Lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1, lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2, lysineCount));//these are for quantification
            List<Component> components = generate_neucode_components(proteoformMass, intensity, intensity/2d, lysineCount); // these are for indentification
            ExperimentalProteoform e1 = new ExperimentalProteoform("E1", components[0], components, quant_components_list, true);

            Lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            Lollipop.getObservationParameters();


            //e1.make_biorepIntensityList();
            e1.biorepIntensityList = ExperimentalProteoform.make_biorepIntensityList(e1.lt_quant_components, e1.hv_quant_components, Lollipop.ltConditionsBioReps.Keys.ToList(), Lollipop.hvConditionsBioReps.Keys.ToList());
            Assert.AreEqual(4, e1.biorepIntensityList.Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Where(b=>b.biorep == 1).ToList().Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Where(b => b.biorep == 2).ToList().Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Where(b => b.light == true).ToList().Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Where(b => b.light == false).ToList().Count);

            //Lollipop.input_files = quant_components_list.Select(i => i.input_file).ToList();
            Lollipop.getBiorepsFractionsList();

            ExperimentalProteoform.quantitativeValues qval1 = new ExperimentalProteoform.quantitativeValues(e1, (decimal)10.000, (decimal)10.000, "", "", 1);

            Assert.AreEqual(2, qval1.lightBiorepIntensities.Count());
            Assert.AreEqual(2, qval1.heavyBiorepIntensities.Count());
            Assert.AreEqual(200d, qval1.lightBiorepIntensities.Select(i=>i.intensity).Sum());
            Assert.AreEqual(105d, qval1.heavyBiorepIntensities.Select(i => i.intensity).Sum());
            Assert.AreEqual(305d, qval1.intensitySum);
            Assert.AreEqual(0.929610672108602M, qval1.logFoldChange);
            Assert.AreEqual(0.0379131331237966M, qval1.variance);
            //Assert.AreEqual(0.0001d, qval1.pValue);

            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 50d, 100d, 3, lysineCount));//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 48d, 102d, 4, lysineCount));//these are for quantification
            ExperimentalProteoform e2 = new ExperimentalProteoform("E2", components[0], components, quant_components_list, true);
            //e2.make_biorepIntensityList();
            e2.biorepIntensityList = ExperimentalProteoform.make_biorepIntensityList(e2.lt_quant_components, e2.hv_quant_components, Lollipop.ltConditionsBioReps.Keys.ToList(), Lollipop.hvConditionsBioReps.Keys.ToList());
            Lollipop.input_files.Clear();
            Lollipop.input_files = quant_components_list.Select(i => i.input_file).ToList();
            Lollipop.getBiorepsFractionsList();
            ExperimentalProteoform.quantitativeValues qval2 = new ExperimentalProteoform.quantitativeValues(e2, (decimal)10.000, (decimal)10.000, "", "", 1);

            Assert.AreEqual(4, qval2.lightBiorepIntensities.Count());
            Assert.AreEqual(4, qval2.heavyBiorepIntensities.Count());
            Assert.AreEqual(298d, qval2.lightBiorepIntensities.Select(i => i.intensity).Sum());
            Assert.AreEqual(307d, qval2.heavyBiorepIntensities.Select(i => i.intensity).Sum());
            Assert.AreEqual(605d, qval2.intensitySum);
            Assert.AreEqual(-0.0429263249080178M, qval2.logFoldChange);
            Assert.AreEqual(1.97538639776822M, qval2.variance);
            //Assert.Greater(qval2.pValue,0.2M); // permutation returns a varying number.
        }

        [Test]
        public void quantificationInitialization()
        {
            //neucode labelled
            Lollipop.input_files.Clear();
            Lollipop.neucode_labeled = true;
            InputFile i1 = new InputFile("fake.txt", Purpose.Quantification);
            i1.biological_replicate = 1;
            i1.lt_condition = "light";
            i1.hv_condition = "heavy";
            Lollipop.input_files.Add(i1);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i2 = new InputFile("fake.txt", Purpose.Quantification);
            i2.biological_replicate = 2;
            i2.lt_condition = "light";
            i2.hv_condition = "heavy";
            Lollipop.input_files.Add(i2);
            Lollipop.getObservationParameters();
            Assert.AreEqual(2, Lollipop.countOfBioRepsInOneCondition);

            //unlabelled
            Lollipop.neucode_labeled = false;
            Lollipop.input_files.Clear();
            InputFile i3 = new InputFile("fake.txt", Purpose.Quantification);
            i3.biological_replicate = 1;
            i3.lt_condition = "A";
            i3.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i3);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i4 = new InputFile("fake.txt", Purpose.Quantification);
            i4.biological_replicate = 1;
            i4.lt_condition = "B";
            i4.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i4);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i5 = new InputFile("fake.txt", Purpose.Quantification);
            i5.biological_replicate = 1;
            i5.lt_condition = "C";
            i5.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i5);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i6 = new InputFile("fake.txt", Purpose.Quantification);
            i6.biological_replicate = 2;
            i6.lt_condition = "A";
            i6.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i6);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i7 = new InputFile("fake.txt", Purpose.Quantification);
            i7.biological_replicate = 2;
            i7.lt_condition = "B";
            i7.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i7);
            Lollipop.getObservationParameters();
            Assert.AreEqual(1, Lollipop.countOfBioRepsInOneCondition);

            InputFile i8 = new InputFile("fake.txt", Purpose.Quantification);
            i8.biological_replicate = 2;
            i8.lt_condition = "C";
            i8.purpose = Purpose.Quantification;
            Lollipop.input_files.Add(i8);
            Lollipop.getObservationParameters();
            Assert.AreEqual(2, Lollipop.countOfBioRepsInOneCondition);
        }

        [Test]
        public void permutations()
        {
            var resultOne = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6 }, 2);
            Assert.AreEqual(15, resultOne.Count());
            var resultTwo = ExtensionMethods.Combinations(new List<int> { 1}, 1);
            Assert.AreEqual(1, resultTwo.Count());
            var resultThree = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6 }, 6);
            Assert.AreEqual(1, resultThree.Count());
            var resultFour = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }, 4);
            Assert.AreEqual(70, resultFour.Count());
        }


        class DummyBiorepable : IBiorepable //empty class that inherits IBiorepable. useful for testing.
        {
            public InputFile input_file { get; set; }

            public double intensity_sum { get; set; }

            public DummyBiorepable(InputFile inFile, double intSum)
            {
                this.input_file = inFile;
                this.intensity_sum = intSum;
            }
        }

        [Test]
        public void testbiorepintensity()
        {
            Lollipop.neucode_labeled = true;
            List<string> lightConditions = new List<string> { "light" };
            List<string> heavyConditions = new List<string> { "heavy" };

            List<DummyBiorepable> db = new List<DummyBiorepable>();
            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(new InputFile("path", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 1),1d));
            }

            List<biorepIntensity> bril = ExperimentalProteoform.make_biorepIntensityList(db,db,lightConditions,heavyConditions);

            Assert.AreEqual(2, bril.Count());

            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(new InputFile("path", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 2), 2d));
            }

            bril = ExperimentalProteoform.make_biorepIntensityList(db, db, lightConditions, heavyConditions);

            Assert.AreEqual(4, bril.Count());

            Lollipop.neucode_labeled = false;

            bril = ExperimentalProteoform.make_biorepIntensityList(db, db, lightConditions, heavyConditions);

            Assert.AreEqual(2, bril.Count());

        }

        [Test]
        public void testComputeIndividualProteoformFDR()
        {
            decimal testStatistic = 0.2m;
            List<List<decimal>> permutedTestStatistics = new List<List<decimal>>();
            int satisfactoryProteoformsCount = 10;
            List<decimal> sortedProteoformTestStatistics = new List<decimal>();

            for (int i = 0; i < 10; i++)
            {
                List<decimal> onepst = new List<decimal>();
                for (int j = 0; j < 10; j++)
                {
                    onepst.Add((decimal)j);
                }
                sortedProteoformTestStatistics.Add((decimal)i/10);
                permutedTestStatistics.Add(onepst);
            }

            decimal fdr = ExperimentalProteoform.quantitativeValues.computeExperimentalProteoformFDR(testStatistic, permutedTestStatistics,satisfactoryProteoformsCount,sortedProteoformTestStatistics);
            Assert.AreEqual(1.125, fdr);

            testStatistic = 0.8m;
            fdr = ExperimentalProteoform.quantitativeValues.computeExperimentalProteoformFDR(testStatistic, permutedTestStatistics, satisfactoryProteoformsCount, sortedProteoformTestStatistics);
            Assert.AreEqual(4.5, fdr);
        }

        [Test]
        public void test_addBiorepIntensity()
        {
            List<biorepIntensity> briList = new List<biorepIntensity>();

            for (int i = 0; i < 10000; i++)
            {
                briList.Add(ExperimentalProteoform.quantitativeValues.add_biorep_intensity((decimal)Math.Log((double)100, 2), (decimal)Math.Log((double)5, 2), 1, "key", true));
            }

            List<double> allIntensity = briList.Select(b => b.intensity).ToList();
            double average = allIntensity.Average();
            double sum = allIntensity.Sum(d => Math.Pow(d - average, 2));
            double stdev = Math.Sqrt(sum / (allIntensity.Count() - 1));

            Assert.AreEqual(100d, Math.Round(average));
            Assert.AreEqual(5d, Math.Round(stdev));

        }

        [Test]
        public void test_imputedIntensities()
        {
            Dictionary<string, List<int>> observedConditions = new Dictionary<string, List<int>>();
            observedConditions.Add("light", new List<int> { 0, 1, 2 });
            List<biorepIntensity> briList = new List<biorepIntensity>();
            briList.AddRange(ExperimentalProteoform.quantitativeValues.imputedIntensities(true,briList,(decimal)Math.Log(100d,2), (decimal)Math.Log(5d, 2),observedConditions));
            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 3);//we started with no real observations but there were three observed bioreps in the experiment. Therefore we need 3 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);

            briList.Clear();
            briList.Add(new biorepIntensity(true, false, 0, "light", 1000d));
            briList.AddRange(ExperimentalProteoform.quantitativeValues.imputedIntensities(true, briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), observedConditions));

            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 2);//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(briList.Where(b => b.imputed == false).ToList().Count(), 1);//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);


            briList.Clear();
            briList.Add(new biorepIntensity(true, false, 0, "light", 1000d));
            briList.Add(new biorepIntensity(true, false, 1, "light", 2000d));
            briList.Add(new biorepIntensity(true, false, 2, "light", 3000d));
            briList.AddRange(ExperimentalProteoform.quantitativeValues.imputedIntensities(true, briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), observedConditions));

            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 0);//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(briList.Where(b => b.imputed == false).ToList().Count(), 3);//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);
        }

    }
}
