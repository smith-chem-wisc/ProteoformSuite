using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ProteoformSuiteInternal
{
    public class Sweet
    {

        public static Lollipop lollipop = new Lollipop();
        public static List<Tuple<object, string>> actions = new List<Tuple<object, string>>();

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
            foreach (Tuple<object, string> action in actions)
            {
                writer.WriteStartElement("action");
                writer.WriteAttributeString("action_type", action.Item1.GetType().FullName);
                writer.WriteAttributeString("action", action.Item2);
                writer.WriteEndElement();
            }
        }

        public static void open_method(string text)
        {
            FieldInfo[] lollipop_fields = typeof(Lollipop).GetFields();
            List<XElement> settings = new List<XElement>();
            List<XElement> actions = new List<XElement>();
            using (XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "setting")
                            settings.Add(XElement.ReadFrom(reader) as XElement);
                        else if (new string[] { "input_file", "delta_mass_peak"}.Contains(reader.Name))
                            actions.Add(XElement.ReadFrom(reader) as XElement);
                        else return; //Settings come first. Return when done
                    }
                }
            }

            foreach (XElement setting in settings)
            {
                string type_string = GetAttribute(setting, "field_type");
                Type type = Type.GetType(type_string); //Takes only full name of type
                string name = GetAttribute(setting, "field_name");
                string value = GetAttribute(setting, "field_value");
                lollipop_fields.FirstOrDefault(p => p.Name == name).SetValue(lollipop, Convert.ChangeType(value, type));
            }

            foreach (XElement preset in actions)
            {
                string preset_type = preset.Name.LocalName;
                if (preset_type == "input_file")
                {
                    string type_string = GetAttribute(preset, "property_type");
                    Type type = Type.GetType(type_string); //Takes only full name of type
                    string name = GetAttribute(preset, "property_name");
                    string value = GetAttribute(preset, "property_value");
                    lollipop_fields.FirstOrDefault(p => p.Name == name).SetValue(lollipop, Convert.ChangeType(value, type));
                }
                
            }
        }

        public static void open_method(string[] lines)
        {
            open_method(String.Join(Environment.NewLine, lines));
        }

        #endregion METHOD SAVE/LOAD

        #region Public Action Methods

        public static void add_file_action(InputFile file)
        {
            actions.Add(new Tuple<object, string>(file, "add file " + file.complete_path + " with purpose " + file.purpose.ToString()));
        }

        public static void change_file(InputFile file, string property, string from, string to)
        {
            actions.Add(new Tuple<object, string>(file, "change file " + property + " from " + from + " to " + to));
        }

        public static void accept_peak_action(DeltaMassPeak peak)
        {
            actions.Add(new Tuple<object, string>(peak, "accept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString()));
        }

        public static void unaccept_peak_action(DeltaMassPeak peak)
        {
            actions.Add(new Tuple<object, string>(peak, "unaccept " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString()));
        }

        public static void shift_peak_action(DeltaMassPeak peak)
        {
            actions.Add(new Tuple<object, string>(peak, "shift " + peak.RelationType.ToString() + " peak with delta-mass " + peak.DeltaMass.ToString() + " by " + peak.mass_shifter));
        }

        #endregion Public Action Methods
    }
}