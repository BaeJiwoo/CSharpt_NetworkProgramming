using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Chapter3_Assignment_Server
{
    class Program
    {
        private static readonly List<ClientHandler> clients;

        private static readonly object clientsLock = new object();

        static void Main()
        {

        }

        public static void BroadcastMessage(string message, ClientHandler sender)
        {
            lock (clientsLock)
            {
                foreach (ClientHandler client in clients)
                {
                    if (client == sender) continue;

                    try
                    {
                        client.SendMessage(message);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine($"소켓 전송 오류: {ex.Message}");
                    }
                }
            }
            
        }

        public static void RemoveClient(ClientHandler handler)
        {
            lock (clientsLock)
            {
                if (clients.Contains(handler))
                {
                    clients.Remove(handler);
                    Console.WriteLine($"클라이언트 제거 완료. 현재 클라이언트 수 {clients.Count}");
                }
            }
        }

    }
}