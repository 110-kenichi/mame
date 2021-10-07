// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YM3812;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM3812Editor : FormFmEditor
    {
        private YM3812Timbre timbre;

        /// <summary>
        /// 
        /// </summary>
        public string MmlValueGeneral
        {
            get
            {
                return GetControl("General").SerializeData;
            }
            set
            {
                GetControl("General").SerializeData = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] MmlValueOps
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    var op = timbre.Ops[i];
                    string opt = GetControl("Operator " + (i + 1)).SerializeData;
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    var op = timbre.Ops[i];
                    GetControl("Operator " + (i + 1)).SerializeData = value[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM3812Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM3812Editor(YM3812 inst, YM3812Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM3812EdSize;

            AddControl(new YM3812GeneralContainer(inst, timbre, "General"));

            AddControl(new YM3812OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM3812OperatorContainer(timbre.Ops[1], "Operator 2"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM3812EdSize = Size;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string SupportedExtensionsFilter
        {
            get
            {
                return "Tone file(MUCOM88, FMP, PMD, VOPM, GWI, FITOM, OPL)|*.muc;*.dat;*.mwi;*.mml;*.fxb;*.gwi;*.bnk;*.opl";
            }
        }

        class Head
        {
            public byte Patch;
            public byte Bank;
            public uint Offset;
        }

        public override IEnumerable<Tone> ImportToneFile(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            var Option = new Option();
            List<Tone> tones = new List<Tone>();
            try
            {
                switch (ext.ToUpper(CultureInfo.InvariantCulture))
                {
                    case ".OPL":
                        {
                            List<Head> list = new List<Head>();
                            byte maxBankNumber = 0;

                            using (var file = new System.IO.BinaryReader(new System.IO.FileStream(fileName, System.IO.FileMode.Open)))
                            {
                                while (true)
                                {
                                    //read head
                                    Head p = new Head();
                                    p.Patch = file.ReadByte();
                                    p.Bank = file.ReadByte();
                                    p.Offset = file.ReadUInt32();

                                    if (p.Patch == 0xff && p.Bank == 0xff)
                                        break;

                                    if ((p.Bank != 0x7F) && (p.Bank > maxBankNumber))
                                        maxBankNumber = p.Bank;

                                    list.Add(p);
                                };

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var p = list[i];
                                    file.BaseStream.Seek(p.Offset, System.IO.SeekOrigin.Begin);

                                    bool isPerc = (p.Bank == 0x7F);
                                    int gmPatchId = isPerc ? p.Patch : (p.Patch + (p.Bank * 128));

                                    ushort insLen = file.ReadUInt16();
                                    insLen -= 2;

                                    byte[] idata = new byte[24];
                                    if (insLen < 24)
                                    {
                                        idata = file.ReadBytes(insLen);
                                    }
                                    else
                                    {
                                        idata = file.ReadBytes(24);
                                        file.BaseStream.Seek(insLen - 24, System.IO.SeekOrigin.Current);
                                    }

                                    //var tim = Timbres[p.Patch + (isPerc ? 128 : 0)];
                                    Tone tone = new Tone();
                                    tone.Number = i;
                                    tone.Name = i.ToString();

                                    //ins.percNoteNum = (isPerc) ? idata[0] : 0;
                                    //ins.note_offset1 = (isPerc) ? 0 : idata[0];
                                    //if (isPerc)
                                    //{
                                    //    var t = DrumTimbres[p.Patch];
                                    //    t.BaseNote = (NoteNames)idata[0];
                                    //    t.TimbreNumber = (ProgramAssignmentNumber)(p.Patch + 128);
                                    //}

                                    tone.aOp[0].AM = (byte)((idata[1] >> 7) & 0x1);
                                    tone.aOp[0].VIB = (byte)((idata[1] >> 6) & 0x1);
                                    tone.aOp[0].EG = (byte)((idata[1] >> 5) & 0x1);
                                    tone.aOp[0].KSR = (byte)((idata[1] >> 4) & 0x1);
                                    tone.aOp[0].ML = (byte)((idata[1]) & 0xf);

                                    tone.aOp[0].KS = (byte)((idata[2] >> 6) & 0x03);
                                    tone.aOp[0].TL = (byte)((idata[2]) & 0x3f);

                                    tone.aOp[0].AR = (byte)((idata[3] >> 4) & 0x0f);
                                    tone.aOp[0].DR = (byte)((idata[3]) & 0x0f);

                                    tone.aOp[0].SL = (byte)((idata[4] >> 4) & 0x0f);
                                    tone.aOp[0].RR = (byte)((idata[4]) & 0x0f);
                                    tone.aOp[0].SR = -1;

                                    tone.aOp[0].WS = (byte)((idata[5]) & 0x07);

                                    tone.aOp[1].AM = (byte)((idata[7] >> 7) & 0x1);
                                    tone.aOp[1].VIB = (byte)((idata[7] >> 6) & 0x1);
                                    tone.aOp[1].EG = (byte)((idata[7] >> 5) & 0x1);
                                    tone.aOp[1].KSR = (byte)((idata[7] >> 4) & 0x1);
                                    tone.aOp[1].ML = (byte)((idata[7]) & 0xf);

                                    tone.aOp[1].KS = (byte)((idata[8] >> 6) & 0x03);
                                    tone.aOp[1].TL = (byte)((idata[8]) & 0x3f);

                                    tone.aOp[1].AR = (byte)((idata[9] >> 4) & 0x0f);
                                    tone.aOp[1].DR = (byte)((idata[9]) & 0x0f);

                                    tone.aOp[1].SL = (byte)((idata[10] >> 4) & 0x0f);
                                    tone.aOp[1].RR = (byte)((idata[10]) & 0x0f);
                                    tone.aOp[1].SR = -1;

                                    tone.aOp[1].WS = (byte)((idata[11]) & 0x07);

                                    tone.CNT = (byte)((idata[6]) & 0x01);
                                    tone.FB = (byte)((idata[6] >> 1) & 0x07);

                                    tones.Add(tone);
                                }
                            }
                        }
                        break;
                    default:
                        return base.ImportToneFile(fileName);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;

                MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
            }
            return tones;

        }

        protected override void ApplyTone(Tone tone)
        {
            bool native = false;
            if (tone.CNT == -1)
            {
                ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            }
            else
            {
                ((RegisterValue)this["General"]["ALG"]).Value = tone.CNT;
                native = true;
            }

            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.VIB"]).NullableValue = null;

            for (int i = 0; i < 2; i++)
            {
                if (!native)
                {
                    ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR / 2;
                    ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tone.aOp[i].DR / 2;
                    ((RegisterValue)this["Operator " + (i + 1)]["SR"]).Value = tone.aOp[i].SR / 2;
                    ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL / 2;
                }
                else
                {
                    ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR;
                    ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tone.aOp[i].DR;
                    ((RegisterValue)this["Operator " + (i + 1)]["SR"]).Value = -1;
                    ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL;
                }
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["KSL"]).Value = tone.aOp[i].KS;
                ((RegisterValue)this["Operator " + (i + 1)]["KSR"]).Value = tone.aOp[i].KSR;
                ((RegisterValue)this["Operator " + (i + 1)]["MFM"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["VR"]).Value = tone.aOp[i].VIB;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tone.aOp[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tone.aOp[i].WS;
            }
            timbre.TimbreName = tone.Name;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            YM3812Timbre tim = (YM3812Timbre)timbre;

            bool native = false;
            if (tone.CNT == -1)
            {
                tim.ALG = (byte)tone.AL;
            }
            else
            {
                tim.ALG = (byte)tone.CNT;
                native = true;
            }

            tim.FB = (byte)tone.FB;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.AMD = null;
            tim.GlobalSettings.VIB = null;

            for (int i = 0; i < 2; i++)
            {
                if (!native)
                {
                    tim.Ops[i].AR = (byte)(tone.aOp[i].AR / 2);
                    tim.Ops[i].DR = (byte)(tone.aOp[i].DR / 2);
                    tim.Ops[i].SR = (byte)(tone.aOp[i].SR / 2);
                    tim.Ops[i].TL = (byte)(tone.aOp[i].TL / 2);
                }
                else
                {
                    tim.Ops[i].AR = (byte)(tone.aOp[i].AR);
                    tim.Ops[i].DR = (byte)(tone.aOp[i].DR);
                    tim.Ops[i].SR = null;
                    tim.Ops[i].TL = (byte)(tone.aOp[i].TL);
                }
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].KSL = (byte)tone.aOp[i].KS;
                tim.Ops[i].KSR = 0;
                tim.Ops[i].MFM = (byte)tone.aOp[i].ML;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].VR = 0;
                tim.Ops[i].EG = 0;
                tim.Ops[i].WS = 0;
            }
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM3812Timbre tim = (YM3812Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = tim.GlobalSettings.AMD;
            ((RegisterValue)this["General"]["GlobalSettings.VIB"]).NullableValue = tim.GlobalSettings.VIB;
            for (int i = 0; i < 2; i++)
            {
                this["Operator " + (i + 1)].Target = tim.Ops[i];
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tim.Ops[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tim.Ops[i].DR;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tim.Ops[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tim.Ops[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["SR"]).NullableValue = tim.Ops[i].SR;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tim.Ops[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["KSL"]).Value = tim.Ops[i].KSL;
                ((RegisterValue)this["Operator " + (i + 1)]["KSR"]).Value = tim.Ops[i].KSR;
                ((RegisterValue)this["Operator " + (i + 1)]["MFM"]).Value = tim.Ops[i].MFM;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tim.Ops[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["VR"]).Value = tim.Ops[i].VR;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tim.Ops[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tim.Ops[i].WS;
            }
        }
    }

}
