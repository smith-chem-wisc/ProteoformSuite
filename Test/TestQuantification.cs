using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

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
                ChargeState light_charge_state = new ChargeState(1, light.intensity_sum_olcs, light.weighted_monoisotopic_mass + 1.00727645D);
                ChargeState heavy_charge_state = new ChargeState(1, heavy.intensity_sum_olcs, heavy.weighted_monoisotopic_mass + 1.00727645D);
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
        public void proteoformQuantificationStatistics1()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;

            SaveState.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1, lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2, lysineCount));//these are for quantification
            List<Component> components = generate_neucode_components(proteoformMass, intensity, intensity/2d, lysineCount); // these are for indentification
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, quant_components_list, true);
            Assert.AreEqual(2, e1.lt_quant_components.Count);
            Assert.AreEqual(2, e1.hv_quant_components.Count);

            SaveState.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            SaveState.lollipop.getObservationParameters(true, SaveState.lollipop.input_files);

            e1.make_biorepIntensityList(e1.lt_quant_components, e1.hv_quant_components, SaveState.lollipop.ltConditionsBioReps.Keys, SaveState.lollipop.hvConditionsBioReps.Keys);
            Assert.AreEqual(4, e1.biorepIntensityList.Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.biorep == 1));
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.biorep == 2));
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.light == true));
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.light == false));

            SaveState.lollipop.getBiorepsFractionsList(SaveState.lollipop.input_files);

            SaveState.lollipop.computeProteoformTestStatistics(true, new List<ExperimentalProteoform> { e1 }, (decimal)10.000, (decimal)10.000, "", "", 1);

            Assert.AreEqual(2, e1.quant.lightBiorepIntensities.Count);
            Assert.AreEqual(2, e1.quant.heavyBiorepIntensities.Count);
            Assert.AreEqual(200d, e1.quant.lightBiorepIntensities.Sum(i => i.intensity));
            Assert.AreEqual(105d, e1.quant.heavyBiorepIntensities.Sum(i => i.intensity));
            Assert.AreEqual(305d, e1.quant.intensitySum);
            Assert.AreEqual(0.929610672108602M, e1.quant.logFoldChange);
            Assert.AreEqual(0.0379131331237966M, e1.quant.variance);
            Assert.True(0 <= e1.quant.pValue && e1.quant.pValue <= 1);
        }

        [Test]
        public void proteoformQuantificationStatistics2()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;

            SaveState.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1, lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2, lysineCount));//these are for quantification
            List<Component> components = generate_neucode_components(proteoformMass, intensity, intensity / 2d, lysineCount); // these are for indentification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 50d, 100d, 3, lysineCount));//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 48d, 102d, 4, lysineCount));//these are for quantification
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", components[0], components, quant_components_list, true);
            Assert.AreEqual(4, e2.lt_quant_components.Count);
            Assert.AreEqual(4, e2.hv_quant_components.Count);

            SaveState.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            SaveState.lollipop.getObservationParameters(true, SaveState.lollipop.input_files);
            e2.make_biorepIntensityList(e2.lt_quant_components, e2.hv_quant_components, SaveState.lollipop.ltConditionsBioReps.Keys, SaveState.lollipop.hvConditionsBioReps.Keys);
            Assert.AreEqual(4 * 2, e2.biorepIntensityList.Count);
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == 1));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == 2));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == 3));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == 4));
            Assert.AreEqual(2 * 2, e2.biorepIntensityList.Count(b => b.light));
            Assert.AreEqual(2 * 2, e2.biorepIntensityList.Count(b => !b.light));

            SaveState.lollipop.getBiorepsFractionsList(SaveState.lollipop.input_files);

            SaveState.lollipop.computeProteoformTestStatistics(true, new List<ExperimentalProteoform> { e2 }, (decimal)10.000, (decimal)10.000, "", "", 1);

            Assert.AreEqual(4, e2.quant.lightBiorepIntensities.Count);
            Assert.AreEqual(4, e2.quant.heavyBiorepIntensities.Count);
            Assert.AreEqual(298d, e2.quant.lightBiorepIntensities.Sum(i => i.intensity));
            Assert.AreEqual(307d, e2.quant.heavyBiorepIntensities.Sum(i => i.intensity));
            Assert.AreEqual(605d, e2.quant.intensitySum);
            Assert.AreEqual(-0.0429263249080178M, e2.quant.logFoldChange);
            Assert.AreEqual(1.97538639776822M, e2.quant.variance);
            Assert.True(0 <= e2.quant.pValue && e2.quant.pValue <= 1);
            Assert.AreEqual(0.410m, Math.Round(e2.quant.getProteinLevelStdDev(e2.quant.lightBiorepIntensities, e2.quant.heavyBiorepIntensities), 3));
            Assert.AreEqual(-0.03045m, Math.Round(e2.quant.testStatistic, 5));
            Assert.AreEqual(e2.quant.permutedTestStatistics.Count, SaveState.lollipop.permutedTestStatistics.Count());
        }

        [Test]
        public void proteinLevelStDev_divide_by_zero_crash()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            List<BiorepIntensity> singleton_list = new List<BiorepIntensity> { new BiorepIntensity(false, false, 1, "", 0) };
            Assert.True(e.quant.getProteinLevelStdDev(singleton_list, singleton_list) > 0);
        }

        [Test]
        public void quant_variance_without_imputation_aka_unequal_list_lengths()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            List<BiorepIntensity> singleton_list = new List<BiorepIntensity> { new BiorepIntensity(false, false, 1, "", 0) };
            List<BiorepIntensity> shorter_list = new List<BiorepIntensity>();
            try
            {
                e.quant.Variance(0, shorter_list, singleton_list);
            }
            catch (ArgumentException ex)
            {
                Assert.NotNull(ex.Message);
            }
        }

        [Test]
        public void quant_permuations_without_imputation_aka_unequal_list_lengths()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            List<BiorepIntensity> singleton_list = new List<BiorepIntensity> { new BiorepIntensity(false, false, 1, "", 0) };
            List<BiorepIntensity> shorter_list = new List<BiorepIntensity>();
            try
            {
                e.quant.getPermutedTestStatistics(shorter_list, singleton_list, 0, 1);
            }
            catch (ArgumentException ex)
            {
                Assert.NotNull(ex.Message);
            }
        }

        [Test]
        public void quant_pvalue_without_imputation_aka_unequal_list_lengths()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            List<BiorepIntensity> singleton_list = new List<BiorepIntensity> { new BiorepIntensity(false, false, 1, "", 0) };
            List<BiorepIntensity> shorter_list = new List<BiorepIntensity>();
            try
            {
                e.quant.PValue(0, shorter_list, singleton_list);
            }
            catch (ArgumentException ex)
            {
                Assert.NotNull(ex.Message);
            }
        }

        [Test]
        public void make_multiple_biorepintensity_litss()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;

            SaveState.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1, lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2, lysineCount));//these are for quantification
            List<Component> components = generate_neucode_components(proteoformMass, intensity, intensity / 2d, lysineCount); // these are for indentification
            SaveState.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            SaveState.lollipop.getObservationParameters(true, SaveState.lollipop.input_files);
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, quant_components_list, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", components[0], components, quant_components_list.Concat(generate_neucode_quantitative_components(proteoformMass, 50d, 100d, 3, lysineCount)).ToList(), true);
            SaveState.lollipop.computeBiorepIntensities(new List<ExperimentalProteoform> { e1, e2 }, SaveState.lollipop.ltConditionsBioReps.Keys, SaveState.lollipop.hvConditionsBioReps.Keys);
            Assert.True(e1.biorepIntensityList.Count > 0);
            Assert.True(e2.biorepIntensityList.Count > 0);
        }

        [Test]
        public void quantificationInitialization()
        {
            //neucode labelled
            SaveState.lollipop.input_files.Clear();
            SaveState.lollipop.neucode_labeled = true;
            InputFile i1 = new InputFile("fake.txt", Purpose.Quantification);
            i1.biological_replicate = 1;
            i1.lt_condition = "light";
            i1.hv_condition = "heavy";
            SaveState.lollipop.input_files.Add(i1);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i2 = new InputFile("fake.txt", Purpose.Quantification);
            i2.biological_replicate = 2;
            i2.lt_condition = "light";
            i2.hv_condition = "heavy";
            SaveState.lollipop.input_files.Add(i2);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(2, SaveState.lollipop.countOfBioRepsInOneCondition);

            //unlabelled
            SaveState.lollipop.neucode_labeled = false;
            SaveState.lollipop.input_files.Clear();
            InputFile i3 = new InputFile("fake.txt", Purpose.Quantification);
            i3.biological_replicate = 1;
            i3.lt_condition = "A";
            SaveState.lollipop.input_files.Add(i3);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i4 = new InputFile("fake.txt", Purpose.Quantification);
            i4.biological_replicate = 1;
            i4.lt_condition = "B";
            SaveState.lollipop.input_files.Add(i4);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i5 = new InputFile("fake.txt", Purpose.Quantification);
            i5.biological_replicate = 1;
            i5.lt_condition = "C";
            SaveState.lollipop.input_files.Add(i5);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i6 = new InputFile("fake.txt", Purpose.Quantification);
            i6.biological_replicate = 2;
            i6.lt_condition = "A";
            SaveState.lollipop.input_files.Add(i6);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i7 = new InputFile("fake.txt", Purpose.Quantification);
            i7.biological_replicate = 2;
            i7.lt_condition = "B";
            SaveState.lollipop.input_files.Add(i7);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(1, SaveState.lollipop.countOfBioRepsInOneCondition);

            InputFile i8 = new InputFile("fake.txt", Purpose.Quantification);
            i8.biological_replicate = 2;
            i8.lt_condition = "C";
            SaveState.lollipop.input_files.Add(i8);
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files);
            Assert.AreEqual(2, SaveState.lollipop.countOfBioRepsInOneCondition);
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
            SaveState.lollipop.neucode_labeled = true;
            List<string> lightConditions = new List<string> { "light" };
            List<string> heavyConditions = new List<string> { "heavy" };

            List<DummyBiorepable> db = new List<DummyBiorepable>();
            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 1, -1, -1), 1d));
            }

            List<BiorepIntensity> bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db,db,lightConditions,heavyConditions);

            Assert.AreEqual(2, bril.Count());

            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 2, -1, -1), 2d));
            }

            bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, db, lightConditions, heavyConditions);

            Assert.AreEqual(4, bril.Count());

            SaveState.lollipop.neucode_labeled = false;

            bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, db, lightConditions, heavyConditions);

            Assert.AreEqual(2, bril.Count());

        }

        [Test]
        public void testComputeIndividualProteoformFDR()
        {
            List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>();

            for (int i = 0; i < 10; i++)
            {
                ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
                List<decimal> onepst = new List<decimal>();
                for (int j = 0; j < 10; j++)
                {
                    onepst.Add((decimal)j);
                }
                e.quant.testStatistic = ((decimal)i/10);
                e.quant.permutedTestStatistics = onepst;
                satisfactoryProteoforms.Add(e);
            }

            SaveState.lollipop.computeSortedTestStatistics(satisfactoryProteoforms);
            SaveState.lollipop.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, SaveState.lollipop.sortedProteoformTestStatistics, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity);

            //testStatistic = 0.2m;
            Assert.AreEqual(1.125, satisfactoryProteoforms[2].quant.FDR);

            //testStatistic = 0.8m;
            Assert.AreEqual(4.5, satisfactoryProteoforms[8].quant.FDR);
        }

        [Test]
        public void test_compute_sorted_statistics_and_tusher_fdr_calculation()
        {
            List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>();

            for (int i = 0; i < 10; i++)
            {
                ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
                List<decimal> onepst = new List<decimal>();
                for (int j = 0; j < 10; j++)
                {
                    if (j == 9) onepst.Add(9);
                    else onepst.Add((decimal)7); //making it asymmetrical
                }
                e.quant.testStatistic = ((decimal)i);
                e.quant.permutedTestStatistics = onepst;
                satisfactoryProteoforms.Add(e);
            }

            SaveState.lollipop.computeSortedTestStatistics(satisfactoryProteoforms);
            Assert.AreEqual(SaveState.lollipop.sortedAvgPermutationTestStatistics.Count, SaveState.lollipop.sortedProteoformTestStatistics.Count);

            var sorted_check1 = SaveState.lollipop.sortedProteoformTestStatistics.OrderBy(x => x);
            var sorted_check2 = SaveState.lollipop.sortedAvgPermutationTestStatistics.OrderBy(x => x);
            Assert.IsTrue(sorted_check1.SequenceEqual(SaveState.lollipop.sortedProteoformTestStatistics));
            Assert.IsTrue(sorted_check2.SequenceEqual(SaveState.lollipop.sortedAvgPermutationTestStatistics));

            //Average permuted of the set {0,0,0,0,0,0,0,0,0,9} is 7.18 for each
            //First passing above 8.18 is 9
            //First below 6.18 is 6
            //One permuted value passes each time, the nine
            //Eight values in the set {0,1,2,3,4,5,6,7,8,9} pass the two cutoffs, 6 and 9
            Assert.AreEqual((double)1 / (double)8, SaveState.lollipop.computeFoldChangeFDR(SaveState.lollipop.sortedAvgPermutationTestStatistics, SaveState.lollipop.sortedProteoformTestStatistics, satisfactoryProteoforms, satisfactoryProteoforms.SelectMany(e => e.quant.permutedTestStatistics), 1));

            SaveState.lollipop.satisfactoryProteoforms = satisfactoryProteoforms;
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void test_addBiorepIntensity()
        {
            List<BiorepIntensity> briList = new List<BiorepIntensity>();

            for (int i = 0; i < 10000; i++)
            {
                briList.Add(QuantitativeProteoformValues.add_biorep_intensity((decimal)Math.Log((double)100, 2), (decimal)Math.Log((double)5, 2), 1, "key", true));
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
            List<BiorepIntensity> briList = new List<BiorepIntensity>();
            briList.AddRange(QuantitativeProteoformValues.imputedIntensities(true,briList,(decimal)Math.Log(100d,2), (decimal)Math.Log(5d, 2),observedConditions));
            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 3);//we started with no real observations but there were three observed bioreps in the experiment. Therefore we need 3 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);

            briList.Clear();
            briList.Add(new BiorepIntensity(true, false, 0, "light", 1000d));
            briList.AddRange(QuantitativeProteoformValues.imputedIntensities(true, briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), observedConditions));

            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 2);//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(briList.Where(b => b.imputed == false).ToList().Count(), 1);//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);


            briList.Clear();
            briList.Add(new BiorepIntensity(true, false, 0, "light", 1000d));
            briList.Add(new BiorepIntensity(true, false, 1, "light", 2000d));
            briList.Add(new BiorepIntensity(true, false, 2, "light", 3000d));
            briList.AddRange(QuantitativeProteoformValues.imputedIntensities(true, briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), observedConditions));

            Assert.AreEqual(briList.Where(b => b.imputed == true).ToList().Count(), 0);//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(briList.Where(b => b.imputed == false).ToList().Count(), 3);//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(briList[0].condition, "light");
            Assert.AreEqual(briList[0].light, true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(0), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(1), true);
            Assert.AreEqual(briList.Select(b => b.biorep).ToList().Contains(2), true);
        } 

        [Test]
        public void test_gaussian_area_calculation()
        {
            SortedDictionary<decimal, int> histogram = new SortedDictionary<decimal, int>();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            
            //Each biorepIntensity has a unique combination of light/heavy + condition + biorep, since that's how they're made in the program
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(true, false, i, "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(true, false, i, "second", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, false, i, "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, false, i, "second", 0));

            double log2_intensity = 0.06; //rounds up
            foreach(BiorepIntensity b in e.biorepIntensityList)
            {
                b.intensity = Math.Pow(2, log2_intensity);
                log2_intensity += 0.05;
            }

            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { e };
            List<decimal> rounded_intensities = SaveState.lollipop.define_intensity_distribution(exps, histogram);

            //12 intensity values, bundled in twos; therefore 6 rounded values
            Assert.AreEqual(12, rounded_intensities.Count);
            Assert.AreEqual(6, histogram.Keys.Count);

            //5 bins of width 0.1 and height 2; the first one gets swallowed up in smoothing
            Assert.AreEqual(1, SaveState.lollipop.get_gaussian_area(histogram));
            histogram.Add(Math.Round((decimal)log2_intensity, 1), 1);

            //Added 1 bin of width 0.1 and height 1.5, since we're smoothing by taking width and the mean of the two adjacent bars
            Assert.AreEqual(1.15, SaveState.lollipop.get_gaussian_area(histogram));            
        }

        [Test]
        public void all_intensity_distribution_avgIntensity_and_stdv_calculations()
        {
            SortedDictionary<decimal, int> histogram = new SortedDictionary<decimal, int>();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");

            //Each biorepIntensity has a unique combination of light/heavy + condition + biorep, since that's how they're made in the program
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(true, false, i, "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(true, false, i, "second", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, false, i, "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, false, i, "second", 0));

            double log2_intensity = 1.06; //rounds up
            foreach (BiorepIntensity b in e.biorepIntensityList)
            {
                b.intensity = Math.Pow(2, log2_intensity);
                log2_intensity += 0.05;
            }

            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { e };
            List<decimal> rounded_intensities = SaveState.lollipop.define_intensity_distribution(exps, histogram);
            SaveState.lollipop.get_gaussian_area(histogram);

            //ALL INTENSITIES
            //Test the standard deviation and other calculations
            SaveState.lollipop.defineAllObservedIntensityDistribution(exps, histogram); // creates the histogram again, checking that it's cleared, too
            Assert.AreEqual(1m, SaveState.lollipop.observedGaussianArea);
            Assert.AreEqual(1.35m, SaveState.lollipop.observedAverageIntensity);
            Assert.AreEqual(0.171m, Math.Round(SaveState.lollipop.observedStDev, 3));
            Assert.AreEqual(2.34m, Math.Round(SaveState.lollipop.observedGaussianHeight, 2));

            //The rest of the calculations should be based off of selected, so setting those to zero
            SaveState.lollipop.observedGaussianArea = 0;
            SaveState.lollipop.observedAverageIntensity = 0;
            SaveState.lollipop.observedStDev = 0;
            SaveState.lollipop.observedGaussianHeight = 0;

            //SELECTED INTENSITIES
            SaveState.lollipop.defineSelectObservedIntensityDistribution(exps, histogram);
            Assert.AreEqual(1m, SaveState.lollipop.selectGaussianArea);
            Assert.AreEqual(1.35m, SaveState.lollipop.selectAverageIntensity);
            Assert.AreEqual(0.171m, Math.Round(SaveState.lollipop.selectStDev, 3));
            Assert.AreEqual(2.34m, Math.Round(SaveState.lollipop.selectGaussianHeight, 2)); //shouldn't this be calculated with the selectStDev? changed from //selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));

            //SELECTED BACKGROUND
            SaveState.lollipop.condition_count = e.biorepIntensityList.Select(b => b.condition + b.light.ToString()).Distinct().Count();
            Dictionary<int, List<int>> qBioFractions = e.biorepIntensityList.Select(b => b.biorep).Distinct().ToDictionary(b => b, b => new List<int>());
            SaveState.lollipop.defineBackgroundIntensityDistribution(false, qBioFractions, exps, -2, 0.5m);
            Assert.AreEqual(1.01m, Math.Round(SaveState.lollipop.bkgdAverageIntensity, 2));
            Assert.AreEqual(0.085m, Math.Round(SaveState.lollipop.bkgdStDev, 3));
            Assert.AreEqual(0, Math.Round(SaveState.lollipop.bkgdGaussianHeight, 2));

            //unlabeled works similarly
            SaveState.lollipop.defineBackgroundIntensityDistribution(true, qBioFractions, exps, -2, 0.5m);
            Assert.AreEqual(1.01m, Math.Round(SaveState.lollipop.bkgdAverageIntensity, 2));
            Assert.AreEqual(0.085m, Math.Round(SaveState.lollipop.bkgdStDev, 3));
            Assert.AreEqual(0, Math.Round(SaveState.lollipop.bkgdGaussianHeight, 2));
        }

        [Test]
        public void proteoforms_meeting_criteria()
        {
            string anysingle = "From Any Single Condition";
            string any = "From Any Condition";
            string fromeach = "From Each Condition";

            List<string> conditions = new List<string> { "s", "ns" };
            BiorepIntensity b1 = new BiorepIntensity(false, false, 1, conditions[0], 0);
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ConstructorsForTesting.ExperimentalProteoform("E") };
            exps[0].biorepIntensityList.Add(b1);
            List<ExperimentalProteoform> exps_out = new List<ExperimentalProteoform>();

            //PASSES WHEN THERE ARE ENOUGH IN SPECIFIED CONDITIONS
            //One biorep obs passes any-single-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs passes any-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs doesn't pass for-each-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(0, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD
            //One biorep obs doesn't pass 2 from any-single-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from any-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from for-each-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            BiorepIntensity b2 = new BiorepIntensity(false, false, 100, conditions[1], 0);
            exps[0].biorepIntensityList.Add(b2);

            //One biorep in each condition passes for-each-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(1, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD IN SPECIFIC CONDITIONS UNLESS ANY-CONDITION
            //One biorep obs in two different conditions doesn't pass 2 from any-single-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs in two different conditions passes 2 from any-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs in two different conditions doesn't pass 2 from for-each-conditon test
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //DOESN'T PASS WHEN NOT MATCHING LISTED CONDITIONS, EXCEPT FOR ANY-CONDITION
            foreach (BiorepIntensity b in exps[0].biorepIntensityList) b.condition = "not_a_condition";
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //NOT JUST COUNTING BIOREP INTENSITIES, BUT RATHER BIOREPS WITH OBSERVATIONS
            BiorepIntensity b3 = new BiorepIntensity(false, false, 1, conditions[0], 0);
            BiorepIntensity b4 = new BiorepIntensity(false, false, 1, conditions[0], 0);
            BiorepIntensity b5 = new BiorepIntensity(false, false, 1, conditions[0], 0);
            BiorepIntensity b6 = new BiorepIntensity(false, false, 1, conditions[0], 0);
            exps[0].biorepIntensityList = new List<BiorepIntensity> { b3, b4, b5, b6 };
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            BiorepIntensity b7 = new BiorepIntensity(false, false, 1, conditions[1], 0);
            BiorepIntensity b8 = new BiorepIntensity(false, false, 1, conditions[1], 0);
            BiorepIntensity b9 = new BiorepIntensity(false, false, 1, conditions[1], 0);
            BiorepIntensity b10 = new BiorepIntensity(false, false, 1, conditions[1], 0);
            exps[0].biorepIntensityList.Add(b7);
            exps[0].biorepIntensityList.Add(b8);
            exps[0].biorepIntensityList.Add(b9);
            exps[0].biorepIntensityList.Add(b10);
            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = SaveState.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);
        }

        [Test]
        public void benjaminiYekutieli()
        {
            int nbp = 100;
            List<double> pvals = new List<double>();
            double pValue = 0.001d;

            for (double i = 1; i <= nbp; i++)
            {
                pvals.Add(0.1d / i);
            }
            pvals.Sort();
            Assert.AreEqual(Math.Round(GoTermNumber.benjaminiYekutieli(nbp, pvals, pValue), 4), 0.5187);
            nbp++;
        }

        [Test]
        public void test_calculateGoTermFDR() // this is not finished yet
        {
            int numberOfGoTermNumbers = 100;
            List<GoTermNumber> gtns = new List<GoTermNumber>();
            for (double i = 1; i <= numberOfGoTermNumbers; i++)
            {
                DatabaseReference d = new DatabaseReference("GO", ":id", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:description") });
                GoTerm g = new GoTerm(d);
                GoTermNumber gtn = new GoTermNumber(g, 0, 0, 0, 0);
                gtn.p_value = 0.1d / i - 0.0005d;
                gtns.Add(gtn);
            }
            Lollipop.calculateGoTermFDR(gtns);
            foreach (GoTermNumber num in gtns)
            {
                Assert.IsNotNull(num.by);
                Assert.True(num.by <= 1);
            }
        }

        [Test]
        public void test_results_summary_with_gtns()
        {
            int numberOfGoTermNumbers = 100;
            List<GoTermNumber> gtns = new List<GoTermNumber>();
            for (double i = 1; i <= numberOfGoTermNumbers; i++)
            {
                DatabaseReference d = new DatabaseReference("GO", ":id", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:description") });
                GoTerm g = new GoTerm(d);
                GoTermNumber gtn = new GoTermNumber(g, 0, 0, 0, 0);
                gtn.p_value = 0.1d / i - 0.0005d;
                gtns.Add(gtn);
            }
            Lollipop.calculateGoTermFDR(gtns);
            foreach (GoTermNumber num in gtns)
            {
                Assert.IsNotNull(num.by);
                Assert.True(num.by <= 1);
            }
            SaveState.lollipop.goTermNumbers = gtns;
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void test_computeExperimentalProteoformFDR()
        {
            decimal testStatistic = 0.001m;
            List<List<decimal>> permutedTestStatistics = new List<List<decimal>>();
            int satisfactoryProteoformsCount = 100;
            List<decimal> sortedProteoformTestStatistics = new List<decimal>();

            for (int i = 1; i <= satisfactoryProteoformsCount; i++)
            {
                sortedProteoformTestStatistics.Add(0.01m / (decimal)i);
                List<decimal> pts = new List<decimal>();

                for (int j = -2; j <= 2; j++)
                {
                    if (j != 0)
                        pts.Add(0.1m / (decimal)j);
                }
                permutedTestStatistics.Add(pts);
            }
            Assert.AreEqual(0.4m, QuantitativeProteoformValues.computeExperimentalProteoformFDR(testStatistic, permutedTestStatistics, satisfactoryProteoformsCount, sortedProteoformTestStatistics));
            satisfactoryProteoformsCount++;
        }

        [Test]
        public void test_get_observed_proteins()
        {
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p1 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p2 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p3 } },
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            TheoreticalProteoform u = ConstructorsForTesting.make_a_theoretical("T2_T1_asdf_asdf", p2, dict);
            TheoreticalProteoform v = ConstructorsForTesting.make_a_theoretical("T3_T1_asdf_Asdf_Asdf", p3, dict);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            ProteoformRelation et = new ProteoformRelation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            DeltaMassPeak etp = new DeltaMassPeak(et, new List<ProteoformRelation> { et });
            et.Accepted = true;
            et.peak = etp;
            etp.Accepted = true;
            e.relationships.Add(et);
            t.relationships.Add(et);
            ProteoformRelation eu = new ProteoformRelation(e, u, ProteoformComparison.ExperimentalTheoretical, 0);
            DeltaMassPeak eup = new DeltaMassPeak(eu, new List<ProteoformRelation> { eu });
            eu.Accepted = true;
            eu.peak = eup;
            eu.Accepted = true;
            eup.Accepted = true;
            e.relationships.Add(eu);
            u.relationships.Add(eu);
            ProteoformFamily f = new ProteoformFamily(e);
            f.construct_family();
            e.family = f;
            t.family = f;
            u.family = f;
            List<ProteinWithGoTerms> prots = SaveState.lollipop.getProteins(new List<ExperimentalProteoform> { e });
            Assert.AreEqual(2, prots.Count);
            Assert.True(prots.Select(p => p.Accession).Contains("T1"));
            Assert.True(prots.Select(p => p.Accession).Contains("T2"));
            Assert.False(prots.Select(p => p.Accession).Contains("T3"));
        }

        [Test]
        public void test_get_repressed_or_induced_proteins()
        {
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p1 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p2 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p3 } },
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            TheoreticalProteoform u = ConstructorsForTesting.make_a_theoretical("T2_T1_asdf_asdf", p2, dict);
            TheoreticalProteoform v = ConstructorsForTesting.make_a_theoretical("T3_T1_asdf_Asdf_Asdf", p3, dict);
            ExperimentalProteoform ex = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform fx = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform gx = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform hx = ConstructorsForTesting.ExperimentalProteoform("E1");
            ConstructorsForTesting.make_relation(ex, t);
            ConstructorsForTesting.make_relation(ex, u);
            //ConstructorsForTesting.make_relation(ex, v);
            ConstructorsForTesting.make_relation(ex, fx);
            ConstructorsForTesting.make_relation(ex, gx);
            ConstructorsForTesting.make_relation(ex, hx);
            ProteoformFamily f = new ProteoformFamily(ex);
            f.construct_family();
            ex.family = f;
            fx.family = f;
            gx.family = f;
            hx.family = f;
            t.family = f;
            u.family = f;

            //Nothing passing, but one thing passing for each
            ex.quant.logFoldChange = 12;
            ex.quant.FDR = 1;
            ex.quant.intensitySum = 0;
            fx.quant.logFoldChange = -12;
            fx.quant.FDR = 1;
            fx.quant.intensitySum = 0;
            gx.quant.logFoldChange = 8;
            gx.quant.FDR = 0.4m;
            gx.quant.intensitySum = 0;
            hx.quant.logFoldChange = 8;
            hx.quant.FDR = 1;
            hx.quant.intensitySum = 2;
            List<ProteinWithGoTerms> prots = SaveState.lollipop.getInducedOrRepressedProteins(new List<ExperimentalProteoform> { ex,fx,gx }, 10, 0.5m, 1);
            Assert.AreEqual(0, prots.Count);

            //Nothing passing, but two things passing for each
            ex.quant.logFoldChange = 12;
            ex.quant.FDR = 0.4m;
            ex.quant.intensitySum = 0;
            fx.quant.logFoldChange = -12;
            fx.quant.FDR = 0.4m;
            fx.quant.intensitySum = 0;
            gx.quant.logFoldChange = 8;
            gx.quant.FDR = 0.4m;
            gx.quant.intensitySum = 2;
            hx.quant.logFoldChange = 12;
            hx.quant.FDR = 1;
            hx.quant.intensitySum = 2;
            prots = SaveState.lollipop.getInducedOrRepressedProteins(new List<ExperimentalProteoform> { ex, fx, gx }, 10, 0.5m, 1);
            Assert.AreEqual(0, prots.Count);

            //Passing
            ex.quant.logFoldChange = 12;
            ex.quant.FDR = 0.4m;
            ex.quant.intensitySum = 2;
            prots = SaveState.lollipop.getInducedOrRepressedProteins(new List<ExperimentalProteoform> { ex, fx, gx }, 10, 0.5m, 1);
            Assert.AreEqual(2, prots.Count);
            Assert.True(prots.Select(p => p.Accession).Contains("T1"));
            Assert.True(prots.Select(p => p.Accession).Contains("T2"));
            Assert.False(prots.Select(p => p.Accession).Contains("T3"));
        }

        [Test]
        public void get_interesting_pfs()
        {
            ExperimentalProteoform ex = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform fx = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform gx = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform hx = ConstructorsForTesting.ExperimentalProteoform("E4");
            ex.quant.logFoldChange = 12;
            ex.quant.FDR = 0.4m;
            ex.quant.intensitySum = 2;
            fx.quant.logFoldChange = 12;
            fx.quant.FDR = 0.4m;
            fx.quant.intensitySum = 2;
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ex, fx, gx, hx };
            List<ExperimentalProteoform> interesting = SaveState.lollipop.getInterestingProteoforms(exps, 10, 0.5m, 1).ToList();
            Assert.AreEqual(2, interesting.Count);
        }

        [Test]
        public void get_interesting_families()
        {
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p1 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p2 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p3 } },
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            TheoreticalProteoform u = ConstructorsForTesting.make_a_theoretical("T2_T1_asdf_asdf", p2, dict);
            TheoreticalProteoform v = ConstructorsForTesting.make_a_theoretical("T3_T1_asdf_Asdf_Asdf", p3, dict);
            ExperimentalProteoform ex = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform fx = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform gx = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform hx = ConstructorsForTesting.ExperimentalProteoform("E4");
            ex.quant.logFoldChange = 12;
            ex.quant.FDR = 0.4m;
            ex.quant.intensitySum = 2;
            fx.quant.logFoldChange = 12;
            fx.quant.FDR = 0.4m;
            fx.quant.intensitySum = 2;
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ex, fx, gx, hx };
            ConstructorsForTesting.make_relation(gx, v);
            ConstructorsForTesting.make_relation(fx, v);
            ConstructorsForTesting.make_relation(hx, t);
            ConstructorsForTesting.make_relation(hx, u);        
            ProteoformFamily e = new ProteoformFamily(ex);
            ProteoformFamily f = new ProteoformFamily(v);
            ProteoformFamily h = new ProteoformFamily(hx);
            e.construct_family();
            f.construct_family();
            h.construct_family();
            List<ProteoformFamily> families = new List<ProteoformFamily> { e, f, h };
            ex.family = e;
            fx.family = f;
            gx.family = f;
            hx.family = h;
            t.family = h;
            u.family = h;
            v.family = f;

            List<ProteoformFamily> fams = SaveState.lollipop.getInterestingFamilies(exps, 10, 0.5m, 1);
            Assert.AreEqual(2, fams.Count);
            Assert.AreEqual(1, fams.Where(x => x.theoretical_proteoforms.Count == 0).Count());
            Assert.AreEqual(1, fams.Where(x => x.theoretical_proteoforms.Count == 1).Count());
            Assert.AreEqual(0, fams.Where(x => x.theoretical_proteoforms.Count == 2).Count());
            Assert.AreEqual(1, fams.Where(x => x.experimental_proteoforms.Count == 2).Count());
        }
    }
}
