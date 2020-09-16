using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//https://vstcpp.wpblog.jp/?page_id=2076

namespace Kermalis.SoundFont2
{
    public class SF2Chunk
    {
        protected readonly SF2 sf2;

        readonly char[] chunkName; // Length 4
        public string ChunkName => new string(chunkName);
        public uint Size { get; protected set; } // Size in bytes

        protected SF2Chunk(SF2 inSf2, string name)
        {
            sf2 = inSf2;
            chunkName = name.ToCharArray();
        }
        protected SF2Chunk(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            chunkName = reader.ReadChars(4);
            Size = reader.ReadUInt32();
        }
        internal virtual void Write(BinaryWriter writer)
        {
            writer.Write(chunkName);
            writer.Write(Size);
        }
    }

    public abstract class SF2ListChunk : SF2Chunk
    {
        readonly char[] listChunkName; // Length 4
        public string ListChunkName => new string(listChunkName);

        protected SF2ListChunk(SF2 inSf2, string name) : base(inSf2, "LIST")
        {
            listChunkName = name.ToCharArray();
            Size = 4;
        }
        protected SF2ListChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            listChunkName = reader.ReadChars(4);
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(listChunkName);
        }

        internal abstract uint UpdateSize();
    }

    public sealed class SF2PresetHeader
    {
        public const uint Size = 38;
        readonly SF2 sf2;

        char[] presetName; // Length 20
        public string PresetName
        {
            get => new string(presetName).TrimEnd('\0');
            set => SF2Utils.TruncateOrNot(value, 20, ref presetName);
        }
        public ushort Preset, Bank, PresetBagIndex;
        // Reserved for future implementations
        readonly uint library = 0, genre = 0, morphology = 0;

        internal SF2PresetHeader(SF2 inSf2)
        {
            sf2 = inSf2;
            presetName = new char[20];
        }
        internal SF2PresetHeader(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            presetName = reader.ReadChars(20);
            Preset = reader.ReadUInt16();
            Bank = reader.ReadUInt16();
            PresetBagIndex = reader.ReadUInt16();
            library = reader.ReadUInt32();
            genre = reader.ReadUInt32();
            morphology = reader.ReadUInt32();
        }
        internal void Write(BinaryWriter writer)
        {
            writer.Write(presetName);
            writer.Write(Preset);
            writer.Write(Bank);
            writer.Write(PresetBagIndex);
            writer.Write(library);
            writer.Write(genre);
            writer.Write(morphology);
        }

        public override string ToString() => $"Preset Header - Bank = {Bank}, " +
            $"\nPreset = {Preset}, " +
            $"\nName = \"{PresetName}\"";
    }

    // Covers sfPresetBag and sfInstBag
    public sealed class SF2Bag
    {
        public const uint Size = 4;
        readonly SF2 sf2;

        public ushort GeneratorIndex; // Index in list of generators
        public ushort ModulatorIndex; // Index in list of modulators

        internal SF2Bag(SF2 inSf2, bool preset)
        {
            sf2 = inSf2;
            if (preset)
            {
                GeneratorIndex = (ushort)sf2.HydraChunk.PGENSubChunk.Count;
                ModulatorIndex = (ushort)sf2.HydraChunk.PMODSubChunk.Count;
            }
            else
            {
                GeneratorIndex = (ushort)sf2.HydraChunk.IGENSubChunk.Count;
                ModulatorIndex = (ushort)sf2.HydraChunk.IMODSubChunk.Count;
            }
        }
        internal SF2Bag(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            GeneratorIndex = reader.ReadUInt16();
            ModulatorIndex = reader.ReadUInt16();
        }
        internal void Write(BinaryWriter writer)
        {
            writer.Write(GeneratorIndex);
            writer.Write(ModulatorIndex);
        }

        public override string ToString() => $"Bag - Generator index = {GeneratorIndex}, " +
            $"\nModulator index = {ModulatorIndex}";
    }

    // Covers sfModList and sfInstModList
    public sealed class SF2ModulatorList
    {
        public const uint Size = 10;
        readonly SF2 sf2;

        public SF2Modulator ModulatorSource;
        public SF2Generator ModulatorDestination;
        public short ModulatorAmount;
        public SF2Modulator ModulatorAmountSource;
        public SF2Transform ModulatorTransform;

        internal SF2ModulatorList(SF2 inSf2)
        {
            sf2 = inSf2;
        }
        internal SF2ModulatorList(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            ModulatorSource = (SF2Modulator)reader.ReadUInt16();
            ModulatorDestination = (SF2Generator)reader.ReadUInt16();
            ModulatorAmount = reader.ReadInt16();
            ModulatorAmountSource = (SF2Modulator)reader.ReadUInt16();
            ModulatorTransform = (SF2Transform)reader.ReadUInt16();
        }
        internal void Write(BinaryWriter writer)
        {
            writer.Write((ushort)ModulatorSource);
            writer.Write((ushort)ModulatorDestination);
            writer.Write(ModulatorAmount);
            writer.Write((ushort)ModulatorAmountSource);
            writer.Write((ushort)ModulatorTransform);
        }

        public override string ToString() => $"Modulator List - Modulator source = {ModulatorSource}, " +
            $"\nModulator destination = {ModulatorDestination}, " +
            $"\nModulator amount = {ModulatorAmount}, " +
            $"\nModulator amount source = {ModulatorAmountSource}, " +
            $"\nModulator transform = {ModulatorTransform}";
    }

    public sealed class SF2GeneratorList
    {
        public const uint Size = 4;
        readonly SF2 sf2;

        public SF2Generator Generator;
        public SF2GeneratorAmount GeneratorAmount;

        internal SF2GeneratorList(SF2 inSf2)
        {
            sf2 = inSf2;
        }
        internal SF2GeneratorList(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            Generator = (SF2Generator)reader.ReadUInt16();
            GeneratorAmount = new SF2GeneratorAmount { UAmount = reader.ReadUInt16() };
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)Generator);
            writer.Write(GeneratorAmount.UAmount);
        }

        public override string ToString() => $"Generator List - Generator = {Generator}, " +
            $"\nGenerator amount = \"{GeneratorAmount}\"";
    }

    public sealed class SF2Instrument
    {
        public const uint Size = 22;
        readonly SF2 sf2;

        char[] instrumentName; // Length 20
        public string InstrumentName
        {
            get => new string(instrumentName).TrimEnd('\0');
            set => SF2Utils.TruncateOrNot(value, 20, ref instrumentName);
        }
        public ushort InstrumentBagIndex;

        internal SF2Instrument(SF2 inSf2)
        {
            sf2 = inSf2;
            instrumentName = new char[20];
        }
        internal SF2Instrument(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            instrumentName = reader.ReadChars(20);
            InstrumentBagIndex = reader.ReadUInt16();
        }
        internal void Write(BinaryWriter writer)
        {
            writer.Write(instrumentName);
            writer.Write(InstrumentBagIndex);
        }

        public override string ToString() => $"Instrument - Name = \"{InstrumentName}\"";
    }

    public sealed class SF2SampleHeader
    {
        public const uint Size = 46;
        readonly SF2 sf2;

        char[] sampleName; // Length 20
        public string SampleName
        {
            get => new string(sampleName).TrimEnd('\0');
            set => SF2Utils.TruncateOrNot(value, 20, ref sampleName);
        }
        public uint Start;
        public uint End;
        public uint LoopStart;
        public uint LoopEnd;
        public uint SampleRate;
        public byte OriginalKey;
        public sbyte PitchCorrection;
        public ushort SampleLink;
        public SF2SampleLink SampleType = SF2SampleLink.MonoSample;

        internal SF2SampleHeader(SF2 inSf2)
        {
            sf2 = inSf2;
            sampleName = new char[20];
        }
        internal SF2SampleHeader(SF2 inSf2, BinaryReader reader)
        {
            sf2 = inSf2;
            sampleName = reader.ReadChars(20);
            Start = reader.ReadUInt32();
            End = reader.ReadUInt32();
            LoopStart = reader.ReadUInt32();
            LoopEnd = reader.ReadUInt32();
            SampleRate = reader.ReadUInt32();
            OriginalKey = reader.ReadByte();
            PitchCorrection = reader.ReadSByte();
            SampleLink = reader.ReadUInt16();
            SampleType = (SF2SampleLink)reader.ReadUInt16();
        }
        internal void Write(BinaryWriter writer)
        {
            writer.Write(sampleName);
            writer.Write(Start);
            writer.Write(End);
            writer.Write(LoopStart);
            writer.Write(LoopEnd);
            writer.Write(SampleRate);
            writer.Write(OriginalKey);
            writer.Write(PitchCorrection);
            writer.Write(SampleLink);
            writer.Write((ushort)SampleType);
        }

        public override string ToString() => $"Sample - Name = \"{SampleName}\", " +
            $"\nType = {SampleType}";
    }

    #region Sub-Chunks

    public sealed class VersionSubChunk : SF2Chunk
    {
        public SF2VersionTag Version;

        internal VersionSubChunk(SF2 inSf2, string subChunkName) : base(inSf2, subChunkName)
        {
            Size = SF2VersionTag.Size;
            sf2.UpdateSize();
        }
        internal VersionSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            Version = new SF2VersionTag(reader.ReadUInt16(), reader.ReadUInt16());
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Version.Major);
            writer.Write(Version.Minor);
        }

        public override string ToString() => $"Version Chunk - Revision = {Version}";
    }

    public sealed class HeaderSubChunk : SF2Chunk
    {
        readonly int maxSize;

        char[] field;
        public string Field
        {
            get => new string(field).TrimEnd('\0');
            set
            {
                var strAsList = value.ToCharArray().ToList();
                if (strAsList.Count >= maxSize) // Input too long; cut it down
                {
                    strAsList = strAsList.Take(maxSize).ToList();
                    strAsList[maxSize - 1] = '\0';
                }
                else if (strAsList.Count % 2 == 0) // Even amount of characters
                {
                    strAsList.Add('\0'); // Add two null-terminators to keep the byte count even
                    strAsList.Add('\0');
                }
                else // Odd amount of characters
                {
                    strAsList.Add('\0'); // Add one null-terminator since that would make byte the count even
                }
                field = strAsList.ToArray();
                Size = (uint)field.Length;
                sf2.UpdateSize();
            }
        }

        internal HeaderSubChunk(SF2 inSf2, string subChunkName, int maxSize = 0x100) : base(inSf2, subChunkName)
        {
            this.maxSize = maxSize;
        }
        internal HeaderSubChunk(SF2 inSf2, BinaryReader reader, int maxSize = 0x100) : base(inSf2, reader)
        {
            this.maxSize = maxSize;
            field = reader.ReadChars((int)Size);
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(field);
        }

        public override string ToString() => $"Header Chunk - Name = \"{ChunkName}\", " +
            $"\nField Max Size = {maxSize}, " +
            $"\nField = \"{Field}\"";
    }

    public sealed class SMPLSubChunk : SF2Chunk
    {
        List<short> samples = new List<short>(); // Block of sample data

        /// <summary>
        /// 
        /// </summary>
        public short[] Samples
        {
            get
            {
                return samples.ToArray();
            }
        }

        internal SMPLSubChunk(SF2 inSf2) : base(inSf2, "smpl") { }
        internal SMPLSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / sizeof(short); i++)
                samples.Add(reader.ReadInt16());
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            foreach (short s in samples)
                writer.Write(s);
        }

        // Returns index of the start of the sample
        internal uint AddSample(short[] pcm16, bool bLoop, uint loopPos)
        {
            uint start = (uint)samples.Count;

            // Write wave
            samples.AddRange(pcm16);

            // If looping is enabled, write 8 samples from the loop point
            if (bLoop)
            {
                // In case (loopPos + i) is greater than the sample length
                uint max = (uint)pcm16.Length - loopPos;
                for (uint i = 0; i < 8; i++)
                    samples.Add(pcm16[loopPos + (i % max)]);
            }

            // Write 46 empty samples
            samples.AddRange(new short[46]);

            Size = (uint)samples.Count * sizeof(short);
            sf2.UpdateSize();
            return start;
        }

        public override string ToString() => $"Sample Data Chunk";
    }

    public sealed class PHDRSubChunk : SF2Chunk
    {
        readonly List<SF2PresetHeader> presets = new List<SF2PresetHeader>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2PresetHeader> Presets
        {
            get
            {
                return presets;
            }
        }

        public uint Count => (uint)presets.Count;

        internal PHDRSubChunk(SF2 inSf2) : base(inSf2, "phdr") { }
        internal PHDRSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2PresetHeader.Size; i++)
                presets.Add(new SF2PresetHeader(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                presets[i].Write(writer);
        }

        internal void AddPreset(SF2PresetHeader preset)
        {
            presets.Add(preset);
            Size = Count * SF2PresetHeader.Size;
            sf2.UpdateSize();
        }

        public override string ToString() => $"Preset Header Chunk - Preset count = {Count}";
    }

    public sealed class INSTSubChunk : SF2Chunk
    {
        readonly List<SF2Instrument> instruments = new List<SF2Instrument>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2Instrument> Instruments
        {
            get
            {
                return instruments;
            }
        }

        public uint Count => (uint)instruments.Count;

        internal INSTSubChunk(SF2 inSf2) : base(inSf2, "inst") { }
        internal INSTSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2Instrument.Size; i++)
                instruments.Add(new SF2Instrument(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                instruments[i].Write(writer);
        }

        internal uint AddInstrument(SF2Instrument instrument)
        {
            instruments.Add(instrument);
            Size = Count * SF2Instrument.Size;
            sf2.UpdateSize();
            return Count - 1;
        }

        public override string ToString() => $"Instrument Chunk - Instrument count = {Count}";
    }

    public sealed class BAGSubChunk : SF2Chunk
    {
        readonly List<SF2Bag> bags = new List<SF2Bag>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2Bag> Bags
        {
            get
            {
                return bags;
            }
        }

        public uint Count => (uint)bags.Count;

        internal BAGSubChunk(SF2 inSf2, bool preset) : base(inSf2, preset ? "pbag" : "ibag") { }
        internal BAGSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2Bag.Size; i++)
                bags.Add(new SF2Bag(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                bags[i].Write(writer);
        }

        internal void AddBag(SF2Bag bag)
        {
            bags.Add(bag);
            Size = Count * SF2Bag.Size;
            sf2.UpdateSize();
        }

        public override string ToString() => $"Bag Chunk - Name = \"{ChunkName}\", " +
            $"\nBag count = {Count}";
    }

    public sealed class MODSubChunk : SF2Chunk
    {
        readonly List<SF2ModulatorList> modulators = new List<SF2ModulatorList>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2ModulatorList> Modulators
        {
            get
            {
                return modulators;
            }
        }

        public uint Count => (uint)modulators.Count;

        internal MODSubChunk(SF2 inSf2, bool preset) : base(inSf2, preset ? "pmod" : "imod") { }
        internal MODSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2ModulatorList.Size; i++)
                modulators.Add(new SF2ModulatorList(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                modulators[i].Write(writer);
        }

        internal void AddModulator(SF2ModulatorList modulator)
        {
            modulators.Add(modulator);
            Size = Count * SF2ModulatorList.Size;
            sf2.UpdateSize();
        }

        public override string ToString() => $"Modulator Chunk - Name = \"{ChunkName}\", " +
            $"\nModulator count = {Count}";
    }

    public sealed class GENSubChunk : SF2Chunk
    {
        readonly List<SF2GeneratorList> generators = new List<SF2GeneratorList>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2GeneratorList> Generators
        {
            get
            {
                return generators;
            }
        }

        public uint Count => (uint)generators.Count;

        internal GENSubChunk(SF2 inSf2, bool preset) : base(inSf2, preset ? "pgen" : "igen") { }
        internal GENSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2GeneratorList.Size; i++)
                generators.Add(new SF2GeneratorList(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                generators[i].Write(writer);
        }

        internal void AddGenerator(SF2GeneratorList generator)
        {
            generators.Add(generator);
            Size = Count * SF2GeneratorList.Size;
            sf2.UpdateSize();
        }

        public override string ToString() => $"Generator Chunk - Name = \"{ChunkName}\", " +
            $"\nGenerator count = {Count}";
    }

    public sealed class SHDRSubChunk : SF2Chunk
    {
        readonly List<SF2SampleHeader> samples = new List<SF2SampleHeader>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2SampleHeader> Samples
        {
            get
            {
                return samples;
            }
        }

        public uint Count => (uint)samples.Count;

        internal SHDRSubChunk(SF2 inSf2) : base(inSf2, "shdr") { }
        internal SHDRSubChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            for (int i = 0; i < Size / SF2SampleHeader.Size; i++)
                samples.Add(new SF2SampleHeader(inSf2, reader));
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            for (int i = 0; i < Count; i++)
                samples[i].Write(writer);
        }

        internal uint AddSample(SF2SampleHeader sample)
        {
            samples.Add(sample);
            Size = Count * SF2SampleHeader.Size;
            sf2.UpdateSize();
            return Count - 1;
        }

        public override string ToString() => $"Sample Header Chunk - Sample header count = {Count}";
    }

    #endregion

    #region Main Chunks

    public sealed class InfoListChunk : SF2ListChunk
    {
        readonly List<SF2Chunk> subChunks = new List<SF2Chunk>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SF2Chunk> SubChunks
        {
            get
            {
                return subChunks;
            }
        }

        const string defaultEngine = "EMU8000";
        public string Engine
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "isng") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                {
                    subChunks.Add(new HeaderSubChunk(sf2, "isng") { Field = defaultEngine });
                    return defaultEngine;
                }
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "isng") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "isng") { Field = value });
            }
        }
        const string defaultBank = "General MIDI";
        public string Bank
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "INAM") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                {
                    subChunks.Add(new HeaderSubChunk(sf2, "INAM") { Field = defaultBank });
                    return defaultBank;
                }
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "INAM") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "INAM") { Field = value });
            }
        }
        public string ROM
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "irom") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "irom") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "irom") { Field = value });
            }
        }
        public SF2VersionTag ROMVersion
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "iver") is VersionSubChunk chunk)
                    return chunk.Version;
                else
                    return null;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "iver") is VersionSubChunk chunk)
                    chunk.Version = value;
                else
                    subChunks.Add(new VersionSubChunk(sf2, "iver") { Version = value });
            }
        }
        public string Date
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "ICRD") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "ICRD") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "ICRD") { Field = value });
            }
        }
        public string Designer
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "IENG") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "IENG") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "IENG") { Field = value });
            }
        }
        public string Products
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "IPRD") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "IPRD") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "IPRD") { Field = value });
            }
        }
        public string Copyright
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "ICOP") is HeaderSubChunk icop)
                    return icop.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "ICOP") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "ICOP") { Field = value });
            }
        }
        const int commentMaxSize = 0x10000;
        public string Comment
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "ICMT") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "ICMT") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "ICMT", commentMaxSize) { Field = value });
            }
        }
        public string Tools
        {
            get
            {
                if (subChunks.Find(s => s.ChunkName == "ISFT") is HeaderSubChunk chunk)
                    return chunk.Field;
                else
                    return string.Empty;
            }
            set
            {
                if (subChunks.Find(s => s.ChunkName == "ISFT") is HeaderSubChunk chunk)
                    chunk.Field = value;
                else
                    subChunks.Add(new HeaderSubChunk(sf2, "ISFT") { Field = value });
            }
        }
        
        internal InfoListChunk(SF2 inSf2) : base(inSf2, "INFO")
        {
            // Mandatory sub-chunks
            subChunks.Add(new VersionSubChunk(inSf2, "ifil") { Version = new SF2VersionTag(2, 1) });
            subChunks.Add(new HeaderSubChunk(inSf2, "isng") { Field = defaultEngine });
            subChunks.Add(new HeaderSubChunk(inSf2, "INAM") { Field = defaultBank });
            sf2.UpdateSize();
        }
        internal InfoListChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            var startOffset = reader.BaseStream.Position;
            while (reader.BaseStream.Position < startOffset + Size - 4) // The 4 represents the INFO that was already read
            {
                // Peek 4 chars for the chunk name
                char[] name = reader.ReadChars(4);
                reader.BaseStream.Position -= 4;
                string strName = new string(name);
                switch (strName)
                {
                    case "ICMT": subChunks.Add(new HeaderSubChunk(inSf2, reader, commentMaxSize)); break;
                    case "ifil":
                    case "iver": subChunks.Add(new VersionSubChunk(inSf2, reader)); break;
                    case "isng":
                    case "INAM":
                    case "ICRD":
                    case "IENG":
                    case "IPRD":
                    case "ICOP":
                    case "ISFT":
                    case "irom": subChunks.Add(new HeaderSubChunk(inSf2, reader)); break;
                    default:
                        throw new NotSupportedException($"Unsupported chunk name at 0x{reader.BaseStream.Position:X}: \"{strName}\"");
                }
            }
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            foreach (var sub in subChunks)
                sub.Write(writer);
        }

        internal override uint UpdateSize()
        {
            Size = 4;
            foreach (var sub in subChunks)
                Size += sub.Size + 8;
            return Size;
        }

        public override string ToString() => $"Info List Chunk - Sub-chunk count = {subChunks.Count}";
    }

    public sealed class SdtaListChunk : SF2ListChunk
    {
        public readonly SMPLSubChunk SMPLSubChunk;

        internal SdtaListChunk(SF2 inSf2) : base(inSf2, "sdta")
        {
            SMPLSubChunk = new SMPLSubChunk(inSf2);
            sf2.UpdateSize();
        }
        internal SdtaListChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            SMPLSubChunk = new SMPLSubChunk(inSf2, reader);
        }
        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            SMPLSubChunk.Write(writer);
        }

        internal override uint UpdateSize()
        {
            return Size = 4
                + SMPLSubChunk.Size + 8;
        }

        public override string ToString() => $"Sample Data List Chunk";
    }

    public sealed class PdtaListChunk : SF2ListChunk
    {
        public readonly PHDRSubChunk PHDRSubChunk;
        public readonly BAGSubChunk PBAGSubChunk;
        public readonly MODSubChunk PMODSubChunk;
        public readonly GENSubChunk PGENSubChunk;
        public readonly INSTSubChunk INSTSubChunk;
        public readonly BAGSubChunk IBAGSubChunk;
        public readonly MODSubChunk IMODSubChunk;
        public readonly GENSubChunk IGENSubChunk;
        public readonly SHDRSubChunk SHDRSubChunk;

        internal PdtaListChunk(SF2 inSf2) : base(inSf2, "pdta")
        {
            PHDRSubChunk = new PHDRSubChunk(inSf2);
            PBAGSubChunk = new BAGSubChunk(inSf2, true);
            PMODSubChunk = new MODSubChunk(inSf2, true);
            PGENSubChunk = new GENSubChunk(inSf2, true);
            INSTSubChunk = new INSTSubChunk(inSf2);
            IBAGSubChunk = new BAGSubChunk(inSf2, false);
            IMODSubChunk = new MODSubChunk(inSf2, false);
            IGENSubChunk = new GENSubChunk(inSf2, false);
            SHDRSubChunk = new SHDRSubChunk(inSf2);
            sf2.UpdateSize();
        }
        internal PdtaListChunk(SF2 inSf2, BinaryReader reader) : base(inSf2, reader)
        {
            PHDRSubChunk = new PHDRSubChunk(inSf2, reader);
            PBAGSubChunk = new BAGSubChunk(inSf2, reader);
            PMODSubChunk = new MODSubChunk(inSf2, reader);
            PGENSubChunk = new GENSubChunk(inSf2, reader);
            INSTSubChunk = new INSTSubChunk(inSf2, reader);
            IBAGSubChunk = new BAGSubChunk(inSf2, reader);
            IMODSubChunk = new MODSubChunk(inSf2, reader);
            IGENSubChunk = new GENSubChunk(inSf2, reader);
            SHDRSubChunk = new SHDRSubChunk(inSf2, reader);
        }

        internal override uint UpdateSize()
        {
            return Size = 4
                + PHDRSubChunk.Size + 8
                + PBAGSubChunk.Size + 8
                + PMODSubChunk.Size + 8
                + PGENSubChunk.Size + 8
                + INSTSubChunk.Size + 8
                + IBAGSubChunk.Size + 8
                + IMODSubChunk.Size + 8
                + IGENSubChunk.Size + 8
                + SHDRSubChunk.Size + 8;
        }

        internal override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            PHDRSubChunk.Write(writer);
            PBAGSubChunk.Write(writer);
            PMODSubChunk.Write(writer);
            PGENSubChunk.Write(writer);
            INSTSubChunk.Write(writer);
            IBAGSubChunk.Write(writer);
            IMODSubChunk.Write(writer);
            IGENSubChunk.Write(writer);
            SHDRSubChunk.Write(writer);
        }

        public override string ToString() => $"Hydra List Chunk";
    }

    #endregion
}
