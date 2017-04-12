using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class RelationUtility
    {
        public event PeakAcceptabilityChangedEventHandler PeakAcceptabilityChanged;

        public delegate void PeakAcceptabilityChangedEventHandler(object sender, PeakAcceptabilityChangedEventArgs e);

        public RelationUtility()
        {
            PeakAcceptabilityChanged += Relation_PeakAcceptabilityChanged;
        }

        public Tuple<string, string, string> updateFiguresOfMerit(List<DeltaMassPeak> peaks)
        {
            List<DeltaMassPeak> big_peaks = peaks.Where(p => p.peak_accepted).ToList();
            string max = (big_peaks.Count > 0) ? Math.Round(big_peaks.Max(p => p.peak_group_fdr), 3).ToString() : "";
            return new Tuple<string, string, string>(big_peaks.Select(p => p.grouped_relations.Count).Sum().ToString(), big_peaks.Count.ToString(), max);
        }

        public void peak_acceptability_change(DataGridView dgv)
        {
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);

                int columnIndex = dgv.CurrentCell.ColumnIndex;
                int rowIndex = dgv.CurrentCell.RowIndex;

                if (columnIndex < 0) return;
                string columnName = dgv.Columns[columnIndex].Name;

                if (columnName == "peak_accepted")
                {
                    bool acceptibilityStatus = Convert.ToBoolean(dgv.Rows[rowIndex].Cells[columnIndex].Value);
                    DeltaMassPeak selected_peak = (DeltaMassPeak)dgv.Rows[rowIndex].DataBoundItem;
                    PeakAcceptabilityChangedEventArgs AcceptabilityChangedEventData = new PeakAcceptabilityChangedEventArgs(acceptibilityStatus, selected_peak);
                    ONEAcceptibilityChanged(AcceptabilityChangedEventData);
                }
            }
        }

        protected virtual void ONEAcceptibilityChanged(PeakAcceptabilityChangedEventArgs e)
        {
            if (PeakAcceptabilityChanged != null) Relation_PeakAcceptabilityChanged(this, e);
        }

        protected void Relation_PeakAcceptabilityChanged(object sender, PeakAcceptabilityChangedEventArgs e)
        {
            Parallel.ForEach(Lollipop.et_relations.Where(p => e.Peak.grouped_relations.Contains(p)), pRelation => pRelation.accepted = e.IsPeakAcceptable);
            Parallel.ForEach(Lollipop.ee_relations.Where(p => e.Peak.grouped_relations.Contains(p)), pRelation => pRelation.accepted = e.IsPeakAcceptable);
        }
    }
}