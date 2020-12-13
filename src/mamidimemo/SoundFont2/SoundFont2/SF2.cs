using System.IO;
using System.Text;

namespace Kermalis.SoundFont2
{
    public sealed class SF2
    {
        uint size;
        public readonly InfoListChunk InfoChunk;
        public readonly SdtaListChunk SoundChunk;
        public readonly PdtaListChunk HydraChunk;

        // For creating
        public SF2()
        {
            InfoChunk = new InfoListChunk(this);
            SoundChunk = new SdtaListChunk(this);
            HydraChunk = new PdtaListChunk(this);
        }

        // For reading
        public SF2(string path)
        {
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.ASCII))
            {
                char[] chars = reader.ReadChars(4);
                if (new string(chars) != "RIFF")
                    throw new InvalidDataException("RIFF header was not found at the start of the file.");

                size = reader.ReadUInt32();
                chars = reader.ReadChars(4);
                if (new string(chars) != "sfbk")
                    throw new InvalidDataException("sfbk header was not found at the expected offset.");

                InfoChunk = new InfoListChunk(this, reader);
                SoundChunk = new SdtaListChunk(this, reader);
                HydraChunk = new PdtaListChunk(this, reader);
            }
        }

        public void Save(string path)
        {
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create), Encoding.ASCII))
            {
                AddTerminals();

                writer.Write("RIFF".ToCharArray());
                writer.Write(size);
                writer.Write("sfbk".ToCharArray());

                InfoChunk.Write(writer);
                SoundChunk.Write(writer);
                HydraChunk.Write(writer);
            }
        }


        // Returns sample index
        public uint AddSample(short[] pcm16, string name, bool bLoop, uint loopPos, uint sampleRate, byte originalKey, sbyte pitchCorrection)
        {
            uint start = SoundChunk.SMPLSubChunk.AddSample(pcm16, bLoop, loopPos);
            // If the sample is looped the standard requires us to add the 8 bytes from the start of the loop to the end
            uint end, loopEnd, loopStart;

            uint len = (uint)pcm16.Length;
            if (bLoop)
            {
                end = start + len + 8;
                loopStart = start + loopPos; loopEnd = start + len;
            }
            else
            {
                end = start + len;
                loopStart = 0; loopEnd = 0;
            }

            return AddSampleHeader(name, start, end, loopStart, loopEnd, sampleRate, originalKey, pitchCorrection);
        }
        // Returns instrument index
        public uint AddInstrument(string name)
        {
            return HydraChunk.INSTSubChunk.AddInstrument(new SF2Instrument(this)
            {
                InstrumentName = name,
                InstrumentBagIndex = (ushort)HydraChunk.IBAGSubChunk.Count
            });
        }
        public void AddInstrumentBag()
        {
            HydraChunk.IBAGSubChunk.AddBag(new SF2Bag(this, false));
        }
        public void AddInstrumentModulator()
        {
            HydraChunk.IMODSubChunk.AddModulator(new SF2ModulatorList(this));
        }
        public void AddInstrumentGenerator()
        {
            HydraChunk.IGENSubChunk.AddGenerator(new SF2GeneratorList(this));
        }
        public void AddInstrumentGenerator(SF2Generator generator, SF2GeneratorAmount amount)
        {
            HydraChunk.IGENSubChunk.AddGenerator(new SF2GeneratorList(this)
            {
                Generator = generator,
                GeneratorAmount = amount
            });
        }
        public void AddPreset(string name, ushort preset, ushort bank)
        {
            HydraChunk.PHDRSubChunk.AddPreset(new SF2PresetHeader(this)
            {
                PresetName = name,
                Preset = preset,
                Bank = bank,
                PresetBagIndex = (ushort)HydraChunk.PBAGSubChunk.Count
            });
        }
        public void AddPresetBag()
        {
            HydraChunk.PBAGSubChunk.AddBag(new SF2Bag(this, true));
        }
        public void AddPresetModulator()
        {
            HydraChunk.PMODSubChunk.AddModulator(new SF2ModulatorList(this));
        }
        public void AddPresetGenerator()
        {
            HydraChunk.PGENSubChunk.AddGenerator(new SF2GeneratorList(this));
        }
        public void AddPresetGenerator(SF2Generator generator, SF2GeneratorAmount amount)
        {
            HydraChunk.PGENSubChunk.AddGenerator(new SF2GeneratorList(this)
            {
                Generator = generator,
                GeneratorAmount = amount
            });
        }

        uint AddSampleHeader(string name, uint start, uint end, uint loopStart, uint loopEnd, uint sampleRate, byte originalKey, sbyte pitchCorrection)
        {
            return HydraChunk.SHDRSubChunk.AddSample(new SF2SampleHeader(this)
            {
                SampleName = name,
                Start = start,
                End = end,
                LoopStart = loopStart,
                LoopEnd = loopEnd,
                SampleRate = sampleRate,
                OriginalKey = originalKey,
                PitchCorrection = pitchCorrection
            });
        }
        void AddTerminals()
        {
            AddSampleHeader("EOS", 0, 0, 0, 0, 0, 0, 0);
            AddInstrument("EOI");
            AddInstrumentBag();
            AddInstrumentGenerator();
            AddInstrumentModulator();
            AddPreset("EOP", 0xFF, 0xFF);
            AddPresetBag();
            AddPresetGenerator();
            AddPresetModulator();
        }

        internal void UpdateSize()
        {
            if (InfoChunk == null || SoundChunk == null || HydraChunk == null)
                return;
            size = 4
                + InfoChunk.UpdateSize() + 8
                + SoundChunk.UpdateSize() + 8
                + HydraChunk.UpdateSize() + 8;
        }
    }
}
