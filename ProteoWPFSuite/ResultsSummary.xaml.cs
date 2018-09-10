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
using ProteoformSuiteInternal;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for ResultsSummary.xaml
    /// </summary>
    /// <TODO>
    /// Implement all methods
    /// </TODO>
    public partial class ResultsSummary : UserControl,ISweetForm
    {
        public ResultsSummary()
        {
            InitializeComponent();
        }

        public void create_summary()
        { }
        public IGoAnalysis get_go_analysis()
        {
            return null;
            //return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as IGoAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as IGoAnalysis : Sweet.lollipop.Log2FoldChangeAnalysis as IGoAnalysis;
        }

        public TusherAnalysis get_tusher_analysis()
        {
            return null;
            //return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as TusherAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as TusherAnalysis : null;
        }
        public List<DataTable> DataTables => throw new NotImplementedException();

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
    }
}
