using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Interop;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;
using ZeroFormatter;
using FastDelegate.Net;

namespace LegacyWrapper.Common.Wrapper
{
    public class WrapperHelper
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        /// <summary>
        /// Outsourced main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        [HandleProcessCorruptedStateExceptions]
        public static void Call(string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            string token = args[0];
            string libraryName = args[1];

            // Create new named pipe with token from client
            using (var pipe = new NamedPipeServerStream(token, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                pipe.WaitForConnection();

                try
                {
                    //CallData data = (CallData)formatter.Deserialize(pipe);
                    CallData data = ZeroFormatterSerializer.Deserialize<CallData>(getMessage(pipe));

                    // Load requested library
                    using (NativeLibrary library = NativeLibrary.Load(libraryName, NativeLibraryLoadOptions.SearchAll))
                    {
                        // Receive CallData from client

                        while (data.Status != KeepAliveStatus.Close)
                        {
                            InvokeFunction(data, pipe, library);

                            //data = (CallData)formatter.Deserialize(pipe);
                            data = ZeroFormatterSerializer.Deserialize<CallData>(getMessage(pipe));
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteExceptionToClient(pipe, e);
                }
            }
        }

        private static byte[] getMessage(NamedPipeServerStream pipe)
        {
            List<byte> buff = new List<byte>();
            do
            {
                int dt = pipe.ReadByte();
                if (dt < 0)
                    break;
                buff.Add((byte)dt);
            } while (!pipe.IsMessageComplete);

            return buff.ToArray();
        }

        private static Dictionary<string, Func<Object, Object[], Object>> loadedFunction = new Dictionary<string, Func<Object, Object[], Object>>();
        private static Dictionary<string, Delegate> loadedDelegate = new Dictionary<string, Delegate>();

        [HandleProcessCorruptedStateExceptions]
        private static void InvokeFunction(CallData data, Stream pipe, NativeLibrary library)
        {
            try
            {
                Func<Object, Object[], Object> func = null;
                Delegate delg = null;
                if (loadedFunction.ContainsKey(data.ProcedureName))
                {
                    func = loadedFunction[data.ProcedureName];
                    delg = loadedDelegate[data.ProcedureName];
                }
                else
                {
                    IntPtr fp = library.GetFunctionPointer(data.ProcedureName);
                    delg = Marshal.GetDelegateForFunctionPointer(fp, Type.GetType(data.DelegateType));
                    func = delg.Method.Bind();
                    loadedDelegate.Add(data.ProcedureName, delg);
                    loadedFunction.Add(data.ProcedureName, func);
                }

                // Invoke requested method

                object result = func.Invoke(delg, data.Parameters.OfType<object>().ToArray());

                CallResult callResult = new CallResult
                {
                    Result = result != null ? (uint)result : (uint?)null,
                    Parameters = (uint[])data.Parameters,
                };

                // Write result back to client
                //formatter.Serialize(pipe, callResult);
                ZeroFormatterSerializer.Serialize(pipe, callResult);
            }
            catch (Exception e)
            {
                WriteExceptionToClient(pipe, e);
            }
        }

        private static void WriteExceptionToClient(Stream pipe, Exception e)
        {
            var cr = new CallResult
            {
                Exception = "An error occured while calling a library function. See the inner exception for details.\r\n" + e.Message,
            };
            //formatter.Serialize(pipe, cr);
            ZeroFormatterSerializer.Serialize(pipe, cr);
        }
    }
}
