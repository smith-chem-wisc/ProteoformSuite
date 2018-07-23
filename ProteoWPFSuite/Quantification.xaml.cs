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
    /// Interaction logic for Quantification.xaml
    /// </summary>
    public partial class Quantification : UserControl,ISweetForm
    {
        public Quantification()
        {
            InitializeComponent();
        }
        public void InitializeConditionsParameters()
        {

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
