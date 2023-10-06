using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GetSomeInput;
using RestWrapper;

namespace TestAsync
{
    class Program
    {
        static Action<string> _Logger = Console.WriteLine;
        static int _TimeoutMs = 10000;
        static string _Username = null;
        static string _Password = null;
        static string _BearerToken = null;
        static bool _Encode = true;

        static async Task Main(string[] args)
        {
            try
            {
                bool runForever = true;

                string url;
                string contentType;
                string body;
                Dictionary<string, string> form;

                while (runForever)
                {
                    string userInput = "";
                    while (String.IsNullOrEmpty(userInput))
                    {
                        Console.Write("Command [? for help]: ");
                        userInput = Console.ReadLine();
                    }

                    switch (userInput)
                    {
                        case "quit":
                        case "q":
                            runForever = false;
                            break;

                        case "cls":
                            Console.Clear();
                            break;

                        case "?":
                            Menu();
                            break;

                        case "user":
                            _Username = Inputty.GetString("Username:", null, true);
                            break;

                        case "pass":
                            _Password = Inputty.GetString("Password:", null, true);
                            break;

                        case "encode":
                            _Encode = !_Encode;
                            break;

                        case "token":
                            _BearerToken = Inputty.GetString("Bearer Token:", null, true);
                            break;

                        case "put":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            body = Inputty.GetString("Body:", null, true);
                            Console.WriteLine(await GetResponse(url, HttpMethod.Put, contentType, body));
                            break;

                        case "post":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            body = Inputty.GetString("Body:", null, true);
                            Console.WriteLine(await GetResponse(url, HttpMethod.Post, contentType, body));
                            break;

                        case "form":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            form = Inputty.GetDictionary<string, string>("Key:", "Val:");
                            Console.WriteLine(await GetResponseWithForm(url, HttpMethod.Post, contentType, form));
                            break;

                        case "del":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            body = Inputty.GetString("Body:", null, true);
                            Console.WriteLine(await GetResponse(url, HttpMethod.Delete, contentType, body));
                            break;

                        case "head":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            Console.WriteLine(await GetResponse(url, HttpMethod.Head));
                            break;

                        case "get":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            Console.WriteLine(await GetResponse(url, HttpMethod.Get));
                            break;

                        case "query":
                            url = Inputty.GetString("URL:", "http://localhost:8888?foo=bar", true);
                            using (RestRequest req = new RestRequest(url))
                            {
                                if (req.Query.AllKeys.Count() > 0)
                                {
                                    for (int i = 0; i < req.Query.AllKeys.Count(); i++)
                                    {
                                        Console.WriteLine(req.Query.AllKeys[i] + ": " + req.Query.Get(i));
                                    }
                                }
                            }
                            break;

                        case "debug":
                            if (_Logger == null) _Logger = Console.WriteLine;
                            else _Logger = null;
                            break;

                        case "timeout":
                            Console.Write("Timeout (ms): ");
                            _TimeoutMs = Convert.ToInt32(Console.ReadLine());
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionConsole("Main", "Outer exception", e);
            }
        }

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("--- Available Commands ---");
            Console.WriteLine("  ?           Help, this menu");
            Console.WriteLine("  q           Quit the application");
            Console.WriteLine("  c           Clear the screen");
            Console.WriteLine("  user        Set basic auth username");
            Console.WriteLine("  pass        Set basic auth password");
            Console.WriteLine("  encode      Enable or disable credential encoding (currently " + _Encode + ")");
            Console.WriteLine("  token       Set bearer token");
            Console.WriteLine("  get         Submit a GET request");
            Console.WriteLine("  put         Submit a PUT request");
            Console.WriteLine("  post        Submit a POST request");
            Console.WriteLine("  del         Submit a DELETE request");
            Console.WriteLine("  form        Submit a POST request using form data");
            Console.WriteLine("  head        Submit a HEAD request");
            Console.WriteLine("  query       Retrieve the query from a given URL");
            Console.WriteLine("  debug       Enable or disable console debugging (currently " + (_Logger != null) + ")");
            Console.WriteLine("  timeout     Set timeout milliseconds (currently " + _TimeoutMs + "ms)");
            Console.WriteLine("");
        }

        static string StackToString()
        {
            string ret = "";

            StackTrace t = new StackTrace();
            for (int i = 0; i < t.FrameCount; i++)
            {
                if (i == 0)
                {
                    ret += t.GetFrame(i).GetMethod().Name;
                }
                else
                {
                    ret += " <= " + t.GetFrame(i).GetMethod().Name;
                }
            }

            return ret;
        }

        static void ExceptionConsole(string method, string text, Exception e)
        {
            var st = new StackTrace(e, true);
            var frame = st.GetFrame(0);
            int line = frame.GetFileLineNumber();
            string filename = frame.GetFileName();

            Console.WriteLine("---");
            Console.WriteLine("An exception was encountered which triggered this message.");
            Console.WriteLine("  Method: " + method);
            Console.WriteLine("  Text: " + text);
            Console.WriteLine("  Type: " + e.GetType().ToString());
            Console.WriteLine("  Data: " + e.Data);
            Console.WriteLine("  Inner: " + e.InnerException);
            Console.WriteLine("  Message: " + e.Message);
            Console.WriteLine("  Source: " + e.Source);
            Console.WriteLine("  StackTrace: " + e.StackTrace);
            Console.WriteLine("  Stack: " + StackToString());
            Console.WriteLine("  Line: " + line);
            Console.WriteLine("  File: " + filename);
            Console.WriteLine("  ToString: " + e.ToString());
            Console.WriteLine("---");

            return;
        }

        static async Task<string> GetResponse(string url, HttpMethod method, string contentType = null, string body = null)
        {
            Console.WriteLine("");
            string msg = method.ToString() + " " + url;
            if (!String.IsNullOrEmpty(contentType)) msg += " (" + contentType + ")";
            if (!String.IsNullOrEmpty(body)) msg += " (" + body.Length + " bytes)";
            Console.WriteLine(msg);

            using (RestRequest req = new RestRequest(url, method, contentType))
            {
                req.TimeoutMilliseconds = _TimeoutMs;
                req.Logger = _Logger;
                req.Authorization.User = _Username;
                req.Authorization.Password = _Password;
                req.Authorization.BearerToken = _BearerToken;
                req.Authorization.EncodeCredentials = _Encode;

                if (!String.IsNullOrEmpty(req.Authorization.User)
                    || !String.IsNullOrEmpty(req.Authorization.Password)
                    || !String.IsNullOrEmpty(req.Authorization.BearerToken))
                {
                    Console.WriteLine("Using authentication material:");
                    if (!String.IsNullOrEmpty(req.Authorization.User))
                        Console.WriteLine("| User: " + req.Authorization.User);
                    if (!String.IsNullOrEmpty(req.Authorization.Password))
                        Console.WriteLine("| Password: " + req.Authorization.Password);
                    if (!String.IsNullOrEmpty(req.Authorization.BearerToken))
                        Console.WriteLine("| Bearer Token: " + req.Authorization.BearerToken);
                }

                if (req.Query.AllKeys.Count() > 0)
                {
                    Console.WriteLine("Using query:");
                    for (int i = 0; i < req.Query.AllKeys.Count(); i++)
                        Console.WriteLine("| " + req.Query.AllKeys[i] + ": " + req.Query.Get(i));
                }

                if (String.IsNullOrEmpty(body))
                {
                    using (RestResponse resp = await req.SendAsync())
                    {
                        if (resp == null)
                        {
                            Console.WriteLine("*** Null response");
                        }
                        else
                        {
                            Console.WriteLine("Status: " + resp.StatusCode + " " + resp.ContentLength + " bytes [" + resp.Time.TotalMs + "ms]");
                        }

                        Console.WriteLine("");
                        if (resp.ContentLength > 0)
                        {
                            Console.WriteLine("Returning " + resp.ContentLength + " bytes");
                            Console.WriteLine(resp.DataAsString);
                            return resp.DataAsString;
                        }
                        else return null;
                    }
                }
                else
                {
                    using (RestResponse resp = await req.SendAsync(body))
                    {
                        if (resp == null)
                        {
                            Console.WriteLine("*** Null response");
                        }
                        else
                        {
                            Console.WriteLine("Status: " + resp.StatusCode + " " + resp.ContentLength + " bytes [" + resp.Time.TotalMs + "ms]");
                        }

                        Console.WriteLine("");
                        if (resp.ContentLength > 0)
                        {
                            Console.WriteLine("Returning " + resp.ContentLength + " bytes");
                            Console.WriteLine(resp.DataAsString);
                            return resp.DataAsString;
                        }
                        else return null;
                    }
                }
            }
        }

        static async Task<string> GetResponseWithForm(string url, HttpMethod method, string contentType, Dictionary<string, string> form)
        {
            Console.WriteLine("");
            string msg = method.ToString() + " " + url;
            if (!String.IsNullOrEmpty(contentType)) msg += " (" + contentType + ")";
            if (form != null) msg += " (" + form.Count + " key-value pairs)";
            Console.WriteLine(msg);

            using (RestRequest req = new RestRequest(url, method, contentType))
            {
                req.TimeoutMilliseconds = _TimeoutMs;
                req.Logger = _Logger;
                req.Authorization.User = _Username;
                req.Authorization.Password = _Password;
                req.Authorization.BearerToken = _BearerToken;
                req.Authorization.EncodeCredentials = _Encode;

                if (!String.IsNullOrEmpty(req.Authorization.User)
                    || !String.IsNullOrEmpty(req.Authorization.Password)
                    || !String.IsNullOrEmpty(req.Authorization.BearerToken))
                {
                    Console.WriteLine("Using authentication material:");
                    if (!String.IsNullOrEmpty(req.Authorization.User))
                        Console.WriteLine("| User: " + req.Authorization.User);
                    if (!String.IsNullOrEmpty(req.Authorization.Password))
                        Console.WriteLine("| Password: " + req.Authorization.Password);
                    if (!String.IsNullOrEmpty(req.Authorization.BearerToken))
                        Console.WriteLine("| Bearer Token: " + req.Authorization.BearerToken);
                }

                if (req.Query.AllKeys.Count() > 0)
                {
                    Console.WriteLine("Using query:");
                    for (int i = 0; i < req.Query.AllKeys.Count(); i++)
                        Console.WriteLine("| " + req.Query.AllKeys[i] + ": " + req.Query.Get(i));
                }

                if (form == null)
                {
                    using (RestResponse resp = await req.SendAsync())
                    {
                        if (resp == null)
                        {
                            Console.WriteLine("*** Null response");
                        }
                        else
                        {
                            Console.WriteLine("Status: " + resp.StatusCode + " " + resp.ContentLength + " bytes [" + resp.Time.TotalMs + "ms]");
                        }

                        Console.WriteLine("");
                        if (resp.ContentLength > 0)
                        {
                            Console.WriteLine("Returning " + resp.ContentLength + " bytes");
                            Console.WriteLine(resp.DataAsString);
                            return resp.DataAsString;
                        }
                        else return null;
                    }
                }
                else
                {
                    using (RestResponse resp = await req.SendAsync(form))
                    {
                        if (resp == null)
                        {
                            Console.WriteLine("*** Null response");
                        }
                        else
                        {
                            Console.WriteLine("Status: " + resp.StatusCode + " " + resp.ContentLength + " bytes [" + resp.Time.TotalMs + "ms]");
                        }

                        Console.WriteLine("");
                        if (resp.ContentLength > 0)
                        {
                            Console.WriteLine("Returning " + resp.ContentLength + " bytes");
                            Console.WriteLine(resp.DataAsString);
                            return resp.DataAsString;
                        }
                        else return null;
                    }
                }
            }
        }
    }
}
