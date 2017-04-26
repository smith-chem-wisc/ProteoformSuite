using System.Windows.Forms;
using System.Collections.Generic;


namespace ProteoformSuiteGUI
{
    interface ISweetForm
    {
        void ClearListsTablesFigures();
        void RunTheGamut();
        bool ReadyToRunTheGamut();
        void InitializeParameterSet();
        void FillTablesAndCharts();
        List<DataGridView> GetDGVs();
    }
}
