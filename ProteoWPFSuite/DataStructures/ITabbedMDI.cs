/*
 Author: Sohel_Rana
 Link:   https://www.codeproject.com/Articles/32362/Tabbed-MDI-in-WPF
 Modified by: Smith Group
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteoWPFSuite
{
    public delegate void delClosed(ITabbedMDI sender, EventArgs e);
    public interface ITabbedMDI
    {
        /// <summary>
        /// When the MDI child is closed, parent will be informed and remove the mdi child from dictionary.
        /// </summary>
        event delClosed BeingClosed;
        
        string UniqueTabName { get; }

        /// <summary>
        /// even raiser that help raise the cloing event
        /// </summary>
        void OnClosing(ITabbedMDI sender);
    }
}
