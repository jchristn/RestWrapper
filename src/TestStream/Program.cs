using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GetSomeInput;
using RestWrapper;

namespace TestStream
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
                byte[] data;
                MemoryStream ms;

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
                                Inputty.GetString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.Put,
                                null,
                                Inputty.GetString("Content type:", "text/plain", false));

                            if (debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false));
                            ms = new MemoryStream(data);
                            resp = req.Send(data.Length, ms);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                Console.WriteLine("Content:" + Environment.NewLine + resp.DataAsString);
                            }
                            break;

                        case "post":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.Post,
                                null,
                                Inputty.GetString("Content type:", "text/plain", false));

                            if (debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false));
                            ms = new MemoryStream(data);
                            resp = req.Send(data.Length, ms);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                Console.WriteLine("Content:" + Environment.NewLine + resp.DataAsString);
                            }
                            break;

                        case "delete":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.Delete,
                                null,
                                Inputty.GetString("Content type:", "text/plain", false));

                            if (debug) req.Logger = Console.WriteLine;

                            data = Encoding.UTF8.GetBytes(Inputty.GetString("Data:", "Hello, world!", false));
                            ms = new MemoryStream(data);
                            resp = req.Send(data.Length, ms);
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                Console.WriteLine("Content:" + Environment.NewLine + resp.DataAsString);
                            }
                            break;

                        case "head":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.Head,
                                null,
                                null);

                            if (debug) req.Logger = Console.WriteLine;

                            resp = req.Send();
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                Console.WriteLine("Content:" + Environment.NewLine + resp.DataAsString);
                            }
                            break;

                        case "get":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://127.0.0.1:8000/", false),
                                HttpMethod.Get,
                                null,
                                null);

                            if (debug) req.Logger = Console.WriteLine;

                            resp = req.Send();
                            if (resp == null)
                            {
                                Console.WriteLine("Null response");
                            }
                            else
                            {
                                Console.WriteLine(resp.ToString());
                                Console.WriteLine("Content:" + Environment.NewLine + resp.DataAsString);
                            }
                            break;

                        case "query":
                            req = new RestRequest(
                                Inputty.GetString("URL:", "http://localhost:8888/?foo&bar=baz&another=value&key=val", false),
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
            Console.WriteLine("  query       Retrieve the query from a given URL");
            Console.WriteLine("  debug       Enable or disable console debugging (currently " + debug + ")");
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
    }
}
