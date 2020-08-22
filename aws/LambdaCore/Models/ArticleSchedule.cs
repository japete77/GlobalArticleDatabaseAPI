using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace LambdaCore.Models
{
    public class ArticleSchedule
    {
        [JsonProperty("Mes")]
        public string Month { get; set; }

        [JsonProperty("Semana")]
        public string Week { get; set; }

        [JsonProperty("Fecha")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonProperty("Articulo")]
        public string Article { get; set; }
    }

    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            base.DateTimeFormat = "dd/MM/yyyy";
        }
    }
}
