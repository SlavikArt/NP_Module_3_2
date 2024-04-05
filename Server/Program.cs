using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class Server
    {
        private UdpClient udpServer;
        private IPEndPoint endPoint;
        private Dictionary<string, List<DateTime>> clientRequestTimes;
        private object lockObject = new object();

        public Server(int port)
        {
            udpServer = new UdpClient(port);
            endPoint = new IPEndPoint(IPAddress.Any, port);
            clientRequestTimes = new Dictionary<string, List<DateTime>>();
        }

        public void Start()
        {
            try
            {
                while (true)
                {
                    var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var clientRequest = Encoding.ASCII.GetString(
                        udpServer.Receive(ref clientEndPoint));

                    lock (lockObject)
                    {
                        if (!clientRequestTimes.ContainsKey(clientEndPoint.ToString()))
                            clientRequestTimes[clientEndPoint.ToString()] = new List<DateTime>();

                        var requestTimes = clientRequestTimes[clientEndPoint.ToString()];
                        requestTimes.Add(DateTime.Now);

                        CleanOldRequests(clientEndPoint.ToString());

                        if (requestTimes.Count > 10)
                        {
                            var msg = Encoding.ASCII.GetBytes(
                                "The maximum number of requests per hour has been exceeded.");
                            udpServer.Send(msg, msg.Length, clientEndPoint);
                            continue;
                        }
                    }

                    var response = ProcessRequest(clientRequest);
                    var responseMsg = Encoding.ASCII.GetBytes(response);
                    udpServer.Send(responseMsg, responseMsg.Length, clientEndPoint);
                    Console.WriteLine($"'{clientRequest}' at {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }

        private void CleanOldRequests(string clientEndPoint)
        {
            var requestTimes = clientRequestTimes[clientEndPoint];
            requestTimes.RemoveAll(time => (DateTime.Now - time).TotalHours > 1);
        }

        private Dictionary<string, Dictionary<string, double>> prices = new()
        {
            { "cpu", new Dictionary<string, double> {
                { "AMD Ryzen 5 5600X Processor", 150 },
                { "Intel Core i7-12700KF", 300 },
                { "Intel Core i9-10900K", 500 } } },
            { "ram", new Dictionary<string, double> {
                { "Corsair Vengeance LPX 16GB", 80 },
                { "G.SKILL Trident Z RGB 16GB", 90 },
                { "Kingston HyperX Fury 8GB", 40 } } },
            { "gpu", new Dictionary<string, double> {
                { "NVIDIA GeForce RTX 3080", 700 },
                { "AMD Radeon RX 6800 XT", 650 },
                { "NVIDIA GeForce RTX 3090", 1500 } } },
            { "ssd", new Dictionary<string, double> {
                { "Samsung 970 EVO Plus 1TB", 150 },
                { "Crucial MX500 1TB", 100 },
                { "Kingston A2000 NVMe PCIe M.2 1TB", 120 } } },
            { "hdd", new Dictionary<string, double> {
                { "Seagate BarraCuda 2TB", 55 },
                { "WD Blue 1TB", 45 },
                { "Toshiba P300 3TB", 75 } } },
            { "motherboard", new Dictionary<string, double> {
                { "ASUS ROG Strix B450-F Gaming", 130 },
                { "MSI MPG X570 GAMING PLUS", 160 },
                { "GIGABYTE B450M DS3H", 70 } } },
            { "psu", new Dictionary<string, double> {
                { "EVGA 600 W1, 80+ WHITE 600W", 50 },
                { "Corsair CX Series CX450", 45 },
                { "Seasonic FOCUS Plus 650 Gold", 100 } } }
        };

        private string ProcessRequest(string request)
        {
            var parts = request.Split(' ');
            if (parts.Length == 2 && parts[0].ToLower() == "price")
            {
                var component = parts[1].ToLower();
                if (prices.ContainsKey(component))
                {
                    var models = prices[component];
                    var response = new StringBuilder($"Prices for {component} models:\n");
                    foreach (var model in models)
                        response.AppendLine($"- {model.Key}, Price: {model.Value}");
                    return response.ToString();
                }
                else
                    return $"No price information for {component}";
            }
            else
                return "Invalid request. Please type 'price [component]'";
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(8080);
            server.Start();
        }
    }
}
