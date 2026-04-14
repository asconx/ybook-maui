using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace yBook.Models.Api
{
    /// <summary>
    /// JsonConverter that accepts boolean values represented as true/false, numeric (0/1) or string ("0","1","true","false").
    /// Used to tolerate inconsistent API typing.
    /// </summary>
    public class BoolIntJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var l))
                        return l != 0;
                    if (reader.TryGetDouble(out var d))
                        return Math.Abs(d) > double.Epsilon;
                    return false;
                case JsonTokenType.String:
                    var s = reader.GetString();
                    if (string.IsNullOrEmpty(s)) return false;
                    if (long.TryParse(s, out var li)) return li != 0;
                    if (bool.TryParse(s, out var b)) return b;
                    return false;
                case JsonTokenType.Null:
                    return false;
                default:
                    return false;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
