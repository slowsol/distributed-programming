using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

namespace Client
{
    class Program
    {
        private struct Address
        {
            public static readonly string localhost = "localhost";
        }

        public static void StartClient(string address, int port, string message)
        {
            try
            {
                // Разрешение сетевых имён
                IPAddress ipAddress = Address.localhost != address ? IPAddress.Parse(address) : IPAddress.Loopback;

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // CREATE
                Socket sender = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    // Подготовка данных к отправке
                    const string EndDataPostfix = "<EOF>";
                    byte[] msg = Encoding.UTF8.GetBytes($"{message}{EndDataPostfix}");

                    // SEND
                    int bytesSent = sender.Send(msg);

                    // RECEIVE
                    byte[] buf = new byte[1024];

                    var history = new List<string>();
                    string data = null;

                    while (true)
                    {
                        int bytesRec = sender.Receive(buf);
                        data += Encoding.UTF8.GetString(buf, 0, bytesRec);

                        try
                        {
                            history = JsonSerializer.Deserialize<List<string>>(data);
                            break;
                        }
                        catch { }
                    }

                    foreach (var item in history)
                    {
                        Console.WriteLine(item);
                    }

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Invalid argument count. Usage: <address> <port> <message>");
            }

            StartClient(args[0], int.Parse(args[1]), args[2]);
        }
    }
}