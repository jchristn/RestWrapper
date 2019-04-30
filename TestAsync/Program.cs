using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestWrapper;

namespace TestAsync
{
    class Program
    {
        static bool debug = false;

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

                        case "put":
                            req = new RestRequest(
                                InputString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.PUT,
                                null,
                                InputString("Content type:", "text/plain", false),
                                true);
                            req.ConsoleDebug = debug;
                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false));
                            resp = req.SendAsync(data).Result;
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                            }
                            break;

                        case "post":
                            req = new RestRequest(
                                InputString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.POST,
                                null,
                                InputString("Content type:", "text/plain", false),
                                true);
                            req.ConsoleDebug = debug;
                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false));
                            resp = req.SendAsync(data).Result;
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                            }
                            break;

                        case "delete":
                            req = new RestRequest(
                                InputString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.DELETE,
                                null,
                                InputString("Content type:", "text/plain", false),
                                true);
                            req.ConsoleDebug = debug;
                            data = Encoding.UTF8.GetBytes(InputString("Data:", "Hello, world!", false));
                            resp = req.SendAsync(data).Result;
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                            }
                            break;

                        case "head":
                            req = new RestRequest(
                                InputString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.HEAD,
                                null,
                                null,
                                true);
                            req.ConsoleDebug = debug;
                            resp = req.SendAsync(null).Result;
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                            }
                            break;

                        case "get":
                            req = new RestRequest(
                                InputString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.GET,
                                null,
                                null,
                                true);
                            req.ConsoleDebug = debug;
                            resp = req.SendAsync(null).Result;
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                            }
                            break;

                        case "debug":
                            debug = !debug;
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
            Console.WriteLine("--- Available Commands ---");
            Console.WriteLine("  ?           Help, this menu");
            Console.WriteLine("  q           Quit the application");
            Console.WriteLine("  c           Clear the screen");
            Console.WriteLine("  get         Submit a GET request");
            Console.WriteLine("  put         Submit a PUT request");
            Console.WriteLine("  post        Submit a POST request");
            Console.WriteLine("  delete      Submit a DELETE request");
            Console.WriteLine("  head        Submit a HEAD request");
            Console.WriteLine("  debug       Enable or disable console debugging (currently " + debug + ")");
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
    }
}

