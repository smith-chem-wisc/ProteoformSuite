using NetSerializer;
using Proteomics;
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
    public class SaveState
    {

        public static Lollipop lollipop = new Lollipop();

        #region Private Field

        private static Serializer ser = new Serializer(new Type[]
        {
            typeof(Lollipop),
            typeof(ComponentReader), // not serialized currently because InputFile -> ComponentReader -> Component -> InputFile messes things up, and it's only used in loading components
            typeof(ProteinSequenceGroup),
            typeof(TheoreticalProteoformGroup),
            typeof(Protein),
            typeof(ModificationWithLocation),
            typeof(ModificationWithMass),
            typeof(ModificationWithMassAndCf),
            typeof(List<DatabaseReference>),
            typeof(List<Tuple<string,string>>),
            typeof(Dictionary<int, List<Modification>>),
            typeof(Dictionary<string, IList<string>>),
            typeof(List<ProteolysisProduct>),
            typeof(ChemicalFormulaTerminus),
            typeof(List<double>)
        });

        #endregion Private Field

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

        public static void open_method(string text)
        {
            FieldInfo[] lollipop_fields = typeof(Lollipop).GetFields();
            List<XElement> settings = new List<XElement>();
            using (XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "setting") settings.Add(XElement.ReadFrom(reader) as XElement);
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
        }

        public static void open_method(string[] lines)
        {
            open_method(String.Join(Environment.NewLine, lines));
        }

        #endregion METHOD SAVE/LOAD

    }
}
