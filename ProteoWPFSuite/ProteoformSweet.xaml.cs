using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ProteoformSuiteInternal;
/*
 * - MDI Form replaced by user control
 * - MDI Parent replaced by the getparentwindow (private)
     */
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProteoformSweet : UserControl
    {

        #region Public Fields
        
        //MDI Iterface
        public HashSet<String> MDIChildren
        {
            get;
            set;
        }
        
        //original
        public LoadResults loadResults = new LoadResults();
        public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        public NeuCodePairs neuCodePairs = new NeuCodePairs();
        public AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        public TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        public ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        public ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        public ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        public Quantification quantification = new Quantification();
        public TopDown topDown = new TopDown();
        public IdentifiedProteoforms identifiedProteoforms = new IdentifiedProteoforms();
        public ResultsSummary resultsSummary = new ResultsSummary();
        public List<ISweetForm> forms = new List<ISweetForm>();
        #endregion

        #region Private Fields
        System.Windows.Forms.FolderBrowserDialog resultsFolderOpen = new System.Windows.Forms.FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        OpenFileDialog openResults = new OpenFileDialog();
        SaveFileDialog saveResults = new SaveFileDialog();
        ISweetForm current_Tab;
        SaveFileDialog saveExcelDialog;
        #endregion

        #region Public Constructor

        public ProteoformSweet()
        {
            InitializeComponent();/*
            InitializeForms();
            WindowState = FormWindowState.Maximized;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            loadResults.InitializeParameterSet();
            showForm(loadResults);
            methodFileOpen.Filter = "Method XML File (*.xml)| *.xml";
            methodFileSave.DefaultExt = ".xml";
            methodFileSave.Filter = "Method XML File (*.xml)| *.xml";
            saveExcelDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveExcelDialog.DefaultExt = ".xlsx";
            openResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.DefaultExt = ".sweet";
            loadResults.Focus();
            Form.ActiveForm.Show();
            LoadResults.ActiveForm.Show();*/

        }

        #endregion Public Constructor

        #region Private Setup Methods
        private void initializeForms()
        {
            forms = new List<ISweetForm>
            {
                loadResults,
                theoreticalDatabase,

            };
            foreach(UserControl uc in forms)
            {
                (uc as ITabbedMDI).MDIParent = this; //set the mdi parent
            }
        }
        #endregion

        


















        
        private void ExportTables_Click(object sender, RoutedEventArgs e)
        {
            List<DataTable> data_tables = current_Tab.SetTables();

            if (data_tables == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            writer.ExportToExcel(data_tables, (current_Tab as UserControl).Name);
            SaveExcelFile(writer, (current_Tab as UserControl).Name +"_table.xlsx");
        }

        /**
         * MDI Missing in WPF; Used Tabbed MDI instead; See README.md
         **/
        private void exportAllTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as Window).Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as Window).Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SaveExcelFile(writer, MDIHelpers.getParentWindow(current_Tab as UserControl).Name + "_table.xlsx"); //get the window hosting tabcontrol, which hosts usercontrol
        }
        private void SaveExcelFile(ProteoformSuiteGUI.ExcelWriter writer, string filename)
        {
            saveExcelDialog.FileName = filename;
            
            if (saveExcelDialog.ShowDialog()==true)
            {
                MessageBox.Show(writer.SaveToExcel(saveExcelDialog.FileName));
            }
            else return;
        }

        private void printToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void testWin(object sender, RoutedEventArgs e)
        {
            testWin curr = new testWin();
            curr.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Enum.GetValues(typeof(MassSpectrometry.DissociationType)))
            {
                MessageBox.Show(item.ToString());
                
            }
        }
    }
}
