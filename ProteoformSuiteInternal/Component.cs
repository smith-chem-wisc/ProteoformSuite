using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Component
    {
        public string file_origin { get; set; }
        public int id { get; set; }
        public double monoisotopic_mass { get; set; }
        public double intensity_sum { get; set; }
        public double intensity_sum_olcs { get; set; } //intensity sum for overlapping charge states -> determined when grouped into neucode pairs.
        public double delta_mass { get; set; }
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public string scan_range { get; set; }
        public string rt_range { get; set; }
        public double rt_apex { get; set; }
        public double weighted_monoisotopic_mass { get; set; }
        public double _manual_mass_shift { get; set; } = 0;
        public double manual_mass_shift
        {
            get { return _manual_mass_shift; }
            set { _manual_mass_shift = value;
                this.calculate_weighted_monoisotopic_mass();
            }
        }
        public double corrected_mass { get; set; }
        public List<ChargeState> charge_states { get; set; } = new List<ChargeState>();
        public int num_charge_states
        {
            get { return this.charge_states.Count; }
        }
        private int num_detected_intervals { get; set; }
        public int num_charge_states_fromFile { get; set; }
        public bool accepted { get; set; }
        public Component()
        { }
        public Component(List<string> cellStrings, string filename)
        {
            this.id = Convert.ToInt32(cellStrings[0]);
            this.monoisotopic_mass = Convert.ToDouble(cellStrings[1]);
            this.intensity_sum = Convert.ToDouble(cellStrings[2]);
            this.num_charge_states_fromFile = Convert.ToInt32(cellStrings[3]);
            this.num_detected_intervals = Convert.ToInt32(cellStrings[4]);
            this.delta_mass = Convert.ToDouble(cellStrings[5]);
            this.relative_abundance = Convert.ToDouble(cellStrings[6]);
            this.fract_abundance = Convert.ToDouble(cellStrings[7]);
            this.scan_range = cellStrings[8];
            this.rt_range = cellStrings[9];
            this.rt_apex = Convert.ToDouble(cellStrings[10]);
            this.id = Convert.ToInt32(cellStrings[0]);
            this.monoisotopic_mass = Convert.ToDouble(cellStrings[1]);
            this.intensity_sum = Convert.ToDouble(cellStrings[2]);
            this.num_charge_states_fromFile = Convert.ToInt32(cellStrings[3]);
            this.num_detected_intervals = Convert.ToInt32(cellStrings[4]);
            this.delta_mass = Convert.ToDouble(cellStrings[5]);
            this.relative_abundance = Convert.ToDouble(cellStrings[6]);
            this.fract_abundance = Convert.ToDouble(cellStrings[7]);
            this.file_origin = filename;
            this.accepted = true;
        }
        public Component(Component c)
        {
            this.file_origin = c.file_origin;
            this.id = c.id;
            this.monoisotopic_mass = c.monoisotopic_mass;
            this.weighted_monoisotopic_mass = c.weighted_monoisotopic_mass;
            this.corrected_mass = c.corrected_mass;
            //this.manual_mass_shift = c.manual_mass_shift;
            this.intensity_sum = c.intensity_sum;
            this.delta_mass = c.delta_mass;
            this.relative_abundance = c.relative_abundance;
            this.fract_abundance = c.fract_abundance;
            this.scan_range = c.scan_range;
            this.rt_apex = c.rt_apex;
            this.rt_range = c.rt_range;
            this.charge_states = c.charge_states;
            this.num_detected_intervals = c.num_detected_intervals;
            this.num_charge_states_fromFile = c.num_charge_states_fromFile;
            this.intensity_sum_olcs = c.intensity_sum_olcs;
            this.accepted = c.accepted;
        }

        public double calculate_sum_intensity()
        {
            this.intensity_sum = this.charge_states.Select(charge_state => charge_state.intensity).Sum();
            return this.intensity_sum;
        }

        public double calculate_sum_intensity(List<int> charges_to_sum)
        {
            this.intensity_sum_olcs = this.charge_states.Where(cs => charges_to_sum.Contains(cs.charge_count)).Select(charge_state => charge_state.intensity).Sum();
            return this.intensity_sum_olcs;
        }

        public void calculate_weighted_monoisotopic_mass()
        {
            this.weighted_monoisotopic_mass = this.charge_states.Select(charge_state => charge_state.intensity / this.intensity_sum * charge_state.calculated_mass).Sum();
            this.corrected_mass = this.weighted_monoisotopic_mass + this.manual_mass_shift;
        }

        public void add_charge_state(List<string> charge_row, double correction)
        {
            this.charge_states.Add(new ChargeState(charge_row, correction));
        }
    }

    public class ChargeState
    {
        public int charge_count { get; set; }
        public double intensity { get; set; }
        public double mz_centroid { get; set; }
        public double mz_correction { get; set; }
        public double reported_mass { get; set; }
        public double calculated_mass { get; set; }

        public ChargeState(List<string> charge_row, double correction)
        {
            this.charge_count = Convert.ToInt32(charge_row[0]);
            this.intensity = Convert.ToDouble(charge_row[1]);
            this.mz_centroid = Convert.ToDouble(charge_row[2]);
            this.reported_mass = Convert.ToDouble(charge_row[3]);
            this.calculated_mass = CorrectCalculatedMass(correction);
        }

        public double CorrectCalculatedMass(double mz_correction)
        {
            this.mz_correction = mz_correction;
            return (this.charge_count * (this.mz_centroid + mz_correction - 1.00727645D));//Thermo deconvolution 4.0 miscalculates the monoisotopic mass from the reported mz and charge state values.
        }

        public override string ToString()
        {
            return String.Join("\t", new List<string> { charge_count.ToString(), intensity.ToString() });
        }


    }
}
