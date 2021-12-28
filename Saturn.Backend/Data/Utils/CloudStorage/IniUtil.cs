using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Saturn.Backend.Data.Utils.CloudStorage
{
    internal class IniUtil
    {
        private readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        private readonly string Path;

        public IniUtil(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default,
            StringBuilder RetVal, int Size, string FilePath);

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(9999);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 9999, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}