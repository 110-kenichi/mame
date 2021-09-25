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
        protected override string SupportedExtensionsFilter
        {
            get
            {
                return "Tone file(MUCOM88, FMP, PMD, VOPM, GWI, WOPL)|*.muc;*.dat;*.mwi;*.mml;*.fxb;*.gwi;*.wopl";
            }
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
                                    tone1.PitchShift = file.ReadSByte();
                                    tone2.PitchShift *= 2;

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
                    op.RR = (byte)((reg >> 0) & 0x0f);
                }
                //1
                reg = file.ReadByte();
                if (op != null)
                {
                    op.WS = (byte)((reg >> 0) & 0x07);
                }
            }
            //2
            ReadInt16Big(file);
            //2
            ReadInt16Big(file);
        }

        protected override void ApplyTone(Tone tone)
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
                    ((RegisterValue)this["Operator " + (i + 1)]["SR"]).Value = tone.aOp[i].SR;
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

            timbre.KeyShift = tone.KeyShift;
            timbre.PitchShift = tone.PitchShift;
            timbre.TimbreName = tone.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            YMF262Timbre tim = (YMF262Timbre)timbre;

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

            tim.ALG = (byte)alg;
            tim.FB = (byte)tone.FB;
            tim.FB2 = (byte)tone.FB2;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.DAM = null;
            tim.GlobalSettings.DVB = null;

            for (int i = 0; i < 3; i++)
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
                    tim.Ops[i].SR = (byte)(tone.aOp[i].SR);
                    tim.Ops[i].TL = (byte)(tone.aOp[i].TL);
                }
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].KSL = (byte)tone.aOp[i].KS;
                tim.Ops[i].KSR = (byte)tone.aOp[i].KSR;
                tim.Ops[i].MFM = (byte)tone.aOp[i].ML;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].VIB = (byte)tone.aOp[i].VIB;
                tim.Ops[i].EG = (byte)tone.aOp[i].EG;
                tim.Ops[i].WS = (byte)tone.aOp[i].WS;
            }

            tim.KeyShift = tone.KeyShift;
            tim.PitchShift = tone.PitchShift;
            timbre.TimbreName = tone.Name;
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
    }

}
