// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class WaveFileReader
    {

        public static WaveFileHeader ReadWaveFile(string waveFilePath)
        {
            // ファイルの存在を確認する
            if (!File.Exists(waveFilePath))
                return null;

            WaveFileHeader waveHeader = new WaveFileHeader();

            using (FileStream fs = new FileStream(waveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                waveReaderCore(waveHeader, fs);
            }

            return waveHeader;
        }


        public static WaveFileHeader ReadWaveData(byte[] waveData)
        {
            WaveFileHeader waveHeader = new WaveFileHeader();

            using (MemoryStream fs = new MemoryStream(waveData))
            {
                waveReaderCore(waveHeader, fs);
            }

            return waveHeader;
        }

        private static void waveReaderCore(WaveFileHeader waveHeader, Stream fs)
        {
            BinaryReader br = new BinaryReader(fs);
            waveHeader.RiffHeader = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));
            waveHeader.FileSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
            waveHeader.WaveHeaderData = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));

            bool readFmtChunk = false;
            bool readDataChunk = false;
            while (!readFmtChunk || !readDataChunk)
            {
                // ChunkIDを取得する
                string chunk = Encoding.GetEncoding(20127).GetString(br.ReadBytes(4));

                if (chunk.ToLower().CompareTo("fmt ") == 0)
                {
                    // fmtチャンクの読み込み
                    waveHeader.FormatChunk = chunk;
                    waveHeader.FormatChunkSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    waveHeader.FormatID = BitConverter.ToInt16(br.ReadBytes(2), 0);
                    waveHeader.Channel = BitConverter.ToInt16(br.ReadBytes(2), 0);
                    waveHeader.SampleRate = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    waveHeader.BytePerSec = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    waveHeader.BlockSize = BitConverter.ToInt16(br.ReadBytes(2), 0);
                    waveHeader.BitPerSample = BitConverter.ToInt16(br.ReadBytes(2), 0);

                    int skip = waveHeader.FormatChunkSize - 2 - 2 - 4 - 4 - 2 - 2;
                    if(skip > 0)
                        br.ReadBytes(skip);

                    readFmtChunk = true;
                }
                else if (chunk.ToLower().CompareTo("data") == 0)
                {
                    // dataチャンクの読み込み
                    waveHeader.DataChunk = chunk;
                    waveHeader.DataChunkSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    waveHeader.Data = br.ReadBytes(waveHeader.DataChunkSize);

                    // 再生時間を算出する
                    int bytesPerSec = waveHeader.SampleRate * waveHeader.BlockSize;
                    waveHeader.PlayTimeMsec = (int)(((double)waveHeader.DataChunkSize / (double)bytesPerSec) * 1000);

                    readDataChunk = true;
                }
                else if (chunk.ToLower().CompareTo("smpl") == 0)
                {
                    // 不要なチャンクの読み捨て
                    Int32 size = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    if (0 < size)
                        br.ReadBytes(size);
                }
                else
                {
                    // 不要なチャンクの読み捨て
                    Int32 size = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    if (0 < size)
                        br.ReadBytes(size);
                }
            }
        }

    }

    public class WaveFileHeader
    {
        public string RiffHeader { get; internal set; }
        public string WaveHeaderData { get; internal set; }
        public int FileSize { get; internal set; }
        public string FormatChunk { get; internal set; }
        public int FormatChunkSize { get; internal set; }
        public short FormatID { get; internal set; }
        public short Channel { get; internal set; }
        public int SampleRate { get; internal set; }
        public int BytePerSec { get; internal set; }
        public short BlockSize { get; internal set; }
        public short BitPerSample { get; internal set; }
        public string DataChunk { get; internal set; }
        public int DataChunkSize { get; internal set; }
        public int PlayTimeMsec { get; internal set; }
        public byte[] Data { get; internal set; }
    }
}
