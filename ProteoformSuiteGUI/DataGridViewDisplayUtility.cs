using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public class DataGridViewDisplayUtility
    {

        public static void FillDataGridView(DataGridView dgv, IEnumerable<object> someList)
        {
            SortableBindingList<object> sbl = new SortableBindingList<object>(someList);
            dgv.DataSource = sbl;
            dgv.ReadOnly = true;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        public static void sortDataGridViewColumn(DataGridView dgv, int columnIndex)
        {
            DataGridViewColumn newColumn = dgv.Columns[columnIndex];
            DataGridViewColumn oldColumn = dgv.SortedColumn;
            ListSortDirection direction;

            // If oldColumn is null, then the DataGridView is not sorted.
            if (oldColumn != null)
            {
                if (oldColumn == newColumn)
                {
                    if (dgv.SortOrder == SortOrder.Ascending)                    
                        direction = ListSortDirection.Descending;                   
                    else
                        direction = ListSortDirection.Ascending;

                }
                else
                {
                    direction = ListSortDirection.Descending;
                    oldColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                }

            }
            else
            {
                direction = ListSortDirection.Descending;
            }

            // Sort the selected column.
            dgv.Sort(newColumn, direction);
            newColumn.HeaderCell.SortGlyphDirection =
                direction == ListSortDirection.Ascending ?
                SortOrder.Ascending : SortOrder.Descending;
        }

    }
}
