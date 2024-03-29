﻿using System;
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

namespace RestWrapper
{
    /// <summary>
    /// RESTful response from the server.
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
        /// The number of bytes contained in the response body byte array.
        /// </summary>
        public long ContentLength { get; internal set; } = 0;

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
        public Stream Data { get; internal set; } = null;

        /// <summary>
        /// Read the data stream fully into a byte array.
        /// If you use this property, the 'Data' property will be fully read.
        /// </summary>
        [JsonIgnore]
        public byte[] DataAsBytes
        {
            get
            {
                if (_Data == null && ContentLength > 0 && Data != null && Data.CanRead)
                {
                    _Data = StreamToBytes(Data);
                }

                return _Data;
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
                if (_Data == null && ContentLength > 0 && Data != null && Data.CanRead)
                {
                    _Data = StreamToBytes(Data);
                }

                if (_Data != null)
                {
                    return Encoding.UTF8.GetString(_Data);
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
                return _SerializationHelper;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SerializationHelper));
                _SerializationHelper = value;
            }
        }

        #endregion

        #region Private-Members

        private byte[] _Data = null;
        private ISerializationHelper _SerializationHelper = new DefaultSerializationHelper();
        private NameValueCollection _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        private bool _DisposedValue = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// An organized object containing frequently used response parameters from a RESTful HTTP request.
        /// </summary>
        public RestResponse()
        {
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
                _SerializationHelper = null;
                _Data = null;

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
            string ret = "";
            ret += "REST Response" + Environment.NewLine;

            if (Headers != null && Headers.Count > 0)
            {
                ret += "  Headers" + Environment.NewLine;
                for (int i = 0; i < Headers.Count; i++)
                {
                    ret += "  | " + Headers.GetKey(i) + ": " + Headers.Get(i) + Environment.NewLine;
                }
            }

            if (!String.IsNullOrEmpty(ContentEncoding))
                ret += "  Content Encoding   : " + ContentEncoding + Environment.NewLine;
            if (!String.IsNullOrEmpty(ContentType))
                ret += "  Content Type       : " + ContentType + Environment.NewLine;

            ret += "  Status Code        : " + StatusCode + Environment.NewLine;
            ret += "  Status Description : " + StatusDescription + Environment.NewLine;
            ret += "  Content Length     : " + ContentLength + Environment.NewLine;
            ret += "  Time" + Environment.NewLine;
            ret += "  | Start (UTC)      : " + Time.Start.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;

            if (Time.End != null)
            {
                ret += "  | End (UTC)        : " + Time.End.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                ret += "  | Total            : " + Time.TotalMs + "ms" + Environment.NewLine;
            }

            ret += "  Data               : ";
            if (Data != null && ContentLength > 0) ret += "[stream]";
            else ret += "[none]";
            ret += Environment.NewLine;

            return ret;
        }

        /// <summary>
        /// Deserialize JSON data to an object type of your choosing.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Instance.</returns>
        public T DataFromJson<T>() where T : class, new()
        {
            if (String.IsNullOrEmpty(DataAsString)) throw new InvalidOperationException("No data in the REST response.");
            return _SerializationHelper.DeserializeJson<T>(DataAsString);
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

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
