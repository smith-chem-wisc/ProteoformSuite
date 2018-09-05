using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoWPFSuite
{
    public interface IParentMDI
    {
        HashSet<String> MDIChildren { get;set; }
        void CloseEvent(ITabbedMDI sender, EventArgs e);
    }
}
