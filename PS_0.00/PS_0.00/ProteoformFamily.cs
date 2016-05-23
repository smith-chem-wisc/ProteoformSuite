using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
   public class ProteoformCommunity
    {
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; } = new List<ExperimentalProteoform>();
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; } = new List<TheoreticalProteoform>();
        public Dictionary<string, List<TheoreticalProteoform>> decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
        public List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        private Proteoform[][] unequal_lysine_proteoform_subsets;
        public List<ProteoformRelation> unequal_lysine_relations = new List<ProteoformRelation>();

        //INITIALIZATION of PROTEOFORMS
        public void add(string accession, NeuCodePair candidate)
        {
            foreach(ExperimentalProteoform pf in experimental_proteoforms) //Need to keep order of proteoforms coming in to keep ordering of max intensity; parallelized beforehand
            {
                if (pf.includes(candidate)) pf.add(candidate); return;
            }
            ExperimentalProteoform new_pf = new ExperimentalProteoform(accession, candidate);
            //Lollipop.experimental_proteoforms.Add(new_pf);
            this.experimental_proteoforms.Add(new_pf);
        }

        public void add(ExperimentalProteoform pf)
        {
            this.experimental_proteoforms.Add(pf);
        }

        public void add(TheoreticalProteoform pf)
        {
            this.theoretical_proteoforms.Add(pf);
        }

        public void add(TheoreticalProteoform pf, string decoy_database)
        {
            this.decoy_proteoforms[decoy_database].Add(pf);
        }

        public void calculate_aggregated_experimental_masses()
        {
            Parallel.ForEach<ExperimentalProteoform>(this.experimental_proteoforms, pf => { pf.calculate_properties(); });
        }

        public void Clear()
        {
            this.experimental_proteoforms.Clear();
            this.theoretical_proteoforms.Clear();
            this.decoy_proteoforms.Clear();
        }

        //ANALYZING RELATIONSHIPS
        public void make_relationships()
        {
            Parallel.Invoke(
                () => this.et_relations = relate(experimental_proteoforms.ToArray(), theoretical_proteoforms.ToArray()),
                () => this.ee_relations = relate(experimental_proteoforms.ToArray(), experimental_proteoforms.ToArray()),
                () => Parallel.ForEach<KeyValuePair<string, List<TheoreticalProteoform>>>(this.decoy_proteoforms, decoys => 
                    { ed_relations[decoys.Key] = relate(experimental_proteoforms.ToArray(), decoys.Value.ToArray()); }),
                () =>
                    { unequal_lysine_proteoform_subsets = random_unequal_lysine_relations();
                    relate(unequal_lysine_proteoform_subsets); }
            );
        }

        private List<ProteoformRelation> relate(Proteoform[] pfs1, Proteoform[] pfs2)
        {
            return new List<ProteoformRelation>( 
                from pf1 in pfs1
                from pf2 in pfs2
                select new ProteoformRelation(pf1, pf2, pf1.modified_mass - pf2.modified_mass)
            );
        }

        private void relate(Proteoform[][] unequal_lysine_relations)
        {
            relate(unequal_lysine_relations[0], unequal_lysine_relations[1]);
        }

        private Proteoform[][] random_unequal_lysine_relations()
        {
            Proteoform[][] unequal_lysine_relations = new Proteoform[2][];
            unequal_lysine_relations[0] = new Proteoform[this.experimental_proteoforms.Count];
            unequal_lysine_relations[1] = new Proteoform[this.experimental_proteoforms.Count];
            Parallel.For(0, unequal_lysine_relations.Length, i =>
            {
                ExperimentalProteoform experimental_pf = this.experimental_proteoforms[new Random().Next(this.experimental_proteoforms.Count)];
                unequal_lysine_relations[0][i] = experimental_pf;
                List<ExperimentalProteoform> unequal_lysine_pfs = this.experimental_proteoforms.Where(pf => pf.lysine_count != experimental_pf.lysine_count).ToList();
                unequal_lysine_relations[1][i] = unequal_lysine_pfs[new Random().Next(unequal_lysine_pfs.Count)];
            });
            return unequal_lysine_relations;
        }

        public void find_peaks_and_thresholds()
        {
            Parallel.Invoke(
                () => Parallel.Invoke(
                    () => calculate_local_densities(et_relations.OrderBy(r => r.delta_mass)),
                    () => Parallel.ForEach<KeyValuePair<string, List<ProteoformRelation>>>(this.ed_relations, decoy_relations =>
                        { calculate_local_densities(decoy_relations.Value.OrderBy(r => r.delta_mass)); })
                    ),
                () => compute_and_set_et_threshold()
            );

            Parallel.Invoke(
                () => Parallel.Invoke(
                    () => calculate_local_densities(ee_relations.OrderBy(r => r.delta_mass)),
                    () => calculate_local_densities(unequal_lysine_relations.OrderBy(r => r.delta_mass))
                ),
                () => compute_and_set_ee_threshold()
            );
        }

        //Average the nearby deltaM twice to compute a count for each peak
        private void calculate_local_densities(IOrderedEnumerable<ProteoformRelation> ordered_relations)
        {
            List<ProteoformRelation> relations = ordered_relations.ToList();
            Parallel.For(0, relations.Count, i =>
            {
                relations[i].set_peak(select_nearby_relations(i, relations, relations[i]));
                relations[i].set_peak(select_nearby_relations(i, relations, relations[i]));
            });
        }

        //Very fast, assuming an ascending-ordered list by delta mass, and assuming the number of nearby ProteoformRelations is small compared to the size of the list
        private List<ProteoformRelation> select_nearby_relations(int i, List<ProteoformRelation> relations, ProteoformRelation base_relation)
        {
            double lower_limit = base_relation.local_peak_deltaM - Convert.ToDouble(Lollipop.peak_width_base) / 2;
            double upper_limit = base_relation.local_peak_deltaM + Convert.ToDouble(Lollipop.peak_width_base) / 2;
            int current_lower = i - 1;
            int current_upper = i;
            List<ProteoformRelation> nearby_relations = new List<ProteoformRelation>();
            Parallel.Invoke(
                () => { while (relations[current_lower].delta_mass >= lower_limit) nearby_relations.Add(relations[current_lower]); current_lower -= 1; },
                () => { while (relations[current_upper].delta_mass <= lower_limit) nearby_relations.Add(relations[current_upper]); current_upper += 1; }
            );
            return nearby_relations;
        }

        private void compute_and_set_et_threshold()
        {
            Parallel.ForEach<ProteoformRelation>(et_relations, relation =>
            {
                double lower_limit = relation.local_peak_deltaM - Convert.ToDouble(Lollipop.peak_width_base) / 2;
                double upper_limit = relation.local_peak_deltaM + Convert.ToDouble(Lollipop.peak_width_base) / 2;
                List<int> nearby_decoy_counts = new List<int>();
                Parallel.ForEach<KeyValuePair<string, List<ProteoformRelation>>>(this.ed_relations, decoy_relation =>
                {
                    nearby_decoy_counts.Add(decoy_relation.Value.Where(r => r.delta_mass >= lower_limit && r.delta_mass <= upper_limit).ToList().Count);
                });
                relation.calculate_fdr(nearby_decoy_counts);
            });
        }

        private void compute_and_set_ee_threshold()
        {
            Parallel.ForEach<ProteoformRelation>(et_relations, relation =>
            {
                double lower_limit = relation.local_peak_deltaM - Convert.ToDouble(Lollipop.peak_width_base) / 2;
                double upper_limit = relation.local_peak_deltaM + Convert.ToDouble(Lollipop.peak_width_base) / 2;
                int local_false_peak_count = unequal_lysine_relations.Where(r => r.delta_mass >= lower_limit && r.delta_mass <= upper_limit).ToList().Count;
                relation.calculate_fdr(local_false_peak_count);
            });
        }

        //CONSTRUCTING FAMILIES
        public void construct_families()
        {

        }
        
        private void determine_adjacencies()
        {

        }
    }

    public class ProteoformFamily
    {
        public int lysine_count;


    }
}
