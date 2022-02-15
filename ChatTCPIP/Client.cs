using Bunifu.UI.WinForms;
using Bunifu.UI.WinForms.BunifuButton;
using Newtonsoft.Json;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace ChatTCPIP
{
    public partial class Client : Form
    {
        [System.ComponentModel.Browsable(false)]
        public override bool AutoSize { get; set; }

        private SimpleTcpClient client = new SimpleTcpClient("127.0.0.1:9999");
        private SimpleTcpClient update = new SimpleTcpClient("127.0.0.1:10000");

        private bool sendAll = true;
        private BunifuPanel panelNow, panelNowLeft;
        private Panel panelEdit = null;

        private string img = null;
        private string file = null;
        private string fileN = null;
        private bool enter = false;
        private string myName = "";
        private string you = "";

        private List<Download> listDataDownload = new List<Download>();
        private List<string> pathIcon = new List<string>();
        private List<InfoClient> listInfo = new List<InfoClient>();

        private Handel handel = new Handel();

        private List<ShowMess> showMesses = new List<ShowMess>();

        public static Panel panel1;
        public static string trans;

        public Client()
        {
            InitializeComponent();
            panelNow = bunifuPanelTextMessenger;
            bunifuPanelShowListUser.Visible = false;

            handel.setIcon(pathIcon, imageList1, listViewEmoji);
            panel1 = panelSend;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            do {
               myName = Interaction.InputBox("Nhập tên của bạn?", "Nhập Tên", "");
            } while (myName == "");

            client.Events.DataReceived += Events_DataReceived;
            client.Connect();

            update.Events = client.Events;
            update.Connect();
            update.Send(myName);
        }
        
        private void addMyMess(BunifuPanel panelBoss, string data)
        {
            Messenger messenger = JsonConvert.DeserializeObject<Messenger>(data);
            ChatBoxMe chatBoxMe = new ChatBoxMe();

            Panel panel;
            if (messenger.file != null)
            {
                chatBoxMe.setFile(messenger.fileName, messenger.file, messenger.time);
                panel = chatBoxMe.getBoxFile();
            }
            else
            {
                if (messenger.image != null)
                    chatBoxMe.setImage(handel.Base64ToImage(messenger.image), messenger.time);
                else if (messenger.emoji != -1)
                {
                    Image img = imageList1.Images[messenger.emoji];
                    chatBoxMe.setImage(img, messenger.time);
                }
                else
                    chatBoxMe.setText(messenger.text, messenger.time);
                panel = chatBoxMe.getBox();
            }

            panelBoss.Controls.Add(panel);
            //thêm vào trên cùng nhưng sau item cuối cùng
            panel.BringToFront();
            //cuộn dưới cùng
            panelBoss.VerticalScroll.Value = panelBoss.VerticalScroll.Maximum;
        }


        private void addYouMess(BunifuPanel panelBoss, string text)
        {
            Messenger messenger = JsonConvert.DeserializeObject<Messenger>(text);
            Panel panel;
            ChatBoxYou chatBoxYou = new ChatBoxYou();
            //gửi file
            if (messenger.file != null)
            {
                chatBoxYou.setFile(messenger.name, messenger.fileName, messenger.file, messenger.time);
                panel = chatBoxYou.getBoxFile();
            }
            else
            {
                if (messenger.image != null)
                    chatBoxYou.setImage(messenger.name, handel.Base64ToImage(messenger.image), messenger.time);
                else if (messenger.emoji != -1)
                {
                    Image img = imageList1.Images[messenger.emoji];
                    chatBoxYou.setImage(messenger.name, img, messenger.time);
                }
                else
                {
                    chatBoxYou.setInfoText(messenger.name, messenger.text, messenger.time);
                    if (messenger.trans)
                        chatBoxYou.setFly(messenger.name);
                }
                panel = chatBoxYou.getBox();
            }

            //nếu chat tổng thì in tên
            if (!messenger.all)
                chatBoxYou.sendPrivate();

            panelBoss.Controls.Add(panel);

            //thêm vào trên cùng nhưng sau item cuối cùng
            panel.BringToFront();
            //cuộn dưới cùng
            panelBoss.VerticalScroll.Value = panelBoss.VerticalScroll.Maximum;
        }

        private void openPanelChat(string name, string port)
        {
            bunifuPanelTextMessenger.Visible = false;

            BunifuPanel panel = new BunifuPanel();
            panel.AutoScroll = true;
            panel.BackgroundColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));

            panel.BorderColor = Color.White;
            panel.Dock = DockStyle.Fill;
            panel.ShowBorders = true;
            panel.Padding = new Padding(0, 50, 0, 50);

            bunifuPanelMessenger.Controls.Add(panel);

            //ô thông tin người nhận
            BunifuPanel infoUser = new BunifuPanel()
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 0, 0, 5),
            };
            bunifuPanelListInbox.Controls.Add(infoUser);
            Button button = new Button()
            {
                Height = 35,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopLeft,
                Text = name,
                Font = new Font("Segoe UI", 14F)
            };

            Label labelText = new Label()
            {
                Text = "",
                Dock = DockStyle.Bottom,
                Font = new Font("Segoe UI", 7F),
                BackColor = Color.Transparent
            };
            infoUser.Controls.Add(button);
            infoUser.Controls.Add(labelText);

            /*infoUser.Controls.Add(labelName);
            */
            ShowMess show = new ShowMess()
            {
                panel = panel,
                panelClick = infoUser,
                port = port,
                name = name,
                button = button
            };
            panelNow.Visible = false;
            showMesses.Add(show);
            panelNow = panel;
            panelNowLeft = infoUser;
            labelName.Text = name;
            button.Click += infoUser_Click;

        }


        //thêm tin nhắn vào thanh bên trái
        private void addMessLeft(BunifuPanel infoUser, string data, bool my)
        {
            infoUser.SendToBack();
            Messenger messenger = JsonConvert.DeserializeObject<Messenger>(data);

            Label label = (Label)infoUser.Controls[1];
            if (my)
            {
                label.Enabled = false;
                if (messenger.emoji != -1)
                    label.Text = "Bạn đã gửi 1 emoji";
                else if (messenger.image != null)
                    label.Text = "Bạn đã gửi 1 ảnh";
                else
                    label.Text = $"Bạn : {messenger.text}";
            }
            else
            {
                label.Enabled = true;
                if (messenger.emoji != -1)
                    label.Text = "Gửi 1 emoji";
                else if (messenger.image != null)
                    label.Text = "Gửi 1 ảnh";
                else
                    label.Text = messenger.text;
            }


            label.Font = new Font(label.Font, FontStyle.Bold);
        }

        private void infoUser_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            ShowMess item = showMesses.Find(showMesses => showMesses.button == button);
            setInbox(item);

        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(rtbTextMessenger.Text))
            {
                Messenger messenger = setSmsText(sendAll);
                client.Send(JsonConvert.SerializeObject(messenger));
                addMyMess(panelNow, JsonConvert.SerializeObject(messenger));

                if (!sendAll)
                    addMessLeft(panelNowLeft, JsonConvert.SerializeObject(messenger), true);

                img = null;
                rtbTextMessenger.SelectAll();
            }
        }

        private void addList(string data)
        {
            try
            {
                //thêm 1 client nếu sai thì thêm list
                InfoClient info = JsonConvert.DeserializeObject<InfoClient>(data);
                //true là thêm
                if (info.type == 1)
                {
                    listViewUser.Items.Add(info.name);
                    checkedListBox1.Items.Add(info.name);
                    listInfo.Add(info);
                }
                else if (info.type == 2)
                {
                    listInfo.Remove(listInfo.Find(listInfo => listInfo.port == info.port));

                    listViewUser.Items.Clear();
                    checkedListBox1.Items.Clear();
                    foreach (InfoClient value in listInfo)
                    {
                        listViewUser.Items.Add(value.name);
                        checkedListBox1.Items.Add(value.name);
                    }
                }
                else
                {
                    sendAll = false;
                    you = info.port;
                    openPanelChat(info.name, info.port);
                }
            }
            catch
            {
                listInfo = JsonConvert.DeserializeObject<List<InfoClient>>(data);
                foreach (InfoClient info in listInfo)
                {
                    listViewUser.Items.Add(info.name);
                    checkedListBox1.Items.Add(info.name);
                }

            }
            numberClient.Text = (listInfo.Count() + 1).ToString();
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if (update == (SimpleTcpClient)(sender))
                {
                    try
                    {
                        Messenger messenger = JsonConvert.DeserializeObject<Messenger>(Encoding.UTF8.GetString(e.Data));

                        ShowMess item = showMesses.Find(showMesses => showMesses.port == messenger.receiver);

                        if (item != null)
                        {
                            addMessLeft(item.panelClick, Encoding.UTF8.GetString(e.Data), false);
                            addYouMess(item.panel, Encoding.UTF8.GetString(e.Data));
                        }
                        else
                            addList(Encoding.UTF8.GetString(e.Data));
                    }
                    catch
                    {
                        addList(Encoding.UTF8.GetString(e.Data));
                    }
                }
                else
                {
                    addYouMess(bunifuPanelTextMessenger, Encoding.UTF8.GetString(e.Data));
                }
            });
        }

        private void rtbTextMessenger_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                rtbTextMessenger.ResetText();
                enter = false;
            }
        }

        private void rtbTextMessenger_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            if (enter)
            {
                ((RichTextBox)sender).Height = e.NewRectangle.Height + 5;
                ((RichTextBox)sender).ScrollToCaret();
                ((RichTextBox)sender).SelectionStart = ((RichTextBox)sender).Text.Length;
            }
        }


        private void rtbTextMessenger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                if (!string.IsNullOrEmpty(rtbTextMessenger.Text))
                {
                    Messenger messenger = setSmsText(sendAll);
                    client.Send(JsonConvert.SerializeObject(messenger));
                    addMyMess(panelNow, JsonConvert.SerializeObject(messenger));

                    if (!sendAll)
                        addMessLeft(panelNowLeft, JsonConvert.SerializeObject(messenger), true);

                    img = null;
                    rtbTextMessenger.SelectAll();
                }
            }

            else if (e.KeyCode == Keys.Enter && e.Shift)
                enter = true;
        }

        private void setInbox(ShowMess show)
        {
            panelNow.Visible = false;

            panelNow = show.panel;
            panelNowLeft = show.panelClick;
            panelNow.Visible = true;
            you = show.port;
            sendAll = false;
            labelName.Text = show.name;
        }
        private void listViewUser_DoubleClick(object sender, EventArgs e)
        {

            int index = listViewUser.SelectedItems[0].Index;
            //đã chat riêng trước đó
            ShowMess item = showMesses.Find(showMesses => showMesses.port == listInfo[index].port);

            if (item != null)
            {
                setInbox(item);
            }
            else
            {
                string name = listInfo[index].port;
                you = name;

                openPanelChat(listInfo[index].name, name);

                InfoClient info = new InfoClient()
                {
                    port = "-1",
                    name = name,
                    type = 3
                };
                setInbox(showMesses[showMesses.Count - 1]);

                update.Send(JsonConvert.SerializeObject(info));
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {

            update.Send(rtbTextMessenger.Text);
            myName = rtbTextMessenger.Text;
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            panelNow.Visible = false;
            panelNow = bunifuPanelTextMessenger;
            panelNow.Visible = true;
            sendAll = true;
            labelName.Text = "Kênh Chat Tổng";
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            // image filters  
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                Clipboard.SetImage(Image.FromFile(open.FileName));
                img = handel.ImageToBase64(open.FileName);

                rtbTextMessenger.Paste();
            }
        }

        private void listViewEmoji_Click(object sender, EventArgs e)
        {
            int index = listViewEmoji.SelectedItems[0].Index;
            Messenger messenger = setSmsEmoji(sendAll, index);
            client.Send(JsonConvert.SerializeObject(messenger));
            addMyMess(panelNow, JsonConvert.SerializeObject(messenger));

            if (!sendAll)
                addMessLeft(panelNowLeft, JsonConvert.SerializeObject(messenger), true);
        }

        
        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                try
                {
                    ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        byte[] bytes = File.ReadAllBytes(ofd.FileName);

                        Messenger messenger = setSmsFile(sendAll, bytes, ofd.SafeFileName);
                        addMyMess(panelNow, JsonConvert.SerializeObject(messenger));
                        client.Send(JsonConvert.SerializeObject(messenger));
                        
                        if (!sendAll)
                            addMessLeft(panelNowLeft, JsonConvert.SerializeObject(messenger), true);
                    }
                }
                catch { }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            byte[] bytes = Convert.FromBase64String(file);

            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            File.WriteAllBytes($"{download}/{fileN}", bytes);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i) == true)
                {
                    //đã chat riêng trước đó
                    ShowMess item = showMesses.Find(showMesses => showMesses.port == listInfo[i].port);

                    if (item != null)
                    {
                        setInbox(item);
                    }
                    else
                    {
                        string name = listInfo[i].port;
                        you = name;

                        openPanelChat(listInfo[i].name, name);

                        InfoClient info = new InfoClient()
                        {
                            port = "-1",
                            name = name,
                            type = 3
                        };
                        setInbox(showMesses[showMesses.Count - 1]);

                        update.Send(JsonConvert.SerializeObject(info));
                    }

                    Messenger messenger = new Messenger()
                    {
                        text = trans,
                        name = myName,
                        receiver = you,
                        all = false,
                        time = DateTime.Now,
                        image = null,
                        emoji = -1,
                        file = null,
                        trans = true
                    };
                    client.Send(JsonConvert.SerializeObject(messenger));

                    img = null;

                    addMyMess(panelNow, JsonConvert.SerializeObject(messenger));

                }
            }
            panelSend.Visible = false;
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            bunifuPanelShowListUser.Visible = !bunifuPanelShowListUser.Visible;
            labelName.Text = "Chat Tổng";
        }

        private Messenger setSmsText(bool all)
        {
            Messenger messenger = new Messenger()
            {
                text = rtbTextMessenger.Text,
                name = myName,
                receiver = (all == true ? "" : you),
                all = all,
                time = DateTime.Now,
                image = null,
                emoji = -1,
                file = null,
                trans = false
            };
            if (img != null)
                messenger.image = img;
            return messenger;
        }

        private Messenger setSmsEmoji(bool all, int index)
        {
            Messenger messenger = new Messenger()
            {
                text = "",
                name = myName,
                receiver = (all == true ? "" : you),
                all = all,
                time = DateTime.Now,
                image = null,
                emoji = index,
                file = null,
                trans = false
            };
            return messenger;
        }

        private Messenger setSmsFile(bool all, byte[] bytes, string fileName)
        {
            Messenger messenger = new Messenger()
            {
                text = "",
                name = myName,
                receiver = (all == true ? "" : you),
                all = all,
                time = DateTime.Now,
                image = null,
                emoji = -1,
                file = Convert.ToBase64String(bytes),
                fileName = fileName,
                trans = false
            };
            return messenger;
        }

        //sự kiện 
   
        private void bunifuImageButton3_Click(object sender, EventArgs e)
        => listViewEmoji.Visible = true;
        private void bunifuImageButton3_Leave(object sender, EventArgs e)
        => listViewEmoji.Visible = false;

    }
}
