using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MAST
{
    static class Program
    {
        public static PDSLibrary.Utilities.RegistryInformation registryInfo = new PDSLibrary.Utilities.RegistryInformation();
        public static int MajorVersion = 2;
        public static int MinorVersion = 17;
        public static int InternalVersion = 0;
        public static string Version = "";
        public static bool demonstrationVersion = false;
        public static string VersionDate = "26 June 2015";
        public static string ApplicationDirectory = string.Empty;
        public static PDSLibrary.Globals.Architecture architecture = PDSLibrary.Globals.Architecture.x32;
        public static string ApplicationPath = "";
        public static bool useVerboseMode = false;

        public static string SolverLocation
        {
            get
            {
                return System.IO.Path.Combine(Program.registryInfo.GetData("ProteusDSDirectory"), "ProteusDS.exe");
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            PDSLibrary.Utilities.LocalizedParser.ForceUSLocale(true);
            PDSLibrary.Program.ApplicationDirectory = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\') + 1);
            ApplicationDirectory = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\') + 1);

            Program.GetRegistryInfo();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

            Application.Run(new Presentation.MainForm());

            PDSLibrary.Utilities.LocalizedParser.RestoreNativeLocale(true);
        }

        public static void GetRegistryInfo()
        {
            Program.registryInfo.StoreData(PDSLibrary.Utilities.RegistryType.LocalMachine, PDSLibrary.Globals.Constants.RegistryPath, "WorkingDirectory");
            Program.registryInfo.StoreData(PDSLibrary.Utilities.RegistryType.LocalMachine, PDSLibrary.Globals.Constants.RegistryPath, "InstallationDirectory");
            Program.registryInfo.StoreData(PDSLibrary.Utilities.RegistryType.LocalMachine, PDSLibrary.Globals.Constants.RegistryPath, "LicenseDirectory");
            Program.registryInfo.StoreData(PDSLibrary.Utilities.RegistryType.LocalMachine, PDSLibrary.Globals.Constants.RegistryPath, "ProteusDSDirectory");
        }
    }
}
