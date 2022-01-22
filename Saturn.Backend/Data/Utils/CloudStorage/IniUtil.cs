using IniParser;
using IniParser.Model;
using System.Reflection;

namespace Saturn.Backend.Data.Utils.CloudStorage
{
    public class IniUtil
    {

        private readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        private readonly IniData _data;

        public IniUtil(string IniPath = null)
        {
            var parser = new FileIniDataParser();
            _data = parser.ReadFile(IniPath ?? EXE + ".ini");
        }

        public string Read(string Key, string Section = null)
            => _data[Section][Key];

        public SectionDataCollection GetSections()
            => _data.Sections;

    }
}