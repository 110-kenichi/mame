using System.Runtime.ExceptionServices;
using LegacyWrapper.Common.Wrapper;

namespace LegacyWrapper32
{
    /// <summary>
    /// 
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        [HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {
            WrapperHelper.Call(args);
        }   
    }
}
