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
                    string header = "[Client] ";
                    req.ChunkedTransfer = true;

                    Console.WriteLine(header + "Sending request");
                    for (int i = 0; i < 10; i++)
                        await req.SendChunkAsync("Chunk " + i.ToString() + Environment.NewLine, false);
                    Console.WriteLine(header + "Sent request");

                    using (RestResponse resp = await req.SendChunkAsync(Array.Empty<byte>(), true))
                    {
                        Console.WriteLine(header + "Waiting for response");
                        
                        if (resp.ChunkedTransferEncoding)
                        {
                            Console.WriteLine(header + "Chunked transfer encoding detected");

                            try
                            {
                                int chunkCount = 0;
                                while (true)
                                {
                                    ChunkData chunk = await resp.ReadChunkAsync();

                                    if (chunk == null)
                                    {
                                        Console.WriteLine(header + "No more chunks available");
                                        break;
                                    }

                                    chunkCount++;
                                    string chunkStr = "";
                                    if (chunk.Data != null && chunk.Data.Length > 0) chunkStr = Encoding.UTF8.GetString(chunk.Data);
                                    
                                    Console.WriteLine($"{header}Chunk {chunkCount}: {chunk.Data?.Length ?? 0} bytes (final: {chunk.IsFinal}): {chunkStr}");

                                    if (chunk.IsFinal) break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{header}Exception while reading chunks: {ex.GetType().Name}: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine(header + "Client received non-chunked data:");
                            Console.WriteLine(header + "Response: " + resp.ToString());
                            Console.WriteLine(resp.DataAsString);
                        }
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
            string header = "[Server] ";
            
            try
            {
                if (ctx.Request.ChunkedTransfer)
                {
                    Console.WriteLine(header + "Server received chunked data:");

                    while (true)
                    {
                        Chunk chunk = await ctx.Request.ReadChunk();
                        if (chunk.Data != null && chunk.Data.Length > 0)
                            Console.Write(header + Encoding.UTF8.GetString(chunk.Data));
                        else
                            Console.WriteLine(header + "(empty chunk)");

                        if (chunk.IsFinal)
                        {
                            Console.WriteLine(header + "Last chunk received");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine(header + "Server received non-chunked data: " + ctx.Request.DataAsString);
                }

                ctx.Response.ChunkedTransfer = true;
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/plain";

                for (int i = 0; i < 10; i++)
                {
                    byte[] data = Encoding.UTF8.GetBytes(("Chunk " + i.ToString() + Environment.NewLine));
                    await ctx.Response.SendChunk(data, false);
                    await Task.Delay(500);
                }

                await ctx.Response.SendChunk(Array.Empty<byte>(), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(header + "Webserver exception:" + Environment.NewLine + e.ToString());
            }
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}