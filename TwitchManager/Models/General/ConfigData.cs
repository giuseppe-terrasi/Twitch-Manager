using System.Text.Json.Serialization;

namespace TwitchManager.Models.General
{
    public class ConfigData
    {
        public string DbConnectionString { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TokenUrl { get; set; }

        public string BaseUrl { get; set; }

        public string Token { get; set; }

        public DateTime? TokenExpiration { get; set; }

        public string ClipDownloadPath { get; set; }

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
                    return DbConnectionString.Replace("Data Source=", "");
                }
                else
                {
                    return DbConnectionString;
                }
            }
            set
            {
                DbConnectionString = $"Data Source={value}";
            }
        }

        public bool IsConfigured() =>
            !string.IsNullOrEmpty(DbConnectionString) && !string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(TokenUrl) && !string.IsNullOrEmpty(BaseUrl)
            && !string.IsNullOrEmpty(ClipDownloadPath);
    }
}
