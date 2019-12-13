using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Proteomics;
using Proteomics.Fragmentation;
using MzLibUtil;
using Proteomics.ProteolyticDigestion;
using System.Threading.Tasks;


namespace ProteoformSuiteGUI
{
    public partial class ProteoformFamilies : Form, ISweetForm
    {
        #region Public Constructor

        public ProteoformFamilies()
        {
            InitializeComponent();

            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);
            cmbx_nodeLabel.Items.AddRange(Lollipop.node_labels);
            cmbx_geneLabel.Items.AddRange(Lollipop.gene_name_labels.ToArray());
            cmbx_tableSelector.Items.AddRange(table_names);

            cmbx_colorScheme.SelectedIndex = 1;
            cmbx_nodeLayout.SelectedIndex = 1;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;
            cmbx_edgeLabel.SelectedIndex = 1;
            cmbx_nodeLabel.SelectedIndex = 1;
            cmbx_geneLabel.SelectedIndex = 1;

            InitializeParameterSet();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public void initialize_every_time()
        {
            tb_familyBuildFolder.Text = Sweet.lollipop.family_build_folder_path;
            nud_decimalRoundingLabels.Value = Convert.ToDecimal(Sweet.lollipop.deltaM_edge_display_rounding);
            cb_buildAsQuantitative.Enabled = Sweet.lollipop.qVals.Count > 0;
            cb_buildAsQuantitative.Checked = false;
            cmbx_geneLabel.SelectedIndex = Lollipop.gene_name_labels.IndexOf(Lollipop.preferred_gene_label);
            cb_geneCentric.Checked = Lollipop.gene_centric_families;
        }

        public void InitializeParameterSet()
        {
            Lollipop.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
            Lollipop.gene_centric_families = cb_geneCentric.Checked;

            cmbx_tableSelector.SelectedIndexChanged -= cmbx_tableSelector_SelectedIndexChanged;
            cmbx_tableSelector.SelectedIndex = 0;
            cmbx_tableSelector.SelectedIndexChanged += cmbx_tableSelector_SelectedIndexChanged;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;

            cb_count_adducts_as_id.Checked = Sweet.lollipop.count_adducts_as_identifications;

            cb_only_assign_common_known_mods.Checked = Sweet.lollipop.only_assign_common_or_known_mods;

            cb_id_from_td_nodes.Checked = Sweet.lollipop.identify_from_td_nodes;
            cb_td_ID_reduce_ambiguity.Checked = Sweet.lollipop.topdown_theoretical_reduce_ambiguity;
            cb_remove_bad_connections.Checked = Sweet.lollipop.remove_bad_connections;
            nUD_notch_tolerance.Value = (decimal)Sweet.lollipop.id_ppm_tolerance;
            cb_ppm_tolerance.Checked = Sweet.lollipop.id_use_ppm_tolerance;
            cb_annotated_PTMs_reduce_ambiguity.Checked = Sweet.lollipop.annotated_PTMs_reduce_ambiguity;

            initialize_every_time();
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable> { DisplayProteoformFamily.FormatFamiliesTable(Sweet.lollipop.target_proteoform_community.families.Select(x => new DisplayProteoformFamily(x)).ToList(), "ProteoformFamilies") };
            return DataTables;
        }

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.target_proteoform_community.has_e_proteoforms;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.construct_target_and_decoy_families();
            cmbx_tableSelector.SelectedIndex = 0;
            tb_tableFilter.Text = "";
            FillTablesAndCharts();


            List<string> accessions = new List<string>();
            using (var writer = new System.IO.StreamWriter("C:\\users\\lschaffer2\\desktop\\peptides_theo_count.tsv"))
            {
                writer.WriteLine("peptideseq\tbegin\tend\tpeptidePTMlist\ttheoreticals\ttheoreticalsPTMlists");
                foreach (var accession in Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession)
                {
                    if (Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number].ContainsKey(accession.Key))
                    {
                        var theoreticals = Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number][accession.Key];
                        foreach (var peptide in accession.Value)
                        {
                            var theoreticals_with_this_peptide = theoreticals.Where(t => t.new_topdown_proteoform == false && t.bottom_up_PSMs.Contains(peptide));
                            if (theoreticals_with_this_peptide.Count() > 0)
                            {
                                writer.WriteLine(peptide.pfr_accession + "\t" + peptide.begin + "\t" + peptide.end + "\t" +
                                    (peptide.ptm_list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", peptide.ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(m => UnlocalizedModification.LookUpId(m.modification) + "@" + m.position)))
                                    + "\t" + theoreticals_with_this_peptide.Count() + "\t" + String.Join("|", theoreticals_with_this_peptide.Select(t => t.accession))
                                + "\t" + String.Join("|", theoreticals_with_this_peptide.Select(t => t.ptm_set.ptm_description)));
                            }
                        }
                    }
                    else
                    {
                        accessions.Add(accession.Key);
                    }
                }
            }

            using (var writer = new System.IO.StreamWriter("C:\\users\\lschaffer2\\desktop\\peptides_td_count.tsv"))
            {
                writer.WriteLine("peptideseq\tbegin\tend\tpeptidePTMlist\ttheoreticals\ttheoreticalsPTMlists");

                foreach (var accession in Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession)
                {
                    var topdowns = Sweet.lollipop.topdown_proteoforms.Where(t => t.accession.Split('_')[0].Split('-')[0] == accession.Key && t.ambiguous_topdown_hits.Count == 0);
                    foreach (var peptide in accession.Value)
                    {
                        var theoreticals_with_this_peptide = topdowns.Where(t => Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                           t.topdown_end, true).Contains(peptide));
                        if (theoreticals_with_this_peptide.Count() > 0)
                        {
                            writer.WriteLine(peptide.pfr_accession + "\t" + peptide.begin + "\t" + peptide.end + "\t" + (peptide.ptm_list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", peptide.ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(m => UnlocalizedModification.LookUpId(m.modification) + "@" + m.position)))
                                + "\t" + theoreticals_with_this_peptide.Count() + "\t" + String.Join("|", theoreticals_with_this_peptide.Select(t => t.accession))
                                + "\t" + String.Join("|", theoreticals_with_this_peptide.Select(t => t.topdown_ptm_description)));
                        }
                    }
                }
            }

            using (var writer = new System.IO.StreamWriter("C:\\users\\lschaffer2\\desktop\\peptides_other_peptides.tsv"))
            {
                writer.WriteLine("accession\tpeptideseq\tbegin\tend\tpeptidePTMlist\ttheoreticalsPTMlists");

                foreach (var accession in Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession)
                {
                    foreach (var peptide in accession.Value)
                    {
                        foreach (var mod in peptide.ptm_list.Where(m => UnlocalizedModification.bio_interest(m.modification)))
                        {
                            string m = UnlocalizedModification.LookUpId(mod.modification) + "@" + mod.position;
                            var diff = accession.Value.Where(p => p != peptide && p.begin <= mod.position && p.end >= mod.position && !p.ptm_list.Where(q => UnlocalizedModification.bio_interest(q.modification)).Select(d => UnlocalizedModification.LookUpId(d.modification) + "@" + d.position).Contains(m)).ToList();
                            if (diff.Count > 0)
                            {
                                writer.WriteLine(peptide.accession + "\t" + peptide.pfr_accession + "\t" + peptide.begin + "\t" + peptide.end + "\t" + (peptide.ptm_list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", peptide.ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(b => UnlocalizedModification.LookUpId(b.modification) + "@" + b.position))) +
                                "\t" + String.Join("|", diff.Select(d => (d.ptm_list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", d.ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(a => UnlocalizedModification.LookUpId(a.modification) + "@" + a.position)))).Distinct()));

                            }
                        }
                    }
                }
            }

            //var parameters = new DigestionParams(protease: "top-down", maxModsForPeptides: 3);
            //var proteins = Sweet.lollipop.theoretical_database.theoretical_proteins.SelectMany(p => p.Value);
            //var proteoforms = proteins.SelectMany(p => p.Digest(parameters, null, null));
            //using (var writer = new System.IO.StreamWriter("C:\\users\\lschaffer2\\desktop\\theoreticalPTMlocalized.tsv"))
            //{
            //    writer.WriteLine("count\taccession\tpeptideseq\tpepbegin\tpepend\tpeptidePTMlist\tbegin\tend\ttheoreticalsPTMlists");
            //    int count = 1;
            //    foreach (var proteoform in proteoforms)
            //    {
            //        var list = new List<Ptm>();
            //        int begin = proteoform.OneBasedStartResidueInProtein;
            //        int end = proteoform.OneBasedEndResidueInProtein;
            //        foreach (var entry in proteoform.AllModsOneIsNterminus)
            //        {
            //            Modification mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
            //                       .SelectMany(m => m).Where(m => m.IdWithMotif == entry.Value.IdWithMotif)
            //                       .FirstOrDefault();
            //            if (mod != null)
            //            {
            //                list.Add(new Ptm(entry.Key + begin - (entry.Key == 1 ? 1 : 2), entry.Value));
            //            }
            //        }
            //        var peptides = Proteoform.get_possible_PSMs(proteoform.Protein.Accession, new PtmSet(list), begin, end, true);
            //        var i_care = peptides.Where(p => p.begin == begin || p.end == end || p.ptm_list.Any(m => UnlocalizedModification.bio_interest(m.modification)));
            //        foreach(var peptide in i_care)
            //        {
            //            if (!list.Where(m => UnlocalizedModification.bio_interest(m.modification)).All(m => peptide.ptm_list.Select(m2 => UnlocalizedModification.LookUpId(m2.modification) + "@" + m2.position).Contains(UnlocalizedModification.LookUpId(m.modification) + "@" + m.position))) continue;
            //            writer.WriteLine(proteoform.MonoisotopicMass + "\t" + peptide.accession + "\t" + peptide.pfr_accession + "\t" + peptide.begin + "\t" + peptide.end + "\t" + (peptide.ptm_list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", peptide.ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(m => UnlocalizedModification.LookUpId(m.modification) + "@" + m.position))) +
            //                "\t" + begin + "\t" + end + "\t" + (list.Count(p => UnlocalizedModification.bio_interest(p.modification)) == 0 ? "Unmodified" : String.Join(";", list.Where(p => UnlocalizedModification.bio_interest(p.modification)).Select(m => UnlocalizedModification.LookUpId(m.modification) + "@" + m.position))));

            //        }
            //        count++;
            //    }
            //}
        }

        public void FillTablesAndCharts()
        {
            fill_proteoform_families("", -1);
            update_figures_of_merit();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.clear_all_families();
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();
            dgv_proteoform_family_members.DataSource = null;
            dgv_proteoform_family_members.Rows.Clear();
            tb_tableFilter.Clear();
            rtb_proteoformFamilyResults.Clear();

            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    sweet.ClearListsTablesFigures(false);
                }
            }
        }

        /// <summary>
        /// generate report and change selected table names based on # decoy communities
        /// </summary>
        public void update_figures_of_merit()
        {
            rtb_proteoformFamilyResults.Text = ResultsSummaryGenerator.proteoform_families_report();
            int selection = cmbx_tableSelector.SelectedIndex;
            cmbx_tableSelector.Items.Clear();
            cmbx_tableSelector.Items.AddRange(table_names);
            cmbx_tableSelector.Items.AddRange(Enumerable.Range(0, Sweet.lollipop.decoy_proteoform_communities.Count).Select(i => "Decoy Community " + i).ToArray());
            cmbx_tableSelector.SelectedIndex = selection < cmbx_tableSelector.Items.Count ? selection : 0;
        }

        #endregion Public Methods

        #region Main Table Private Methods

        private static string[] table_names = new string[]
        {
            "Proteoform Families",
            "Theoretical Proteoforms in Families",
            "GO Terms of Families -- " + Aspect.BiologicalProcess,
            "GO Terms of Families -- " + Aspect.CellularComponent,
            "GO Terms of Families -- " + Aspect.MolecularFunction,
        };

        private void CmbxChanged()
        {
            if (cmbx_tableSelector.SelectedIndex == 0) { fill_proteoform_families(tb_tableFilter.Text, -1); }
            else if (cmbx_tableSelector.SelectedIndex == 1) { fill_theoreticals(tb_tableFilter.Text); }
            else if (cmbx_tableSelector.SelectedIndex == 2) { fill_go(Aspect.BiologicalProcess, tb_tableFilter.Text); }
            else if (cmbx_tableSelector.SelectedIndex == 3) { fill_go(Aspect.CellularComponent, tb_tableFilter.Text); }
            else if (cmbx_tableSelector.SelectedIndex == 4) { fill_go(Aspect.MolecularFunction, tb_tableFilter.Text); }
            else if (cmbx_tableSelector.SelectedIndex > 4) { fill_proteoform_families(tb_tableFilter.Text, cmbx_tableSelector.SelectedIndex - 5); }
        }

        private void cmbx_tableSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            CmbxChanged();
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            CmbxChanged();
        }

        public void fill_proteoform_families(string filter, int decoyCommunityMinusOneIsTarget)
        {
            IEnumerable<object> families = filter == "" ?
                 (decoyCommunityMinusOneIsTarget < 0 ?
                Sweet.lollipop.target_proteoform_community.families.OrderByDescending(f => f.relations.Count) :
                Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + decoyCommunityMinusOneIsTarget].families.OrderByDescending(f => f.relations.Count)) :
                (decoyCommunityMinusOneIsTarget < 0 ?
                ExtensionMethods.filter(Sweet.lollipop.target_proteoform_community.families.OrderByDescending(f => f.relations.Count), filter)
                : ExtensionMethods.filter(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + decoyCommunityMinusOneIsTarget].families.OrderByDescending(f => f.relations.Count), filter));
            DisplayUtility.FillDataGridView(dgv_main, families.OfType<ProteoformFamily>().Select(f => new DisplayProteoformFamily(f)));
            DisplayProteoformFamily.FormatFamiliesTable(dgv_main);
        }

        private void fill_theoreticals(string filter)
        {
            IEnumerable<object> theoreticals = filter == "" ?
                Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms) :
                ExtensionMethods.filter(Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms), filter);
            DisplayUtility.FillDataGridView(dgv_main, theoreticals.OfType<TheoreticalProteoform>().Select(t => new DisplayTheoreticalProteoform(t)));
            DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_main);
        }

        private void fill_go(Aspect aspect, string filter)
        {
            DisplayUtility.FillDataGridView(dgv_main, filter == "" ?
                Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ExpandedProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect) :
                ExtensionMethods.filter(Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.theoretical_proteoforms).SelectMany(t => t.ExpandedProteinList).SelectMany(g => g.GoTerms).Where(g => g.Aspect == aspect), filter));
        }

        private void dgv_proteoform_families_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((cmbx_tableSelector.SelectedIndex == 0 || cmbx_tableSelector.SelectedIndex > 4) && e.RowIndex >= 0)
            {
                display_family_members(e.RowIndex, e.ColumnIndex);
            }
        }

        private void dgv_proteoform_families_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((cmbx_tableSelector.SelectedIndex == 0 || cmbx_tableSelector.SelectedIndex > 4) && e.RowIndex >= 0)
            {
                display_family_members(e.RowIndex, e.ColumnIndex);
            }
        }

        private void display_family_members(int row_index, int column_index)
        {
            ProteoformFamily selected_family = (ProteoformFamily)((DisplayObject)this.dgv_main.Rows[row_index].DataBoundItem).display_object;

            if (column_index < 0) { return; }

            if (new List<string> { nameof(DisplayProteoformFamily.theoretical_count), nameof(DisplayProteoformFamily.accession_list), nameof(DisplayProteoformFamily.name_list) }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.theoretical_proteoforms.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.theoretical_proteoforms.Select(t => new DisplayTheoreticalProteoform(t)));
                    DisplayTheoreticalProteoform.FormatTheoreticalProteoformTable(dgv_proteoform_family_members);
                }
                else
                {
                    dgv_proteoform_family_members.Rows.Clear();
                }
            }
            else if (new List<string> { nameof(DisplayProteoformFamily.experimental_count), nameof(DisplayProteoformFamily.experimentals_list), nameof(DisplayProteoformFamily.agg_mass_list) }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.experimental_proteoforms.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms.Where(e => !e.topdown_id).Select(e => new DisplayExperimentalProteoform(e)));
                    DisplayExperimentalProteoform.FormatAggregatesTable(dgv_proteoform_family_members);
                }
                else
                {
                    dgv_proteoform_family_members.Rows.Clear();
                }
            }
            else if (new List<string> { nameof(DisplayProteoformFamily.topdown_count) }.Contains(dgv_main.Columns[column_index].Name))
            {
                if (selected_family.experimental_proteoforms.Count(p => p.topdown_id) > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.experimental_proteoforms.Where(p => p.topdown_id).Select(td => new DisplayTopDownProteoform(td as TopDownProteoform)));
                    DisplayTopDownProteoform.FormatTopDownTable(dgv_proteoform_family_members, false);
                }
                else
                {
                    dgv_proteoform_family_members.Rows.Clear();
                }
            }
            else if (dgv_main.Columns[column_index].Name == nameof(DisplayProteoformFamily.relation_count))
            {
                if (selected_family.relations.Count > 0)
                {
                    DisplayUtility.FillDataGridView(dgv_proteoform_family_members, selected_family.relations.Select(r => new DisplayProteoformRelation(r)));
                    DisplayProteoformRelation.FormatRelationsGridView(dgv_proteoform_family_members, false, false, false);
                }
                else
                {
                    dgv_proteoform_family_members.Rows.Clear();
                }
            }
        }

        #endregion Main Table Private Methods

        #region Cytoscape Visualization Private Fields

        private readonly FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        private bool got_cyto_temp_folder;

        #endregion Cytoscape Visualization Private Fields

        #region Cytoscape Visualization Private Methods

        private void btn_browseTempFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_familyBuildFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void tb_tempFileFolderPath_TextChanged(object sender, EventArgs e)
        {
            string path = tb_familyBuildFolder.Text;
            Sweet.lollipop.family_build_folder_path = path;
            got_cyto_temp_folder = true;
            enable_buildAllFamilies_button();
            enable_buildSelectedFamilies_button();
        }

        private void enable_buildAllFamilies_button()
        {
            if (got_cyto_temp_folder) { btn_buildAllFamilies.Enabled = true; }
        }

        private void enable_buildSelectedFamilies_button()
        {
            if (got_cyto_temp_folder && dgv_main.SelectedRows.Count > 0) { btn_buildSelectedFamilies.Enabled = true; }
        }

        private void btn_buildAllFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp,
                cb_buildAsQuantitative.Checked ? (MdiParent as ProteoformSweet).resultsSummary.get_go_analysis() : null, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildSelectedFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            object[] selected = DisplayUtility.get_selected_objects(dgv_main);
            string message = CytoscapeScript.write_cytoscape_script(selected, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp,
                cb_buildAsQuantitative.Checked ? (MdiParent as ProteoformSweet).resultsSummary.get_go_analysis() : null, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void Families_update_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut(false);
                Cursor = Cursors.Default;
            }
        }

        private void nud_decimalRoundingLabels_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.deltaM_edge_display_rounding = Convert.ToInt32(this.nud_decimalRoundingLabels.Value);
        }

        private void cmbx_geneLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lollipop.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
        }

        #endregion Cytoscape Visualization Private Methods

        #region Private Methods

        private void cb_buildAsQuantitative_CheckedChanged(object sender, EventArgs e)
        {
            cb_redBorder.Enabled = cb_buildAsQuantitative.Checked;
            cb_boldLabel.Enabled = cb_buildAsQuantitative.Checked;
        }

        private void cb_geneCentric_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.gene_centric_families = cb_geneCentric.Checked;
        }

        private void cb_count_adducts_as_id_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.count_adducts_as_identifications = cb_count_adducts_as_id.Checked;
            update_figures_of_merit();
        }

        private void cmbx_empty_TextChanged(object sender, EventArgs e)
        {
            // Simplifies the empty TextChanged methods for combo boxes
        }

        #endregion Private Methods

        private void cb_only_assign_common_known_mods_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.only_assign_common_or_known_mods = cb_only_assign_common_known_mods.Checked;
        }

        private void cb_id_from_td_nodes_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.identify_from_td_nodes = cb_id_from_td_nodes.Checked;
        }

        private void cb_remove_bad_connections_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.remove_bad_connections = cb_remove_bad_connections.Checked;
        }

        private void nUD_notch_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.id_ppm_tolerance = (double)nUD_notch_tolerance.Value;
        }

        private void cb_ppm_tolerance_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.id_use_ppm_tolerance = cb_ppm_tolerance.Checked;
        }

        private void cb_td_ID_reduce_ambiguity_CheckedChanged_1(object sender, EventArgs e)
        {
            Sweet.lollipop.topdown_theoretical_reduce_ambiguity = cb_td_ID_reduce_ambiguity.Checked;
        }

        private void cb_annotated_PTMs_reduce_ambiguity_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = cb_annotated_PTMs_reduce_ambiguity.Checked;
        }
    }
}