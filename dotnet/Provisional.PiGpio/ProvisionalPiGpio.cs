using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Swan.DependencyInjection;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO.Abstractions;

namespace Provisional.PiGpio
{
    public class ProvisionalPiGpio : IBootstrap
    {
        private static readonly object SyncLock = new object();

        /// <inheritdoc />
        public void Bootstrap()
        {
            lock (SyncLock)
            {
                EmbeddedResources.ExtractAll();
                Setup.GpioInitialise();

                DependencyContainer.Current.Register<IGpioController>(new GpioController());
                DependencyContainer.Current.Register<ISpiBus>(new SpiBus());
                DependencyContainer.Current.Register<II2CBus>(new I2CBus());
                DependencyContainer.Current.Register<ISystemInfo>(new SystemInfo());
                DependencyContainer.Current.Register<ITiming>(new Timing());
                DependencyContainer.Current.Register<IThreading>(new Threading());
            }
        }
    }
    
    internal static class EmbeddedResources
    {
        /// <summary>
        /// Initializes static members of the <see cref="EmbeddedResources"/> class.
        /// </summary>
        static EmbeddedResources()
        {
            ResourceNames =
                new ReadOnlyCollection<string>(typeof(Board).Assembly.GetManifestResourceNames());
        }

        /// <summary>
        /// Gets the resource names.
        /// </summary>
        /// <value>
        /// The resource names.
        /// </value>
        public static ReadOnlyCollection<string> ResourceNames { get; }

        /// <summary>
        /// Extracts all the file resources to the specified base path.
        /// </summary>
        public static void ExtractAll()
        {
            var type = typeof(Board);
            var basePath = Path.GetDirectoryName(type.Assembly.Location);
            var executablePermissions = SysCall.StringToInteger("0777", IntPtr.Zero, 8);

            foreach (var resourceName in ResourceNames)
            {
                Console.WriteLine($"Found {resourceName}");
                var filename = resourceName.Substring($"{type.Namespace}.Resources.".Length);
                var targetPath = Path.Combine(basePath, filename);
                if (File.Exists(targetPath)) return;

                using var stream = type.Assembly
                    .GetManifestResourceStream($"{type.Namespace}.Resources.{filename}");
                using (var outputStream = File.OpenWrite(targetPath))
                {
                    stream?.CopyTo(outputStream);
                }

                try
                {
                    SysCall.Chmod(targetPath, (uint)executablePermissions);
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
    } 
    
    internal static class SysCall
    {
        internal const string LibCLibrary = "libc";

        [DllImport(LibCLibrary, EntryPoint = "chmod", SetLastError = true)]
        public static extern int Chmod(string filename, uint mode);

        [DllImport(LibCLibrary, EntryPoint = "strtol", SetLastError = true)]
        public static extern int StringToInteger(string numberString, IntPtr endPointer, int numberBase);

        [DllImport(LibCLibrary, EntryPoint = "write", SetLastError = true)]
        public static extern int Write(int fd, byte[] buffer, int count);
    }
}