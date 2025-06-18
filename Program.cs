using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TRPGLogArrangeTool
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // DLL探索パス設定
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLibFolder;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        private static Assembly ResolveFromLibFolder(object sender, ResolveEventArgs args)
        {
            string libPath = Path.Combine(AppContext.BaseDirectory, "lib");
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";
            string fullPath = Path.Combine(libPath, assemblyName);

            if (File.Exists(fullPath))
            {
                return Assembly.LoadFrom(fullPath);
            }

            return null;
        }
    }
}
