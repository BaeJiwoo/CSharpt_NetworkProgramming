using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class SocketLifeCycleExample
{
    private static bool isConnected = false;

    private static ushort port;

    static void Main()
    {
        port = 9000;
        Socket mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            mListener.Bind(new IPEndPoint(IPAddress.Any, port));
            mListener.Listen(10);

            Console.WriteLine("서버가 시작되었습니다. 클라이언트 연결 대기 중...");

            Socket clientSocket = mListener.Accept();

            isConnected = true;

            IPEndPoint? clientEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"클라이언트 연결됨: {clientEndPoint?.Address}:{clientEndPoint?.Port}");

            Thread heartbeatThread = new(() => HeartBeatCheck(clientSocket));
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();

            try
            {
                while (isConnected)
                {
                    byte[] buffer = new byte[1024];

                    try
                    {
                        int bytesRead = clientSocket.Receive(buffer);

                        if (bytesRead == 0)
                        {
                            Console.WriteLine("클라이언트가 연결을 종료했습니다.");
                            break;
                        }

                        string message = Encoding.UTF8.GetString(buffer);
                        Console.WriteLine($"수신한 메시지: {message}");

                        clientSocket.Send(buffer);

                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"소켓 오류: {ex.Message}");
                        break;
                    }
                }
            }
            finally
            {
                isConnected = false;
                CleanupSocket(clientSocket);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"서버 오류: {ex.Message}");
        }
        finally
        {
            mListener.Close();
        }
    }

    static void HeartBeatCheck(Socket socket)
    {
        while (isConnected)
        {
            try
            {
                if (socket.Poll(0, SelectMode.SelectWrite))
                {
                    Console.WriteLine($"소켓({socket}): 활성");
                }
                else
                {
                    Console.WriteLine($"소켓({socket}): 비활성");
                    isConnected = false;
                    break;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"소켓({socket}) 하트비트 체크 false");
                isConnected = false;
                break;
            }

            Thread.Sleep(5000);
        }
    }

    static void CleanupSocket(Socket socket)
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch(SocketException ex)
        {

        }
        finally
        {
            socket.Close();
        }

        Console.WriteLine("소켓 연결이 정리되었습니다.");
    }
}