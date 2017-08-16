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

        public DisplayQuantitativeValues(QuantitativeProteoformValues q, TusherAnalysis analysis)
            : base(q)
        {
            proteoform = q.proteoform;
            qval = q;
            this.analysis = analysis;
        }

        #endregion

        #region Private Fields

        private Proteoform proteoform;
        private QuantitativeProteoformValues qval;
        private TusherAnalysis analysis;

        #endregion

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

        public decimal NumeratorIntensitySum
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.numeratorIntensitySum : qval.TusherValues2.numeratorIntensitySum; }
        }
        
        public decimal DenominatorIntensitySum
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.denominatorIntensitySum : qval.TusherValues2.denominatorIntensitySum; }
        }

        public decimal IntensitySum
        {
            get { return qval.intensitySum; }
        }

        public decimal LogFoldChange
        {
            get { return qval.logFoldChange; }
        }

        public decimal Scatter_linear
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.scatter : qval.TusherValues2.scatter; }
        }

        public double pValue
        {
            get { return qval.Log2FoldChangeValues.pValue_uncorrected; }
        }

        public bool Significant
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.significant : qval.TusherValues2.significant; }
        }

        public bool Significant_RelDiff
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.significant_relative_difference : qval.TusherValues2.significant_relative_difference; }
        }

        public bool Significant_FoldChange
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.significant_fold_change : qval.TusherValues2.significant_fold_change; }
        }

        public decimal RelativeDifference
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.relative_difference : qval.TusherValues2.relative_difference; }
        }

        public decimal AvgPermutedTestStatistic
        {
            get { return analysis as TusherAnalysis1 != null ? qval.TusherValues1.correspondingAvgSortedRelDiff : qval.TusherValues2.correspondingAvgSortedRelDiff; }
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
                c.Name = h != null ? h : c.Name;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatGridView(List<DisplayQuantitativeValues> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayQuantitativeValues).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(GeneName)) return "Gene Name";
            if (property_name == nameof(NumeratorIntensitySum)) return "Light Intensity Sum";
            if (property_name == nameof(DenominatorIntensitySum)) return "Heavy Intensity Sum";
            if (property_name == nameof(IntensitySum)) return "Intensity Sum";
            if (property_name == nameof(LogFoldChange)) return "Log2 Fold Change";
            if (property_name == nameof(pValue)) return "p-value (by randomization test)";
            if (property_name == nameof(RelativeDifference)) return "Student's t-Test Statistic (Linear Intensities)";
            if (property_name == nameof(AvgPermutedTestStatistic)) return "Corresponding Avg. Permuted Student's t-Test Statistic (Linear Intensities)";
            if (property_name == nameof(Significant_RelDiff)) return "Significant by Relative Difference Analysis";
            if (property_name == nameof(Significant_FoldChange)) return "Significant by Fold Change";
            return null;
        }
        
        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(NumeratorIntensitySum)) return "0.##";
            if (property_name == nameof(DenominatorIntensitySum)) return "0.##";
            if (property_name == nameof(IntensitySum)) return "0.##";
            if (property_name == nameof(LogFoldChange)) return "0.####";
            if (property_name == nameof(pValue)) return "E2";
            if (property_name == nameof(RelativeDifference)) return "0.#####";
            if (property_name == nameof(AvgPermutedTestStatistic)) return "0.#####";
            return null;
        }

        #endregion Private Methods

    }
}
