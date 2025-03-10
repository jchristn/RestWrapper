namespace RestWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Content-type header parser.
    /// </summary>
    public static class ContentTypeParser
    {
        /// <summary>
        /// Parses a Content-Type header and extracts the media type.
        /// </summary>
        /// <param name="contentTypeHeader">The Content-Type header value (e.g., "text/html; charset=utf-8").</param>
        /// <returns>The media type portion of the Content-Type header (e.g., "text/html").</returns>
        public static string ExtractMediaType(string contentTypeHeader)
        {
            if (string.IsNullOrWhiteSpace(contentTypeHeader)) return null;
            string[] parts = contentTypeHeader.Split(';');
            return parts[0].Trim();
        }

        /// <summary>
        /// Extracts the character set from a Content-Type header value.
        /// </summary>
        /// <param name="contentTypeHeader">The Content-Type header value (e.g., "text/html; charset=utf-8").</param>
        /// <returns>The character set or null if not specified (e.g., "charset=utf-8").</returns>
        public static string ExtractCharset(string contentTypeHeader)
        {
            Dictionary<string, string> parameters = ExtractParameters(contentTypeHeader);
            if (parameters.TryGetValue("charset", out string charset)) return charset;
            return null;
        }

        /// <summary>
        /// Extracts the boundary parameter from a Content-Type header value.
        /// </summary>
        /// <param name="contentTypeHeader">The Content-Type header value.</param>
        /// <returns>The boundary value or null if not specified.</returns>
        public static string ExtractBoundary(string contentTypeHeader)
        {
            Dictionary<string, string> parameters = ExtractParameters(contentTypeHeader);
            if (parameters.TryGetValue("boundary", out string boundary)) return boundary;
            return null;
        }

        /// <summary>
        /// Extracts all parameters from a Content-Type header value.
        /// </summary>
        /// <param name="contentTypeHeader">The Content-Type header value.</param>
        /// <returns>A dictionary containing all parameters.</returns>
        public static Dictionary<string, string> ExtractParameters(string contentTypeHeader)
        {
            if (string.IsNullOrWhiteSpace(contentTypeHeader)) return new Dictionary<string, string>();

            Dictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] parts = contentTypeHeader.Split(';');

            // Parse all parts after the media type as parameters
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i].Trim();

                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string key = part.Substring(0, equalsIndex).Trim();
                    string value = part.Substring(equalsIndex + 1).Trim();

                    if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    parameters[key] = value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Extracts a specific parameter from a Content-Type header.
        /// </summary>
        /// <param name="contentTypeHeader">The Content-Type header value.</param>
        /// <param name="parameterName">The name of the parameter to extract.</param>
        /// <returns>The parameter value or null if not found.</returns>
        public static string ExtractParameter(string contentTypeHeader, string parameterName)
        {
            Dictionary<string, string> parameters = ExtractParameters(contentTypeHeader);
            if (parameters.TryGetValue(parameterName, out string value)) return value;
            return null;
        }
    }
}
