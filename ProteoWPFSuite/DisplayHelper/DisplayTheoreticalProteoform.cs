﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ProteoformSuiteInternal;

namespace ProteoWPFSuite
{
    class DisplayTheoreticalProteoform : DisplayObject
    {
        #region Public Constructors

        public DisplayTheoreticalProteoform(TheoreticalProteoform t)
            : base(t)
        {
            this.t = t;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly TheoreticalProteoform t;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get { return t.accession; }
        }

        public string Name
        {
            get { return t.name; }
        }

        public string Description
        {
            get { return t.description; }
        }

        public string gene_name
        {
            get
            {
                return t.gene_name != null ?
                    t.gene_name.get_prefered_name(Lollipop.preferred_gene_label) :
                    "";
            }
        }

        public string GeneID
        {
            get
            {
                return string.Join("; ", t.ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct());
            }
        }

        public string Fragment
        {
            get { return t.fragment; }
        }

        public int Begin
        {
            get { return t.begin; }
        }

        public int End
        {
            get { return t.end; }
        }

        public string Sequence
        {
            get { return t.sequence; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public double unmodified_mass
        {
            get { return t.unmodified_mass; }
        }

        public double ptm_mass
        {
            get { return t.ptm_mass; }
        }

        public bool Contaminant
        {
            get { return t.contaminant; }
        }

        public double lysine_count
        {
            get { return t.lysine_count; }
        }

        public string ptm_description
        {
            get { return t.ptm_set.ptm_description; }
        }

        public string goTerm_IDs
        {
            get { return t.goTerm_IDs; }
        }

        public string groupedAccessions
        {
            get { return string.Join(", ", t.ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Distinct()); }
        }

        public bool topdown_theoretical
        {
            get { return t.topdown_theoretical; }
        }

        public bool new_topdown_proteoform
        {
            get { return t.new_topdown_proteoform; }
        }

        public int bu_PSMs_count
        {
            get { return t.bottom_up_PSMs.Count; }
        }

        public string bu_PSMs_PTMs_peptide_specific
        {
            get { return (String.Join(", ", t.bottom_up_PSMs.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0).Select(p => p.ptm_description).Distinct())); }
        }


        public string bu_PSMs_PTMs
        {
            get
            {
                return (String.Join(", ", t.bottom_up_PSMs.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0).SelectMany(p => p.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification))
                                               .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()));
            }
        }


        public bool bottom_up_evidence_for_all_PTMs
        {
            get
            {
                return t.bottom_up_evidence_for_all_PTMs;
            }
        }

        public bool begin_peptide
        {
            get
            {
                return t.bottom_up_PSMs.Any(p => p.begin == t.begin);
            }
        }

        public bool end_peptide
        {
            get
            {
                return t.bottom_up_PSMs.Any(p => p.end == t.end);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTheoreticalProteoformTable(System.Windows.Forms.DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;

            foreach (System.Windows.Forms.DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatTheoreticalProteoformTable(List<DisplayTheoreticalProteoform> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayTheoreticalProteoform).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(unmodified_mass)) { return "Unmodified Mass"; }
            if (property_name == nameof(ptm_mass)) { return "PTM Mass"; }
            if (property_name == nameof(ptm_description)) { return "PTM Description"; }
            if (property_name == nameof(modified_mass)) { return "Modified Mass"; }
            if (property_name == nameof(lysine_count)) { return "Lysine Count"; }
            if (property_name == nameof(goTerm_IDs)) { return "GO Term IDs"; }
            if (property_name == nameof(gene_name)) { return "Gene Name"; }
            if (property_name == nameof(groupedAccessions)) { return "Grouped Accessions"; }
            if (property_name == nameof(topdown_theoretical)) { return "Top-Down Theoretical"; }
            if (property_name == nameof(new_topdown_proteoform)) { return "Not in Original Database"; }
            if (property_name == nameof(bu_PSMs_PTMs)) return "Modified Bottom-Up PSMs";
            if (property_name == nameof(bu_PSMs_count)) return "Bottom-Up PSMs Count";
            if (property_name == nameof(bu_PSMs_PTMs_peptide_specific)) return "Peptide-Specific Modified Bottom-Up PSMs";
            if (property_name == nameof(bottom_up_evidence_for_all_PTMs)) return "Bottom-Up Evidence for All PTMs";
            if(property_name == nameof(begin_peptide)) return "Bottom-Up Evidence for Begin";
            if (property_name == nameof(end_peptide)) return "Bottom-Up Evidence for End";


            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(unmodified_mass)) { return "0.####"; }
            if (property_name == nameof(ptm_mass)) { return "0.####"; }
            if (property_name == nameof(modified_mass)) { return "0.####"; }
            return null;
        }

        #endregion Private Methods
    }
}
