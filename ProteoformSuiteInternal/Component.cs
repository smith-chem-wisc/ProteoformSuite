using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Component
        : IFileIntensity, IAggregatable
    {
        #region Private Fields

        private double _manual_mass_shift;
        private int _num_charge_states;
        private double _intensity_sum;
        private double _weighted_monoisotopic_mass;

        #endregion Private Fields

        #region Public Properties

        public InputFile input_file { get; set; }
        public string id { get; set; } // deconvolution 4.0 assigns a component id. This is made unique by appending the inputFile id.
        public double reported_monoisotopic_mass { get; set; }  //from deconvolution 4.0
        public double intensity_reported { get; set; } //from deconvolution 4.0
        public double reported_delta_mass { get; set; } // the difference between the mass of the compound and the highest intensity component in the window
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public string scan_range { get; set; }
        public string rt_range { get; set; }
        public double rt_apex { get; set; }
        public List<ChargeState> charge_states { get; set; } = new List<ChargeState>();
        public List<Component> incorporated_missed_monoisotopics = new List<Component>();
        public bool calculating_properties { get; set; } = false;
        public int num_detected_intervals { get; set; }
        public bool accepted { get; set; }

        /// <summary>
        /// Added or substracted from weighted monoisotopic mass. This value is adjusted manually after observing ET histograms. Eventually also after see EE histograms.
        /// </summary>
        public double manual_mass_shift
        {
            get { return _manual_mass_shift; }
            set
            {
                _manual_mass_shift = value;
                calculate_properties();
            }
        }

        public int num_charge_states
        {
            get
            {
                return _num_charge_states;
            }
            set
            {
                if (!calculating_properties && charge_states.Count > 0)
                {
                    throw new ArgumentException("Charge state data exists that can't be overwritten with input");
                }
                else
                {
                    _num_charge_states = value;
                }
            }
        }

        /// <summary>
        /// intensity sum for all charge states. Different value than what is reported by deconv 4.0 for some reason
        /// </summary>
        public double intensity_sum
        {
            get
            {
                return _intensity_sum;
            }
            set
            {
                if (!calculating_properties && charge_states.Count > 0)
                    throw new ArgumentException("Charge state data exists that can't be overwritten with intensity input");
                else
                    _intensity_sum = value;
            }
        }

        /// <summary>
        /// this is computed as the weighted sum of charge state masses.
        /// </summary>
        public double weighted_monoisotopic_mass
        {
            get
            {
                return _weighted_monoisotopic_mass;
            }
            set
            {
                if (!calculating_properties && charge_states.Count > 0)
                    throw new ArgumentException("Charge state data exists that can't be overwritten with mass input");
                else
                    _weighted_monoisotopic_mass = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        public Component()
        { }

        public Component(List<string> cellStrings, InputFile input_file) // this is used when we read stored data from previous computation.
        {
            this.id = Convert.ToInt32(cellStrings[0]).ToString();
            this.input_file = input_file;
            this.id = input_file.UniqueId.ToString() + "_" + Convert.ToInt32(cellStrings[0]);
            this.reported_monoisotopic_mass = Convert.ToDouble(cellStrings[1]);
            this.weighted_monoisotopic_mass = Convert.ToDouble(cellStrings[1]); // this will get immediately replaced and updated as charge states are added.
            this.intensity_reported = Convert.ToDouble(cellStrings[2]);
            this.num_charge_states = (Convert.ToInt32(cellStrings[3]));
            this.num_detected_intervals = Convert.ToInt32(cellStrings[4]);
            this.reported_delta_mass = Convert.ToDouble(cellStrings[5]);
            this.relative_abundance = Convert.ToDouble(cellStrings[6]);
            this.fract_abundance = Convert.ToDouble(cellStrings[7]);
            this.scan_range = cellStrings[8];
            this.rt_range = cellStrings[9];
            this.rt_apex = Convert.ToDouble(cellStrings[10]);
            this.intensity_sum = Convert.ToDouble(cellStrings[2]); // this needs to be fixed.
            this.accepted = true;
            this.charge_states = new List<ChargeState>();
        }

        #endregion Constructors

        #region Public Methods

        public void add_charge_state(List<string> charge_row)
        {
            charge_states.Add(new ChargeState(charge_row));
        }

        public void calculate_properties()
        {
            if (charge_states.Count > 0)
            {
                calculating_properties = true;
                intensity_sum = charge_states.Sum(cs => cs.intensity);
                weighted_monoisotopic_mass = charge_states.Sum(charge_state => charge_state.intensity / intensity_sum * charge_state.calculated_mass) + manual_mass_shift;
                num_charge_states = charge_states.Count;
                calculating_properties = false;
            }
        }

        public Component mergeTheseComponents(Component cpToMerge) //this method is used just after initial read of components to get rid of missed monoisotopics in the same scan.
        {
            //Note: the max missed monoisotopics is hard coded for now. Need more analysis to see whether this should be subject to user adjustment.
            if (Math.Abs((this.weighted_monoisotopic_mass - cpToMerge.weighted_monoisotopic_mass)) <= (3 * Lollipop.MONOISOTOPIC_UNIT_MASS + Math.Max(this.weighted_monoisotopic_mass, cpToMerge.weighted_monoisotopic_mass) / 1000000d * Sweet.lollipop.mass_tolerance))
            {// we're merging missed monoisotopics
                foreach (ChargeState cpCS in cpToMerge.charge_states)
                {
                    if (cpCS.calculated_mass > this.weighted_monoisotopic_mass)
                    {
                        double monoIsoTopicDifference = Math.Round(cpCS.calculated_mass - this.weighted_monoisotopic_mass) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        cpCS.calculated_mass = cpCS.calculated_mass - monoIsoTopicDifference;
                        cpCS.mz_centroid = (cpCS.calculated_mass + Lollipop.PROTON_MASS * cpCS.charge_count) / cpCS.charge_count;
                    }
                    else
                    {
                        double monoIsoTopicDifference = Math.Round(this.weighted_monoisotopic_mass - cpCS.calculated_mass) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        cpCS.calculated_mass = cpCS.calculated_mass + monoIsoTopicDifference;
                        cpCS.mz_centroid = (cpCS.calculated_mass + Lollipop.PROTON_MASS * cpCS.charge_count) / cpCS.charge_count;
                    }

                    if (this.charge_states.Exists(thisCS => thisCS.charge_count == cpCS.charge_count))
                    {
                        this.charge_states.Where(thisCS => thisCS.charge_count == cpCS.charge_count).First().mergeTheseChargeStates(cpCS);
                    }
                    else
                    {
                        this.charge_states.Add(cpCS);
                    }

                    this.calculate_properties();
                }
            }
            else // we're merging harmonics
            {
                foreach (ChargeState cpCS in cpToMerge.charge_states)
                {
                    if (cpCS.calculated_mass > this.weighted_monoisotopic_mass)
                    {
                        double harmonicRatio = Math.Round(cpCS.calculated_mass / this.weighted_monoisotopic_mass);// this should be 2 or 3
                        cpCS.charge_count = (int)(cpCS.charge_count / harmonicRatio);//charge counts of harmonics are doubled or tripled and need to be lowered
                        cpCS.calculated_mass = cpCS.calculated_mass / harmonicRatio;
                        cpCS.calculated_mass = cpCS.calculated_mass - (Math.Round(cpCS.calculated_mass - this.weighted_monoisotopic_mass)) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        cpCS.mz_centroid = (cpCS.calculated_mass + Lollipop.PROTON_MASS * cpCS.charge_count) / cpCS.charge_count;
                    }
                    else
                    {
                        double harmonicRatio = Math.Round(this.weighted_monoisotopic_mass / cpCS.calculated_mass);// this should be 2 or 3
                        cpCS.charge_count = (int)(cpCS.charge_count * harmonicRatio);//charge counts of harmonics are halved or thirded and need to be raised
                        cpCS.calculated_mass = cpCS.calculated_mass * harmonicRatio;
                        cpCS.calculated_mass = cpCS.calculated_mass - (Math.Round(cpCS.calculated_mass - this.weighted_monoisotopic_mass)) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        cpCS.mz_centroid = (cpCS.calculated_mass + Lollipop.PROTON_MASS * cpCS.charge_count) / cpCS.charge_count;
                    }
                    if (this.charge_states.Exists(thisCS => thisCS.charge_count == cpCS.charge_count))
                        this.charge_states.Where(thisCS => thisCS.charge_count == cpCS.charge_count).First().mergeTheseChargeStates(cpCS);
                    else
                        this.charge_states.Add(cpCS);

                    this.calculate_properties();
                }
            }
            return this;
        }

        #endregion Public Methods
    }
}