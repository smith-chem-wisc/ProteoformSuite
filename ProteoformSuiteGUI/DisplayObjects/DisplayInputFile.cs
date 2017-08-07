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


        public string Filename
        {
            get { return file.filename; }
        }
       
        // For quantification or calibration files
        public string biological_replicate
        {
            get
            {
                return file.biological_replicate;
            }
            set
            {
                Sweet.change_file(file, file.biological_replicate, nameof(file.biological_replicate), file.biological_replicate.ToString(), value.ToString());
                file.biological_replicate = value;
            }
        }

        //For protein databases
        public bool ContaminantDB
        {
            get
            {
                return file.ContaminantDB;
            }
            set
            {
                Sweet.change_file(file, file.ContaminantDB, nameof(file.ContaminantDB), file.ContaminantDB.ToString(), value.ToString());
                file.ContaminantDB = value;
            }
        }

        public string Fraction
        {
            get
            {
                return file.fraction;
            }
            set
            {
                Sweet.change_file(file, file.fraction, nameof(file.fraction), file.fraction.ToString(), value.ToString());
                file.fraction = value;
            }
        }

        public string TechnicalReplicate
        {
            get
            {
                return file.technical_replicate;
            }
            set
            {
                Sweet.change_file(file, file.technical_replicate, nameof(file.technical_replicate), file.technical_replicate.ToString(), value.ToString());
                file.technical_replicate = value;
            }
        }

        public string lt_condition
        {
            get
            {
                return file.lt_condition;
            }
            set
            {
                Sweet.change_file(file, file.lt_condition, nameof(file.lt_condition), file.lt_condition.ToString(), value.ToString());
                file.lt_condition = value;
            }
        }


        public string hv_condition
        { 
            get
            {
                return file.hv_condition;
            }
            set
            {
                Sweet.change_file(file, file.hv_condition, nameof(file.hv_condition), file.hv_condition.ToString(), value.ToString());
                file.hv_condition = value;
            }
        }

        public Labeling Labeling
        {
            get
            {
                return file.label;
            }
            set
            {
                Sweet.change_file(file, file.label, nameof(file.label), file.label.ToString(), value.ToString());
                file.label = value;
            }
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

        public Purpose Purpose
        {
            get { return file.purpose; }
        }

        // For all files
        public int UniqueId
        {
            get { return file.UniqueId; }
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
            dgv.Columns[nameof(TechnicalReplicate)].HeaderText = "Technical Replicate";
            dgv.Columns[nameof(lt_condition)].HeaderText = Sweet.lollipop.neucode_labeled ? "NeuCode Light Condition" : "Condition";
            dgv.Columns[nameof(hv_condition)].HeaderText = "NeuCode Heavy Condition";
            dgv.Columns[nameof(ContaminantDB)].HeaderText = "Contaminant Database";

            //EDITABILITY
            dgv.Columns[nameof(UniqueId)].ReadOnly = true;
            dgv.Columns[nameof(complete_path)].ReadOnly = true;
            dgv.Columns[nameof(Directory)].ReadOnly = true;
            dgv.Columns[nameof(Filename)].ReadOnly = true;
            dgv.Columns[nameof(Purpose)].ReadOnly = true;

            //VISIBILITY
            dgv.Columns[nameof(Labeling)].Visible = dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(biological_replicate)].Visible = dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(Fraction)].Visible = dgv_purposes.Contains(Purpose.Quantification) || dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(lt_condition)].Visible = dgv_purposes.Contains(Purpose.Quantification);
            dgv.Columns[nameof(TechnicalReplicate)].Visible = dgv_purposes.Contains(Purpose.CalibrationIdentification) || dgv_purposes.Contains(Purpose.RawFile);
            dgv.Columns[nameof(hv_condition)].Visible = Sweet.lollipop.neucode_labeled && dgv_purposes.Contains(Purpose.Quantification);
            dgv.Columns[nameof(ContaminantDB)].Visible = dgv_purposes.Contains(Purpose.ProteinDatabase);
        }

        #endregion

    }
}
