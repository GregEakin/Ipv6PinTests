using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace PinUnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            const string input = "ff02::1";

            Assert.True(IPAddress.TryParse(input, out var address));
            Assert.AreEqual(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        [Test]
        public void Test2()
        {
            // Nonroutable. Documentation purposes only [RFC 3849]
            const string input = "2001:DB8::";

            Assert.True(IPAddress.TryParse(input, out var address));
            Assert.AreEqual(AddressFamily.InterNetworkV6, address.AddressFamily);
        }

        [Test]
        public void Test3()
        {
            Assert.True(Socket.OSSupportsIPv6);
        }

        // [Test]
        public void Server()
        {
            const int port = 1979;

            using var listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(IPAddress.IPv6Any, port);
            listener.Bind(endPoint);
            listener.Listen(0);

            using var socket = listener.Accept();
            listener.Close();

            while (true)
            {
                var b = new byte[11];
                var len = socket.Receive(b);
                if (len == 0)
                    break;

                Console.WriteLine("RX: " + Encoding.ASCII.GetString(b, 0, len));
            }

            socket.Close();
        }

        // [Test]
        public void Clent()
        {
            const int port = 1979;
            const string ipAddress = "fe80::4cff:fe4f:4f50";

            var address = IPAddress.Parse(ipAddress);
            var endPoint = new IPEndPoint(address, port);
            using var connection = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            connection.Connect(endPoint);

            var b = Encoding.ASCII.GetBytes("hello world");
            for (var x = 0; x < 10; x++)
            {
                Console.WriteLine("TX: " + Encoding.ASCII.GetString(b));
                connection.Send(b);

                Thread.Sleep(100);
            }

            connection.Close();
        }

        [Test]
        public async Task Test6()
        {
            var hostname = "localhost";
            var port = 80;
            var timeout = 200;
            var ct = new CancellationToken();

            using var tcpClient = new TcpClient();
            var connection = tcpClient.ConnectAsync(hostname, port);
            var whenAny = await Task.WhenAny(connection, Task.Delay(timeout, ct));
            Assert.AreNotEqual(connection, whenAny);
            Assert.IsNull(connection.Exception);
            tcpClient.Close();
        }
    }

    // netsh interface ipv6 show address
}