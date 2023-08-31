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

namespace Test
{
    class Program
    {
        static bool _Debug = false;
        static int _Timeout = 10000;
        static string _Username = null;
        static string _Password = null;
        static bool _Encode = true;

        static void Main(string[] args)
        {
            try
            { 
                RestRequest req = null;
                RestResponse resp = null;
                bool runForever = true;
                byte[] data = null;

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

                        case "put":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Put,
                                Inputty.GetString("Content type:", "text/plain", false));
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false));
                            resp = req.Send(data);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(Encoding.UTF8.GetString(StreamToBytes(resp.Data)));
                                }
                            }
                            break;

                        case "post":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Post,
                                Inputty.GetString("Content type:", "text/plain", false));
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false));
                            resp = req.Send(data);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(resp.DataAsString);
                                }
                            }
                            break;

                        case "form":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Post);
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;
                             
                            resp = req.Send(InputDictionary());
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(resp.DataAsString);
                                }
                            }
                            break;

                        case "del":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Delete,
                                Inputty.GetString("Content type:", "text/plain", false));
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false)); 
                            resp = req.Send(data);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(Encoding.UTF8.GetString(StreamToBytes(resp.Data)));
                                }
                            }
                            break;

                        case "head":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Head);
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            resp = req.Send();
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(Encoding.UTF8.GetString(StreamToBytes(resp.Data)));
                                }
                            }
                            break;

                        case "get":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Get);
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            resp = req.Send();
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                if (resp.Data != null && resp.ContentLength > 0)
                                {
                                    Console.WriteLine("Content:");
                                    Console.WriteLine(Encoding.UTF8.GetString(StreamToBytes(resp.Data)));
                                }
                            }
                            break;

                        case "query":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/?foo&bar=baz&another=val1&another=val2&another=val3&key=val", false),
                                HttpMethod.Get);

                            if (req.Query.AllKeys.Count() > 0)
                            {
                                for (int i = 0; i < req.Query.AllKeys.Count(); i++)
                                {
                                    Console.WriteLine(req.Query.AllKeys[i] + ": " + req.Query.Get(i));
                                }
                            }
                            break;

                        case "debug":
                            _Debug = !_Debug;
                            break;

                        case "timeout":
                            Console.Write("Timeout (ms): ");
                            _Timeout = Convert.ToInt32(Console.ReadLine());
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
            Console.WriteLine("  get         Submit a GET request");
            Console.WriteLine("  put         Submit a PUT request");
            Console.WriteLine("  post        Submit a POST request");
            Console.WriteLine("  del         Submit a DELETE request"); 
            Console.WriteLine("  form        Submit a POST request using form data"); 
            Console.WriteLine("  head        Submit a HEAD request");
            Console.WriteLine("  query       Retrieve the query from a given URL");
            Console.WriteLine("  debug       Enable or disable console debugging (currently " + _Debug + ")");
            Console.WriteLine("  timeout     Set timeout milliseconds (currently " + _Timeout + "ms)");
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

        static Dictionary<string, string> InputDictionary()
        {
            Console.WriteLine("Build form, press ENTER on 'Key' to exit");

            Dictionary<string, string> ret = new Dictionary<string, string>();

            while (true)
            {
                Console.Write("Key   : ");
                string key = Console.ReadLine();
                if (String.IsNullOrEmpty(key)) return ret;

                Console.Write("Value : ");
                string val = Console.ReadLine();
                ret.Add(key, val);
            }
        }

        static byte[] StreamToBytes(Stream input)
        {
            if (input == null) return null;
            if (!input.CanRead) return null;

            if (input is MemoryStream)
                return ((MemoryStream)input).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
