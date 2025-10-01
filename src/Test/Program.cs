namespace Test
{
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

    class Program
    {
        static Action<string> _Logger = Console.WriteLine;
        static int _TimeoutMs = 10000;
        static string _Username = null;
        static string _Password = null;
        static string _BearerToken = null;
        static bool _Encode = true;
        static DefaultSerializationHelper _Serializer = new DefaultSerializationHelper();

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
                            await GetResponse(url, HttpMethod.Put, contentType, body);
                            break;

                        case "post":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            body = Inputty.GetString("Body:", null, true);
                            await GetResponse(url, HttpMethod.Post, contentType, body);
                            break;

                        case "form":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            form = Inputty.GetDictionary<string, string>("Key:", "Val:");
                            await GetResponseWithForm(url, HttpMethod.Post, contentType, form);
                            break;

                        case "del":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            contentType = Inputty.GetString("Content-Type:", null, true);
                            body = Inputty.GetString("Body:", null, true);
                            await GetResponse(url, HttpMethod.Delete, contentType, body);
                            break;

                        case "head":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            await GetResponse(url, HttpMethod.Head);
                            break;

                        case "get":
                            url = Inputty.GetString("URL:", "http://localhost:8888/", true);
                            if (String.IsNullOrEmpty(url)) break;
                            await GetResponse(url, HttpMethod.Get);
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
                Console.WriteLine(e.ToString());
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

        static async Task GetResponse(string url, HttpMethod method, string contentType = null, string body = null)
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

                RestResponse resp = null;
                if (String.IsNullOrEmpty(body)) resp = await req.SendAsync();
                else resp = await req.SendAsync(body);

                if (resp == null)
                {
                    Console.WriteLine("*** Null response");
                    return;
                }

                Console.WriteLine("");
                Console.WriteLine(resp.ToString());
                Console.WriteLine("");

                if (resp.ServerSentEvents)
                {
                    while (true)
                    {
                        ServerSentEvent sse = await resp.ReadEventAsync();
                        if (sse != null)
                            Console.WriteLine(_Serializer.SerializeJson(sse, false));
                        else
                            return;
                    }
                }
                else if (resp.ChunkedTransferEncoding)
                {
                    while (true)
                    {
                        ChunkData chunk = await resp.ReadChunkAsync();
                        if (chunk == null) break;
                        if (chunk.Data != null) Console.WriteLine(Encoding.UTF8.GetString(chunk.Data));
                        if (chunk.IsFinal) break;
                    }
                    Console.WriteLine("");
                }
                else
                {
                    if (!String.IsNullOrEmpty(resp.DataAsString)) Console.WriteLine(resp.DataAsString);
                    else Console.WriteLine("(null)");
                }

                resp.Dispose();
            }
        }

        static async Task GetResponseWithForm(string url, HttpMethod method, string contentType, Dictionary<string, string> form)
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

                RestResponse resp = null;
                if (form == null) resp = await req.SendAsync();
                else resp = await req.SendAsync(form);

                if (resp == null)
                {
                    Console.WriteLine("*** Null response");
                    return;
                }

                Console.WriteLine("");
                Console.WriteLine(resp.ToString());
                Console.WriteLine("");

                if (resp.ServerSentEvents)
                {
                    while (true)
                    {
                        ServerSentEvent sse = await resp.ReadEventAsync();
                        if (sse != null)
                            Console.WriteLine(_Serializer.SerializeJson(sse, false));
                        else
                            return;
                    }
                }
                else if (resp.ChunkedTransferEncoding)
                {
                    while (true)
                    {
                        byte[] bytes = await ReadStreamFullyAsync(resp.Data, 64, true);
                        if (bytes == null) break;
                    }
                    Console.WriteLine("");
                }
                else
                {
                    if (!String.IsNullOrEmpty(resp.DataAsString)) Console.WriteLine(resp.DataAsString);
                    else Console.WriteLine("(null)");
                }

                resp.Dispose();
            }
        }

        static async Task<byte[]> ReadStreamFullyAsync(Stream stream, int bufferSize, bool logToConsole)
        {
            if (bufferSize <= 0) throw new ArgumentException("Buffer size must be greater than zero", nameof(bufferSize));

            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);

                    if (logToConsole)
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.Write(chunk);
                    }
                }

                if (bytesRead > 0) return memoryStream.ToArray();
                return null;
            }
        }
    }
}
