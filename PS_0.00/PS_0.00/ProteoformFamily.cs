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
