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
            this.Header = header;
        }
        private ClosingTabHeader header;
        public string Title
        {
            set {
                ((ClosingTabHeader)this.header).tab_title.Content = value;
            }
            get {
                return ((ClosingTabHeader)this.header).tab_title.Content as string;
            }
        }
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
        }
        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (freeze)
            {
                return;
            }
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }
        // Button MouseEnter - When the mouse is over the button - change color to Red
        
        
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
       
    }
}
