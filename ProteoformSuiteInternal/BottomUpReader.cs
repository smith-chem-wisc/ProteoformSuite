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
            for (int sirIndex = 0; sirIndex < identifications.Count; sirIndex++)
            {
                for (int siiIndex = 0; siiIndex < identifications.NumPSMsFromScan(siiIndex); siiIndex++)
                {
                    if (identifications.IsDecoy(sirIndex, siiIndex) || identifications.QValue(sirIndex, siiIndex) < 0 || identifications.QValue(sirIndex, siiIndex) >= 0.01) continue;

                    List<Ptm> modifications = new List<Ptm>();
                    for (int p = 0; p < identifications.NumModifications(sirIndex, siiIndex); p++)
                    {
                        ModificationWithMass mod = SaveState.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id == identifications.ModificationAcession(sirIndex, siiIndex, p)).FirstOrDefault();
                        if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(sirIndex, siiIndex, p), mod));
                        else
                        {
                            ModificationMotif motif;
                            ModificationMotif.TryGetMotif(identifications.PeptideSequenceWithoutModifications(sirIndex, siiIndex)[identifications.ModificationLocation(sirIndex, siiIndex, p) - 1].ToString(), out motif);
                            ModificationWithMass new_ptm = bottom_up_PTMs.Where(m => m.id == identifications.ModificationAcession(sirIndex, siiIndex, p)).FirstOrDefault();
                            if (new_ptm == null) //if not in bottom_up_PTMs list, add it (will show in warning)
                            {
                                new_ptm = new ModificationWithMass(identifications.ModificationAcession(sirIndex, siiIndex, p), null, motif, ModificationSites.Any, 0, null, new List<double>(), new List<double>(), null);
                                bottom_up_PTMs.Add(new_ptm);
                                modifications.Add(new Ptm(identifications.ModificationLocation(sirIndex, siiIndex, p), new_ptm));
                            }
                        }
                    }
                    psm_list.Add(new BottomUpPSM(identifications.PeptideSequenceWithoutModifications(sirIndex, siiIndex), identifications.StartResidueInProtein(sirIndex, siiIndex), identifications.EndResidueInProtein(sirIndex, siiIndex), modifications, identifications.Ms2SpectrumID(sirIndex), identifications.ProteinAccession(sirIndex, siiIndex), identifications.ProteinFullName(sirIndex, siiIndex), identifications.ExperimentalMassToCharge(sirIndex, siiIndex), identifications.ChargeState(sirIndex, siiIndex), (identifications.ExperimentalMassToCharge(sirIndex, siiIndex) - identifications.CalculatedMassToCharge(sirIndex, siiIndex))));
                }
            }
            return psm_list;

        }
    }
}
