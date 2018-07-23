using System.Data;
using System.Collections.Generic;

namespace ProteoWPFSuite
{
    public interface ISweetForm
    {
        bool ReadyToRunTheGamut();

        /// <summary>
        /// Each process(RunTheGamut) has three steps:
        ///    a. Clear lists, tables and figures
        ///    b. Data processing
        ///    c. Fill the tables figures
        /// </summary>
        void RunTheGamut(bool full_run);

        void ClearListsTablesFigures(bool clear_following_forms);

        void InitializeParameterSet();

        void FillTablesAndCharts();

        List<DataTable> DataTables { get; }
        List<DataTable> SetTables();
    }
}
