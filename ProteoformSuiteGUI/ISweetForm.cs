using System.Collections.Generic;
using System.Data;

namespace ProteoformSuiteGUI
{
    /// <summary>
    /// Each form in this program should perform several processes consistently.
    /// Namely, a button click ensures the user wants to start processing (no processing is performed upon form load)
    /// </summary>
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