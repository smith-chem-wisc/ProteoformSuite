using System.Windows;
using System.Windows.Media;
namespace ProteoWPFSuite
{
    public class MDIHelpers
    {
        /// <summary>
        /// Used for locating parent window; as a equicalent for midParent
        /// </summary>
        /// <param name="child">the control needing to find its parent</param>
        /// <returns>the parent winow if any; or null if there isn't any</returns>
        public static Window getParentWindow(DependencyObject child)
        {

            if (child == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while ((parent != null) && ((parent as Window) == null))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent == null) return null;
            else
            {
                return (Window)parent;
            }
        }
    }
}
