using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;

namespace PS_0._00
{
    class DataTableHandler
    {
        public int RtDecimals { get; set; }
        public int IntensityDecimals { get; set; }
        public int AbundanceDecimals { get; set; }
        public int MassDecimals { get; set; }

        public DataTableHandler()
        {
            this.RtDecimals = 2;
            this.IntensityDecimals = 0;
            this.AbundanceDecimals = 4;
            this.MassDecimals = 4;
        }
        
        public BindingSource DisplayWithRoundedDoubles(DataGridView dataGridView, DataTable table, string[] rt_column_names, string[] intensity_column_names, string[] abundance_column_names, string[] mass_column_names)
        {
            DataTable displayTable = new DataTable();
            displayTable = table;
            foreach (string rt_column_name in rt_column_names) { RoundDoubleColumn(displayTable, rt_column_name, this.RtDecimals); }
            foreach (string intensity_column_name in intensity_column_names) { RoundDoubleColumn(displayTable, intensity_column_name, this.IntensityDecimals); }
            foreach (string abundance_column_name in abundance_column_names) { RoundDoubleColumn(displayTable, abundance_column_name, this.AbundanceDecimals); }
            foreach (string mass_column_name in mass_column_names) { RoundDoubleColumn(displayTable, mass_column_name, this.MassDecimals); }
            return this.Display(dataGridView, displayTable);
        }

        private static void RoundDoubleColumn(DataTable table, string column_name, int num_decimal_places)
        {
            table.AsEnumerable().ToList().ForEach(p => p.SetField<Double>(column_name, Math.Round(p.Field<Double>(column_name), num_decimal_places)));
        }

        public BindingSource Display(DataGridView dataGridView, DataTable displayTable)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = displayTable;

            dataGridView.DataSource = bs;
            dataGridView.AutoGenerateColumns = true;
            dataGridView.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;

            return bs;
        }
    }
}
