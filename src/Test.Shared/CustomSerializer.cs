namespace Test.Shared
{
    using System.Text.Json;

    using RestWrapper;

    /// <summary>
    /// Alternate serializer implementation used to verify serializer extensibility.
    /// </summary>
    public class CustomSerializer : ISerializationHelper
    {
        /// <summary>
        /// Deserialize JSON into an instance of the supplied type.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="json">JSON payload.</param>
        /// <returns>Deserialized instance.</returns>
        public T DeserializeJson<T>(string json) where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }

        /// <summary>
        /// Serialize an object into JSON.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="pretty">True to pretty-print the output.</param>
        /// <returns>Serialized JSON string.</returns>
        public string SerializeJson(object obj, bool pretty = true)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = pretty
            };

            return JsonSerializer.Serialize(obj, options);
        }
    }
}
