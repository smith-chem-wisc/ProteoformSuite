using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ProteoformSuiteInternal
{
    public static class CytoscapeScript
    {

        #region Private Methods

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
                   from p in t.ExpandedProteinList
                   from g in p.GoTerms
                   where go_terms.Contains(g)
                   select f;
        }

        private static IEnumerable<ProteoformFamily> get_families(IEnumerable<GoTermNumber> go_terms, List<ProteoformFamily> all_families)
        {
            return from f in all_families
                   from t in f.theoretical_proteoforms
                   from p in t.ExpandedProteinList
                   from g in p.GoTerms
                   where go_terms.Any(selected => selected.Id == g.Id && selected.Description == g.Description && selected.Aspect == g.Aspect)
                   select f;
        }

        private static IEnumerable<ProteoformFamily> get_families(IEnumerable<QuantitativeProteoformValues> qvals, List<ProteoformFamily> all_families)
        {
            return from f in all_families
                   from e in f.experimental_proteoforms
                   where qvals.Contains(e.quant)
                   select f;
        }

        #endregion

        #region CYTOSCAPE SCRIPT Fields

        public static string node_file_prefix = "cytoscape_nodes_";

        public static string edge_file_prefix = "cytoscape_edges_";

        public static string style_file_prefix = "cytoscape_style_";

        public static string script_file_prefix = "cytoscape_script_";

        public static string node_file_extension = ".tsv";

        public static string edge_file_extension = ".tsv";

        public static string style_file_extension = ".xml";

        public static string script_file_extension = ".txt";

        #endregion

        #region CYTOSCAPE SCRIPT Public Methods

        public static string write_cytoscape_script(List<ProteoformFamily> families, List<ProteoformFamily> all_families,
            string folder_path, string file_prefix, string time_stamp,
            IGoAnalysis quantitative, bool quantitative_redBorder, bool quantitative_boldFace,
            string color_scheme, string edge_label, string node_label, string node_label_position, string node_position, int double_rounding,
            bool gene_centric_families, string prefered_gene_label)
        {
            return write_script(families, all_families,
                folder_path, file_prefix, time_stamp,
                quantitative, quantitative_redBorder, quantitative_boldFace,
                color_scheme, edge_label, node_label, node_label_position, node_position, double_rounding,
                gene_centric_families, prefered_gene_label);
        }

        public static string write_cytoscape_script(object[] stuff, List<ProteoformFamily> all_families,
            string folder_path, string file_prefix, string time_stamp,
            IGoAnalysis quantitative, bool quantitative_redBorder, bool quantitative_boldFace,
            string color_scheme, string edge_label, string node_label, string node_label_position, string node_position, int double_rounding,
            bool gene_centric_families, string prefered_gene_label)
        {
            List<ProteoformFamily> families = stuff.OfType<ProteoformFamily>().ToList();

            if (stuff.Length <= 0) return "No objects were selected";
            if (families.Count <= 0 && typeof(TheoreticalProteoform).IsAssignableFrom(stuff[0].GetType()))
                families = get_families(stuff.OfType<TheoreticalProteoform>(), all_families).Distinct().ToList();
            if (families.Count <= 0 && typeof(GoTerm) == stuff[0].GetType())
                families = get_families(stuff.OfType<GoTerm>(), all_families).Distinct().ToList();
            if (families.Count <= 0 && typeof(GoTermNumber) == stuff[0].GetType())
                families = get_families(stuff.OfType<GoTermNumber>(), all_families).Distinct().ToList();
            if (families.Count <= 0 && typeof(QuantitativeProteoformValues).IsAssignableFrom(stuff[0].GetType()))
                families = get_families(stuff.OfType<QuantitativeProteoformValues>(), all_families).Distinct().ToList();
            if (families.Count <= 0) return "Selected objects were not recognized.";

            return write_script(families, all_families,
                folder_path, file_prefix, time_stamp,
                quantitative, quantitative_redBorder, quantitative_boldFace,
                color_scheme, edge_label, node_label, node_label_position, node_position, double_rounding,
                gene_centric_families, prefered_gene_label);
        }

        #endregion Public Methods

        #region CYTOSCAPE SCRIPT Private Methods

        private static string write_script(List<ProteoformFamily> families, List<ProteoformFamily> all_families,
            string folder_path, string file_prefix, string time_stamp,
            IGoAnalysis quantitative, bool quantitative_redBorder, bool quantitative_boldFace,
            string color_scheme, string edge_label, string node_label, string node_label_position, string node_position, int double_rounding,
            bool gene_centric_families, string preferred_gene_label)
        {
            //Check if valid folder
            if (folder_path == "" || !Directory.Exists(folder_path))
                return "Please choose a folder in which the families will be built, so you can load them into Cytoscape.";

            if (families.Any(f => f.experimental_proteoforms.Count == 0))
                return "Error: there is a family with zero experimental proteoforms.";

            string nodes_path = Path.Combine(folder_path, file_prefix + node_file_prefix + time_stamp + node_file_extension);
            string edges_path = Path.Combine(folder_path, file_prefix + edge_file_prefix + time_stamp + edge_file_extension);
            string styles_path = Path.Combine(folder_path, file_prefix + style_file_prefix + time_stamp + style_file_extension);
            string script_path = Path.Combine(folder_path, file_prefix + script_file_prefix + time_stamp + script_file_extension);
            string style_name = "ProteoformFamilies" + time_stamp;

            IEnumerable<TheoreticalProteoform> theoreticals = families.SelectMany(f => f.theoretical_proteoforms);
            //Dictionary<string, GeneName> gene_dict = new Dictionary<string, GeneName>();
            //if (gene_centric_families)
            //    foreach (TheoreticalProteoform t in theoreticals)
            //    {
            //        string preferred = t.gene_name.get_prefered_name(prefered_gene_label);
            //        if (gene_dict.ContainsKey(preferred)) gene_dict[preferred].merge(t.gene_name);
            //        else gene_dict.Add(preferred, t.gene_name);
            //    }

            string script = get_script(families.Sum(f => f.proteoforms.Count() + f.relations.Count), quantitative, node_position, edges_path, nodes_path, styles_path, style_name);
            string node_table = get_cytoscape_nodes_tsv(families, quantitative, color_scheme, node_label, node_label_position, node_position, double_rounding, theoreticals, gene_centric_families, preferred_gene_label);
            string edge_table = get_cytoscape_edges_tsv(families, edge_label, node_label, double_rounding, theoreticals, gene_centric_families, preferred_gene_label);
            File.WriteAllText(edges_path, edge_table);
            File.WriteAllText(nodes_path, node_table);
            File.WriteAllText(script_path, script);
            write_styles(all_families, styles_path, style_name, time_stamp,
                edge_label, node_label, node_label_position, color_scheme, quantitative, quantitative_redBorder, quantitative_boldFace);

            string selected_family_string = "Finished building selected famil";
            selected_family_string += families.Count == 1 ? "y :" : "ies :#";
            selected_family_string += (families.Count <= 3) ? String.Join(", #", families.Select(f => f.family_id)) : String.Join(", #", families.Select(f => f.family_id).ToList().Take(3)) + ", etc.";
            return selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.";
        }

        //CYTOSCAPE SCRIPT
        private static string get_script(int feature_count, IGoAnalysis quantitative, string node_position, string edges_path, string nodes_path, string styles_path, string style_name)
        {
            double sleep_factor = feature_count / 1000;
            string node_column_types = quantitative != null ? "s,s,d,s,i,d,d,boolean,s" : "s,s,d,s,i"; //Cytoscape bug: "b" doesn't work in 3.4.0, only "boolean" does
            string edge_column_types = "s,s,s,s,s";
            return String.Join(Environment.NewLine, new string[] {

                //Load Tables
                "network import file file=\"" + edges_path + "\" firstRowAsColumnNames=true delimiters=\"\\t\" indexColumnSourceInteraction=\"1\" indexColumnTargetInteraction=\"3\" startLoadRow=\"0\" dataTypeList=\"" + edge_column_types + "\"",
                "command sleep duration=" + (0.8 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                "table import file file=\"" + nodes_path + "\" startLoadRow=\"0\" keyColumnIndex=\"1\" DataTypeTargetForNetworkCollection=\"Node Table Columns\" dataTypeList=\"" + node_column_types + "\"",
                "command sleep duration=" + (0.5 + Math.Round((0.5 * sleep_factor), 2)).ToString(),


                //Load Settings
                "vizmap load file file=\"" + styles_path + "\"",
                "command sleep duration=" + (1.0 + Math.Round((1.0 * sleep_factor), 2)).ToString(),
                Lollipop.node_positioning.ToList().IndexOf(node_position) == 0 ? "layout degree-circle" : "layout attribute-circle NodeAttribute=" + layout_header,
                "command sleep duration=" + (0.5 + Math.Round((0.5 * sleep_factor), 2)).ToString(),
                "view fit content",

                //Mash applying the style because it flakes out
                "command sleep duration=1",
                "vizmap apply styles=\"" + style_name + "\"",
                "command sleep duration=1",
                "vizmap apply styles=\"" + style_name + "\"",
                "command sleep duration=1",
                "vizmap apply styles=\"" + style_name + "\"",
            });
        }

        #endregion CYTOSCAPE SCRIPT Private Methods

        #region CYTOSCAPE NODE AND EDGE TABLES Fields

        //Headers
        public static string lysine_count_header = "lysine_ct";
        public static string delta_mass_header = "delta_mass";
        public static string edge_ptm_header = "modification";
        public static string proteoform_type_header = "E_or_T";
        public static string size_header = "total_intensity";
        public static string tooltip_header = "more_info";
        public static string layout_header = "layout";
        public static string piechart_header = "piechart_graphics";
        public static string significant_header = "significant_difference";

        //Node types
        public static string experimental_label = "experimental";
        public static string experimental_notQuantified_label = "experimental_below_quantification_threshold";
        public static string unmodified_theoretical_label = "theoretical";
        public static string modified_theoretical_label = "modified_theoretical";
        public static string gene_name_label = "gene_name";
        public static string td_label = "topdown";

        //Other
        public static string mock_intensity = "20"; //set all theoretical proteoforms with observations=20 for node sizing purposes

        #endregion CYTOSCAPE NODE AND EDGE TABLES Public Fields

        #region  CYTOSCAPE NODE AND EDGE TABLES Methods

        public static string get_cytoscape_edges_tsv(List<ProteoformFamily> families,
            string edge_label, string node_label, int double_rounding,
            IEnumerable<TheoreticalProteoform> theoreticals, bool gene_centric_families, string preferred_gene_label)
        {
            DataTable edge_table = new DataTable();
            edge_table.Columns.Add("accession_1", typeof(string));
            edge_table.Columns.Add(lysine_count_header, typeof(int));
            edge_table.Columns.Add("accession_2", typeof(string));
            edge_table.Columns.Add(delta_mass_header, typeof(string));
            edge_table.Columns.Add(edge_ptm_header, typeof(string));

            foreach (ProteoformRelation r in families.SelectMany(f => f.relations).Distinct())
            {
                string delta_mass = Math.Round(r.peak.DeltaMass, double_rounding).ToString("0." + String.Join("", Enumerable.Range(0, double_rounding).Select(i => "0")));
                bool append_ptmlist = r.represented_ptmset != null && (r.RelationType != ProteoformComparison.ExperimentalTheoretical || r.represented_ptmset.ptm_combination.First().modification.id != "Unmodified");
                edge_table.Rows.Add
                (
                    get_proteoform_shared_name(r.connected_proteoforms[0], node_label, double_rounding),
                    r.lysine_count,
                    get_proteoform_shared_name(r.connected_proteoforms[1], node_label, double_rounding),
                    delta_mass,
                    edge_label == Lollipop.edge_labels[1] && append_ptmlist ?
                        delta_mass + " " + String.Join("; ", r.represented_ptmset.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup[ptm.modification].id)) :
                        delta_mass
                );
            }

            if (gene_centric_families)
            {
                foreach (TheoreticalProteoform t in theoreticals)
                {
                    string gene_name = t.gene_name.get_prefered_name(preferred_gene_label);
                    if (gene_name != null)
                    {
                        edge_table.Rows.Add
                        (
                            get_proteoform_shared_name(t, node_label, double_rounding),
                            t.lysine_count,
                            t.gene_name.get_prefered_name(preferred_gene_label),
                            "",
                            ""
                        );
                    }
                }
                foreach (TopDownProteoform t in families.SelectMany(f => f.experimental_proteoforms.Where(exp => exp.topdown_id)))
                {
                    string gene_name = t.gene_name.get_prefered_name(preferred_gene_label);
                    if (gene_name != null)
                    {
                        edge_table.Rows.Add
                         (
                        get_proteoform_shared_name(t, node_label, double_rounding),
                        t.lysine_count,
                        t.gene_name.get_prefered_name(preferred_gene_label),
                        "",
                        ""
                                                );
                    }
                }
            }



            return get_table_string(edge_table);
        }

        public static string get_cytoscape_nodes_tsv(List<ProteoformFamily> families,
            IGoAnalysis quantitative,
            string color_scheme, string node_label, string node_label_position, string node_position, int double_rounding,
            IEnumerable<TheoreticalProteoform> theoreticals, bool gene_centric_families, string preferred_gene_label)
        {
            DataTable node_table = new DataTable();
            node_table.Columns.Add("accession", typeof(string));
            node_table.Columns.Add(proteoform_type_header, typeof(string));
            node_table.Columns.Add(size_header, typeof(double));
            node_table.Columns.Add(tooltip_header, typeof(string));
            node_table.Columns.Add(layout_header, typeof(int));

            if (quantitative != null)
            {
                node_table.Columns.Add(Sweet.lollipop.numerator_condition, typeof(string));
                node_table.Columns.Add(Sweet.lollipop.denominator_condition, typeof(string));
                node_table.Columns.Add(significant_header, typeof(string));
                node_table.Columns.Add(piechart_header, typeof(string));
            }


            //Choose the layout order
            IEnumerable<Proteoform> layout_order;
            switch (Lollipop.node_positioning.ToList().IndexOf(node_position))
            {
                case 0: //arbitrary circle
                case 2: //mass circle
                default:
                    layout_order = families.SelectMany(f => f.experimental_proteoforms).OfType<Proteoform>().Concat(theoreticals).OrderBy(p => p.modified_mass);
                    break;
                case 1: //mass-based spiral
                    layout_order = theoreticals.OrderByDescending(p => p.modified_mass).OfType<Proteoform>().Concat(families.SelectMany(f => f.experimental_proteoforms).OfType<Proteoform>().OrderBy(p => p.modified_mass));
                    break;
            }

            int layout_rank = 1;
            string node_rows = "";
            foreach (Proteoform p in layout_order.ToList())
            {
                if (p as TheoreticalProteoform != null)
                {
                    string node_type = String.Equals(p.ptm_description, "unmodified", StringComparison.CurrentCultureIgnoreCase) ? unmodified_theoretical_label : modified_theoretical_label;
                    node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, mock_intensity, "", layout_rank);
                }

                if (p as ExperimentalProteoform != null)
                {
                    ExperimentalProteoform ep = p as ExperimentalProteoform;

                    string node_type = quantitative != null && ep.quant.intensitySum == 0 ?
                        experimental_notQuantified_label :
                        quantitative == null && ep.topdown_id? 
                        td_label :
                        experimental_label;

                    string total_intensity = quantitative != null ?
                        ep.quant.intensitySum == 0 ? mock_intensity : ((double)ep.quant.intensitySum).ToString() :
                        ep.agg_intensity == 0 ? mock_intensity : ep.agg_intensity.ToString();

                    //Names and size
                    node_rows += String.Join("\t", new List<string> { get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity });

                    //Set tooltip information
                    string tooltip = String.Join("; ", new string[]
                    {
                        "Accession = " + p.accession.ToString(),
                        "Aggregated Mass = " + ep.agg_mass.ToString(),
                        "Aggregated Retention Time = " + ep.agg_rt.ToString(),
                        "Total Intensity = " + total_intensity.ToString(),
                        "Aggregated Component Count = " + (ep.topdown_id ? (ep as TopDownProteoform).topdown_hits.Count.ToString() : ep.aggregated.Count.ToString()),
                        Sweet.lollipop.neucode_labeled ? "; Lysine Count = " + p.lysine_count : "",
                        "Abundant Component for Manual Validation of Identification: " + ep.manual_validation_id,
                        "Abundant Component for Manual Validation of Identification Validation: " + ep.manual_validation_verification
                    });
                    if (quantitative != null && ep.quant.intensitySum > 0)
                    {
                        tooltip += "\\n\\nQuantitation Results:" +
                        String.Join("; ", new string[] {
                            "Q-Value = " + (quantitative as TusherAnalysis1 != null ? ep.quant.TusherValues1.roughSignificanceFDR.ToString() : quantitative as TusherAnalysis2 != null ? ep.quant.TusherValues2.roughSignificanceFDR.ToString() : ""),
                            "Log2FC = " + (quantitative as Log2FoldChangeAnalysis != null ? ep.quant.Log2FoldChangeValues.logfold2change.ToString() : ep.quant.tusherlogFoldChange.ToString()),
                            "Significant = " + (quantitative as TusherAnalysis1 != null ? ep.quant.TusherValues1.significant.ToString() : quantitative as TusherAnalysis2 != null ? ep.quant.TusherValues2.significant.ToString() : quantitative as Log2FoldChangeAnalysis != null ? ep.quant.Log2FoldChangeValues.significant.ToString() : ""),
                            Sweet.lollipop.numerator_condition + " Quantitative Component Count = " + ep.lt_quant_components.Count.ToString(),
                            Sweet.lollipop.denominator_condition + " Quantitative Component Count = " + ep.hv_quant_components.Count.ToString(),
                            "Abundant Component for Manual Validation of Quantification: " + ep.manual_validation_quant
                        });
                    }

                    if (quantitative as TusherAnalysis1 != null && ep.quant.intensitySum != 0)
                        node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity, tooltip, layout_rank, ((double)ep.quant.TusherValues1.numeratorIntensitySum).ToString(), ((double)ep.quant.TusherValues1.denominatorIntensitySum).ToString(), ep.quant.TusherValues1.significant.ToString(), get_piechart_string(color_scheme));
                    else if (quantitative as TusherAnalysis2 != null && ep.quant.intensitySum != 0)
                        node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity, tooltip, layout_rank, ((double)ep.quant.TusherValues2.numeratorIntensitySum).ToString(), ((double)ep.quant.TusherValues2.denominatorIntensitySum).ToString(), ep.quant.TusherValues2.significant.ToString(), get_piechart_string(color_scheme));
                    else if (quantitative as Log2FoldChangeAnalysis != null && ep.quant.intensitySum != 0)
                        node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity, tooltip, layout_rank, ep.quant.Log2FoldChangeValues.allIntensities.Where(kv => kv.Value.condition == Sweet.lollipop.induced_condition).Sum(kv => kv.Value.intensity_sum).ToString(), ep.quant.Log2FoldChangeValues.allIntensities.Where(kv => kv.Value.condition != Sweet.lollipop.induced_condition).Sum(kv => kv.Value.intensity_sum).ToString(), ep.quant.Log2FoldChangeValues.significant.ToString(), get_piechart_string(color_scheme));
                    else if (quantitative != null)
                        node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity, tooltip, layout_rank, "", "", "", "");
                    else
                        node_table.Rows.Add(get_proteoform_shared_name(p, node_label, double_rounding), node_type, total_intensity, tooltip, layout_rank);
                }

                layout_rank++;
            }
            if (gene_centric_families)
            {
                foreach (string gene_name in theoreticals.Select(t => t.gene_name.get_prefered_name(preferred_gene_label)).ToList().
                    Concat(families.SelectMany(f => f.experimental_proteoforms.Where(pf => pf.topdown_id)).
                    Select(t => t.gene_name.get_prefered_name(preferred_gene_label))).Distinct())
                {
                    if (gene_name != null && quantitative != null)
                        node_table.Rows.Add(gene_name, gene_name_label, mock_intensity, "Other Gene Names: ", 0, "", "", "", "");
                    else if (gene_name != null)
                        node_table.Rows.Add(gene_name, gene_name_label, mock_intensity, "Other Gene Names: ", 0);
                }
            }

            return get_table_string(node_table);
        }

        private static string get_table_string(DataTable table)
        {
            StringBuilder result_string = new StringBuilder();

            string header = "";
            foreach (DataColumn column in table.Columns)
            {
                header += column.ColumnName + "\t";
            }
            result_string.AppendLine(header);

            foreach (DataRow row in table.Rows)
            {
                result_string.AppendLine(String.Join("\t", row.ItemArray));
            }

            return result_string.ToString();
        }

        public static string get_proteoform_shared_name(Proteoform p, string node_label, int double_rounding)
        {
            if (p as ExperimentalProteoform != null)
            {
                ExperimentalProteoform e = p as ExperimentalProteoform;
                string name = Math.Round(e.agg_mass, double_rounding) + "_Da_" + Math.Round(e.agg_rt, double_rounding) + "_min_"  + e.accession;
                if (node_label == Lollipop.node_labels[1] && e.linked_proteoform_references != null && e.linked_proteoform_references.Count > 0)
                    name += " " + (e.linked_proteoform_references.First() as TheoreticalProteoform).accession
                          + " " + e.begin + "to" + e.end + " " +
                          (e.ptm_set.ptm_combination.Count == 0 ?
                            "Unmodified" :
                            String.Join("; ", e.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup[ptm.modification].id)));
                return name;
            }

            else if (p as TheoreticalProteoform != null)
            {
                return p.accession + " " + p.ptm_description;
            }

            else
            {
                return p.accession;
            }
        }

        private static string get_piechart_string(string color_scheme)
        {
            return "piechart: attributelist = \"" + Sweet.lollipop.denominator_condition + "," + Sweet.lollipop.numerator_condition +
                "\" colorlist = \"" + color_schemes[color_scheme][0] + "," + color_schemes[color_scheme][3] +
                "\" labellist = \",\"";
        }

        #endregion  CYTOSCAPE NODE AND EDGE TABLES Methods

        #region CYTOSCAPE STYLES XML Fields

        public static string[] color_scheme_names = new string[6]
        {
            "Licorice",
            "Smarties",
            "Candy Corn",
            "Popsicle",
            "Marshmallow",
            "Wisconsin Schaum Torte"
        };
        
        public static Dictionary<string, List<string>> color_schemes = new Dictionary<string, List<string>>
        {
            //Colors: 0) exp, 1) ptm, 2) theo, 3) pie, 4) gene, 5) annulus, 6) top-down
            { color_scheme_names[0], new List<string> { "#3333FF", "#00CC00", "#FF0000", "#FFFF00", "#DC89BA", "#FF0000", "#897AB9" } },
            { color_scheme_names[1], new List<string> { "#00C0F3", "#39B54A", "#39B54A", "#FFF56D", "#F173AC", "#F37053", "#897AB9" } },
            { color_scheme_names[2], new List<string> { "#2F5E91", "#2D6A00", "#F45512", "#916415", "#916415", "#F45512", "#897AB9" } },
            { color_scheme_names[3], new List<string> { "#5338FF", "#1F8A70", "#FF6533", "#FFE11A", "#FFE11A", "#FF6533", "#897AB9" } },
            { color_scheme_names[4], new List<string> { "#A8E1FF", "#B29162", "#B26276", "#FFF08C", "#FFF08C", "#B26276", "#897AB9" } },
            { color_scheme_names[5], new List<string> { "#3D8A99", "#979C9C", "#963C4B", "#F2EBC7", "#F2EBC7", "#963C4B", "#897AB9" } }
        };

        public static string not_quantified = "#D3D3D3";

        public static string[] node_label_positions = new string[3]
        {
            "On Node",
            "Above Node",
            "Below Node",
            //"Outside Circle"
        };

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

        #endregion CYTOSCAPE STYLES XML Public Fields

        #region CYTOSCAPE STYLES XML Methods

        public static void write_styles(List<ProteoformFamily> all_families, string styles_path, string style_name, string time_stamp,
            string edge_label, string node_label, string node_label_position, string color_scheme,
            IGoAnalysis quantitative, bool quantitative_redBorder, bool quantitative_boldFace)
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
                double max_total_intensity = all_families.SelectMany(f => f.experimental_proteoforms.Where(e => !e.topdown_id)).Count() > 0 ? quantitative != null ?
                    (double)all_families.SelectMany(f => f.experimental_proteoforms).Max(p => p.quant.intensitySum) :
                    all_families.SelectMany(f => f.experimental_proteoforms).Max(p => p.agg_intensity) : 1e6;
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
                    else
                    {
                        writer.WriteAttributeString("default", style.Value);
                    }


                    //Discrete and continuous mapping
                    if (style.Key == "NODE_FILL_COLOR")
                    {
                        write_discreteMapping(writer, "string", proteoform_type_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>(quantitative != null ? "#FFFFFF" : color_schemes[color_scheme][0], experimental_label),
                            new Tuple<string, string>(not_quantified, experimental_notQuantified_label),
                            new Tuple<string, string>(color_schemes[color_scheme][1], modified_theoretical_label),
                            new Tuple<string, string>(color_schemes[color_scheme][2], unmodified_theoretical_label),
                            new Tuple<string, string>(color_schemes[color_scheme][6], td_label),
                            new Tuple<string, string>(color_schemes[color_scheme][4], gene_name_label)
                            //new Tuple<string, string>(color_schemes[color_scheme][4], transcript_name_label)
                        });
                    }

                    if (style.Key == "NODE_SHAPE")
                    {
                        write_discreteMapping(writer, "string", proteoform_type_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("ELLIPSE", experimental_label),
                            new Tuple<string, string>("ELLIPSE", experimental_notQuantified_label),
                            new Tuple<string, string>("ELLIPSE", modified_theoretical_label),
                            new Tuple<string, string>("ELLIPSE", unmodified_theoretical_label),
                            new Tuple<string, string>("RECTANGLE", gene_name_label)
                            //new Tuple<string, string>("DIAMOND", transcript_name_label)
                        });
                    }

                    if (style.Key == "NODE_LABEL_FONT_FACE")
                    {
                        write_discreteMapping(writer, "string", proteoform_type_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("Arial Italic,plain,14", gene_name_label),
                            new Tuple<string, string>("Arial,plain,14", modified_theoretical_label),
                            new Tuple<string, string>("Arial,plain,14", unmodified_theoretical_label),
                            new Tuple<string, string>("Arial,plain,14", experimental_label),
                            new Tuple<string, string>("Arial,plain,14", experimental_notQuantified_label)
                        });
                    }

                    if (style.Key == "NODE_LABEL_COLOR")
                    {
                        write_passthrough(writer, "string", "shared name");
                    }

                    if (style.Key == "NODE_LABEL")
                    {
                        write_passthrough(writer, "string", "name");
                    }

                    if (style.Key == "NODE_SIZE")
                    {
                        write_continuousMapping(writer, "float", size_header, new List<Tuple<string, string, string, string>>()
                        {
                            new Tuple<string, string, string, string>("1.0", "20.0", "20.0", "1.0"),
                            new Tuple<string, string, string, string>("300.0", "1.0", "300.0", max_total_intensity.ToString()) //max node size should be set to the total intensity of the proteoform
                        });
                    }

                    if (style.Key == "NODE_CUSTOMGRAPHICS_1" && quantitative != null)
                    {
                        write_passthrough(writer, "string", piechart_header);
                    }

                    if (style.Key == "NODE_BORDER_WIDTH" && quantitative != null && quantitative_redBorder)
                    {
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("7.0", "True")
                        });
                    }

                    if (style.Key == "NODE_BORDER_PAINT" && quantitative != null && quantitative_redBorder)
                    {
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("#FFFFFF", "False"),
                            new Tuple<string, string>(color_schemes[color_scheme][5], "True")
                        });
                    }

                    if (style.Key == "NODE_BORDER_TRANSPARENCY" && quantitative != null && quantitative_redBorder)
                    {
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("255", "True"),
                        });
                    }

                    if (style.Key == "NODE_LABEL_FONT_FACE" && quantitative != null && quantitative_boldFace)
                    {
                        write_discreteMapping(writer, "boolean", significant_header, new List<Tuple<string, string>>()
                        {
                            new Tuple<string, string>("Dialog.bold,plain,14", "True"),
                        });
                    }

                    if (style.Key == "NODE_TOOLTIP")
                    {
                        write_passthrough(writer, "string", tooltip_header);
                    }

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

                    if (style.Key == "EDGE_LABEL" && edge_label == Lollipop.edge_labels[0])
                    {
                        write_passthrough(writer, "string", delta_mass_header);
                    }

                    if (style.Key == "EDGE_LABEL" && edge_label == Lollipop.edge_labels[1])
                    {
                        write_passthrough(writer, "string", edge_ptm_header);
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                //OTHER PROPERTIES
                //  none, currently

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
            foreach (Tuple<string, string, string, string> p in points)
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

        #endregion CYTOSCAPE STYLES XML Methods

    }
}
