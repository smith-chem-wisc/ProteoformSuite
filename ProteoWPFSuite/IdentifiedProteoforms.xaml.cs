﻿ using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProteoformSuiteInternal;


namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for IdentifiedProteoforms.xaml
    /// </summary>
    public partial class IdentifiedProteoforms : UserControl,ISweetForm, ITabbedMDI
    {
        public IdentifiedProteoforms()
        {
            InitializeComponent();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            FillTablesAndCharts();
        }


        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_identified_experimentals.DataSource = null;
            dgv_identified_experimentals.Rows.Clear();
            dgv_td_proteoforms.DataSource = null;
            dgv_td_proteoforms.Rows.Clear();
        }

        public void InitializeParameterSet()
        {
            //not applicable
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_identified_experimentals, Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_identified_experimentals);
            DisplayUtility.FillDataGridView(dgv_td_proteoforms, Sweet.lollipop.topdown_proteoforms.Select(e => new DisplayTopDownProteoform(e as TopDownProteoform)));
            DisplayTopDownProteoform.FormatTopDownTable(dgv_td_proteoforms, true);
            tb_not_td.Text = "Identified Experimental Proteoforms Not in Top-Down";
            tb_topdown.Text = "Top-Down Proteoforms";
            tb_bottomup.Text = "Bottom-Up Peptides from Selected Proteoform";
        }

        public List<DataTable> DataTables { get; private set; }
        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayTopDownProteoform.FormatTopDownTable( Sweet.lollipop.topdown_proteoforms.Select(e => new DisplayTopDownProteoform(e as TopDownProteoform)).ToList(), "TopdownProteoforms", true),
                DisplayExperimentalProteoform.FormatAggregatesTable(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(e => new DisplayExperimentalProteoform(e)).ToList(), "IdentifiedExperimentals")
            };
            return DataTables;
        }
               
        
        public ProteoformSweet MDIParent { get; set; }

        private void tb_tableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            IEnumerable<object> filter_experimentals = tb_tableFilter.Text == "" ?
                Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms.Where(p => !p.topdown_id && p.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !p.adduct))).Select(ep => new DisplayExperimentalProteoform(ep)) :
                ExtensionMethods.filter(Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms.Where(p => !p.topdown_id && p.linked_proteoform_references != null)).Select(ep => new DisplayExperimentalProteoform(ep)), tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_identified_experimentals, filter_experimentals);
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_identified_experimentals);

            IEnumerable<object> filter_topdown = tb_tableFilter.Text == "" ?
                Sweet.lollipop.topdown_proteoforms.Select(p => new DisplayTopDownProteoform(p as TopDownProteoform)) :
                ExtensionMethods.filter(Sweet.lollipop.topdown_proteoforms.Select(p => new DisplayTopDownProteoform(p as TopDownProteoform)), tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_td_proteoforms, filter_topdown);
            DisplayTopDownProteoform.FormatTopDownTable(dgv_td_proteoforms, true);
        }

        private void btn_compare_with_td_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select a top-down results file.");
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Title = "Top-Down Results";
            openFileDialog.Filter = "Excel Files (*.xlsx) | *.xlsx";
            openFileDialog.Multiselect = false;
            System.Windows.Forms.DialogResult dr = openFileDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show("Save comparison results file.");
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Title = "Top-Down Comparison Results";
                saveFileDialog.Filter = "Text Files (*.tsv) | *.tsv";
                System.Windows.Forms.DialogResult sdr = saveFileDialog.ShowDialog();
                if (sdr == System.Windows.Forms.DialogResult.OK)
                {
                    InputFile file = new InputFile(openFileDialog.FileName, Purpose.TopDown);
                    TDBUReader reader = new TDBUReader();
                    List<SpectrumMatch> hits = reader.ReadTDFile(file);
                    List<TopDownProteoform> td_proteoforms = Sweet.lollipop.aggregate_td_hits(hits, 0, true, true);
                    List<ExperimentalProteoform> experimentals = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(p => p.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !p.adduct) && !p.topdown_id).ToList();
                    experimentals = Sweet.lollipop.add_topdown_proteoforms(experimentals, td_proteoforms);
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("Experimental Accession\tExperimental Mass\tExperimental Retention Time\tTheoretical Accession\tTheoretical Description\tTheoretical Begin\tTheoretical End\tTheoretical PTM Description\tTop-Down Accession\tTop-Down Begin\tTop-Down End\tTop-Down PTM Description\tTop-Down Observed Mass\tTop-Down Retention Time\tTop Top-Down C-Score");
                        foreach (ExperimentalProteoform ep in experimentals)
                        {
                            if (ep.topdown_id)
                            {
                                TopDownProteoform tdp = ep as TopDownProteoform;
                                if (tdp.matching_experimental != null)
                                {
                                    ExperimentalProteoform exp = tdp.matching_experimental;
                                    string exp_ptm = exp.ptm_set.ptm_combination.Count == 0 ? "Unmodified" : String.Join(", ", exp.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.OriginalId).OrderBy(p => p));
                                    string td_ptm = tdp.topdown_ptm_set.ptm_combination.Count == 0 ? "Unmodified" : String.Join(", ", tdp.topdown_ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.OriginalId).OrderBy(p => p));
                                    writer.WriteLine(exp.accession + "\t" + exp.agg_mass + "\t" + exp.agg_rt + "\t" + exp.linked_proteoform_references.First().accession.Split('_')[0] + "\t" + (exp.linked_proteoform_references.First() as TheoreticalProteoform).description + "\t" + exp.begin + "\t" + exp.end + "\t" + exp_ptm
                                         + "\t" + tdp.accession.Split('_')[0] + "\t" + tdp.topdown_begin + "\t" + tdp.topdown_end + "\t" + td_ptm + "\t" + tdp.modified_mass + "\t" + tdp.agg_rt + "\t" + tdp.topdown_hits.Max(h => h.score));
                                }
                            }
                            else
                            {
                                string exp_ptm = ep.ptm_set.ptm_combination.Count == 0 ? "Unmodified" : String.Join(", ", ep.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.OriginalId).OrderBy(p => p));
                                TheoreticalProteoform t = ep.linked_proteoform_references.First() as TheoreticalProteoform;
                                writer.WriteLine(ep.accession + "\t" + ep.agg_mass + "\t" + ep.agg_rt + "\t" + t.accession.Split('_')[0] + "\t" + t.description + "\t" + t.begin + "\t" + t.end + "\t" + exp_ptm
                                     + "\t" + "N\\A" + "\t" + "N\\A" + "\t" + "N\\A" + "\t" + "N\\A" + "\t" + "N\\A" + "\t" + "N\\A" + "\t" + "N\\A");
                            }
                        }
                    }
                    MessageBox.Show("Successfully saved top-down comparison results.");
                }
                else return;
            }
            else return;
        }

        private ExperimentalProteoform selected_pf;

        private void dgv_identified_experimentals_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selected_pf = (ExperimentalProteoform)((DisplayExperimentalProteoform)this.dgv_identified_experimentals.Rows[e.RowIndex].DataBoundItem).display_object;
            }
            else
            {
                selected_pf = null;
            }
            display_bu_peptides();
        }

        private void dgv_td_proteoforms_experimentals_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selected_pf = (ExperimentalProteoform)((DisplayTopDownProteoform)this.dgv_td_proteoforms.Rows[e.RowIndex].DataBoundItem).display_object;
            }
            else
            {
                selected_pf = null;
            }
            display_bu_peptides();
        }


        private void display_bu_peptides()
        {
            List<SpectrumMatch> bu_psms = selected_pf == null ? new List<SpectrumMatch>() :
                (selected_pf as TopDownProteoform) != null ? (selected_pf as TopDownProteoform).topdown_bottom_up_PSMs
                .Concat((selected_pf as TopDownProteoform).ambiguous_topdown_hits.SelectMany(p => p.bottom_up_PSMs).Distinct()).ToList()
                : selected_pf.bottom_up_PSMs.Concat(selected_pf.ambiguous_identifications.SelectMany(p => p.bottom_up_PSMs)).Distinct().ToList();
            DisplayUtility.FillDataGridView(dgv_bottomUp, bu_psms.Select(c => new DisplayTopDownHit(c)));
            DisplayTopDownHit.FormatTopDownHitsTable(dgv_bottomUp, true);
        }
    }
}
