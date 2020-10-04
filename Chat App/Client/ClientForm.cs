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

namespace Client
{
    public partial class ClientForm : Form
    {
        RSA_Crypto RSA_Crypto;
        string[,] IP_PK;
        int index;
        public ClientForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            RSA_Crypto = new RSA_Crypto();
            address = new List<string>();
            IP_PK = new string[10, 2];
            index = 0;

            Connect();
            
            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            
            if(txbMessage.Text == "fuck")
            {
                AddMessage("index = " + index);
                for(int i = 0;i<index;i++)
                {
                    AddMessage(IP_PK[i, 0] + " - " + IP_PK[i, 1]);
                }    
            }   
            else
            {
                Send();
                AddMessage(txbMessage.Text);
            }    
        }

        IPEndPoint IP;
        Socket client;
        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                client.Connect(IP);   
            }
            catch
            {
                MessageBox.Show("Khong the ket noi server!", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
            SendPublicKey();
            this.Text = "Client " + client.LocalEndPoint.ToString();
        }
        void Close()
        {
            client.Close();
        }
        void Send()
        {
            if(txbMessage.Text != string.Empty && 
                cmbIP.SelectedItem.ToString() != string.Empty &&
                cmbIP.SelectedItem.ToString() != "none")
            {
                string publickey = "";
                for (int i = 0; i < index; i++)
                {
                    if (cmbIP.SelectedItem.ToString() == IP_PK[i, 0])
                    {
                        publickey = IP_PK[i, 1];
                        break;
                    }
                }
                string cypher = Convert.ToBase64String(RSA_Crypto.Encrypt(txbMessage.Text, publickey));
                //client.Send(Serialize(client.LocalEndPoint.ToString() + "|" + cmbIP.SelectedItem.ToString() + "| " + txbMessage.Text));
                client.Send(Serialize(client.LocalEndPoint.ToString() + "|" + cmbIP.SelectedItem.ToString() + "| " + cypher));
            }        
        }

        void SendPublicKey()
        {
            string s = RSA_Crypto.PublicKeyString();
            client.Send(Serialize(client.LocalEndPoint.ToString() + "|all|"+ s));
            //client.Send(Serialize(RSA_Crypto.PublicKeyString()));
            /*client.Send(Serialize("hello"));*/
        }

        List<string> address;
        
        void Receive()
        {
            try
            {
                bool check;
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                      
                    if (message.Contains("server") == true)
                    {
                        message = message.Replace("server ", "");
                        check = false;
                        if(message != client.LocalEndPoint.ToString())
                        {
                            foreach (string str in address)
                            {
                                if (str == message)
                                {
                                    check = true;
                                    break;
                                }
                            }
                            if (check == false)
                                address.Add(message);
                            BindingSource bs = new BindingSource();
                            bs.DataSource = address;
                            cmbIP.DataSource = bs;
                        }
                    }
                    else if(message.Contains("all") == true)
                    {  
                        string sendPoint = message.Split('|')[0];
                        bool checkD = false;
                        for(int i = 0;i<index;i++)
                        {
                            if(IP_PK[i,0] == sendPoint)
                            {
                                checkD = true;
                                break;
                            }    
                        }    
                        if(sendPoint != client.LocalEndPoint.ToString() && checkD == false)
                        {
                            IP_PK[index, 0] = message.Split('|')[0];
                            IP_PK[index, 1] = message.Split('|')[2];
                            index++;
                            
                        }
                        //AddMessage(message);
                    }    
                    else
                    {
                        string str = RSA_Crypto.Decrypt(Convert.FromBase64String(message.Split('|')[2]));
                        AddMessage(str);
                    }    
                }
            }
            catch
            {
                Close();
            }
        }
        void AddMessage(string s)
        {
            s = s.Replace("|", ">>");
            //s = s.Replace(client.LocalEndPoint.ToString(), "you");
            lsvMessage.Items.Add(new ListViewItem() { Text = s }) ;  
            txbMessage.Clear();
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
        private void txbMessage_TextChanged(object sender, EventArgs e)
        {

        }

        private void lsvMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
    }
}
