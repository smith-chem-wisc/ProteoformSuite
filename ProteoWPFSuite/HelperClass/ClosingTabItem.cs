using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProteoWPFSuite
{
    

    class ClosingTabItem: TabItem
    {
        public static Dictionary<string, int> tabTable=new Dictionary<string, int>();
        public bool freeze;
        public ClosingTabItem()
        {
            freeze = false;
            header = new ClosingTabHeader();
            header.tab_close.MouseEnter += new MouseEventHandler(button_close_MouseEnter);
            header.tab_close.MouseLeave += new MouseEventHandler(button_close_MouseLeave);
            header.tab_close.Click += new RoutedEventHandler(button_close_Click);
            header.tab_title.SizeChanged += new SizeChangedEventHandler(label_TabTitle_SizeChanged);
            this.Header = header;

        }
        private ClosingTabHeader header;
        public string Title
        {
            set {
                ((ClosingTabHeader)this.header).tab_title.Content = value;
            }
        }
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((ClosingTabHeader)this.Header).tab_close.Visibility = Visibility.Visible;
        }
        protected override void OnUnselected(RoutedEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            base.OnUnselected(e);
            ((ClosingTabHeader)this.Header).tab_close.Visibility = Visibility.Hidden;
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            base.OnMouseEnter(e);
            ((ClosingTabHeader)this.Header).tab_close.Visibility = Visibility.Visible;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                ((ClosingTabHeader)this.Header).tab_close.Visibility = Visibility.Hidden;
            }
        }
        // Button MouseEnter - When the mouse is over the button - change color to Red
        private void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            ((ClosingTabHeader)this.Header).tab_close.Foreground = Brushes.Red;
        }
        // Button MouseLeave - When mouse is no longer over button - change color back to black
        private void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            ((ClosingTabHeader)this.Header).tab_close.Foreground = Brushes.Black;
        }
        // Button Close Click - Remove the Tab - (or raise
        // an event indicating a "CloseTab" event has occurred)
        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            ((TabControl)this.Parent).Items.Remove(this);
        }
        // Label SizeChanged - When the Size of the Label changes
        // (due to setting the Title) set position of button properly
        private void label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            ((ClosingTabHeader)this.Header).tab_close.Margin = new Thickness(
            ((ClosingTabHeader)this.Header).tab_title.ActualWidth + 5, 3, 4, 0);
        }
    }
}
