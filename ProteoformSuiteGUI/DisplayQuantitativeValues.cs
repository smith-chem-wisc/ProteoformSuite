using System;
using ProteoformSuiteInternal;
using System.Windows.Forms;


namespace ProteoformSuiteGUI
{
    public class DisplayQuantitativeValues : DisplayObject
    {
        #region Public Constructors

        public DisplayQuantitativeValues(ExperimentalProteoform.quantitativeValues q)
            : base(q)
        {
            proteoform = q.proteoform;
            qval = q;
        }

        #endregion

        #region Private Fields

        private Proteoform proteoform;
        private ExperimentalProteoform.quantitativeValues qval;

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

        public decimal LightIntensitySum
        {
            get { return qval.lightIntensitySum; }
        }

        public decimal HeavyIntensitySum
        {
            get { return qval.heavyIntensitySum; }
        }

        public decimal IntensitySum
        {
            get { return qval.intensitySum; }
        }

        public decimal LogFoldChange
        {
            get { return qval.logFoldChange; }
        }

        public decimal Variance
        {
            get { return qval.variance; }
        }

        public decimal pValue
        {
            get { return qval.pValue; }
        }

        public bool Significant
        {
            get { return qval.significant; }
        }

        public decimal TestStatistic
        {
            get { return qval.testStatistic; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatGridView(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            //NUMBER FORMAT
            dgv.Columns[nameof(LightIntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(HeavyIntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(IntensitySum)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(LogFoldChange)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(pValue)].DefaultCellStyle.Format = "E2";
            dgv.Columns[nameof(TestStatistic)].DefaultCellStyle.Format = "0.###";

            //HEADERS
            dgv.Columns[nameof(GeneName)].HeaderText = "Gene Name";
            dgv.Columns[nameof(LightIntensitySum)].HeaderText = "Light Intensity Sum";
            dgv.Columns[nameof(HeavyIntensitySum)].HeaderText = "Heavy Intensity Sum";
            dgv.Columns[nameof(IntensitySum)].HeaderText = "Intensity Sum";
            dgv.Columns[nameof(LogFoldChange)].HeaderText = "Log2 Fold Change";
            dgv.Columns[nameof(pValue)].HeaderText = "p-value (by randomization test)";
            dgv.Columns[nameof(TestStatistic)].HeaderText = "Student's t-Test Statistic";
        } 

        #endregion
    }
}
