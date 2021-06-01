using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chain
{
    class Program
    {
        private struct Address
        {
            public static readonly string localhost = "localhost";
        }

        private static void StartProcess(int listeningPort, string nextHost, int nextPort, bool isInitiator)
        {
            var ipAddress = Address.localhost != nextHost ? IPAddress.Parse(nextHost) : IPAddress.Loopback;
            var remoteEP = new IPEndPoint(ipAddress, nextPort);

            var sender = PrepareSender(ipAddress);
            var listener = PrepareListener(listeningPort);

            var numberX = int.Parse(Console.ReadLine());

            ConnectSender(sender, remoteEP);

            var handler = listener.Accept();

            if (isInitiator)
            {
                SendNumber(numberX, sender);

                numberX = ReceiveNumber(handler);

                Console.WriteLine(numberX);

                SendNumber(numberX, sender);
            }
            else
            {
                var numberY = ReceiveNumber(handler);

                var maxNumber = Math.Max(numberX, numberY);
                SendNumber(maxNumber, sender);

                numberY = ReceiveNumber(handler);

                Console.WriteLine(numberY);

                SendNumber(numberY, sender);
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private static void SendNumber(int number, Socket sender)
        {
            sender.Send(BitConverter.GetBytes(number));
        }

        private static int ReceiveNumber(Socket handler)
        {
            var buf = new byte[sizeof(int)];
            handler.Receive(buf);

            return BitConverter.ToInt32(buf);
        }

        private static Socket PrepareListener(int port)
        {
            var ipAddress = IPAddress.Any;

            var localEndPoint = new IPEndPoint(ipAddress, port);

            var listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);

            return listener;
        }

        private static Socket PrepareSender(IPAddress ipAddress)
        {
            var sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            return sender;
        }

        private static void ConnectSender(Socket sender, IPEndPoint endPoint)
        {
            while (true)
            {
                try
                {
                    sender.Connect(endPoint);
                    return;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("Invalid arguments count.");

                return;
            }

            var listeningPort = int.Parse(args[0]);
            var nextHost = args[1];
            var nextPort = int.Parse(args[2]);
            var isInitiator = args.Length == 4 ? bool.Parse(args[3]) : false;

            StartProcess(listeningPort, nextHost, nextPort, isInitiator);

            Console.ReadKey();
        }
    }
}
