using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace PS_0._00
{
    public partial class TheoreticalDatabase : Form
    {
        OpenFileDialog openFileDialog2 = new OpenFileDialog();
        OpenFileDialog openFileDialog3 = new OpenFileDialog();
        DataGridView dgv_database = new DataGridView();

        struct protein
        {
            public string accession, name, fragment, sequence;
            public int begin, end;
            public Dictionary<int, List<string>> positionsAndPtms;
        }

        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        private void FillDataBaseTable(string table)
        {
            BindingSource dgv_DB_BS = new BindingSource();
            dgv_DB_BS.DataSource = GlobalData.theoreticalAndDecoyDatabases.Tables[table];
            dgv_Database.DataSource = dgv_DB_BS;
            dgv_database.AutoGenerateColumns = true;
            dgv_database.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_database.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void btn_GetUniProtXML_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog2.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                String file = openFileDialog2.FileName;
                try
                {
                    if (ValidateUniProtXML(file))
                    {
                        tb_UniProtXML_Path.Text = file;
                    }
                }
                catch (SecurityException ex)
                {
                    // The user lacks appropriate permissions to read files, discover paths, etc.
                    MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                        "Error message: " + ex.Message + "\n\n" +
                        "Details (send to Support):\n\n" + ex.StackTrace
                    );
                }
                catch (Exception ex)
                {
                    // Could not load the result file - probably related to Windows file system permissions.
                    MessageBox.Show("Cannot display the file: " + file.Substring(file.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or " +
                        "it may be corrupt.\n\nReported error: " + ex.Message);
                }

            }
        }

        private bool FirstLineOK(string fileName)
        {
            bool fileOK = true;

            return fileOK;
        }

        private void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            InitializeOpenFileDialog2();
            InitializeOpenFileDialog3();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            ckbx_OxidMeth.Checked = false;
            ckbx_Carbam.Checked = true;
            ckbx_Meth_Cleaved.Checked = true;

            btn_NeuCode_Lt.Checked = true;

            nUD_MaxPTMs.Minimum = 0;
            nUD_MaxPTMs.Maximum = 5;
            nUD_MaxPTMs.Value = 3;

            nUD_NumDecoyDBs.Minimum = 0;
            nUD_NumDecoyDBs.Maximum = 50;
            nUD_NumDecoyDBs.Value = 0;
        }

        private void InitializeOpenFileDialog2()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog2.Filter =
                "UniProt XML (*.xml)|*.xml";

            // Allow the user to select multiple images.
            this.openFileDialog2.Multiselect = false;
            this.openFileDialog2.Title = "UniProt XML Format Database";
        }

        private void InitializeOpenFileDialog3()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog3.Filter =
                "UniProt PTM List (*.txt)|*.txt";

            // Allow the user to select multiple images.
            this.openFileDialog3.Multiselect = false;
            this.openFileDialog3.Title = "UniProt PTM List";
        }
        private void btn_UniPtPtmList_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog3.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                String file = openFileDialog3.FileName;
                try
                {
                    if (FirstLineOK(file))
                    {
                        tb_UniProtPtmList_Path.Text = file;
                    }
                }
                catch (SecurityException ex)
                {
                    // The user lacks appropriate permissions to read files, discover paths, etc.
                    MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                        "Error message: " + ex.Message + "\n\n" +
                        "Details (send to Support):\n\n" + ex.StackTrace
                    );
                }
                catch (Exception ex)
                {
                    // Could not load the result file - probably related to Windows file system permissions.
                    MessageBox.Show("Cannot display the file: " + file.Substring(file.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or " +
                        "it may be corrupt.\n\nReported error: " + ex.Message);
                }

            }
        }

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {

            Dictionary<char, double> aaIsotopeMassList = new Dictionary<char, double>();
            string kI = WhichLysineIsotopeComposition(); 
            aaIsotopeMassList = AminoAcidMasses.GetAA_Masses(Convert.ToBoolean(ckbx_OxidMeth.Checked), Convert.ToBoolean(ckbx_Carbam.Checked), kI);
            
            ReadUniprotPtmList rup = new ReadUniprotPtmList();
            ReadUniprotPtmList.oldPtmFilePath = tb_UniProtPtmList_Path.Text; // gets the exising path to PTM list into read_uniprot_ptlist class
            Dictionary<string, ModData> uniprotModificationTable = new Dictionary<string, ModData>();
            uniprotModificationTable = rup.rd_unip_ptms();

            int totalNumEntries = NumberOfUniProtEntrys(tb_UniProtXML_Path.Text);
            string giantProtein = getOneGiantProtein(tb_UniProtXML_Path.Text, Convert.ToBoolean(ckbx_Meth_Cleaved.Checked));
            protein[] proteinRawInfo = new protein[totalNumEntries];
            proteinRawInfo = GetProteinRawInfo(tb_UniProtXML_Path.Text, totalNumEntries);
            processEntries(proteinRawInfo, Convert.ToBoolean(ckbx_Meth_Cleaved.Checked), totalNumEntries, aaIsotopeMassList, Convert.ToInt32(nUD_MaxPTMs.Value), uniprotModificationTable);

            processDecoys(Convert.ToInt32(nUD_NumDecoyDBs.Value), giantProtein, proteinRawInfo, Convert.ToBoolean(ckbx_Meth_Cleaved.Checked), totalNumEntries, aaIsotopeMassList, Convert.ToInt32(nUD_MaxPTMs.Value), uniprotModificationTable);

            BindingList<string> bindinglist = new BindingList<string>();
            BindingSource bSource = new BindingSource();
            bSource.DataSource = bindinglist;
            cmbx_DisplayWhichDB.DataSource = bSource;


            foreach (DataTable dt in GlobalData.theoreticalAndDecoyDatabases.Tables)
            {
                bindinglist.Add(dt.TableName);
                //cmbx_DisplayWhichDB.Items.Add(dt.TableName[0].ToString());
            }

            FillDataBaseTable(cmbx_DisplayWhichDB.SelectedItem.ToString());
        }

        static void processDecoys(int numDb, string giantProtein, protein[] pRD, bool mC, int num, Dictionary<char, double> aIML, int maxPTMsPerProteoform, Dictionary<string, ModData> uniprotModificationTable)
        {

            for (int decoyNumber = 0; decoyNumber < numDb; decoyNumber++)
            {
                string tableName = "DecoyDatabase_" + decoyNumber;

                DataTable decoy = new DataTable(tableName);//datatable name goes in parentheses.
                decoy.Columns.Add("Accession", typeof(string));
                decoy.Columns.Add("Name", typeof(string));
                decoy.Columns.Add("Fragment", typeof(string));
                decoy.Columns.Add("Begin", typeof(int));
                decoy.Columns.Add("End", typeof(int));
                decoy.Columns.Add("Mass", typeof(double));
                decoy.Columns.Add("Lysine Count", typeof(int));
                decoy.Columns.Add("PTM List", typeof(string));
                decoy.Columns.Add("PTM Group Mass", typeof(double));
                decoy.Columns.Add("Proteoform Mass", typeof(double));

                new Random().Shuffle(pRD); //Randomize Order of Protein Array

                for (int i = 0; i < num; i++)
                {
                    double mass;
                    if (mC && pRD[i].begin == 0 && pRD[i].sequence.Substring(0, 1) == "M") // methionine cleavage of N-terminus specified
                    {
                        int hunkLength = pRD[i].sequence.Length - 1;
                        string hunk = giantProtein.Substring(0, hunkLength);
                        giantProtein.Remove(0, hunkLength);

                        mass = CalculateProteoformMass(ref aIML, hunk);

                        int kCount = LysineCount(hunk, "K");

                        PtmCombos pc = new PtmCombos();

                        List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>();

                        OneUniquePtmGroup unmod = new OneUniquePtmGroup();
                        unmod.mass = 0;
                        List<string> unmod_string = new List<string>();
                        unmod_string.Add("unmodified");
                        unmod.unique_ptm_combinations = unmod_string;
                        aupg.Add(unmod);
                        
                        if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                        {
                            aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                        }

                        foreach (OneUniquePtmGroup group in aupg)
                        {
                            List<string> ptm_list = group.unique_ptm_combinations;
                            Double ptm_mass = group.mass;
                            Double proteoform_mass = mass + group.mass;
                            decoy.Rows.Add(pRD[i].accession + "_DECOY_" + decoyNumber, pRD[i].name, pRD[i].fragment, pRD[i].begin + 1, pRD[i].end, mass, kCount, string.Join("; ", ptm_list), ptm_mass, proteoform_mass);
                        }

                    }
                    else
                    {
                        int hunkLength = pRD[i].sequence.Length;
                        string hunk = giantProtein.Substring(0, hunkLength);
                        giantProtein.Remove(0, hunkLength);

                        mass = CalculateProteoformMass(ref aIML, hunk);

                        int kCount = LysineCount(hunk, "K");

                        PtmCombos pc = new PtmCombos();

                        List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>();

                        OneUniquePtmGroup unmod = new OneUniquePtmGroup();
                        unmod.mass = 0;
                        List<string> unmod_string = new List<string>();
                        unmod_string.Add("unmodified");
                        unmod.unique_ptm_combinations = unmod_string;
                        aupg.Add(unmod);
                        
                        if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                        {
                            aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                        }

                        foreach (OneUniquePtmGroup group in aupg)
                        {
                            List<string> ptm_list = group.unique_ptm_combinations;
                            Console.WriteLine("PTM Combinations: " + String.Join("; ", ptm_list));
                            Double ptm_mass = group.mass;
                            Double proteoform_mass = mass + group.mass;
                            decoy.Rows.Add(pRD[i].accession + "_DECOY_" + decoyNumber, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, string.Join("; ", ptm_list), ptm_mass, proteoform_mass);
                        }

                    }

                }

                GlobalData.theoreticalAndDecoyDatabases.Tables.Add(decoy);
            }
        }

        static void processEntries(protein[] pRD, bool mC, int num, Dictionary<char, double> aIML, int maxPTMsPerProteoform, Dictionary<string, ModData> uniprotModificationTable)
        {
            DataTable target = new DataTable("Target");//datatable name goes in parentheses.
            target.Columns.Add("Accession", typeof(string));
            target.Columns.Add("Name", typeof(string));
            target.Columns.Add("Fragment", typeof(string));
            target.Columns.Add("Begin", typeof(int));
            target.Columns.Add("End", typeof(int));
            target.Columns.Add("Mass", typeof(double));
            target.Columns.Add("Lysine Count", typeof(int));
            target.Columns.Add("PTM List", typeof(string));
            target.Columns.Add("PTM Group Mass", typeof(double));
            target.Columns.Add("Proteoform Mass", typeof(double));

            for (int i = 0; i < num; i++)
            {
                double mass;
                if (mC && pRD[i].begin == 0 && pRD[i].sequence.Substring(0, 1) == "M") // methionine cleavage of N-terminus specified
                {
                    mass = CalculateProteoformMass(ref aIML, pRD[i].sequence.Substring(1, (pRD[i].sequence.Length - 1)));

                    int kCount = LysineCount(pRD[i].sequence.Substring(1, (pRD[i].sequence.Length - 1)), "K");

                    PtmCombos pc = new PtmCombos();

                    List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>();
                    OneUniquePtmGroup unmod = new OneUniquePtmGroup();
                    unmod.mass = 0;
                    List<string> unmod_string = new List<string>();
                    unmod_string.Add("unmodified");
                    unmod.unique_ptm_combinations = unmod_string;
                    aupg.Add(unmod);

                    if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                    {
                        aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                    }

                    foreach (OneUniquePtmGroup group in aupg)
                    {
                        List<string> ptm_list = group.unique_ptm_combinations;
                        Double ptm_mass = group.mass;
                        Double proteoform_mass = mass + group.mass;
                        target.Rows.Add(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin + 1, pRD[i].end, mass, kCount, string.Join("; ", ptm_list), ptm_mass, proteoform_mass);
                    }

                }
                else
                {
                    mass = CalculateProteoformMass(ref aIML, pRD[i].sequence);
                    int kCount = LysineCount(pRD[i].sequence, "K");
                    PtmCombos pc = new PtmCombos();
                    List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>();
                    OneUniquePtmGroup unmod = new OneUniquePtmGroup();
                    unmod.mass = 0;
                    List<string> unmod_string = new List<string>();
                    unmod_string.Add("unmodified");
                    unmod.unique_ptm_combinations = unmod_string;
                    aupg.Add(unmod);

                    if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                    {
                        aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                    }

                    foreach (OneUniquePtmGroup group in aupg)
                    {
                        List<string> ptm_list = group.unique_ptm_combinations;
                        Double ptm_mass = group.mass;
                        Double proteoform_mass = mass + group.mass;
                        target.Rows.Add(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, string.Join("; ",ptm_list), ptm_mass, proteoform_mass);
                    }

                }

            }
            GlobalData.theoreticalAndDecoyDatabases.Tables.Add(target);

        }

        static int LysineCount(string text, string search)
        {
            int num = 0;
            int pos = 0;

            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(search))
            {
                while ((pos = text.IndexOf(search, pos)) > -1)
                {
                    num++;
                    pos += search.Length;
                }
            }
            return num;
        }

        static double CalculateProteoformMass(ref Dictionary<char, double> aaIsotopeMassList, string pForm)
        {
            double proteoformMass = 18.010565; // start with water

            char[] aminoAcids = pForm.ToCharArray();

            for (int i = 0; i < pForm.Length; i++)
            {
                double aMass = 0;
                try
                {
                    aMass = aaIsotopeMassList[aminoAcids[i]];
                }
                catch
                {
                    aMass = 0;
                }
                proteoformMass = proteoformMass + aMass;
            }

            return proteoformMass;
        }

        static protein[] GetProteinRawInfo(string inFile, int num)
        {
            protein[] pRI = new protein[num];

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    int count = 0;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        Dictionary<int, List<string>> pAP = new Dictionary<int, List<string>>();

                        pAP = GetPositionsPTMs(_entry); // dictionary of positions and PTMs in complete entry

                        pRI[count].accession = _entry.accession[0];
                        //Console.WriteLine(_entry.accession[0]);
                        pRI[count].name = GetProteinName(_entry);
                        pRI[count].fragment = "full";

                        //this next bit eliminates return characters etc from sequence string
                        string fullSequence = _entry.sequence.Value;
                        char[] arr = fullSequence.ToCharArray();
                        arr = Array.FindAll<char>(arr, (c => (char.IsLetter(c))));
                        fullSequence = new string(arr);

                        pRI[count].sequence = fullSequence;
                        pRI[count].begin = 0;
                        pRI[count].end = fullSequence.Length - 1;
                        int fullSequenceLength = _entry.sequence.length;
                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, pRI[count].begin, pRI[count].end);

                        count++;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin < 0) || (end < 0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                    if (realFeature)
                                    {
                                        position = position - 1;
                                        if (position < 0)
                                        {
                                            realFeature = false;
                                        }
                                    }
                                }
                            }
                            if (realFeature)
                            {
                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "signal peptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin + 1));
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    case "chain":
                                        if ((end - begin + 1) != fullSequenceLength)
                                        {
                                            pRI[count].accession = _entry.accession[0];
                                            pRI[count].name = GetProteinName(_entry);
                                            pRI[count].fragment = "chain";
                                            pRI[count].sequence = fullSequence.Substring(begin, (end - begin + 1));
                                            pRI[count].begin = begin;
                                            pRI[count].end = end;
                                            pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                            count++;
                                        }
                                        break;
                                    case "propeptide":
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "propeptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin) + 1);
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    case "peptide":
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "peptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin) + 1);
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return pRI;
        }

        static Dictionary<int, List<string>> SegmentPTMs(Dictionary<int, List<string>> allPosPTMs, int begin, int end)
        {
            Dictionary<int, List<string>> segPosPTMs = new Dictionary<int, List<string>>();

            foreach (int position in allPosPTMs.Keys)
            {
                if (position >= begin && position <= end)
                {
                    segPosPTMs.Add(position, allPosPTMs[position]);
                }
            }

            return segPosPTMs;// the int is the amino acid position and the string[] are the different ptms at that position
        }

        static string GetProteinName(entry _ent)
        {
            string name = "";

            int proteinNameType = 1;
            try
            {
                if ((_ent.protein.recommendedName.fullName.Value) != null)
                {
                    proteinNameType = 1;
                }
            }
            catch
            {
                try
                {
                    if ((_ent.protein.submittedName[0].fullName.Value) != null)
                    {
                        proteinNameType = 2;
                    }
                }
                catch
                {
                    proteinNameType = 3;
                }
            }


            switch (proteinNameType)
            {
                case 1: //Recommended Name
                    {
                        name = _ent.protein.recommendedName.fullName.Value;
                        break;
                    }
                case 2: //Submitted Name
                    {
                        name = _ent.protein.submittedName[0].fullName.Value.ToString();
                        break;
                    }
                case 3: //Alternative Name
                    {
                        name = _ent.protein.alternativeName[0].fullName.Value.ToString();
                        break;
                    }
                default:
                    name = "";
                    break;
            }

            return name;
        }

        static Dictionary<int, List<string>> GetPositionsPTMs(entry _ent)
        {
            Dictionary<int, List<string>> local_pAP = new Dictionary<int, List<string>>();

            foreach (var proteinFeature in _ent.feature)//process protein ptms
            {
                string type = proteinFeature.type.ToString();

                int position = 0;
                int begin = 0;
                int end = 0;
                bool realFeature = false;

                int noPosition = 0;
                try
                {
                    if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                }
                catch
                {
                    noPosition = 0;
                }
                if (noPosition == 1) // has begin and end
                {
                    if (proteinFeature.location.Items[0].status.ToString() == "certain"
                        && proteinFeature.location.Items[0].positionSpecified)
                    {
                        realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                        if (realFeature)
                        {
                            if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                && proteinFeature.location.Items[1].positionSpecified)
                            {
                                realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                            }
                        }
                    }

                    if (realFeature)
                    {
                        begin = begin - 1;
                        end = end - 1;
                        if ((begin < 0) || (end < 0))
                        {
                            realFeature = false;
                        }
                    }

                }
                else // protein only has single position location
                {
                    if (proteinFeature.location.Items[0].status.ToString() == "certain"
                        && proteinFeature.location.Items[0].positionSpecified)
                    {
                        realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                        if (realFeature)
                        {
                            position = position - 1;
                            if (position < 0)
                            {
                                realFeature = false;
                            }
                        }
                    }

                }

                if (realFeature)
                {
                    switch (type)
                    {
                        case "modifiedresidue":
                            string description = proteinFeature.description.ToString();
                            //Console.WriteLine(_ent.accession[0] + "\t" + description + "\t" + position);
                            if (local_pAP.ContainsKey(position))
                            {
                                List<string> morePtms = new List<string>();
                                morePtms = local_pAP[position].ToList();
                                morePtms.Add(description.Split(';')[0]);//take description up to ';' if there is one
                                local_pAP[position] = morePtms;
                            }
                            else
                            {
                                List<string> ptms = new List<string>();
                                ptms.Add(description.Split(';')[0]);
                                local_pAP.Add(position, ptms);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }


            return local_pAP;
        }

        static string getOneGiantProtein(string inFile, bool mC)//returns sum of full length, signal peptide, chain, propetide and peptide
        {
            StringBuilder giantProtein = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        string thisFullSequence = _entry.sequence.Value.Replace("\r", "").Replace("\n", "");
                        //Console.WriteLine(thisFullSequence.ToString());

                        if (mC && (thisFullSequence.Substring(0, 1) == "M"))
                        {
                            //Console.WriteLine("MC: " + mC);
                            giantProtein.Append("-" + thisFullSequence.Substring(1)); // should be everything after the first character
                        }
                        else
                        {
                            Console.WriteLine("full");
                            giantProtein.Append("-" + thisFullSequence);
                        }



                        int fullSequenceLength = thisFullSequence.Length;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin < 0) || (end < 0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                    if (realFeature)
                                    {
                                        position = position - 1;
                                        if (position < 0)
                                        {
                                            realFeature = false;
                                        }
                                    }
                                }
                            }
                            if (realFeature)
                            {
                                if (mC && (begin == 0) && end >= 1 && (thisFullSequence.Substring(0, 1) == "M"))
                                {
                                    //Console.WriteLine("inside begin + 1");
                                    begin = begin + 1;
                                }

                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                        //Console.WriteLine(thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    case "chain":
                                        if (mC == true)
                                        {
                                            if ((end - begin + 1) != (fullSequenceLength - 1))
                                            {
                                                giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                            }
                                        }
                                        else
                                        {
                                            if ((end - begin + 1) != fullSequenceLength)
                                            {
                                                giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                            }
                                        }
                                        break;
                                    case "propeptide":
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    case "peptide":
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return giantProtein.ToString();
        }

        static int NumberOfUniProtEntrys(string inFile)//returns sum of full length, signal peptide, chain, propetide and peptide
        {
            var nodeCount = 0;

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        nodeCount++;//one for main entry
                        int fullSequenceLength = _entry.sequence.length;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin < 0) || (end < 0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                    if (realFeature)
                                    {
                                        position = position - 1;
                                        if (position < 0)
                                        {
                                            realFeature = false;
                                        }
                                    }
                                }
                            }
                            if (realFeature)
                            {
                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        nodeCount++;
                                        break;
                                    case "chain":
                                        if ((end - begin + 1) != fullSequenceLength)
                                        {
                                            //Console.WriteLine("b: {0} e: {1} f: {2}", begin, end, fullSequenceLength);
                                            nodeCount++;
                                        }
                                        break;
                                    case "propeptide":
                                        nodeCount++;
                                        break;
                                    case "peptide":
                                        nodeCount++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return nodeCount;
        }

        private string WhichLysineIsotopeComposition()
        {
            string kI;

            if (btn_NaturalIsotopes.Checked)
            {
                kI = "n";
            }
            else if (btn_NeuCode_Lt.Checked)
            {
                kI = "l";
            }
            else  // must be heavy neucode (aka btn_NeuCode_Hv.Checked
            {
                kI = "h";
            }
            return kI;
        }

        static bool ValidateUniProtXML(string testXmlFile)
        {
            bool valid = false;
            string line1, line2;
            try
            {
                using (StreamReader reader = new StreamReader(testXmlFile))
                {
                    line1 = reader.ReadLine();
                    line2 = reader.ReadLine();
                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                    if (line2.Contains("uniprot"))
                    {
                        valid = true;
                    }
                    else
                    {
                        Console.WriteLine("This is not a valid uniprot .xml file. Try again.");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Try again.");
            }
            return valid;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            //don't know how to delete this
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillDataBaseTable(cmbx_DisplayWhichDB.SelectedItem.ToString());
        }
    }
}
