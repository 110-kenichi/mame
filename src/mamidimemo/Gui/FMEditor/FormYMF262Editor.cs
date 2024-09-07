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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYMF262Editor : FormFmEditor
    {
        private YMF262Timbre timbre;

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
        public FormYMF262Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYMF262Editor(YMF262 inst, YMF262Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YMF262EdSize;

            var gen = new YMF262GeneralContainer(inst, timbre, "General");
            AddControl(gen);

            AddControl(new YMF262OperatorContainer(timbre.Ops[0], null, "Operator 1"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[1], null, "Operator 2"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[2], gen, "Operator 3"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[3], gen, "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YMF262EdSize = Size;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string SupportedExtensionsCustomFilterLabel
        {
            get
            {
                return "Tone file(OPL)";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string SupportedExtensionsCustomFilterExt
        {
            get
            {
                return "*.opl";
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
                    case ".WOPL":
                        {
                            using (var file = new System.IO.BinaryReader(new System.IO.FileStream(fileName, System.IO.FileMode.Open)))
                            {
                                file.BaseStream.Seek(0x11, System.IO.SeekOrigin.Begin);
                                {
                                    var val = file.ReadByte();
                                    //if ((val & 1) != 0)
                                    //    tone.AMD = 1;
                                    //if ((val & 2) != 0)
                                    //    tone.VIB = 1;
                                }

                                file.BaseStream.Seek(0x57, System.IO.SeekOrigin.Begin);
                                for (int i = 0; i < 128; i++)
                                {
                                    Tone tone1 = new Tone();
                                    Tone tone2 = new Tone();
                                    tone1.Number = i * 2;
                                    tone2.Number = i * 2 + 1;

                                    //32
                                    var name = Encoding.ASCII.GetString(file.ReadBytes(32));
                                    var nidx = name.IndexOf('\0');
                                    if (nidx >= 0)
                                        name = name.Substring(0, nidx);
                                    if (name != null && name.Length != 0)
                                    {
                                        tone1.Name = name;
                                        tone2.Name = name + "(2nd)";
                                    }
                                    //2
                                    tone1.KeyShift = YMF262.ReadInt16Big(file);
                                    //2
                                    tone2.KeyShift = YMF262.ReadInt16Big(file);

                                    //1
                                    file.ReadByte();    //MIDI Velocity offset
                                    //1
                                    tone2.PitchShift = file.ReadSByte();

                                    //1
                                    //DrumTimbres[i].BaseNote = (NoteNames)file.ReadByte();
                                    file.ReadByte();
                                    //1
                                    var opmode = file.ReadByte();

                                    if (opmode == 0)        //2OP
                                    {
                                        setRegisters(file, tone1, null);
                                        tones.Add(tone1);
                                    }
                                    else if (opmode == 1)   //4OP
                                    {
                                        setRegisters(file, tone1, tone2);
                                        tone1.CNT = (tone1.CNT << 1 | tone2.CNT) + 2;
                                        tone1.FB2 = tone2.FB;
                                        tone1.aOp[2] = tone2.aOp[0];
                                        tone1.aOp[3] = tone2.aOp[1];
                                        tones.Add(tone1);
                                    }
                                    else if (opmode == 3)
                                    {
                                        setRegisters(file, tone1, tone2);
                                        tones.Add(tone1);
                                        tones.Add(tone2);
                                    }
                                    else if ((opmode & 4) == 4)
                                    {
                                        //empty
                                    }
                                    else
                                    {
                                        // System.Windows.Forms.MessageBox.Show("Unsupported op mode " + opmode);
                                    }
                                }

                                for (int i = 0; i < 128; i++)
                                {
                                    Tone tone1 = new Tone();
                                    Tone tone2 = new Tone();
                                    tone1.Number = i * 2 + 256;
                                    tone2.Number = i * 2 + 256 + 1;

                                    //32
                                    var name = Encoding.ASCII.GetString(file.ReadBytes(32));
                                    var nidx = name.IndexOf('\0');
                                    if (nidx >= 0)
                                        name = name.Substring(0, nidx);
                                    if (name != null && name.Length != 0)
                                    {
                                        tone1.Name = name;
                                        tone2.Name = name + "(2nd)";
                                    }
                                    else
                                    {
                                        tone1.Name = tone1.Number.ToString();
                                        tone2.Name = tone2.Number.ToString();
                                    }

                                    //2
                                    tone1.KeyShift = ReadInt16Big(file);
                                    //2
                                    tone2.KeyShift = ReadInt16Big(file);

                                    //1
                                    file.ReadByte();    //MIDI Velocity offset
                                    //1
                                    tone2.PitchShift = file.ReadSByte();

                                    //1
                                    //TODO: Drum
                                    //DrumTimbres[i].BaseNote = (NoteNames)file.ReadByte();
                                    file.ReadByte();
                                    //1
                                    var opmode = file.ReadByte();

                                    if (opmode == 0)        //2OP
                                    {
                                        setRegisters(file, tone1, null);
                                        tones.Add(tone1);
                                    }
                                    else if (opmode == 1)   //4OP
                                    {
                                        setRegisters(file, tone1, tone2);
                                        tone1.CNT = (tone1.CNT << 1 | tone2.CNT) + 2;
                                        tone1.aOp[2] = tone2.aOp[0];
                                        tone1.aOp[3] = tone2.aOp[1];
                                        tones.Add(tone1);
                                    }
                                    else if (opmode == 3)
                                    {
                                        setRegisters(file, tone1, tone2);
                                        tones.Add(tone1);
                                        tones.Add(tone2);
                                    }
                                    else if ((opmode & 4) == 4)
                                    {
                                        //empty
                                    }
                                    else
                                    {
                                        // System.Windows.Forms.MessageBox.Show("Unsupported op mode " + opmode);
                                    }
                                }

                            }


                        }
                        break;
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
                                    tone.aOp[0].SR = -1;
                                    tone.aOp[0].RR = (byte)((idata[4]) & 0x0f);

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
                                    tone.aOp[1].SR = -1;
                                    tone.aOp[1].RR = (byte)((idata[10]) & 0x0f);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tim1"></param>
        /// <param name="tim2"></param>
        private void setRegisters(BinaryReader file, Tone tim1, Tone tim2)
        {
            //1
            var reg = file.ReadByte();
            tim1.CNT = (byte)(reg & 1);
            tim1.FB = (byte)((reg >> 1) & 7);
            //1
            reg = file.ReadByte();
            if (tim2 != null)
            {
                tim2.CNT = (byte)(reg & 1);
                tim2.FB = (byte)((reg >> 1) & 7);
            }
            for (int opi = 0; opi < 4; opi++)
            {
                Op op = null;
                switch (opi)
                {
                    case 0:
                        op = tim1.aOp[1];
                        break;
                    case 1:
                        op = tim1.aOp[0];
                        break;
                    case 2:
                        if (tim2 != null)
                            op = tim2.aOp[1];
                        break;
                    case 3:
                        if (tim2 != null)
                            op = tim2.aOp[0];
                        break;
                }

                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.AM = (byte)((reg >> 7) & 0x01);
                    op.VIB = (byte)((reg >> 6) & 0x01);
                    op.EG = (byte)((reg >> 5) & 0x01);
                    op.KSR = (byte)((reg >> 4) & 0x01);
                    op.ML = (byte)((reg >> 0) & 0x0f);
                }
                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.KS = (byte)((reg >> 6) & 0x03);
                    op.TL = (byte)((reg >> 0) & 0x3f);
                }
                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.AR = (byte)((reg >> 4) & 0x0f);
                    op.DR = (byte)((reg >> 0) & 0x0f);
                }
                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.SL = (byte)((reg >> 4) & 0x0f);
                    op.SR = -1;
                    op.RR = (byte)((reg >> 0) & 0x0f);
                }
                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.WS = (byte)((reg >> 0) & 0x07);
                }
            }
            if (tim2 != null)
            {
                //2
                tim2.KeyOnDelay = ReadInt16Big(file);
                //2
                tim2.KeyOffDelay = ReadInt16Big(file);
            }
            else
            {
                //2
                ReadInt16Big(file);
                //2
                ReadInt16Big(file);
            }
        }

        protected override void ApplyTone(Tone tone)
        {
            if (tone.MML != null)
            {
                Timbre.TimbreName = tone.MML[0];
                MmlValueGeneral = tone.MML[1];
                MmlValueOps = new string[] { tone.MML[2], tone.MML[3], tone.MML[4], tone.MML[5] };
            }
            else
            {
                int alg = 0;
                bool native = false;
                if (tone.CNT == -1)
                {
                    switch (tone.AL)
                    {
                        case 0:
                            alg = 2;
                            break;
                        case 1:
                            alg = 4;    //?
                            break;
                        case 2:
                            alg = 4;    //?
                            break;
                        case 3:
                            alg = 3;    //?
                            break;
                        case 4:
                            alg = 3;
                            break;
                        case 5:
                            alg = 5;    //?
                            break;
                        case 6:
                            alg = 5;    //?
                            break;
                        case 7:
                            alg = 5;
                            break;
                    }
                }
                else
                {
                    alg = tone.CNT;
                    native = true;
                }
                ((RegisterValue)this["General"]["ALG"]).Value = alg;
                ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
                ((RegisterValue)this["General"]["FB2"]).Value = tone.FB;
                ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
                ((RegisterValue)this["General"]["GlobalSettings.DAM"]).NullableValue = null;
                ((RegisterValue)this["General"]["GlobalSettings.DVB"]).NullableValue = null;

                for (int i = 0; i < 4; i++)
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
                    ((RegisterValue)this["Operator " + (i + 1)]["VIB"]).Value = tone.aOp[i].VIB;
                    ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tone.aOp[i].EG;
                    ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tone.aOp[i].WS;
                }

                timbre.MDS.KeyShift = tone.KeyShift;
                timbre.MDS.PitchShift = tone.PitchShift;
                timbre.TimbreName = tone.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            if (tone.MML != null)
            {
                ApplyTimbre(timbre);
                ApplyTone(tone);
            }
            else
            {
                ((YMF262)Instrument).ImportToneFile(timbre, tone);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YMF262Timbre tim = (YMF262Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.DAM"]).NullableValue = tim.GlobalSettings.DAM;
            ((RegisterValue)this["General"]["GlobalSettings.DVB"]).NullableValue = tim.GlobalSettings.DVB;
            for (int i = 0; i < 4; i++)
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
                ((RegisterValue)this["Operator " + (i + 1)]["VIB"]).Value = tim.Ops[i].VIB;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = tim.Ops[i].EG;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = tim.Ops[i].WS;
            }
        }

        protected override string[] GetMMlValues()
        {
            return new string[] { Timbre.TimbreName, MmlValueGeneral, MmlValueOps[0], MmlValueOps[1], MmlValueOps[2], MmlValueOps[3] };

        }

    }

}
