using ProteoformSuiteInternal;
using System;
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
        public int PeakCenterCount
        {
            get
            {
                return peak != null ? 
                    peak.peak_relation_group_count :
                    -1000000;
            }
        }

        public double PeakCenterDeltaMass
        {
            get
            {
                return peak != null ? 
                    peak.DeltaMass : 
                    Double.NaN;
            }
        }

        public string RelationType
        {
            get
            {
                return relation_type.ToString();
            }
        }

        public bool Accepted
        {
            get
            {
                return ((ProteoformRelation)display_object).Accepted;
            }
            set
            {
                ((ProteoformRelation)display_object).Accepted = value;
            }
        }

        public double DeltaMass
        {
            get
            {
                return ((ProteoformRelation)display_object).DeltaMass;
            }
        }

        public int LysineCount
        {
            get
            {
                return ((ProteoformRelation)display_object).lysine_count;
            }
        }

        public int NearbyRelationCount
        {
            get
            {
                return ((ProteoformRelation)display_object).nearby_relations_count;
            }
        }

        public bool OutsideNoMansLand
        {
            get
            {
                return ((ProteoformRelation)display_object).outside_no_mans_land;
            }
        }


        // For DataGridView display of proteoform1
        public double agg_intensity_1
        {
            get
            {
                return connected_proteoforms[0] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[0]).agg_intensity :
                    Double.NaN;
            }
        }

        public double agg_RT_1
        {
            get
            {
                return connected_proteoforms[0] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[0]).agg_rt :
                    connected_proteoforms[0] as TopDownProteoform != null ?
                    ((TopDownProteoform)connected_proteoforms[0]).agg_rt : 0;
            }
        }

        public int num_observations_1
        {
            get
            {
                return connected_proteoforms[0] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[0]).aggregated.Count :
                    -1000000;
            }
        }

        public double proteoform_mass_1
        {
            get
            {
                return connected_proteoforms[0].modified_mass;
            }
        }

        public string accession_1
        {
            get
            {
                return connected_proteoforms[0].accession;
            }
        }

        public string manual_validation_id_1
        {
            get
            {
                return connected_proteoforms[0] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[0]).manual_validation_id :
                    "";
            }
        }


        // For DataGridView display of proteform2
        public double proteoform_mass_2
        {
            get
            {
                return connected_proteoforms[1].modified_mass;
            }
        }

        public double agg_intensity_2
        {
            get
            {
                return connected_proteoforms[1] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[1]).agg_intensity :
                    0;
            }
        }

        public double agg_RT_2
        {
            get
            {
                return connected_proteoforms[1] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[1]).agg_rt :
                    0;
            }
        }

        public int num_observations_2
        {
            get
            {
                return connected_proteoforms[1] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[1]).aggregated.Count :
                    0;
            }
        }

        public string accession_2
        {
            get
            {
                return connected_proteoforms[1].accession;
            }
        }

        public string manual_validation_id_2
        {
            get
            {
                return connected_proteoforms[1] as ExperimentalProteoform != null ?
                    ((ExperimentalProteoform)connected_proteoforms[1]).manual_validation_id :
                    "";
            }
        }

        public string Name
        {
            get
            {
                return connected_proteoforms[1] as TheoreticalProteoform != null ?
                    ((TheoreticalProteoform)connected_proteoforms[1]).name :
                    "";
            }
        }

        public string Fragment
        {
            get
            {
                return connected_proteoforms[1] as TheoreticalProteoform != null ?
                    ((TheoreticalProteoform)connected_proteoforms[1]).fragment :
                    "";
            }
        }

        public string TheoreticalDescription
        {
            get
            {
                return connected_proteoforms[1] as TheoreticalProteoform != null ?
                    ((TheoreticalProteoform)connected_proteoforms[1]).description :
                    "";
            }
        }

        public string PTMDescription
        {
            get
            {
                return connected_proteoforms[1] as TheoreticalProteoform != null ?
                    ((TheoreticalProteoform)connected_proteoforms[1]).ptm_description :
                    "";
            }
        }

        #endregion Public Properties

        #region Public Constructors

        public DisplayProteoformRelation(ProteoformRelation r)
            : base(r)
        {
            peak = r.peak;
            relation_type = r.RelationType;
            connected_proteoforms = r.connected_proteoforms;
        }

        #endregion

        #region Public Methods

        public static void FormatRelationsGridView(DataGridView dgv, bool mask_experimental, bool mask_theoretical, bool mask_peaks)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //round table values
            dgv.Columns[nameof(DeltaMass)].DefaultCellStyle.Format = "0.####";
            if(!mask_peaks) dgv.Columns[nameof(PeakCenterDeltaMass)].DefaultCellStyle.Format = "0.####";
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
            if(!mask_peaks) dgv.Columns[nameof(PeakCenterCount)].HeaderText = "Peak Center Count";
            dgv.Columns[nameof(LysineCount)].HeaderText = "Lysine Count";
            if (!mask_peaks) dgv.Columns[nameof(OutsideNoMansLand)].HeaderText = "Outside No Man's Land";

            //Raw ET histogram and TD relation formatting
            if(mask_peaks)
            {
                dgv.Columns[nameof(PeakCenterCount)].Visible = false;
                dgv.Columns[nameof(PeakCenterDeltaMass)].Visible = false;
                dgv.Columns[nameof(OutsideNoMansLand)].Visible = false;
            }

            //ET formatting
            dgv.Columns[nameof(TheoreticalDescription)].HeaderText = "Theoretical Description";
            dgv.Columns[nameof(PTMDescription)].HeaderText = "PTM Description";
            if (mask_experimental)
            {
                dgv.Columns[nameof(num_observations_1)].HeaderText = "Number Experimental Observations";
                dgv.Columns[nameof(accession_1)].HeaderText = "Experimental Accession";
                dgv.Columns[nameof(accession_2)].HeaderText = "Theoretical Accession";
                dgv.Columns[nameof(proteoform_mass_2)].HeaderText = "Theoretical Proteoform Mass";
                dgv.Columns[nameof(proteoform_mass_1)].HeaderText = "Experimental Aggregated Proteoform Mass";
                dgv.Columns[nameof(agg_intensity_1)].HeaderText = "Experimental Aggregated Intensity";
                dgv.Columns[nameof(agg_RT_1)].HeaderText = "Experimental Aggregated RT";
                dgv.Columns[nameof(manual_validation_id_1)].HeaderText = "Abundant Exp. Component for Manual Validation";
                dgv.Columns[nameof(agg_intensity_2)].Visible = false;
                dgv.Columns[nameof(agg_RT_2)].Visible = false;
                dgv.Columns[nameof(num_observations_2)].Visible = false;
                dgv.Columns[nameof(manual_validation_id_2)].Visible = false;
                dgv.Columns[nameof(RelationType)].Visible = false;
            }

            //EE formatting
            if (mask_theoretical)
            {
                dgv.Columns[nameof(TheoreticalDescription)].Visible = false;
                dgv.Columns[nameof(PTMDescription)].Visible = false;
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
                dgv.Columns[nameof(manual_validation_id_1)].HeaderText = "Heavy Abundant Exp. Component for Manual Validation";
                dgv.Columns[nameof(manual_validation_id_2)].HeaderText = "Light Abundant Exp. Component for Manual Validation";
                dgv.Columns[nameof(RelationType)].Visible = false;
                dgv.Columns[nameof(Name)].Visible = false;
                dgv.Columns[nameof(Fragment)].Visible = false;
            }

            //ProteoformFamilies formatting
            if (!mask_experimental && !mask_theoretical)
            {
                dgv.Columns[nameof(num_observations_2)].HeaderText = "Number Observations #2";
                dgv.Columns[nameof(num_observations_1)].HeaderText = "Number Observations #1";
                dgv.Columns[nameof(agg_intensity_2)].HeaderText = "Intensity #2";
                dgv.Columns[nameof(agg_intensity_1)].HeaderText = "Intensity #1";
                dgv.Columns[nameof(agg_RT_1)].HeaderText = "Aggregated RT #1";
                dgv.Columns[nameof(agg_RT_2)].HeaderText = "Aggregated RT #2";
                dgv.Columns[nameof(accession_1)].HeaderText = "Accession #1";
                dgv.Columns[nameof(accession_2)].HeaderText = "Accession #2";
                dgv.Columns[nameof(proteoform_mass_1)].HeaderText = "Proteoform Mass #1";
                dgv.Columns[nameof(proteoform_mass_2)].HeaderText = "Proteoform Mass #2";
                dgv.Columns[nameof(manual_validation_id_1)].HeaderText = "Abundant Exp. Component for Manual Validation #1";
                dgv.Columns[nameof(manual_validation_id_2)].HeaderText = "Abundant Exp. Component for Manual Validation #2";
                dgv.Columns[nameof(RelationType)].HeaderText = "Relation Type";
            }

            //making these columns invisible
            dgv.Columns[nameof(LysineCount)].Visible = Sweet.lollipop.neucode_labeled;
        }
        #endregion

    }
}
