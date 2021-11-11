using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace EzBzDownloader.Client.Logic
{
    public sealed class BzDateConverter : JsonConverter
    {
        private static readonly Regex DateRegex = new(@"^\d(?<year>\d{4})(?<month>\d\d)(?<day>\d\d)_m(?<hours>\d\d)(?<minutes>\d\d)(?<seconds>\d\d)$", RegexOptions.Compiled);

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotSupportedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string value)
                return null;

            var match = DateRegex.Match(value);
            if (!match.Success)
                return null;

            var year = int.Parse(match.Groups["year"].Value);
            var month = int.Parse(match.Groups["month"].Value);
            var day = int.Parse(match.Groups["day"].Value);
            var hours = int.Parse(match.Groups["hours"].Value);
            var minutes = int.Parse(match.Groups["minutes"].Value);
            var seconds = int.Parse(match.Groups["seconds"].Value);

            return new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc);
        }
    }
}