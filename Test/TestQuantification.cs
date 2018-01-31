using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static List<IAggregatable> generate_neucode_components(double mass, double lightIntensity, double heavyIntensity, int lysineCount)
        {
            List<IAggregatable> components = new List<IAggregatable>();
            InputFile inFile = new InputFile("somepath", Labeling.NeuCode, Purpose.Identification);

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
                light.intensity_sum = lightIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
                heavy.intensity_sum = heavyIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
                light.rt_apex = starter_rt;
                heavy.rt_apex = starter_rt;
                light.accepted = true;
                heavy.accepted = true;
                ChargeState light_charge_state = new ChargeState(1, light.intensity_sum, light.weighted_monoisotopic_mass);
                ChargeState heavy_charge_state = new ChargeState(1, heavy.intensity_sum, heavy.weighted_monoisotopic_mass);
                light.charge_states = new List<ChargeState> { light_charge_state };
                heavy.charge_states = new List<ChargeState> { heavy_charge_state };
                double mass_difference = heavy.weighted_monoisotopic_mass - light.weighted_monoisotopic_mass;
                NeuCodePair n = new NeuCodePair(light, light.intensity_sum, heavy, heavy.intensity_sum, mass_difference, new HashSet<int> { 1 }, true);
                n.rt_apex = light.rt_apex;
                n.weighted_monoisotopic_mass = light.weighted_monoisotopic_mass;
                n.accepted = true;
                n.lysine_count = lysineCount;
                components.Add(n);
            }
            return components;
        }

        public static List<Component> generate_neucode_quantitative_components(double mass, double lightIntensity, double heavyIntensity, string biorep, int lysineCount)
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
            light.intensity_sum = lightIntensity; //using the special intensity sum for overlapping charge states in a neucode pair
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
        public void getobsparameters_doesnt_crash_when_no_quant_files()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.getConditionBiorepFractionLabels(true, new List<InputFile>());
        }

        [Test]
        public void proteoformQuantificationStatistics1()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-TestExperimentalProteoform.missed_monoisotopics, TestExperimentalProteoform.missed_monoisotopics * 2 + 1).ToList();

            Sweet.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1.ToString(), lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2.ToString(), lysineCount));//these are for quantification
            List<IAggregatable> components = generate_neucode_components(proteoformMass, intensity, intensity / 2d, lysineCount); // these are for indentification
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, quant_components_list, true);
            Assert.AreEqual(2, e1.lt_quant_components.Count);
            Assert.AreEqual(2, e1.hv_quant_components.Count);

            Sweet.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            Sweet.lollipop.getConditionBiorepFractionLabels(true, Sweet.lollipop.input_files);

            e1.make_biorepIntensityList(e1.lt_quant_components, e1.hv_quant_components, Sweet.lollipop.ltConditionsBioReps.Keys, Sweet.lollipop.hvConditionsBioReps.Keys);
            Assert.AreEqual(4, e1.biorepIntensityList.Count);
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.biorep == "1"));
            Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.biorep == "2"));
            //Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.light == true));
            //Assert.AreEqual(2, e1.biorepIntensityList.Count(b => b.light == false));

            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            string numerator_condition = Sweet.lollipop.ltConditionsBioReps.Keys.First();
            string denominator_condition = Sweet.lollipop.hvConditionsBioReps.Keys.First();
            string induced_condition = Sweet.lollipop.hvConditionsBioReps.Keys.First();
            Dictionary<string, List<string>> cbr = e1.biorepIntensityList.Select(x => x.condition).Distinct().ToDictionary(c => c, c => e1.biorepIntensityList.Where(br => br.condition == c).Select(br => br.biorep).Distinct().ToList());

            Sweet.lollipop.TusherAnalysis1.compute_proteoform_statistics(new List<ExperimentalProteoform> { e1 }, (decimal)10.000, (decimal)10.000, cbr, numerator_condition, denominator_condition, induced_condition, 1, false);

            Assert.AreEqual(2, e1.quant.TusherValues1.numeratorOriginalIntensities.Count);
            Assert.AreEqual(2, e1.quant.TusherValues1.denominatorOriginalIntensities.Count);
            Assert.AreEqual(0d /*200d*/, e1.quant.TusherValues1.numeratorOriginalIntensities.Sum(i => i.intensity_sum));
            Assert.AreEqual(0d /*105d*/, e1.quant.TusherValues1.denominatorOriginalIntensities.Sum(i => i.intensity_sum));
            Assert.AreEqual(305d, e1.quant.intensitySum);
            Assert.AreEqual(0.929610672108602M, e1.quant.tusherlogFoldChange);
        }

        [Test]
        public void proteoformQuantificationStatistics2()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-TestExperimentalProteoform.missed_monoisotopics, TestExperimentalProteoform.missed_monoisotopics * 2 + 1).ToList();

            Sweet.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1.ToString(), lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2.ToString(), lysineCount));//these are for quantification
            List<IAggregatable> components = generate_neucode_components(proteoformMass, intensity, intensity / 2d, lysineCount); // these are for indentification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 50d, 100d, 3.ToString(), lysineCount));//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 48d, 102d, 4.ToString(), lysineCount));//these are for quantification
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", components[0], components, quant_components_list, true);
            Assert.AreEqual(4, e2.lt_quant_components.Count);
            Assert.AreEqual(4, e2.hv_quant_components.Count);

            Sweet.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            Sweet.lollipop.getConditionBiorepFractionLabels(true, Sweet.lollipop.input_files);
            e2.make_biorepIntensityList(e2.lt_quant_components, e2.hv_quant_components, Sweet.lollipop.ltConditionsBioReps.Keys, Sweet.lollipop.hvConditionsBioReps.Keys);
            Assert.AreEqual(4 * 2, e2.biorepIntensityList.Count);
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == "1"));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == "2"));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == "3"));
            Assert.AreEqual(2, e2.biorepIntensityList.Count(b => b.biorep == "4"));
            //Assert.AreEqual(2 * 2, e2.biorepIntensityList.Count(b => b.light));
            //Assert.AreEqual(2 * 2, e2.biorepIntensityList.Count(b => !b.light));

            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            string numerator_condition = "light";
            string denominator_condition = "heavy";
            string induced_condition = "heavy";
            Dictionary<string, List<string>> cbr = e2.biorepIntensityList.Select(x => x.condition).Distinct().ToDictionary(c => c, c => e2.biorepIntensityList.Where(br => br.condition == c).Select(br => br.biorep).Distinct().ToList());

            Sweet.lollipop.TusherAnalysis1.compute_proteoform_statistics(new List<ExperimentalProteoform> { e2 }, (decimal)10.000, (decimal)10.000, cbr, numerator_condition, denominator_condition, induced_condition, 1, false);

            Assert.AreEqual(4, e2.quant.TusherValues1.numeratorOriginalIntensities.Count);
            Assert.AreEqual(4, e2.quant.TusherValues1.denominatorOriginalIntensities.Count);
            Assert.AreEqual(0d /*298d*/, e2.quant.TusherValues1.numeratorOriginalIntensities.Sum(i => i.intensity_sum));
            Assert.AreEqual(0d /*307d*/, e2.quant.TusherValues1.denominatorOriginalIntensities.Sum(i => i.intensity_sum));
            Assert.AreEqual(605d, e2.quant.intensitySum);
            Assert.AreEqual(-0.0429263249080178M, e2.quant.tusherlogFoldChange);
            //Assert.AreEqual(20.338m, Math.Round(e2.quant.StdDev(e2.quant.numeratorOriginalBiorepIntensities, e2.quant.denominatorOriginalBiorepIntensities), 3));
            //Assert.AreEqual(0.10544m, Math.Round(e2.quant.relative_difference, 5));
        }

        [Test]
        public void proteinLevelStDev_divide_by_zero_crash()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            List<BiorepIntensity> singleton_list = new List<BiorepIntensity> { new BiorepIntensity(false, 1.ToString(), "", 0) };
            Assert.True(e.quant.TusherValues1.StdDev(singleton_list, singleton_list) > 0);
        }

        [Test]
        public void quant_permuations_without_imputation_aka_unequal_list_lengths()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            Dictionary<string, List<string>> unbalanced_design = new Dictionary<string, List<string>> { { "n", new List<string> { 1.ToString(), 2.ToString() } }, { "s", new List<string> { 1.ToString() } } };
            try
            {
                Sweet.lollipop.TusherAnalysis1.compute_balanced_biorep_permutation_relativeDifferences(unbalanced_design, "s", new List<ExperimentalProteoform>(), 0);
            }
            catch (ArgumentException ex)
            {
                Assert.NotNull(ex.Message);
            }
        }

        [Test]
        public void quant_balanced_permutation_works_with_3()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            BiorepIntensity b1 = new BiorepIntensity(false, 1.ToString(), "n", 99);
            BiorepIntensity b2 = new BiorepIntensity(false, 2.ToString(), "n", 101);
            BiorepIntensity b3 = new BiorepIntensity(false, 3.ToString(), "n", 50);
            BiorepIntensity b5 = new BiorepIntensity(false, 1.ToString(), "s", 51);
            BiorepIntensity b6 = new BiorepIntensity(false, 2.ToString(), "s", 54);
            BiorepIntensity b7 = new BiorepIntensity(false, 3.ToString(), "s", 100);
            List<BiorepIntensity> uninduced = new List<BiorepIntensity> { b1, b2, b3 };
            List<BiorepIntensity> induced = new List<BiorepIntensity> { b5, b6, b7 };
            e.quant.TusherValues1.numeratorOriginalIntensities = uninduced;
            e.quant.TusherValues1.denominatorOriginalIntensities = induced;
            e.quant.TusherValues1.allIntensities = induced.Concat(uninduced).ToDictionary(b => new Tuple<string, string>(b.condition, b.biorep), b => b);
            Dictionary<string, List<string>> cbr = new Dictionary<string, List<string>> { { "n", new List<string> { 1.ToString(), 2.ToString(), 3.ToString() } }, { "s", new List<string> { 1.ToString(), 2.ToString(), 3.ToString() } } };
            List<ExperimentalProteoform> satisfy = new List<ExperimentalProteoform> { e };
            List<List<TusherStatistic>> perms = Sweet.lollipop.TusherAnalysis1.compute_balanced_biorep_permutation_relativeDifferences(cbr, "s", satisfy, 1);
            Assert.AreEqual(8, perms.Count);
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.7185m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.7185m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.6867m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.6867m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.6247m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.6247m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -20.7143m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 20.7143m));
        }

        [Test]
        public void quant_balanced_permutation_works_with_4()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            BiorepIntensity b1 = new BiorepIntensity(false, 1.ToString(), "n", 99);
            BiorepIntensity b2 = new BiorepIntensity(false, 2.ToString(), "n", 101);
            BiorepIntensity b3 = new BiorepIntensity(false, 3.ToString(), "n", 50);
            BiorepIntensity b4 = new BiorepIntensity(false, 4.ToString(), "n", 48);
            BiorepIntensity b5 = new BiorepIntensity(false, 1.ToString(), "s", 51);
            BiorepIntensity b6 = new BiorepIntensity(false, 2.ToString(), "s", 54);
            BiorepIntensity b7 = new BiorepIntensity(false, 3.ToString(), "s", 100);
            BiorepIntensity b8 = new BiorepIntensity(false, 4.ToString(), "s", 102);
            List<BiorepIntensity> uninduced = new List<BiorepIntensity> { b1, b2, b3, b4 };
            List<BiorepIntensity> induced = new List<BiorepIntensity> { b5, b6, b7, b8 };
            e.quant.TusherValues1.numeratorOriginalIntensities = uninduced;
            e.quant.TusherValues1.denominatorOriginalIntensities = induced;
            e.quant.TusherValues1.allIntensities = induced.Concat(uninduced).ToDictionary(b => new Tuple<string, string>(b.condition, b.biorep), b => b);
            Dictionary<string, List<string>> cbr = new Dictionary<string, List<string>> { { "n", new List<string> { 1.ToString(), 2.ToString(), 3.ToString(), 4.ToString() } }, { "s", new List<string> { 1.ToString(), 2.ToString(), 3.ToString(), 4.ToString() } } };
            List<ExperimentalProteoform> satisfy = new List<ExperimentalProteoform> { e };
            List<List<TusherStatistic>> perms = Sweet.lollipop.TusherAnalysis1.compute_balanced_biorep_permutation_relativeDifferences(cbr, "s", satisfy, 1);
            Assert.AreEqual(16, perms.Count);
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 20.6704m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -20.6704m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.0585m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.0585m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.0351m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.0351m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -0.0585m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 0.0585m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 1.4336m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -1.4336m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 1.3968m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -1.3968m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -1.1900m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 1.1900m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == -1.3253m));
            Assert.AreEqual(1, perms.SelectMany(x => x).Count(v => Math.Round(v.relative_difference, 4) == 1.3253m));
        }

        [Test]
        public void make_multiple_biorepintensity_litss()
        {
            double proteoformMass = 1000d;
            int lysineCount = 3;
            double intensity = 100;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-TestExperimentalProteoform.missed_monoisotopics, TestExperimentalProteoform.missed_monoisotopics * 2 + 1).ToList();

            Sweet.lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components(proteoformMass, 99d, 51d, 1.ToString(), lysineCount);//these are for quantification
            quant_components_list.AddRange(generate_neucode_quantitative_components(proteoformMass, 101d, 54d, 2.ToString(), lysineCount));//these are for quantification
            List<IAggregatable> components = generate_neucode_components(proteoformMass, intensity, intensity / 2d, lysineCount); // these are for indentification
            Sweet.lollipop.input_files = quant_components_list.Select(c => c.input_file).Distinct().ToList();
            Sweet.lollipop.getConditionBiorepFractionLabels(true, Sweet.lollipop.input_files);
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, quant_components_list, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", components[0], components, quant_components_list.Concat(generate_neucode_quantitative_components(proteoformMass, 50d, 100d, 3.ToString(), lysineCount)).ToList(), true);
            Sweet.lollipop.computeBiorepIntensities(new List<ExperimentalProteoform> { e1, e2 }, Sweet.lollipop.ltConditionsBioReps.Keys, Sweet.lollipop.hvConditionsBioReps.Keys);
            Assert.True(e1.biorepIntensityList.Count > 0);
            Assert.True(e2.biorepIntensityList.Count > 0);
        }

        [Test]
        public void quantificationInitialization()
        {
            //neucode labelled
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.neucode_labeled = true;
            InputFile i1 = new InputFile("fake.txt", Purpose.Quantification);
            i1.biological_replicate = "1";
            i1.lt_condition = "light";
            i1.hv_condition = "heavy";
            Sweet.lollipop.input_files.Add(i1);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i2 = new InputFile("fake.txt", Purpose.Quantification);
            i2.biological_replicate = "2";
            i2.lt_condition = "light";
            i2.hv_condition = "heavy";
            Sweet.lollipop.input_files.Add(i2);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(2, Sweet.lollipop.countOfBioRepsInOneCondition);

            //unlabelled
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.input_files.Clear();
            InputFile i3 = new InputFile("fake.txt", Purpose.Quantification);
            i3.biological_replicate = "1";
            i3.lt_condition = "A";
            Sweet.lollipop.input_files.Add(i3);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i4 = new InputFile("fake.txt", Purpose.Quantification);
            i4.biological_replicate = "1";
            i4.lt_condition = "B";
            Sweet.lollipop.input_files.Add(i4);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i5 = new InputFile("fake.txt", Purpose.Quantification);
            i5.biological_replicate = "1";
            i5.lt_condition = "C";
            Sweet.lollipop.input_files.Add(i5);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i6 = new InputFile("fake.txt", Purpose.Quantification);
            i6.biological_replicate = "2";
            i6.lt_condition = "A";
            Sweet.lollipop.input_files.Add(i6);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i7 = new InputFile("fake.txt", Purpose.Quantification);
            i7.biological_replicate = "2";
            i7.lt_condition = "B";
            Sweet.lollipop.input_files.Add(i7);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(1, Sweet.lollipop.countOfBioRepsInOneCondition);

            InputFile i8 = new InputFile("fake.txt", Purpose.Quantification);
            i8.biological_replicate = "2";
            i8.lt_condition = "C";
            Sweet.lollipop.input_files.Add(i8);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files);
            Assert.AreEqual(2, Sweet.lollipop.countOfBioRepsInOneCondition);
        }

        [Test]
        public void permutations()
        {
            var resultOne = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6 }, 2);
            Assert.AreEqual(15, resultOne.Count());
            var resultTwo = ExtensionMethods.Combinations(new List<int> { 1 }, 1);
            Assert.AreEqual(1, resultTwo.Count());
            var resultThree = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6 }, 6);
            Assert.AreEqual(1, resultThree.Count());
            var resultFour = ExtensionMethods.Combinations(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }, 4);
            Assert.AreEqual(70, resultFour.Count());
        }


        class DummyBiorepable : IFileIntensity //empty class that inherits IBiorepable. useful for testing.
        {
            public InputFile input_file { get; set; }

            public double intensity_sum { get; set; }

            public DummyBiorepable(InputFile inFile, double intSum)
            {
                input_file = inFile;
                intensity_sum = intSum;
            }
        }

        [Test]
        public void testbiorepintensity()
        {
            Sweet.lollipop.neucode_labeled = true;
            List<string> lightConditions = new List<string> { "light" };
            List<string> heavyConditions = new List<string> { "heavy" };

            List<DummyBiorepable> db = new List<DummyBiorepable>();
            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 1.ToString(), (-1).ToString(), (-1).ToString()), 1d));
            }

            List<BiorepIntensity> bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, db, lightConditions, heavyConditions);
            Assert.AreEqual(2, bril.Count());

            for (int i = 0; i < 6; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 2.ToString(), (-1).ToString(), (-1).ToString()), 2d));
            }
            bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, db, lightConditions, heavyConditions);
            Assert.AreEqual(4, bril.Count());

            Sweet.lollipop.neucode_labeled = false;
            bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, new List<DummyBiorepable>(), lightConditions, heavyConditions);
            Assert.AreEqual(2, bril.Count());
        }

        [Test]
        public void testmixedbiorepintensity()
        {
            Sweet.lollipop.neucode_labeled = true;
            List<string> lightConditions = new List<string> { "light", "heavy" };
            List<string> heavyConditions = new List<string> { "heavy", "light" };

            List<DummyBiorepable> db = new List<DummyBiorepable>();
            for (int i = 0; i < 4; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "light", "heavy", 1.ToString(), (-1).ToString(), (-1).ToString()), 1d));
            }
            for (int i = 0; i < 2; i++)
            {
                db.Add(new DummyBiorepable(ConstructorsForTesting.InputFile("path.txt", Labeling.NeuCode, Purpose.Quantification, "heavy", "light", 1.ToString(), (-1).ToString(), (-1).ToString()), 1d));
            }

            List<BiorepIntensity> bril = ConstructorsForTesting.ExperimentalProteoform("E").make_biorepIntensityList(db, db, lightConditions, heavyConditions);
            Assert.AreEqual(2, bril.Count());
        }

        [Test]
        public void testComputeIndividualProteoformFDR()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.useFoldChangeCutoff = false;
            Sweet.lollipop.useAveragePermutationFoldChange = false;
            Sweet.lollipop.useBiorepPermutationFoldChange = false;
            List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>();

            List<List<TusherStatistic>> permutedStats = new List<List<TusherStatistic>>();
            for (int i = 0; i < 10; i++) //proteoforms
            {
                ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
                for (int j = 0; j < 10; j++) // permutations
                {
                    if (i == 0) permutedStats.Add(new List<TusherStatistic>());
                    permutedStats[j].Add(new TusherStatistic(j, j, new List<decimal>()));
                }
                e.quant.TusherValues1.relative_difference = ((decimal)i / 10);
                e.quant.TusherValues1.fold_change = ((decimal)i / 10);
                e.quant.TusherValues1.tusher_statistic = new TusherStatistic(e.quant.TusherValues1.relative_difference, e.quant.TusherValues1.fold_change, new List<decimal> { e.quant.TusherValues1.fold_change });
                satisfactoryProteoforms.Add(e);
            }

            Sweet.lollipop.TusherAnalysis1.computeSortedRelativeDifferences(satisfactoryProteoforms, permutedStats);
            Sweet.lollipop.TusherAnalysis1.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, permutedStats.SelectMany(x => x).ToList(), Sweet.lollipop.TusherAnalysis1.sortedProteoformRelativeDifferences);

            //testStatistic = 0.2m;
            Assert.AreEqual(1.125, satisfactoryProteoforms[2].quant.TusherValues1.roughSignificanceFDR);

            //testStatistic = 0.8m;
            Assert.AreEqual(4.5, satisfactoryProteoforms[8].quant.TusherValues1.roughSignificanceFDR);
        }

        [Test]
        public void test_computeExperimentalProteoformFDR()
        {
            Sweet.lollipop.useFoldChangeCutoff = false;
            decimal testStatistic = 2m;
            List<TusherStatistic> permutedTestStatistics = new List<TusherStatistic>();
            int satisfactoryProteoformsCount = 10;
            List<TusherStatistic> sortedProteoformTestStatistics = new List<TusherStatistic>();

            for (int i = 1; i <= satisfactoryProteoformsCount; i++)
            {
                decimal stat = 5m / (decimal)i;
                sortedProteoformTestStatistics.Add(new TusherStatistic(stat, stat, new List<decimal>()));
                List<TusherStatistic> pts = new List<TusherStatistic>();

                for (int j = -3; j <= 3; j++)
                {
                    if (j != 0)
                        pts.Add(new TusherStatistic(j, Math.Abs(j) + 1, new List<decimal>()));
                }
                permutedTestStatistics.AddRange(pts);
            }

            // 10 experimental proteoforms
            // test statistics: { 5, 2.5, 1.25 ..., 1 }
            // permuted test statistics for each: {-3, -2, -1, 1, 2, 3} with average fold changes {-2, -1, 0, 1, 2, 3, 4} and fold changes { {-3,-2,-1}, {-2,-1,0}, {-1,0,1}, {0,1,2}, {1,2,3}, {2,3,4}, }
            // lower threshold is -2; upper threshold is 2
            // 2 permuted test statistics pass each of 10 times (-2 and 2), therefore 20 permuted test statistics pass
            // estimated passing false proteoforms = 20 permuted test statistics pass / 60 total test statistics * 10 proteoforms = 3.333 proteoforms
            // 2 proteoform test statistic passes
            // FDR = 3.33 / 2 = 1.6666
            Assert.AreEqual(1.67m, Math.Round(QuantitativeProteoformValues.computeExperimentalProteoformFDR(testStatistic, permutedTestStatistics, satisfactoryProteoformsCount, sortedProteoformTestStatistics), 2));

            Sweet.lollipop.useFoldChangeCutoff = true;
            Sweet.lollipop.minBiorepsWithFoldChange = 2;
            Sweet.lollipop.foldChangeCutoff = 2.51m;
            Assert.AreEqual(3.33m, Math.Round(QuantitativeProteoformValues.computeExperimentalProteoformFDR(testStatistic, permutedTestStatistics, satisfactoryProteoformsCount, sortedProteoformTestStatistics), 2));
        }

        [Test]
        public void test_addBiorepIntensity()
        {
            List<BiorepIntensity> briList = new List<BiorepIntensity>();

            Random random = new Random(1);
            for (int i = 0; i < 100000; i++)
            {
                briList.Add(new BiorepIntensity(true, 1.ToString(), "key", QuantitativeProteoformValues.imputed_intensity(20m, 1m, true, random))); // based on log 2 intensities
            }

            List<double> allIntensity = briList.Select(b => b.intensity_sum).ToList();
            double log_average = allIntensity.Average(i => Math.Log(i, 2));
            double log_sum = allIntensity.Sum(d => Math.Pow(Math.Log(d, 2) - log_average, 2));
            double log_stdev = Math.Sqrt(log_sum / (allIntensity.Count - 1));

            Assert.AreEqual(20d, Math.Round(log_average, 2));
            Assert.AreEqual(1.0d, Math.Round(log_stdev, 1));
        }

        [Test]
        public void test_imputedIntensities()
        {
            List<BiorepIntensity> briList = new List<BiorepIntensity>();
            List<BiorepIntensity> imputed = TusherValues1.imputedIntensities(briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), "light", new List<string> { 0.ToString(), 1.ToString(), 2.ToString() }, false, new Random());
            //we started with no real observations but there were three observed bioreps in the experiment. Therefore, we need 0 imputed bioreps.
            Assert.AreEqual(3, imputed.Count(b => b.imputed));
            Assert.AreEqual("light", imputed[0].condition);
            Assert.AreEqual(true, imputed.Any(b => b.condition == "light" && b.biorep == "0"));
            Assert.AreEqual(true, imputed.Any(b => b.condition == "light" && b.biorep == "1"));
            Assert.AreEqual(true, imputed.Any(b => b.condition == "light" && b.biorep == "2"));

            imputed.Clear();
            briList.Add(new BiorepIntensity(false, 0.ToString(), "light", 1000d));
            imputed.AddRange(TusherValues1.imputedIntensities(briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), "light", new List<string> { 0.ToString(), 1.ToString(), 2.ToString() }, false, new Random()));

            Assert.AreEqual(2, imputed.Count(b => b.imputed));//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(0, imputed.Count(b => !b.imputed));//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual(1, briList.Count(b => !b.imputed));//we started with one real observation but there were three observed bioreps in the experiment. Therefore we need 2 imputed bioreps
            Assert.AreEqual("light", imputed[0].condition);
            Assert.AreEqual(false, imputed.Any(b => b.biorep == "0"));
            Assert.AreEqual(true, imputed.Any(b => b.biorep == "1"));
            Assert.AreEqual(true, imputed.Any(b => b.biorep == "2"));

            imputed.Clear();
            briList.Clear();
            briList.Add(new BiorepIntensity(false, 0.ToString(), "light", 1000d));
            briList.Add(new BiorepIntensity(false, 1.ToString(), "light", 2000d));
            briList.Add(new BiorepIntensity(false, 2.ToString(), "light", 3000d));
            imputed.AddRange(TusherValues1.imputedIntensities(briList, (decimal)Math.Log(100d, 2), (decimal)Math.Log(5d, 2), "light", new List<string> { 0.ToString(), 1.ToString(), 2.ToString() }, false, new Random()));

            Assert.AreEqual(0, imputed.Count(b => b.imputed));//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(0, imputed.Count(b => !b.imputed));//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.AreEqual(3, briList.Count(b => !b.imputed));//we started with three real observations and there were three observed bioreps in the experiment. Therefore we need 0 imputed bioreps
            Assert.IsEmpty(imputed);
            Assert.AreEqual(false, imputed.Any(b => b.biorep == "0"));
            Assert.AreEqual(false, imputed.Any(b => b.biorep == "1"));
            Assert.AreEqual(false, imputed.Any(b => b.biorep == "2"));
        }

        [Test]
        public void test_gaussian_area_calculation()
        {
            SortedDictionary<decimal, int> histogram = new SortedDictionary<decimal, int>();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");

            //Each biorepIntensity has a unique combination of light/heavy + condition + biorep, since that's how they're made in the program
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "second", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "second", 0));

            double log2_intensity = 0.06; //rounds up
            foreach (BiorepIntensity b in e.biorepIntensityList)
            {
                b.intensity_sum = Math.Pow(2, log2_intensity);
                log2_intensity += 0.05;
            }

            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { e };
            QuantitativeDistributions distributions = new QuantitativeDistributions(new TusherAnalysis1() as TusherAnalysis);
            List<decimal> rounded_intensities = distributions.define_rounded_intensity_distribution(exps.SelectMany(p => p.biorepIntensityList), histogram);

            //12 intensity values, bundled in twos; therefore 6 rounded values
            Assert.AreEqual(12, rounded_intensities.Count);
            Assert.AreEqual(6, histogram.Keys.Count);

            //5 bins of width 0.1 and height 2; the first one gets swallowed up in smoothing
            Assert.AreEqual(1, distributions.get_gaussian_area(histogram));
            histogram.Add(Math.Round((decimal)log2_intensity, 1), 1);

            //Added 1 bin of width 0.1 and height 1.5, since we're smoothing by taking width and the mean of the two adjacent bars
            Assert.AreEqual(1.15, distributions.get_gaussian_area(histogram));
        }

        [Test]
        public void all_intensity_distribution_avgIntensity_and_stdv_calculations()
        {
            SortedDictionary<decimal, int> histogram = new SortedDictionary<decimal, int>();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");

            //Each biorepIntensity has a unique combination of light/heavy + condition + biorep, since that's how they're made in the program
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "first", 0));
            e.biorepIntensityList.AddRange(from i in Enumerable.Range(1, 3) select new BiorepIntensity(false, i.ToString(), "second", 0));

            double log2_intensity = 1.06; //rounds up
            foreach (BiorepIntensity b in e.biorepIntensityList)
            {
                b.intensity_sum = Math.Pow(2, log2_intensity);
                log2_intensity += 0.05;
            }

            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { e };
            QuantitativeDistributions distributions = new QuantitativeDistributions(new TusherAnalysis1() as TusherAnalysis);
            List<decimal> rounded_intensities = distributions.define_rounded_intensity_distribution(exps.SelectMany(p => p.biorepIntensityList), histogram);
            distributions.get_gaussian_area(histogram);

            //ALL INTENSITIES
            //Test the standard deviation and other calculations
            distributions.defineAllObservedIntensityDistribution(exps.SelectMany(p => p.biorepIntensityList), histogram); // creates the histogram again, checking that it's cleared, too
            Assert.AreEqual(0.4m, distributions.allObservedGaussianArea);
            Assert.AreEqual(1.185, distributions.allObservedAverageIntensity);
            Assert.AreEqual(0.085m, Math.Round(distributions.allObservedStDev, 3));
            Assert.AreEqual(1.87m, Math.Round(distributions.allObservedGaussianHeight, 2));

            //The rest of the calculations should be based off of selected, so setting those to zero
            distributions.allObservedGaussianArea = 0;
            distributions.allObservedAverageIntensity = 0;
            distributions.allObservedStDev = 0;
            distributions.allObservedGaussianHeight = 0;

            //SELECTED INTENSITIES
            distributions.defineSelectObservedIntensityDistribution(exps.SelectMany(p => p.biorepIntensityList), histogram);
            Assert.AreEqual(0.4m, distributions.selectGaussianArea);
            Assert.AreEqual(1.185m, distributions.selectAverageIntensity);
            Assert.AreEqual(0.085m, Math.Round(distributions.selectStDev, 3));
            Assert.AreEqual(1.87m, Math.Round(distributions.selectGaussianHeight, 2)); //shouldn't this be calculated with the selectStDev? changed from //selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));

            //SELECTED BACKGROUND
            Dictionary<string, List<string>> qBioFractions = e.biorepIntensityList.Select(b => b.biorep).Distinct().ToDictionary(b => b, b => new List<string>());
            distributions.defineBackgroundIntensityDistribution(qBioFractions, exps, e.biorepIntensityList.Select(b => b.condition).Distinct().Count(), -2, 0.5m);
            Assert.AreEqual(1.01m, Math.Round(distributions.bkgdAverageIntensity, 2));
            Assert.AreEqual(0.043m, Math.Round(distributions.bkgdStDev, 3));
            Assert.AreEqual(0, Math.Round(distributions.bkgdGaussianHeight, 2));

            //unlabeled works similarly
            distributions.defineBackgroundIntensityDistribution(qBioFractions, exps, e.biorepIntensityList.Select(b => b.condition).Distinct().Count(), -2, 0.5m);
            Assert.AreEqual(1.01m, Math.Round(distributions.bkgdAverageIntensity, 2));
            Assert.AreEqual(0.043m, Math.Round(distributions.bkgdStDev, 3));
            Assert.AreEqual(0, Math.Round(distributions.bkgdGaussianHeight, 2));
        }

        [Test]
        public void proteoforms_meeting_criteria()
        {
            string anysingle = "Bioreps From Any Single Condition";
            string any = "Bioreps From Any Condition";
            string fromeach = "Bioreps From Each Condition";

            List<string> conditions = new List<string> { "s", "ns" };
            BiorepIntensity b1 = new BiorepIntensity(false, 1.ToString(), conditions[0], 0);
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ConstructorsForTesting.ExperimentalProteoform("E") };
            exps[0].biorepIntensityList.Add(b1);
            List<ExperimentalProteoform> exps_out = new List<ExperimentalProteoform>();

            //PASSES WHEN THERE ARE ENOUGH IN SPECIFIED CONDITIONS
            //One biorep obs passes any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs passes any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs doesn't pass for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(0, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD
            //One biorep obs doesn't pass 2 from any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            BiorepIntensity b2 = new BiorepIntensity(false, 100.ToString(), conditions[1], 0);
            exps[0].biorepIntensityList.Add(b2);

            //One biorep in each condition passes for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(1, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD IN SPECIFIC CONDITIONS UNLESS ANY-CONDITION
            //One biorep obs in two different conditions doesn't pass 2 from any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs in two different conditions passes 2 from any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs in two different conditions doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //DOESN'T PASS WHEN NOT MATCHING LISTED CONDITIONS, EXCEPT FOR ANY-CONDITION
            foreach (BiorepIntensity b in exps[0].biorepIntensityList) b.condition = "not_a_condition";
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //NOT JUST COUNTING BIOREP INTENSITIES, BUT RATHER BIOREPS WITH OBSERVATIONS
            BiorepIntensity b3 = new BiorepIntensity(false, 1.ToString(), conditions[0], 0);
            BiorepIntensity b4 = new BiorepIntensity(false, 1.ToString(), conditions[0], 0);
            BiorepIntensity b5 = new BiorepIntensity(false, 1.ToString(), conditions[0], 0);
            BiorepIntensity b6 = new BiorepIntensity(false, 1.ToString(), conditions[0], 0);
            exps[0].biorepIntensityList = new List<BiorepIntensity> { b3, b4, b5, b6 };
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            BiorepIntensity b7 = new BiorepIntensity(false, 1.ToString(), conditions[1], 0);
            BiorepIntensity b8 = new BiorepIntensity(false, 1.ToString(), conditions[1], 0);
            BiorepIntensity b9 = new BiorepIntensity(false, 1.ToString(), conditions[1], 0);
            BiorepIntensity b10 = new BiorepIntensity(false, 1.ToString(), conditions[1], 0);
            exps[0].biorepIntensityList.Add(b7);
            exps[0].biorepIntensityList.Add(b8);
            exps[0].biorepIntensityList.Add(b9);
            exps[0].biorepIntensityList.Add(b10);
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);
        }

        [Test]
        public void proteoforms_meeting_criteria2()
        {
            string anysingle = "Biorep+Techreps From Any Single Condition";
            string any = "Biorep+Techreps From Any Condition";
            string fromeach = "Biorep+Techreps From Each Condition";

            List<string> conditions = new List<string> { "s", "ns" };
            BiorepTechrepIntensity b1 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 1.ToString(), 0);
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ConstructorsForTesting.ExperimentalProteoform("E") };
            exps[0].biorepTechrepIntensityList.Add(b1);
            List<ExperimentalProteoform> exps_out = new List<ExperimentalProteoform>();

            //PASSES WHEN THERE ARE ENOUGH IN SPECIFIED CONDITIONS
            //One biorep+techrep obs passes any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 1);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition passes any-single-conditon test
            exps[0].biorepTechrepIntensityList.Add(new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 2.ToString(), 0));
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition passes any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition doesn't pass for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(0, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD
            //Two biorep+techrep obs in single condition doesn't pass 3 from any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two biorep+techrep obs in single condition doesn't pass 3 from any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two biorep+techrep obs in single condition  doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            BiorepTechrepIntensity b2 = new BiorepTechrepIntensity(false, 100.ToString(), conditions[1], 1.ToString(), 0);
            exps[0].biorepTechrepIntensityList.Add(b2);

            //At least one biorep+techrep in each condition passes for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(1, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD IN SPECIFIC CONDITIONS UNLESS ANY-CONDITION
            //Two and one biorep+techrep obs in two different conditions doesn't pass 3 from any-single-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two and one biorep+techrep obs in two different conditions passes 3 from any-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(1, exps_out.Count);

            //Two and one biorep+techrep obs in two different conditions doesn't pass 3 from for-each-conditon test
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 3);
            Assert.AreEqual(0, exps_out.Count);


            //DOESN'T PASS WHEN NOT MATCHING LISTED CONDITIONS, EXCEPT FOR ANY-CONDITION
            foreach (BiorepTechrepIntensity b in exps[0].biorepTechrepIntensityList) b.condition = "not_a_condition";
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 3);
            Assert.AreEqual(0, exps_out.Count);


            //NOT JUST COUNTING BIOREP INTENSITIES, BUT RATHER BIOREPS WITH OBSERVATIONS
            BiorepTechrepIntensity b3 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 1.ToString(), 0);
            BiorepTechrepIntensity b4 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 1.ToString(), 0);
            BiorepTechrepIntensity b5 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 1.ToString(), 0);
            BiorepTechrepIntensity b6 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[0], 1.ToString(), 0);
            exps[0].biorepTechrepIntensityList = new List<BiorepTechrepIntensity> { b3, b4, b5, b6 };
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            BiorepTechrepIntensity b7 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[1], 1.ToString(), 0);
            BiorepTechrepIntensity b8 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[1], 1.ToString(), 0);
            BiorepTechrepIntensity b9 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[1], 1.ToString(), 0);
            BiorepTechrepIntensity b10 = new BiorepTechrepIntensity(false, 1.ToString(), conditions[1], 1.ToString(), 0);
            exps[0].biorepTechrepIntensityList.Add(b7);
            exps[0].biorepTechrepIntensityList.Add(b8);
            exps[0].biorepTechrepIntensityList.Add(b9);
            exps[0].biorepTechrepIntensityList.Add(b10);
            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
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
            GoAnalysis.calculateGoTermFDR(gtns);
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
            GoAnalysis.calculateGoTermFDR(gtns);
            foreach (GoTermNumber num in gtns)
            {
                Assert.IsNotNull(num.by);
                Assert.True(num.by <= 1);
            }
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers = gtns;
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void test_get_observed_proteins()
        {
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p1 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p2 } },
                { new InputFile("fake.txt", Purpose.ProteinDatabase), new Protein[] { p3 } },
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            TheoreticalProteoform u = ConstructorsForTesting.make_a_theoretical("T2_T1_asdf_asdf", p2, dict);
            TheoreticalProteoform v = ConstructorsForTesting.make_a_theoretical("T3_T1_asdf_Asdf_Asdf", p3, dict);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            ProteoformRelation et = new ProteoformRelation(e, t, ProteoformComparison.ExperimentalTheoretical, 0, TestContext.CurrentContext.TestDirectory);
            DeltaMassPeak etp = new DeltaMassPeak(et, new HashSet<ProteoformRelation> { et });
            et.Accepted = true;
            et.peak = etp;
            etp.Accepted = true;
            e.relationships.Add(et);
            t.relationships.Add(et);
            ProteoformRelation eu = new ProteoformRelation(e, u, ProteoformComparison.ExperimentalTheoretical, 0, TestContext.CurrentContext.TestDirectory);
            DeltaMassPeak eup = new DeltaMassPeak(eu, new HashSet<ProteoformRelation> { eu });
            eu.Accepted = true;
            eu.peak = eup;
            eup.Accepted = true;
            e.relationships.Add(eu);
            u.relationships.Add(eu);
            ProteoformFamily f = new ProteoformFamily(e);
            f.construct_family();
            e.family = f;
            t.family = f;
            u.family = f;
            List<ProteinWithGoTerms> prots = Sweet.lollipop.getProteins(new List<ExperimentalProteoform> { e });
            Assert.AreEqual(2, prots.Count);
            Assert.True(prots.Select(p => p.Accession).Contains("T1"));
            Assert.True(prots.Select(p => p.Accession).Contains("T2"));
            Assert.False(prots.Select(p => p.Accession).Contains("T3"));
        }

        [Test]
        public void test_get_repressed_or_induced_proteins()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.natural_lysine_isotope_abundance, Sweet.lollipop.neucode_light_lysine, Sweet.lollipop.neucode_heavy_lysine).AA_Masses;
            Sweet.lollipop.significance_by_permutation = true;
            Sweet.lollipop.significance_by_log2FC = false;
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange = 10;
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR = 0.5m;
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity = 1;
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("ASDF", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("ASDF", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("ASDF", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
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
            f.identify_experimentals();
            ex.family = f;
            fx.family = f;
            gx.family = f;
            hx.family = f;
            t.family = f;
            u.family = f;

            //Nothing passing, but one thing passing for each
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.TusherValues1.significant = false;
            ex.quant.intensitySum = 0;
            fx.quant.tusherlogFoldChange = -12;
            fx.quant.TusherValues1.significant = false; ;
            fx.quant.intensitySum = 0;
            gx.quant.tusherlogFoldChange = 8;
            gx.quant.TusherValues1.significant = true;
            gx.quant.intensitySum = 0;
            hx.quant.tusherlogFoldChange = 8;
            hx.quant.TusherValues1.significant = false; ;
            hx.quant.intensitySum = 2;
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ex, fx, gx };
            List<ProteinWithGoTerms> prots = Sweet.lollipop.getInducedOrRepressedProteins(exps.Where(p => p.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis);
            Assert.AreEqual(0, prots.Count);

            //Nothing passing, but two things passing for each
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.TusherValues1.significant = true;
            ex.quant.intensitySum = 0;
            fx.quant.tusherlogFoldChange = -12;
            fx.quant.TusherValues1.significant = true;
            fx.quant.intensitySum = 0;
            gx.quant.tusherlogFoldChange = 8;
            gx.quant.TusherValues1.significant = true;
            gx.quant.intensitySum = 2;
            hx.quant.tusherlogFoldChange = 12;
            hx.quant.TusherValues1.significant = false; ;
            hx.quant.intensitySum = 2;
            prots = Sweet.lollipop.getInducedOrRepressedProteins(exps.Where(p => p.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis);
            Assert.AreEqual(0, prots.Count);

            //Passing
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.TusherValues1.significant = true;
            ex.quant.intensitySum = 2;
            prots = Sweet.lollipop.getInducedOrRepressedProteins(exps.Where(p => p.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis);
            Assert.AreEqual(1, prots.Count); // only taking one ET connection by definition in forming ET relations; only one is used in identify theoreticals
            Assert.True(prots.Select(p => p.Accession).Contains("T1"));
            //Assert.True(prots.Select(p => p.Accession).Contains("T2"));
            Assert.False(prots.Select(p => p.Accession).Contains("T3"));
        }

        [Test]
        public void get_interesting_pfs()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.significance_by_log2FC = true;
            ExperimentalProteoform ex = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform fx = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform gx = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform hx = ConstructorsForTesting.ExperimentalProteoform("E4");
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.Log2FoldChangeValues.significant = true;
            ex.quant.intensitySum = 2;
            fx.quant.tusherlogFoldChange = 12;
            fx.quant.Log2FoldChangeValues.significant = true;
            fx.quant.intensitySum = 2;
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ex, fx, gx, hx };
            List<ExperimentalProteoform> interesting = Sweet.lollipop.getInterestingProteoforms(exps.Where(e => e.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis).ToList();
            Assert.AreEqual(2, interesting.Count);

            Sweet.lollipop.significance_by_permutation = true;
            Sweet.lollipop.significance_by_log2FC = false;
            ex = ConstructorsForTesting.ExperimentalProteoform("E1");
            fx = ConstructorsForTesting.ExperimentalProteoform("E2");
            gx = ConstructorsForTesting.ExperimentalProteoform("E3");
            hx = ConstructorsForTesting.ExperimentalProteoform("E4");
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.TusherValues1.significant = true;
            ex.quant.intensitySum = 2;
            fx.quant.tusherlogFoldChange = 12;
            fx.quant.TusherValues1.significant = true;
            fx.quant.intensitySum = 2;
            exps = new List<ExperimentalProteoform> { ex, fx, gx, hx };
            interesting = Sweet.lollipop.getInterestingProteoforms(exps.Where(e => e.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis).ToList();
            Assert.AreEqual(2, interesting.Count);
        }

        [Test]
        public void get_interesting_families()
        {
            Sweet.lollipop.significance_by_permutation = true;
            Sweet.lollipop.significance_by_log2FC = false;
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
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
            ex.quant.tusherlogFoldChange = 12;
            ex.quant.TusherValues1.significant = true;
            ex.quant.intensitySum = 2;
            fx.quant.tusherlogFoldChange = 12;
            fx.quant.TusherValues1.significant = true;
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

            List<ProteoformFamily> fams = Sweet.lollipop.getInterestingFamilies(exps.Where(p => p.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis);
            Assert.AreEqual(2, fams.Count);
            Assert.AreEqual(1, fams.Where(x => x.theoretical_proteoforms.Count == 0).Count());
            Assert.AreEqual(1, fams.Where(x => x.theoretical_proteoforms.Count == 1).Count());
            Assert.AreEqual(0, fams.Where(x => x.theoretical_proteoforms.Count == 2).Count());
            Assert.AreEqual(1, fams.Where(x => x.experimental_proteoforms.Count == 2).Count());
        }

        // WITHOUT NORMALIZATION (keep)
        //
        //[Test]
        //public void full_quant_test() // see proteoform_quantification_minimal_test.xlsx in the Examples folder for a full excel workup on this example
        //{
        //    Sweet.lollipop = new Lollipop();
        //    string[] table = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, @"full_quant_test_table.txt"));
        //    List<Tuple<string, string>> condition_bioreps = new List<Tuple<string, string>>();
        //    List<ExperimentalProteoform> prots = new List<ExperimentalProteoform>();
        //    for (int i = 0; i < table.Length; i++)
        //    {
        //        string[] line = table[i].Split('\t');
        //        if (line.Length < 7) return;
        //        if (i == 0)
        //        {
        //            condition_bioreps = Enumerable.Range(1, line.Length - 1).Select(x => line[x].Split('_')).Select(duple => new Tuple<string, string>(duple[0], duple[1])).ToList();
        //            Sweet.lollipop.input_files = condition_bioreps.Select(kv => ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, kv.Item1, "", kv.Item2, (-1).ToString(), (-1).ToString())).ToList();
        //            Sweet.lollipop.getConditionBiorepFractionLabels(false, Sweet.lollipop.input_files);
        //            Sweet.lollipop.numerator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("n"));
        //            Sweet.lollipop.denominator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("s"));
        //            Sweet.lollipop.induced_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("s"));
        //        }
        //        else
        //        {
        //            ExperimentalProteoform ep = ConstructorsForTesting.ExperimentalProteoform(line[0]);
        //            for (int j = 1; j < line.Length; j++)
        //            {
        //                Component c = new Component();
        //                string condition = table[0].Split('\t')[j].Split('_')[0];
        //                string biorep = table[0].Split('\t')[j].Split('_')[1];
        //                c.input_file = Sweet.lollipop.input_files.FirstOrDefault(f => f.lt_condition == condition && f.biological_replicate == biorep);
        //                c.intensity_sum = Convert.ToDouble(line[j]);
        //                ep.lt_quant_components.Add(c);
        //            }
        //            ep.family = new ProteoformFamily(ep);
        //            ep.family.construct_family();
        //            prots.Add(ep);
        //        }
        //    }
        //    Sweet.lollipop.target_proteoform_community.experimental_proteoforms = prots.ToArray();
        //    Sweet.lollipop.quantify();
        //    Assert.AreEqual(100, Sweet.lollipop.satisfactoryProteoforms.Count);
        //    Assert.AreEqual(0, Sweet.lollipop.satisfactoryProteoforms.SelectMany(p => p.quant.allIntensities.Values.Where(br => br.imputed)).Count()); // avoiding imputation for this test

        //    //real relative differences
        //    Assert.AreEqual(100, Sweet.lollipop.sortedProteoformRelativeDifferences.Count);
        //    Assert.AreEqual(-3.49, Math.Round(Sweet.lollipop.sortedProteoformRelativeDifferences.Min(), 2));
        //    Assert.AreEqual(4.49, Math.Round(Sweet.lollipop.sortedProteoformRelativeDifferences.Max(), 2));

        //    //averages across sorted permutations
        //    Assert.AreEqual(3, Sweet.lollipop.permutedRelativeDifferences.Count);
        //    Assert.AreEqual(300, Sweet.lollipop.flattenedPermutedRelativeDifferences.Count);
        //    Assert.AreEqual(100, Sweet.lollipop.avgSortedPermutationRelativeDifferences.Count);
        //    Assert.AreEqual(-2.09, Math.Round(Sweet.lollipop.avgSortedPermutationRelativeDifferences.Min(), 2));
        //    Assert.AreEqual(0.92, Math.Round(Sweet.lollipop.avgSortedPermutationRelativeDifferences.Max(), 2));

        //    Assert.AreEqual(-1.65, Math.Round(Sweet.lollipop.minimumPassingNegativeTestStatistic, 2));
        //    Assert.AreEqual(4.49, Math.Round(Sweet.lollipop.minimumPassingPositiveTestStatisitic, 2));
        //    Assert.AreEqual(13, Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant));
        //    Assert.AreEqual(0.1538, Math.Round(Sweet.lollipop.relativeDifferenceFDR, 4));

        //    //change up a parameter and reevaluate
        //    Sweet.lollipop.offsetTestStatistics = 0.8m;
        //    Sweet.lollipop.reestablishSignficance();
        //    Assert.AreEqual(22, Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant));
        //    Assert.AreEqual(0.1667, Math.Round(Sweet.lollipop.relativeDifferenceFDR, 4));

        //    //rough test of quant summary
        //    Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        //}

        [Test]
        public void full_quant_test() // see proteoform_quantification_minimal_test.xlsx in the Examples folder for a full excel workup on this example
        {
            Sweet.lollipop = new Lollipop();
            string[] table = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, @"full_quant_test_table.txt"));
            List<Tuple<string, string>> condition_bioreps = new List<Tuple<string, string>>();
            List<ExperimentalProteoform> prots = new List<ExperimentalProteoform>();
            for (int i = 0; i < table.Length; i++)
            {
                string[] line = table[i].Split('\t');
                if (line.Length < 7) return;
                if (i == 0)
                {
                    condition_bioreps = Enumerable.Range(1, line.Length - 1).Select(x => line[x].Split('_')).Select(duple => new Tuple<string, string>(duple[0], duple[1])).ToList();
                    Sweet.lollipop.input_files = condition_bioreps.Select(kv => ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, kv.Item1, "", kv.Item2, (-1).ToString(), (-1).ToString())).ToList();
                    Sweet.lollipop.getConditionBiorepFractionLabels(false, Sweet.lollipop.input_files);
                    Sweet.lollipop.numerator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("n"));
                    Sweet.lollipop.denominator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("s"));
                    Sweet.lollipop.induced_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("s"));
                }
                else
                {
                    ExperimentalProteoform ep = ConstructorsForTesting.ExperimentalProteoform(line[0]);
                    for (int j = 1; j < line.Length; j++)
                    {
                        Component c = new Component();
                        string condition = table[0].Split('\t')[j].Split('_')[0];
                        string biorep = table[0].Split('\t')[j].Split('_')[1];
                        c.input_file = Sweet.lollipop.input_files.FirstOrDefault(f => f.lt_condition == condition && f.biological_replicate == biorep);
                        c.intensity_sum = Convert.ToDouble(line[j]);
                        ep.lt_quant_components.Add(c);
                    }
                    ep.family = new ProteoformFamily(ep);
                    ep.family.construct_family();
                    prots.Add(ep);
                }
            }

            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = prots.ToArray();
            Sweet.lollipop.quantify();
            Assert.AreEqual(100, Sweet.lollipop.satisfactoryProteoforms.Count);
            Assert.AreEqual(0, Sweet.lollipop.satisfactoryProteoforms.SelectMany(p => p.quant.TusherValues1.allIntensities.Values.Where(br => br.imputed)).Count()); // avoiding imputation for this test

            //real relative differences
            Assert.AreEqual(100, Sweet.lollipop.TusherAnalysis1.sortedProteoformRelativeDifferences.Count);
            Assert.AreEqual(-2.08, Math.Round(Sweet.lollipop.TusherAnalysis1.sortedProteoformRelativeDifferences.Min(x => x.relative_difference), 2));
            Assert.AreEqual(5.77, Math.Round(Sweet.lollipop.TusherAnalysis1.sortedProteoformRelativeDifferences.Max(x => x.relative_difference), 2));

            //averages across sorted permutations
            Assert.AreEqual(8, Sweet.lollipop.TusherAnalysis1.permutedRelativeDifferences.Count);
            Assert.AreEqual(800, Sweet.lollipop.TusherAnalysis1.flattenedPermutedRelativeDifferences.Count);
            Assert.AreEqual(100, Sweet.lollipop.TusherAnalysis1.avgSortedPermutationRelativeDifferences.Count);
            Assert.AreEqual(-2.60, Math.Round(Sweet.lollipop.TusherAnalysis1.avgSortedPermutationRelativeDifferences.Min(), 2));
            Assert.AreEqual(2.60, Math.Round(Sweet.lollipop.TusherAnalysis1.avgSortedPermutationRelativeDifferences.Max(), 2));

            Assert.AreEqual(Decimal.MinValue, Math.Round(Sweet.lollipop.TusherAnalysis1.minimumPassingNegativeTestStatistic, 2));
            Assert.AreEqual(5.77, Math.Round(Sweet.lollipop.TusherAnalysis1.minimumPassingPositiveTestStatisitic, 2));
            Assert.AreEqual(1, Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant));
            Assert.AreEqual(0, Math.Round(Sweet.lollipop.TusherAnalysis1.relativeDifferenceFDR, 4));

            //change up a parameter and reevaluate
            Sweet.lollipop.offsetTestStatistics = 0.5m;
            Sweet.lollipop.TusherAnalysis1.reestablishSignficance(Sweet.lollipop.TusherAnalysis1 as IGoAnalysis);
            Assert.AreEqual(9, Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant));
            Assert.AreEqual(0.25, Math.Round(Sweet.lollipop.TusherAnalysis1.relativeDifferenceFDR, 4));

            //rough test of quant summary
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void full_log2fc_quant_test() // see biorep_fraction_techrep_intensities_logFC_ttest_minimal.xlsx in the Examples folder for a full excel workup on this example
        {
            Sweet.lollipop = new Lollipop();
            string[] table = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, @"full_log2FC_quant_test_table.txt"));
            List<string> conditions = new List<string>();
            List<Tuple<string, string, string, string>> condition_bioreps = new List<Tuple<string, string, string, string>>();
            List<ExperimentalProteoform> prots = new List<ExperimentalProteoform>();
            for (int i = 0; i < table.Length; i++)
            {
                string[] line = table[i].Split('\t');
                if (line.Length < 7) return;
                if (i == 0)
                {
                    condition_bioreps = Enumerable.Range(1, line.Length - 1).Select(x => line[x].Split('_')).Select(duple => new Tuple<string, string, string, string>(duple[0], duple[1], duple[2], duple[3])).ToList();
                    conditions = condition_bioreps.Select(x => x.Item1).Distinct().ToList();
                    Sweet.lollipop.input_files = condition_bioreps.DistinctBy(x => x.Item2 + x.Item3 + x.Item4).Select(kv => ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, conditions[0], conditions[1], kv.Item2, kv.Item3, kv.Item4)).ToList();
                    Sweet.lollipop.getConditionBiorepFractionLabels(true, Sweet.lollipop.input_files);
                    Sweet.lollipop.numerator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("Stress"));
                    Sweet.lollipop.denominator_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("Normal"));
                    Sweet.lollipop.induced_condition = Sweet.lollipop.conditionsBioReps.Keys.FirstOrDefault(x => x.StartsWith("Stress"));
                }
                else
                {
                    ExperimentalProteoform ep = ConstructorsForTesting.ExperimentalProteoform(line[0]);
                    for (int j = 1; j < line.Length; j++)
                    {
                        Component c = new Component();
                        string condition = table[0].Split('\t')[j].Split('_')[0];
                        string biorep = table[0].Split('\t')[j].Split('_')[1];
                        string fraction = table[0].Split('\t')[j].Split('_')[2];
                        string techrep = table[0].Split('\t')[j].Split('_')[3];
                        c.input_file = Sweet.lollipop.input_files.FirstOrDefault(f => f.biological_replicate == biorep && f.fraction == fraction && f.technical_replicate == techrep);
                        bool is_number = Double.TryParse(line[j], out double value);
                        if (!is_number) continue; // many don't have values
                        c.intensity_sum = value;
                        if (condition == conditions[0]) ep.lt_quant_components.Add(c);
                        else ep.hv_quant_components.Add(c);
                    }
                    ep.family = new ProteoformFamily(ep);
                    ep.family.construct_family();
                    prots.Add(ep);
                }
            }
            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = prots.ToArray();
            Sweet.lollipop.quantify();
            Assert.AreEqual(216, Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count);

            Sweet.lollipop.Log2FoldChangeAnalysis.compute_proteoform_statistics(Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.conditionsBioReps, Sweet.lollipop.numerator_condition, Sweet.lollipop.denominator_condition, Sweet.lollipop.induced_condition, Sweet.lollipop.sKnot_minFoldChange, true);

            // Check mixing normalization
            List<BiorepIntensity> allBfts = Sweet.lollipop.satisfactoryProteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.allIntensities.Values.Where(i => !i.imputed)).ToList();
            Assert.AreEqual(596, allBfts.Count);
            //normalize before imputing so should be normalized to these values....
            Assert.AreEqual(Math.Round(allBfts.Where(bft => bft.condition == conditions[0]).Sum(bft => bft.intensity_sum), 1), Math.Round(allBfts.Where(bft => bft.condition == conditions[1]).Sum(bft => bft.intensity_sum), 1));

            //Check that all values are imputed
            Assert.True(Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorep_avgLog2I.Values.All(v => 16.4 < v && v < 26.9));
            Assert.True(Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorep_stdevLog2I.Values.All(v => 0.66 < v && v < 3.6));
            Assert.AreEqual(600, Sweet.lollipop.satisfactoryProteoforms.Sum(pf => pf.quant.Log2FoldChangeValues.allIntensities.Values.Count));
            Assert.AreEqual(0, Sweet.lollipop.satisfactoryProteoforms.Count(pf => pf.quant.Log2FoldChangeValues.pValue_uncorrected < 0.0001));
            Assert.AreEqual(0, Sweet.lollipop.satisfactoryProteoforms.Count(pf => pf.quant.Log2FoldChangeValues.significant));
        }

        [Test]
        public void ttest()
        {
            Assert.AreEqual(1e-5, Math.Round(ExtensionMethods.Student2T(11.2, 7), 6));
            Assert.AreEqual(0.900, Math.Round(ExtensionMethods.Student2T(0.127379, 21), 3));
        }

        [Test]
        public void gauss()
        {
            Assert.AreEqual(0.50, Math.Round(ExtensionMethods.Gauss(0), 2));
            Assert.AreEqual(1.00, Math.Round(ExtensionMethods.Gauss(6), 2));
            Assert.AreEqual(1.00, Math.Round(ExtensionMethods.Gauss(10), 2));
            Assert.AreEqual(0.98, Math.Round(ExtensionMethods.Gauss(2), 2));
        }

        //Example saved in benjihoch.xlsx in Examples folder
        [Test]
        public void benjiHoch()
        {
            Sweet.lollipop = new Lollipop();

            for(int i = 1; i <= 100; i++)
            {
                ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("");
                e.quant.Log2FoldChangeValues.pValue_uncorrected = ExtensionMethods.Student2T((double)i / 10, 10);
                Sweet.lollipop.satisfactoryProteoforms.Add(e);
            }
            Sweet.lollipop.Log2FoldChangeAnalysis.minFoldChange = 0;
            Sweet.lollipop.Log2FoldChangeAnalysis.establish_benjiHoch_significance();

            Assert.AreEqual(77, Sweet.lollipop.satisfactoryProteoforms.Count(pf => pf.quant.Log2FoldChangeValues.significant));
        }

        [Test]
        public void tusher_relative_difference_test_conditions()
        {
            Sweet.lollipop = new Lollipop();
            bool is_passing_relative_difference;
            bool is_passing_fold_change;

            // relative difference passes negative easily
            TusherStatistic stat1 = new TusherStatistic(-3, 1, new List<decimal>());
            Assert.IsTrue(stat1.is_passing_real(-2, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat1.is_passing_permutation(-2, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference passes positive easily
            TusherStatistic stat2 = new TusherStatistic(3, 1, new List<decimal>());
            Assert.IsTrue(stat2.is_passing_real(-4, 2, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat2.is_passing_permutation(-4, 2, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference edge case negative
            TusherStatistic stat3 = new TusherStatistic(-3, 1, new List<decimal>());
            Assert.IsTrue(stat3.is_passing_real(-3, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat3.is_passing_permutation(-3, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference edge case positive
            TusherStatistic stat4 = new TusherStatistic(3, 1, new List<decimal>());
            Assert.IsTrue(stat4.is_passing_real(-4, 3, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat4.is_passing_permutation(-4, 3, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference not passing negative easily
            TusherStatistic stat5 = new TusherStatistic(-3, 1, new List<decimal>());
            Assert.IsFalse(stat5.is_passing_real(-4, 2, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat5.is_passing_permutation(-4, 2, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference not passing positive easily
            TusherStatistic stat6 = new TusherStatistic(3, 1, new List<decimal>());
            Assert.IsFalse(stat6.is_passing_real(-2, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat6.is_passing_permutation(-2, 4, "AND", false, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
        }

        [Test]
        public void tusher_averagefoldchange_test_conditions()
        {
            Sweet.lollipop = new Lollipop();
            bool is_passing_relative_difference;
            bool is_passing_fold_change;

            // relative difference and fold change pass easily
            TusherStatistic stat1 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsTrue(stat1.is_passing_real(-2, 4, "AND", true, 1, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat1.is_passing_permutation(-2, 4, "AND", true, 1, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // relative difference passes; fold change does not
            TusherStatistic stat2 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsFalse(stat2.is_passing_real(-2, 4, "AND", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat2.is_passing_permutation(-2, 4, "AND", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // useaverage not marked
            TusherStatistic stat3 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsFalse(stat3.is_passing_real(-2, 4, "AND", true, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat3.is_passing_permutation(-2, 4, "AND", true, 1, false, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // passing fold change, not passing rel diff, passes with or
            TusherStatistic stat4 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsTrue(stat4.is_passing_real(-4, 2, "OR", true, 1, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat4.is_passing_permutation(-4, 2, "OR", true, 1, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // passing rel diff, not passing fold change, passes with or
            TusherStatistic stat5 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsTrue(stat5.is_passing_real(-2, 4, "OR", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat5.is_passing_permutation(-2, 4, "OR", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));

            // neither passing, doesn't pass with or
            TusherStatistic stat6 = new TusherStatistic(-3, 2, new List<decimal>());
            Assert.IsFalse(stat6.is_passing_real(-4, 2, "OR", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat6.is_passing_permutation(-4, 2, "OR", true, 3, true, false, 1, out is_passing_relative_difference, out is_passing_fold_change));
        }

        [Test]
        public void tusher_biorepfoldchange_test_conditions()
        {
            Sweet.lollipop = new Lollipop();
            bool is_passing_relative_difference;
            bool is_passing_fold_change;

            // sufficient passing bioreps and passing rel diff passes
            TusherStatistic stat1 = new TusherStatistic(-3, 0, new List<decimal> { 3, 3, 1 });
            Assert.IsTrue(stat1.is_passing_real(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat1.is_passing_permutation(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // insufficient passing bioreps
            TusherStatistic stat2 = new TusherStatistic(-3, 0, new List<decimal> { 3, 1, 1 });
            Assert.IsFalse(stat2.is_passing_real(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat2.is_passing_permutation(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // insufficient passing bioreps with passing average doesn't pass
            TusherStatistic stat3 = new TusherStatistic(-3, 3, new List<decimal> { 3, 1, 1 });
            Assert.IsFalse(stat3.is_passing_real(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat3.is_passing_permutation(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // usebioreps not marked
            TusherStatistic stat4 = new TusherStatistic(-3, 0, new List<decimal> { 3, 3, 1 });
            Assert.IsFalse(stat4.is_passing_real(-2, 4, "AND", true, 2, false, false, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat4.is_passing_permutation(-2, 4, "AND", true, 2, false, false, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // passing rel diff, but not fold change, passes with or
            TusherStatistic stat5 = new TusherStatistic(-3, 0, new List<decimal> { 3, 1, 1 });
            Assert.IsTrue(stat5.is_passing_real(-2, 4, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat5.is_passing_permutation(-2, 4, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // passing fold chasnge, but not rel diff, passes with or
            TusherStatistic stat6 = new TusherStatistic(-3, 0, new List<decimal> { 3, 3, 1 });
            Assert.IsTrue(stat6.is_passing_real(-4, 2, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsTrue(stat6.is_passing_permutation(-4, 2, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // neither passing, doesn't pass with or
            TusherStatistic stat7 = new TusherStatistic(-3, 0, new List<decimal> { 3, 1, 1 });
            Assert.IsFalse(stat7.is_passing_real(-4, 2, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat7.is_passing_permutation(-4, 2, "OR", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));

            // sufficient passing on either side of 1 doesn't count as passing
            TusherStatistic stat8 = new TusherStatistic(-3, 0, new List<decimal> { 3, 0.25m, 1 });
            Assert.IsFalse(stat8.is_passing_real(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
            Assert.IsFalse(stat8.is_passing_permutation(-2, 4, "AND", true, 2, false, true, 2, out is_passing_relative_difference, out is_passing_fold_change));
        }
    }
}
