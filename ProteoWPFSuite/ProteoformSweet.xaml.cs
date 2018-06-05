using System;
using System.Data;
using System.Collections.Generic;
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
using Microsoft.Win32;

/*
 * - MDI Form replaced by user control
 * - MDI Parent replaced by the getparentwindow (private)
     */
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProteoformSweet : UserControl, IParentMDI
    {

        #region Public Fields
        
        #region MDI Iterface
        public HashSet<String> MDIChildren
        {
            get;
            set;
        }
        public void CloseEvent(ITabbedMDI sender, EventArgs e)
        {
            MDIChildren.Remove(sender.UniqueTabName);

        }
        #endregion

        public LoadResults loadResults = new LoadResults();
        //public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();

        public List<ISweetForm> forms = new List<ISweetForm>();

        #endregion

        #region Private Fields
        ISweetForm current_Tab;
        SaveFileDialog saveExcelDialog;
        #endregion

        #region Private Methods
        /// <summary>
        /// Used for locating parent window; as a equicalent for midParent
        /// </summary>
        /// <param name="child">the control needing to find its parent</param>
        /// <returns>the parent winow if any; or null if there isn't any</returns>
        private static Window getParentWindow(DependencyObject child)
        {

            if (child == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while( (parent != null) && ((parent as Window) == null))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent == null) return null;
            else
            {
                return (Window)parent;
            }
        }
        #endregion

        public ProteoformSweet()
        {
            InitializeComponent();
        }

        
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
            ExcelWriter writer = new ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as Window).Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as Window).Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SaveExcelFile(writer, getParentWindow(current_Tab as UserControl).Name + "_table.xlsx"); //get the window hosting tabcontrol, which hosts usercontrol
        }
        private void SaveExcelFile(ExcelWriter writer, string filename)
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
    }
}
