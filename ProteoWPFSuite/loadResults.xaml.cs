using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProteoformSuiteInternal;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.IO;
using Xceed.Wpf.Toolkit;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for loadResults.xaml
    /// </summary>
    public partial class LoadResults : Window
    {
        public LoadResults()
        {
            InitializeComponent();
        }

        #region GENERAL TABLE OPTIONS Private Methods
        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();

            if ((bool)rb_standardOptions.IsChecked)
            {
                for (int i = 0; i < 4; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = true;


                bt_calibrate.Visibility = Visibility.Collapsed;
                cb_calibrate_raw_files.Visibility = Visibility.Collapsed;
                cb_calibrate_td_files.Visibility = Visibility.Collapsed;
                bt_deconvolute.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Visible;
                bt_fullrun.Visibility = Visibility.Visible;
                bt_calibrate.Visibility = Visibility.Collapsed;
                panel_deconv_calib.Visibility = Visibility.Collapsed;
                panel_step.Visibility = Visibility.Visible;
                nud_maxcharge.Visibility = Visibility.Collapsed;
                nud_mincharge.Visibility = Visibility.Collapsed;
                label_maxcharge.Visibility = Visibility.Collapsed;
                label_mincharge.Visibility = Visibility.Collapsed;
                label_maxRT.Visibility = Visibility.Collapsed;
                label_minRT.Visibility = Visibility.Collapsed;
                rb_neucode.Visibility = Visibility.Visible;
                rb_unlabeled.Visibility = Visibility.Visible;
                calib_stand_splitContainer.Visibility = Visibility.Visible;
                fullrun_groupbox.Visibility = Visibility.Visible;

            }

            else if ((bool)rb_chemicalCalibration.IsChecked)
            {
                for (int i = 4; i < 7; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                bt_calibrate.Visibility = Visibility.Visible;
                cb_calibrate_td_files.Visibility = Visibility.Visible;
                cb_calibrate_raw_files.Visibility = Visibility.Visible;
                bt_deconvolute.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Collapsed;
                bt_fullrun.Visibility = Visibility.Collapsed;
                bt_calibrate.Visibility = Visibility.Visible;
                panel_deconv_calib.Visibility = Visibility.Visible;
                panel_step.Visibility = Visibility.Collapsed;
                nud_maxcharge.Visibility = Visibility.Collapsed;
                nud_mincharge.Visibility = Visibility.Collapsed;
                label_maxcharge.Visibility = Visibility.Collapsed;
                label_mincharge.Visibility = Visibility.Collapsed;
                label_maxRT.Visibility = Visibility.Collapsed;
                label_minRT.Visibility = Visibility.Collapsed;
                rb_neucode.Visibility = Visibility.Visible;
                rb_unlabeled.Visibility = Visibility.Visible;
                calib_stand_splitContainer.Visibility = Visibility.Visible;
                fullrun_groupbox.Visibility = Visibility.Collapsed;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = true;
            }

            else if ((bool)rb_deconvolution.IsChecked)
            {

                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = false;

                bt_calibrate.Visibility = Visibility.Collapsed;
                cb_calibrate_raw_files.Visibility = Visibility.Collapsed;
                cb_calibrate_td_files.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Collapsed;
                bt_fullrun.Visibility = Visibility.Collapsed;
                bt_calibrate.Visibility = Visibility.Collapsed;
                bt_deconvolute.Visibility = Visibility.Visible;
                panel_deconv_calib.Visibility = Visibility.Visible;
                panel_step.Visibility = Visibility.Collapsed;
                nud_maxcharge.Visibility = Visibility.Visible;
                nud_mincharge.Visibility = Visibility.Visible;
                label_maxcharge.Visibility = Visibility.Visible;
                label_mincharge.Visibility = Visibility.Visible;
                label_maxRT.Visibility = Visibility.Visible;
                label_minRT.Visibility = Visibility.Visible;
                rb_neucode.Visibility = Visibility.Collapsed;
                rb_unlabeled.Visibility = Visibility.Collapsed;
                calib_stand_splitContainer.Visibility = Visibility.Collapsed;
                fullrun_groupbox.Visibility = Visibility.Collapsed;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = false;
            }

            lb_filter1.Content = cmb_loadTable1.SelectedItem.ToString();

            //reload_dgvs();
            //refresh_dgvs();
        }
        #endregion GENERAL TABLE OPTIONS Private Methods

        #region DGV DRAG AND DROP Private Methods
        /*
        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_quantResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_calibrationResults_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_deconResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void dgv_calibrationResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void dgv_quantResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void drag_drop(DragEventArgs e, ComboBox cmb, DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DisplayUtility.CheckForProteinFastas(cmb, files)) return; // todo: implement protein fasta usage
            Sweet.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[selected_index], Lollipop.file_types[selected_index], Sweet.lollipop.input_files, true);
            refresh_dgvs();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        private void refresh_dgvs()
        {
            foreach (DataGridView dgv in new List<DataGridView> { dgv_loadFiles1 })
            {
                dgv.Refresh();
            }

            if (MdiParent != null) //doesn't work first time
            {
                ((ProteoformSweet)MdiParent).enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
                ((ProteoformSweet)MdiParent).enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]);
        }*/

        #endregion DGV DRAG AND DROP Private Methods
    }
}
