using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Util
{
    internal class PropertyGridUtility
    {

        public static ScrollBar GetPropertyGridViewScrollBar(PropertyGrid propertyGrid)
        {
            if (propertyGrid == null)
                throw new ArgumentNullException("propertyGrid");

            foreach (Control c in propertyGrid.Controls)
            {
                if (IsPropertyGridView(c))
                {
                    PropertyInfo pi = c.GetType().GetProperty("ScrollBar",
                        BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    return (ScrollBar)pi.GetValue(c, null);
                }
            }
            return null;
        }

        public static bool IsPropertyGridView(Control control)
        {
            if (control == null)
                return false;

            try
            {
                if (control.GetType().FullName.Equals("System.Windows.Forms.PropertyGridInternal.PropertyGridView",
                    StringComparison.Ordinal))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                return false;
            }
        }

        public static void SetPropertyGridViewScrollOffset(PropertyGrid propertyGrid, int offset)
        {
            if (propertyGrid == null)
                throw new ArgumentNullException("propertyGrid");

            foreach (Control c in propertyGrid.Controls)
            {
                if (IsPropertyGridView(c))
                {
                    MethodInfo mi = c.GetType().GetMethod("SetScrollOffset",
                        BindingFlags.InvokeMethod | BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(c, new object[] { offset });
                    break;
                }
            }
        }

        public static bool GridViewEditVisible(PropertyGrid propertyGrid)
        {
            if (propertyGrid == null)
                throw new ArgumentNullException("propertyGrid");

            foreach (Control c in propertyGrid.Controls)
            {
                if (IsPropertyGridView(c))
                {
                    PropertyInfo pi = c.GetType().GetProperty("Edit",
                        BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    TextBox editBox = (TextBox)pi.GetValue(c, null);
                    return editBox.Visible;
                }
            }
            return false;
        }

        public static void CommonEditorHide(PropertyGrid propertyGrid)
        {
            if (propertyGrid == null)
                throw new ArgumentNullException("propertyGrid");

            foreach (Control c in propertyGrid.Controls)
            {
                if (IsPropertyGridView(c))
                {
                    MethodInfo mi = c.GetType().GetMethod("CommonEditorHide",
                        BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new Type[] { }, null);
                    mi.Invoke(c, null);
                }
            }
        }


        public static void SelectGridItem(PropertyGrid propertyGrid, string categoryName, string memberName)
        {
            GridItem gi = FindTopGridItem(propertyGrid);
            while (gi != null)
            {
                gi = GetNextGridItem(gi, false);
                if (gi != null)
                {
                    if (string.Equals(categoryName, gi.Label, StringComparison.Ordinal))
                    {
                        if (memberName == null)
                        {
                            SelectGridItem(gi);
                            break;
                        }
                        else
                        {
                            if (gi.PropertyDescriptor != null &&
                                string.Equals(gi.PropertyDescriptor.Name, memberName, StringComparison.Ordinal))
                            {
                                SelectGridItem(gi);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static GridItem FindTopGridItem(PropertyGrid propertyGrid)
        {
            GridItem gi = propertyGrid.SelectedGridItem;
            while (gi != null)
            {
                if (gi.Parent == null)
                    break;
                gi = gi.Parent;
            }
            return gi;
        }


        public static GridItem GetNextGridItem(GridItem gridItem, bool skipChild)
        {
            if (gridItem == null)
                return null;

            if (gridItem.GridItems.Count != 0 && !skipChild)
                return gridItem.GridItems[0];

            GridItem ngi = null;
            if (gridItem.Parent != null)
            {
                for (int i = 0; i < gridItem.Parent.GridItems.Count; i++)
                {
                    GridItem gi = gridItem.Parent.GridItems[i];
                    if (gi == gridItem)
                    {
                        if (i < gridItem.Parent.GridItems.Count - 1)
                        {
                            ngi = gridItem.Parent.GridItems[i + 1];
                            break;
                        }
                        else
                        {
                            ngi = GetNextGridItem(gridItem.Parent, true);
                            break;
                        }
                    }
                }
            }
            return ngi;
        }

        public static void SelectGridItem(GridItem gridItem)
        {
            if (gridItem == null)
                return;

            GridItem parent = gridItem.Parent;
            while (parent != null)
            {
                if (parent.Expandable)
                    parent.Expanded = true;
                parent = parent.Parent;
            }

            gridItem.Select();
            PropertyGridUtility.SetFocus(gridItem, true);
        }

        public static void SetFocus(GridItem gridItem, bool value)
        {
            if (gridItem == null)
                throw new ArgumentNullException("gridItem");

            if (gridItem.GetType().Name.EndsWith("GridEntry", StringComparison.Ordinal))
            {
                Type t = gridItem.GetType();
                PropertyInfo mi = t.BaseType.GetProperty("Focus",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                mi.SetValue(gridItem, value, null);
            }
        }

        public static void BeginUpdate(Control control)
        {
            if (control != null && !control.IsDisposed)
                BeginUpdateInternalMethodInfo.Invoke(control, null);
        }

        public static void EndUpdate(Control control)
        {
            if (control != null && !control.IsDisposed)
                EndUpdateInternalMethodInfo.Invoke(control, null);
        }

        private static MethodInfo beginUpdateInternalMethodInfo;

        private static MethodInfo BeginUpdateInternalMethodInfo
        {
            get
            {
                if (beginUpdateInternalMethodInfo == null)
                    beginUpdateInternalMethodInfo = typeof(Control).
                        GetMethod("BeginUpdateInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                return beginUpdateInternalMethodInfo;
            }
        }

        private static MethodInfo endUpdateInternalMethodInfo;

        private static MethodInfo EndUpdateInternalMethodInfo
        {
            get
            {
                if (endUpdateInternalMethodInfo == null)
                    endUpdateInternalMethodInfo = typeof(Control).
                        GetMethod("EndUpdateInternal", BindingFlags.NonPublic | BindingFlags.Instance,
                        null, new Type[0], null);
                return endUpdateInternalMethodInfo;
            }
        }
    }
}
