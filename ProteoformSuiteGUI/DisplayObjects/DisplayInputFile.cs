using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayInputFile : DisplayObject
    {

        #region Public Constructors

        public DisplayInputFile(InputFile file)
            : base(file)
        {
            this.file = file;
        }

        #endregion

        #region Private Fields

        private InputFile file;

        #endregion

        #region Public Properties

        // For all files
        public int UniqueId
        {
            get { return file.UniqueId; }
        }

        public string Filename
        {
            get { return file.filename; }
        }

        public Purpose Purpose
        {
            get { return file.purpose; }
        }


        //For protein databases
        public bool ContaminantDB
        {
            get { return file.ContaminantDB; }
            set { file.ContaminantDB = value; }
        }


        // For identification and quantification files
        public int biological_replicate
        {
            get { return file.biological_replicate; }
            set { file.biological_replicate = value; }
        }

        public int Fraction
        {
            get { return file.fraction; }
            set { file.fraction = value; }
        }

        public string lt_condition
        {
            get { return file.lt_condition; }
            set { file.lt_condition = value; }
        }

        public string hv_condition
        {
            get { return file.hv_condition; }
            set { file.hv_condition = value; }
        }

        public int technical_replicate
        {
            get { return file.technical_replicate; }
            set { file.technical_replicate = value; }
        }

        public Labeling Labeling
        {
            get { return file.label; }
            set { file.label = value; }
        }



        // Other for all files
        public string complete_path
        {
            get { return file.complete_path; }
        }

        public string Directory
        {
            get { return file.directory; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatInputFileTable(DataGridView dgv, IEnumerable<Purpose> dgv_purposes)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //HEADERS
            dgv.Columns[nameof(UniqueId)].HeaderText = "File ID";
            dgv.Columns[nameof(complete_path)].HeaderText = "File Path";
            dgv.Columns[nameof(biological_replicate)].HeaderText = "Biological Replicate";
            dgv.Columns[nameof(lt_condition)].HeaderText = "NeuCode Light Condition";
            dgv.Columns[nameof(hv_condition)].HeaderText = "NeuCode Heavy Condition";
            dgv.Columns[nameof(ContaminantDB)].HeaderText = "Contaminant Database";

            //EDITABILITY
            dgv.Columns[nameof(UniqueId)].ReadOnly = true;
            dgv.Columns[nameof(complete_path)].ReadOnly = true;
            dgv.Columns[nameof(Directory)].ReadOnly = true;
            dgv.Columns[nameof(Filename)].ReadOnly = true;
            dgv.Columns[nameof(Purpose)].ReadOnly = true;

            //VISIBILITY
            dgv.Columns[nameof(Labeling)].Visible = dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification);
            dgv.Columns[nameof(biological_replicate)].Visible = dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(Fraction)].Visible = dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(lt_condition)].Visible = dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(technical_replicate)].Visible = dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(hv_condition)].Visible = dgv_purposes.Contains(Purpose.Quantification);
            dgv.Columns[nameof(ContaminantDB)].Visible = dgv_purposes.Contains(Purpose.ProteinDatabase);
        }

        #endregion

    }
}
