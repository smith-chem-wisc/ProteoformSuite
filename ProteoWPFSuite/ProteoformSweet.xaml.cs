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
using ProteoformSuiteGUI;


namespace ProteoformWPFSuite
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProteoformSweet : UserControl
    {
        #region Private Fields
        ISweetForm current_form;
        SaveFileDialog saveExcelDialog;
        #endregion

        public ProteoformSweet()
        {
            InitializeComponent();
        }

        /**
         * !!!!!!!!!!!!!!Problem: MDI Application look may not be supported; TBA what the application looks
         **/
        private void ExportTables_Click(object sender, RoutedEventArgs e)
        {
            List<DataTable> data_tables = current_form.SetTables();

            if (data_tables == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            writer.ExportToExcel(data_tables, (current_form as Window).Name);
            SaveExcelFile(writer, (current_form as Window).Name +"_table.xlsx");
        }
        private void exportAllTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*ExcelWriter writer = new ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as Form).Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as Form).Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            SaveExcelFile(writer, (current_form as Form).MdiParent.Name + "_table.xlsx");*/
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
