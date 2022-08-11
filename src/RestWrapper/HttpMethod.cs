using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RestWrapper
{
    /// <summary>
    /// HTTP methods, i.e. GET, PUT, POST, DELETE, etc.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HttpMethod
    {
        /// <summary>
        /// HTTP GET request.
        /// </summary>
        [EnumMember(Value = "GET")]
        GET,
        /// <summary>
        /// HTTP HEAD request.
        /// </summary>
        [EnumMember(Value = "HEAD")]
        HEAD,
        /// <summary>
        /// HTTP PUT request.
        /// </summary>
        [EnumMember(Value = "PUT")]
        PUT,
        /// <summary>
        /// HTTP POST request.
        /// </summary>
        [EnumMember(Value = "POST")]
        POST,
        /// <summary>
        /// HTTP DELETE request.
        /// </summary>
        [EnumMember(Value = "DELETE")]
        DELETE,
        /// <summary>
        /// HTTP PATCH request.
        /// </summary>
        [EnumMember(Value = "PATCH")]
        PATCH,
        /// <summary>
        /// HTTP CONNECT request.
        /// </summary>
        [EnumMember(Value = "CONNECT")]
        CONNECT,
        /// <summary>
        /// HTTP OPTIONS request.
        /// </summary>
        [EnumMember(Value = "OPTIONS")]
        OPTIONS,
        /// <summary>
        /// HTTP TRACE request.
        /// </summary>
        [EnumMember(Value = "TRACE")]
        TRACE
    }
}