namespace RestWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Default serialization helper.
    /// </summary>
    public class DefaultSerializationHelper : ISerializationHelper
    {
        #region Public-Members

        /// <summary>
        /// Enable pretty print.
        /// </summary>
        public static bool Pretty { get; set; } = true;

        /// <summary>
        /// Ignore null properties when serializing.
        /// </summary>
        public static bool IgnoreNull { get; set; } = true;

        /// <summary>
        /// Serializer options.
        /// </summary>
        public static JsonSerializerOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Options));
                _Options = value;
            }
        }

        #endregion

        #region Private-Members

        private static JsonSerializerOptions _Options = new JsonSerializerOptions();

        #endregion

        #region Constructors-and-Factories

        #endregion

        #region Public-Methods

        /// <summary>
        /// Deserialize JSON to an instance.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Instance.</returns>
        public T DeserializeJson<T>(string json) where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(json, GetOptions(Pretty));
        }

        /// <summary>
        /// Serialize an object to a JSON string.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Pretty print.</param>
        /// <returns>JSON string.</returns>
        public string SerializeJson(object obj, bool pretty = true)
        {
            return JsonSerializer.Serialize(obj, GetOptions(pretty));
        }

        #endregion

        #region Private-Methods

        private static JsonSerializerOptions GetOptions(bool pretty)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(_Options);

            if (pretty) options.WriteIndented = true;
            else options.WriteIndented = false;

            if (IgnoreNull) options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            else options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

            return options;
        }

        #endregion
    }
}
