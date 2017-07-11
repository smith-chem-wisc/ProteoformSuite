using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ProteoformSuiteInternal
{
    public class Sweet
    {

        public static Lollipop lollipop = new Lollipop();
        public static List<string> actions = new List<string>();

        #region BASICS FOR XML WRITING

        public static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "  "
        };

        public static string time_stamp()
        {
            return DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + "-" + DateTime.Now.Second.ToString("00");
        }

        private static void initialize_doc(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("proteoform_suite");
            writer.WriteAttributeString("documentVersion", "0.01");
            writer.WriteAttributeString("id", time_stamp());
        }

        private static void finalize_doc(XmlWriter writer)
        {
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        private static string GetAttribute(XElement element, string attribute_name)
        {
            XAttribute attribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName == attribute_name);
            return attribute == null ? "" : attribute.Value;
        }

        #endregion BASICS FOR XML WRITING

        #region METHOD SAVE/LOAD

        public static StringBuilder save_method(StringBuilder builder)
        {
            using (XmlWriter writer = XmlWriter.Create(builder, xmlWriterSettings))
            {
                initialize_doc(writer);
                add_settings(writer);
                add_actions(writer);
                finalize_doc(writer);
            }
            return builder;
        }

        public static StringBuilder save_method()
        {
            return save_method(new StringBuilder());
        }

        private static void add_settings(XmlWriter writer)
        {
            //Gather field type, name, values that are not constants, which are literal, i.e. set at compile time
            //Note that fields do not have {get;set;} methods, where Properties do.

            foreach (FieldInfo field in typeof(Lollipop).GetFields().Where(f => !f.IsLiteral))
            {
                if (field.FieldType == typeof(int) ||
                    field.FieldType == typeof(double) ||
                    field.FieldType == typeof(string) ||
                    field.FieldType == typeof(decimal) ||
                    field.FieldType == typeof(bool))
                {
                    writer.WriteStartElement("setting");
                    writer.WriteAttributeString("field_type", field.FieldType.FullName);
                    writer.WriteAttributeString("field_name", field.Name);
                    writer.WriteAttributeString("field_value", field.GetValue(lollipop).ToString());
                    writer.WriteEndElement();
                }
            }
        }

        private static void add_actions(XmlWriter writer)
        {
            foreach (string action in actions)
            {
                writer.WriteStartElement("action");
                writer.WriteAttributeString("action", action);
                writer.WriteEndElement();
            }
        }

        public static void open_method(string alltext, bool add_files)
        {
            FieldInfo[] lollipop_fields = typeof(Lollipop).GetFields();
            List<XElement> setting_elements = new List<XElement>();
            List<XElement> action_elements = new List<XElement>();
            using (XmlReader reader = XmlReader.Create(new StringReader(alltext)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "setting")
                            setting_elements.Add(XElement.ReadFrom(reader) as XElement);
                        else if (reader.Name == "action")
                            action_elements.Add(XElement.ReadFrom(reader) as XElement);
                        else return; //Settings come first. Return when done
                    }
                }
            }

            foreach (XElement setting in setting_elements)
            {
                string type_string = GetAttribute(setting, "field_type");
                Type type = Type.GetType(type_string); //Takes only full name of type
                string name = GetAttribute(setting, "field_name");
                string value = GetAttribute(setting, "field_value");
                lollipop_fields.FirstOrDefault(p => p.Name == name).SetValue(lollipop, Convert.ChangeType(value, type));
            }

            foreach (XElement action in action_elements)
            {
                actions.Add(GetAttribute(action, "action"));
            }

            update_files_from_presets(add_files);
        }

        public static void open_method(string[] lines, bool add_files)
        {
            open_method(String.Join(Environment.NewLine, lines), add_files);
        }

        #endregion METHOD SAVE/LOAD

        #region Public Action Methods

        public static void add_file_action(InputFile file)
        {
            actions.Add("add file " + file.complete_path + " with purpose " + file.purpose.ToString());
        }

        public static void change_file(InputFile file, string property, string from, string to)
        {
            actions.Add("change file " + file.complete_path + " property " + property + " of type " + property.GetType().FullName + " from " + from + " to " + to);
        }

        public static void accept_peak_action(DeltaMassPeak peak)
        {
            actions.Add("accept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString());
        }

        public static void unaccept_peak_action(DeltaMassPeak peak)
        {
            actions.Add("unaccept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString());
        }

        public static void shift_peak_action(DeltaMassPeak peak)
        {
            actions.Add("shift " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString() + " by " + peak.mass_shifter);
        }

        public static void update_files_from_presets(bool add_files)
        {
            if (add_files)
            {
                Regex findaddfile = new Regex(@"add file (\S+)");
                Regex findpurpose = new Regex(@"purpose (.+)");
                foreach (string add_file in actions.Where(x => x.StartsWith("add file ")))
                {
                    string filepath = findaddfile.Match(add_file).Groups[1].ToString();
                    Purpose? purpose = ExtensionMethods.EnumUntil.GetValues<Purpose?>().FirstOrDefault(p => findpurpose.Match(add_file).Groups[1].ToString() == p.ToString());
                    if (purpose == null || !File.Exists(filepath))
                        continue;
                    string ext = Path.GetExtension(filepath);
                    lollipop.enter_input_files(new string[] { filepath }, new string[] { ext }, new List<Purpose> { (Purpose)purpose }, lollipop.input_files);
                }
            }

            Regex findchangefile = new Regex(@"change file (\S+)");
            Regex findproperty = new Regex(@" property (\S+)");
            Regex findtype = new Regex(@" type (\S+)");
            Regex findto = new Regex(@" to (.+)"); // matches to end of line
            foreach (string change_file in actions.Where(x => x.StartsWith("change file ")))
            {
                string filename = Path.GetFileName(findchangefile.Match(change_file).Groups[1].ToString());
                string property = findproperty.Match(change_file).Groups[1].ToString();
                string typefullname = findtype.Match(change_file).Groups[1].ToString();
                string value = findto.Match(change_file).Groups[1].ToString();
                InputFile file = lollipop.input_files.FirstOrDefault(f => f.filename == filename); //match the filename, not the path, in case it changed folders
                PropertyInfo propertyinfo = typeof(InputFile).GetProperties().FirstOrDefault(p => p.Name == property);
                Type type = Type.GetType(typefullname);

                if (file == null || propertyinfo == null)
                    continue;
                propertyinfo.SetValue(file, Convert.ChangeType(value, type));
            }
        }

        private static Regex findrelationtype = new Regex(@"shift (\S+)");
        private static Regex findmass = new Regex(@"mass (\S+)");
        public static void mass_shifts_from_presets()
        {
            
            Regex findshift = new Regex(@" by (.+)");
            foreach (string mass_shift in actions.Where(x => x.StartsWith("shift ")))
            {
                ProteoformComparison? comparison = ExtensionMethods.EnumUntil.GetValues<ProteoformComparison?>().FirstOrDefault(x => findrelationtype.Match(mass_shift).Groups[1].ToString() == x.ToString());
                bool converted = Double.TryParse(findmass.Match(mass_shift).Groups[1].ToString(), out double mass);
                if (comparison == null || !converted)
                    continue;
                string shift = findshift.Match(mass_shift).Groups[1].ToString();
                DeltaMassPeak peak = null;
                if (comparison == ProteoformComparison.ExperimentalTheoretical)
                    peak = lollipop.et_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 4) == Math.Round(mass, 4));
                else if (comparison == ProteoformComparison.ExperimentalExperimental)
                    peak = lollipop.ee_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 4) == Math.Round(mass, 4));
                if (peak != null)
                    peak.mass_shifter = shift;
            }
        }

        public static void update_peaks_from_presets()
        {
            foreach (string peak_change in actions.Where(x => x.StartsWith("accept ") || x.StartsWith("unaccept ")))
            {
                ProteoformComparison? comparison = ExtensionMethods.EnumUntil.GetValues<ProteoformComparison?>().FirstOrDefault(x => findrelationtype.Match(peak_change).Groups[1].ToString() == x.ToString());
                bool converted = Double.TryParse(findmass.Match(peak_change).Groups[1].ToString(), out double mass);
                if (comparison == null || !converted)
                    continue;
                DeltaMassPeak peak = null;
                if (comparison == ProteoformComparison.ExperimentalTheoretical)
                    peak = lollipop.et_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 4) == Math.Round(mass, 4));
                else if (comparison == ProteoformComparison.ExperimentalExperimental)
                    peak = lollipop.ee_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 4) == Math.Round(mass, 4));
                if (peak != null)
                    lollipop.change_peak_acceptance(peak, peak_change.StartsWith("accept "));
            }
        }

        #endregion Public Action Methods
    }
}