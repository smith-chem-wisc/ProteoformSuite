using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Proteomics;
using System.IO;

namespace ProteoformSuiteInternal
{
    public static class CytoscapeScript
    {
        public static string write_cytoscape_script(List<ProteoformFamily> families, List<ProteoformFamily> all_families, string folder_path, string time_stamp, bool quantitative, bool quantitative_redBorder, bool quantitative_boldFace, bool quantitative_moreOpacity,
            string color_scheme, string node_label_position, int double_rounding)
        {
            return write_script(families, all_families, folder_path, time_stamp, quantitative, quantitative_redBorder, quantitative_boldFace, quantitative_moreOpacity, color_scheme, node_label_position, double_rounding);
        }

        public static string write_cytoscape_script(object[] stuff, List<ProteoformFamily> all_families, string folder_path, string time_stamp, bool quantitative, bool quantitative_redBorder, bool quantitative_boldFace, bool quantitative_moreOpacity,
            string color_scheme, string node_label_position, int double_rounding)
        {
            List<ProteoformFamily> families = stuff.OfType<ProteoformFamily>().ToList();

            if (stuff.Length <= 0) return "No objects were selected";
            if (families.Count <= 0 && typeof(TheoreticalProteoform).IsAssignableFrom(stuff[0].GetType()))
                families = get_families(stuff.OfType<TheoreticalProteoform>(), all_families).ToList();
            if (families.Count <= 0 && typeof(GoTerm).IsAssignableFrom(stuff[0].GetType()))
                families = get_families(stuff.OfType<GoTerm>(), all_families).ToList();
            if (families.Count <= 0 && typeof(ExperimentalProteoform.quantitativeValues).IsAssignableFrom(stuff[0].GetType()))
                families = get_families(stuff.OfType<ExperimentalProteoform.quantitativeValues>(), all_families).ToList();
            if (families.Count <= 0) return "Selected objects were not recognized.";

            return write_script(families, all_families, folder_path, time_stamp, quantitative, quantitative_redBorder, quantitative_boldFace, quantitative_moreOpacity, color_scheme, node_label_position, double_rounding);
        }

        private static IEnumerable<ProteoformFamily> get_families(IEnumerable<TheoreticalProteoform> theoreticals, List<ProteoformFamily> all_families)
        {
            return from f in all_families
                   from t in f.theoretical_proteoforms
                   where theoreticals.Contains(t)
                   select f;
        }

        private static IEnumerable<ProteoformFamily> get_families(IEnumerable<GoTerm> go_terms, List<ProteoformFamily> all_families)
        {
            return from f in all_families
                   from t in f.theoretical_proteoforms
                   from p in t.proteinList
                   from g in p.GoTerms
                   where go_terms.Contains(g)
                   select f;
        }

        private static IEnumerable<ProteoformFamily> get_families(IEnumerable<ExperimentalProteoform.quantitativeValues> qvals, List<ProteoformFamily> all_families)
        {
            return from f in all_families
                   from e in f.experimental_proteoforms
                   where qvals.Contains(e.quant)
                   select f;
        }

        public static string node_file_prefix = "cytoscape_nodes_";
        public static string edge_file_prefix = "cytoscape_edges_";
        public static string style_file_prefix = "cytoscape_style_";
        public static string script_file_prefix = "cytoscape_script_";
        public static string node_file_extension = ".tsv";
        public static string edge_file_extension = ".tsv";
        public static string style_file_extension = ".xml";
        public static string script_file_extension = ".txt";
        private static string write_script(List<ProteoformFamily> families, List<ProteoformFamily> all_families, string folder_path, string time_stamp, bool quantitative, bool quantitative_redBorder, bool quantitative_boldFace, bool quantitative_moreOpacity,
            string color_scheme, string node_label_position, int double_rounding)
        {
            //Check if valid folder
            if (folder_path == "" || !Directory.Exists(folder_path))
                return "Please choose a folder in which the families will be built, so you can load them into Cytoscape.";

            if (families.Any(f => f.experimental_count == 0))
                return "Error: there is a family with zero experimental proteoforms.";

            string nodes_path = Path.Combine(folder_path, node_file_prefix + time_stamp + node_file_extension);
            string edges_path = Path.Combine(folder_path, edge_file_prefix + time_stamp + edge_file_extension);
            string styles_path = Path.Combine(folder_path, style_file_prefix + time_stamp + style_file_extension);
            string script_path = Path.Combine(folder_path, script_file_prefix + time_stamp + script_file_extension);
            string style_name = "ProteoformFamilies" + time_stamp;

            string script = get_script(families.Sum(f => f.proteoforms.Count() + f.relation_count), quantitative, edges_path, nodes_path, styles_path, style_name);
            string node_table = get_cytoscape_nodes_tsv(families, quantitative, color_scheme, double_rounding);
            string edge_table = get_cytoscape_edges_tsv(families, double_rounding);
            File.WriteAllText(edges_path, edge_table);
            File.WriteAllText(nodes_path, node_table);
            File.WriteAllText(script_path, script);
            write_styles(all_families, styles_path, style_name, time_stamp, node_label_position, color_scheme, quantitative, quantitative_redBorder, quantitative_moreOpacity, quantitative_boldFace);

            string selected_family_string = "Finished building selected famil";
            selected_family_string += families.Count == 1 ? "y :" : "ies :#";
            selected_family_string += (families.Count <= 3) ? String.Join(", #", families.Select(f => f.family_id)) : String.Join(", #", families.Select(f => f.family_id).ToList().Take(3)) + ", etc.";
            return selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.";
        }

        //CYTOSCAPE SCRIPT
        private static string get_script(int feature_count, bool quantitative, string edges_path, string nodes_path, string styles_path, string style_name)
        {
            double sleep_factor = feature_count / 1000;
            string node_column_types = quantitative ? "s,s,d,d,d,boolean,s" : "s,s,d"; //Cytoscape bug: "b" doesn't work in 3.4.0, only "boolean" does
            string edge_column_types = "s,s,s,s";
            return String.Join(Environment.NewLine, new string[] {

                //Load Tables
                "network import file file=\"" + edges_path + "\" firstRowAsColumnNames=true delimiters=\"\\t\" indexColumnSourceInteraction=\"1\" indexColumnTargetInteraction=\"3\" startLoadRow=\"0\" dataTypeList=\"" + edge_column_types + "\"",
                "command sleep duration=" + (0.8 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                "table import file file=\"" + nodes_path + "\" startLoadRow=\"0\" keyColumnIndex=\"1\" DataTypeTargetForNetworkCollection=\"Node Table Columns\" dataTypeList=\"" + node_column_types + "\"",
                "command sleep duration=" + (0.5 + Math.Round((0.5 * sleep_factor), 2)).ToString(),


                //Load Settings
                "vizmap load file file=\"" + styles_path + "\"",
                "command sleep duration=0.5",
                "vizmap apply styles=\"" + style_name + "\"",
                "command sleep duration=" + (1.0 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                "layout degree-circle",
                "command sleep duration=" + (0.5 + Math.Round((0.5 * sleep_factor), 2)).ToString(),
                "view fit content"
            });
        }

        //CYTOSCAPE NODE AND EDGE TABLES
        public static string lysine_count_header = "lysine_ct";
        public static string delta_mass_header = "delta_mass";
        public static string proteoform_type_header = "E_or_T";
        public static string size_header = "total_intensity";
        public static string piechart_header = "piechart_graphics";
        public static string significant_header = "significant_difference";
        public static string experimental_label = "experimental";
        public static string experimental_notQuantified_label = "experimental_below_quantification_threshold";
        public static string unmodified_theoretical_label = "theoretical";
        public static string modified_theoretical_label = "modified_theoretical";
        public static string mock_intensity = "20"; //set all theoretical proteoforms with observations=20 for node sizing purposes
        public static string get_cytoscape_edges_tsv(List<ProteoformFamily> families, int double_rounding)
        {
            string tsv_header = "accession_1\t" + lysine_count_header + "\taccession_2\t" + delta_mass_header;
            string edge_rows = "";
            foreach (ProteoformRelation r in families.SelectMany(f => f.relations))
            {
                edge_rows += String.Join("\t", new List<string>
                {
                    get_proteoform_shared_name(r.connected_proteoforms[0], double_rounding),
                    r.lysine_count.ToString(),
                    get_proteoform_shared_name(r.connected_proteoforms[1], double_rounding),
                    Math.Round(r.peak_center_deltaM, double_rounding).ToString("0." + String.Join("", Enumerable.Range(0, double_rounding).Select(i => "0")))
                });
                edge_rows += Environment.NewLine;
            }
            return tsv_header + Environment.NewLine + edge_rows;
        }

        public static string get_cytoscape_nodes_tsv(List<ProteoformFamily> families, bool quantitative, string color_scheme, int double_rounding)
        {
            string tsv_header = "accession\t" + proteoform_type_header + "\t" + size_header;
            if (quantitative) tsv_header += "\t" + Lollipop.numerator_condition + "\t" + Lollipop.denominator_condition + "\t" + significant_header + "\t" + piechart_header;

            string node_rows = "";
            foreach (ExperimentalProteoform p in families.SelectMany(f => f.experimental_proteoforms))
            {
                string node_type = quantitative && p.quant.intensitySum == 0 ? 
                    experimental_notQuantified_label : 
                    experimental_label;
                string total_intensity = quantitative ?
                    p.quant.intensitySum == 0 ? mock_intensity : ((double)p.quant.intensitySum).ToString() : 
                    p.agg_intensity.ToString();
                node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p, double_rounding), node_type, total_intensity });
                if (quantitative && p.quant.intensitySum != 0) node_rows += "\t" + String.Join("\t", new List<string> { ((double)p.quant.lightIntensitySum).ToString(), ((double)p.quant.heavyIntensitySum).ToString(), p.quant.significant.ToString(), get_piechart_string(color_scheme) });
                node_rows += Environment.NewLine;
            }

            foreach (TheoreticalProteoform p in families.SelectMany(f => f.theoretical_proteoforms))
            {
                string node_type = String.Equals(p.ptm_list_string(), "unmodified", StringComparison.CurrentCultureIgnoreCase) ? unmodified_theoretical_label : modified_theoretical_label;
                node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p, double_rounding), node_type, mock_intensity }) + Environment.NewLine;
            }

            return tsv_header + Environment.NewLine + node_rows;
        }

        public static string get_proteoform_shared_name(Proteoform p, int double_rounding)
        {
            if (typeof(ExperimentalProteoform).IsAssignableFrom(p.GetType()))
            {
                return Math.Round(((ExperimentalProteoform)p).agg_mass, double_rounding) + "_Da_" + p.accession;
            }

            else if (typeof(TheoreticalProteoform).IsAssignableFrom(p.GetType()))
            {
                return p.accession.Split(new char[] { '_' }).FirstOrDefault() + " " + ((TheoreticalProteoform)p).ptm_list_string();
            }

            else
            {
                return p.accession;
            }
        }

        private static string get_piechart_string(string color_scheme)
        {
            return "piechart: attributelist = \"" + Lollipop.numerator_condition + "," + Lollipop.denominator_condition +
                "\" colorlist = \"" + color_schemes[color_scheme][0] + "," + color_schemes[color_scheme][3] +
                "\" labellist = \",\"";
        }

        //CYTOSCAPE STYLES XML
        public static string[] color_scheme_names = new string[6] 
        {
            "Licorice",
            "Smarties",
            "Candy Corn",
            "Popsicle",
            "Marshmallow",
            "Wisconsin Schaum Torte"
        };
        //Colors: exp, ptm, theo, pie
        public static Dictionary<string, List<string>> color_schemes = new Dictionary<string, List<string>>
        {
            { color_scheme_names[0], new List<string> { "#3333FF", "#00CC00", "#FF0000", "#FFFF00" } },
            { color_scheme_names[1], new List<string> { "#9886E8", "#97CACB", "#FF77A1", "#FFFFBE" } },
            { color_scheme_names[2], new List<string> { "#2F5E91", "#2D6A00", "#F45512", "#916415" } },
            { color_scheme_names[3], new List<string> { "#5338FF", "#1F8A70", "#FF6533", "#FFE11A" } },
            { color_scheme_names[4], new List<string> { "#A8E1FF", "#B29162", "#B26276", "#FFF08C" } },
            { color_scheme_names[5], new List<string> { "#3D8A99", "#979C9C", "#963C4B", "#F2EBC7" } }
        };
        public static string not_quantified = "#D3D3D3";

        public static string[] node_label_positions = new string[3] 
        {
            "On Node",
            "Above Node",
            "Below Node",
            //"Outside Circle"
        };

        public static void write_styles(List<ProteoformFamily> all_families, string styles_path, string style_name, string time_stamp, string node_label_position, string color_scheme, 
            bool quantitative, bool quantitative_redBorder, bool quantitative_moreOpacity, bool quantitative_boldFace)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };

            using (XmlWriter writer = XmlWriter.Create(styles_path, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("vizmap");
                writer.WriteAttributeString("documentVersion", "3.0");
                writer.WriteAttributeString("id", time_stamp);
                writer.WriteStartElement("visualStyle");
                writer.WriteAttributeString("name", style_name);

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
                double max_total_intensity = quantitative ?
                    (double)all_families.SelectMany(f => f.experimental_proteoforms).Max(p => p.quant.intensitySum) :
                    all_families.SelectMany(f => f.experimental_proteoforms).Max(p => p.agg_intensity);
                writer.WriteStartElement("node");
                writer.WriteStartElement("dependency");
                writer.WriteAttributeString("name", "nodeCustomGraphicsSizeSync");
                writer.WriteAttributeString("value", "true");
                writer.WriteEndElement();
                writer.WriteStartElement("dependency");
                writer.WriteAttributeString("name", "nodeSizeLocked");
                writer.WriteAttributeString("value", "true");
                writer.WriteEndElement();
                IEnumerable<KeyValuePair<string, string>> default_node_styles = default_styles.Where(n => n.Key.StartsWith("NODE") || n.Key.StartsWith("COMPOUND_NODE"));
                foreach (KeyValuePair<string, string> style in default_node_styles)
                {
                    writer.WriteStartElement("visualProperty");
                    writer.WriteAttributeString("name", style.Key);

                    //Defaults
                    if (style.Key == "NODE_LABEL_POSITION")
                    {
                        if (node_label_position == node_label_positions[1]) writer.WriteAttributeString("default", "N,S,c,0.00,0.00");
                        else if (node_label_position == node_label_positions[2]) writer.WriteAttributeString("default", "S,N,c,0.00,0.00");
                        writer.WriteEndElement();
                        continue;
                    }
                    else writer.WriteAttributeString("default", style.Value);

                    //Discrete and continuous mapping
                    if (style.Key == "NODE_FILL_COLOR")
                        write_discreteMapping(writer, "string", proteoform_type_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>(quantitative ? "#FFFFFF" : color_schemes[color_scheme][0], experimental_label),
                            new Tuple<string, string>("#D3D3D3", experimental_notQuantified_label),
                            new Tuple<string, string>(color_schemes[color_scheme][1], modified_theoretical_label),
                            new Tuple<string, string>(color_schemes[color_scheme][2], unmodified_theoretical_label)
                        });
                    if (style.Key == "NODE_LABEL_COLOR") write_passthrough(writer, "string", "shared name");
                    if (style.Key == "NODE_LABEL") write_passthrough(writer, "string", "name");
                    if (style.Key == "NODE_SIZE")
                        write_continuousMapping(writer, "float", size_header, new List<Tuple<string, string, string, string>>()
                        {
                            new Tuple<string, string, string, string>("1.0", "20.0", "20.0", "1.0"),
                            new Tuple<string, string, string, string>("300.0", "1.0", "300.0", max_total_intensity.ToString()) //max node size should be set to the total intensity of the proteoform
                        });
                    if (style.Key == "NODE_CUSTOMGRAPHICS_1" && quantitative) write_passthrough(writer, "string", piechart_header);
                    if (style.Key == "NODE_BORDER_WIDTH" && quantitative && quantitative_redBorder)
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("7.0", "True")
                        });
                    if (style.Key == "NODE_BORDER_PAINT" && quantitative && quantitative_redBorder)
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("#FFFFFF", "False"),
                            new Tuple<string, string>("#FF0033", "True")
                        });
                    if (style.Key == "NODE_TRANSPARENCY" && quantitative && quantitative_moreOpacity)
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("255", "True"),
                            new Tuple<string, string>("127", "False")
                        });
                    if (style.Key == "NODE_BORDER_TRANSPARENCY" && quantitative && quantitative_moreOpacity)
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("255", "True"),
                            new Tuple<string, string>("127", "False")
                        });
                    if (style.Key == "NODE_LABEL_FONT_FACE" && quantitative && quantitative_boldFace)
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("Dialog.bold,plain,14", "True"),
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

                //OTHER PROPERTIES


                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private static void write_passthrough(XmlWriter writer, string attribute_type, string attribute_name)
        {
            writer.WriteStartElement("passthroughMapping");
            writer.WriteAttributeString("attributeType", attribute_type);
            writer.WriteAttributeString("attributeName", attribute_name);
            writer.WriteEndElement();
        }

        private static void write_discreteMapping(XmlWriter writer, string attribute_type, string attribute_name, List<Tuple<string, string>> entries)
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

        private static void write_continuousMapping(XmlWriter writer, string attribute_type, string attribute_name, List<Tuple<string, string, string, string>> points)
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
        public static Dictionary<string, string> default_styles = new Dictionary<string, string>()
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
            { "NODE_BORDER_TRANSPARENCY", "100"},
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
            { "NODE_BORDER_WIDTH", "2.0"},
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
