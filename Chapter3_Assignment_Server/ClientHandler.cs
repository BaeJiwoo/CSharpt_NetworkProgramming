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


        public ClientHandler(Socket socket)
        {
            this.clientSocket = socket;

            IPEndPoint? ipEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"{ipEndPoint.Address}::{ipEndPoint.Port} 연결");

        }

        public void HandleClient()
        {
            // WhileLoop으로 유저 정보 처리

            while (isConnected)
            {
                byte[] recvByte= new byte[1024];

                int readn = clientSocket.Receive(recvByte);

                if (readn == 0) 
                {
                    Console.WriteLine("마 종료한다.");
                    break;
                }

                ProcessMessage(Encoding.UTF8.GetString(recvByte));
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
            if (isConnected) return;
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
    }
}
