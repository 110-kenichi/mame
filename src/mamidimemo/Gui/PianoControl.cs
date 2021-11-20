// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using System.Drawing.Drawing2D;
using zanac.MAmidiMEmo.Midi;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class PianoControl : UserControl
    {
        private Brush blackBrush = new SolidBrush(Color.Black);

        private Brush whiteBrush = new SolidBrush(Color.White);

        //private Brush blueBrush = new SolidBrush(Color.LightBlue);

        private Brush grayBrush = new SolidBrush(Color.LightGray);

        private Pen blackPen = new Pen(Color.Black);

        private int wKeyW;

        private int wKeyH;

        private int bKeyH;

        private int bKeyW;

        private int bKeyWOfst;

        private Rectangle cr;

        private Dictionary<SoundBase, SoundUpdatedEventArgs> soundKeyOn = new Dictionary<SoundBase, SoundUpdatedEventArgs>();

        private Dictionary<int, bool> receiveChs = new Dictionary<int, bool>();

        private int entryDataValue;


        /// <summary>
        /// 
        /// </summary>
        public int EntryDataValue
        {
            get
            {
                return InternalEntryDataValue / SystemInformation.MouseWheelScrollDelta;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int InternalEntryDataValue
        {
            get
            {
                return entryDataValue;
            }
            set
            {
                if (entryDataValue != value)
                {
                    entryDataValue = value;
                    if (entryDataValue / SystemInformation.MouseWheelScrollDelta < 0)
                        entryDataValue = 0;
                    else if (entryDataValue / SystemInformation.MouseWheelScrollDelta > 127)
                        entryDataValue = 127 * SystemInformation.MouseWheelScrollDelta;

                    EntryDataChanged?.Invoke(this, EventArgs.Empty);

                    Invalidate(new Rectangle(0, 0, wKeyW, Height));
                }
            }
        }

        public TimbreBase[] TargetTimbres
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public PianoControl()
        {
            InitializeComponent();

            for (int i = 0; i < 16; i++)
                receiveChs[i] = true;

            SetStyle(ControlStyles.ResizeRedraw, true);

            SoundBase.SoundKeyOn += SoundBase_SoundKeyOn;
            SoundBase.SoundKeyOff += SoundBase_SoundKeyOff;
            SoundBase.SoundPitchUpdated += SoundBase_SoundPitchUpdated;
            //SoundBase.SoundSoundOff += SoundBase_SoundSoundOff;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="receive"></param>
        public void SetReceiveChannel(int ch, bool receive)
        {
            receiveChs[ch] = receive;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            keyPathBlackTable.Clear();
            keyPathTable.Clear();

            cr = ClientRectangle;
            wKeyW = cr.Width / 76;
            if (wKeyW < 1)
                wKeyW = 2;
            wKeyH = cr.Height;
            bKeyH = (wKeyH * 95) / 150;
            bKeyW = (wKeyW * 11) / 23;
            if (bKeyW % 2 == 1)
                bKeyW++;
            bKeyWOfst = bKeyW / 5;

            base.OnClientSizeChanged(e);
        }

        private void SoundBase_SoundKeyOn(object sender, SoundUpdatedEventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (IsDisposed)
                    return;

                bool fill;
                SoundBase snd = (SoundBase)sender;

                if (TargetTimbres != null)
                {
                    bool found = false;
                    foreach (var tt in TargetTimbres)
                    {
                        if (snd.Timbre == tt)
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                        return;
                }

                if (!receiveChs[snd.NoteOnEvent.Channel])
                    return;

                if (!soundKeyOn.ContainsKey(snd))
                    soundKeyOn.Add(snd, e);
                else
                {
                    Invalidate(new Region(getKeyPath(soundKeyOn[snd].NoteNumber, out fill)));
                    soundKeyOn[snd] = e;
                }
                Invalidate(new Region(getKeyPath(soundKeyOn[snd].NoteNumber, out fill)));
            }));
        }

        private void SoundBase_SoundKeyOff(object sender, SoundUpdatedEventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (IsDisposed)
                    return;

                bool fill;
                SoundBase snd = (SoundBase)sender;
                if (soundKeyOn.ContainsKey(snd))
                {
                    Invalidate(new Region(getKeyPath(soundKeyOn[snd].NoteNumber, out fill)));
                    soundKeyOn.Remove(snd);
                }
            }));
        }

        private void SoundBase_SoundPitchUpdated(object sender, SoundUpdatedEventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            SoundBase snd = (SoundBase)sender;
            if (snd.IsKeyOff)
                return;

            this.BeginInvoke(new MethodInvoker(() =>
            {
                if (IsDisposed)
                    return;

                bool fill;

                if (TargetTimbres != null)
                {
                    bool found = false;
                    foreach (var tt in TargetTimbres)
                    {
                        if (snd.Timbre == tt)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return;
                }

                if (!receiveChs[snd.NoteOnEvent.Channel])
                    return;

                int nn = (int)Math.Round(e.NoteNumber + e.Pitch);
                if (!soundKeyOn.ContainsKey(snd))
                    soundKeyOn.Add(snd, new SoundUpdatedEventArgs(nn, e.Velocity, 0));
                else
                {
                    Invalidate(new Region(getKeyPath(soundKeyOn[snd].NoteNumber, out fill)));
                    soundKeyOn[snd] = new SoundUpdatedEventArgs(nn, e.Velocity, 0);
                }
                Invalidate(new Region(getKeyPath(soundKeyOn[snd].NoteNumber, out fill)));
            }));
        }

        private Dictionary<int, int> onKeys = new Dictionary<int, int>();

        protected override void OnPaint(PaintEventArgs e)
        {
            // Call the OnPaint method of the base class.  
            base.OnPaint(e);

            var g = e.Graphics;
            var rect = g.ClipBounds;

            onKeys.Clear();
            foreach (var snd in soundKeyOn.Values)
                onKeys[snd.NoteNumber] = snd.Velocity;

            g.FillRectangle(SystemBrushes.Control, 0, 0, wKeyW, Height);
            g.FillPolygon(SystemBrushes.ControlDarkDark, new Point[]{
            new Point(0, 0),
            new Point(wKeyW, Height),
            new Point(wKeyW, 0) });

            //g.FillRectangle(SystemBrushes.ControlDarkDark, 0, 0, wKeyW, Height);
            //g.FillRectangle(SystemBrushes.ControlDark, 0, 5, wKeyW, Height - 10);
            //g.FillRectangle(SystemBrushes.Control, 0, 15, wKeyW, Height - 30);
            int h = (Height - 1) / 127;
            if (h == 0)
                h = 1;
            int wv = entryDataValue / SystemInformation.MouseWheelScrollDelta;
            g.FillRectangle(SystemBrushes.ControlDark, 0, Height - 1 - (wv * (Height - 1) / 127) - (h * 5), wKeyW, h + (h * 5 * 2));
            g.FillRectangle(SystemBrushes.WindowText, 0, Height - 1 - (wv * (Height - 1) / 127), wKeyW, h);

            for (int keyNum = 0; keyNum < 128; keyNum++)
            {
                bool black;
                GraphicsPath path = getKeyPath(keyNum, out black);
                if (rect.IntersectsWith(path.GetBounds()))
                {
                    {
                        if (keyNum == 60)
                        {
                            if (onKeys.ContainsKey(keyNum))
                            {
                                var velo = onKeys[keyNum] * 2;
                                using (Brush blueBrush = new SolidBrush(Color.FromArgb(0, velo, 255)))
                                    g.FillRegion(blueBrush, new Region(path));
                            }
                            else
                            {
                                var r = new Region(path);
                                g.FillRegion(whiteBrush, r);
                                var b = r.GetBounds(g);
                                g.FillPie(grayBrush, new Rectangle((int)b.X, (int)b.Bottom - (int)b.Width, (int)b.Width, (int)b.Width), 0, 360);
                            }
                            g.DrawPath(blackPen, path);
                        }
                        else if (!black)
                        {
                            if (onKeys.ContainsKey(keyNum))
                            {
                                var velo = onKeys[keyNum] * 2;
                                using (Brush blueBrush = new SolidBrush(Color.FromArgb(0, velo, 255)))
                                    g.FillRegion(blueBrush, new Region(path));
                            }
                            else
                                g.FillRegion(whiteBrush, new Region(path));
                            g.DrawPath(blackPen, path);
                        }
                        else
                        {
                            if (onKeys.ContainsKey(keyNum))
                            {
                                var velo = onKeys[keyNum] * 2;
                                using (Brush blueBrush = new SolidBrush(Color.FromArgb(0, velo, 255)))
                                    g.FillRegion(blueBrush, new Region(path));
                            }
                            else
                                g.FillRegion(blackBrush, new Region(path));
                            g.DrawPath(blackPen, path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs> EntryDataChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            InternalEntryDataValue += e.Delta;

            base.OnMouseWheel(e);
        }

        private Dictionary<int, GraphicsPath> keyPathTable = new Dictionary<int, GraphicsPath>();
        private Dictionary<int, bool> keyPathBlackTable = new Dictionary<int, bool>();

        private GraphicsPath getKeyPath(int keyNum, out bool black)
        {
            if (keyPathTable.ContainsKey(keyNum))
            {
                black = keyPathBlackTable[keyNum];
                return keyPathTable[keyNum];
            }

            GraphicsPath r = new GraphicsPath();

            int octave = keyNum / 12;
            black = false;
            List<Point> pts = new List<Point>();
            switch (keyNum % 12)
            {
                case 0: //C
                    {
                        var x = wKeyW + (octave * 7 + 0) * wKeyW;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) - bKeyWOfst, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) - bKeyWOfst, 0));
                    }
                    break;
                case 1: //C#
                    {
                        var x = wKeyW + (octave * 7 + 1) * wKeyW - (bKeyW / 2) - bKeyWOfst;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x + bKeyW, bKeyH));
                        pts.Add(new Point(x + bKeyW, 0));

                        black = true;
                    }
                    break;
                case 2: //D
                    {
                        var x = wKeyW + (octave * 7 + 1) * wKeyW;

                        pts.Add(new Point(x + (bKeyW / 2) - bKeyWOfst, 0));
                        pts.Add(new Point(x + (bKeyW / 2) - bKeyWOfst, bKeyH));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) + bKeyWOfst, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) + bKeyWOfst, 0));
                    }
                    break;
                case 3: //D#
                    {
                        var x = wKeyW + (octave * 7 + 2) * wKeyW - (bKeyW / 2) + bKeyWOfst;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x + bKeyW, bKeyH));
                        pts.Add(new Point(x + bKeyW, 0));

                        black = true;
                    }
                    break;
                case 4: //E
                    {
                        var x = wKeyW + (octave * 7 + 2) * wKeyW;

                        pts.Add(new Point(x + (bKeyW / 2) + bKeyWOfst, 0));
                        pts.Add(new Point(x + (bKeyW / 2) + bKeyWOfst, bKeyH));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, 0));
                    }
                    break;
                case 5: //F
                    {
                        var x = wKeyW + (octave * 7 + 3) * wKeyW;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) - bKeyWOfst, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) - bKeyWOfst, 0));
                    }
                    break;
                case 6: //F#
                    {
                        var x = wKeyW + (octave * 7 + 4) * wKeyW - (bKeyW / 2) - bKeyWOfst;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x + bKeyW, bKeyH));
                        pts.Add(new Point(x + bKeyW, 0));

                        black = true;
                    }
                    break;
                case 7: //G
                    {
                        var x = wKeyW + (octave * 7 + 4) * wKeyW;
                        pts.Add(new Point(x + (bKeyW / 2) - bKeyWOfst, 0));
                        pts.Add(new Point(x + (bKeyW / 2) - bKeyWOfst, bKeyH));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        if (keyNum == 127)
                        {
                            pts.Add(new Point(x + wKeyW, 0));
                        }
                        else
                        {
                            pts.Add(new Point(x + wKeyW, bKeyH));
                            pts.Add(new Point(x + wKeyW - (bKeyW / 2), bKeyH));
                            pts.Add(new Point(x + wKeyW - (bKeyW / 2), 0));
                        }
                    }
                    break;
                case 8: //G#
                    {
                        var x = wKeyW + (octave * 7 + 5) * wKeyW - (bKeyW / 2);

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x + bKeyW, bKeyH));
                        pts.Add(new Point(x + bKeyW, 0));

                        black = true;
                    }
                    break;
                case 9: //A
                    {
                        var x = wKeyW + (octave * 7 + 5) * wKeyW;

                        pts.Add(new Point(x + (bKeyW / 2), 0));
                        pts.Add(new Point(x + (bKeyW / 2), bKeyH));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) + bKeyWOfst, bKeyH));
                        pts.Add(new Point(x + wKeyW - (bKeyW / 2) + bKeyWOfst, 0));
                    }
                    break;
                case 10: //A#
                    {
                        var x = wKeyW + (octave * 7 + 6) * wKeyW - (bKeyW / 2) + bKeyWOfst;

                        pts.Add(new Point(x, 0));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x + bKeyW, bKeyH));
                        pts.Add(new Point(x + bKeyW, 0));

                        black = true;
                    }
                    break;
                case 11: //B
                    {
                        var x = wKeyW + (octave * 7 + 6) * wKeyW;

                        pts.Add(new Point(x + (bKeyW / 2) + bKeyWOfst, 0));
                        pts.Add(new Point(x + (bKeyW / 2) + bKeyWOfst, bKeyH));
                        pts.Add(new Point(x, bKeyH));
                        pts.Add(new Point(x, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, wKeyH - 1));
                        pts.Add(new Point(x + wKeyW, 0));
                    }
                    break;
            }
            if (pts.Count != 0)
                r.AddPolygon(pts.ToArray());

            keyPathBlackTable[keyNum] = black;
            keyPathTable[keyNum] = r;

            return r;
        }

        private int lastKeyOn = -1;

        private bool dataEntryPressed;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (0 <= e.X && e.X < wKeyW && 0 <= e.Y && e.Y < Height)
            {
                InternalEntryDataValue = (SystemInformation.MouseWheelScrollDelta * 127 * (Height - e.Y)) / Height;
                Invalidate(new Rectangle(0, 0, wKeyW, Height));
                dataEntryPressed = true;
            }
            else
            {
                for (int keyNum = 0; keyNum < 128; keyNum++)
                {
                    bool black;
                    GraphicsPath path = getKeyPath(keyNum, out black);
                    var r = new Region(path);
                    if (r.IsVisible(e.Location))
                    {
                        lastKeyOff();

                        lastKeyOn = keyNum;
                        var rect = path.GetBounds();
                        var velo = (127 * (e.Y - (Height / 6))) / (Height / 3);
                        if (velo < 1)
                            velo = 1;
                        if (velo > 127)
                            velo = 127;
                        var noe = new TaggedNoteOnEvent((SevenBitNumber)keyNum, (SevenBitNumber)velo);
                        NoteOn?.Invoke(this, noe);
                    }
                }
            }
        }

        public event EventHandler<TaggedNoteOnEvent> NoteOn;

        public event EventHandler<NoteOffEvent> NoteOff;

        private void lastKeyOff()
        {
            if (lastKeyOn >= 0)
            {
                NoteOffEvent noe = new NoteOffEvent((SevenBitNumber)lastKeyOn, (SevenBitNumber)127);
                NoteOff?.Invoke(this, noe);
                lastKeyOn = -1;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            lastKeyOff();
            dataEntryPressed = false;

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dataEntryPressed)
            {
                InternalEntryDataValue = (SystemInformation.MouseWheelScrollDelta * 127 * (Height - e.Y)) / Height;
                Invalidate(new Rectangle(0, 0, wKeyW, Height));
            }
            else
            {
                for (int keyNum = 0; keyNum < 128; keyNum++)
                {
                    bool black;
                    GraphicsPath path = getKeyPath(keyNum, out black);
                    var r = new Region(path);
                    if (r.IsVisible(e.Location))
                    {
                        if (lastKeyOn >= 0)
                        {
                            if (lastKeyOn != keyNum)
                            {
                                lastKeyOff();

                                lastKeyOn = keyNum;
                                var rect = path.GetBounds();
                                var velo = (127 * (e.Y - (Height / 6))) / (Height / 3);
                                if (velo < 1)
                                    velo = 1;
                                if (velo > 127)
                                    velo = 127;
                                var noe = new TaggedNoteOnEvent((SevenBitNumber)keyNum, (SevenBitNumber)velo);
                                NoteOn?.Invoke(this, noe);
                            }
                        }
                    }
                }
            }
        }
    }
}
