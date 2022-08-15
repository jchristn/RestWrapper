using System;
using System.Collections.Generic;
using System.Text;

namespace RestWrapper
{
    /// <summary>
    /// Serialization helper.
    /// </summary>
    public interface ISerializationHelper
    {
        /// <summary>
        /// Deserialize from JSON to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Instance.</returns>
        T DeserializeJson<T>(string json);
    }
}
