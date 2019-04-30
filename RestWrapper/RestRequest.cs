using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RestWrapper
{
    /// <summary>
    /// RESTful HTTP request to be sent to a server.
    /// </summary>
    public class RestRequest
    {
        #region Public-Members

        /// <summary>
        /// The URL to which the request should be directed.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The HTTP method to use, also known as a verb (GET, PUT, POST, DELETE, etc).
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The username to use in the authorization header, if any.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The password to use in the authorization header, if any.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Enable to encode credentials in the authorization header.
        /// </summary>
        public bool EncodeCredentials { get; set; }

        /// <summary>
        /// Ignore certificate errors such as expired certificates, self-signed certificates, or those that cannot be validated.
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; }

        /// <summary>
        /// The filename of the file containing the certificate.
        /// </summary>
        public string CertificateFilename { get; set; }
        
        /// <summary>
        /// The password to the certificate file.
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// The HTTP headers to attach to the request.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The content type of the payload (i.e. Data or DataStream).
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the payload (i.e. Data or DataStream).
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Data to send to the server.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _Data;
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    ContentLength = value.Length;
                    _Data = new byte[value.Length];
                    Buffer.BlockCopy(value, 0, Data, 0, value.Length);
                } 
                else 
                {
                    ContentLength = 0;
                    _Data = null;
                }
            }
        }

        /// <summary>
        /// The stream from which data should be read to send to the server.  If using DataStream, set ContentLength first.
        /// </summary>
        public Stream DataStream
        {
            get
            {
                return _DataStream;
            }
            set
            {
                if (ContentLength < 0)
                {
                    throw new ArgumentException("ContentLength must be set before assigning DataStream.");
                }

                if (value != null)
                {
                    _DataStream = value;
                }
            }
        }

        /// <summary>
        /// The size of the buffer to use while reading from the DataStream and the response stream from the server.
        /// </summary>
        public int StreamReadBufferSize
        {
            get
            {
                return _StreamReadBufferSize;
            }
            set
            {
                if (value < 1) throw new ArgumentException("StreamReadBufferSize must be at least one byte in size.");
                _StreamReadBufferSize = value;
            }
        }

        /// <summary>
        /// Enable console debugging.
        /// </summary>
        public bool ConsoleDebug = false;

        #endregion

        #region Private-Members

        private byte[] _Data;
        private Stream _DataStream;
        private int _StreamReadBufferSize = 65536;
        private bool _ReturnStream;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="headers">HTTP headers to use.</param>
        /// <param name="contentType">Content type to use.</param>
        /// <param name="readResponseData">Indicate if the response stream should be read fully with the resultant data returned in Data (true), or, if the response stream should be returned in DataStream (false).</param>
        public RestRequest(
            string url,
            HttpMethod method,
            Dictionary<string, string> headers,
            string contentType,
            bool readResponseData)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
            Method = method;

            EncodeCredentials = true;
            IgnoreCertificateErrors = false;
            Headers = headers;
            ContentType = contentType;

            _Data = null;
            _DataStream = null;
            _ReturnStream = readResponseData;
        }

        #endregion

        #region Public-Methods

        public RestResponse Send()
        {
            _Data = null;
            _DataStream = null;
            return SendInternal();
        }

        /// <summary>
        /// Send the HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public RestResponse Send(byte[] data)
        {
            _Data = data;
            _DataStream = null;
            _ReturnStream = false;
            return SendInternal();
        }

        /// <summary>
        /// Send the HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="stream">A stream containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public RestResponse Send(Stream stream, long contentLength)
        {
            if (contentLength < 1) throw new ArgumentException("Content length must be one byte or greater.");

            _Data = null;
            _DataStream = stream;
            ContentLength = contentLength;
            _ReturnStream = true;
            return SendInternal();
        }

        /// <summary>
        /// Send the HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public async Task<RestResponse> SendAsync(byte[] data)
        {
            _Data = data;
            _DataStream = null;
            _ReturnStream = false;
            return await SendInternalAsync();
        }

        /// <summary>
        /// Send the HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="stream">A stream containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public async Task<RestResponse> SendAsync(Stream stream, long contentLength)
        {
            if (contentLength < 1) throw new ArgumentException("Content length must be one byte or greater.");

            _Data = null;
            _DataStream = stream;
            ContentLength = contentLength;
            _ReturnStream = true;
            return await SendInternalAsync();
        }

        #endregion

        #region Private-Methods

        private byte[] StreamToBytes(Stream inStream)
        {
            if (inStream == null) return null;
            if (!inStream.CanRead) return null;

            byte[] buffer = new byte[_StreamReadBufferSize];
            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead;

                while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                return ms.ToArray();
            }
        }

        private bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private RestResponse SendInternal()
        {
            if (String.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));
            Log(Method.ToString() + " to " + Url);

            try
            {
                #region Setup-Webrequest

                Log("- Setting up HttpWebRequest");

                if (IgnoreCertificateErrors) ServicePointManager.ServerCertificateValidationCallback = Validator;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(Url);
                client.KeepAlive = false;
                client.Method = Method.ToString();
                client.AllowAutoRedirect = true;
                client.Timeout = 30000;
                client.ContentLength = 0;
                client.ContentType = ContentType;
                client.UserAgent = "RestWrapper (https://www.github.com/jchristn/RestWrapper)";

                Log("- Setup HttpWebRequest successfully");

                #endregion

                #region Add-Certificate

                if (!String.IsNullOrEmpty(CertificateFilename))
                {
                    if (!String.IsNullOrEmpty(CertificatePassword))
                    {
                        Log("- Adding certificate using filename and password");

                        X509Certificate2 cert = new X509Certificate2(CertificateFilename, CertificatePassword);
                        client.ClientCertificates.Add(cert);
                    }
                    else
                    {
                        Log("- Adding certificate using filename only");

                        X509Certificate2 cert = new X509Certificate2(CertificateFilename);
                        client.ClientCertificates.Add(cert);
                    }
                }

                #endregion

                #region Add-Headers

                if (Headers != null && Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in Headers)
                    {
                        if (String.IsNullOrEmpty(pair.Key)) continue;
                        if (String.IsNullOrEmpty(pair.Value)) continue;

                        Log("- Adding header " + pair.Key + ": " + pair.Value);

                        if (pair.Key.ToLower().Trim().Equals("accept"))
                        {
                            client.Accept = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("close"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-length"))
                        {
                            client.ContentLength = Convert.ToInt64(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-type"))
                        {
                            client.ContentType = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("date"))
                        {
                            client.Date = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("expect"))
                        {
                            client.Expect = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("host"))
                        {
                            client.Host = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("if-modified-since"))
                        {
                            client.IfModifiedSince = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("keep-alive"))
                        {
                            client.KeepAlive = Convert.ToBoolean(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("proxy-connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("referer"))
                        {
                            client.Referer = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("transfer-encoding"))
                        {
                            client.TransferEncoding = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("user-agent"))
                        {
                            client.UserAgent = pair.Value;
                        }
                        else
                        {
                            client.Headers.Add(pair.Key, pair.Value);
                        }
                    }
                }

                #endregion

                #region Add-Auth-Info

                if (!String.IsNullOrEmpty(User))
                {
                    if (EncodeCredentials)
                    {
                        Log("- Adding encoded credentials for user " + User);

                        string authInfo = User + ":" + Password;
                        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                        client.Headers.Add("Authorization", "Basic " + authInfo);
                    }
                    else
                    {
                        Log("- Adding plaintext credentials");

                        client.Headers.Add("Authorization", User);
                    }
                }

                #endregion

                #region Write-Request-Body-Data

                if (Method == HttpMethod.POST ||
                    Method == HttpMethod.PUT ||
                    Method == HttpMethod.DELETE)
                {
                    if (_Data != null && _Data.Length > 0)
                    {
                        Log("- Attaching byte data to request body (" + _Data.Length + " bytes)");

                        client.ContentLength = _Data.Length;
                        client.ContentType = ContentType;
                        Stream clientStream = client.GetRequestStream();
                        clientStream.Write(_Data, 0, _Data.Length);
                        clientStream.Close();
                    }
                    else if (_DataStream != null && ContentLength > 0)
                    {
                        Log("- Reading stream and writing to request body (" + ContentLength + " bytes)");

                        client.ContentLength = ContentLength;
                        client.ContentType = ContentType;
                        Stream clientStream = client.GetRequestStream();

                        byte[] buffer = new byte[_StreamReadBufferSize];
                        long bytesRemaining = ContentLength;
                        int bytesRead = 0;

                        while (bytesRemaining > 0)
                        {
                            if (bytesRemaining >= _StreamReadBufferSize)
                            {
                                buffer = new byte[_StreamReadBufferSize];
                                bytesRead = _DataStream.Read(buffer, 0, _StreamReadBufferSize);
                            }
                            else
                            {
                                buffer = new byte[bytesRemaining];
                                bytesRead = _DataStream.Read(buffer, 0, (int)bytesRemaining);
                            }

                            if (bytesRead > 0) clientStream.Write(buffer, 0, bytesRead);
                            bytesRemaining -= bytesRead;
                        }

                        clientStream.Close();

                        Log("- Successfully added (" + ContentLength + " bytes to request body");
                    }
                }

                #endregion

                #region Submit-Request-and-Build-Response

                Log("- Submitting web request");

                HttpWebResponse response = (HttpWebResponse)client.GetResponse();

                Log("- Response received");

                RestResponse ret = new RestResponse();
                ret.ProtocolVersion = "HTTP/" + response.ProtocolVersion.ToString();
                ret.ContentEncoding = response.ContentEncoding;
                ret.ContentType = response.ContentType;
                ret.ContentLength = response.ContentLength;
                ret.ResponseURI = response.ResponseUri.ToString();
                ret.StatusCode = (int)response.StatusCode;
                ret.StatusDescription = response.StatusDescription;

                #endregion

                #region Headers

                Log("- Processing headers from response");

                if (response.Headers != null && response.Headers.Count > 0)
                {
                    ret.Headers = new Dictionary<string, string>();

                    for (int i = 0; i < response.Headers.Count; i++)
                    {
                        string key = response.Headers.GetKey(i);
                        string val = "";
                        int valCount = 0;
                        foreach (string value in response.Headers.GetValues(i))
                        {
                            if (valCount == 0)
                            {
                                val += value;
                                valCount++;
                            }
                            else
                            {
                                val += "," + value;
                                valCount++;
                            }
                        }

                        Log("- Adding header " + key + ": " + val);
                        ret.Headers.Add(key, val);
                    }
                }

                #endregion

                #region Payload

                if (_ReturnStream && response.ContentLength > 0)
                {
                    Log("- Attaching response stream to response");

                    ret.Data = null;
                    ret.ContentLength = response.ContentLength;
                    ret.DataStream = response.GetResponseStream();
                }
                else
                {
                    Log("- Reading response stream to byte array");

                    ret.DataStream = null;
                    Stream responseStream = response.GetResponseStream();

                    if (responseStream != null)
                    {
                        if (responseStream.CanRead)
                        {
                            ret.Data = StreamToBytes(responseStream);
                            ret.ContentLength = ret.Data.Length;
                            responseStream.Close();

                            Log("- Attached " + ret.ContentLength + " bytes to response");
                        }
                        else
                        {
                            ret.Data = null;
                            ret.ContentLength = 0;
                        }
                    }
                    else
                    {
                        ret.Data = null;
                        ret.ContentLength = 0;
                    }
                }

                #endregion

                return ret;
            }
            catch (WebException we)
            {
                #region WebException

                Log("- Web exception encountered: " + we.Message);

                RestResponse ret = new RestResponse();
                ret.Headers = null;
                ret.ContentEncoding = null;
                ret.ContentType = null;
                ret.ContentLength = 0;
                ret.ResponseURI = null;
                ret.StatusCode = 0;
                ret.StatusDescription = null;
                ret.Data = null;

                HttpWebResponse exceptionResponse = we.Response as HttpWebResponse;
                if (exceptionResponse != null)
                {
                    ret.ProtocolVersion = "HTTP/" + exceptionResponse.ProtocolVersion.ToString();
                    ret.ContentEncoding = exceptionResponse.ContentEncoding;
                    ret.ContentType = exceptionResponse.ContentType;
                    ret.ContentLength = exceptionResponse.ContentLength;
                    ret.ResponseURI = exceptionResponse.ResponseUri.ToString();
                    ret.StatusCode = (int)exceptionResponse.StatusCode;
                    ret.StatusDescription = exceptionResponse.StatusDescription;

                    Log("- Server returned status code " + ret.StatusCode);

                    if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                    {
                        ret.Headers = new Dictionary<string, string>();
                        for (int i = 0; i < exceptionResponse.Headers.Count; i++)
                        {
                            string key = exceptionResponse.Headers.GetKey(i);
                            string val = "";
                            int valCount = 0;

                            foreach (string value in exceptionResponse.Headers.GetValues(i))
                            {
                                if (valCount == 0)
                                {
                                    val += value;
                                    valCount++;
                                }
                                else
                                {
                                    val += "," + value;
                                    valCount++;
                                }
                            }

                            Log("- Adding header " + key + ": " + val);
                            ret.Headers.Add(key, val);
                        }
                    }

                    if (_ReturnStream)
                    {
                        ret.Data = null;
                        ret.DataStream = exceptionResponse.GetResponseStream();
                    }
                    else
                    {
                        ret.DataStream = null;
                        ret.Data = StreamToBytes(exceptionResponse.GetResponseStream());
                    }
                }

                return ret;

                #endregion
            }
        }

        private async Task<RestResponse> SendInternalAsync()
        {
            if (String.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));
            Log(Method.ToString() + " to " + Url);

            try
            {
                #region Setup-Webrequest

                Log("- Setting up HttpWebRequest");

                if (IgnoreCertificateErrors) ServicePointManager.ServerCertificateValidationCallback = Validator;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(Url);
                client.KeepAlive = false;
                client.Method = Method.ToString();
                client.AllowAutoRedirect = true;
                client.Timeout = 30000;
                client.ContentLength = 0;
                client.ContentType = ContentType;
                client.UserAgent = "RestWrapper (https://www.github.com/jchristn/RestWrapper)";

                Log("- Setup HttpWebRequest successfully");

                #endregion

                #region Add-Certificate

                if (!String.IsNullOrEmpty(CertificateFilename))
                {
                    if (!String.IsNullOrEmpty(CertificatePassword))
                    {
                        Log("- Adding certificate using filename and password");

                        X509Certificate2 cert = new X509Certificate2(CertificateFilename, CertificatePassword);
                        client.ClientCertificates.Add(cert);
                    }
                    else
                    {
                        Log("- Adding certificate using filename only");

                        X509Certificate2 cert = new X509Certificate2(CertificateFilename);
                        client.ClientCertificates.Add(cert);
                    }
                }

                #endregion

                #region Add-Headers

                if (Headers != null && Headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in Headers)
                    {
                        if (String.IsNullOrEmpty(pair.Key)) continue;
                        if (String.IsNullOrEmpty(pair.Value)) continue;

                        Log("- Adding header " + pair.Key + ": " + pair.Value);

                        if (pair.Key.ToLower().Trim().Equals("accept"))
                        {
                            client.Accept = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("close"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-length"))
                        {
                            client.ContentLength = Convert.ToInt64(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("content-type"))
                        {
                            client.ContentType = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("date"))
                        {
                            client.Date = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("expect"))
                        {
                            client.Expect = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("host"))
                        {
                            client.Host = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("if-modified-since"))
                        {
                            client.IfModifiedSince = Convert.ToDateTime(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("keep-alive"))
                        {
                            client.KeepAlive = Convert.ToBoolean(pair.Value);
                        }
                        else if (pair.Key.ToLower().Trim().Equals("proxy-connection"))
                        {
                            // do nothing
                        }
                        else if (pair.Key.ToLower().Trim().Equals("referer"))
                        {
                            client.Referer = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("transfer-encoding"))
                        {
                            client.TransferEncoding = pair.Value;
                        }
                        else if (pair.Key.ToLower().Trim().Equals("user-agent"))
                        {
                            client.UserAgent = pair.Value;
                        }
                        else
                        {
                            client.Headers.Add(pair.Key, pair.Value);
                        }
                    }
                }

                #endregion

                #region Add-Auth-Info

                if (!String.IsNullOrEmpty(User))
                {
                    if (EncodeCredentials)
                    {
                        Log("- Adding encoded credentials for user " + User);

                        string authInfo = User + ":" + Password;
                        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                        client.Headers.Add("Authorization", "Basic " + authInfo);
                    }
                    else
                    {
                        Log("- Adding plaintext credentials");

                        client.Headers.Add("Authorization", User);
                    }
                }

                #endregion

                #region Write-Request-Body-Data

                if (Method == HttpMethod.POST ||
                    Method == HttpMethod.PUT ||
                    Method == HttpMethod.DELETE)
                {
                    if (_Data != null && _Data.Length > 0)
                    {
                        Log("- Attaching byte data to request body (" + _Data.Length + " bytes)");

                        client.ContentLength = _Data.Length;
                        client.ContentType = ContentType;
                        Stream clientStream = client.GetRequestStream();
                        clientStream.Write(_Data, 0, _Data.Length);
                        clientStream.Close();
                    }
                    else if (_DataStream != null && ContentLength > 0)
                    {
                        Log("- Reading stream and writing to request body (" + ContentLength + " bytes)");

                        client.ContentLength = ContentLength;
                        client.ContentType = ContentType;
                        Stream clientStream = client.GetRequestStream();

                        byte[] buffer = new byte[_StreamReadBufferSize];
                        long bytesRemaining = ContentLength;
                        int bytesRead = 0;

                        while (bytesRemaining > 0)
                        {
                            if (bytesRemaining >= _StreamReadBufferSize)
                            {
                                buffer = new byte[_StreamReadBufferSize];
                                bytesRead = await _DataStream.ReadAsync(buffer, 0, _StreamReadBufferSize);
                            }
                            else
                            {
                                buffer = new byte[bytesRemaining];
                                bytesRead = await _DataStream.ReadAsync(buffer, 0, (int)bytesRemaining);
                            }

                            if (bytesRead > 0) clientStream.Write(buffer, 0, bytesRead);
                            bytesRemaining -= bytesRead;
                        }

                        clientStream.Close();

                        Log("- Successfully added (" + ContentLength + " bytes to request body");
                    }
                }

                #endregion

                #region Submit-Request-and-Build-Response

                Log("- Submitting web request");

                HttpWebResponse response = (HttpWebResponse)(await client.GetResponseAsync());

                Log("- Response received");

                RestResponse ret = new RestResponse();
                ret.ProtocolVersion = "HTTP/" + response.ProtocolVersion.ToString();
                ret.ContentEncoding = response.ContentEncoding;
                ret.ContentType = response.ContentType;
                ret.ContentLength = response.ContentLength;
                ret.ResponseURI = response.ResponseUri.ToString();
                ret.StatusCode = (int)response.StatusCode;
                ret.StatusDescription = response.StatusDescription;

                #endregion

                #region Headers

                Log("- Processing headers from response");

                if (response.Headers != null && response.Headers.Count > 0)
                {
                    ret.Headers = new Dictionary<string, string>();

                    for (int i = 0; i < response.Headers.Count; i++)
                    {
                        string key = response.Headers.GetKey(i);
                        string val = "";
                        int valCount = 0;
                        foreach (string value in response.Headers.GetValues(i))
                        {
                            if (valCount == 0)
                            {
                                val += value;
                                valCount++;
                            }
                            else
                            {
                                val += "," + value;
                                valCount++;
                            }
                        }

                        Log("- Adding header " + key + ": " + val);
                        ret.Headers.Add(key, val);
                    }
                }

                #endregion

                #region Payload

                if (_ReturnStream && response.ContentLength > 0)
                {
                    Log("- Attaching response stream to response");

                    ret.Data = null;
                    ret.ContentLength = response.ContentLength;
                    ret.DataStream = response.GetResponseStream();
                }
                else
                {
                    Log("- Reading response stream to byte array");

                    ret.DataStream = null;
                    Stream responseStream = response.GetResponseStream();

                    if (responseStream != null)
                    {
                        if (responseStream.CanRead)
                        {
                            ret.Data = StreamToBytes(responseStream);
                            ret.ContentLength = ret.Data.Length;
                            responseStream.Close();

                            Log("- Attached " + ret.ContentLength + " bytes to response");
                        }
                        else
                        {
                            ret.Data = null;
                            ret.ContentLength = 0;
                        }
                    }
                    else
                    {
                        ret.Data = null;
                        ret.ContentLength = 0;
                    }
                }

                #endregion

                return ret;
            }
            catch (WebException we)
            {
                #region WebException

                Log("- Web exception encountered: " + we.Message);

                RestResponse ret = new RestResponse();
                ret.Headers = null;
                ret.ContentEncoding = null;
                ret.ContentType = null;
                ret.ContentLength = 0;
                ret.ResponseURI = null;
                ret.StatusCode = 0;
                ret.StatusDescription = null;
                ret.Data = null;

                HttpWebResponse exceptionResponse = we.Response as HttpWebResponse;
                if (exceptionResponse != null)
                {
                    ret.ProtocolVersion = "HTTP/" + exceptionResponse.ProtocolVersion.ToString();
                    ret.ContentEncoding = exceptionResponse.ContentEncoding;
                    ret.ContentType = exceptionResponse.ContentType;
                    ret.ContentLength = exceptionResponse.ContentLength;
                    ret.ResponseURI = exceptionResponse.ResponseUri.ToString();
                    ret.StatusCode = (int)exceptionResponse.StatusCode;
                    ret.StatusDescription = exceptionResponse.StatusDescription;

                    Log("- Server returned status code " + ret.StatusCode);

                    if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                    {
                        ret.Headers = new Dictionary<string, string>();
                        for (int i = 0; i < exceptionResponse.Headers.Count; i++)
                        {
                            string key = exceptionResponse.Headers.GetKey(i);
                            string val = "";
                            int valCount = 0;

                            foreach (string value in exceptionResponse.Headers.GetValues(i))
                            {
                                if (valCount == 0)
                                {
                                    val += value;
                                    valCount++;
                                }
                                else
                                {
                                    val += "," + value;
                                    valCount++;
                                }
                            }

                            Log("- Adding header " + key + ": " + val);
                            ret.Headers.Add(key, val);
                        }
                    }

                    if (_ReturnStream)
                    {
                        ret.Data = null;
                        ret.DataStream = exceptionResponse.GetResponseStream();
                    }
                    else
                    {
                        ret.DataStream = null;
                        ret.Data = StreamToBytes(exceptionResponse.GetResponseStream());
                    }
                }

                return ret;

                #endregion
            }
        }

        private void Log(string msg)
        {
            if (ConsoleDebug) Console.WriteLine(msg);
        }

        #endregion
    }
}
