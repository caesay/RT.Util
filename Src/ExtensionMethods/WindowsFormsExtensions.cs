﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RT.Util.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods for Windows Forms controls.
    /// </summary>
    public static class WindowsFormsExtensions
    {
        /// <summary>
        /// If this control is located within a <see cref="TabPage"/>, returns that TabPage
        /// by iterating recursively through this item's parents. Otherwise returns null.
        /// </summary>
        public static TabPage ParentTab(this Control control)
        {
            while (control != null)
            {
                if (control.Parent is TabPage)
                    return control.Parent as TabPage;
                control = control.Parent;
            }
            return null;
        }
    }
}
