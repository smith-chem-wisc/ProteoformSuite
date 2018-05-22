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
                bt_calibrate.Visible = true;
                cb_calibrate_td_files.Visible = true;
                cb_calibrate_raw_files.Visible = true;
                bt_deconvolute.Visible = false;
                bt_stepthru.Visible = false;
                bt_fullrun.Visible = false;
                bt_calibrate.Visible = true;
                panel_deconv_calib.Visible = true;
                panel_step.Visible = false;
                nud_maxcharge.Visible = false;
                nud_mincharge.Visible = false;
                label_maxcharge.Visible = false;
                label_mincharge.Visible = false;
                label_maxRT.Visible = false;
                label_minRT.Visible = false;
                rb_neucode.Visible = true;
                rb_unlabeled.Visible = true;
                calib_stand_splitContainer.Visible = true;
                fullrun_groupbox.Visible = false;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = true;
            }

            else if (rb_deconvolution.Checked)
            {

                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = false;

                bt_calibrate.Visible = false;
                cb_calibrate_raw_files.Visible = false;
                cb_calibrate_td_files.Visible = false;
                bt_stepthru.Visible = false;
                bt_fullrun.Visible = false;
                bt_calibrate.Visible = false;
                bt_deconvolute.Visible = true;
                panel_deconv_calib.Visible = true;
                panel_step.Visible = false;
                nud_maxcharge.Visible = true;
                nud_mincharge.Visible = true;
                label_maxcharge.Visible = true;
                label_mincharge.Visible = true;
                label_maxRT.Visible = true;
                label_minRT.Visible = true;
                rb_neucode.Visible = false;
                rb_unlabeled.Visible = false;
                calib_stand_splitContainer.Visible = false;
                fullrun_groupbox.Visible = false;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.Enabled = false;
            }

            lb_filter1.Text = cmb_loadTable1.SelectedItem.ToString();

            reload_dgvs();
            refresh_dgvs();
        }
        #endregion GENERAL TABLE OPTIONS Private Methods
    }
}
