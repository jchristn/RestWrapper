using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                            _Username = InputString("Username:", null, true);
                            break;

                        case "pass":
                            _Password = InputString("Password:", null, true);
                            break;

                        case "encode":
                            _Encode = !_Encode;
                            break;

                        case "put":
                            req = new RestRequest(
                                InputString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Put, 
                                InputString("Content type:", "text/plain", false));
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false));
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
                                InputString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Post, 
                                InputString("Content type:", "text/plain", false));
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false));
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
                                InputString("URL:", "http://localhost:8888/", false),
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
                                InputString("URL:", "http://localhost:8888/", false),
                                HttpMethod.Delete,
                                InputString("Content type:", "text/plain", false));
                            req.UserAgent = null;
                            req.TimeoutMilliseconds = _Timeout;
                            req.Authorization.User = _Username;
                            req.Authorization.Password = _Password;
                            req.Authorization.EncodeCredentials = _Encode;

                            if (_Debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false)); 
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
                                InputString("URL:", "http://localhost:8888/", false),
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
                                InputString("URL:", "http://localhost:8888/", false),
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

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }

        static bool InputBoolean(string question, bool trueDefault)
        {
            Console.Write(question);

            if (trueDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (trueDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (trueDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
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
