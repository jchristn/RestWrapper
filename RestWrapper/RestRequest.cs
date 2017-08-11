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
    public class RestRequest
    {
        #region Constructor

        /// <summary>
        /// A simple RESTful HTTP library.
        /// </summary>
        public RestRequest()
        {
        }

        #endregion

        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Static-Methods

        /// <summary>
        /// Send a RESTful HTTP request without specifying a certificate file.  All exceptions are thrown to the caller.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent.</param>
        /// <param name="contentType">The type of content contained in Data (the request body).</param>
        /// <param name="method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="user">The HTTP user authorization field data.</param>
        /// <param name="password">The HTTP password authorization field data.</param>
        /// <param name="encodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="userHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequest(
            string url,
            string contentType,
            string method,
            string user,
            string password,
            bool encodeCredentials,
            bool ignoreCertErrors,
            Dictionary<string, string> userHeaders,
            byte[] data
            )
        {
            return SendRequestInternal(url, contentType, method, user, password, encodeCredentials, ignoreCertErrors, null, null, userHeaders, data);
        }

        /// <summary>
        /// Send a RESTful HTTP request with a specified certificate file.  All exceptions are thrown to the caller.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent.</param>
        /// <param name="contentType">The type of content contained in Data (the request body).</param>
        /// <param name="method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="user">The HTTP user authorization field data.</param>
        /// <param name="password">The HTTP password authorization field data.</param>
        /// <param name="encodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="certFile">Specify the PFX certificate file to use.</param>
        /// <param name="certPass">Specify the password, if any, to the PFX certificate file.</param>
        /// <param name="userHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="Data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequest(
            string url,
            string contentType,
            string method,
            string user,
            string password,
            bool encodeCredentials,
            bool ignoreCertErrors,
            string certFile,
            string certPass,
            Dictionary<string, string> userHeaders,
            byte[] Data
            )
        {
            return SendRequestInternal(url, contentType, method, user, password, encodeCredentials, ignoreCertErrors, certFile, certPass, userHeaders, Data);
        }

        /// <summary>
        /// Send a RESTful HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent.</param>
        /// <param name="contentType">The type of content contained in Data (the request body).</param>
        /// <param name="method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="user">The HTTP user authorization field data.</param>
        /// <param name="password">The HTTP password authorization field data.</param>
        /// <param name="encodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="userHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequestSafe(
            string url,
            string contentType,
            string method,
            string user,
            string password,
            bool encodeCredentials,
            bool ignoreCertErrors,
            Dictionary<string, string> userHeaders,
            byte[] data)
        {
            return SendRequestInternalSafe(url, contentType, method, user, password, encodeCredentials, ignoreCertErrors, null, null, userHeaders, data);
        }

        /// <summary>
        /// Send a RESTful HTTP request with a specified certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent.</param>
        /// <param name="contentType">The type of content contained in Data (the request body).</param>
        /// <param name="method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="user">The HTTP user authorization field data.</param>
        /// <param name="password">The HTTP password authorization field data.</param>
        /// <param name="encodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="certFile">Specify the PFX certificate file to use.</param>
        /// <param name="certPass">Specify the password, if any, to the PFX certificate file.</param>
        /// <param name="userHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequestSafe(
            string url,
            string contentType,
            string method,
            string user,
            string password,
            bool encodeCredentials,
            bool ignoreCertErrors,
            string certFile,
            string certPass,
            Dictionary<string, string> userHeaders,
            byte[] data)
        {
            return SendRequestInternalSafe(url, contentType, method, user, password, encodeCredentials, ignoreCertErrors, certFile, certPass, userHeaders, data);
        }

        #endregion

        #region Private-Static-Methods

        private static byte[] StreamToBytes(Stream inStream)
        {
            if (inStream == null) return null;
            
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        private static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static RestResponse SendRequestInternal(
           string url,
           string contentType,
           string method,
           string user,
           string password,
           bool encodeCredentials,
           bool ignoreCertErrors,
           string certFile,
           string certPass,
           Dictionary<string, string> userHeaders,
           byte[] Data
           )
        {
            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (String.IsNullOrEmpty(method)) throw new ArgumentNullException(nameof(method));

            #endregion

            #region Check-Method

            if ((String.Compare(method.ToLower().Trim(), "head") != 0) &&
                (String.Compare(method.ToLower().Trim(), "get") != 0) &&
                (String.Compare(method.ToLower().Trim(), "post") != 0) &&
                (String.Compare(method.ToLower().Trim(), "put") != 0) &&
                (String.Compare(method.ToLower().Trim(), "delete") != 0) &&
                (String.Compare(method.ToLower().Trim(), "options") != 0))
            {
                throw new ArgumentException("Invalid method supplied: " + method + ".");
            }

            #endregion

            #region Setup-Webrequest

            if (ignoreCertErrors)
            {
                ServicePointManager.ServerCertificateValidationCallback = Validator;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(url);
            client.KeepAlive = false;
            client.Method = method.ToUpper().Trim();
            client.AllowAutoRedirect = true;
            client.Timeout = 30000;
            client.ContentLength = 0;
            client.ContentType = contentType;
            client.UserAgent = "RestWrapper (www.github.com/jchristn/RestWrapper)";

            #endregion

            #region Add-Certificate

            if (!String.IsNullOrEmpty(certFile))
            {
                if (!String.IsNullOrEmpty(certPass))
                {
                    X509Certificate2 cert = new X509Certificate2(certFile, certPass);
                    client.ClientCertificates.Add(cert);
                }
                else
                {
                    X509Certificate2 cert = new X509Certificate2(certFile);
                    client.ClientCertificates.Add(cert);
                }
            }

            #endregion

            #region Add-Headers

            if (userHeaders != null && userHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in userHeaders)
                {
                    if (String.IsNullOrEmpty(pair.Key)) continue;
                    if (String.IsNullOrEmpty(pair.Value)) continue;

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

            if (!String.IsNullOrEmpty(user))
            {
                if (encodeCredentials)
                {
                    string authInfo = user + ":" + password;
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    client.Headers.Add("Authorization", "Basic " + authInfo);
                }
                else
                {
                    client.Headers.Add("Authorization", user);
                }
            }

            #endregion

            #region Package-Payload

            if ((String.Compare(method.ToLower().Trim(), "post") == 0) ||
                (String.Compare(method.ToLower().Trim(), "put") == 0) ||
                (String.Compare(method.ToLower().Trim(), "delete") == 0))
            {
                if (Data != null && Data.Length > 0)
                {
                    client.ContentLength = Data.Length;
                    client.ContentType = contentType;
                    Stream clientStream = client.GetRequestStream();
                    clientStream.Write(Data, 0, Data.Length);
                    clientStream.Close();
                }
            }

            #endregion

            #region Submit-Request-and-Build-Response

            HttpWebResponse response = (HttpWebResponse)client.GetResponse();
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

            if (response.Headers != null)
            {
                if (response.Headers.Count > 0)
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
                        ret.Headers.Add(key, val);
                    }
                }
            }

            #endregion

            #region Payload

            StringBuilder sb = new StringBuilder();
            Byte[] buf = new byte[8192];
            Stream responseStream = response.GetResponseStream();

            if (responseStream != null)
            {
                if (responseStream.CanRead)
                {
                    ret.Data = StreamToBytes(responseStream);
                    ret.ContentLength = ret.Data.Length;
                    responseStream.Close();
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

            #endregion

            return ret;
        }

        private static RestResponse SendRequestInternalSafe(
           string url,
           string contentType,
           string method,
           string user,
           string password,
           bool encodeCredentials,
           bool ignoreCertErrors,
           string certFile,
           string certPass,
           Dictionary<string, string> userHeaders,
           byte[] data
           )
        {
            try
            {
                return SendRequestInternal(url, contentType, method, user, password, encodeCredentials, ignoreCertErrors, certFile, certPass, userHeaders, data);
            }
            catch (WebException we)
            {
                #region WebException

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

                            ret.Headers.Add(key, val);
                        }
                    }

                    ret.Data = StreamToBytes(exceptionResponse.GetResponseStream());
                }

                return ret;

                #endregion
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
