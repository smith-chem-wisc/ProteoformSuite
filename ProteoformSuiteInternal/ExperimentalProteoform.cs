using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    //Note ExperimentalProteoform is a bit of a misnomer. These are not experimental observations, but rather aggregated experimental
    //observations. Each NeuCodePair is an ExperimentalProteoform, but this class is used after accounting for missed lysines and monoisotopics.
    //However, I think this makes the programming a bit cleaner, since "Experimental-Theoretical" pairs should naturally be between 
    //"ExperimentalProteoform" and "TheoreticalProteoform" objects

    public class ExperimentalProteoform : Proteoform
    {

        #region Public Field

        public Component root;

        #endregion Public Field

        #region Public Properties

        public List<Component> aggregated_components { get; set; } = new List<Component>();

        public List<Component> lt_verification_components { get; set; } = new List<Component>();

        public List<Component> hv_verification_components { get; set; } = new List<Component>();

        public List<Component> lt_quant_components { get; set; } = new List<Component>();

        public List<Component> hv_quant_components { get; set; } = new List<Component>();

        public List<BiorepIntensity> biorepIntensityList { get; set; } = new List<BiorepIntensity>(); //(bool light = true, int biorep, double intensity)

        public QuantitativeProteoformValues quant { get; set; }

        public bool accepted { get; set; } = true;

        public double agg_mass { get; set; } = 0;

        public double agg_intensity { get; set; } = 0;

        public double agg_rt { get; set; } = 0;

        public bool mass_shifted { get; set; } = false; //make sure in ET if shifting multiple peaks, not shifting same E > once. 

        public string manual_validation_id { get; set; }

        public string manual_validation_verification { get; set; }

        public string manual_validation_quant { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) 
            : base(accession)
        {
            quant = new QuantitativeProteoformValues(this);
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        public ExperimentalProteoform(string accession, Component root, bool is_target) 
            : base(accession)
        {
            quant = new QuantitativeProteoformValues(this);
            this.root = root;
            this.is_target = is_target;
        }

        public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, bool is_target, bool neucode_labeled) 
            : base(accession) //this is for first mass of aggregate components. uses a temporary component
        {
            if (neucode_labeled)
            {
                NeuCodePair ncRoot = new NeuCodePair();
                ncRoot.weighted_monoisotopic_mass = temp.agg_mass;
                ncRoot.intensity_sum = temp.agg_intensity;
                ncRoot.rt_apex = temp.agg_rt;
                ncRoot.lysine_count = temp.lysine_count;

                this.root = ncRoot;
                this.aggregated_components.AddRange(candidate_observations.Where(p => includes(p, this.root)));
                this.calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }

            else
            {
                Component cRoot = new Component();
                cRoot.weighted_monoisotopic_mass = temp.agg_mass;
                cRoot.intensity_sum = temp.agg_intensity;
                cRoot.rt_apex = temp.agg_rt;

                this.root = cRoot;
                this.aggregated_components.AddRange(candidate_observations.Where(p => includes(p, this.root)));
                calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
        }


        // COPYING CONSTRUCTOR
        public ExperimentalProteoform(ExperimentalProteoform eP)
            : base(eP.accession, eP.modified_mass, eP.lysine_count, eP.is_target)
        {
            copy_aggregate(eP);
            quant = new QuantitativeProteoformValues(this);
        }

        #endregion Public Constructors

        #region Private Methods

        private void copy_aggregate(ExperimentalProteoform e)
        {
            root = e.root;
            //doesn't copy quant on purpose
            agg_intensity = e.agg_intensity;
            agg_mass = e.agg_mass;
            modified_mass = e.modified_mass;
            agg_rt = e.agg_rt;
            lysine_count = e.lysine_count;
            accepted = e.accepted;
            mass_shifted = e.mass_shifted;
            is_target = e.is_target;
            family = e.family;
            aggregated_components = new List<Component>(e.aggregated_components);
            lt_quant_components = new List<Component>(e.lt_quant_components);
            lt_verification_components = new List<Component>(e.lt_verification_components);
            hv_quant_components = new List<Component>(e.hv_quant_components);
            hv_verification_components = new List<Component>(e.hv_verification_components);
            biorepIntensityList = new List<BiorepIntensity>(e.biorepIntensityList);
            manual_validation_id = e.manual_validation_id;
            manual_validation_verification = e.manual_validation_verification;
            manual_validation_quant = e.manual_validation_quant;
        }

        public string find_manual_inspection_component(IEnumerable<Component> components)
        {
            Component intense = components.OrderByDescending(c => c.intensity_sum).FirstOrDefault();
            if (intense == null)
                return "";

            ChargeState intense_cs = intense.charge_states.OrderByDescending(c => c.intensity).FirstOrDefault();
            if (intense_cs == null)
                return "";

            return "File: " + intense.input_file.filename 
                + "; Scan Range: " + intense.scan_range 
                + "; Charge State m/z (+" + intense_cs.charge_count.ToString() + "): " + intense_cs.mz_centroid + "; RT (min): " + intense.rt_apex;
        }

        #endregion

        #region Aggregation Public Methods

        public void aggregate()
        {
            ExperimentalProteoform temp_pf = new ExperimentalProteoform("tbd", root, new List<Component>(SaveState.lollipop.remaining_components), true); //first pass returns temporary proteoform
            ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", temp_pf, new List<Component>(SaveState.lollipop.remaining_components), true, SaveState.lollipop.neucode_labeled); //second pass uses temporary protoeform from first pass.
            copy_aggregate(new_pf); //doesn't copy quant on purpose
            root = temp_pf.root; //maintain the original component root
        }

        public void verify()
        {
            foreach (Component c in SaveState.lollipop.remaining_verification_components)
            {
                if (includes_neucode_component(c, this, true))
                    lt_verification_components.Add(c);
                if (SaveState.lollipop.neucode_labeled && includes_neucode_component(c, this, false))
                    hv_verification_components.Add(c);
            }
        }

        public void assign_quantitative_components()
        {
            foreach (Component c in SaveState.lollipop.remaining_quantification_components)
            {
                if (includes_neucode_component(c, this, true))
                    lt_quant_components.Add(c);
                if (includes_neucode_component(c, this, false))
                    hv_quant_components.Add(c);
            }

            //lt_quant_components.AddRange(Lollipop.remaining_components.Where(r => this.includes(r, this, true)));
            ////ep.getBiorepAndFractionIntensities(false); //split lt components by biorep and fraction
            //hv_quant_components.AddRange(Lollipop.remaining_components.Where(r => this.includes(r, this, false)));
            ////ep.getBiorepAndFractionIntensities(true); //split hv components by biorep and fraction
        }

        public void calculate_properties()
        {
            //if not neucode labeled, the intensity sum of overlapping charge states was calculated with all charge states.
            agg_intensity = aggregated_components.Sum(c => SaveState.lollipop.neucode_labeled ? c.intensity_sum_olcs : c.intensity_sum);
            agg_mass = aggregated_components.Sum(c => (c.weighted_monoisotopic_mass - Math.Round(c.weighted_monoisotopic_mass - root.weighted_monoisotopic_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS) * (SaveState.lollipop.neucode_labeled ? c.intensity_sum_olcs : c.intensity_sum) / agg_intensity); //remove the monoisotopic errors before aggregating masses
            agg_rt = aggregated_components.Sum(c => c.rt_apex * (SaveState.lollipop.neucode_labeled ? c.intensity_sum_olcs : c.intensity_sum) / agg_intensity);
            lysine_count = root as NeuCodePair != null ? (root as NeuCodePair).lysine_count : lysine_count;
            modified_mass = agg_mass;
            accepted = aggregated_components.Count >= SaveState.lollipop.min_agg_count && aggregated_components.Select(c => c.input_file.biological_replicate).Distinct().Count() >= SaveState.lollipop.min_num_bioreps;
        }

        //This aggregates based on lysine count, mass, and retention time all at the same time. Note that in the past we aggregated based on 
        //lysine count first, and then aggregated based on mass and retention time afterwards, which may give a slightly different root for the 
        //experimental proteoform because the precursor aggregation may shuffle the intensity order slightly. We haven't observed any negative
        //impact of this difference as of 160812. -AC
        public bool includes(Component candidate, Component root)
        {
            return tolerable_rt(candidate, root.rt_apex) && tolerable_mass(candidate, root.weighted_monoisotopic_mass)
                && (candidate as NeuCodePair == null || tolerable_lysCt(candidate as NeuCodePair, (root as NeuCodePair).lysine_count));
        }

        public bool includes_neucode_component(Component candidate, ExperimentalProteoform root, bool light)
        {
            double corrected_mass = light ?
                root.agg_mass :
                root.agg_mass + root.lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
            return tolerable_rt(candidate, root.agg_rt) && tolerable_mass(candidate, corrected_mass);
        }

        #endregion Aggregation Public Methods

        #region Aggregation Private Methods

        private bool tolerable_rt(Component candidate, double rt_apex)
        {
            return candidate.rt_apex >= rt_apex - Convert.ToDouble(SaveState.lollipop.retention_time_tolerance) &&
                candidate.rt_apex <= rt_apex + Convert.ToDouble(SaveState.lollipop.retention_time_tolerance);
        }

        private bool tolerable_lysCt(NeuCodePair candidate, int lysine_count)
        {
            int max_missed_lysines = Convert.ToInt32(SaveState.lollipop.missed_lysines);
            List<int> acceptable_lysineCts = Enumerable.Range(lysine_count - max_missed_lysines, max_missed_lysines * 2 + 1).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(Component candidate, double corrected_mass)
        {
            int max_missed_monoisotopics = Convert.ToInt32(SaveState.lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (int missed_mono_count in missed_monoisotopics_range)
            {
                double shift = missed_mono_count * Lollipop.MONOISOTOPIC_UNIT_MASS;
                double shifted_mass = corrected_mass + shift;
                double mass_tolerance = shifted_mass / 1000000 * (double)SaveState.lollipop.mass_tolerance;
                double low = shifted_mass - mass_tolerance;
                double high = shifted_mass + mass_tolerance;
                bool tolerable_mass = candidate.weighted_monoisotopic_mass >= low && candidate.weighted_monoisotopic_mass <= high;
                if (tolerable_mass)
                    return true; //Return a true result immediately; acts as an OR between these conditions
            }
            return false;
        }

        #endregion Aggregation Private Methods

        #region Quantitation Public Method

        public List<BiorepIntensity> make_biorepIntensityList<T>(List<T> lt_quant_components, List<T> hv_quant_components, IEnumerable<string> ltConditionStrings, IEnumerable<string> hvConditionStrings)
            where T : IBiorepable
        {
            quant = new QuantitativeProteoformValues(this); //Reset quantitation if starting over from biorep requirements

            List<BiorepIntensity> biorepIntensityList = new List<BiorepIntensity>();
            foreach (string condition in ltConditionStrings.ToList())
            {
                foreach (int b in lt_quant_components.Where(c => c.input_file.lt_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                {
                    biorepIntensityList.Add(new BiorepIntensity(true, false, b, condition, lt_quant_components.Where(c => c.input_file.biological_replicate == b).Sum(i => i.intensity_sum)));
                }
            }

            if (SaveState.lollipop.neucode_labeled)
            {
                foreach (string condition in hvConditionStrings.ToList())
                {
                    foreach (int b in hv_quant_components.Where(c => c.input_file.hv_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                    {
                        biorepIntensityList.Add(new BiorepIntensity(false, false, b, condition, hv_quant_components.Where(c => c.input_file.biological_replicate == b).Sum(i => i.intensity_sum)));
                    }
                }
            }
            this.biorepIntensityList = biorepIntensityList;
            return biorepIntensityList;
        }

        #endregion Quantitation Public Method
       
        #region Public Methods

        public void shift_masses(int shift, bool neucode_labeled)
        {
            foreach (Component c in aggregated_components)
            {
                c.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                if (neucode_labeled)
                {
                    (c as NeuCodePair).neuCodeLight.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    (c as NeuCodePair).neuCodeHeavy.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                }
            }

            mass_shifted = true; //if shifting multiple peaks @ once, won't shift same E more than once if it's in multiple peaks.
        }

        #endregion Public Methods

    }
}
