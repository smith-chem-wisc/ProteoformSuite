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
        public List<ProteoformRelation> et_pairs = new List<ProteoformRelation>();
        public List<ProteoformRelation> ee_pairs = new List<ProteoformRelation>();
        public Dictionary<string, List<ProteoformRelation>> ed_pairs = new Dictionary<string, List<ProteoformRelation>>();
        public List<ProteoformRelation> unequal_lysine_pairs = new List<ProteoformRelation>();

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
                () => this.et_pairs = relate(experimental_proteoforms.ToArray(), theoretical_proteoforms.ToArray()),
                () => this.ee_pairs = relate(experimental_proteoforms.ToArray(), experimental_proteoforms.ToArray()),
                () => Parallel.ForEach<KeyValuePair<string, List<TheoreticalProteoform>>>(this.decoy_proteoforms, decoys => 
                {
                    ed_pairs[decoys.Key] = relate(experimental_proteoforms.ToArray(), decoys.Value.ToArray());
                }),
                () => relate(random_unequal_lysine_relations())
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



        //CONSTRUCTING FAMILIES
        public void determine_adjacencies()
        {

        }
    }

    public class ProteoformFamily
    {
        public int lysine_count;


    }
}
