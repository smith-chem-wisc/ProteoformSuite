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
    public static class Sweet
    {
        public static Lollipop lollipop = new Lollipop();
        public static string methodFilePath = "";
        public static List<string> save_actions = new List<string>();
        public static List<string> loaded_actions = new List<string>();

        #region BASICS FOR XML WRITING

        public static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
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
            foreach (string action in save_actions)
            {
                writer.WriteStartElement("action");
                writer.WriteAttributeString("action", action);
                writer.WriteEndElement();
            }
        }

        public static bool open_method(string methodFilePath, string alltext, bool add_files, out string warning_message)
        {
            Sweet.methodFilePath = methodFilePath;
            warning_message = "";
            loaded_actions.Clear();
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
                        {
                            setting_elements.Add(XElement.ReadFrom(reader) as XElement);
                        }
                        else if (reader.Name == "action")
                        {
                            action_elements.Add(XElement.ReadFrom(reader) as XElement);
                        }
                        else return false; //unknown element
                    }
                }
            }

            foreach (XElement setting in setting_elements)
            {
                string type_string = GetAttribute(setting, "field_type");
                Type type = Type.GetType(type_string); //Takes only full name of type
                string name = GetAttribute(setting, "field_name");
                string value = GetAttribute(setting, "field_value");
                FieldInfo field = lollipop_fields.FirstOrDefault(p => p.Name == name);

                if (field == null)
                {
                    warning_message += "Setting " + name + " has changed, and it was not changed to preset " + type_string + " " + value + " in the current run" + Environment.NewLine;
                    continue;
                }

                field.SetValue(lollipop, Convert.ChangeType(value, type));
            }

            List<FieldInfo> fields = lollipop_fields.Where(f => !f.IsLiteral &&
                   (f.FieldType == typeof(int) ||
                   f.FieldType == typeof(double) ||
                   f.FieldType == typeof(string) ||
                   f.FieldType == typeof(decimal) ||
                   f.FieldType == typeof(bool))
                   && !setting_elements.Any(s => GetAttribute(s, "field_name") == f.Name)).ToList();
            if (fields.Count > 0)
            {
                warning_message += "The following parameters did not have a setting specified: " + String.Join(", ", fields.Select(f => f.Name)) + Environment.NewLine;
            }
            lollipop.results_folder = !lollipop.results_folder.StartsWith(".") ? lollipop.results_folder : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(methodFilePath), lollipop.results_folder));

            foreach (XElement action in action_elements)
            {
                loaded_actions.Add(GetAttribute(action, "action"));
            }

            if (loaded_actions.Any(a => !a.StartsWith("add file ") && !a.StartsWith("change file ") && !a.StartsWith("shift ") && !a.StartsWith("accept ") && !a.StartsWith("unaccept ")))
            {
                return false;
            }

            if (add_files) { add_files_from_presets(lollipop.input_files); }
            else { update_files_from_presets(lollipop.input_files); }
            return true;
        }

        #endregion METHOD SAVE/LOAD

        #region Public Action Methods

        public static void add_file_action(InputFile file)
        {
            save_actions.Add("add file '" + file.complete_path + "' with purpose " + file.purpose.ToString());
        }

        public static void change_file(InputFile file, object property, string property_name, string from, string to)
        {
            save_actions.Add("change file '" + file.complete_path + "' with purpose " + file.purpose + " property " + property_name + " of type " + property.GetType().FullName + " from " + from + " to " + to);
        }

        public static void accept_peak_action(IMassDifference peak)
        {
            save_actions.Add("accept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString());
        }

        public static void unaccept_peak_action(IMassDifference peak)
        {
            save_actions.Add("unaccept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString());
        }

        public static void shift_peak_action(DeltaMassPeak peak)
        {
            save_actions.Add("shift " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString() + " by " + peak.mass_shifter);
        }

        public static void add_files_from_presets(List<InputFile> destination)
        {
            Regex findaddfile = new Regex(@"(add file ')(.+)(' with purpose )");
            Regex findpurpose = new Regex(@"( purpose )(.+)");
            foreach (string add_file in loaded_actions.Where(x => x.StartsWith("add file ")))
            {
                string filestring = findaddfile.Match(add_file).Groups[2].ToString();
                string filepath = !filestring.StartsWith(".") ? filestring : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(methodFilePath), filestring));
                Purpose? purpose = ExtensionMethods.EnumUntil.GetValues<Purpose>().FirstOrDefault(p => findpurpose.Match(add_file).Groups[2].ToString() == p.ToString());
                if (purpose == null || !File.Exists(filepath))
                {
                    continue;
                }
                string ext = Path.GetExtension(filepath);
                lollipop.enter_input_files(new[] { filepath }, new[] { ext }, new List<Purpose> { (Purpose)purpose }, destination, false);
                if (!save_actions.Contains(add_file)) save_actions.Add(add_file);
            }
        }

        public static void update_files_from_presets(List<InputFile> destination)
        {
            Regex findchangefile = new Regex(@"(change file ')(.+)(' with purpose )");
            Regex findpurpose = new Regex(@"( purpose )(.+)( property )");
            Regex findproperty = new Regex(@"( property )(\S+)");
            Regex findtype = new Regex(@"( type )(\S+)");
            Regex findto = new Regex(@"( to )(.+)"); // matches to end of line
            foreach (string change_file in loaded_actions.Where(x => x.StartsWith("change file ")))
            {
                string filestring = findchangefile.Match(change_file).Groups[2].ToString();
                string filepath = !filestring.StartsWith(".") ? filestring : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(methodFilePath), filestring));
                string filename = Path.GetFileNameWithoutExtension(filepath);
                string property = findproperty.Match(change_file).Groups[2].ToString();
                string typefullname = findtype.Match(change_file).Groups[2].ToString();
                string value = findto.Match(change_file).Groups[2].ToString();
                Purpose? purpose = ExtensionMethods.EnumUntil.GetValues<Purpose>().FirstOrDefault(p => findpurpose.Match(change_file).Groups[2].ToString() == p.ToString());
                InputFile file = destination.FirstOrDefault(f => (f.complete_path == filepath || f.filename == filename) && f.purpose == purpose); //match the filename, not the path, in case it changed folders
                PropertyInfo propertyinfo = typeof(InputFile).GetProperties().FirstOrDefault(p => p.Name == property);
                Type type = Type.GetType(typefullname);

                if (file == null || propertyinfo == null)
                {
                    continue;
                }
                propertyinfo.SetValue(file, Convert.ChangeType(value, type));
                if (!save_actions.Contains(change_file)) save_actions.Add(change_file);
            }
        }

        private static Regex findmass = new Regex(@"(mass )(\S+)");

        public static void mass_shifts_from_presets()
        {
            Regex findshift = new Regex(@"( by )(.+)");
            Regex findshiftrelationtype = new Regex(@"(shift )(\S+)");
            foreach (string mass_shift in loaded_actions.Where(x => x.StartsWith("shift ")))
            {
                string relationshiptype = findshiftrelationtype.Match(mass_shift).Groups[2].ToString();
                ProteoformComparison? comparison = ExtensionMethods.EnumUntil.GetValues<ProteoformComparison>().FirstOrDefault(x => relationshiptype == x.ToString());
                bool converted = Double.TryParse(findmass.Match(mass_shift).Groups[2].ToString(), out double mass);
                if (comparison == null || !converted)
                {
                    continue;
                }
                string shift = findshift.Match(mass_shift).Groups[2].ToString();
                DeltaMassPeak peak = null;
                if (comparison == ProteoformComparison.ExperimentalTheoretical)
                {
                    peak = lollipop.et_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 2) == Math.Round(mass, 2));
                }
                if (peak != null)
                {
                    peak.mass_shifter = shift;
                    save_actions.Add(mass_shift);
                }
            }
        }

        public static void update_peaks_from_presets(ProteoformComparison comparison_to_update)
        {
            Regex findacceptrelationtype = new Regex(@"(accept )(\S+)");
            foreach (string peak_change in loaded_actions.Where(x => x.StartsWith("accept ") || x.StartsWith("unaccept ")))
            {
                string relationshiptype = findacceptrelationtype.Match(peak_change).Groups[2].ToString();
                ProteoformComparison? comparison = ExtensionMethods.EnumUntil.GetValues<ProteoformComparison>().FirstOrDefault(x => relationshiptype == x.ToString());
                bool converted = Double.TryParse(findmass.Match(peak_change).Groups[2].ToString(), out double mass);
                if (comparison == null || comparison != comparison_to_update || !converted)
                {
                    continue;
                }
                DeltaMassPeak peak = null;
                if (comparison == ProteoformComparison.ExperimentalTheoretical)
                {
                    peak = lollipop.et_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 2) == Math.Round(mass, 2));
                }
                else if (comparison == ProteoformComparison.ExperimentalExperimental)
                {
                    peak = lollipop.ee_peaks.FirstOrDefault(p => Math.Round(p.DeltaMass, 2) == Math.Round(mass, 2));
                }
                if (peak != null)
                {
                    lollipop.change_peak_acceptance(peak, peak_change.StartsWith("accept "), false);
                    save_actions.Add(peak_change);
                }
            }
        }

        #endregion Public Action Methods
    }
}