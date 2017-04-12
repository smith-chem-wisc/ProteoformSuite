using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MzIdentML;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class BottomUpReader
    {
        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<Psm> ReadBUFile(string filename)
        {
            List<Psm> psm_list = new List<Psm>();
            var identifications = new MzidIdentifications(filename);
            for (int i = 0; i < identifications.Count; i++)
            {
                if (identifications.IsDecoy(i) || identifications.QValue(i) < 0 || identifications.QValue(i) >= 0.01) continue;

                List<Ptm> modifications = new List<Ptm>();
                for (int p = 0; p < identifications.NumModifications(i); p++)
                {
                    ModificationWithMass mod = Lollipop.uniprotModificationTable.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id == identifications.ModificationAcession(i, p)).FirstOrDefault();
                    if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(i, p), mod));
                    else modifications.Add(new Ptm(identifications.ModificationLocation(i, p), new ModificationWithMass(identifications.ModificationAcession(i, p), null, null, ModificationSites.Any, 0, null, null, null, null)));
                }
                psm_list.Add(new Psm(identifications.PeptideSequenceWithoutModifications(i), identifications.StartResidueInProtein(i), identifications.EndResidueInProtein(i), modifications, identifications.Ms2SpectrumID(i), identifications.ProteinAccession(i), identifications.ProteinFullName(i), identifications.ExperimentalMassToCharge(i), identifications.ChargeState(i), (identifications.ExperimentalMassToCharge(i) - identifications.CalculatedMassToCharge(i))));
            }
            return psm_list;
        }
    }
}
