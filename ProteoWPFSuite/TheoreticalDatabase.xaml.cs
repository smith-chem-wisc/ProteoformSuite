using System;
using System.Collections.Generic;
using System.Data;
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

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for TheoreticalDatabase.xaml
    /// </summary>
    public partial class TheoreticalDatabase : UserControl,ISweetForm,ITabbedMDI
    {
        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        public List<DataTable> DataTables => throw new NotImplementedException();

        public string UniqueTabName => throw new NotImplementedException();

        public ProteoformSweet MDIParent { get; set; }

        public event delClosed BeingClosed;

        public void ClearListsTablesFigures(bool clear_following_forms)
        {
            throw new NotImplementedException();
        }

        public void FillTablesAndCharts()
        {
            throw new NotImplementedException();
        }

        public void InitializeParameterSet()
        {
            throw new NotImplementedException();
        }

        public void OnClosing(ITabbedMDI sender)
        {
            throw new NotImplementedException();
        }

        public bool ReadyToRunTheGamut()
        {
            throw new NotImplementedException();
        }

        public void RunTheGamut(bool full_run)
        {
            throw new NotImplementedException();
        }

        public List<DataTable> SetTables()
        {
            throw new NotImplementedException();
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmb_empty_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void dgv_loadFiles_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {

        }

        private void dgv_loadFiles_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {

        }

        private void btn_addFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_downloadUniProtPtmList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_clearFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_NaturalIsotopes_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void btn_NeuCode_Lt_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void btn_NeuCode_Hv_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ckbx_Carbam_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ckbx_OxidMeth_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ckbx_Meth_Cleaved_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void nud_randomSeed_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nUD_NumDecoyDBs_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cb_useRandomSeed_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void nUD_MinPeptideLength_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ckbx_combineIdenticalSequences_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void nUD_MaxPTMs_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cb_limitLargePtmSets_CheckedChanged(object sender, RoutedEventArgs e)
        {

        }

        private void tb_modTypesToExclude_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tb_modTypesToExclude_TextChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
