using System;
using ProteoformSuiteInternal;
using System.Windows.Forms;


namespace ProteoformSuiteGUI
{
    public class DisplayProteoformRelation : DisplayObject
    {

        #region Private Fields

        private DeltaMassPeak peak;
        private ProteoformComparison relation_type;
        private Proteoform[] connected_proteoforms;

        #endregion

        #region Public Properties

        // Relation properties
        public int PeakCenterCount { get { return peak != null ? peak.peak_relation_group_count : -1000000; } }
        public double PeakCenterDeltaMass { get { return peak != null ? peak.peak_deltaM_average : Double.NaN; } }
        public string RelationType { get { return relation_type.ToString(); } }
        public bool Accepted
        {
            get
            {
                return ((ProteoformRelation)display_object).accepted;
            }
            set
            {
                ((ProteoformRelation)display_object).accepted = value;
            }
        }
        public double DeltaMass { get { return ((ProteoformRelation)display_object).delta_mass; } }
        public int LysineCount { get { return ((ProteoformRelation)display_object).lysine_count; } }
        public int NearbyRelationCount { get { return ((ProteoformRelation)display_object).nearby_relations.Count; } }
        public bool OutsideNoMansLand { get { return ((ProteoformRelation)display_object).outside_no_mans_land; } }

        // For DataGridView display of proteoform1
        public double agg_intensity_1
        {
            get
            {
                try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_intensity; }
                catch { return Double.NaN; }
            }
        }
        public double agg_RT_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_rt; } catch { return Double.NaN; } }
        }
        public int num_observations_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).observation_count; } catch { return -1000000; } }
        }
        public double proteoform_mass_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_mass; } catch { return Double.NaN; } }
        }

        public string accession_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).accession; } catch { return null; } }
        }

        // For DataGridView display of proteform2
        public double proteoform_mass_2
        {
            get
            {
                return connected_proteoforms[1] is ExperimentalProteoform ?
                    ((ExperimentalProteoform)connected_proteoforms[1]).agg_mass :
                    ((TheoreticalProteoform)connected_proteoforms[1]).modified_mass;
            }
        }

        public double agg_intensity_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).agg_intensity; } catch { return 0; } }
        }
        public double agg_RT_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).agg_rt; } catch { return 0; } }
        }
        public int num_observations_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).observation_count; } catch { return 0; } }
        }
        public string accession_2
        {
            get { try { return (connected_proteoforms[1]).accession; } catch { return null; } }
        }
        public string Name { get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).name; } catch { return null; } } }
        public string Fragment { get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).fragment; } catch { return null; } } }
        public string PtmDescription { get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).ptm_descriptions; } catch { return null; } } }

        #endregion Public Properties

        public DisplayProteoformRelation(ProteoformRelation r)
            : base(r)
        {
            peak = r.peak;
            relation_type = r.relation_type;
            connected_proteoforms = r.connected_proteoforms;
        }

        public static void FormatRelationsGridView(DataGridView dgv, bool mask_experimental, bool mask_theoretical)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //round table values
            dgv.Columns[nameof(DeltaMass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(PeakCenterDeltaMass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(proteoform_mass_1)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(proteoform_mass_2)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(agg_intensity_1)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(agg_intensity_2)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(agg_RT_1)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(agg_RT_2)].DefaultCellStyle.Format = "0.##";

            //set column header
            dgv.Columns[nameof(DeltaMass)].HeaderText = "Delta Mass";
            dgv.Columns[nameof(NearbyRelationCount)].HeaderText = "Nearby Relation Count";
            dgv.Columns[nameof(Accepted)].HeaderText = "Accepted";
            dgv.Columns[nameof(PeakCenterDeltaMass)].HeaderText = "Peak Center Delta Mass";
            dgv.Columns[nameof(PeakCenterCount)].HeaderText = "Peak Center Count";
            dgv.Columns[nameof(LysineCount)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(OutsideNoMansLand)].HeaderText = "Outside No Man's Land";

            //ET formatting
            dgv.Columns[nameof(PtmDescription)].HeaderText = "PTM Description";
            if (mask_experimental)
            {
                dgv.Columns[nameof(num_observations_1)].HeaderText = "Number Experimental Observations";
                dgv.Columns[nameof(accession_1)].HeaderText = "Experimental Accession";
                dgv.Columns[nameof(accession_2)].HeaderText = "Theoretical Accession";
                dgv.Columns[nameof(proteoform_mass_2)].HeaderText = "Theoretical Proteoform Mass";
                dgv.Columns[nameof(proteoform_mass_1)].HeaderText = "Experimental Aggregated Proteoform Mass";
                dgv.Columns[nameof(agg_intensity_1)].HeaderText = "Experimental Aggregated Intensity";
                dgv.Columns[nameof(agg_RT_1)].HeaderText = "Experimental Aggregated RT";
                dgv.Columns[nameof(agg_intensity_2)].Visible = false;
                dgv.Columns[nameof(agg_RT_2)].Visible = false;
                dgv.Columns[nameof(num_observations_2)].Visible = false;
                dgv.Columns[nameof(RelationType)].Visible = false;
            }

            //EE formatting
            if (mask_theoretical)
            {
                dgv.Columns[nameof(num_observations_2)].HeaderText = "Number Light Experimental Observations";
                dgv.Columns[nameof(num_observations_1)].HeaderText = "Number Heavy Experimental Observations";
                dgv.Columns[nameof(agg_intensity_2)].HeaderText = "Light Experimental Aggregated Intensity";
                dgv.Columns[nameof(agg_intensity_1)].HeaderText = "Heavy Experimental Aggregated Intensity";
                dgv.Columns[nameof(agg_RT_1)].HeaderText = "Aggregated RT Heavy";
                dgv.Columns[nameof(agg_RT_2)].HeaderText = "Aggregated RT Light";
                dgv.Columns[nameof(accession_1)].HeaderText = "Heavy Experimental Accession";
                dgv.Columns[nameof(accession_2)].HeaderText = "Light Experimental Accession";
                dgv.Columns[nameof(proteoform_mass_1)].HeaderText = "Heavy Experimental Aggregated Mass";
                dgv.Columns[nameof(proteoform_mass_2)].HeaderText = "Light Experimental Aggregated Mass";
                dgv.Columns[nameof(Name)].Visible = false;
                dgv.Columns[nameof(Fragment)].Visible = false;
            }

            //ProteoformFamilies formatting
            dgv.Columns[nameof(RelationType)].HeaderText = "Relation Type";

            //making these columns invisible
            dgv.Columns[nameof(LysineCount)].Visible = Lollipop.neucode_labeled;
        }
    }
}
