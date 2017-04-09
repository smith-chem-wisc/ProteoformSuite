using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProteoformSuiteInternal;
using System.Windows.Forms;


namespace ProteoformSuiteGUI
{
    public class RelationUtility
    {
        public RelationUtility()
        {
            PeakAcceptabilityChanged += Relation_PeakAcceptabilityChanged;
        }

        public void clear_lists(List<ProteoformComparison> comparisons)
        {
            foreach (ProteoformComparison comparison in comparisons)
            {
                foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.relationships.RemoveAll(r => r.relation_type == comparison);
                foreach (Proteoform p in Lollipop.proteoform_community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.relation_type == comparison);
                Lollipop.proteoform_community.relations_in_peaks.RemoveAll(r => r.relation_type == comparison);
                Lollipop.proteoform_community.delta_mass_peaks.RemoveAll(k => k.relation_type == comparison);
                foreach (Proteoform p in Lollipop.proteoform_community.decoy_proteoforms.Values.SelectMany(d => d)) p.relationships.RemoveAll(r => r.relation_type == comparison);
            }
        }

        public Tuple<string, string, string> updateFiguresOfMerit(List<DeltaMassPeak> peaks)
        {
            List<DeltaMassPeak> big_peaks = peaks.Where(p => p.peak_accepted).ToList();
            string max = (big_peaks.Count > 0) ? Math.Round(big_peaks.Max(p => p.peak_group_fdr), 3).ToString() : "";
            return new Tuple<string, string, string>(big_peaks.Select(p => p.grouped_relations.Count).Sum().ToString(), big_peaks.Count.ToString(), max);
        }

        public List<double> get_notch_masses(TextBox tb)
        {
            List<double> masses = new List<double>();
            try
            {
                string[] notch_masses = tb.Text.Split(';');
                if (notch_masses.Length == 0)
                {
                    MessageBox.Show("No notch masses entered.");
                    return null;
                }
                foreach (string mass in notch_masses)
                {
                    masses.Add(Convert.ToDouble(mass));
                }
                return masses;
            }
            catch
            {
                MessageBox.Show("Masses in incorrect format.");
                return null;
            }
        }

        public event PeakAcceptabilityChangedEventHandler PeakAcceptabilityChanged;

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

    public class PeakAcceptabilityChangedEventArgs : EventArgs
    {
        private bool _isPeakAcceptable;
        public bool IsPeakAcceptable
        {
            get
            {
                return this._isPeakAcceptable;
            }
        }

        private DeltaMassPeak _Peak;
        public DeltaMassPeak Peak
        {
            get
            {
                return this._Peak;
            }
        }

        public PeakAcceptabilityChangedEventArgs(bool IsPeakAcceptable, DeltaMassPeak Peak)
        {
            this._isPeakAcceptable = IsPeakAcceptable; //True if peak is acceptable
            this._Peak = Peak;
        }
    }

    public delegate void PeakAcceptabilityChangedEventHandler(object sender, PeakAcceptabilityChangedEventArgs e);
}