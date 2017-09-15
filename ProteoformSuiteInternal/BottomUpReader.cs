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
        //public static List<BottomUpPSM> ReadBUFile(string filename, List<Dictionary<string, List<TheoreticalProteoform>>> theoreticals_by_accession)
        //{
        //    List<BottomUpPSM> psm_list = new List<BottomUpPSM>();
        //    var identifications = new MzidIdentifications(filename);
        //    Parallel.For(0, identifications.Count, sirIndex =>
        //    {
        //        for (int siiIndex = 0; siiIndex < identifications.NumPSMsFromScan(sirIndex); siiIndex++)
        //        {
        //            bool add_psm = true;
        //            if (identifications.IsDecoy(sirIndex, siiIndex) || identifications.QValue(sirIndex, siiIndex) < 0 || identifications.QValue(sirIndex, siiIndex) >= 0.01) continue;
        //            List<Ptm> modifications = new List<Ptm>();
        //            for (int p = 0; p < identifications.NumModifications(sirIndex, siiIndex); p++)
        //            {
        //                double modMass = identifications.ModificationMass(sirIndex, siiIndex, p);
        //                ModificationWithMass mod = null;
        //                List<PtmSet> set;
        //                Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(modMass, 0), out set);
        //                if (set != null) mod = set.Where(m => m.ptm_combination.Count == 1).Select(m => m.ptm_combination.First().modification).Where(m => m.id == identifications.ModificationValue(sirIndex, siiIndex, p) || (m.linksToOtherDbs.ContainsKey("PSI-MOD") && m.linksToOtherDbs["PSI-MOD"].Any(a => a == identifications.ModificationAcession(sirIndex, siiIndex, p).Split(':')[1]))).FirstOrDefault();
        //                if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(sirIndex, siiIndex, p), mod));
        //                else
        //                {
        //                    add_psm = false;
        //                    string mod_id = identifications.ModificationValue(sirIndex, siiIndex, p);
        //                    if (mod_id.Length == 0) mod_id = identifications.ModificationAcession(sirIndex, siiIndex, p);
        //                    lock (bottom_up_PTMs_not_in_dictionary)
        //                    {
        //                        bottom_up_PTMs_not_in_dictionary.Add(mod_id);
        //                    }
        //                }
        //            }
        //            if (add_psm)
        //            {
        //                BottomUpPSM bu_psm = new BottomUpPSM(identifications.PeptideSequenceWithoutModifications(sirIndex, siiIndex), identifications.StartResidueInProtein(sirIndex, siiIndex), identifications.EndResidueInProtein(sirIndex, siiIndex), modifications, identifications.Ms2SpectrumID(sirIndex), identifications.ProteinAccession(sirIndex, siiIndex), identifications.ProteinFullName(sirIndex, siiIndex), identifications.ExperimentalMassToCharge(sirIndex, siiIndex), identifications.ChargeState(sirIndex, siiIndex), identifications.CalculatedMassToCharge(sirIndex, siiIndex));
        //                lock (psm_list) psm_list.Add(bu_psm);
        //                foreach (var dictionary in theoreticals_by_accession)
        //                {
        //                    List<TheoreticalProteoform> theoreticals;
        //                    lock (dictionary) dictionary.TryGetValue(bu_psm.protein_accession, out theoreticals);
        //                    if (theoreticals != null)
        //                    {
        //                        foreach (TheoreticalProteoform t in theoreticals)
        //                        {
        //                            lock (t) t.psm_list.Add(bu_psm);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    });
        //    return psm_list;
        //}
    }
}
