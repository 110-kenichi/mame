#region License
//Copyright 2014 Lukas Pokorny
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using LegacyWrapper.ErrorHandling;

namespace LegacyWrapper.Common.Interop
{
    /// <summary>
    /// Represents a native (unmanaged) dynamically loaded library.
    /// </summary>
    public class NativeLibrary : IDisposable
    {
        private readonly bool _windows;
        private readonly IntPtr _handle;
        private bool _disposed;

        private const string ErrorUnsupportedPlatform = "Unsupported platform.";
        private const string ErrorCannotLoadLibrary = "Unable to load a library.";
        private const string ErrorFunctionNotFound = "Function not found in a library.";

        private const string LibPrefix = "lib";

        private static string PlatformSuffix { get { return Environment.Is64BitProcess ? "64" : "32"; } }

        private static string _executableDirectory;

        /// <summary>
        /// Loads a native (unmanaged) dynamically loaded library. A return value indicates whether the method succeeded.
        /// </summary>
        /// <param name="name">Library name. Can be a full path as well.</param>
        /// <returns>An instance of <see cref="NativeLibrary"/>.</returns>
        public static NativeLibrary Load(string name)
        {
            return Load(name, true, NativeLibraryLoadOptions.None, null);
        }

        /// <summary>
        /// Loads a native (unmanaged) dynamically loaded library.
        /// </summary>
        /// <param name="name">Library name. Can be a full path as well.</param>
        /// <param name="library">When the method returns, contains an instance of <see cref="NativeLibrary"/> on success, or null on failure.</returns>
        /// <returns>True on succes; false on failure.</returns>
        public static bool TryLoad(string name, out NativeLibrary library)
        {
            library = Load(name, false, NativeLibraryLoadOptions.None, null);
            return library != null;
        }

        /// <summary>
        /// Loads a native (unmanaged) dynamically loaded library. A return value indicates whether the method succeeded.
        /// </summary>
        /// <param name="name">Library name. Can be a full path as well.</param>
        /// <param name="options">Load options.</param>
        /// <returns>An instance of <see cref="NativeLibrary"/>.</returns>
        public static NativeLibrary Load(string name, NativeLibraryLoadOptions options)
        {
            return Load(name, true, options, ((options & NativeLibraryLoadOptions.SearchAssemblyDirectory) != 0) ? Assembly.GetCallingAssembly() : null);
        }

        /// <summary>
        /// Loads a native (unmanaged) dynamically loaded library.
        /// </summary>
        /// <param name="name">Library name. Can be a full path as well.</param>
        /// <param name="options">Load options.</param>
        /// <param name="library">When the method returns, contains an instance of <see cref="NativeLibrary"/> on success, or null on failure.</returns>
        /// <returns>True on succes; false on failure.</returns>
        public static bool TryLoad(string name, NativeLibraryLoadOptions options, out NativeLibrary library)
        {
            library = Load(name, false, options, ((options & NativeLibraryLoadOptions.SearchAssemblyDirectory) != 0) ? Assembly.GetCallingAssembly() : null);
            return library != null;
        }

        private static NativeLibrary Load(string name, bool failOnError, NativeLibraryLoadOptions options, Assembly callingAssembly)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", "name");

            bool addExtension = !Path.HasExtension(name) && string.IsNullOrWhiteSpace(Path.GetDirectoryName(name));
            bool windows;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    windows = false;
                    if (addExtension)
                        name += ".so";
                    break;
                case PlatformID.Win32NT:
                    windows = true;
                    if (addExtension)
                        name += ".dll";
                    break;
                default:
                    throw new ApplicationException(ErrorUnsupportedPlatform);
            }

            string path;
            if (!Path.IsPathRooted(name))
                path = SearchLocations(name, options, windows, callingAssembly);
            else
                path = name;

            var library = new NativeLibrary(path, windows, failOnError);
            if (library._handle == IntPtr.Zero)
                return null;

            return library;
        }

        private static string SearchLocations(string name, NativeLibraryLoadOptions options, bool windows, Assembly callingAssembly)
        {
            if ((options & NativeLibraryLoadOptions.SearchAll) == 0)
                return name;

            bool nameIsPath = Path.IsPathRooted(name) || !string.IsNullOrEmpty(Path.GetDirectoryName(name));

            var locations = new NativeLibraryLoadOptions[] {
                NativeLibraryLoadOptions.SearchAssemblyDirectory,
                NativeLibraryLoadOptions.SearchExecutableDirectory,
                NativeLibraryLoadOptions.SearchCurrentDirectory
            };

            string assemblyDirectory = callingAssembly != null ? GetAssemblyDirectory(callingAssembly) : null;

            foreach (NativeLibraryLoadOptions location in locations)
            {
                string path = null;
                switch (location & options)
                {
                    case NativeLibraryLoadOptions.SearchAssemblyDirectory:
                        path = SearchFile(assemblyDirectory, name, nameIsPath, windows);
                        break;
                    case NativeLibraryLoadOptions.SearchExecutableDirectory:
                        path = SearchFile(GetExecutableDirectory(), name, nameIsPath, windows);
                        break;
                    case NativeLibraryLoadOptions.SearchCurrentDirectory:
                        path = SearchFile(Directory.GetCurrentDirectory(), name, nameIsPath, windows);
                        break;
                }
                if (path != null)
                    return path;
            }

            return name;
        }

        private static string SearchFile(string location, string name, bool nameIsPath, bool windows)
        {
            if (location == null)
                return null;

            string path;
            if (TryCheckExists(location, name, out path))
                return path;

            if (nameIsPath)
                return null;

            if (TryCheckExists(location, ExtendName("", name, PlatformSuffix), out path))
                return path;

            if (!windows && !name.StartsWith(LibPrefix, StringComparison.Ordinal))
            {
                if (TryCheckExists(location, ExtendName(LibPrefix, name, ""), out path))
                    return path;

                if (TryCheckExists(location, ExtendName(LibPrefix, name, PlatformSuffix), out path))
                    return path;
            }

            return null;
        }

        private static string ExtendName(string prefix, string name, string suffix)
        {
            if (!Path.HasExtension(name))
                return prefix + name + suffix;

            return prefix + Path.GetFileNameWithoutExtension(name) + suffix + Path.GetExtension(name);
        }

        private static bool TryCheckExists(string basePath, string fileName, out string filePath)
        {
            string path = Path.Combine(basePath, fileName);
            if (!File.Exists(path))
            {
                filePath = null;
                return false;
            }

            filePath = path;
            return true;
        }

        private static string GetAssemblyDirectory(Assembly assembly)
        {
            try
            {
                string path = Path.GetDirectoryName(assembly.Location);
                if (Directory.Exists(path))
                    return path;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string GetExecutableDirectory()
        {
            if (_executableDirectory == null)
                _executableDirectory = GetAssemblyDirectory(Assembly.GetEntryAssembly());

            return _executableDirectory;
        }

        private NativeLibrary(string name, bool windows, bool failOnError)
        {
            _windows = windows;

            if (_windows)
            {
                _handle = NativeMethods.LoadLibrary(name);
                if (_handle == IntPtr.Zero && failOnError)
                    throw new NativeLibraryException(ErrorCannotLoadLibrary + " " + Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()).Message);
            }
            else
            {
                _handle = NativeMethods.dlopen(name, NativeMethods.RTLD_GLOBAL | NativeMethods.RTLD_NOW);
                if (_handle == IntPtr.Zero && failOnError)
                    throw new NativeLibraryException(ErrorCannotLoadLibrary + " " + Marshal.PtrToStringAnsi(NativeMethods.dlerror()));
            }

        }

        /// <summary>
        /// Loads an exported unmanaged function and returns its pointer.
        /// </summary>
        /// <param name="name">Name of the exported function.</param>
        /// <returns>Unmanaged function pointeer.</returns>
        /// <remarks>
        /// Warning: Function pointers returned by this method MUST NOT be used after their parent <see cref="NativeLibrary"/> has been disposed.
        /// Doing so will crash the application.
        /// </remarks>
        public IntPtr GetFunctionPointer(string name)
        {
            return GetFunctionPointer(name, true);
        }

        /// <summary>
        /// Loads an exported unmanaged function and returns its pointer. A return value indicates whether the method succeeded.
        /// </summary>
        /// <param name="name">Name of the exported function.</param>
        /// <param name="functionPointer">When the method returns, contains an unmanaged function pointer on success, or IntPtr.Zero on failure.</returns>
        /// <returns>True on succes; false on failure.</returns>
        /// <remarks>
        /// Warning: Function pointers returned by this method MUST NOT be used after their parent <see cref="NativeLibrary"/> has been disposed.
        /// Doing so will crash the application.
        /// </remarks>
        public bool TryGetFunctionPointer(string name, out IntPtr functionPointer)
        {
            functionPointer = GetFunctionPointer(name, false);
            return functionPointer != IntPtr.Zero;
        }

        /// <summary>
        /// Loads an exported unmanaged function and converts it into a delegate.
        /// </summary>
        /// <typeparam name="T">Delegate type. Must be subclass of <see cref="Delegate"/>.</typeparam>
        /// <param name="name">Name of the exported function.</param>
        /// <returns>Delegate for the specified unmanaged function.</returns>
        /// <remarks>
        /// Warning: Delegates returned by this method MUST NOT be used after their parent <see cref="NativeLibrary"/> has been disposed.
        /// Doing so will crash the application.
        /// </remarks>
        public T GetDelegate<T>(string name)
            where T : class
        {
            return GetDelegate<T>(name, true);
        }

        /// <summary>
        /// Loads an exported unmanaged function and converts it into a delegate. A return value indicates whether the method succeeded.
        /// </summary>
        /// <typeparam name="T">Delegate type. Must be subclass of <see cref="Delegate"/>.</typeparam>
        /// <param name="name">Name of the exported function.</param>
        /// <param name="functionDelegate">When the method returns, contains a delegate for the specified unmanaged function on success, or null on failure.</returns>
        /// <returns>True on succes; false on failure.</returns>
        /// <remarks>
        /// Warning: Delegates returned by this method MUST NOT be used after their parent <see cref="NativeLibrary"/> has been disposed.
        /// Doing so will crash the application.
        /// </remarks>
        public bool TryGetDelegate<T>(string name, out T functionDelegate)
            where T : class
        {
            functionDelegate = GetDelegate<T>(name, false);
            return functionDelegate != null;
        }

        private IntPtr GetFunctionPointer(string name, bool failOnError)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", "name");

            IntPtr ptr;
            if (_windows)
            {
                ptr = NativeMethods.GetProcAddress(_handle, name);
                if (ptr == IntPtr.Zero && failOnError)
                    throw new NativeLibraryException(ErrorFunctionNotFound + " " + Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()).Message);
            }
            else
            {
                ptr = NativeMethods.dlsym(_handle, name);
                if (ptr == IntPtr.Zero && failOnError)
                    throw new NativeLibraryException(ErrorFunctionNotFound + " " + Marshal.PtrToStringAnsi(NativeMethods.dlerror()));
            }

            return ptr;
        }

        private T GetDelegate<T>(string name, bool failOnError)
            where T : class
        {
            Type type = typeof(T);
            if (!type.IsSubclassOf(typeof(Delegate)))
                throw new InvalidOperationException("Supplied type must be a delegate type.");

            IntPtr ptr;
            ptr = GetFunctionPointer(name, failOnError);
            if (ptr == IntPtr.Zero)
                return null;

            T method = Marshal.GetDelegateForFunctionPointer(ptr, type) as T;
            if (method == null)
                throw new NativeLibraryException("Unable to get a delegate for a function pointer.");

            return method;
        }

        /// <summary>
        /// Unloads the library.
        /// </summary>
        /// <remarks>
        /// Warning: Delegates and function pointers returned returned by <see cref="NativeLibrary"/> methods MUST NOT be used after the library has been unloaded.
        /// Doing so will crash the application.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_windows)
                NativeMethods.FreeLibrary(_handle);
            else
                NativeMethods.dlclose(_handle);

            //GC.SuppressFinalize(this);
        }

        //~NativeLibrary()
        //{
        // Finalizer is empty by design. We don't want the garbage collector to unload any libraries that might still be needed by function pointers and delegates.
        // We only want to unload the library when the Dispose method is called explicitly.
        //}
    }

    /// <summary>
    /// Represents an error that occured while loading an unmanaged library or getting a function pointer or delegate.
    /// </summary>
    [Serializable]
    public class NativeLibraryException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="NativeLibraryException"/>.
        /// </summary>
        public NativeLibraryException()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="NativeLibraryException"/> with the specified error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public NativeLibraryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="NativeLibraryException"/> with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        public NativeLibraryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LegacyWrapperException"/> with the specified serialization info and streaming context.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected NativeLibraryException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Specifies options for <see cref="NativeLibrary.Load(name, NativeLibraryLoadOptions)"/>
    /// and <see cref="NativeLibrary.TryLoad(name, NativeLibraryLoadOptions, NativeLibrary)"/>
    /// </summary>
    [Flags]
    public enum NativeLibraryLoadOptions
    {
        /// <summary>
        /// No options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Look for a library with a suitable name in the assembly directory.
        /// </summary>
        SearchAssemblyDirectory = 1,

        /// <summary>
        /// Look for a library with a suitable name in the executable directory.
        /// </summary>
        SearchExecutableDirectory = 2,

        /// <summary>
        /// Look for a library with a suitable name in the current directory.
        /// </summary>
        SearchCurrentDirectory = 4,

        /// <summary>
        /// Look for a library with a suitable name in the assembly directory, executable directory, current directory and system directories.
        /// </summary>
        SearchAll = SearchAssemblyDirectory | SearchExecutableDirectory | SearchCurrentDirectory,
    }

    /// <summary>
    /// Native methods for dynamic library loading.
    /// </summary>
    internal static class NativeMethods
    {
        #region Windows

        //HMODULE LoadLibrary(LPCTSTR lpFileName)
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        //FARPROC GetProcAddress(HMODULE hModule, LPCSTR lpProcName)
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        //BOOL FreeLibrary(HMODULE hModule);
        [DllImport("kernel32", SetLastError = true)]
        public static extern int FreeLibrary(IntPtr hModule);

        #endregion

        #region Linux, *BSD, Mac OS X, Solaris, etc.

        // resolve all undefined symbols in the library
        public const int RTLD_NOW = 2;

        // make symbols defined by this library available for symbol resolution of subsequently loaded libraries
        public const int RTLD_GLOBAL = 0x100;

        //void *dlopen(const char *filename, int flag)
        [DllImport("libdl")]
        public static extern IntPtr dlopen(string filename, int flag);

        //char *dlerror(void)
        [DllImport("libdl")]
        public static extern IntPtr dlerror();

        //void *dlsym(void *handle, const char *symbol)
        [DllImport("libdl")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        //int dlclose(void *handle)
        [DllImport("libdl")]
        public static extern int dlclose(IntPtr handle);

        #endregion
    }

}
