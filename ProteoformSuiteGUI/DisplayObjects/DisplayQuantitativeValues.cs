using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayQuantitativeValues : DisplayObject
    {

        #region Public Constructors

        public DisplayQuantitativeValues(QuantitativeProteoformValues q)
            : base(q)
        {
            proteoform = q.proteoform;
            qval = q;
        }

        #endregion

        #region Private Fields

        private Proteoform proteoform;
        private QuantitativeProteoformValues qval;

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
                    proteoform.gene_name.get_prefered_name(ProteoformCommunity.preferred_gene_label) :
                    "";
            }
        }

        public decimal NumeratorIntensitySum
        {
            get { return qval.TusherValues1.numeratorIntensitySum; }
        }
        
        public decimal DenominatorIntensitySum
        {
            get { return qval.TusherValues1.denominatorIntensitySum; }
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
            get { return qval.TusherValues1.scatter; }
        }

        public decimal pValue
        {
            get { return qval.pValue_randomization; }
        }

        public bool Significant
        {
            get { return qval.TusherValues1.significant; }
        }

        public decimal TestStatistic_linear
        {
            get { return qval.TusherValues1.relative_difference; }
        }

        public decimal AvgPermutedTestStatistic
        {
            get { return qval.TusherValues1.correspondingAvgSortedRelDiff; }
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

            //NUMBER FORMAT
            dgv.Columns[nameof(NumeratorIntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(DenominatorIntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(IntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(LogFoldChange)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(pValue)].DefaultCellStyle.Format = "E2";
            dgv.Columns[nameof(TestStatistic_linear)].DefaultCellStyle.Format = "0.#####";
            dgv.Columns[nameof(AvgPermutedTestStatistic)].DefaultCellStyle.Format = "0.#####";

            //HEADERS
            dgv.Columns[nameof(GeneName)].HeaderText = "Gene Name";
            dgv.Columns[nameof(NumeratorIntensitySum)].HeaderText = "Light Intensity Sum";
            dgv.Columns[nameof(DenominatorIntensitySum)].HeaderText = "Heavy Intensity Sum";
            dgv.Columns[nameof(IntensitySum)].HeaderText = "Intensity Sum";
            dgv.Columns[nameof(LogFoldChange)].HeaderText = "Log2 Fold Change";
            dgv.Columns[nameof(pValue)].HeaderText = "p-value (by randomization test)";
            dgv.Columns[nameof(TestStatistic_linear)].HeaderText = "Student's t-Test Statistic (Linear Intensities)";
            dgv.Columns[nameof(AvgPermutedTestStatistic)].HeaderText = "Corresponding Avg. Permuted Student's t-Test Statistic (Linear Intensities)";
            dgv.Columns[nameof(manual_validation_quant)].HeaderText = "Abundant Component for Manual Validation of Quantification";
        }

        #endregion

    }
}
