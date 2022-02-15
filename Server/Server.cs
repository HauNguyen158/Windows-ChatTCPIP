using Bunifu.UI.WinForms;
using Newtonsoft.Json;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Server
{
    public partial class Server : Form
    {
        SimpleTcpServer server = new SimpleTcpServer("127.0.0.1:9999");
        SimpleTcpServer update = new SimpleTcpServer("127.0.0.1:10000");

        private List<InfoClient> listClient = new List<InfoClient>();
        private List<string> listClientInbox = new List<string>();
        public Server()
        {
            InitializeComponent();
            
        }

        private void Server_Load(object sender, EventArgs e)
        {
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.DataReceived += Events_DataReceived;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Start();

            update.Events = server.Events;
            update.Start();

        }

        private void sendClient(SimpleTcpServer server, byte[] data, string clientNow)
        {
            Messenger messenger = JsonConvert.DeserializeObject<Messenger>(Encoding.UTF8.GetString(data));
            if(messenger.text.Trim().Length != 0)
                richTextBox1.Text += $"{clientNow} : {messenger.text}\n";
            
 

                if (messenger.all)
            {
                foreach (string client in server.GetClients())
                {
                    if (clientNow != client)
                        server.Send(client, Encoding.UTF8.GetString(data));
                }
            }
            else
            {
                int index = listClientInbox.FindIndex(listClientInbox => listClientInbox == clientNow);
                string port = messenger.receiver;

                messenger.receiver = listClient[index].port;

                this.update.Send(port, JsonConvert.SerializeObject(messenger));
            }
              
        }
        private void UpdateClient(SimpleTcpServer server, string data, string port)
        {
            try
            {
                InfoClient info = JsonConvert.DeserializeObject<InfoClient>(data);
                openServer(server, port, info.name);
                
            }
            catch
            {
                InfoClient info = new InfoClient()
                {
                    port = port,
                    name = data,
                    type = 1
                };
                string item = JsonConvert.SerializeObject(info);
                string list = JsonConvert.SerializeObject(listClient);
                //TRẢ VỀ DANH SÁCH CÁC Client trước đó
                foreach (string client in server.GetClients())
                {
                    if (client != port)
                        server.Send(client, item);
                    else
                        server.Send(client, list);
                }
                listClient.Add(info);
            }
        }
        private void removeClient(SimpleTcpServer server, string port)
        {
            try
            {
                InfoClient info = listClient.Find(listClient => listClient.port == port);
                listClientInbox.RemoveAt(listClient.FindIndex(listClient => listClient.port == port));
                listClient.Remove(info);

                info.type = 2;

                string item = JsonConvert.SerializeObject(info);

                foreach (string client in server.GetClients())
                    if (client != port)
                        server.Send(client, item);
            }
            catch { }

        }
        private void openServer(SimpleTcpServer server, string port, string you)
        {
       
            int i = listClient.FindIndex(listClient => listClient.port == port);
            string name = listClient[i].name;
            richTextBox1.Text += port + "\n";

            InfoClient info1 = new InfoClient()
                {
                    port = port,
                    name = name,
                    type = 3
                };
                
                string item = JsonConvert.SerializeObject(info1);
                server.Send(you, item);
        }

        private void Events_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if (update == (SimpleTcpServer)sender)
                    removeClient(update, e.IpPort);
                else
                {
                    checkedListBox1.Items.Remove(e.IpPort);
                    richTextBox1.Text += $"{e.IpPort} ngắt kết nối\n";
                    listClientInbox.Remove(e.IpPort);
                }
             /*   else
                    richTextBox1.Text += $"{e.IpPort} ngắt kết nối\n";*/
                
            });
        }
       
        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if (update == (SimpleTcpServer)sender)
                    UpdateClient(update, Encoding.UTF8.GetString(e.Data), e.IpPort);
                else
                    sendClient((SimpleTcpServer)sender, e.Data, e.IpPort);
            });
        }

        private void Events_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if ((SimpleTcpServer)sender == server)
                {
                    listClientInbox.Add(e.IpPort);
                    richTextBox1.Text += $"{e.IpPort} kết nối\n";
                    checkedListBox1.Items.Add(e.IpPort);
                }
                    
            });
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbSend.Text))
            {
                richTextBox1.Text += rtbSend.Text;

                Messenger messenger = new Messenger()
                {
                    text = rtbSend.Text,
                    name = "SERVER",
                    receiver = "",
                    all = true,
                    time = DateTime.Now,
                    image = null,
                    emoji = -1,
                    file = null,
                    trans = false
                };
                int sum = 0;
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i) == true)
                    {
                        sum++;
                        server.Send(listClientInbox[i], JsonConvert.SerializeObject(messenger));
                    }
                }
                if(sum == 0)
                    foreach(string client in server.GetClients())
                        server.Send(client, JsonConvert.SerializeObject(messenger));
            }
  
        }

    }
    public class InfoClient
    {
        public string port { set; get; }
        public string name { set; get; }
        public int type { set; get; }
    }
    public class Messenger
    {
        public string image { set; get; }
        public string name { set; get; }
        public string text { set; get; }
        public string receiver { set; get; }
        public bool all { set; get; }
        public DateTime time { set; get; }
        public int emoji { set; get; }
        public string file { set; get; }
        public string fileName { set; get; }
        public bool trans { set; get; }
    }
    public class RemoveSms
    {
        public BunifuPanel panel { set; get; }
        public string receiver { set; get; }
        public bool all { set; get; }
    }
}
