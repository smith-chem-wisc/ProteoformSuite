using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class RelationUtility
    {
        #region Public Methods

        public void updateFiguresOfMerit(List<DeltaMassPeak> peaks, TextBox tb_accepted_relations, TextBox tb_total_peaks, TextBox tb_max_fdr)
        {
            List<DeltaMassPeak> big_peaks = peaks.Where(p => p.Accepted).ToList();
            string max_fdr = (big_peaks.Count > 0) ?
                Math.Round(big_peaks.Max(p => p.peak_group_fdr), 3).ToString() :
                "";
            tb_max_fdr.Text = max_fdr;
            tb_accepted_relations.Text = big_peaks.Sum(p => p.grouped_relations.Count(r => r.Accepted)).ToString();
            tb_total_peaks.Text = big_peaks.Count.ToString();
        }

        #endregion Public Methods
    }
}