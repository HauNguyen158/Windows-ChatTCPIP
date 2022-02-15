using Bunifu.UI.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatTCPIP
{
    public partial class ChatBoxYou : UserControl
    {
        public ChatBoxYou()
        {
            InitializeComponent();
        }
        public void setInfoText(string name, string text, DateTime time)
        {
            labelName.Text = name;
            labelText.Text = text;
            labelTime.Text = time.ToString("HH:mm");
        }
        public void setImage(string name, Image image, DateTime time)
        {
            labelText.Text = "";
            labelName.Text = name;
            labelText.Image = image;
            labelText.MinimumSize = new Size(image.Width, image.Height);
            labelTime.Text = time.ToString("HH:mm");
        }

        public void sendPrivate()
        {
            boxText.Controls.Remove(labelName);
        }
        //chuyển tiếp
        public void setFly(string name)
        {
            labelName.Text = $"{name} đã chuyển tiếp tin nhắn";
        }

        public void setFile(string nameSend, string name, string data, DateTime time)
        {
            labelName2.Text = nameSend;
            linkLabel.Text = name;
            linkLabel.Links[0].LinkData = data;
            labelTime2.Text = time.ToString("HH:mm");
        }

        public Panel getBox()
        {
            return boxText;
        }
        public Panel getBoxFile()
        {
            return boxFile;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string name = ((LinkLabel)sender).Text;
            string data = ((LinkLabel)sender).Links[0].LinkData.ToString();

            byte[] bytes = Convert.FromBase64String(data);

            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            File.WriteAllBytes($"{download}/{name}", bytes);
            bunifuSnackbar.Show(Client.ActiveForm, "Tải file thành công",
            BunifuSnackbar.MessageTypes.Success, 3000, "",
            BunifuSnackbar.Positions.BottomLeft);

        }
    }
}
