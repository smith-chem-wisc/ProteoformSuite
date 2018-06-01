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
        /// This event will be fired from user control and will be listened
        /// by parent MDI form. when this event will be fired from child control
        /// parent will close the form
        /// </summary>
        event delClosed OnClosing;
        /// <summary>
        /// This is unique name for the control. This will be used in dictonary object
        /// to keep track of the opened user control in parent form.
        /// </summary>
        string UniqueTabName { get; }
    }
}
