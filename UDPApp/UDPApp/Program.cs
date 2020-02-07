using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main start");

            string localHost = "192.168.1.6";
            int localPort_server=8082;
            UdpServiceManager udpServiceManager = new UdpServiceManager(localPort_server);
            udpServiceManager.recvDataEvent = (str) => { Console.WriteLine("Server get:" + BitConverter.ToString(str)); };
            //udpServiceManager.sendResultEvent = (len) => { Console.WriteLine("Server send len:" + len); };
            udpServiceManager.Start();

            int localPort_client = 8081;
            int lastflag = -1;
            UdpServiceManager udpClientManager = new UdpServiceManager(localPort_client);
            udpClientManager.recvDataEvent = (str) => {
                Console.WriteLine("Client get:" + BitConverter.ToString(str));
                if (str[0] <= lastflag) Console.WriteLine("Client get seq ERROR");
                lastflag = str[0];
            };
            //udpClientManager.sendResultEvent = (len) => { Console.WriteLine("Client send len:" + len); };
            udpClientManager.Start();

            for(int i=0;i<100;i++)
            {
                udpServiceManager.SendData(new byte[] { (byte)i, 2, 3, 4, 5 }, localHost, localPort_client);
            }
            //Thread.Sleep(1000);


            //Thread.Sleep(1000);
            //udpClientManager.SendData(new byte[] { 11, 22, 33, 44 }, localHost, localPort_server);


            Console.WriteLine("Main end");
            Thread.Sleep(50 * 1000);
        }
    }
}
