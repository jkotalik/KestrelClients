using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KestrelClients
{
    class Program
    {

        public static string url = "http://localhost:61397/";
        public int port = 61397;
        public static void Main(string[] args)
        {
            TestMinResponseDataRate();
            Console.ReadLine();
        }


        // For max client connections and upgraded client connections
        public static async void TestMaxClientConnections()
        {
            var limit = 10;


            try
            {

                for (var i = 0; i < limit; i++)
                {
                    var client = new HttpClient();
                    for (int j = 0; j < 5; j++)
                    {
                        var content = new StringContent("asdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdf");
                        var res = await client.PostAsync(url, content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Limit Hit: {ex.Message}");
            }
        }

        public static async void TestMaxBodySize()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Transfer-Encoding", "chunked");

                var fileStream = File.Open("companies.json", FileMode.Open);
                var content = new StreamContent(fileStream);
                var res = await client.PostAsync(url, content);
                // Success!
                var helloWorld = await res.Content.ReadAsStringAsync();
                Console.WriteLine(helloWorld);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Limit Hit: {ex.Message}");
            }
        }


        public static void GetClientWithSocketConnection()
        {
            try
            {
                var ipAddress = IPAddress.Parse("127.0.0.1");
                var ipe = new IPEndPoint(ipAddress, 61397);
                var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = 1;
                socket.Connect(ipe);

                var bytesToSend = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + "localhost" + "\r\nConnection: Close\r\n\r\n");
                socket.Send(bytesToSend, bytesToSend.Length, 0);

                int bytes = 0;
                var bytesReceived = new byte[1];

                while ((bytes = socket.Receive(bytesReceived, 1, 0)) > 0)
                {
                    Console.WriteLine($"Read a piece of data {(char)bytesReceived[0]}");
                    Task.Delay(TimeSpan.FromSeconds(1));
                }
                Console.WriteLine("Done reading stream");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Limit Hit: {ex.Message}");

            }
        }

        public static void TestMinResponseDataRate()
        {
            try
            {
                var ipAddress = IPAddress.Parse("127.0.0.1");
                var ipe = new IPEndPoint(ipAddress, 61397);
                var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = 1;
                socket.Connect(ipe);

                var bytesToSend = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + "localhost" + "\r\n\r\n");
                socket.Send(bytesToSend, bytesToSend.Length, 0);

                int bytes = 0;
                var bytesReceived = new byte[1];

                while ((bytes = socket.Receive(bytesReceived, 1, 0)) > 0)
                {
                    Console.WriteLine($"Read a piece of data {(char)bytesReceived[0]}");
                    Thread.Sleep(100);
                }
                Console.WriteLine("Done reading stream");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Limit Hit: {ex.Message}");
            }
        }

        public static void TestMinRequestBodyDataRate()
        {
            while (true)
            {
                try
                {
                    var ipAddress = IPAddress.Parse("127.0.0.1");
                    var ipe = new IPEndPoint(ipAddress, 61397);
                    var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // Making buffer size 1 to trigger slow requests
                    socket.Connect(ipe);
                    socket.NoDelay = true;
                    socket.SendTimeout = 1000;

                    var testString = "hello world";
                    var request = $"POST / HTTP/1.0\r\n" +
                        $"Content-Length: {testString.Length}" +
                        "\r\n\r\n" +
                        testString;

                    Console.WriteLine(DateTimeOffset.UtcNow);
                    int bytes = 0;
                    var bytesToSend = Encoding.ASCII.GetBytes(request);
                    while ((bytes += socket.Send(bytesToSend, bytes, 1, SocketFlags.None)) < bytesToSend.Length)
                    {
                        Console.WriteLine($"Sent a byte of data to server {(char)bytesToSend[bytes - 1]}");
                        Thread.Sleep(100);
                    }

                    Console.WriteLine("Successfully sent data to server");

                    var buffer = new byte[1];
                    while (socket.Receive(buffer) != 0)
                    {
                        Console.Write((char)buffer[0]);
                        Console.Out.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" {ex.GetType()} {ex.Message}");
                }
                Console.ReadLine();
            }
        }
    }
}
