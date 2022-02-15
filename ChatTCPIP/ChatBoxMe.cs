using Bunifu.UI.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatTCPIP
{
    public partial class ChatBoxMe : UserControl
    {
        public ChatBoxMe()
        {
            InitializeComponent();
            Image img = Image.FromFile("icons/icondelete.jpg");
            
            imageList1.Images.Add(img);

            labelRemove.Image = imageList1.Images[0];
        }
        public Panel getBox()
        {
            return boxText;
        }
        public Panel getBoxFile()
        {
            return boxFile;
        }

        public void setText(string text, DateTime time)
        {
            labelText.Text = text;
            labelTime.Text = time.ToString("HH:mm");
        }
        public void setImage(Image image, DateTime time)
        {
            labelText.Text = "";
            labelText.Image = image;
            labelText.MinimumSize = new Size(image.Width, image.Height);
            labelTime.Text = time.ToString("HH:mm");
        }
        public void setFile(string name, string data, DateTime time)
        {
            linkLabel.Text = name;
            linkLabel.Links[0].LinkData = data;
            labelTime2.Text = time.ToString("HH:mm");
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string name = ((LinkLabel)sender).Text;
            string data = ((LinkLabel)sender).Links[0].LinkData.ToString();

            byte[] bytes = Convert.FromBase64String(data);

            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            File.WriteAllBytes($"{download}/{name}", bytes);
            bunifuSnackbar1.Show(Client.ActiveForm, "Tải file thành công",
            BunifuSnackbar.MessageTypes.Success, 3000, "",
            BunifuSnackbar.Positions.BottomLeft);

        }

        private void panelEdit_MouseHover(object sender, EventArgs e)
        {
        }

        private void panelEdit_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void bunifuPanel1_Click(object sender, EventArgs e)
        {
            labelRemove.Visible = true;
            labelTrans.Visible = true;
        }

        private void labelRemove_Click(object sender, EventArgs e)
        {
            labelText.Text = "Tin nhắn đã thu hồi";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Client.panel1.Visible = true;
            Client.trans = labelText.Text;
        }
    }
}
