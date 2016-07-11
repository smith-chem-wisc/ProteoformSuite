using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public class Component
    {
        public string file_origin { get; set; }
        public int id { get; set; }
        public double monoisotopic_mass { get; set; }
        public double intensity_sum { get; set; }
        public double delta_mass { get; set; }
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public string scan_range { get; set; }
        public string rt_range { get; set; }
        public double rt_apex { get; set; }
        public double weighted_monoisotopic_mass { get; set; }
        public double corrected_mass { get; set; }
        public List<ChargeState> charge_states { get; set; } = new List<ChargeState>();
        public int num_charge_states
        {
            get { return this.charge_states.Count; }
        }
        private int num_detected_intervals { get; set; }
        private int num_charge_states_fromFile { get; set; }

        public Component()
        { }
        public Component(List<Cell> component_cells, string filename)
        {
            this.id = Convert.ToInt32(component_cells[0].InnerText);
            this.monoisotopic_mass = Convert.ToDouble(component_cells[1].InnerText);
            this.intensity_sum = Convert.ToDouble(component_cells[2].InnerText);
            this.num_charge_states_fromFile = Convert.ToInt32(component_cells[3].InnerText);
            this.num_detected_intervals = Convert.ToInt32(component_cells[4].InnerText);
            this.delta_mass = Convert.ToDouble(component_cells[5].InnerText);
            this.relative_abundance = Convert.ToDouble(component_cells[6].InnerText);
            this.fract_abundance = Convert.ToDouble(component_cells[7].InnerText);
            this.scan_range = component_cells[8].InnerText;
            this.rt_range = component_cells[9].InnerText;
            this.rt_apex = Convert.ToDouble(component_cells[10].InnerText);
            this.file_origin = filename;
        }
        public Component(Component c)
        {
            this.file_origin = c.file_origin;
            this.id = c.id;
            this.monoisotopic_mass = c.monoisotopic_mass;
            this.weighted_monoisotopic_mass = c.weighted_monoisotopic_mass;
            this.corrected_mass = c.corrected_mass;
            this.intensity_sum = c.intensity_sum = c.intensity_sum;
            this.delta_mass = c.delta_mass;
            this.relative_abundance = c.relative_abundance;
            this.fract_abundance = c.fract_abundance;
            this.scan_range = c.scan_range;
            this.rt_apex = c.rt_apex;
            this.rt_range = c.rt_range;
            this.charge_states = c.charge_states;
            this.num_detected_intervals = c.num_detected_intervals;
            this.num_charge_states_fromFile = c.num_charge_states_fromFile;
        }

        public double calculate_sum_intensity()
        {
            this.intensity_sum = this.charge_states.Select(charge_state => charge_state.intensity).Sum();
            return this.intensity_sum;
        }

        public double calculate_sum_intensity(List<int> charges_to_sum)
        {
            return this.charge_states.Where(cs => charges_to_sum.Contains(cs.charge_count)).Select(charge_state => charge_state.intensity).Sum();
        }

        public void calculate_weighted_monoisotopic_mass()
        {
            this.weighted_monoisotopic_mass = this.charge_states.Select(charge_state => charge_state.intensity / this.intensity_sum * charge_state.calculated_mass).Sum();
            this.corrected_mass = this.weighted_monoisotopic_mass;
        }

        public void add_charge_state(List<Cell> charge_row)
        {
            this.charge_states.Add(new ChargeState(charge_row));
        }

        public string as_csv_row()
        {
            return String.Join(",", new List<string> { this.id.ToString(), this.monoisotopic_mass.ToString(), this.weighted_monoisotopic_mass.ToString(), this.intensity_sum.ToString(), this.num_charge_states.ToString(),
                this.delta_mass.ToString(), this.relative_abundance.ToString(), this.fract_abundance.ToString(), this.scan_range.ToString(), this.rt_apex.ToString(),
                this.rt_apex.ToString(), this.file_origin.ToString() });
        }

        public static string get_csv_header()
        {
            return String.Join(",", new List<string> { "id", "monoisotopic_mass", "weighted_monoisotopic_mass", "intensity_sum", "num_charge_states",
                "delta_mass", "relative_abundance", "fract_abundance", "scan_range", "rt_apex",
                "rt_apex", "file_origin" });
        }
    }

    public class ChargeState
    {
        public int charge_count { get; set; }
        public double intensity { get; set; }
        public double mz_centroid { get; set; }
        public double calculated_mass { get; set; }

        public ChargeState(List<Cell> charge_row)
        {
            this.charge_count = Convert.ToInt32(charge_row[0].InnerText);
            this.intensity = Convert.ToDouble(charge_row[1].InnerText);
            this.mz_centroid = Convert.ToDouble(charge_row[2].InnerText);
            this.calculated_mass = Convert.ToDouble(charge_row[3].InnerText);
        }

        public override string ToString()
        {
            return String.Join("\t", new List<string> { charge_count.ToString(), intensity.ToString() });
        }
    }
}
