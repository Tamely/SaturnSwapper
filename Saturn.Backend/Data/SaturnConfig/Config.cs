using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Saturn.Backend.Data.SaturnConfig.Models;
using Saturn.Backend.Data.Services.OobeServiceUtils;
using Saturn.Backend.Data.Swapper.Swapping.Models;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.SaturnConfig
{
    public class Config
    {
        public readonly ConfigModel _config;
        private static Config _instance;

        public static Config Get()
        {
            return _instance ??= new Config();
        }

        public Config()
        {
            if (!File.Exists(Constants.ConfigPath))
            {
                _config = new ConfigModel();
                File.WriteAllText(Constants.ConfigPath, JsonConvert.SerializeObject(_config));
            }
            else
            {
                _config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(Constants.ConfigPath));
            }
        }

        public void Dispose()
        {
            File.WriteAllText(Constants.ConfigPath, JsonConvert.SerializeObject(_config));
        }

        public void SetKey(string key)
        {
            _config.Key = key;
        }

        public string GetKey()
        {
            return _config.Key;
        }

        public void SetFortniteVersion(string version)
        {
            _config.FortniteVersion = version;
        }
        
        public string GetFortniteVersion()
        {
            return _config.FortniteVersion;
        }

        public OobeType GetOobeType()
        {
            return _config.OobeType;
        }
        
        public void SetOobeType(OobeType oobeType)
        {
            _config.OobeType = oobeType;
        }

        public List<PresetModel> GetPresets()
        {
            return _config.Presets;
        }

        public void AddPreset(PresetModel preset)
        {
            _config.Presets.Add(preset);
        }

        public void RemovePreset(PresetModel preset)
        {
            _config.Presets.Remove(preset);
        }

        public void ClearPresets()
        {
            _config.Presets.Clear();
        }

        public bool CheckVersion(string dependency, string version)
        {
            if (_config.DependencyVersions.ContainsKey(dependency))
                if (_config.DependencyVersions[dependency] == version)
                    return true;
                else
                {
                    _config.DependencyVersions[dependency] = version;
                    return false;
                }

            _config.DependencyVersions.Add(dependency, version);
            return false;
        }
    }
}
