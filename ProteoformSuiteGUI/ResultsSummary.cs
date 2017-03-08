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

        BindingList<string> identificationFileNames = new BindingList<string>();
        int numRawExpComponents;
        int numNeucodePairs;
        int numExperimentalProteoforms;
        string uniprotXmlFiles;
        int numETPairs;
        int numETPeaks;
        int numEEPairs;
        int numEEPeaks;
        int numTheoreticalProteoforms;
        int numFamilies;
        public static string loadDescription = null; //set in GUI
        

        public ResultsSummary()
        {
            InitializeComponent();
        }

        private void ResultsSummary_Load(object sender, EventArgs e)
        {
            createResultsSummary();
            displayResultsSummary();

        }

        public void createResultsSummary()
        {
            identificationFileNames = new BindingList<string>((from s in Lollipop.input_files.Where(f => f.purpose == Purpose.Identification) select s.filename).ToList());
            numRawExpComponents = Lollipop.raw_experimental_components.Count;
            numNeucodePairs = Lollipop.raw_neucode_pairs.Where(c => c.accepted).Count();
            numExperimentalProteoforms = Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).Count();
            uniprotXmlFiles = String.Join(", ", Lollipop.get_files(Lollipop.input_files, Purpose.ProteinDatabase).Select(f => f.filename));
            numETPairs = Lollipop.et_relations.Count;
            numETPeaks = Lollipop.et_peaks.Count;
            numEEPairs = Lollipop.ee_relations.Count;
            numEEPeaks = Lollipop.ee_peaks.Count;
            numTheoreticalProteoforms = Lollipop.proteoform_community.theoretical_proteoforms.Count();
            numFamilies = Lollipop.proteoform_community.families.Count;
        }

        public void displayResultsSummary()
        {
            lb_deconResults.DataSource = identificationFileNames; 
            tb_RawExperimentalComponents.Text = numRawExpComponents.ToString();
            tb_neucodePairs.Text = numNeucodePairs.ToString();
            tb_experimentalProteoforms.Text = numExperimentalProteoforms.ToString();
            tb_uniprotXmlDatabase.Text = uniprotXmlFiles;
            tb_ETPairs.Text = numETPairs.ToString();
            tb_ETPeaks.Text = numETPeaks.ToString();
            tb_EEPairs.Text = numEEPairs.ToString();
            tb_EEPeaks.Text = numEEPeaks.ToString();
            tb_theoreticalProteoforms.Text = numTheoreticalProteoforms.ToString();
            tb_families.Text = numFamilies.ToString();
            if (loadDescription == null) { tb_load_description.Visible = false; label11.Visible = false; }
            else { tb_load_description.Text = loadDescription.ToString() ; }
        }
    }
}
