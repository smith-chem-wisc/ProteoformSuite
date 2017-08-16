using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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


        // For quantification files
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


        // For identification files
        public bool matchingCalibrationFile
        {
            get
            {
                return file.matchingCalibrationFile;
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

        #endregion Public Properties

        #region Public Methods

        public static void FormatInputFileTable(DataGridView dgv, IEnumerable<Purpose> dgv_purposes)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.Visible = visible(c.Name, c.Visible, dgv_purposes);
            }

            //EDITABILITY
            dgv.Columns[nameof(UniqueId)].ReadOnly = true;
            dgv.Columns[nameof(complete_path)].ReadOnly = true;
            dgv.Columns[nameof(Directory)].ReadOnly = true;
            dgv.Columns[nameof(Filename)].ReadOnly = true;
            dgv.Columns[nameof(Purpose)].ReadOnly = true;
            dgv.Columns[nameof(matchingCalibrationFile)].ReadOnly = true;
        }

        public static DataTable FormatInputFileTable(List<DisplayInputFile> display, string table_name, IEnumerable<Purpose> dgv_purposes)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayInputFile).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true, dgv_purposes)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion

        #region Private Methods


        private static string header(string property_name)
        {
            if (property_name == nameof(UniqueId)) return "File ID";
            if (property_name == nameof(complete_path)) return "File Path";
            if (property_name == nameof(matchingCalibrationFile)) return "Matching Calibration File";
            if (property_name == nameof(biological_replicate)) return "Biological Replicate";
            if (property_name == nameof(TechnicalReplicate)) return "Technical Replicate";
            if (property_name == nameof(lt_condition)) return Sweet.lollipop.neucode_labeled ? "NeuCode Light Condition" : "Condition";
            if (property_name == nameof(hv_condition)) return "NeuCode Heavy Condition";
            if (property_name == nameof(ContaminantDB)) return "Contaminant Database";
            return null;
        }

        private static bool visible(string property_name, bool current, IEnumerable<Purpose> dgv_purposes)
        {
            if (property_name == nameof(matchingCalibrationFile)) return dgv_purposes.Contains(Purpose.Calibration) || dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(Labeling)) return dgv_purposes.Contains(Purpose.Identification) || dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(biological_replicate)) return dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(Fraction)) return dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(TechnicalReplicate)) return dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(lt_condition)) return dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(hv_condition)) return Sweet.lollipop.neucode_labeled && dgv_purposes.Contains(Purpose.Quantification);
            if (property_name == nameof(ContaminantDB)) return dgv_purposes.Contains(Purpose.ProteinDatabase);
            return current;
        }

        #endregion Private Methods
    }
}
