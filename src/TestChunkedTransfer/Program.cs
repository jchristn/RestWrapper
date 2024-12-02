namespace TestChunkedTransfer
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using RestWrapper;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    public static class Program
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        private static string _Hostname = "localhost";
        private static int _Port = 8000;
        private static WebserverSettings _WebserverSettings = null;
        private static Webserver _Webserver = null;

        public static async Task Main(string[] args)
        {
            try
            {
                _WebserverSettings = new WebserverSettings(_Hostname, _Port, false);
                _Webserver = new Webserver(_WebserverSettings, DefaultRoute);
                _Webserver.Start();

                Console.WriteLine("Webserver started on " + _Hostname + ":" + _Port);

                await Task.Delay(1000);

                using (RestRequest req = new RestRequest("http://localhost:8000", System.Net.Http.HttpMethod.Post))
                {
                    req.ChunkedTransfer = true;

                    for (int i = 0; i < 10; i++)
                        await req.SendChunkAsync("Chunk " + i.ToString() + Environment.NewLine, false);

                    using (RestResponse resp = await req.SendChunkAsync(Array.Empty<byte>(), true))
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Response: " + resp.ToString());

                        if (resp.ChunkedTransferEncoding)
                        {
                            Console.WriteLine("Client received chunked data:");
                        }
                        else
                        {
                            Console.WriteLine("Client received non-chunked data:");
                        }

                        Console.WriteLine(resp.DataAsString);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + Environment.NewLine + e.ToString());
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            try
            {
                if (ctx.Request.ChunkedTransfer)
                {
                    Console.WriteLine("Server received chunked data:");

                    while (true)
                    {
                        Chunk chunk = await ctx.Request.ReadChunk();
                        if (chunk.Data != null && chunk.Data.Length > 0)
                            Console.Write(Encoding.UTF8.GetString(chunk.Data));
                        else
                            Console.WriteLine("(empty chunk)");

                        if (chunk.IsFinal)
                        {
                            Console.WriteLine("Last chunk received");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Server received non-chunked data: " + ctx.Request.DataAsString);
                }

                ctx.Response.ChunkedTransfer = true;
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/plain";

                for (int i = 0; i < 10; i++)
                {
                    byte[] data = Encoding.UTF8.GetBytes(("Chunk " + i.ToString() + Environment.NewLine));
                    await ctx.Response.SendChunk(data);
                }

                await ctx.Response.SendFinalChunk(Array.Empty<byte>());
            }
            catch (Exception e)
            {
                Console.WriteLine("Webserver exception:" + Environment.NewLine + e.ToString());
            }
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}