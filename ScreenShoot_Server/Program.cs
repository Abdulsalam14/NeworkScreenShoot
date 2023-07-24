using System.Runtime.InteropServices;
using System.Drawing;
using System.Net;
using System.Net.Sockets;


byte[] GetScreenshot()
{
    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;


    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
    int screenHeight = GetSystemMetrics(SM_CYSCREEN);

    Bitmap Image = new Bitmap(screenWidth, screenHeight);

    Size s = new Size(Image.Width, Image.Height);


    using (Graphics memoryGraphics = Graphics.FromImage(Image))
    {
        memoryGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        memoryGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        memoryGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

        using (MemoryStream ms = new MemoryStream())
        {
            Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            Image.Dispose();
            return ms.ToArray();
        }
    }
}





var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

var Ip = IPAddress.Parse("127.0.0.1");
var port = 27001;

var endpoint = new IPEndPoint(Ip, port);

listener.Bind(endpoint);

EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);


var buffer = new byte[ushort.MaxValue - 29];


int count = 0;






while (true)
{
    listener.ReceiveFrom(buffer, ref remoteEP);
    var imagebuffer = GetScreenshot();

    var fullen = imagebuffer.Length.ToString();
    buffer = new byte[fullen.Length];
    foreach (var item in fullen)
    {
        buffer[count++] = (byte)item;
    }
    listener.SendTo(buffer, remoteEP);
    count = 0;
    var receivesize = 0;
    var index = 0;


    while (imagebuffer.Length > count)
    {
        buffer = new byte[ushort.MaxValue - 29];
        var sendsize = buffer.Length;

        if (imagebuffer.Length - index < sendsize)
        {
            sendsize = imagebuffer.Length - count;
            buffer = new byte[sendsize];
        }
        listener.Receive(buffer);

        imagebuffer.ToList().CopyTo(index, buffer, 0, sendsize);
        receivesize = listener.SendTo(buffer, remoteEP);
        count += receivesize;
        index = count - 1;
    }
    buffer = new byte[ushort.MaxValue - 29];
    count = 0;
}
