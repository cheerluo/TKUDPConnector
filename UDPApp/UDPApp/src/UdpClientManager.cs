// using System;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using System.Threading;
// 
// public class UdpClientManager
// {
//     //接收数据事件
//     public Action<string> recvMessageEvent = null;
//     //发送结果事件
//     public Action<int> sendResultEvent = null;
//     //本地监听端口
//     public int localPort = 0;
// 
//     private UdpClient udpClient = null;
// 
//     public UdpClientManager(int localPort)
//     {
//         if (localPort < 0 || localPort > 65535)
//             throw new ArgumentOutOfRangeException("localPort is out of range");
// 
//         this.localPort = localPort;
//     }
// 
//     public void Start()
//     {
//         while (true)
//         {
//             try
//             {
//                 Console.WriteLine("UdpClient create");
//                 udpClient = new UdpClient(localPort, AddressFamily.InterNetwork);//指定本地监听port
//                 Console.WriteLine("UdpClient ReceiveMessage");
//                 ReceiveMessage();
//                 break;
//             }
//             catch (Exception)
//             {
//                 Thread.Sleep(100);
//             }
//         }
//     }
// 
//     private async void ReceiveMessage()
//     {
//         while (true)
//         {
//             if (udpClient == null)
//                 return;
// 
//             try
//             {
//                 Console.WriteLine("UdpClient ReceiveMessage ReceiveAsync");
//                 UdpReceiveResult udpReceiveResult = await udpClient.ReceiveAsync();
//                 string message = Encoding.UTF8.GetString(udpReceiveResult.Buffer);
//                 if (recvMessageEvent != null)
//                     recvMessageEvent(message);
//             }
//             catch (Exception ex)
//             {
//             }
//         }
//     }
// 
//     //单播
//     public async void SendMessageByUnicast(string message, string destHost, int destPort)
//     {
//         if (string.IsNullOrEmpty(message))
//             throw new ArgumentNullException("message cant not null");
//         if (udpClient == null)
//             throw new ArgumentNullException("udpClient cant not null");
//         if (string.IsNullOrEmpty(destHost))
//             throw new ArgumentNullException("destHost cant not null");
//         if (destPort < 0 || destPort > 65535)
//             throw new ArgumentOutOfRangeException("destPort is out of range");
// 
//         byte[] buffer = Encoding.UTF8.GetBytes(message);
//         int len = 0;
//         for (int i = 0; i < 3; i++)
//         {
//             try
//             {
//                 len = await udpClient.SendAsync(buffer, buffer.Length, new IPEndPoint(IPAddress.Parse(destHost), destPort));
//             }
//             catch (Exception)
//             {
//                 len = 0;
//             }
// 
//             if (len <= 0)
//                 Thread.Sleep(100);
//             else
//                 break;
//         }
// 
//         if (sendResultEvent != null)
//             sendResultEvent(len);
//     }
// 
//     public void CloseUdpCliend()
//     {
//         if (udpClient == null)
//             throw new ArgumentNullException("udpClient cant not null");
// 
//         try
//         {
//             udpClient.Client.Shutdown(SocketShutdown.Both);
//         }
//         catch (Exception)
//         {
//         }
//         udpClient.Close();
//         udpClient = null;
//     }
// }