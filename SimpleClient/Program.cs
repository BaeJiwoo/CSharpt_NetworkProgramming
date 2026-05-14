using System.Net;
using System.Net.Sockets;
using System.Text;

class SimpleClient
{
    private static ushort port= 9000;
    static void Main()
    {
        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                while (true)
                {
                    Console.Write("전송할 메시지 (종료하려면 'exit' 입력): ");
                    string? message = Console.ReadLine();

                    if (string.IsNullOrEmpty(message) || message.ToLower() == "exit")
                    {
                        break;
                    }

                    byte[] sendData = Encoding.UTF8.GetBytes(message);

                    client.Send(sendData);

                    byte[] receiveData = new byte[1024];
                    int bytesRead = client.Receive(receiveData);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("서버가 연결을 종료했습니다.");
                        break;
                    }

                    string response = Encoding.UTF8.GetString(receiveData);
                    Console.WriteLine(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }

            
        }
    }
}