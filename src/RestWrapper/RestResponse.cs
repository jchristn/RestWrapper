namespace RestWrapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Timestamps;
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// RESTful response from the server.
    /// Encapsulate this object in a using block.
    /// </summary>
    public class RestResponse : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Information related to the start, end, and total time for the operation.
        /// </summary>
        public Timestamp Time { get; internal set; } = new Timestamp();

        /// <summary>
        /// The protocol and version.
        /// </summary>
        public string ProtocolVersion { get; internal set; } = null;

        /// <summary>
        /// User-supplied headers.
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return _Headers;
            }
            set
            {
                if (value == null) _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
                else _Headers = value;
            }
        }

        /// <summary>
        /// The content encoding returned from the server.
        /// </summary>
        public string ContentEncoding { get; internal set; } = null;

        /// <summary>
        /// The content type returned from the server.
        /// </summary>
        public string ContentType { get; internal set; } = null;

        /// <summary>
        /// The character set used in the encoding.
        /// </summary>
        public string CharacterSet { get; internal set; } = null;

        /// <summary>
        /// The number of bytes contained in the response body byte array.
        /// </summary>
        public long? ContentLength { get; internal set; } = null;

        /// <summary>
        /// Boolean indicating if chunked transfer encoding is being used.
        /// </summary>
        public bool ChunkedTransferEncoding { get; set; } = false;

        /// <summary>
        /// Boolean indicating if server-sent events are being used.
        /// </summary>
        public bool ServerSentEvents { get; set; } = false;

        /// <summary>
        /// The HTTP status code returned with the response.
        /// </summary>
        public int StatusCode { get; internal set; } = 0;

        /// <summary>
        /// The HTTP status description associated with the HTTP status code.
        /// </summary>
        public string StatusDescription { get; internal set; } = null;

        /// <summary>
        /// The stream containing the response data returned from the server.
        /// </summary>
        [JsonIgnore]
        public Stream Data
        {
            get
            {
                if (ServerSentEvents) throw new InvalidOperationException("The REST response is configured with server-sent events.  Use ReadEventAsync() instead.");
                if (ChunkedTransferEncoding) throw new InvalidOperationException("The REST response is configured with chunked transfer encoding.  Use ReadChunkAsync() instead.");
                return _Data;
            }
            internal set
            {
                _Data = value;
            }
        }

        /// <summary>
        /// Read the data stream fully into a byte array.
        /// If you use this property, the 'Data' property will be fully read.
        /// </summary>
        [JsonIgnore]
        public byte[] DataAsBytes
        {
            get
            {
                if (ServerSentEvents) throw new InvalidOperationException("The REST response is configured with server-sent events.  Use ReadEventAsync() instead.");
                if (ChunkedTransferEncoding) throw new InvalidOperationException("The REST response is configured with chunked transfer encoding.  Use ReadChunkAsync() instead.");
                if (_DataAsBytes == null && Data != null && Data.CanRead)
                    _DataAsBytes = StreamToBytes(Data);
                return _DataAsBytes;
            }
        }

        /// <summary>
        /// Read the data stream fully into a string.
        /// If you use this property, the 'Data' property will be fully read.
        /// </summary>
        [JsonIgnore]
        public string DataAsString
        {
            get
            {
                if (ServerSentEvents) throw new InvalidOperationException("The REST response is configured with server-sent events.  Use ReadEventAsync() instead.");
                if (ChunkedTransferEncoding) throw new InvalidOperationException("The REST response is configured with chunked transfer encoding.  Use ReadChunkAsync() instead.");
                if (_DataAsBytes != null) return Encoding.UTF8.GetString(_DataAsBytes);
                if (Data == null) return null;
                if (Data.CanRead)
                {
                    _DataAsBytes = StreamToBytes(Data);
                    return Encoding.UTF8.GetString(_DataAsBytes);
                }
                return null;
            }
        }

        /// <summary>
        /// JSON serialization helper.
        /// </summary>
        [JsonIgnore]
        public ISerializationHelper SerializationHelper
        {
            get
            {
                return _Serializer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SerializationHelper));
                _Serializer = value;
            }
        }

        #endregion

        #region Private-Members

        private HttpResponseMessage _Response = null;
        private Stream _Data = null;
        private byte[] _DataAsBytes = null;
        private ISerializationHelper _Serializer = new DefaultSerializationHelper();
        private NameValueCollection _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        private bool _DisposedValue = false;

        private ServerSentEventReader _ServerSentEventReader = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// RESTful response from the server.
        /// Encapsulate this object in a using block.
        /// </summary>
        public RestResponse()
        {

        }

        /// <summary>
        /// An organized object containing frequently used response parameters from a RESTful HTTP request.
        /// </summary>
        /// <param name="resp">HTTP response message.</param>
        public RestResponse(HttpResponseMessage resp)
        {
            _Response = resp ?? throw new ArgumentNullException(nameof(resp));

            ProtocolVersion = "HTTP/" + _Response.Version.ToString();
            StatusCode = (int)_Response.StatusCode;
            StatusDescription = _Response.StatusCode.ToString();

            ChunkedTransferEncoding = _Response.Headers.TransferEncoding?.Any(x => x.Value.Equals("chunked", StringComparison.OrdinalIgnoreCase)) ?? false;
            ServerSentEvents = _Response.Content.Headers.ContentType?.MediaType == "text/event-stream";

            if (_Response.Content != null && _Response.Content.Headers != null)
            {
                ContentType = _Response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                CharacterSet = _Response.Content.Headers.ContentType?.CharSet ?? "utf-8";

                if (_Response.Content.Headers.ContentLength != null)
                    ContentLength = _Response.Content.Headers.ContentLength;

                if (_Response.Content.Headers.ContentEncoding != null)
                    ContentEncoding = string.Join(",", _Response.Content.Headers.ContentEncoding);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in _Response.Headers)
            {
                string key = header.Key;
                string val = string.Join(",", header.Value);
                Headers.Add(key, val);
            }

            if (_Response.Content != null && _Response.RequestMessage.Method != HttpMethod.Head)
            {
                _Data = _Response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (ServerSentEvents) _ServerSentEventReader = new ServerSentEventReader(_Data);
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                }

                Time = null;
                ProtocolVersion = null;
                ContentEncoding = null;
                ContentType = null;
                StatusDescription = null;
                Data = null;
                
                _Headers = null;
                _Serializer = null;
                _DataAsBytes = null;

                _Response?.Dispose();
                _ServerSentEventReader?.Dispose();
                _ChunkedDataReader?.Dispose();

                _DisposedValue = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a human-readable string of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("REST Response");

            if (Headers != null && Headers.Count > 0)
            {
                sb.AppendLine("  Headers");
                for (int i = 0; i < Headers.Count; i++)
                {
                    sb.Append("  | ")
                      .Append(Headers.GetKey(i))
                      .Append(": ")
                      .Append(Headers.Get(i))
                      .AppendLine();
                }
            }

            if (!String.IsNullOrEmpty(ContentEncoding))
                sb.Append("  Content Encoding   : ").AppendLine(ContentEncoding);

            if (!String.IsNullOrEmpty(ContentType))
                sb.Append("  Content Type       : ").AppendLine(ContentType);

            sb.Append("  Status Code        : ").AppendLine(StatusCode.ToString())
              .Append("  Status Description : ").AppendLine(StatusDescription)
              .Append("  Content Length     : ").AppendLine(ContentLength.ToString())
              .Append("  Chunked Transfer   : ").AppendLine(ChunkedTransferEncoding.ToString())
              .Append("  Server-Sent Events : ").AppendLine(ServerSentEvents.ToString())
              .AppendLine("  Time")
              .Append("  | Start (UTC)      : ").AppendLine(Time.Start.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));

            if (Time.End != null)
            {
                sb.Append("  | End (UTC)        : ").AppendLine(Time.End.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"))
                  .Append("  | Total            : ").Append(Time.TotalMs).AppendLine("ms");
            }

            sb.Append("  Data               : ");
            if (ChunkedTransferEncoding) sb.Append("[chunked]");
            else if (ServerSentEvents) sb.Append("[server-sent events]");
            else if (_Data != null && ContentLength > 0) sb.Append("[stream]");
            else sb.Append("[none]");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize JSON data to an object type of your choosing.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Instance.</returns>
        public T DataFromJson<T>()where T : class, new()
        {
            if (ServerSentEvents) throw new InvalidOperationException("The REST response is configured with server-sent events.  Use ReadEventAsync() instead.");
            if (ChunkedTransferEncoding) throw new InvalidOperationException("The REST response is configured with chunked transfer encoding.  Use ReadChunkAsync() instead.");
            if (String.IsNullOrEmpty(DataAsString)) throw new InvalidOperationException("No data in the REST response.");
            return _Serializer.DeserializeJson<T>(DataAsString);
        }

        /// <summary>
        /// Read the next server-sent event.  Only appropriate for responses where ServerSentEvents is true.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Server-sent event.</returns>
        public async Task<ServerSentEvent> ReadEventAsync(CancellationToken token = default)
        {
            if (!ServerSentEvents) throw new InvalidOperationException("The REST response is not configured with server-sent events.");
            return await _ServerSentEventReader.ReadNextEventAsync(token).ConfigureAwait(false);
        }

        private StreamReader _ChunkedDataReader = null;

        /// <summary>
        /// Read the next chunk.  Only appropriate for responses where ChunkedTransferEncoding is true.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Chunk data, or null if called after the final chunk.</returns>
        public async Task<ChunkData> ReadChunkAsync(CancellationToken token = default)
        {
            if (!ChunkedTransferEncoding) throw new InvalidOperationException("The REST response is not configured with chunked transfer encoding.");

            // Initialize the chunked data reader if not already done
            if (_ChunkedDataReader == null)
            {
                if (_Data == null) return null;
                _ChunkedDataReader = new StreamReader(_Data, System.Text.Encoding.UTF8);
            }

            // Read line by line - the chunked transfer protocol naturally creates line boundaries
            // even when the server payload doesn't include line endings
            string line = await _ChunkedDataReader.ReadLineAsync().ConfigureAwait(false);
            if (line == null) return null;

            // Don't add any line endings - return exactly what the server sent as payload
            byte[] data = System.Text.Encoding.UTF8.GetBytes(line);

            // Check if this is the final chunk by checking if more data is available
            bool isFinal = _ChunkedDataReader.EndOfStream;

            return new ChunkData
            {
                Data = data,
                IsFinal = isFinal
            };
        }

        #endregion
         
        #region Private-Methods

        private byte[] StreamToBytes(Stream input)
        {  
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                byte[] ret = ms.ToArray();
                return ret;
            }
        }

        #endregion
    }
}
