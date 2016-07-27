using System;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public partial class ResultsSummary : Form
    {

        BindingList<string> deconResultsFileNames = new BindingList<string>();
        int numRawExpComponents;
        int numNeucodePairs;
        int numExperimentalProteoforms;
        string uniprotXmlFile;
        int numETPairs;
        int numETPeaks;
        int numEEPairs;
        int numEEPeaks;
        int numTheoreticalProteoforms;
      //  string loadAndRunMethod;
        

        public ResultsSummary()
        {
            InitializeComponent();
        }

        private void ResultsSummary_Load(object sender, EventArgs e)
        {
            createResultsSummary();
            displayResultsSummary();

        }

        private void createResultsSummary()
        {
            deconResultsFileNames = Lollipop.deconResultsFileNames;
            numRawExpComponents = Lollipop.raw_experimental_components.Count;
            numNeucodePairs = Lollipop.raw_neucode_pairs.Count;
            numExperimentalProteoforms = Lollipop.proteoform_community.experimental_proteoforms.Count;
            uniprotXmlFile = Lollipop.uniprot_xml_filepath;
            numETPairs = Lollipop.et_relations.Count;
            numETPeaks = Lollipop.et_peaks.Count;
            numEEPairs = Lollipop.ee_relations.Count;
            numEEPeaks = Lollipop.ee_peaks.Count;
            numTheoreticalProteoforms = Lollipop.proteoform_community.theoretical_proteoforms.Count;

        }

        private void displayResultsSummary()
        {
            lb_deconResults.DataSource = deconResultsFileNames;
            tb_RawExperimentalComponents.Text = numRawExpComponents.ToString();
            tb_neucodePairs.Text = numNeucodePairs.ToString();
            tb_experimentalProteoforms.Text = numExperimentalProteoforms.ToString();
            tb_uniprotXmlDatabase.Text = uniprotXmlFile;
            tb_ETPairs.Text = numETPairs.ToString();
            tb_ETPeaks.Text = numETPeaks.ToString();
            tb_EEPairs.Text = numEEPairs.ToString();
            tb_EEPeaks.Text = numEEPeaks.ToString();
            tb_theoreticalProteoforms.Text = numTheoreticalProteoforms.ToString();

        }
    }
}
