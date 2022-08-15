using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RestWrapper
{
    /// <summary>
    /// Default serialization helper.
    /// </summary>
    public class DefaultSerializationHelper : ISerializationHelper
    {
        /// <summary>
        /// Deserialize JSON to an instance.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Instance.</returns>
        public T DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
