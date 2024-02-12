using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Abstractions
{
    public class PaginatedResponseModel<T> : ResponseModel<T>
    {
        [JsonPropertyName("pagination")]
        public PaginationDataModel Pagination { get; set; }
    }
}
