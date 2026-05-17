using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chapter3_Assignment_Server
{
    class ClientHandler
    {
        private Socket clientSocket;

        string nickname;

        bool isConnected;

        Thread mThread;

        public ClientHandler(Socket socket)
        {
            this.clientSocket = socket;

            IPEndPoint? ipEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"{ipEndPoint.Address}::{ipEndPoint.Port} 연결");

            isConnected = true;

            mThread = new Thread(() => HandleClient());
            //mThread.IsBackground = true;
            mThread.Start();
        }

        public void HandleClient()
        {
            try
            {
                // \n >> 를 제거하고 명확한 한 줄 문자열로 전송합니다.
                SendMessage("닉네임을 입력하세요.");

                byte[] recvByte = new byte[1024];
                int readn = clientSocket.Receive(recvByte);

                if (readn == 0) return;

                // 클라이언트가 보낸 닉네임에서 엔터(\r, \n)를 완벽히 제거
                nickname = Encoding.UTF8.GetString(recvByte, 0, readn).Replace("\r", "").Replace("\n", "").Trim();

                Program.BroadcastMessage($"[시스템] {nickname}님이 입장하셨습니다.", this);

                while (isConnected)
                {
                    recvByte = new byte[1024];
                    readn = clientSocket.Receive(recvByte);

                    if (readn == 0) break;

                    // 메시지에서도 엔터 공백 제거
                    string message = Encoding.UTF8.GetString(recvByte, 0, readn).Replace("\r", "").Replace("\n", "").Trim();

                    // 빈 메시지는 무시
                    if (string.IsNullOrEmpty(message)) continue;

                    ProcessMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[에러] {nickname} 통신 에러: {ex.Message}");
            }
            finally
            {
                Program.BroadcastMessage($"[시스템] {nickname}님이 퇴장하셨습니다.", this);
                Program.RemoveClient(this);
            }
        }

        public void ProcessMessage(string message)
        {
            // 메시지 오면 다른 클라들에게 BroadCast...
            if(message.StartsWith("/nick"))
            {
                string newNickname = message.Substring(6).Trim();
                if (!string.IsNullOrEmpty(newNickname))
                {
                    string oldNickName = nickname;
                    nickname = newNickname;

                    SendMessage($"닉넴 {newNickname}으로 바꾸기완료");

                    Program.BroadcastMessage($"닉네임이 {oldNickName}에서 {newNickname}(으)로 변경되었습니다.", this);
                }
            }
            else
            {
                Program.BroadcastMessage($"[{nickname}] {message}", this);
            }
        }

        public void SendMessage(string message)
        {
            if (!isConnected) return;
            // 서버가 클라에게 메시지 전송
            byte[] bytes= Encoding.UTF8.GetBytes(message);
            clientSocket.Send(bytes);
        }

        public void CloseConnection()
        {
            // 걍 닫기

            if(clientSocket != null)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException ex)
                {

                }
                finally
                {
                    clientSocket.Close();
                }

                Console.WriteLine($"클라이언트 연결 종료: {nickname}");
            }
        }

        public void EndClient()
        {
            if (isConnected && mThread.IsAlive)
            {
                mThread.Join();
                CloseConnection();
            }
        }
    }
}
