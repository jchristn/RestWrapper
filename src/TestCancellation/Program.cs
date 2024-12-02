namespace TestCancellation
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using GetSomeInput;
    using RestWrapper;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    public static class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Webserver server = new Webserver(
                new WatsonWebserver.Core.WebserverSettings
                {
                    Hostname = "localhost",
                    Port = 8000

                }, DefaultRoute);

            server.Start();

            Task unawaited = Task.Run(() => RequestSender(token), token);

            string userInput = Inputty.GetString("Press ENTER to cancel", null, true);
            cts.Cancel();

            Console.WriteLine("Press ENTER to cancel");
            Console.ReadLine();
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        private static async Task RequestSender(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(1000);
                    using (RestRequest req = new RestRequest("http://localhost:8000", System.Net.Http.HttpMethod.Get))
                    {
                        req.Logger = Console.WriteLine;
                        Console.WriteLine("[RequestSender] sending request");
                        using (RestResponse resp = await req.SendAsync(token))
                        {
                            Console.WriteLine("[RequestSender] response received");
                        }
                    }
                }
            }   
            catch (Exception e)
            {
                Console.WriteLine("[RequestSender] exception: " + e.Message);
            }
        }

        private static async Task DefaultRoute(HttpContextBase context)
        {
            Console.WriteLine("[Webserver] in route");
            await Task.Delay(10000);
            Console.WriteLine("[Webserver] sending response");
            await context.Response.Send();
        }
    }
}