using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Runtime.Serialization;
using System.Windows.Threading;
using Vts.Extensions;


namespace Vts.IO
{
    /// <summary>
    /// This class includes methods for dynamically loading DLLs
    /// </summary>
    public static class LibraryIO
    {
        private static IDictionary<string, string> _loadedAssemblies;

        static LibraryIO()
        {
            _loadedAssemblies = new Dictionary<string, string>();
        }

        public static void EnsureDllIsLoaded(string assemblyName)
        {
            if (!_loadedAssemblies.ContainsKey(assemblyName))
            {
                LoadFromDLL(DLLLocation + assemblyName);
            }
        }

#if SILVERLIGHT
        // Location of the DLL
        private static string DLLLocation = "http://localhost:50789/Libraries/";
        // a lightweight object to use 
        private static AutoResetEvent _signal = new AutoResetEvent(false);
        /// <summary>
        /// Loads an assembly from a dll
        /// </summary>
        /// <param name="fileName">path name and filename of the dll</param>
        private static void LoadFromDLL(string fileName)
        {
            WebClient downloader = new WebClient();

            downloader.OpenReadCompleted += (sender1, e1) =>
            {
                AssemblyPart assemblyPart = new AssemblyPart();
                var assembly = assemblyPart.Load(e1.Result);
                //Add the current assembly to the list of assemblies
                _loadedAssemblies.Add(fileName, assembly.FullName);
                _signal.Set();
            };

            downloader.OpenReadAsync(new Uri(fileName, UriKind.Absolute));

            // wait for the async operation to complete (-1 specifies an infinte wait time)
            _signal.WaitOne(-1);
        }
#else
        // Location of the DLL
        private static string DLLLocation = "";
        /// <summary>
        /// Loads an assembly from a dll
        /// </summary>
        /// <param name="fileName">Path and name of the dll</param>
        private static void LoadFromDLL(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(DLLLocation + fileName);
            var assembly = Assembly.Load(bytes);
            _loadedAssemblies.Add(fileName, assembly.FullName);
        }
#endif
    }
}