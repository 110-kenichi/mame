/* FILE and FOLDER List Control version 1.0
 * Author: Jayson Ragasa
 * Date: May 03, 2012
 * ----------------------------------------------------
 * License:
 * You are granted to use this control and modify the code and use it in any commercial application
 */
namespace zanac.MAmidiMEmo.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    enum Types
    {
        FILE, FOLDER
    }

    struct ItemType
    {
        public object ItemInfo;
        public Types Type;
        public bool Invalid;
    }

    public partial class FileFolderList : ListView
    {
        Win32 win32 = new Win32();
        ImageList il16 = new ImageList();
        ImageList il32 = new ImageList();
        BackgroundWorker bgIconLoader = new BackgroundWorker();
        List<ItemType> Paths = new List<ItemType>();
        bool use16 = true;

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string[] FilterExts
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<FileSystemInfo, bool> FileValidator
        {
            get;
            set;
        }

        public event EventHandler CurrentDirectoryChanged;

        string f_CurrentPath = "C:\\";
        public string CurrentDirectory
        {
            get { return this.f_CurrentPath; }
            set
            {
                if (this.f_CurrentPath != value)
                {
                    this.f_CurrentPath = value;
                    CurrentDirectoryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        string[] f_SelectedPaths;
        public string[] SelectedPaths
        {
            get { return this.f_SelectedPaths; }
        }

        bool _isSoloBrowser = true;
        public bool isSoloBrowser
        {
            get { return this._isSoloBrowser; }
            set { this._isSoloBrowser = value; }
        }
        #endregion

        public FileFolderList()
        {
            this.il16.ColorDepth = ColorDepth.Depth32Bit;
            this.il32.ColorDepth = ColorDepth.Depth32Bit;
            this.il32.ImageSize = new Size(32, 32);

            this.SmallImageList = il16;
            this.LargeImageList = il32;
            //this.View = System.Windows.Forms.View.Details;
            this.Activation = ItemActivation.TwoClick;

            //if (this.View == System.Windows.Forms.View.Details)
            {
                this.Columns.Add("colName", "Name");
                this.Columns.Add("colType", "Type");
                this.Columns.Add("Size", 1, HorizontalAlignment.Right);
                this.Columns.Add("colDate", "Date");
            }

            this.bgIconLoader.WorkerReportsProgress = true;
            this.bgIconLoader.WorkerSupportsCancellation = true;
            this.bgIconLoader.DoWork += new DoWorkEventHandler(bgIconLoader_DoWork);
            this.bgIconLoader.ProgressChanged += new ProgressChangedEventHandler(bgIconLoader_ProgressChanged);
            this.bgIconLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgIconLoader_RunWorkerCompleted);

            this.ItemActivate += new EventHandler(FileFolderList_ItemActivate);
            this.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(FileFolderList_ItemSelectionChanged);
        }

        private void listView1_DrawItem(object sender,
    DrawListViewItemEventArgs e)
        {
            if ((e.State & ListViewItemStates.Selected) != 0)
            {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Maroon, e.Bounds);
                e.DrawFocusRectangle();
            }
            else
            {
                // Draw the background for an unselected item.
                using (LinearGradientBrush brush =
                    new LinearGradientBrush(e.Bounds, Color.Orange,
                    Color.Maroon, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            // Draw the item text for views other than the Details view.
            if (View != View.Details)
            {
                e.DrawText();
            }
        }

        void bgIconLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                if (this.IsDisposed)
                    return;
                AutoColResize();
            }));
        }

        void bgIconLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                string fullname = (string)e.UserState;
                string name = Path.GetFileName((string)e.UserState);
                ListViewItem item = null;

                this.Invoke(new MethodInvoker(delegate
                {
                    item = FindItemWithText(name, false, 0, true);
                }));

                if (item != null)
                {
                    try
                    {
                        //if (use16)
                        //{
                        //    if (!il16.Images.ContainsKey(fullname))
                        //    {
                        //        il16.Images.Add(fullname, this.win32.GetIcon(fullname, true));
                        //    }
                        //}
                        //else
                        //{
                        //    if (!il32.Images.ContainsKey(fullname))
                        //    {
                        //        il32.Images.Add(fullname, this.win32.GetIcon(fullname, false));
                        //    }
                        //}

                        System.Diagnostics.Debug.WriteLine(fullname);

                        item.ImageKey = fullname;
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        Application.DoEvents();
                    }
                }
            }
        }
        
        void bgIconLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            string fullname = string.Empty;

            foreach (ItemType item in this.Paths)
            {
                if (this.bgIconLoader.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                #region // get associated icon
                if (item.Type == Types.FOLDER)
                {
                    DirectoryInfo di = (DirectoryInfo)item.ItemInfo;

                    if (use16)
                    {
                        if (!il16.Images.ContainsKey(di.FullName))
                        {
                            il16.Images.Add(di.FullName, this.win32.GetIcon(di.FullName, true));
                        }
                    }
                    else
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            if (!il32.Images.ContainsKey(di.FullName))
                            {
                                il32.Images.Add(di.FullName, this.win32.GetIcon(di.FullName, false));
                            }
                        }));
                    }

                    //this.bgIconLoader.ReportProgress(1, di.FullName);
                    fullname = di.FullName;
                }
                else
                {
                    FileInfo fi = (FileInfo)item.ItemInfo;

                    if (use16)
                    {

                        if (!il16.Images.ContainsKey(fi.FullName))
                        {
                            il16.Images.Add(fi.FullName, this.win32.GetIcon(fi.FullName, true));
                        }
                    }
                    else
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            if (!il32.Images.ContainsKey(fi.FullName))
                            {
                                il32.Images.Add(fi.FullName, this.win32.GetIcon(fi.FullName, false));
                            }
                        }));
                    }

                    //this.bgIconLoader.ReportProgress(1, fi.FullName);
                    fullname = fi.FullName;
                }
                #endregion

                #region // add to listview
                string name = Path.GetFileName(fullname);
                ListViewItem lvItem = null;

                this.Invoke(new MethodInvoker(delegate
                {
                    if (this.IsDisposed)
                        return;
                    lvItem = FindItemWithText(name, false, 0, true);
                }));

                if (lvItem != null)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        if (this.IsDisposed)
                            return;

                        lvItem.ImageKey = fullname;

                        //if (View == System.Windows.Forms.View.Details)
                        {
                            if (item.Type == Types.FILE)
                            {
                                lvItem.SubItems[1].Text = win32.GetFileType(fullname);
                                lvItem.SubItems[2].Text = win32.GetFileSize(fullname);
                            }
                        }
                    }));
                }
                #endregion
            }
        }

        void FileFolderList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (this.SelectedItems.Count <= 0) { return; }
            if (!e.IsSelected)
                return;

            //try
            {
                List<string> files = new List<string>();
                foreach (ListViewItem item in this.SelectedItems)
                {
                    ItemType type = (ItemType)item.Tag;
                    if (type.Invalid)
                        continue;

                    if (type.Type == Types.FOLDER)
                    {
                        DirectoryInfo di = (DirectoryInfo)type.ItemInfo;
                        files.Add(di.FullName);
                    }
                    else
                    {
                        FileInfo fi = (FileInfo)type.ItemInfo;
                        files.Add(fi.FullName);
                    }
                }
                f_SelectedPaths = files.ToArray();
            }
            //catch { }
        }

        void FileFolderList_ItemActivate(object sender, EventArgs e)
        {
            if (this.SelectedItems.Count <= 0) { return; }

            ListViewItem thisItem = this.SelectedItems[0];
            ItemType type = (ItemType)thisItem.Tag;

            if (type.Type == Types.FOLDER)
            {
                DirectoryInfo di = (DirectoryInfo)type.ItemInfo;
                Browse(di.FullName);
            }
        }

        public void Browse(string path)
        {
            this.bgIconLoader.CancelAsync();

            if (View == System.Windows.Forms.View.LargeIcon || View == System.Windows.Forms.View.Tile)
            {
                use16 = false;
            }

            while (this.bgIconLoader.IsBusy)
            {
                Application.DoEvents();
            }

            this.Paths.Clear();
            this.Items.Clear();

            this.BeginUpdate();

            #region // add "back" item if necessary
            if (this._isSoloBrowser)
            {
                DirectoryInfo currentPath = new DirectoryInfo(path);
                if (currentPath.FullName.Length > 3)
                {
                    ListViewItem item = new ListViewItem("...");
                    item.Tag = new ItemType()
                    {
                        ItemInfo = currentPath.Parent,
                        Type = Types.FOLDER
                    };
                    Items.Add(item);
                }
            }
            #endregion

            #region // get folders
            foreach (string folder in Directory.GetDirectories(path))
            {
                DirectoryInfo di = new DirectoryInfo(folder);
                    
                if (di.Attributes.ToString().Contains("System"))
                {
                    continue;
                }

                ListViewItem item = new ListViewItem(di.Name);

                bool invalid = false;
                if (FileValidator != null && !FileValidator(di))
                {
                    item.ForeColor = SystemColors.GrayText;
                    invalid = true;
                }

                //if (!il16.Images.ContainsKey(di.FullName))
                //{
                //    Icon ico32;
                //    il16.Images.Add(di.FullName, this.win32.GetIcon(di.FullName, out ico32));
                //    il32.Images.Add(di.FullName, ico32);
                //}

                item.ImageKey = di.FullName;
                item.Tag = new ItemType()
                {
                    ItemInfo = di,
                    Type = Types.FOLDER,
                    Invalid = invalid
                };

                // add temp subitems if View was set to Details
                //if (this.View == System.Windows.Forms.View.Details)
                {
                    for (int i = 0; i < this.Columns.Count; i++)
                    {
                        item.SubItems.Add(string.Empty);
                    }

                    item.SubItems[3].Text = di.CreationTime.ToString();
                    // key should be "colType" but am not sure
                    // why it does not work
                    item.SubItems[1].Text = "File folder";
                }

                Items.Add(item);

                Paths.Add((ItemType)item.Tag);
            }
            #endregion

            #region // get files
            foreach (string file in Directory.GetFiles(path))
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Attributes.ToString().Contains("System"))
                {
                    continue;
                }

                ListViewItem item = new ListViewItem(fi.Name);


                bool ignore = true;
                if (FilterExts != null)
                {
                    foreach (String ext in FilterExts)
                    {
                        if (string.Equals(fi.Extension, ext, StringComparison.OrdinalIgnoreCase))
                        {
                            ignore = false;
                            break;
                        }
                    }
                }
                else
                {
                    ignore = false;
                }
                bool invalid = false;
                if (ignore || (FileValidator != null && !FileValidator(fi)))
                {
                    item.ForeColor = SystemColors.GrayText;
                    invalid = true;
                }
                else
                    item.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);

                //if (!il16.Images.ContainsKey(fi.FullName))
                //{
                //    Icon ico32;
                //    il16.Images.Add(fi.FullName, this.win32.GetIcon(fi.FullName, out ico32));
                //    il32.Images.Add(fi.FullName, ico32);
                //}

                item.ImageKey = fi.FullName;
                item.Tag = new ItemType()
                {
                    ItemInfo = fi,
                    Type = Types.FILE,
                    Invalid = invalid
                };

                // add temp subitems if View was set to Details
                //if (this.View == System.Windows.Forms.View.Details)
                {
                    for (int i = 0; i < this.Columns.Count; i++)
                    {
                        item.SubItems.Add(string.Empty);
                    }

                    item.SubItems[3].Text = fi.CreationTime.ToString();
                }

                Items.Add(item);

                Paths.Add((ItemType)item.Tag);
            }
            #endregion

            this.EndUpdate();
            this.Refresh();

            CurrentDirectory = path;

            bgIconLoader.RunWorkerAsync();
        }

        void AutoColResize()
        {
            foreach (ColumnHeader col in this.Columns)
            {
                //this.Invoke(new MethodInvoker(delegate
                //{
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                //col.Width = -2;
                //}));
            }
        }
    }

    internal class Win32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        static class FILE_ATTRIBUTE
        {
            public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        }

        static class SHGFI
        {
            public const uint SHGFI_TYPENAME = 0x000000400;
            public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        }

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SHGetFileInfo(string pszPath,
                                    uint dwFileAttributes,
                                    ref SHFILEINFO psfi,
                                    uint cbSizeFileInfo,
                                    uint uFlags);

        [DllImport("user32")]
        public static extern int DestroyIcon(IntPtr hIcon);

        public string GetFileSize(string fullpath)
        {
            FileInfo fi = new FileInfo(fullpath);
            long size = fi.Length;
            string sizeString = String.Format(new FileSizeFormatProvider(), "{0:fs}", size);

            return sizeString;
        }

        public string GetFileType(string fullpath)
        {
            uint dwFileAttributes = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
            uint uFlags = (uint)(SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr n = Win32.SHGetFileInfo(fullpath, dwFileAttributes, ref shinfo, (uint)Marshal.SizeOf(shinfo), uFlags);

            return shinfo.szTypeName;
        }

        public Icon GetIcon(string fullpath, bool use16)
        {
            Icon ico = null;
            IntPtr hImgSmall;
            IntPtr hImgLarge;
            SHFILEINFO shinfo = new SHFILEINFO();

            if (use16)
            {
                //Use this to get the small Icon
                hImgSmall = Win32.SHGetFileInfo(fullpath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);
                ico = Icon.FromHandle(shinfo.hIcon);
            }
            else
            {
                //Use this to get the large Icon
                hImgLarge = SHGetFileInfo(fullpath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
                ico = Icon.FromHandle(shinfo.hIcon);
            }            

            return ico;
        }
    }

    internal class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }

        private const string fileSizeFormat = "fs";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(fileSizeFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            Decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = " GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = " MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = " KB";
            }
            else
            {
                suffix = " Byte(s)";
            }

            string precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "0";
            return String.Format("{0:N" + precision + "}{1}", size, suffix);

        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }
            return arg.ToString();
        }

    }
}
