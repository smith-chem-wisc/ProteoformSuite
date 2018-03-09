using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayProteoformRelation : DisplayObject
    {
        #region Private Fields

        private readonly DeltaMassPeak peak;
        private ProteoformComparison relation_type;
        private Proteoform[] connected_proteoforms;

        #endregion Private Fields

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

        #endregion Public Constructors

        #region Public Methods

        public static void FormatRelationsGridView(DataGridView dgv, bool mask_experimental, bool mask_theoretical, bool raw_et_histogram)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name, mask_experimental, mask_theoretical, raw_et_histogram);
                string n = number_format(c.Name, raw_et_histogram);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible, mask_experimental, mask_theoretical, raw_et_histogram);
            }
        }

        public static DataTable FormatRelationsGridView(List<DisplayProteoformRelation> display, string table_name, bool mask_experimental, bool mask_theoretical, bool raw_et_histogram)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayProteoformRelation).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name, mask_experimental, mask_theoretical, raw_et_histogram), visible(x.Name, true, mask_experimental, mask_theoretical, raw_et_histogram)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name, bool mask_experimental, bool mask_theoretical, bool raw_et_histogram)
        {
            //set column header
            if (property_name == nameof(DeltaMass)) { return "Delta Mass"; }
            if (property_name == nameof(NearbyRelationCount)) { return "Nearby Relation Count"; }
            if (property_name == nameof(Accepted)) { return "Accepted"; }
            if (property_name == nameof(PeakCenterDeltaMass)) { return "Peak Center Delta Mass"; }
            if (!raw_et_histogram && property_name == nameof(PeakCenterCount)) { return "Peak Center Count"; }
            if (property_name == nameof(LysineCount)) { return "Lysine Count"; }
            if (!raw_et_histogram && property_name == nameof(OutsideNoMansLand)) { return "Outside No Man's Land"; }

            //ET formatting
            if (property_name == nameof(PTMDescription)) { return "PTM Description"; }
            if (mask_experimental)
            {
                if (property_name == nameof(num_observations_1)) { return "Number Experimental Observations"; }
                if (property_name == nameof(accession_1)) { return "Experimental Accession"; }
                if (property_name == nameof(accession_2)) { return "Theoretical Accession"; }
                if (property_name == nameof(proteoform_mass_2)) { return "Theoretical Proteoform Mass"; }
                if (property_name == nameof(proteoform_mass_1)) { return "Experimental Aggregated Proteoform Mass"; }
                if (property_name == nameof(agg_intensity_1)) { return "Experimental Aggregated Intensity"; }
                if (property_name == nameof(agg_RT_1)) { return "Experimental Aggregated RT"; }
                if (property_name == nameof(manual_validation_id_1)) { return "Abundant Exp. Component for Manual Validation"; }
                if (property_name == nameof(TheoreticalDescription)) { return "Theoretical Description"; }
            }

            //EE formatting
            if (mask_theoretical)
            {
                if (property_name == nameof(num_observations_2)) { return "Number Light Experimental Observations"; }
                if (property_name == nameof(num_observations_1)) { return "Number Heavy Experimental Observations"; }
                if (property_name == nameof(agg_intensity_2)) { return "Light Experimental Aggregated Intensity"; }
                if (property_name == nameof(agg_intensity_1)) { return "Heavy Experimental Aggregated Intensity"; }
                if (property_name == nameof(agg_RT_1)) { return "Aggregated RT Heavy"; }
                if (property_name == nameof(agg_RT_2)) { return "Aggregated RT Light"; }
                if (property_name == nameof(accession_1)) { return "Heavy Experimental Accession"; }
                if (property_name == nameof(accession_2)) { return "Light Experimental Accession"; }
                if (property_name == nameof(proteoform_mass_1)) { return "Heavy Experimental Aggregated Mass"; }
                if (property_name == nameof(proteoform_mass_2)) { return "Light Experimental Aggregated Mass"; }
                if (property_name == nameof(manual_validation_id_1)) { return "Heavy Abundant Exp. Component for Manual Validation"; }
                if (property_name == nameof(manual_validation_id_2)) { return "Light Abundant Exp. Component for Manual Validation"; }
            }

            //ProteoformFamilies formatting
            if (!mask_experimental && !mask_theoretical)
            {
                if (property_name == nameof(num_observations_2)) { return "Number Observations #2"; }
                if (property_name == nameof(num_observations_1)) { return "Number Observations #1"; }
                if (property_name == nameof(agg_intensity_2)) { return "Intensity #2"; }
                if (property_name == nameof(agg_intensity_1)) { return "Intensity #1"; }
                if (property_name == nameof(agg_RT_1)) { return "Aggregated RT #1"; }
                if (property_name == nameof(agg_RT_2)) { return "Aggregated RT #2"; }
                if (property_name == nameof(accession_1)) { return "Accession #1"; }
                if (property_name == nameof(accession_2)) { return "Accession #2"; }
                if (property_name == nameof(proteoform_mass_1)) { return "Proteoform Mass #1"; }
                if (property_name == nameof(proteoform_mass_2)) { return "Proteoform Mass #2"; }
                if (property_name == nameof(manual_validation_id_1)) { return "Abundant Exp. Component for Manual Validation #1"; }
                if (property_name == nameof(manual_validation_id_2)) { return "Abundant Exp. Component for Manual Validation #2"; }
                if (property_name == nameof(RelationType)) { return "Relation Type"; }
            }
            return null;
        }

        private static bool visible(string property_name, bool current, bool mask_experimental, bool mask_theoretical, bool raw_et_histogram)
        {
            //ET formatting
            if (mask_experimental)
            {
                if (property_name == nameof(agg_intensity_2)) { return false; }
                if (property_name == nameof(agg_RT_2)) { return false; }
                if (property_name == nameof(num_observations_2)) { return false; }
                if (property_name == nameof(manual_validation_id_2)) { return false; }
                if (property_name == nameof(RelationType)) { return false; }
            }

            //EE formatting
            if (mask_theoretical)
            {
                if (property_name == nameof(RelationType)) { return false; }
                if (property_name == nameof(Name)) { return false; }
                if (property_name == nameof(Fragment)) { return false; }
                if (property_name == nameof(PTMDescription)) { return false; }
                if (property_name == nameof(TheoreticalDescription)) { return false; }
            }
            if (property_name == nameof(LysineCount)) return Sweet.lollipop.neucode_labeled;
            return current;
        }

        private static string number_format(string property_name, bool raw_et_histogram)
        {
            //round table values
            if (property_name == nameof(DeltaMass)) { return "0.####"; }
            if (!raw_et_histogram && property_name == nameof(PeakCenterDeltaMass)) { return "0.####"; }
            if (property_name == nameof(proteoform_mass_1)) { return "0.####"; }
            if (property_name == nameof(proteoform_mass_2)) { return "0.####"; }
            if (property_name == nameof(agg_intensity_1)) { return "0.##"; }
            if (property_name == nameof(agg_intensity_2)) { return "0.##"; }
            if (property_name == nameof(agg_RT_1)) { return "0.##"; }
            if (property_name == nameof(agg_RT_2)) { return "0.##"; }
            return null;
        }

        #endregion Private Methods
    }
}