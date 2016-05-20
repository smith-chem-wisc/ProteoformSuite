using System;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;// needed for BindingSource


namespace PS_0._00
{
    public class GlobalData
    {
        public static BindingList<string> deconResultsFileNames = new BindingList<string>();
        public static List<DataTable> deconResultsFiles = new List<DataTable>();
        public static BindingList<Component> rawExperimentalComponents = new BindingList<Component>();
        public static List<Proteoform> rawNeuCodePairs = new List<Proteoform>();
        public static List<AggregatedProteoform> aggregatedProteoforms = new List<AggregatedProteoform>();
        public static DataSet theoreticalAndDecoyDatabases = new DataSet();
        public static int numDecoyDatabases;
        public static DataTable experimentTheoreticalPairs = new DataTable();
        public static DataTable etPeakList = new DataTable();
        public static DataSet experimentDecoyPairs = new DataSet();
        public static DataTable edList = new DataTable();
        public static DataTable experimentExperimentPairs = new DataTable();
        public static DataTable eePeakList = new DataTable();
        public static DataTable EE_Parent = new DataTable();
        public static DataSet ProteoformFamiliesET = new DataSet();
        public static DataSet ProteoformFamiliesEE = new DataSet();
        public static DataTable ProteoformFamilyMetrics = new DataTable();
        public static bool repeat = false;
        public static object repeatsender;
        public static EventArgs repeate;
    }
}
