namespace RestWrapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text.Json.Serialization;
    using Timestamps;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    /// <summary>
    /// RESTful HTTP request to be sent to a server.
    /// Encapsulate this object in a using block.
    /// </summary>
    public class RestRequest : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Method to invoke when sending log messages.
        /// </summary>
        [JsonIgnore]
        public Action<string> Logger { get; set; } = null;

        /// <summary>
        /// UTC timestamp from the request.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = new DateTimeOffset(DateTime.UtcNow);

        /// <summary>
        /// The HTTP method to use, also known as a verb (GET, PUT, POST, DELETE, etc).
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// The URL to which the request should be directed.
        /// </summary>
        public string Url { get; set; } = null;

        /// <summary>
        /// Authorization header parameters.
        /// </summary>
        public AuthorizationHeader Authorization
        {
            get
            {
                return _Authorization;
            }
            set
            {
                if (value == null) _Authorization = new AuthorizationHeader();
                else _Authorization = value;
            }
        }

        /// <summary>
        /// Ignore certificate errors such as expired certificates, self-signed certificates, or those that cannot be validated.
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; } = false;

        /// <summary>
        /// The filename of the file containing the certificate.
        /// </summary>
        public string CertificateFilename { get; set; } = null;

        /// <summary>
        /// The password to the certificate file.
        /// </summary>
        public string CertificatePassword { get; set; } = null;

        /// <summary>
        /// The query elements attached to the URL.
        /// </summary>
        public NameValueCollection Query
        {
            get
            {
                NameValueCollection ret = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

                if (!String.IsNullOrEmpty(Url))
                {
                    if (Url.Contains("?"))
                    {
                        string query = Url.Substring(Url.IndexOf("?") + 1);

                        if (!String.IsNullOrEmpty(query))
                        {
                            string[] elements = query.Split('&');

                            if (elements != null && elements.Length > 0)
                            {
                                for (int i = 0; i < elements.Length; i++)
                                {
                                    string[] elementParts = elements[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);

                                    if (elementParts.Length == 1)
                                    {
                                        ret.Add(elementParts[0], null);
                                    }
                                    else
                                    {
                                        ret.Add(elementParts[0], elementParts[1]);
                                    }
                                }
                            }
                        }
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// The HTTP headers to attach to the request.
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
        /// The content type of the payload (i.e. Data or DataStream).
        /// </summary>
        public string ContentType { get; set; } = null;

        /// <summary>
        /// The content length of the payload (i.e. Data or DataStream).
        /// </summary>
        public long? ContentLength
        {
            get
            {
                return _ContentLength;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(ContentLength));
                _ContentLength = value;
            }
        }

        /// <summary>
        /// Boolean indicating if chunked transfer encoding is in use.
        /// </summary>
        public bool ChunkedTransfer { get; set; } = false;

        /// <summary>
        /// The size of the buffer to use while reading from the DataStream and the response stream from the server.
        /// </summary>
        public int BufferSize
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
        /// The number of milliseconds to wait before assuming the request has timed out.
        /// </summary>
        public int TimeoutMilliseconds
        {
            get
            {
                return _TimeoutMilliseconds;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Timeout must be greater than 1ms.");
                _TimeoutMilliseconds = value;
            }
        }

        /// <summary>
        /// The user agent header to set on outbound requests.
        /// </summary>
        public string UserAgent { get; set; } = "RestWrapper (https://www.github.com/jchristn/RestWrapper)";

        /// <summary>
        /// Enable or disable support for automatically handling redirects.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        #endregion

        #region Private-Members

        private string _Header = "[RestWrapper] ";
        private ISerializationHelper _Serializer = new DefaultSerializationHelper();
        private int _StreamReadBufferSize = 65536;
        private int _TimeoutMilliseconds = 60000;
        private NameValueCollection _Headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        private AuthorizationHeader _Authorization = new AuthorizationHeader();
        private long? _ContentLength = null;
        private bool _DisposedValue = false;

        private HttpClientHandler _HttpClientHandler = null;
        private HttpClient _HttpClient = null;
        private HttpRequestMessage _HttpRequestMessage = null;
        private ChunkedSender _ChunkedSender = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param> 
        public RestRequest(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param> 
        /// <param name="method">HTTP method to use.</param>
        public RestRequest(string url, HttpMethod method)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
            Method = method;
        }

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param>
        /// <param name="method">HTTP method to use.</param> 
        /// <param name="contentType">Content type to use.</param>
        public RestRequest(
            string url,
            HttpMethod method, 
            string contentType)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
            Method = method;
            ContentType = contentType;
        }

        /// <summary>
        /// A simple RESTful HTTP client.
        /// </summary>
        /// <param name="url">URL to access on the server.</param>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="headers">HTTP headers to use.</param>
        /// <param name="contentType">Content type to use.</param>
        public RestRequest(
            string url,
            HttpMethod method,
            NameValueCollection headers,
            string contentType)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
            Method = method;
            ContentType = contentType;
            Headers = headers;
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
                    if (_ChunkedSender != null)
                    {
                        _ChunkedSender.Dispose();
                        _ChunkedSender = null;
                    }

                    if (_HttpRequestMessage != null)
                    {
                        _HttpRequestMessage.Dispose();
                        _HttpRequestMessage = null;
                    }

                    if (_HttpClient != null)
                    {
                        _HttpClient.Dispose();
                        _HttpClient = null;
                    }

                    if (_HttpClientHandler != null)
                    {
                        ((IDisposable)_HttpClientHandler).Dispose();
                        _HttpClientHandler = null;
                    }

                    Logger = null;
                    Url = null;
                    CertificateFilename = null;
                    CertificatePassword = null;
                    ContentType = null;
                    UserAgent = null;

                    _Headers = null;
                    _Authorization = null;

                    _DisposedValue = true;
                }
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a human-readable string of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "REST Request" + Environment.NewLine;
            ret += "  Method             : " + Method.ToString() + Environment.NewLine;
            ret += "  URL                : " + Url + Environment.NewLine;
            ret += "  Authorization" + Environment.NewLine;
            ret += "    User             : " + _Authorization.User + Environment.NewLine;
            ret += "    Password         : " + (!String.IsNullOrEmpty(_Authorization.Password) ? "(set)" : "") + Environment.NewLine;
            ret += "    Encode           : " + _Authorization.EncodeCredentials + Environment.NewLine;
            ret += "    Bearer Token     : " + _Authorization.BearerToken + Environment.NewLine;
            ret += "  Content Type       : " + ContentType + Environment.NewLine;
            ret += "  Content Length     : " + ContentLength + Environment.NewLine;
            ret += "  Certificate File   : " + CertificateFilename + Environment.NewLine;
            ret += "  Certificate Pass   : " + (!String.IsNullOrEmpty(CertificatePassword) ? "(set)" : "") + Environment.NewLine;
            ret += "  Auto Redirect      : " + AllowAutoRedirect + Environment.NewLine;

            if (Headers != null && Headers.Count > 0)
            {
                ret += "  Headers" + Environment.NewLine;
                foreach (KeyValuePair<string, string> curr in Headers)
                {
                    ret += "    " + curr.Key + ": " + curr.Value + Environment.NewLine;
                }
            }
              
            ret += Environment.NewLine;
             
            return ret;
        }

        /// <summary>
        /// Send the HTTP request with no data.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendAsync(CancellationToken token = default)
        {
            return SendInternalAsync(0, null, token);
        }

        /// <summary>
        /// Send the HTTP request using form-encoded data.
        /// This method will automatically set the content-type header.
        /// </summary>
        /// <param name="form">Dictionary.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendAsync(Dictionary<string, string> form, CancellationToken token = default)
        {
            // refer to https://github.com/dotnet/runtime/issues/22811
            if (form == null) form = new Dictionary<string, string>();
            var items = form.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
            var content = new StringContent(String.Join("&", items), null, "application/x-www-form-urlencoded");
            byte[] bytes = Encoding.UTF8.GetBytes(content.ReadAsStringAsync().Result);
            ContentLength = bytes.Length;
            if (String.IsNullOrEmpty(ContentType)) ContentType = "application/x-www-form-urlencoded";
            return SendAsync(bytes, token);
        }

        /// <summary>
        /// Send the HTTP request with the supplied data.
        /// </summary>
        /// <param name="data">A string containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendAsync(string data, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(data)) data = "";
            return SendAsync(Encoding.UTF8.GetBytes(data), token);
        }

        /// <summary>
        /// Send the HTTP request with the supplied data.
        /// </summary>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendAsync(byte[] data, CancellationToken token = default)
        {
            long contentLength = 0;
            MemoryStream stream = new MemoryStream(new byte[0]);

            if (data != null && data.Length > 0)
            {
                contentLength = data.Length;
                stream = new MemoryStream(data);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return SendInternalAsync(contentLength, stream, token);
        }

        /// <summary>
        /// Send the HTTP request with the supplied data.
        /// </summary>
        /// <param name="contentLength">The number of bytes to read from the input stream.</param>
        /// <param name="stream">A stream containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendAsync(long contentLength, Stream stream, CancellationToken token = default)
        {
            return SendInternalAsync(contentLength, stream, token);
        }

        /// <summary>
        /// Send a chunk of data.  A RestResponse object will only be returned when isFinal is set to true.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="isFinal">Boolean indicating if this is the final chunk.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendChunkAsync(string data, bool isFinal = false, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(data)) data = "";
            return SendChunkAsync(Encoding.UTF8.GetBytes(data), isFinal, token);
        }

        /// <summary>
        /// Send a chunk of data.  A RestResponse object will only be returned when isFinal is set to true.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="isFinal">Boolean indicating if this is the final chunk.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>RestResponse.</returns>
        public Task<RestResponse> SendChunkAsync(byte[] data, bool isFinal = false, CancellationToken token = default)
        {
            return SendChunkInternalAsync(data, isFinal, token);
        }

        #endregion

        #region Private-Methods
         
        private bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void SetupHttp()
        {
            if (_HttpClientHandler == null && _HttpClient == null && _HttpRequestMessage == null)
            {
                #region HTTP-Handler

                _HttpClientHandler = new HttpClientHandler();
                _HttpClientHandler.AllowAutoRedirect = AllowAutoRedirect;

                #endregion

                #region Handler-SSL

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                if (IgnoreCertificateErrors)
                {
#if NET6_0_OR_GREATER
                    _HttpClientHandler.ServerCertificateCustomValidationCallback = Validator;
#endif
                    ServicePointManager.ServerCertificateValidationCallback = Validator;
                }

                if (!String.IsNullOrEmpty(CertificateFilename))
                {
                    X509Certificate2 cert = null;

                    if (!String.IsNullOrEmpty(CertificatePassword))
                    {
                        Logger?.Invoke(_Header + "adding certificate including password");
                        cert = new X509Certificate2(CertificateFilename, CertificatePassword);
                    }
                    else
                    {
                        Logger?.Invoke(_Header + "adding certificate without password");
                        cert = new X509Certificate2(CertificateFilename);
                    }

                    _HttpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    _HttpClientHandler.SslProtocols = SslProtocols.Tls12;
                    _HttpClientHandler.ClientCertificates.Add(cert);
                }

                #endregion

                #region HTTP-Client

                _HttpClient = new HttpClient(_HttpClientHandler, true);

                _HttpClient.Timeout = TimeSpan.FromMilliseconds(_TimeoutMilliseconds);
                _HttpClient.DefaultRequestHeaders.ExpectContinue = false;
                _HttpClient.DefaultRequestHeaders.ConnectionClose = true;
                _HttpClient.DefaultRequestHeaders.TransferEncodingChunked = ChunkedTransfer;

                #endregion

                #region Authorization

                if (_Authorization != null)
                {
                    if (!String.IsNullOrEmpty(_Authorization.User))
                    {
                        if (_Authorization.EncodeCredentials)
                        {
                            Logger?.Invoke(_Header + "adding encoded credentials for user " + _Authorization.User);
                            string authInfo = _Authorization.User + ":" + _Authorization.Password;
                            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                            _HttpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + authInfo);
                        }
                        else
                        {
                            Logger?.Invoke(_Header + "adding plaintext credentials for user " + _Authorization.User);
                            _HttpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + _Authorization.User + ":" + _Authorization.Password);
                        }
                    }
                    else if (!String.IsNullOrEmpty(_Authorization.BearerToken))
                    {
                        Logger?.Invoke(_Header + "adding authorization bearer token " + _Authorization.BearerToken);
                        _HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _Authorization.BearerToken);
                    }
                    else if (!String.IsNullOrEmpty(_Authorization.Raw))
                    {
                        Logger?.Invoke(_Header + "adding authorization raw " + _Authorization.Raw);
                        if (!_HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _Authorization.Raw))
                            Logger?.Invoke(_Header + "unable to add raw authorization header: " + _Authorization.Raw);
                    }
                }

                #endregion

                #region Request-Message-and-Headers

                _HttpRequestMessage = new HttpRequestMessage(Method, Url);
                _HttpRequestMessage.Content = new ByteArrayContent(Array.Empty<byte>());
                _HttpRequestMessage.Version = new Version(1, 1);
                _HttpRequestMessage.Headers.Accept.ParseAdd("*/*");
                _HttpRequestMessage.Headers.Host = new Uri(Url).Authority;
                _HttpRequestMessage.Headers.UserAgent.ParseAdd(UserAgent);
                _HttpRequestMessage.Headers.Connection.Add("keep-alive");
                _HttpRequestMessage.Headers.TransferEncodingChunked = ChunkedTransfer;

                if (Headers != null && Headers.Count > 0)
                {
                    for (int i = 0; i < Headers.Count; i++)
                    {
                        string key = Headers.GetKey(i);
                        string val = Headers.Get(i);

                        if (String.IsNullOrEmpty(key)) continue;
                        if (String.IsNullOrEmpty(val)) continue;

                        if (key.ToLower().Trim().Equals("close"))
                        {
                            // do nothing
                        }
                        else if (key.ToLower().Trim().Equals("connection"))
                        {
                            // do nothing
                        }
                        else if (key.ToLower().Trim().Equals("content-length"))
                        {
                            // do nothing
                        }
                        else if (key.ToLower().Trim().Equals("content-type"))
                        {
                            _HttpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(val);
                        }
                        else
                        {
                            _HttpClient.DefaultRequestHeaders.Add(key, val);
                        }
                    }
                }

                #endregion

                #region Content

                if (ChunkedTransfer)
                {
                    _ChunkedSender = new ChunkedSender(_HttpClient, _HttpRequestMessage);
                }

                #endregion
            }
        }

        private RestResponse WebExceptionToRestResponse(WebException we)
        {
            RestResponse ret = new RestResponse();
            ret.Headers = null;
            ret.ContentEncoding = null;
            ret.ContentType = null;
            ret.ContentLength = 0;
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
                ret.StatusCode = (int)exceptionResponse.StatusCode;
                ret.StatusDescription = exceptionResponse.StatusDescription;

                Logger?.Invoke(_Header + "server returned status code " + ret.StatusCode);

                if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                {
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

                        Logger?.Invoke(_Header + "adding exception header " + key + ": " + val);
                        ret.Headers.Add(key, val);
                    }
                }

                if (exceptionResponse.ContentLength > 0)
                {
                    Logger?.Invoke(_Header + "attaching exception response stream to response with content length " + exceptionResponse.ContentLength + " bytes");
                    ret.ContentLength = exceptionResponse.ContentLength;
                    ret.Data = exceptionResponse.GetResponseStream();
                }
                else
                {
                    ret.ContentLength = 0;
                    ret.Data = null;
                }
            }

            return ret;
        }

        private async Task<RestResponse> SendInternalAsync(long contentLength, Stream stream, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));
            bool canceled = false;

            using (Timestamp ts = new Timestamp())
            {
                Logger?.Invoke(_Header + Method.ToString() + " " + Url);

                try
                {
                    SetupHttp();

                    #region Write-Request-Body-Data

                    HttpContent content = null;

                    if (Method != HttpMethod.Get && Method != HttpMethod.Head)
                    {
                        if (contentLength > 0 && stream != null)
                        {
                            Logger?.Invoke(_Header + "adding " + contentLength + " bytes to request");
                            content = new StreamContent(stream, _StreamReadBufferSize);

                            if (!String.IsNullOrEmpty(ContentType))
                                content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                            else
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        }
                    }

                    _HttpRequestMessage.Content = content;

                    #endregion

                    #region Submit-Request-and-Build-Response

                    HttpResponseMessage response = await _HttpClient.SendAsync(_HttpRequestMessage, HttpCompletionOption.ResponseHeadersRead, token);
                    RestResponse ret = new RestResponse(response);

                    Logger?.Invoke(_Header + ret.StatusCode + " response received after " + ts.TotalMs + "ms");

                    ts.End = DateTime.UtcNow;
                    ret.Time = ts;
                    return ret;

                    #endregion
                }
                catch (WebException we)
                {
                    #region WebException

                    Logger?.Invoke(_Header + "web exception: " + we.Message);
                    return WebExceptionToRestResponse(we);

                    #endregion
                }
                catch (TaskCanceledException)
                {
                    Logger?.Invoke(_Header + "task canceled");
                    canceled = true;
                    throw;
                }
                catch (OperationCanceledException)
                {
                    Logger?.Invoke(_Header + "operation canceled");
                    canceled = true;
                    throw;
                }
                catch (Exception e)
                {
                    Logger?.Invoke(_Header + "exception: " + e.Message);
                    throw;
                }
                finally
                {
                    ts.End = DateTime.UtcNow;

                    if (!canceled) Logger?.Invoke(_Header + "complete (" + ts.TotalMs + "ms)");
                    else Logger?.Invoke(_Header + "canceled (" + ts.TotalMs + "ms)");
                }
            }
        }

        private async Task<RestResponse> SendChunkInternalAsync(byte[] data, bool isFinal = false, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(Url)) throw new ArgumentNullException(nameof(Url));
            bool canceled = false;

            using (Timestamp ts = new Timestamp())
            {
                Logger?.Invoke(_Header + Method.ToString() + " " + Url);

                try
                {
                    SetupHttp();

                    if (!isFinal)
                    {
                        await _ChunkedSender.SendChunk(data);
                        return null;
                    }
                    else
                    {
                        RestResponse ret = null;

                        HttpResponseMessage response = await _ChunkedSender.SendChunk(data, isFinal, token).ConfigureAwait(false);
                        if (response != null)
                        {
                            ret = new RestResponse(response);
                            Logger?.Invoke(_Header + ret.StatusCode + " response received after " + ts.TotalMs + "ms");
                            ts.End = DateTime.UtcNow;
                            ret.Time = ts;
                        }

                        return ret;
                    }

                }
                catch (WebException we)
                {
                    #region WebException

                    Logger?.Invoke(_Header + "web exception: " + we.Message);
                    return WebExceptionToRestResponse(we);

                    #endregion
                }
                catch (TaskCanceledException)
                {
                    Logger?.Invoke(_Header + "task canceled");
                    canceled = true;
                    throw;
                }
                catch (OperationCanceledException)
                {
                    Logger?.Invoke(_Header + "operation canceled");
                    canceled = true;
                    throw;
                }
                catch (Exception e)
                {
                    Logger?.Invoke(_Header + "exception: " + e.Message);
                    throw;
                }
                finally
                {
                    ts.End = DateTime.UtcNow;

                    if (!canceled) Logger?.Invoke(_Header + "complete (" + ts.TotalMs + "ms)");
                    else Logger?.Invoke(_Header + "canceled (" + ts.TotalMs + "ms)");
                }
            }
        }

        #endregion
    }
}
