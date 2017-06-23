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
        /// <param name="URL">The URL to which the request should be sent.</param>
        /// <param name="ContentType">The type of content contained in Data (the request body).</param>
        /// <param name="Method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="User">The HTTP user authorization field data.</param>
        /// <param name="Password">The HTTP password authorization field data.</param>
        /// <param name="EncodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="UserHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="Data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequest(
            string URL,
            string ContentType,
            string Method,
            string User,
            string Password,
            bool EncodeCredentials,
            bool IgnoreCertErrors,
            Dictionary<string, string> UserHeaders,
            byte[] Data
            )
        {
            return SendRequestInternal(URL, ContentType, Method, User, Password, EncodeCredentials, IgnoreCertErrors, null, null, UserHeaders, Data);
        }

        /// <summary>
        /// Send a RESTful HTTP request with a specified certificate file.  All exceptions are thrown to the caller.
        /// </summary>
        /// <param name="URL">The URL to which the request should be sent.</param>
        /// <param name="ContentType">The type of content contained in Data (the request body).</param>
        /// <param name="Method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="User">The HTTP user authorization field data.</param>
        /// <param name="Password">The HTTP password authorization field data.</param>
        /// <param name="EncodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="CertificateFile">Specify the PFX certificate file to use.</param>
        /// <param name="CertificatePassword">Specify the password, if any, to the PFX certificate file.</param>
        /// <param name="UserHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="Data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequest(
            string URL,
            string ContentType,
            string Method,
            string User,
            string Password,
            bool EncodeCredentials,
            bool IgnoreCertErrors,
            string CertificateFile,
            string CertificatePassword,
            Dictionary<string, string> UserHeaders,
            byte[] Data
            )
        {
            return SendRequestInternal(URL, ContentType, Method, User, Password, EncodeCredentials, IgnoreCertErrors, CertificateFile, CertificatePassword, UserHeaders, Data);
        }

        /// <summary>
        /// Send a RESTful HTTP request without specifying a certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="URL">The URL to which the request should be sent.</param>
        /// <param name="ContentType">The type of content contained in Data (the request body).</param>
        /// <param name="Method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="User">The HTTP user authorization field data.</param>
        /// <param name="Password">The HTTP password authorization field data.</param>
        /// <param name="EncodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="UserHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="Data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequestSafe(
            string URL,
            string ContentType,
            string Method,
            string User,
            string Password,
            bool EncodeCredentials,
            bool IgnoreCertErrors,
            Dictionary<string, string> UserHeaders,
            byte[] Data)
        {
            return SendRequestInternalSafe(URL, ContentType, Method, User, Password, EncodeCredentials, IgnoreCertErrors, null, null, UserHeaders, Data);
        }

        /// <summary>
        /// Send a RESTful HTTP request with a specified certificate file.  WebExceptions are managed and formed into a response object and are not thrown.  Uncaught exceptions are thrown to the caller.
        /// </summary>
        /// <param name="URL">The URL to which the request should be sent.</param>
        /// <param name="ContentType">The type of content contained in Data (the request body).</param>
        /// <param name="Method">The HTTP verb to use for this request (GET, PUT, POST, DELETE, HEAD).</param>
        /// <param name="User">The HTTP user authorization field data.</param>
        /// <param name="Password">The HTTP password authorization field data.</param>
        /// <param name="EncodeCredentials">Specify whether or not credentials should be encoded in the HTTP authorization header.</param>
        /// <param name="CertificateFile">Specify the PFX certificate file to use.</param>
        /// <param name="CertificatePassword">Specify the password, if any, to the PFX certificate file.</param>
        /// <param name="UserHeaders">Supply any custom or user-specified headers.</param>
        /// <param name="Data">A byte array containing the data you wish to send to the server (does not work with GET requests).</param>
        /// <returns>A RestResponse object containing response data.</returns>
        public static RestResponse SendRequestSafe(
            string URL,
            string ContentType,
            string Method,
            string User,
            string Password,
            bool EncodeCredentials,
            bool IgnoreCertErrors,
            string CertificateFile,
            string CertificatePassword,
            Dictionary<string, string> UserHeaders,
            byte[] Data)
        {
            return SendRequestInternalSafe(URL, ContentType, Method, User, Password, EncodeCredentials, IgnoreCertErrors, CertificateFile, CertificatePassword, UserHeaders, Data);
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
           string URL,
           string ContentType,
           string Method,
           string User,
           string Password,
           bool EncodeCredentials,
           bool IgnoreCertErrors,
           string CertificateFile,
           string CertificatePassword,
           Dictionary<string, string> UserHeaders,
           byte[] Data
           )
        {
            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(URL)) throw new ArgumentNullException("URL");
            if (String.IsNullOrEmpty(URL)) throw new ArgumentNullException("Method");

            #endregion

            #region Check-Method

            if ((String.Compare(Method.ToLower().Trim(), "head") != 0) &&
                (String.Compare(Method.ToLower().Trim(), "get") != 0) &&
                (String.Compare(Method.ToLower().Trim(), "post") != 0) &&
                (String.Compare(Method.ToLower().Trim(), "put") != 0) &&
                (String.Compare(Method.ToLower().Trim(), "delete") != 0))
            {
                throw new ArgumentOutOfRangeException("Method");
            }

            #endregion

            #region Setup-Webrequest

            if (IgnoreCertErrors)
            {
                ServicePointManager.ServerCertificateValidationCallback = Validator;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(URL);
            client.KeepAlive = false;
            client.Method = Method.ToUpper().Trim();
            client.AllowAutoRedirect = true;
            client.Timeout = 30000;
            client.ContentLength = 0;
            client.ContentType = ContentType;
            client.UserAgent = "RestWrapper (www.github.com/jchristn/RestWrapper)";

            #endregion

            #region Add-Certificate

            if (!String.IsNullOrEmpty(CertificateFile))
            {
                if (!String.IsNullOrEmpty(CertificatePassword))
                {
                    X509Certificate2 cert = new X509Certificate2(@CertificateFile, CertificatePassword);
                    client.ClientCertificates.Add(cert);
                }
                else
                {
                    X509Certificate2 cert = new X509Certificate2(CertificateFile);
                    client.ClientCertificates.Add(cert);
                }
            }

            #endregion

            #region Add-Headers

            if (UserHeaders != null && UserHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in UserHeaders)
                {
                    if (String.IsNullOrEmpty(pair.Key)) continue;
                    if (String.IsNullOrEmpty(pair.Value)) continue;

                    if (pair.Key.ToLower().Trim().Equals("accept"))
                    {
                        client.Accept = pair.Value; 
                    }
                    else if (pair.Key.ToLower().Trim().Equals("connection"))
                    {
                        client.Connection = pair.Value;  
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
                    string authInfo = User + ":" + Password;
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    client.Headers.Add("Authorization", "Basic " + authInfo);
                }
                else
                {
                    client.Headers.Add("Authorization", User);
                }
            }

            #endregion

            #region Package-Payload

            if ((String.Compare(Method.ToLower().Trim(), "post") == 0) ||
                (String.Compare(Method.ToLower().Trim(), "put") == 0) ||
                (String.Compare(Method.ToLower().Trim(), "delete") == 0))
            {
                if (Data != null && Data.Length > 0)
                {
                    client.ContentLength = Data.Length;
                    client.ContentType = ContentType;
                    Stream clientStream = client.GetRequestStream();
                    clientStream.Write(Data, 0, Data.Length);
                    clientStream.Close();
                }
            }

            #endregion

            #region Submit-Request-and-Build-Response

            HttpWebResponse response = (HttpWebResponse)client.GetResponse();
            RestResponse ret = new RestResponse();
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
           string URL,
           string ContentType,
           string Method,
           string User,
           string Password,
           bool EncodeCredentials,
           bool IgnoreCertErrors,
           string CertificateFile,
           string CertificatePassword,
           Dictionary<string, string> UserHeaders,
           byte[] Data
           )
        {
            try
            {
                return SendRequestInternal(URL, ContentType, Method, User, Password, EncodeCredentials, IgnoreCertErrors, CertificateFile, CertificatePassword, UserHeaders, Data);
            }
            catch (WebException we)
            {
                #region WebException

                RestResponse resp = new RestResponse();
                resp.Headers = null;
                resp.ContentEncoding = null;
                resp.ContentType = null;
                resp.ContentLength = 0;
                resp.ResponseURI = null;
                resp.StatusCode = 0;
                resp.StatusDescription = null;
                resp.Data = null;

                HttpWebResponse exceptionResponse = we.Response as HttpWebResponse;
                if (exceptionResponse != null)
                {
                    resp.ContentEncoding = exceptionResponse.ContentEncoding;
                    resp.ContentType = exceptionResponse.ContentType;
                    resp.ContentLength = exceptionResponse.ContentLength;
                    resp.ResponseURI = exceptionResponse.ResponseUri.ToString();
                    resp.StatusCode = (int)exceptionResponse.StatusCode;
                    resp.StatusDescription = exceptionResponse.StatusDescription;

                    if (exceptionResponse.Headers != null && exceptionResponse.Headers.Count > 0)
                    {
                        resp.Headers = new Dictionary<string, string>();
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

                            resp.Headers.Add(key, val);
                        }
                    }

                    resp.Data = StreamToBytes(exceptionResponse.GetResponseStream());
                }

                return resp;

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
