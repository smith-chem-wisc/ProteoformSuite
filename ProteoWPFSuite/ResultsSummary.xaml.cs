using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using ProteoformSuiteInternal;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for ResultsSummary.xaml
    /// </summary>
    /// <TODO>
    /// Implement all methods
    /// </TODO>
    public partial class ResultsSummary : UserControl,ISweetForm, ITabbedMDI
    {
        #region Public Constructor

        public ResultsSummary()
        {
            InitializeComponent();
            InitializeParameterSet();
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        public ProteoformSweet MDIParent { get; set; }

        #endregion Public Property

        #region Public Methods

        public List<DataTable> SetTables()
        {
            return null;
        }

        public void create_summary()
        {
            rtb_summary.Text = ResultsSummaryGenerator.generate_full_report();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            create_summary();
        }

        public void InitializeParameterSet()
        {
            tb_summarySaveFolder.Text = Sweet.lollipop.results_folder;
            cmbx_analysis.Items.Clear();

            //String[] cmbx_analysis_content = new string[] {
            //    "Tusher Analysis (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
            //    "Tusher Analysis (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
            //    "Log2 Fold Change Analysis (" + Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr.ToString() + " FDR)"
            //};

            // add cmbx_analysis_content to the combobox
            // cmbx_analysis_content.ToList().ForEach(item => cmbx_analysis.Items.Add(item));
            cmbx_analysis.Items.Add("Tusher Analysis (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)");
            cmbx_analysis.Items.Add("Tusher Analysis (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)");
            cmbx_analysis.Items.Add("Log2 Fold Change Analysis (" + Sweet.lollipop.benjiHoch_fdr.ToString() + " FDR)");

            cmbx_analysis.SelectedIndex = 1;
        }

        public void ClearListsTablesFigures(bool clear_following_forms)
        {
            rtb_summary.Text = "";
            tb_summarySaveFolder.Text = "";
        }

        public void FillTablesAndCharts()
        {
            create_summary();
        }

        public IGoAnalysis get_go_analysis()
        {
            //return null;
            return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as IGoAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as IGoAnalysis : Sweet.lollipop.Log2FoldChangeAnalysis as IGoAnalysis;
        }

        public TusherAnalysis get_tusher_analysis()
        {
            //return null;
            return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as TusherAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as TusherAnalysis : null;
        }

        #endregion Public Methods

        #region Private Fields

        private System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

        #endregion Private Fields

        #region Private Methods

        private void btn_browseSummarySaveFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_summarySaveFolder.Text = temp_folder_path;
                Sweet.lollipop.results_folder = temp_folder_path;
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Sweet.lollipop.results_folder)) return;
            string timestamp = Sweet.time_stamp();
            ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp, get_go_analysis(), get_tusher_analysis());
            MDIParent.save_all_plots(Sweet.lollipop.results_folder, timestamp);
            using (StreamWriter file = new StreamWriter(System.IO.Path.Combine(Sweet.lollipop.results_folder, "presets_" + timestamp + ".xml")))
                file.WriteLine(Sweet.save_method());
        }

        private void Cmbx_analysis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We do this to prevent firing TabControl's SelectionChanged event unintendedly
            // Reference: https://stackoverflow.com/questions/3659858/in-c-sharp-wpf-why-is-my-tabcontrols-selectionchanged-event-firing-too-often
            e.Handled = true;
        }

        #endregion Private Methods


    }
}
