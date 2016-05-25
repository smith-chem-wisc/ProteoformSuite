using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{


    public class ProteoformFamily
    {
        public int lysine_count;
        public List<Proteoform> proteoforms
        {
            get { return this.proteoforms; }
            set
            {
                HashSet<int> lysine_counts = new HashSet<int>(value.Select(p => p.lysine_count));
                if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
                else this.lysine_count = -1;
            }
        }

        public ProteoformFamily(List<Proteoform> proteoforms)
        {
            this.proteoforms = proteoforms;
        }
    }
}
