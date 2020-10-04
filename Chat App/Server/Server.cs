using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        string[,] IP_PK;
        int index;
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            IP_PK = new string[10, 2];
            index = 0;
            Connect();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(txbMessage.Text == "fuck")
            {
                AddMessage("list client: ");
                AddMessage(clientList.Count().ToString());
                for(int i = 0;i< clientList.Count;i++)
                {
                    AddMessage(clientList[i].RemoteEndPoint.ToString());
                }
                AddMessage("end");
            }    
            else
            {
                foreach (Socket item in clientList)
                {
                    Send(item);
                }
                AddMessage(txbMessage.Text);
                txbMessage.Clear();
            }    
            
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;
        void Connect()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            server.Bind(IP);
            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);
                        Thread recieve = new Thread(Receive);
                        recieve.IsBackground = true;
                        recieve.Start(client);
                        foreach(Socket item in clientList)
                        {
                            AddMessage(item.RemoteEndPoint.ToString());
                            
                        }
                        /*foreach (Socket item in clientList)
                        {
                            if (item != null)
                            {
                                Send(item, item.RemoteEndPoint.ToString());
                            }
                        }*/


                        /*foreach (Socket item in clientList)
                        {
                            AddMessage(item.RemoteEndPoint.ToString());
                        }*/
                        SendOnlineList();
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }
        void SendOnlineList()
        {
            int count = clientList.Count;
            if (count > 1)
            {
                string msg;
                /*for (int i = 0;i< clientList.Count;i++)
                {
                    msg = "server clear";
                    Send(clientList[i], msg);
                } */
                /*for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (clientList[i] != null && clientList[j] != null)
                        {
                            msg = "server " + clientList[j].RemoteEndPoint.ToString();
                            Send(clientList[i], msg);
                        }
                    }
                }*/
                /*AddMessage("Start1");
                foreach (Socket ski in clientList)
                {
                    foreach (Socket skj in clientList)
                    {
                        *//*if (ski != null && skj != null)
                        {
                            msg = "server " + skj.RemoteEndPoint.ToString();
                            Send(ski, msg);
                        }*//*
                        AddMessage("server => "+ ski.RemoteEndPoint + " = " + skj.RemoteEndPoint);

                        //Send(ski, "server " + skj.RemoteEndPoint);
                    }

                }
                AddMessage("end1");
                AddMessage("Start2");*/
                foreach (Socket ski in clientList)
                {
                    foreach(Socket skj in clientList)
                    {
                        /*if (ski != null && skj != null)
                        {
                            msg = "server " + skj.RemoteEndPoint.ToString();
                            Send(ski, msg);
                        }*/
                        AddMessage("server " + skj.RemoteEndPoint);

                        Send(ski, "server " + skj.RemoteEndPoint);
                    }
                    
                }
                //AddMessage("End2");
            }
        }
        void Close()
        {
            server.Close();
        }
        void Send(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
        }
        void Send(Socket client, string msg)
        {
            if (client != null && msg != string.Empty)
                client.Send(Serialize(msg));
        }
        
        void Receive(object obj)
        {
            Socket client = obj as Socket;

            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                        
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                    
                    string endpoint = message.Split('|')[1].Trim();
                    if (endpoint == "all")
                    {
                        IP_PK[index, 0] = message.Split('|')[0].Trim();
                        IP_PK[index, 1] = message.Split('|')[2].Trim();
                        index++;
                        AddMessage("Start");
                        foreach (Socket item in clientList)
                        {
                            for(int i = 0;i<index;i++)
                            {
                                if (item != null)
                                {
                                    AddMessage("server >>"+item.RemoteEndPoint + IP_PK[i, 0] + "|all|" + IP_PK[i, 1]);
                                    item.Send(Serialize(IP_PK[i, 0] + "|all|" + IP_PK[i, 1]));
                                }    
                                    
                            }    
                            
                        }
                        AddMessage("end");
                    }
                    else
                    {
                        foreach (Socket item in clientList)
                        {
                            /*if (item != null && item != client)
                            item.Send(Serialize(message));*/
                            if (item.RemoteEndPoint.ToString() == endpoint)
                                item.Send(Serialize(message));

                        }
                    }    
                        

                    AddMessage("client " + client.RemoteEndPoint.ToString() + " " + message);
                }
            }
            catch
            {
                 clientList.Remove(client);
                 client.Close();
            }
            
        }
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
        }
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
            //return stream.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lsvMessage.Clear();
        }
    }
}
