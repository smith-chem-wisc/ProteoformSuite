using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_0._00
{
    public partial class ResultsSummary : Form
    {
        BindingList<string> deconResultsFileNames = new BindingList<string>();
         int numRawExperimentalComponents;
         int numNeucodePairs;
         int numAggProteoforms;
        string uniprotXmlFile;
         int numETpairs;
         int numETpeaks;
         int numEEpairs;
         int numEEpeaks;
         List<int> numDDpairs = new List<int>();
        //int numProteoformFamilies

        public ResultsSummary()
        {
            InitializeComponent();
            
        }

        public void ResultsSummary_Load(object sender, EventArgs e)
        {
            createResultsSummary();
            displayResultsSummary();
        }


        private void createResultsSummary()
        {
            deconResultsFileNames = GlobalData.deconResultsFileNames;
            numRawExperimentalComponents = GlobalData.rawExperimentalComponents.Rows.Count;
            numNeucodePairs = GlobalData.rawNeuCodePairs.Rows.Count;
            numAggProteoforms = GlobalData.aggregatedProteoforms.Rows.Count;
            uniprotXmlFile = GlobalData.uniprotXmlFile;
            numETpairs = GlobalData.experimentTheoreticalPairs.Rows.Count;
            numETpeaks = GlobalData.etPeakList.Rows.Count;
            numEEpairs = GlobalData.experimentExperimentPairs.Rows.Count;
            numEEpeaks = GlobalData.eePeakList.Rows.Count;
            for (int i = 0; i < GlobalData.decoyDecoyPairs.Tables.Count; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                int ddPairs = GlobalData.decoyDecoyPairs.Tables[tableName].Rows.Count;
                numDDpairs.Add(ddPairs);
            }
        }
            

        private void displayResultsSummary()
        {
            lb_deconResults.DataSource = deconResultsFileNames;
            tb_RawExperimentalComponents.Text = numRawExperimentalComponents.ToString();
            tb_neucodePairs.Text = numNeucodePairs.ToString();
            tb_aggregatedProteoforms.Text = numAggProteoforms.ToString();
            tb_uniprotXmlDatabase.Text = uniprotXmlFile;
            tb_ETPairs.Text = numETpairs.ToString();
            tb_ETPeaks.Text = numETpeaks.ToString();
            tb_EEPairs.Text = numEEpairs.ToString();
            tb_EEPeaks.Text = numEEpeaks.ToString();
            lb_ddPairs.DataSource = numDDpairs;


        }
    }
}
