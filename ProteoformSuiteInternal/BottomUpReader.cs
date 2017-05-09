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
        public static List<ModificationWithMass> bottom_up_PTMs = new List<ModificationWithMass>(); //PTMs from BU file that did not match to any PTMs in uniprotModifications dictionary, added to warning
        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<BottomUpPSM> ReadBUFile(string filename)
        {
            bottom_up_PTMs.Clear();
            List<BottomUpPSM> psm_list = new List<BottomUpPSM>();
            var identifications = new MzidIdentifications(filename);
            for (int i = 0; i < identifications.Count; i++)
            {
                if (identifications.IsDecoy(i) || identifications.QValue(i) < 0 || identifications.QValue(i) >= 0.01) continue;

                List<Ptm> modifications = new List<Ptm>();
                for (int p = 0; p < identifications.NumModifications(i); p++)
                {
                    ModificationWithMass mod = SaveState.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id == identifications.ModificationAcession(i, p)).FirstOrDefault();
                    if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(i, p), mod));
                    else
                    {
                        ModificationMotif motif;
                        ModificationMotif.TryGetMotif(identifications.PeptideSequenceWithoutModifications(i)[identifications.ModificationLocation(i, p)].ToString(), out motif);
                        ModificationWithMass new_ptm = bottom_up_PTMs.Where(m => m.id == identifications.ModificationAcession(i, p)).FirstOrDefault();
                        if (new_ptm == null) //if not in bottom_up_PTMs list, add it (will show in warning)
                        {
                            new_ptm = new ModificationWithMass(identifications.ModificationAcession(i, p), null, motif, ModificationSites.Any, 0, null, new List<double>(), new List<double>(), null);
                            bottom_up_PTMs.Add(new_ptm);
                            modifications.Add(new Ptm(identifications.ModificationLocation(i, p), new_ptm));
                        }
                    }
                }
                psm_list.Add(new BottomUpPSM(identifications.PeptideSequenceWithoutModifications(i), identifications.StartResidueInProtein(i), identifications.EndResidueInProtein(i), modifications, identifications.Ms2SpectrumID(i), identifications.ProteinAccession(i), identifications.ProteinFullName(i), identifications.ExperimentalMassToCharge(i), identifications.ChargeState(i), (identifications.ExperimentalMassToCharge(i) - identifications.CalculatedMassToCharge(i))));
            }
            return psm_list;
        }
    }
}
