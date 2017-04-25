using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class RelationUtility
    {

        #region Public Fields

        public event PeakAcceptabilityChangedEventHandler PeakAcceptabilityChanged;

        public delegate void PeakAcceptabilityChangedEventHandler(object sender, PeakAcceptabilityChangedEventArgs e);

        #endregion Public Fields

        #region Public Constructor

        public RelationUtility()
        {
            PeakAcceptabilityChanged += Relation_PeakAcceptabilityChanged;
        }

        #endregion Public Constructor

        #region Public Methods

        public void updateFiguresOfMerit(List<DeltaMassPeak> peaks, TextBox tb_accepted_relations, TextBox tb_total_peaks, TextBox tb_max_fdr)
        {
            List<DeltaMassPeak> big_peaks = peaks.Where(p => p.Accepted).ToList();
            string max_fdr = (big_peaks.Count > 0) ? 
                Math.Round(big_peaks.Max(p => p.peak_group_fdr), 3).ToString() : 
                "";
            tb_max_fdr.Text = max_fdr;
            tb_accepted_relations.Text = big_peaks.Sum(p => p.grouped_relations.Count(r => r.Accepted)).ToString();
            tb_total_peaks.Text = big_peaks.Count.ToString();
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

        #endregion Public Methods

        #region Protected Methods

        protected virtual void ONEAcceptibilityChanged(PeakAcceptabilityChangedEventArgs e)
        {
            if (PeakAcceptabilityChanged != null) Relation_PeakAcceptabilityChanged(this, e);
        }

        protected void Relation_PeakAcceptabilityChanged(object sender, PeakAcceptabilityChangedEventArgs e)
        {
            Parallel.ForEach(SaveState.lollipop.et_relations.Where(p => e.Peak.grouped_relations.Contains(p)), pRelation => pRelation.Accepted = e.IsPeakAcceptable);
            Parallel.ForEach(SaveState.lollipop.ee_relations.Where(p => e.Peak.grouped_relations.Contains(p)), pRelation => pRelation.Accepted = e.IsPeakAcceptable);
        }

        #endregion Protected Methods

    }
}