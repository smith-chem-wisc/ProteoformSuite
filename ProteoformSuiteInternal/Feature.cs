using System;
using Chemistry;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Feature
    {
        public readonly int featureID;
        public double monoisotopicMass;
        public Envelope mostIntenseEnv;
        public List<Envelope> isotopicEnvelopes;

        public Feature(Envelope env)
        {
            this.isotopicEnvelopes = new List<Envelope> { env };
            this.featureID = env.FeatureID;
            mostIntenseEnv = env;
            this.monoisotopicMass = mostIntenseEnv.monoisotopicMass;
        }

        public void AddEnvelope(Envelope env)
        {
            isotopicEnvelopes.Add(env);
            if (env.abundance > mostIntenseEnv.abundance)
            {
                mostIntenseEnv = env;
                this.monoisotopicMass = env.monoisotopicMass;
            }
        }

        public string GetThermoFormattedString()
        {
            string returnStr = "";

            double minRT = isotopicEnvelopes.Min(v => v.retentionTime);
            double maxRT = isotopicEnvelopes.Max(v => v.retentionTime);
            double intensity = isotopicEnvelopes.Sum(v => v.abundance);
            double apexIntensity = isotopicEnvelopes.Max(v => v.abundance);
            var apex = isotopicEnvelopes.Where(v => v.abundance == apexIntensity).First();
            var charges = isotopicEnvelopes.GroupBy(v => v.charge).OrderBy(v => v.Key).ToList();
            int minScan = isotopicEnvelopes.Min(v => v.scan_num);
            int maxScan = isotopicEnvelopes.Max(v => v.scan_num);

            string headerForThisFeature = featureID + "\t" + monoisotopicMass + "\t" + intensity + "\t" + isotopicEnvelopes.Select(p => p.charge).Distinct().Count() + "\t" + "1" + "\t" + "0" + "20.0" + "\t" + "0.1" + "\t" + "0.1" + "\t" + minScan + "-" + maxScan + "\t" + minRT + "-" + maxRT + "\t" + apex.retentionTime + "\n";
            returnStr += headerForThisFeature;

            string chargeHeader = "\tCharge State\tIntensity\tMZ Centroid\tCalculated Mass\n";
            returnStr += chargeHeader;

            foreach (var charge in charges)
            {
                string chargeString;
                if (charge == charges.Last())
                    chargeString = "\t" + charge.Key + "\t" + charge.Sum(v => v.abundance / v.charge) + "\t" + ClassExtensions.ToMz(monoisotopicMass, charge.Key) + "\t" + monoisotopicMass;
                else
                    chargeString = "\t" + charge.Key + "\t" + charge.Sum(v => v.abundance / v.charge) + "\t" + ClassExtensions.ToMz(monoisotopicMass, charge.Key) + "\t" + monoisotopicMass + "\n";

                returnStr += chargeString;
            }

            return returnStr;
        }
    }
}
