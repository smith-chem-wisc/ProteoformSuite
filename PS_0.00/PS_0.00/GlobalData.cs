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
        public static DataSet deconResultsFiles = new DataSet();
        public static DataTable rawExperimentalProteoforms = new DataTable();
        public static DataSet rawExperimentalChargeStateData = new DataSet();
        public static DataTable rawNeuCodePairs = new DataTable();
        public static DataTable acceptableNeuCodeLightProteoforms = new DataTable();
        public static DataTable aggregatedProteoforms = new DataTable();
        public static DataSet theoreticalAndDecoyDatabases = new DataSet();
        public static DataTable experimentTheoreticalPairs = new DataTable();
        public static DataTable experimentExperimentPairs = new DataTable();
        public static DataTable eePeakList = new DataTable();
        public static DataTable EE_Parent = new DataTable();
        public static DataSet PF = new DataSet(); 
        public static DataTable PF_Contents = new DataTable();
    }
}
