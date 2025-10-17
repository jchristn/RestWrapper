namespace TestServerSentEvents
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
        private static int _Port = 8080;
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

                using (RestRequest req = new RestRequest("http://" + _Hostname + ":" + _Port.ToString()))
                {
                    using (RestResponse resp = await req.SendAsync())
                    {
                        Console.WriteLine("Status: " + resp.StatusCode);

                        if (resp.ServerSentEvents)
                        {
                            while (true)
                            {
                                RestWrapper.ServerSentEvent ev = await resp.ReadEventAsync();
                                if (ev == null) break;
                                else
                                {
                                    if (!String.IsNullOrEmpty(ev.Data))
                                    {
                                        Console.WriteLine($"| ID: {ev.Id}, Data: {ev.Data}");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
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
            Console.WriteLine("Received request using version " + ctx.Request.ProtocolVersion);

            ctx.Response.StatusCode = 200;
            ctx.Response.ServerSentEvents = true;

            int events = 10;

            for (int i = 1; i <= events; i++)
            {
                await Task.Delay(250);
                await ctx.Response.SendEvent(new WatsonWebserver.Core.ServerSentEvent
                {
                    Id = i.ToString(),
                    Data = "Event " + i.ToString()
                }, (i == events));
            }
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}