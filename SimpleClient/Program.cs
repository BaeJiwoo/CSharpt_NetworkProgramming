using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class SimpleClient
{
    private static ushort port = 9000;
    private static bool isConnected = false;
    private static Socket? client = null;

    static void Main()
    {
        using (client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                Console.WriteLine("서버에 연결을 시도합니다...");
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                isConnected = true; // 연결 성공 시에만 true
            }
            catch (Exception ex)
            {
                Console.WriteLine($"연결 실패: {ex.Message}");
                Console.WriteLine("프로그램을 종료합니다.");
                return; // 연결 실패하면 더 이상 진행하지 않고 종료
            }

            // 1. 수신 스레드 생성 및 배경 스레드 설정 후 '시작'
            Thread recvThread = new Thread(() => ReceiveMessages(client));
            recvThread.IsBackground = true; // 메인 스레드 종료 시 함께 종료되도록 설정
            recvThread.Start();

            Console.WriteLine("서버에 연결되었습니다! 메시지를 입력하세요. (종료: /exit)");

            // 2. 메인 스레드는 '오직 키보드 입력 및 전송'만 담당
            while (isConnected)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                    continue;

                if (message.ToLower() == "/exit")
                {
                    isConnected = false;
                    break;
                }

                // 메시지 전송
                SendMessage(message);
            }

            // 안전하게 소켓 닫기
            CloseConnection();

            Console.WriteLine("프로그램을 종료하려면 아무 키나 누르세요...");
            Console.ReadKey();
        }
    }

    // 3. 수신 스레드는 '오직 서버 메시지 수신 및 화면 출력'만 담당
    static void ReceiveMessages(Socket socket)
    {
        try
        {
            while (isConnected)
            {
                byte[] bytes = new byte[1024];
                int readn = socket.Receive(bytes);

                if (readn == 0)
                {
                    Console.WriteLine("\n[안내] 서버와 연결이 끊어졌습니다.");
                    break;
                }

                // 받은 메시지에서 혹시 모를 개행 문자를 다듬어줍니다.
                string message = Encoding.UTF8.GetString(bytes, 0, readn).Trim();

                if (string.IsNullOrEmpty(message)) continue;

                // ★ 매번 From Server: 를 붙이지 않고, 서버가 보낸 메시지 내용 그대로 출력합니다.
                Console.WriteLine(message);
            }
        }
        catch
        {
            // 종료 시 예외 방어
        }
        finally
        {
            isConnected = false;
        }
    }

    static void SendMessage(string message)
    {
        if (client == null || !isConnected)
        {
            Console.WriteLine("연결이 되어 있지 않아용...");
            return;
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message.Replace("\r", "").Replace("\n", "").Trim());
            client.Send(bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"메시지 전송 실패: {ex.Message}");
        }
    }

    static void CloseConnection()
    {
        if (client != null)
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch { }
            finally
            {
                client.Close();
            }
        }
    }
}