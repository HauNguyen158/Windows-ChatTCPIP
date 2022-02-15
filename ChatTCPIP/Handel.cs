using Bunifu.UI.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatTCPIP
{
    class Handel
    {
        public string ImageToBase64(string path)
        {
            // string path = "D:\SampleImage.jpg";
            using (Image image = Image.FromFile(path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }
        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        //set icon
        public void setIcon(List<string> path, ImageList imageList, ListView view)
        {
            var files = Directory.GetFiles("icons", "*.*", SearchOption.AllDirectories);

            foreach ((string filename, int index) in files.Select((value, i) => (value, i)))
            {
                if (Regex.IsMatch(filename, @".jpg|.png|.gif$"))
                {
                    path.Add(filename);

                    imageList.Images.Add(Image.FromFile(filename));

                    view.Items.Add("", index);
                }
            }
            view.LargeImageList = imageList;
        }
    }
}
