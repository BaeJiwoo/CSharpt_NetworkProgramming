using System.Net;
using System.Net.Sockets;
using System.Text;

class SimpleServer
{
    static void Main()
    {
        ClientHandler clientHandler = new ClientHandler();

        int port = 8888;
        Socket mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        mListener.Bind(new IPEndPoint(IPAddress.Any, port));

        mListener.Listen(5);


        while (true)
        {
            Socket? clientSocket = mListener.Accept();
            if(clientSocket != null)
            {
                clientHandler.AddClient(clientSocket);
            }
        }
    }
}

public class ClientHandler
{
    List<Socket> _ClientSocketList = new List<Socket>();

    private object obj = new object();

    public void AddClient(Socket? newSocket)
    {
        if(newSocket != null)
        {
            lock (obj)
            {
                _ClientSocketList.Add(newSocket);

                Thread newWorkerThread = new Thread(() => WorkerThread(newSocket));
                newWorkerThread.IsBackground = true;
                newWorkerThread.Start();
            }
        }
    }

    private void WorkerThread(Socket socket)
    {
        socket.Send(Encoding.UTF8.GetBytes("Welcome!!"));

        while (true)
        {
            byte[] buffer = new byte[1024];

            int readSize = socket.Receive(buffer);
            if(readSize == 0)
            {
                Console.WriteLine("클라이언트 연결 종료");
            }



            Console.WriteLine(buffer);
        }
    }
}

