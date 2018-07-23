using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayQuantitativeValues : DisplayObject
    {
        #region Public Constructors

        public DisplayQuantitativeValues(QuantitativeProteoformValues q, IGoAnalysis analysis)
            : base(q)
        {
            proteoform = q.proteoform;
            qval = q;
            this.analysis = analysis;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly Proteoform proteoform;
        private QuantitativeProteoformValues qval;
        private IGoAnalysis analysis;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get { return proteoform.accession; }
        }

        public string GeneName
        {
            get
            {
                return proteoform.gene_name != null ?
                    proteoform.gene_name.get_prefered_name(Lollipop.preferred_gene_label) :
                    "";
            }
        }

        public string Theoretical
        {
            get
            {
                return proteoform as TopDownProteoform != null ? proteoform.accession.Split('_')[0] :
                    proteoform.linked_proteoform_references != null ? proteoform.linked_proteoform_references.First().accession.Split('_')[0] : "";
            }
        }

        public double mass
        {
            get
            {
                return proteoform.modified_mass;
            }
        }

        public double retention_time
        {
            get
            {
                return (proteoform as ExperimentalProteoform).agg_rt;
            }
        }

        public decimal NumeratorIntensitySum
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.numeratorIntensitySum :
                  analysis as TusherAnalysis2 != null ? qval.TusherValues2.numeratorIntensitySum :
                  (decimal)qval.Log2FoldChangeValues.numeratorIntensitySum;
            }
        }

        public decimal DenominatorIntensitySum
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.denominatorIntensitySum
                  : analysis as TusherAnalysis2 != null ? qval.TusherValues2.denominatorIntensitySum :
                  (decimal)qval.Log2FoldChangeValues.denominatorIntensitySum;
            }
        }

        public decimal IntensitySum
        {
            get
            {
                return analysis as TusherAnalysis != null ? qval.intensitySum
               : (decimal)(qval.Log2FoldChangeValues.denominatorIntensitySum + qval.Log2FoldChangeValues.numeratorIntensitySum);
            }
        }

        public decimal LogFoldChange
        {
            get
            {
                return analysis as TusherAnalysis != null ? qval.tusherlogFoldChange :
                    (decimal)qval.Log2FoldChangeValues.logfold2change;
            }
        }

        public decimal Scatter_linear
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.scatter
                  : analysis as TusherAnalysis2 != null ? qval.TusherValues2.scatter
                  : 0;
            }
        }

        public double pValue
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? double.NaN
                 : analysis as TusherAnalysis2 != null ? double.NaN
                 : qval.Log2FoldChangeValues.pValue_uncorrected;
            }
        }

        public double benjiHoch
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? double.NaN
                 : analysis as TusherAnalysis2 != null ? double.NaN
                 : qval.Log2FoldChangeValues.benjiHoch_value;
            }
        }

        public bool Significant
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.significant :
                  analysis as TusherAnalysis2 != null ? qval.TusherValues2.significant :
                  qval.Log2FoldChangeValues.significant;
            }
        }

        public decimal RelativeDifference
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.relative_difference :
                  analysis as TusherAnalysis2 != null ? qval.TusherValues2.relative_difference :
                  (decimal)qval.Log2FoldChangeValues.tTestStatistic;
            }
        }

        public decimal AvgPermutedTestStatistic
        {
            get
            {
                return analysis as TusherAnalysis1 != null ? qval.TusherValues1.correspondingAvgSortedRelDiff :
                  analysis as TusherAnalysis2 != null ? qval.TusherValues2.correspondingAvgSortedRelDiff :
                  0;
            }
        }

        public string manual_validation_quant
        {
            get { return qval.proteoform.manual_validation_quant; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatGridView(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatGridView(List<DisplayQuantitativeValues> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayQuantitativeValues).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(GeneName)) { return "Gene Name"; }
            if (property_name == nameof(NumeratorIntensitySum)) { return Sweet.lollipop.numerator_condition + " Intensity Sum"; }
            if (property_name == nameof(DenominatorIntensitySum)) { return Sweet.lollipop.denominator_condition + " Intensity Sum"; }
            if (property_name == nameof(IntensitySum)) { return "Intensity Sum"; }
            if (property_name == nameof(LogFoldChange)) { return "Log2 Fold Change"; }
            if (property_name == nameof(pValue)) { return "p-value"; }
            if (property_name == nameof(benjiHoch)) { return "Benjamini-Hochberg corrected p-value"; }
            if (property_name == nameof(RelativeDifference)) { return "Student's t-Test Statistic"; }
            if (property_name == nameof(AvgPermutedTestStatistic)) { return "Corresponding Avg. Permuted Student's t-Test Statistic"; }
            if (property_name == nameof(retention_time)) { return "Retention Time"; }
            if (property_name == nameof(mass)) { return "Aggregated Mass"; }
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(NumeratorIntensitySum)) { return "0.##"; }
            if (property_name == nameof(DenominatorIntensitySum)) { return "0.##"; }
            if (property_name == nameof(IntensitySum)) { return "0.##"; }
            if (property_name == nameof(LogFoldChange)) { return "0.####"; }
            if (property_name == nameof(pValue)) { return "E2"; }
            if (property_name == nameof(benjiHoch)) { return "E2"; }
            if (property_name == nameof(RelativeDifference)) { return "0.#####"; }
            if (property_name == nameof(AvgPermutedTestStatistic)) { return "0.#####"; }
            if (property_name == nameof(mass)) { return "0.####"; }
            if (property_name == nameof(retention_time)) { return "0.##"; }
            return null;
        }

        #endregion Private Methods
    }
}