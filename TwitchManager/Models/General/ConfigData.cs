using System.Text.Json.Serialization;

namespace TwitchManager.Models.General
{
    public enum ConfigType
    {
        StandAlone,
        Web
    }

    public class ConfigData
    {
        string _dbConnectionString;
        public string DbConnectionString {
            get
            {
                if(string.IsNullOrEmpty(_dbConnectionString))
                {
                    return "";
                }
                else if(_dbConnectionString.Contains("Data Source=") && ConfigType == General.ConfigType.Web)
                {
                    return _dbConnectionString.Replace("Data Source=", "").Replace(@"\\", @"\");
                }
                else
                {
                    return _dbConnectionString;
                }
            }
            set => _dbConnectionString = value;
        }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TokenUrl { get; set; }

        public string BaseUrl { get; set; }

        public string Token { get; set; }

        public DateTime? TokenExpiration { get; set; }

        public string ClipDownloadPath { get; set; }

        public ConfigType? ConfigType { get; set; }

        public List<string> AdminUsers { get; set; } = [];

        [JsonIgnore]
        public string FilePath { 
            get
            {
                if(string.IsNullOrEmpty(DbConnectionString))
                {
                    return "";
                }
                else if(DbConnectionString.Contains("Data Source="))
                {
                    return DbConnectionString.Replace("Data Source=", "").Replace(@"\\", @"\");
                }
                else
                {
                    return DbConnectionString;
                }
            }
            set
            {
                if(ConfigType == General.ConfigType.StandAlone)
                {
                    DbConnectionString = $"Data Source={value}";
                }
            }
        }

        public bool IsConfigured() =>
            ((ConfigType == General.ConfigType.StandAlone && !string.IsNullOrEmpty(FilePath)) || ConfigType == General.ConfigType.Web) 
            && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(TokenUrl) && !string.IsNullOrEmpty(BaseUrl)
            && !string.IsNullOrEmpty(ClipDownloadPath) && ConfigType.HasValue;
    }
}
