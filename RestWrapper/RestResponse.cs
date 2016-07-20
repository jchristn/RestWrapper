using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWrapper
{
    public class RestResponse
    {
        #region Constructor

        /// <summary>
        /// An organized object containing frequently used response parameters from a RESTful HTTP request.
        /// </summary>
        public RestResponse()
        {
        }

        #endregion

        #region Public-Members

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
        public string StatusDescripion;

        /// <summary>
        /// The response data returned from the server.
        /// </summary>
        public byte[] Data;

        #endregion

        #region Private-Members

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
            ret += "  Status Description : " + StatusDescripion + Environment.NewLine;
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

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
