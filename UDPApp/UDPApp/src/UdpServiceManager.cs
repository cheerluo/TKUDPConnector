using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UdpServiceManager
{
    //接收数据事件
    public Action<byte[]> recvDataEvent = null;
    //发送结果事件
    public Action<int> sendResultEvent = null;
    //日志事件
    public Action<string> logger = null;
    
    //本地port
    private int localPort = 0;
    private IPEndPoint groupEP = null;

    //网络连接
    private UdpClient udpClient = null;
    
    //初始化
    public UdpServiceManager(int localPort)
    {
        if (localPort < 0 || localPort > 65535)
            throw new ArgumentOutOfRangeException("localPort is out of range");        
        this.localPort = localPort;
        groupEP = new IPEndPoint(IPAddress.Any, localPort);
        
    }

    //启动网络服务
    public void Start()
    {
        while (true)
        {
            try
            {
                //udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(localHost), localPort));//绑定本地host和port
                udpClient = new UdpClient(localPort, AddressFamily.InterNetwork);//指定本地监听port
                UdpState udpState = new UdpState(udpClient, groupEP);
                udpClient.BeginReceive(ReceiveCallback, udpState);
                
                break;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
            }
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            UdpState udpState = ar.AsyncState as UdpState;
            if (udpState != null)
            {
                UdpClient udpClient = udpState.UdpClient;

                IPEndPoint ip = udpState.IP;
                Byte[] receiveBytes = udpClient.EndReceive(ar, ref ip);
                if (recvDataEvent != null)
                    recvDataEvent(receiveBytes);
                udpClient.BeginReceive(ReceiveCallback, udpState);//在这里重新开始一个异步接收，用于处理下一个网络请求
            }
        }
        catch (Exception ex)
        {
            //处理异常
        }
    }

//     //接收消息数据
//     private void ReceiveData()
//     {
//         while (true)
//         {
//             if (udpClient == null)
//                 return;
// 
//             try
//             {
//                 byte[] data = udpClient.Receive(ref groupEP);
//                 if (recvDataEvent != null)
//                     recvDataEvent(data);
//             }
//             catch (Exception)
//             {
//             }
//         }
//     }

    //发送消息,支持广播地址类似255.255.255.255/192.168.1.255等
    public void SendData(byte[] buffer, string destHost, int destPort)
    {
        if (buffer == null)
            throw new ArgumentNullException("message cant not null at SendMessage");
        if (string.IsNullOrEmpty(destHost))
            throw new ArgumentNullException("destHost cant not null");
        if (destPort < 0 || destPort > 65535)
            throw new ArgumentOutOfRangeException("destPort is out of range");
        if (udpClient == null)
            throw new ArgumentNullException("udpClient cant not null");
        
        int len = 0;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                len = udpClient.Send(buffer, buffer.Length, destHost, destPort);
                //len = await udpClient.SendAsync(buffer, buffer.Length, destHost, destPort);
            }
            catch (Exception)
            {
                len = 0;
            }

            if (len <= 0)
                Thread.Sleep(100);
            else
                break;
        }

        if (sendResultEvent != null)
            sendResultEvent(len);
    }
    

    //关闭连接，清理资源
    public void Close()
    {
        if (udpClient == null) return;

        try
        {
            if (udpClient.Client != null)
            {
                udpClient.Client.Shutdown(SocketShutdown.Both);
            }
            else
            {
                if (logger != null) logger("udpClient.Client is null at Close");
            }
                
        }
        catch (Exception e)
        {
            if(logger != null) logger("Exception at Close:"+e.ToString());
        }
        udpClient.Close();
        udpClient = null;
    }


    public class UdpState
    {
        private UdpClient udpclient = null;

        public UdpClient UdpClient
        {
            get { return udpclient; }
        }

        private IPEndPoint ip;

        public IPEndPoint IP
        {
            get { return ip; }
        }

        public UdpState(UdpClient udpclient, IPEndPoint ip)
        {
            this.udpclient = udpclient;
            this.ip = ip;
        }
    }

}