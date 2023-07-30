using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.SCC1;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormSccMorphEditor : FormBase
    {
        private SCC1 inst;

        private SCC1Timbre timbre;

        private int timbreNo;

        /// <summary>
        /// 
        /// </summary>
        public FormSccMorphEditor()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public FormSccMorphEditor(SCC1 inst, SCC1Timbre timbre)
        {
            this.inst = inst;
            this.timbre = timbre;
            for (int i = 0; i < inst.BaseTimbres.Length; i++)
            {
                if (inst.BaseTimbres[i] == timbre)
                {
                    timbreNo = i;
                    break;
                }
            }

            InitializeComponent();

            Size = Settings.Default.WsgTypeEdSize;

            listBoxWsgList.Items.Add(new WsgDataItem(timbre, listBoxWsgList));
            for (int i = 0; i < timbre.WsgDataMorphs.Count; i++)
                listBoxWsgList.Items.Add(new WsgDataItem(timbre, listBoxWsgList));
            listBoxWsgList.SelectedIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.WsgTypeEdSize = Size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButtonAdd_Click(object sender, EventArgs e)
        {
            timbre.WsgDataMorphs.Add(new SCCWsgMorphData());
            listBoxWsgList.Items.Add(new WsgDataItem(timbre, listBoxWsgList));

            listBoxWsgList.DisplayMember = "";
            listBoxWsgList.DisplayMember = "Name";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButtonInsert_Click(object sender, EventArgs e)
        {
            if (listBoxWsgList.SelectedIndex == 0)
            {
                listBoxWsgList.Items.Add(new WsgDataItem(timbre, listBoxWsgList));
                timbre.WsgDataMorphs.Add(new SCCWsgMorphData());
            }
            else
            {
                int idx = listBoxWsgList.SelectedIndex;
                listBoxWsgList.Items.Insert(idx, new WsgDataItem(timbre, listBoxWsgList));
                timbre.WsgDataMorphs.Insert(idx - 1, new SCCWsgMorphData());
            }

            listBoxWsgList.DisplayMember = "";
            listBoxWsgList.DisplayMember = "Name";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButtonDuplicate_Click(object sender, EventArgs e)
        {
            int idx = listBoxWsgList.SelectedIndex;
            listBoxWsgList.Items.Insert(idx + 1, new WsgDataItem(timbre, listBoxWsgList));
            timbre.WsgDataMorphs.Insert(idx, new SCCWsgMorphData());

            if (idx == 0)
            {
                timbre.WsgDataMorphs[idx].WsgDataSerializeData = timbre.WsgDataSerializeData;
            }
            else
            {
                timbre.WsgDataMorphs[idx].WsgDataSerializeData = timbre.WsgDataMorphs[idx - 1].WsgDataSerializeData;
            }

            listBoxWsgList.DisplayMember = "";
            listBoxWsgList.DisplayMember = "Name";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButtonDelete_Click(object sender, EventArgs e)
        {
            int lidx = 0;
            for (int i = listBoxWsgList.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int idx = listBoxWsgList.SelectedIndices[i];
                if (idx != 0)
                {
                    listBoxWsgList.Items.RemoveAt(idx);
                    timbre.WsgDataMorphs.RemoveAt(idx - 1);
                }
                lidx = idx;
            }
            if (lidx < listBoxWsgList.Items.Count)
                listBoxWsgList.SelectedIndex = lidx;
            else
                listBoxWsgList.SelectedIndex = listBoxWsgList.Items.Count - 1;

            listBoxWsgList.DisplayMember = "";
            listBoxWsgList.DisplayMember = "Name";
        }

        private class WsgDataItem
        {
            private ListBox listbox;

            /// <summary>
            /// 
            /// </summary>
            public int Index
            {
                get
                {
                    return listbox.Items.IndexOf(this);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public string Name
            {
                get
                {
                    return ToString();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public SCC1Timbre Timbre
            {
                get;
                private set;
            }

            /// <summary>
            /// 
            /// </summary>
            public WsgDataItem(SCC1Timbre timbre, ListBox listbox)
            {
                Timbre = timbre;
                this.listbox = listbox;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (Index == 0)
                    return "WsgData";
                return "WsgDataMorphs[" + (Index - 1).ToString() + "]";
            }
        }

        private void listBoxWsgList_SelectedValueChanged(object sender, EventArgs e)
        {
            var wd = (WsgDataItem)listBoxWsgList.SelectedItem;
            if (wd == null || wd.Index < 0)
                return;

            if (wd.Index == 0)
                propertyGrid1.SelectedObject = new SCCWsgMorphData(wd.Timbre.WsgData);
            else
                propertyGrid1.SelectedObject = wd.Timbre.WsgDataMorphs[wd.Index - 1];
        }

        private void metroButtonInterpolate_Click(object sender, EventArgs e)
        {
            int idx = listBoxWsgList.SelectedIndex;
            if (idx < 0)
                return;
            if (idx == listBoxWsgList.Items.Count - 1)
            {
                MessageBox.Show(Resources.WsgMorphError1);
                return;
            }

            using (FormInputNumberScc f = new FormInputNumberScc())
            {
                f.TitleText = Resources.WsgMorphTitle;
                var r = f.ShowDialog();
                if (r == DialogResult.OK)
                {
                    int num = (int)f.InputValue;

                    WsgDataItem first = (WsgDataItem)listBoxWsgList.Items[idx];
                    sbyte[] firstData;
                    if (first.Index == 0)
                        firstData = first.Timbre.WsgData;
                    else
                        firstData = first.Timbre.WsgDataMorphs[first.Index - 1].WsgData;

                    WsgDataItem last = (WsgDataItem)listBoxWsgList.Items[idx + 1];
                    sbyte[] lastData = last.Timbre.WsgDataMorphs[last.Index - 1].WsgData;

                    List<WsgDataItem> list = new List<WsgDataItem>();
                    if (f.MethodSimple)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            listBoxWsgList.Items.Insert(idx + 1, new WsgDataItem(timbre, listBoxWsgList));
                            timbre.WsgDataMorphs.Insert(idx, new SCCWsgMorphData());
                            for (int wi = 0; wi < firstData.Length; wi++)
                            {
                                var diff = (double)lastData[wi] - (double)firstData[wi];
                                var step = diff / (num + 1);
                                timbre.WsgDataMorphs[idx].WsgData[wi] = (sbyte)(Math.Round((double)firstData[wi] + step * (i + 1)));
                            }
                            idx++;
                        }
                    }

                    listBoxWsgList.DisplayMember = "";
                    listBoxWsgList.DisplayMember = "Name";
                }
            }
        }

        private void metroButtonCopy_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listBoxWsgList.Items.Count; i++)
            {
                WsgDataItem item = (WsgDataItem)listBoxWsgList.Items[i];
                if (i == 0)
                    sb.AppendLine(createWsgDataMmlData(item.Timbre.WsgData));
                else
                    sb.AppendLine(createWsgDataMmlData(item.Timbre.WsgDataMorphs[i - 1].WsgData));
            }
            Clipboard.SetText(sb.ToString());
        }

        private static string createWsgDataMmlData(sbyte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (sb.Length != 0)
                    sb.Append(' ');
                sb.Append(data[i].ToString("X2", (IFormatProvider)null));
            }
            return sb.ToString();
        }

    }
}

