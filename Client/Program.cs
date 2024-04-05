using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;

        public Client(string serverIp, int serverPort)
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }

        public void SendRequest()
        {
            while (true)
            {
                Console.Write("Enter your request: ");
                string request = Console.ReadLine();

                var requestData = Encoding.ASCII.GetBytes(request);
                udpClient.Send(requestData, requestData.Length, serverEndPoint);

                var serverResponseData = udpClient.Receive(ref serverEndPoint);
                var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                Console.WriteLine("Server > " + serverResponse);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Client client = new Client("127.0.0.1", 8080);
            client.SendRequest();
        }
    }
}
