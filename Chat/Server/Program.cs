using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

namespace Server
{
    class Program
    {
        public static void StartListening(int port)
        {
            // Привязываем сокет ко всем интерфейсам на текущей машинe
            IPAddress ipAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // CREATE
            Socket listener = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // BIND
                listener.Bind(localEndPoint);

                // LISTEN
                listener.Listen(10);

                const string EndDataPostfix = "<EOF>";
                var history = new List<string>();

                while (true)
                {
                    // ACCEPT
                    Socket handler = listener.Accept();

                    byte[] buf = new byte[1024];
                    string data = null;

                    while (true)
                    {
                        // RECEIVE
                        int bytesRec = handler.Receive(buf);

                        data += Encoding.UTF8.GetString(buf, 0, bytesRec);

                        if (data.IndexOf(EndDataPostfix) > -1)
                        {
                            break;
                        }
                    }

                    data = data.Remove(data.Length - EndDataPostfix.Length, EndDataPostfix.Length);

                    history.Add(data);
                    Console.WriteLine($"Message received: {data}");

                    // Отправляем текст обратно клиенту
                    string historyAsJson = JsonSerializer.Serialize(history);
                    byte[] msg = Encoding.UTF8.GetBytes(historyAsJson);

                    // SEND
                    handler.Send(msg);

                    // RELEASE
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Invalid argument count. Usage: <port>");
            }

            StartListening(int.Parse(args[0]));
        }
    }
}