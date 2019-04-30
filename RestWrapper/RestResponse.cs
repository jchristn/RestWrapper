using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWrapper
{
    /// <summary>
    /// RESTful response from the server.
    /// </summary>
    public class RestResponse
    { 
        #region Public-Members

        /// <summary>
        /// The protocol and version.
        /// </summary>
        public string ProtocolVersion;

        /// <summary>
        /// User-supplied headers.
        /// </summary>
        public Dictionary<string, string> Headers;

        /// <summary>
        /// The content encoding returned from the server.
        /// </summary>
        public string ContentEncoding;

        /// <summary>
        /// The content type returned from the server.
        /// </summary>
        public string ContentType;

        /// <summary>
        /// The number of bytes contained in the response body byte array.
        /// </summary>
        public long ContentLength;

        /// <summary>
        /// The response URI of the responder.
        /// </summary>
        public string ResponseURI;
        
        /// <summary>
        /// The HTTP status code returned with the response.
        /// </summary>
        public int StatusCode;

        /// <summary>
        /// The HTTP status description associated with the HTTP status code.
        /// </summary>
        public string StatusDescription;

        /// <summary>
        /// The response data returned from the server.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// The stream containing the response data returned from the server.
        /// </summary>
        public Stream DataStream;

        #endregion

        #region Private-Members

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

        public override string ToString()
        {
            string ret = "";
            ret += "REST Response" + Environment.NewLine;

            if (Headers != null && Headers.Count > 0)
            {
                ret += "  Headers" + Environment.NewLine;
                foreach (KeyValuePair<string, string> curr in Headers)
                {
                    ret += "    " + curr.Key + ": " + curr.Value + Environment.NewLine;
                }
            }

            if (!String.IsNullOrEmpty(ContentEncoding))
                ret += "  Content Encoding   : " + ContentEncoding + Environment.NewLine;
            if (!String.IsNullOrEmpty(ContentType))
                ret += "  Content Type       : " + ContentType + Environment.NewLine;
            if (!String.IsNullOrEmpty(ResponseURI))
                ret += "  Response URI       : " + ResponseURI + Environment.NewLine;

            ret += "  Status Code        : " + StatusCode + Environment.NewLine;
            ret += "  Status Description : " + StatusDescription + Environment.NewLine;
            ret += "  Content Length     : " + ContentLength + Environment.NewLine;

            if (Data != null && Data.Length > 0)
            {
                ret += "  Data" + Environment.NewLine;
                ret += Encoding.UTF8.GetString(Data) + Environment.NewLine;
            }
            else
            {
                ret += "  No Data" + Environment.NewLine;
            }

            return ret;
        }

        public byte[] ToHttpBytes()
        {
            byte[] ret = null;

            string statusLine = ProtocolVersion + " " + StatusCode + " " + StatusDescription + "\r\n";
            ret = AppendBytes(ret, Encoding.UTF8.GetBytes(statusLine));

            if (!String.IsNullOrEmpty(ContentType))
            {
                string contentTypeLine = "Content-Type: " + ContentType + "\r\n";
                ret = AppendBytes(ret, Encoding.UTF8.GetBytes(contentTypeLine));
            }

            if (Headers != null && Headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> currHeader in Headers)
                {
                    if (String.IsNullOrEmpty(currHeader.Key)) continue;
                    if (currHeader.Key.ToLower().Trim().Equals("content-type")) continue;

                    string headerLine = currHeader.Key + ": " + currHeader.Value + "\r\n";
                    ret = AppendBytes(ret, Encoding.UTF8.GetBytes(headerLine));
                }
            }

            ret = AppendBytes(ret, Encoding.UTF8.GetBytes("\r\n"));

            if (Data != null)
            {
                ret = AppendBytes(ret, (byte[])Data); 
            }

            return ret;

        }

        #endregion

        #region Private-Methods

        private byte[] AppendBytes(byte[] orig, byte[] append)
        {
            if (append == null) return orig;
            if (orig == null) return append;

            byte[] ret = new byte[orig.Length + append.Length];
            Buffer.BlockCopy(orig, 0, ret, 0, orig.Length);
            Buffer.BlockCopy(append, 0, ret, orig.Length, append.Length);
            return ret;
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
