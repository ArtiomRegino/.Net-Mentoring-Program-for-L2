using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_side
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("10.6.242.169"), 8025));

            serverSocket.Listen(13);


        }
    }
}
