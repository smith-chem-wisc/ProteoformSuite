using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MzIdentML;
using Proteomics;
using System.IO;

namespace ProteoformSuiteInternal
{
    public class BottomUpReader
    {
        public static List<string> bottom_up_PTMs_not_in_dictionary = new List<string>(); //PTMs from BU file that did not match to any PTMs in uniprotModifications dictionary, added to warning
        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<BottomUpPSM> ReadBUFile(string filename)
        {
            bottom_up_PTMs_not_in_dictionary.Clear();
            List<BottomUpPSM> psm_list = new List<BottomUpPSM>();
            var identifications = new MzidIdentifications(filename);
            Parallel.For(0, identifications.Count, sirIndex =>
            {
                for (int siiIndex = 0; siiIndex < identifications.NumPSMsFromScan(sirIndex); siiIndex++)
                {
                    bool add_psm = true;
                    if (identifications.IsDecoy(sirIndex, siiIndex) || identifications.QValue(sirIndex, siiIndex) < 0 || identifications.QValue(sirIndex, siiIndex) >= 0.01) continue;
                    List<Ptm> modifications = new List<Ptm>();
                    for (int p = 0; p < identifications.NumModifications(sirIndex, siiIndex); p++)
                    {
                        double modMass = identifications.ModificationMass(sirIndex, siiIndex, p);
                        ModificationWithMass mod = null;
                        List<PtmSet> set;
                        SaveState.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(modMass, 0), out set);
                        if (set != null) mod = set.Where(m => m.ptm_combination.Count == 1).Select(m => m.ptm_combination.First().modification).Where(m => m.id == identifications.ModificationValue(sirIndex, siiIndex, p)).FirstOrDefault();
                        if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(sirIndex, siiIndex, p), mod));
                        else
                        {
                            add_psm = false;
                            string mod_id = identifications.ModificationValue(sirIndex, siiIndex, p);
                            lock (bottom_up_PTMs_not_in_dictionary)
                            {
                                if (!bottom_up_PTMs_not_in_dictionary.Contains(mod_id)) bottom_up_PTMs_not_in_dictionary.Add(mod_id);
                            }
                        }
                    }
                    if (add_psm)
                    {
                        lock (psm_list) psm_list.Add(new BottomUpPSM(identifications.PeptideSequenceWithoutModifications(sirIndex, siiIndex), identifications.StartResidueInProtein(sirIndex, siiIndex), identifications.EndResidueInProtein(sirIndex, siiIndex), modifications, identifications.Ms2SpectrumID(sirIndex), identifications.ProteinAccession(sirIndex, siiIndex), identifications.ProteinFullName(sirIndex, siiIndex), identifications.ExperimentalMassToCharge(sirIndex, siiIndex), identifications.ChargeState(sirIndex, siiIndex), identifications.CalculatedMassToCharge(sirIndex, siiIndex)));
                    }
                }
            });
            return psm_list;
        }
    }
}
