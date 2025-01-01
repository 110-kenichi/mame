using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace zanac.MAmidiMEmo.TurboLink
{

    public class FileInfo
    {
        public string name;
        public int size;
        public UInt16 date;
        public UInt16 time;
        public byte attrib;
    }

    public class Vdc
    {
        public const int size = 8;
        public UInt16 v50;
        public UInt16 v25;
        public UInt16 v12;
        public UInt16 vbt;

        public Vdc(byte[] data)
        {
            v50 = BitConverter.ToUInt16(data, 0);
            v25 = BitConverter.ToUInt16(data, 2);
            v12 = BitConverter.ToUInt16(data, 4);
            vbt = BitConverter.ToUInt16(data, 6);
        }

    }


    class Edio
    {

        const byte STATUS_KEY = 0x5A;
        const byte PROTOCOL_ID = 0x02;

        const int ACK_BLOCK_SIZE = 1024;

        public const int ADDR_CFG = 0x1800000; //system config
        public const int ADDR_ROM = 0x0000000;
        public const int ADDR_FIFO = 0x1810000;


        public const int ADDR_FLA_ICOR = 0x00000; //mcu firmware update

        public const int SIZE_ROM = 0x1000000;
        public const int SIZE_IOCOR = 0x20008;
        public const int SIZE_FLASH = 0x800000;

        public const int MAX_ROM_SIZE = 0x800000;


        public const byte FAT_READ = 0x01;
        public const byte FAT_WRITE = 0x02;
        public const byte FAT_OPEN_EXISTING = 0x00;
        public const byte FAT_CREATE_NEW = 0x04;
        public const byte FAT_CREATE_ALWAYS = 0x08;
        public const byte FAT_OPEN_ALWAYS = 0x10;
        public const byte FAT_OPEN_APPEND = 0x30;

        public const byte HOST_RST_OFF = 0;
        public const byte HOST_RST = 1;

        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_IO_RST = 0x12;
        const byte CMD_GET_VDC = 0x13;
        const byte CMD_RTC_GET = 0x14;
        const byte CMD_RTC_SET = 0x15;
        const byte CMD_FLA_RD = 0x16;
        const byte CMD_FLA_WR = 0x17;
        const byte CMD_FLA_WR_SDC = 0x18;
        const byte CMD_MEM_RD = 0x19;
        public const byte CMD_MEM_WR = 0x1A;
        const byte CMD_MEM_SET = 0x1B;
        const byte CMD_MEM_TST = 0x1C;
        const byte CMD_MEM_CRC = 0x1D;
        const byte CMD_FPG_USB = 0x1E;
        const byte CMD_FPG_SDC = 0x1F;
        const byte CMD_FPG_FLA = 0x20;
        const byte CMD_FPG_CFG = 0x21;
        const byte CMD_USB_WR = 0x22;
        const byte CMD_FIFO_WR = 0x23;
        const byte CMD_UART_WR = 0x24;
        const byte CMD_REINIT = 0x25;
        const byte CMD_SYS_INF = 0x26;
        const byte CMD_GAME_CTR = 0x27;
        const byte CMD_UPD_EXEC = 0x28;
        const byte CMD_HOST_RST = 0x29;


        const byte CMD_DISK_INIT = 0xC0;
        const byte CMD_DISK_RD = 0xC1;
        const byte CMD_DISK_WR = 0xC2;
        const byte CMD_F_DIR_OPN = 0xC3;
        const byte CMD_F_DIR_RD = 0xC4;
        const byte CMD_F_DIR_LD = 0xC5;
        const byte CMD_F_DIR_SIZE = 0xC6;
        const byte CMD_F_DIR_PATH = 0xC7;
        const byte CMD_F_DIR_GET = 0xC8;
        const byte CMD_F_FOPN = 0xC9;
        const byte CMD_F_FRD = 0xCA;
        const byte CMD_F_FRD_MEM = 0xCB;
        const byte CMD_F_FWR = 0xCC;
        const byte CMD_F_FWR_MEM = 0xCD;
        const byte CMD_F_FCLOSE = 0xCE;
        const byte CMD_F_FPTR = 0xCF;
        const byte CMD_F_FINFO = 0xD0;
        const byte CMD_F_FCRC = 0xD1;
        const byte CMD_F_DIR_MK = 0xD2;
        const byte CMD_F_DEL = 0xD3;

        const byte CMD_USB_RECOV = 0xF0;
        const byte CMD_RUN_APP = 0xF1;

        byte force_rst;

        public Edio()
        {
            seek();
            force_rst = HOST_RST_OFF;
        }

        public Edio(SerialPort port, string port_name)
        {
            openConnrction(port_name);
            force_rst = HOST_RST_OFF;
        }

        void seek()
        {
            string[] ports = SerialPort.GetPortNames();

            for (int i = 0; i < ports.Length; i++)
            {
                try
                {
                    openConnrction(ports[i]);
                    return;
                }
                catch (Exception) { }
            }

            throw new Exception("EverDrive not found");
        }

        void openConnrction(string pname)
        {
            SerialPort port = null;
            try
            {
                port = new SerialPort(pname);
                port.ReadTimeout = 300;
                port.WriteTimeout = 300;
                port.Open();
                port.ReadExisting();
                getStatus(port);
                port.ReadTimeout = 2000;
                port.WriteTimeout = 2000;
                return;
            }
            catch (Exception) { }


            try
            {
                port?.Close();
            }
            catch (Exception) { }

            port = null;

            throw new Exception("EverDrive not found");

        }
        /*
        public string PortName
        {
            get
            {
                return port.PortName;
            }
        }

        public int rxTout
        {

            get
            {
                return port.ReadTimeout;
            }

            set
            {
                port.ReadTimeout = value;
            }
        }
        */
        //************************************************************************************************ 

        public static void tx32(SerialPort port, int arg)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)(arg >> 0);
            buff[1] = (byte)(arg >> 8);
            buff[2] = (byte)(arg >> 16);
            buff[3] = (byte)(arg >> 24);

            txData(port, buff, 0, buff.Length);
        }

        public static byte[] getTx32(SerialPort port, int arg)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)(arg >> 0);
            buff[1] = (byte)(arg >> 8);
            buff[2] = (byte)(arg >> 16);
            buff[3] = (byte)(arg >> 24);

            return buff;
        }

        public int rx32(SerialPort port)
        {
            byte[] buff = new byte[4];
            rxData(port, buff, 0, buff.Length);
            return buff[0] | (buff[1] << 8) | (buff[2] << 16) | (buff[3] << 24);
        }


        void tx16(SerialPort port, int arg)
        {
            byte[] buff = new byte[2];
            buff[0] = (byte)(arg >> 0);
            buff[1] = (byte)(arg >> 8);

            txData(port, buff, 0, buff.Length);
        }

        public UInt16 rx16(SerialPort port)
        {
            byte[] buff = new byte[2];
            rxData(port, buff, 0, buff.Length);
            return (UInt16)(buff[0] | (buff[1] << 8));
        }

        public static void tx8(SerialPort port, int arg)
        {
            byte[] buff = new byte[1];
            buff[0] = (byte)(arg);
            txData(port, buff, 0, buff.Length);
        }

        public static byte[] getTx8(SerialPort port, int arg)
        {
            byte[] buff = new byte[1];
            buff[0] = (byte)(arg);
            return buff;
        }

        public byte rx8(SerialPort port)
        {
            return (byte)port.ReadByte();
        }

        public int rxAvailalbe(SerialPort port)
        {
            return port.BytesToRead;
        }


        public static void txData(SerialPort port, byte[] buff)
        {
            txData(port, buff, 0, buff.Length);
        }

        public static void txData(SerialPort port, byte[] buff, int offset, int len)
        {
            while (len > 0)
            {
                int block = 8192;
                if (block > len) block = len;

                port.Write(buff, offset, block);
                len -= block;
                offset += block;

            }
        }

        void txData(SerialPort port, string str)
        {
            port.Write(str);
        }



        void txDataACK(SerialPort port, byte[] buff, int offset, int len)
        {
            while (len > 0)
            {
                int resp = rx8(port);
                if (resp != 0) throw new Exception("tx ack: " + resp.ToString("X2"));

                int block = ACK_BLOCK_SIZE;
                if (block > len) block = len;

                txData(port, buff, offset, block);

                len -= block;
                offset += block;

            }
        }


        void rxData(SerialPort port, byte[] buff, int offset, int len)
        {
            for (int i = 0; i < len;)
            {
                i += port.Read(buff, offset + i, len - i);

            }
        }

        public byte[] rxData(SerialPort port, int len)
        {
            byte[] buff = new byte[len];
            rxData(port, buff, 0, len);
            return buff;
        }

        void rxData(SerialPort port, byte[] buff, int len)
        {
            rxData(port, buff, 0, len);
        }

        void txString(SerialPort port, string str)
        {
            tx16(port, str.Length);
            txData(port, str);
        }

        public string rxString(SerialPort port)
        {
            int len = rx16(port);
            byte[] buff = new byte[len];
            rxData(port, buff, 0, buff.Length);
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        FileInfo rxFileInfo(SerialPort port)
        {
            FileInfo inf = new FileInfo();

            inf.size = rx32(port);
            inf.date = rx16(port);
            inf.time = rx16(port);
            inf.attrib = rx8(port);
            inf.name = rxString(port);

            return inf;
        }

        //public SerialPort getPort()
        //{
        //    return port;
        //}

        public int dataAvailable(SerialPort port)
        {
            return port.BytesToRead;
        }

        public void flush(SerialPort port)
        {

            int len = dataAvailable(port);
            if (len > 0x10000) len = 0x10000;
            byte[] buff = new byte[len];
            port.Read(buff, 0, buff.Length);
        }

        //************************************************************************************************ 

        public static void txCMD(SerialPort port, byte cmd_code)
        {
            byte[] cmd = new byte[4];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            txData(port, cmd);
        }

        public static byte[] getTxCMD(SerialPort port, byte cmd_code)
        {
            byte[] cmd = new byte[4];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            return cmd;
        }

        void checkStatus(SerialPort port)
        {
            int resp = getStatus(port);
            if (resp != 0) throw new Exception("operation error: " + resp.ToString("X2"));
        }

        public int getStatus(SerialPort port)
        {
            byte[] resp = getID(port);

            if (resp[0] != STATUS_KEY || resp[1] != PROTOCOL_ID)
            {
                throw new Exception("unexpected status response (" + BitConverter.ToString(resp) + ")");
            }
            return resp[3];
        }

        public byte[] getID(SerialPort port)
        {
            txCMD(port, CMD_STATUS);
            return rxData(port, 4);
        }

        public void diskInit(SerialPort port)
        {
            txCMD(port, CMD_DISK_INIT);
            checkStatus(port);
        }

        public void diskRead(SerialPort port, int addr, byte slen, byte[] buff)
        {
            byte resp;

            txCMD(port, CMD_DISK_RD);
            tx32(port, addr);
            tx32(port, slen);


            for (int i = 0; i < slen; i++)
            {
                resp = (byte)port.ReadByte();
                if (resp != 0) throw new Exception("disk read error: " + resp);
                rxData(port, buff, i * 512, 512);
            }

        }


        public void dirOpen(SerialPort port, string path)
        {
            txCMD(port, CMD_F_DIR_OPN);
            txString(port, path);
            checkStatus(port);
        }

        public FileInfo dirRead(SerialPort port, UInt16 max_name_len)
        {

            int resp;
            if (max_name_len == 0) max_name_len = 0xffff;
            txCMD(port, CMD_F_DIR_RD);
            tx16(port, max_name_len);//max name lenght
            resp = rx8(port);

            if (resp != 0) throw new Exception("dir read error: " + resp.ToString("X2"));

            return rxFileInfo(port);

        }

        public void dirLoad(SerialPort port, string path, int sorted)
        {

            txCMD(port, CMD_F_DIR_LD);
            tx8(port, sorted);
            txString(port, path);
            checkStatus(port);
        }


        public int dirGetSize(SerialPort port)
        {
            txCMD(port, CMD_F_DIR_SIZE);
            return rx16(port);
        }

        public FileInfo[] dirGetRecs(SerialPort port, int start_idx, int amount, int max_name_len)
        {
            FileInfo[] inf = new FileInfo[amount];
            byte resp;

            txCMD(port, CMD_F_DIR_GET);
            tx16(port, start_idx);
            tx16(port, amount);
            tx16(port, max_name_len);



            for (int i = 0; i < amount; i++)
            {
                resp = rx8(port);
                if (resp != 0) throw new Exception("dir read error: " + resp.ToString("X2"));
                inf[i] = rxFileInfo(port);

            }

            return inf;
        }

        public void dirMake(SerialPort port, string path)
        {
            txCMD(port, CMD_F_DIR_MK);
            txString(port, path);
            int resp = getStatus(port);
            if (resp != 0 && resp != 8)//ignore error 8 (already exist)
            {
                checkStatus(port);
            }
        }

        public void fileOpen(SerialPort port, string path, int mode)
        {
            txCMD(port, CMD_F_FOPN);
            tx8(port, mode);
            txString(port, path);
            checkStatus(port);
        }

        public void fileRead(SerialPort port, byte[] buff, int offset, int len)
        {

            txCMD(port, CMD_F_FRD);
            tx32(port, len);

            while (len > 0)
            {
                int block = 4096;
                if (block > len) block = len;
                int resp = rx8(port);
                if (resp != 0) throw new Exception("file read error: " + resp.ToString("X2"));

                rxData(port, buff, offset, block);
                offset += block;
                len -= block;

            }

        }

        public void fileRead(SerialPort port, int addr, int len)
        {


            while (len > 0)
            {
                int block = 0x10000;
                if (block > len) block = len;

                txCMD(port, CMD_F_FRD_MEM);
                tx32(port, addr);
                tx32(port, block);
                tx8(port, 0);//exec
                checkStatus(port);

                len -= block;
                addr += block;

            }

        }

        public void fileWrite(SerialPort port, byte[] buff, int offset, int len)
        {
            txCMD(port, CMD_F_FWR);
            tx32(port, len);
            txDataACK(port, buff, offset, len);
            checkStatus(port);
        }

        public void fileWrite(SerialPort port, int addr, int len)
        {
            while (len > 0)
            {
                int block = 0x10000;
                if (block > len) block = len;

                txCMD(port, CMD_F_FWR_MEM);
                tx32(port, addr);
                tx32(port, block);
                tx8(port, 0);//exec
                checkStatus(port);

                len -= block;
                addr += block;

            }
        }

        public void fileSetPtr(SerialPort port, int addr)
        {
            txCMD(port, CMD_F_FPTR);
            tx32(port, addr);
            checkStatus(port);
        }

        public void fileClose(SerialPort port)
        {
            txCMD(port, CMD_F_FCLOSE);
            checkStatus(port);
        }

        public void delRecord(SerialPort port, string path)
        {
            txCMD(port, CMD_F_DEL);
            txString(port, path);
            checkStatus(port);
        }


        public void memWR(SerialPort port, int addr, byte[] buff, int offset, int len)
        {
            if (len == 0) return;
            txCMD(port, CMD_MEM_WR);
            tx32(port, addr);
            tx32(port, len);
            tx8(port, 0);//exec
            txData(port, buff, offset, len);
        }

        public void memRD(SerialPort port, int addr, byte[] buff, int offset, int len)
        {
            if (len == 0) return;
            txCMD(port, CMD_MEM_RD);
            tx32(port, addr);
            tx32(port, len);
            tx8(port, 0);//exec
            rxData(port, buff, offset, len);
        }

        public FileInfo fileInfo(SerialPort port, string path)
        {
            txCMD(port, CMD_F_FINFO);
            txString(port, path);
            int resp = rx8(port);
            if (resp != 0) throw new Exception("file access error: " + resp.ToString("X2"));
            return rxFileInfo(port);

        }

        public void fifoWR(SerialPort port, byte[] data, int offset, int len)
        {
            memWR(port, ADDR_FIFO, data, offset, len);
        }

        public void fifoWR(SerialPort port, string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            memWR(port, ADDR_FIFO, bytes, 0, bytes.Length);
        }

        public void fifoTxString(SerialPort port, string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            byte[] len = new byte[2];
            len[0] = (byte)(bytes.Length >> 0);
            len[1] = (byte)(bytes.Length >> 8);
            fifoWR(port, len, 0, 2);
            fifoWR(port, bytes, 0, bytes.Length);
        }

        public void fifoTX32(SerialPort port, int arg)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)(arg >> 0);
            buff[1] = (byte)(arg >> 8);
            buff[2] = (byte)(arg >> 16);
            buff[3] = (byte)(arg >> 24);

            fifoWR(port, buff, 0, buff.Length);
        }

        public void memSet(SerialPort port, byte val, int addr, int len)
        {
            txCMD(port, CMD_MEM_SET);
            tx32(port, addr);
            tx32(port, len);
            tx8(port, val);
            tx8(port, 0);//exec
            checkStatus(port);
        }

        public bool memTest(SerialPort port, byte val, int addr, int len)
        {

            txCMD(port, CMD_MEM_TST);
            tx32(port, addr);
            tx32(port, len);
            tx8(port, val);
            tx8(port, 0);//exec

            if (rx8(port) == 0) return false;

            return true;
        }


        public UInt32 memCRC(SerialPort port, int addr, int len)
        {
            txCMD(port, CMD_MEM_CRC);
            tx32(port, addr);
            tx32(port, len);
            tx32(port, 0);//crc init val
            tx8(port, 0);//exec

            return (UInt32)rx32(port);
        }

        public UInt32 fileCRC(SerialPort port, int len)
        {
            int resp;
            txCMD(port, CMD_F_FCRC);
            tx32(port, len);
            tx32(port, 0);//crc init val

            resp = rx8(port);
            if (resp != 0) throw new Exception("Disk read error: " + resp.ToString("X2"));


            return (UInt32)rx32(port);
        }

        public void flaRD(SerialPort port, int addr, byte[] buff, int offset, int len)
        {
            txCMD(port, CMD_FLA_RD);
            tx32(port, addr);
            tx32(port, len);
            rxData(port, buff, offset, len);
        }


        public void flaWR(SerialPort port, int addr, byte[] buff, int offset, int len)
        {
            if (addr + len > SIZE_FLASH)
            {
                throw new Exception("out of flash size");
            }
            txCMD(port, CMD_FLA_WR);
            tx32(port, addr);
            tx32(port, len);
            txDataACK(port, buff, offset, len);
            checkStatus(port);
        }

        public void fpgInit(SerialPort port, byte[] data)
        {
            txCMD(port, CMD_FPG_USB);
            tx32(port, data.Length);
            txDataACK(port, data, 0, data.Length);
            checkStatus(port);
        }


        public void fpgInit(SerialPort port, int flash_addr)
        {
            txCMD(port, CMD_FPG_FLA);
            tx32(port, flash_addr);
            tx8(port, 0);//exec
            checkStatus(port);
        }

        public void fpgInit(SerialPort port, string sd_path)
        {

            FileInfo f = fileInfo(port, sd_path);
            fileOpen(port, sd_path, FAT_READ);
            txCMD(port, CMD_FPG_SDC);
            tx32(port, f.size);
            tx8(port, 0);
            checkStatus(port);
        }



        public bool isServiceMode(SerialPort port)
        {
            txCMD(port, CMD_GET_MODE);
            byte resp = rx8(port);
            if (resp == 0xA1) return true;
            return false;
        }

        public Vdc GetVdc(SerialPort port)
        {
            txCMD(port, CMD_GET_VDC);
            byte[] buff = rxData(port, Vdc.size);
            Vdc vdc = new Vdc(buff);
            return vdc;
        }

        //public RtcTime rtcGet()
        //{
        //    txCMD(CMD_RTC_GET);
        //    byte[] buff = rxData(RtcTime.size);
        //    RtcTime rtc = new RtcTime(buff);
        //    return rtc;
        //}

        //public void rtcSet(DateTime dt)
        //{
        //    RtcTime rtc = new RtcTime(dt);
        //    byte[] vals = rtc.getVals();
        //    txCMD(CMD_RTC_SET);
        //    txData(vals);
        //}

        public void hostReset(SerialPort port, byte rst)
        {
            if (force_rst != HOST_RST_OFF && rst != HOST_RST_OFF)
            {
                rst = force_rst;
            }
            txCMD(port, CMD_HOST_RST);
            tx8(port, rst);
        }

        public void configReset(SerialPort port)
        {
            byte[] buff = new byte[256];
            memWR(port, ADDR_CFG, buff, 0, buff.Length);
        }

        public void forceRstType(SerialPort port, byte rst)
        {
            force_rst = rst;
        }

        public void updExec(SerialPort port, int addr, int crc)
        {
            txCMD(port, CMD_UPD_EXEC);
            tx32(port, addr);
            tx32(port, crc);
            tx8(port, 0);//exec
        }
        //************************************************************************************************ usb service mode. System enters in service mode if cart powered via usb only
        public void recovery(SerialPort port)
        {
            if (!isServiceMode(port))
            {
                throw new Exception("Device not in service mode");
            }


            byte[] crc = new byte[4];
            flaRD(port, ADDR_FLA_ICOR + 4, crc, 0, 4);
            int crc_int = (crc[0] << 0) | (crc[1] << 8) | (crc[2] << 16) | (crc[3] << 24);


            int old_tout = port.ReadTimeout;
            port.ReadTimeout = 8000;

            txCMD(port, CMD_USB_RECOV);
            tx32(port, ADDR_FLA_ICOR);
            tx32(port, crc_int);
            //txData(crc);
            int status = getStatus(port);

            port.ReadTimeout = old_tout;

            if (status == 0x88)
            {
                throw new Exception("current core matches to recovery copy");
            }
            else if (status != 0)
            {
                throw new Exception("recovery error: " + status.ToString("X2"));
            }

        }

        public void exitServiceMode(SerialPort port)
        {

            if (!isServiceMode(port)) return;

            txCMD(port, CMD_RUN_APP);
            bootWait(port);
            if (isServiceMode(port))
            {
                throw new Exception("Device stuck in service mode");
            }
        }

        public void enterServiceMode(SerialPort port)
        {
            if (isServiceMode(port)) return;

            txCMD(port, CMD_IO_RST);
            tx8(port, 0);
            bootWait(port);

            if (!isServiceMode(port))
            {
                throw new Exception("device stuck in APP mode");
            }
        }

        void bootWait(SerialPort port)
        {

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Thread.Sleep(100);
                    port.Close();
                    Thread.Sleep(100);
                    port.Open();
                    getStatus(port);
                    return;
                }
                catch (Exception) { }
            }

            throw new Exception("boot timeout");
        }
    }



}
