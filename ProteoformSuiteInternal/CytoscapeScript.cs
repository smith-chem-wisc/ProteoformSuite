using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProteoformSuiteInternal
{
    public class CytoscapeScript
    {
        public string time_stamp;
        public string node_table = "";
        public string edge_table = "";
        public string style_xml = "";
        public string script = "";
        public string nodes_path;
        public string edges_path;
        public string styles_path;
        public string script_path;
        public string style_name;

        public CytoscapeScript(List<ProteoformFamily> families, string time_stamp)
        {
            this.time_stamp = time_stamp;
            this.nodes_path = Lollipop.family_build_folder_path + "\\cytoscape_nodes_" + time_stamp + ".tsv";
            this.edges_path = Lollipop.family_build_folder_path + "\\cytoscape_edges_" + time_stamp + ".tsv";
            this.styles_path = Lollipop.family_build_folder_path + "\\cytoscape_style_" + time_stamp + ".xml";
            this.script_path = Lollipop.family_build_folder_path + "\\cytoscape_script_" + time_stamp + ".txt";
            this.style_name = "ProteoformFamilies" + time_stamp;

            this.script = get_script(families.Sum(f => f.proteoforms.Count() + f.relation_count));
            this.node_table = get_cytoscape_nodes_tsv(families);
            this.edge_table = get_cytoscape_edges_tsv(families);
        }

        //CYTOSCAPE SCRIPT
        private string get_script(int feature_count)
        {
            double sleep_factor = feature_count / 1000;
            return String.Join(Environment.NewLine, new string[] {

                //Load Tables
                "network import file file=\"" + this.edges_path + "\" firstRowAsColumnNames=true delimiters=\"\\t\" indexColumnSourceInteraction=\"1\" indexColumnTargetInteraction=\"3\" startLoadRow=\"0\" dataTypeList=\"s,s,s,s\"",
                "command sleep duration=" + (1.0 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                "table import file file =\"" + this.nodes_path + "\" startLoadRow=\"0\" keyColumnIndex=\"1\" DataTypeTargetForNetworkCollection=\"Node Table Columns\" dataTypeList=\"s,s,i\"",
                "command sleep duration=" + (1.0 + Math.Round((0.2 * sleep_factor), 2)).ToString(),


                //Load Settings
                "vizmap load file file=\"" + this.styles_path + "\"",
                "command sleep duration=0.5",
                "vizmap apply styles=\"" + this.style_name + "\"",
                "command sleep duration=" + (1.0 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                "layout degree-circle",
                "command sleep duration=" + (1.0 + Math.Round((0.5 * sleep_factor), 2)).ToString(),
                "view fit content"
            });
        }

        //CYTOSCAPE NODE AND EDGE TABLES
        public static string lysine_count_header = "lysine_ct";
        public static string delta_mass_header = "delta_mass";
        public static string proteoform_type_header = "E_or_T";
        public static string observation_count_header = "observations";
        public static string experimental_label = "exp";
        public static string unmodified_theoretical_label = "theo";
        public static string unmodified_td_label = "td";
        public static string modified_td_label = "td_ptm";
        public static string modified_theoretical_label = "ptm";
        public string get_cytoscape_edges_tsv(List<ProteoformFamily> families)
        {
            string tsv_header = "accession_1\t" + lysine_count_header + "\taccession_2\t" + delta_mass_header;
            string edge_rows = "";
            foreach (ProteoformRelation r in families.SelectMany(f => f.relations))
            {
                double mass_label = r.peak_center_deltaM;
                if (r.relation_type == ProteoformComparison.etd) mass_label = r.delta_mass;
                edge_rows += String.Join("\t", new List<string>
                {
                    get_proteoform_shared_name(r.connected_proteoforms[0]), r.lysine_count.ToString(), get_proteoform_shared_name(r.connected_proteoforms[1]), Math.Round(mass_label, 2).ToString()
                });
                edge_rows += Environment.NewLine;
            }
            return tsv_header + Environment.NewLine + edge_rows;
        }

        private string get_proteoform_shared_name(Proteoform p)
        {
            string result;
            if (p is ExperimentalProteoform) result = p.accession + "_" + Math.Round(((ExperimentalProteoform)p).agg_mass, Lollipop.deltaM_edge_display_rounding);
            else if (p is TheoreticalProteoform)
            {
                if (!Lollipop.use_gene_ID)
                {
                    result = ((TheoreticalProteoform)p).accession + "_" + ((TheoreticalProteoform)p).ptm_list_string();
                }
                else
                {
                    result = ((TheoreticalProteoform)p).gene_id + "_" + ((TheoreticalProteoform)p).ptm_list_string();
                }
            }
            else if ( p is TopDownProteoform)
            {
                result = ((TopDownProteoform)p).accession + "_" + ((TopDownProteoform)p).ptm_list_string();
            }
            else result = p.accession;
            return result;
        }

        public string get_cytoscape_nodes_tsv(List<ProteoformFamily> families)
        {
            string tsv_header = "accession\t" + proteoform_type_header + "\t" + observation_count_header;
            string node_rows = "";
            foreach (ExperimentalProteoform p in families.SelectMany(f => f.experimental_proteoforms))
            {
                string node_type = experimental_label;
                string observations = ((ExperimentalProteoform)p).aggregated_components.Count.ToString();
                node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p), node_type, observations }) + Environment.NewLine;
            }
            int average_node_size  = Convert.ToInt16(families.Average(f => f.experimental_proteoforms.Average(e => e.observation_count))); 
            foreach (TheoreticalProteoform p in families.SelectMany(f => f.theoretical_proteoforms))
            {
                string node_type = unmodified_theoretical_label;
                if (p.ptm_list.Count > 0) node_type = modified_theoretical_label;
                string observations = "" + average_node_size; //set all theoretical proteoforms with observations=20 for node sizing purposes
                node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p), node_type, observations }) + Environment.NewLine;
            }
            foreach (TopDownProteoform p in families.SelectMany(f => f.topdown_proteoforms))
            {
                string node_type = unmodified_td_label;
                if (p.ptm_list.Count > 0) node_type = modified_td_label;
                string observations = "" + average_node_size;
                node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p), node_type, observations }) + Environment.NewLine;
            }
            return tsv_header + Environment.NewLine + node_rows;
        }

        //CYTOSCAPE STYLES XML
        public string blue_hexadecimal = "#3333FF";
        public string green_hexadecimal = "#00CC00";
        public string red_hexadecimal = "#FF0000";
        public string orange_hexadecimal = "#FF6600";
        public string purple_hexadecimal = "#6B238E";
        public void write_styles()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };

            using (XmlWriter writer = XmlWriter.Create(this.styles_path, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("vizmap");
                writer.WriteAttributeString("documentVersion", "3.0");
                writer.WriteAttributeString("id", this.time_stamp);
                writer.WriteStartElement("visualStyle");
                writer.WriteAttributeString("name", this.style_name);

                //NETWORK PROPERTIES
                writer.WriteStartElement("network");
                IEnumerable<KeyValuePair<string, string>> default_network_styles = default_styles.Where(n => n.Key.StartsWith("NETWORK"));
                foreach (KeyValuePair<string, string> style in default_network_styles)
                {
                    writer.WriteStartElement("visualProperty");
                    writer.WriteAttributeString("name", style.Key);
                    writer.WriteAttributeString("default", style.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                //NODE PROPERTIES
                writer.WriteStartElement("node");
                writer.WriteStartElement("dependency");
                writer.WriteAttributeString("name", "nodeCustomGraphicsSizeSync");
                writer.WriteAttributeString("value", "true");
                writer.WriteEndElement();
                writer.WriteStartElement("dependency");
                writer.WriteAttributeString("name", "nodeSizeLocked");
                writer.WriteAttributeString("value", "true");
                writer.WriteEndElement();
                IEnumerable<KeyValuePair<string, string>> default_node_styles = default_styles.Where(n => n.Key.StartsWith("NODE"));
                foreach (KeyValuePair<string, string> style in default_node_styles)
                {
                    writer.WriteStartElement("visualProperty");
                    writer.WriteAttributeString("name", style.Key);
                    writer.WriteAttributeString("default", style.Value);
                    if (style.Key == "NODE_FILL_COLOR")
                        write_discreteMapping(writer, "string", proteoform_type_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>(red_hexadecimal, unmodified_theoretical_label),
                            new Tuple<string, string>(green_hexadecimal, modified_theoretical_label),
                            new Tuple<string, string>(blue_hexadecimal, experimental_label),
                            new Tuple <string, string> (orange_hexadecimal, unmodified_td_label),
                            new Tuple<string, string> (purple_hexadecimal, modified_td_label)
                        });
                    if (style.Key == "NODE_LABEL_COLOR") write_passthrough(writer, "string", "shared name");
                    if (style.Key == "NODE_LABEL") write_passthrough(writer, "string", "name");
                    if (style.Key == "NODE_SIZE")
                        write_continuousMapping(writer, "float", "observations", new List<Tuple<string, string, string, string>>()
                        {
                            new Tuple<string, string, string, string>("1.0", "20.0", "20.0", "1.0"),
                            new Tuple<string, string, string, string>("300.0", "1.0", "300.0", Lollipop.proteoform_community.families.SelectMany(f => f.experimental_proteoforms).Max(p => p.aggregated_components.Count()).ToString() + ".0") //max node size should be set to the max number of observations
                        });
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                //EDGE PROPERTIES
                writer.WriteStartElement("edge");
                writer.WriteStartElement("dependency");
                writer.WriteAttributeString("name", "arrowColorMatchesEdge");
                writer.WriteAttributeString("value", "false");
                writer.WriteEndElement();
                IEnumerable<KeyValuePair<string, string>> default_edge_styles = default_styles.Where(n => n.Key.StartsWith("EDGE"));
                foreach (KeyValuePair<string, string> style in default_edge_styles)
                {
                    writer.WriteStartElement("visualProperty");
                    writer.WriteAttributeString("name", style.Key);
                    writer.WriteAttributeString("default", style.Value);
                    if (style.Key == "EDGE_LABEL") write_passthrough(writer, "string", delta_mass_header);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void write_passthrough(XmlWriter writer, string attribute_type, string attribute_name)
        {
            writer.WriteStartElement("passthroughMapping");
            writer.WriteAttributeString("attributeType", attribute_type);
            writer.WriteAttributeString("attributeName", attribute_name);
            writer.WriteEndElement();
        }

        private void write_discreteMapping(XmlWriter writer, string attribute_type, string attribute_name, List<Tuple<string, string>> entries)
        {
            writer.WriteStartElement("discreteMapping");
            writer.WriteAttributeString("attributeType", attribute_type);
            writer.WriteAttributeString("attributeName", attribute_name);
            foreach (Tuple<string, string> v in entries)
            {
                writer.WriteStartElement("discreteMappingEntry");
                writer.WriteAttributeString("value", v.Item1);
                writer.WriteAttributeString("attributeValue", v.Item2);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void write_continuousMapping(XmlWriter writer, string attribute_type, string attribute_name, List<Tuple<string, string, string, string>> points)
        {
            writer.WriteStartElement("continuousMapping");
            writer.WriteAttributeString("attributeType", attribute_type);
            writer.WriteAttributeString("attributeName", attribute_name);
            foreach(Tuple<string, string, string, string> p in points)
            {
                writer.WriteStartElement("continuousMappingPoint");
                writer.WriteAttributeString("lesserValue", p.Item1);
                writer.WriteAttributeString("greaterValue", p.Item2);
                writer.WriteAttributeString("equalValue", p.Item3);
                writer.WriteAttributeString("attrValue", p.Item4);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        //These are the default styles associated with the "Sample1" style in Cytoscape
        private Dictionary<string, string> default_styles = new Dictionary<string, string>()
        {
            //DEFAULT NETWORK STYLES 
            { "NETWORK_WIDTH", "550.0"},
            { "NETWORK_EDGE_SELECTION", "true"},
            { "NETWORK_TITLE", ""},
            { "NETWORK_CENTER_Z_LOCATION", "0.0"},
            { "NETWORK_NODE_SELECTION", "true"},
            { "NETWORK_CENTER_X_LOCATION", "0.0"},
            { "NETWORK_HEIGHT", "400.0"},
            { "NETWORK_BACKGROUND_PAINT", "#FFFFFF"},
            { "NETWORK_DEPTH", "0.0"},
            { "NETWORK_SCALE_FACTOR", "1.0"},
            { "NETWORK_SIZE", "550.0"},
            { "NETWORK_CENTER_Y_LOCATION", "0.0"},

            //DEFAULT NODE STYLES
            { "NODE_LABEL_POSITION", "C,C,c,0.00,0.00"},
            { "NODE_CUSTOMPAINT_7", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_7, name=Node Custom Paint 7)"},
            { "NODE_CUSTOMGRAPHICS_7", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_CUSTOMGRAPHICS_SIZE_1", "50.0"},
            { "NODE_BORDER_PAINT", "#000000"},
            { "NODE_BORDER_TRANSPARENCY", "255"},
            { "NODE_CUSTOMGRAPHICS_POSITION_2", "C,C,c,0.00,0.00"},
            { "NODE_CUSTOMPAINT_5", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_5, name=Node Custom Paint 5)"},
            { "COMPOUND_NODE_PADDING", "10.0"},
            { "COMPOUND_NODE_SHAPE", "ROUND_RECTANGLE"},
            { "NODE_CUSTOMGRAPHICS_POSITION_9", "C,C,c,0.00,0.00"},
            { "NODE_SHAPE", "ELLIPSE"},
            { "NODE_CUSTOMGRAPHICS_POSITION_1", "C,C,c,0.00,0.00"},
            { "NODE_LABEL_FONT_FACE", "Dialog.plain,plain,12"},
            { "NODE_CUSTOMGRAPHICS_2", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_CUSTOMGRAPHICS_9", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_TRANSPARENCY", "255"},
            { "NODE_CUSTOMGRAPHICS_POSITION_4", "C,C,c,0.00,0.00"},
            { "NODE_CUSTOMGRAPHICS_1", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_CUSTOMGRAPHICS_SIZE_2", "50.0"},
            { "NODE_CUSTOMPAINT_1", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_1, name=Node Custom Paint 1)"},
            { "NODE_HEIGHT", "30.0"},
            { "NODE_SELECTED_PAINT", "#FFFF00"},
            { "NODE_CUSTOMGRAPHICS_8", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_CUSTOMPAINT_4", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_4, name=Node Custom Paint 4)"},
            { "NODE_CUSTOMGRAPHICS_SIZE_7", "50.0"},
            { "NODE_WIDTH", "70.0"},
            { "NODE_X_LOCATION", "0.0"},
            { "NODE_FILL_COLOR", "#CCCCFF"},
            { "NODE_CUSTOMGRAPHICS_SIZE_5", "50.0"},
            { "NODE_TOOLTIP", ""},
            { "NODE_CUSTOMGRAPHICS_SIZE_6", "50.0"},
            { "NODE_BORDER_STROKE", "SOLID"},
            { "NODE_CUSTOMGRAPHICS_SIZE_3", "50.0"},
            { "NODE_SELECTED", "false"},
            { "NODE_LABEL_COLOR", "#000000"},
            { "NODE_CUSTOMGRAPHICS_4", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_VISIBLE", "true"},
            { "NODE_CUSTOMGRAPHICS_SIZE_9", "50.0"},
            { "NODE_CUSTOMPAINT_9", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_9, name=Node Custom Paint 9)"},
            { "NODE_CUSTOMGRAPHICS_3", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_Z_LOCATION", "0.0"},
            { "NODE_LABEL_TRANSPARENCY", "255"},
            { "NODE_LABEL_FONT_SIZE", "12"},
            { "NODE_CUSTOMGRAPHICS_POSITION_6", "C,C,c,0.00,0.00"},
            { "NODE_LABEL", ""},
            { "NODE_CUSTOMGRAPHICS_POSITION_8", "C,C,c,0.00,0.00"},
            { "NODE_CUSTOMGRAPHICS_5", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_CUSTOMGRAPHICS_POSITION_3", "C,C,c,0.00,0.00"},
            { "NODE_PAINT", "#787878"},
            { "NODE_NESTED_NETWORK_IMAGE_VISIBLE", "true"},
            { "NODE_CUSTOMPAINT_8", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_8, name=Node Custom Paint 8)"},
            { "NODE_LABEL_WIDTH", "200.0"},
            { "NODE_DEPTH", "0.0"},
            { "NODE_CUSTOMGRAPHICS_6", "org.cytoscape.ding.customgraphics.NullCustomGraphics,0,[ Remove Graphics ],"},
            { "NODE_SIZE", "40.0"},
            { "NODE_Y_LOCATION", "0.0"},
            { "NODE_BORDER_WIDTH", "0.0"},
            { "NODE_CUSTOMGRAPHICS_POSITION_7", "C,C,c,0.00,0.00"},
            { "NODE_CUSTOMGRAPHICS_SIZE_4", "50.0"},
            { "NODE_CUSTOMPAINT_2", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_2, name=Node Custom Paint 2)"},
            { "NODE_CUSTOMGRAPHICS_SIZE_8", "50.0"},
            { "NODE_CUSTOMPAINT_3", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_3, name=Node Custom Paint 3)"},
            { "NODE_CUSTOMPAINT_6", "DefaultVisualizableVisualProperty(id=NODE_CUSTOMPAINT_3, name=Node Custom Paint 6)"},
            { "NODE_CUSTOMGRAPHICS_POSITION_5", "C,C,c,0.00,0.00"},

            //DEFAULT EDGE STYLES
            { "EDGE_SOURCE_ARROW_SHAPE", "NONE"},
            { "EDGE_SOURCE_ARROW_UNSELECTED_PAINT", "#000000"},
            { "EDGE_LABEL_TRANSPARENCY", "255"},
            { "EDGE_LINE_TYPE", "SOLID"},
            { "EDGE_SOURCE_ARROW_SELECTED_PAINT", "#FFFF00"},
            { "EDGE_TARGET_ARROW_SHAPE", "NONE"},
            { "EDGE_STROKE_SELECTED_PAINT", "#FF0000"},
            { "EDGE_LABEL", ""},
            { "EDGE_TOOLTIP", ""},
            { "EDGE_STROKE_UNSELECTED_PAINT", "#333333"},
            { "EDGE_LABEL_FONT_FACE", "Dialog.plain,plain,10"},
            { "EDGE_LABEL_FONT_SIZE", "15"},
            { "EDGE_TARGET_ARROW_SELECTED_PAINT", "#FFFF00"},
            { "EDGE_LABEL_COLOR", "#333333"},
            { "EDGE_PAINT", "#323232"},
            { "EDGE_SELECTED_PAINT", "#FF0000"},
            { "EDGE_BEND", ""},
            { "EDGE_TRANSPARENCY", "255"},
            { "EDGE_SELECTED", "false"},
            { "EDGE_UNSELECTED_PAINT", "#404040"},
            { "EDGE_CURVED", "true"},
            { "EDGE_LABEL_WIDTH", "200.0"},
            { "EDGE_VISIBLE", "true"},
            { "EDGE_WIDTH", "1.0"},
            { "EDGE_TARGET_ARROW_UNSELECTED_PAINT", "#000000"}
        };
    }
}
