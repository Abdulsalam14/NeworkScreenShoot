using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenShoot_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[]? imagebuffer;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            var ip = IPAddress.Parse("127.0.0.1");
            var port = 27001;
            var remoteEP = new IPEndPoint(ip, port);

            var size = 0;
            var buffer = Array.Empty<byte>();

            buffer = new byte[1] { 4 };
            await client.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);
            buffer = new byte[ushort.MaxValue - 29];
            var result = await client.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);
            size = result.ReceivedBytes;
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                str.Append(((char)buffer[i]));
            }


            var imagesize = int.Parse(str.ToString());
            imagebuffer = new byte[imagesize];
            size = 0;
            var receivesize = 0;
            var index = 0;

            while (imagesize > size)
            {
                buffer = new byte[1];
                await client.SendToAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);
                buffer = new byte[ushort.MaxValue - 29];
                result = await client.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEP);
                receivesize = result.ReceivedBytes;
                buffer.ToList().CopyTo(0, imagebuffer, index, receivesize);
                size += receivesize;
                index = size - 1;
            }
            BuildImage(imagebuffer);

        }

        public void BuildImage(byte[] imagebuffer)
        {
            MemoryStream ms = new MemoryStream(imagebuffer);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            screenimage.Source = image;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (screenimage.Source is not null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "PNG Files(*.png) | *.png";

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialog.FileName, imagebuffer);
                    MessageBox.Show("Image Saved");
                    screenimage.Source = null;
                }
            }
            else
            {
                MessageBox.Show("No image");
            }
        }
    }
}
