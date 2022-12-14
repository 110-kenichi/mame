using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// Dragging items in a ListView control with visual insertion guides
// http://www.cyotek.com/blog/dragging-items-in-a-listview-control-with-visual-insertion-guides

namespace ListViewInsertionDrag
{
    public class DraggableListView : System.Windows.Forms.ListView
    {
        #region Instance Fields

        private bool _allowItemDrag;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DraggableListView"/> class.
        /// </summary>
        public DraggableListView()
        {
            this.InsertionMark.Index = -1;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the AllowItemDrag property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowItemDragChanged;

        /// <summary>
        /// Occurs when a drag-and-drop operation for an item is completed.
        /// </summary>
        [Category("Drag Drop")]
        public event EventHandler<ListViewItemDragEventArgs> ItemDragDrop;

        /// <summary>
        /// Occurs when the user begins dragging an item.
        /// </summary>
        [Category("Drag Drop")]
        public event EventHandler<CancelListViewItemDragEventArgs> ItemDragging;

        #endregion

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            SetWindowTheme(Handle, "Explorer", null);
        }

        #region Overridden Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragDrop" /> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs" /> that contains the event data.</param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (this.IsRowDragInProgress)
            {
                try
                {
                    ListViewItem dropItem;

                    dropItem = this.InsertionMark.Index != -1 ? this.Items[this.InsertionMark.Index] : null;

                    if (dropItem != null)
                    {
                        int dropIndex = dropItem.Index;
                        if (this.InsertionMode == InsertionMode.After)
                            dropIndex++;

                        ArrayList insertItems = new ArrayList(base.SelectedItems.Count);
                        ListViewItem focusedItem = null;
                        foreach (ListViewItem item in base.SelectedItems)
                        {
                            var citem = item.Clone();
                            if (item.Focused)
                                focusedItem = (ListViewItem)citem;
                            insertItems.Add(citem);
                        }
                        for (int i = insertItems.Count - 1; i >= 0; i--)
                        {
                            ListViewItem insertItem = (ListViewItem)insertItems[i];
                            base.Items.Insert(dropIndex, insertItem);
                        }
                        foreach (ListViewItem removeItem in base.SelectedItems)
                            base.Items.Remove(removeItem);
                        for (int i = insertItems.Count - 1; i >= 0; i--)
                        {
                            ListViewItem insertItem = (ListViewItem)insertItems[i];
                            insertItem.Selected = true;
                        }
                        focusedItem.Focused = true;
                    }
                }
                finally
                {
                    this.InsertionMark.Index = -1;
                    this.IsRowDragInProgress = false;
                }
            }

            base.OnDragDrop(drgevent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragLeave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnDragLeave(EventArgs e)
        {
            this.InsertionMark.Index = -1;

            base.OnDragLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragOver" /> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs" /> that contains the event data.</param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            if (this.IsRowDragInProgress)
            {
                int insertionIndex;
                InsertionMode insertionMode;
                ListViewItem dropItem;
                Point clientPoint;

                clientPoint = this.PointToClient(new Point(drgevent.X, drgevent.Y));
                dropItem = this.GetItemAt(0, Math.Min(clientPoint.Y, this.Items[this.Items.Count - 1].GetBounds(ItemBoundsPortion.Entire).Bottom - 1));

                if (dropItem != null)
                {
                    Rectangle bounds;

                    bounds = dropItem.GetBounds(ItemBoundsPortion.Entire);
                    insertionIndex = dropItem.Index;
                    insertionMode = clientPoint.Y < bounds.Top + (bounds.Height / 2) ? InsertionMode.Before : InsertionMode.After;

                    drgevent.Effect = DragDropEffects.Move;
                }
                else
                {
                    insertionIndex = -1;
                    insertionMode = this.InsertionMode;

                    InsertionMark.Index = -1;

                    drgevent.Effect = DragDropEffects.None;
                }

                if (insertionIndex != this.InsertionMark.Index || insertionMode != this.InsertionMode)
                {
                    this.InsertionMode = insertionMode;
                    this.InsertionMark.Index = insertionIndex;
                }
            }

            // Scroll top or bottom for dragging item
            if (this.InsertionMark.Index > -1 && this.InsertionMark.Index < this.Items.Count)
            {
                EnsureVisible(this.InsertionMark.Index);
            }

            base.OnDragOver(drgevent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ListView.ItemDrag" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.ItemDragEventArgs" /> that contains the event data.</param>
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            if (this.AllowItemDrag && this.Items.Count > 1)
            {
                CancelListViewItemDragEventArgs args;

                args = new CancelListViewItemDragEventArgs((ListViewItem)e.Item);

                this.OnItemDragging(args);

                if (!args.Cancel)
                {
                    this.IsRowDragInProgress = true;
                    this.DoDragDrop(e.Item, DragDropEffects.Move);
                }
            }

            base.OnItemDrag(e);
        }

        #endregion

        #region Public Properties

        [Category("Behavior")]
        [DefaultValue(false)]
        public virtual bool AllowItemDrag
        {
            get { return _allowItemDrag; }
            set
            {
                if (this.AllowItemDrag != value)
                {
                    _allowItemDrag = value;

                    this.OnAllowItemDragChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ListViewItem"/>.
        /// </summary>
        /// <value>The selected <see cref="ListViewItem"/>.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListViewItem SelectedItem
        {
            get { return this.SelectedItems.Count != 0 ? this.SelectedItems[0] : null; }
            set
            {
                this.SelectedItems.Clear();
                if (value != null)
                {
                    value.Selected = true;
                }
                this.FocusedItem = value;
            }
        }

        #endregion

        #region Protected Properties

        private InsertionMode insertionMode;

        /// <summary>
        /// 
        /// </summary>
        protected InsertionMode InsertionMode
        {
            get
            {
                return insertionMode;
            }
            set
            {
                insertionMode = value;
                InsertionMark.AppearsAfterItem = insertionMode == InsertionMode.After;
            }
        }

        protected bool IsRowDragInProgress { get; set; }

        #endregion

        #region Protected Members

        /// <summary>
        /// Raises the <see cref="AllowItemDragChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnAllowItemDragChanged(EventArgs e)
        {
            EventHandler handler;

            handler = this.AllowItemDragChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemDragDrop" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ListViewItemDragEventArgs" /> instance containing the event data.</param>
        protected virtual void OnItemDragDrop(ListViewItemDragEventArgs e)
        {
            EventHandler<ListViewItemDragEventArgs> handler;

            handler = this.ItemDragDrop;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemDragging" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelListViewItemDragEventArgs" /> instance containing the event data.</param>
        protected virtual void OnItemDragging(CancelListViewItemDragEventArgs e)
        {
            EventHandler<CancelListViewItemDragEventArgs> handler;

            handler = this.ItemDragging;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
