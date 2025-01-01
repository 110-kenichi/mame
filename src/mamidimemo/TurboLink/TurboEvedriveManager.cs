// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.TurboLink
{

    public static class TurboEvedriveManager
    {

        const byte STATUS_KEY = 0x5A;
        const byte PROTOCOL_ID = 0x02;

        const byte CMD_STATUS = 0x10;

        public static int getStatus(SerialPort port)
        {
            byte[] resp = getID(port);

            if (resp[0] != STATUS_KEY || resp[1] != PROTOCOL_ID)
            {
                throw new ApplicationException("unexpected status response (" + BitConverter.ToString(resp) + ")");
            }
            return resp[3];
        }

        public static byte[] getID(SerialPort port)
        {
            txCMD(port, CMD_STATUS);
            return rxData(port, 4);
        }

        public static byte[] rxData(SerialPort port, int len)
        {
            byte[] buff = new byte[len];
            rxData(port, buff, 0, len);
            return buff;
        }

        public static void rxData(SerialPort port, byte[] buff, int offset, int len)
        {
            for (int i = 0; i < len;)
            {
                i += port.Read(buff, offset + i, len - i);

            }
        }

        public static void txCMD(SerialPort port, byte cmd_code)
        {
            byte[] cmd = new byte[4];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            txData(port, cmd);
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
    }


}
