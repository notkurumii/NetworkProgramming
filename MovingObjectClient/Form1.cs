using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace MovingObjectClient
{
    public partial class Form1 : Form
    {
        private System.ComponentModel.IContainer components = null;
        private Socket clientSocket;
        private byte[] buffer;
        
        private Pen red = new Pen(Color.Red);
        private SolidBrush fillBlue = new SolidBrush(Color.Blue);
        private Rectangle rect = new Rectangle(0, 0, 30, 30);

        public Form1()
        {
            InitializeComponent();
            Text = "CLIENT";
            ConnectToServer();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                clientSocket?.Close();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 344);
            this.Name = "Form1";
            this.Text = "CLIENT";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);

        }

        private void ConnectToServer()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 11111);

                clientSocket.BeginConnect(endPoint, ConnectCallback, null);
                
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndConnect(AR);
                buffer = new byte[8];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Connection callback error: " + ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);

                if (received == 8)
                {
                    int x = BitConverter.ToInt32(buffer, 0);
                    int y = BitConverter.ToInt32(buffer, 4);

                    rect.X = x;
                    rect.Y = y;
                    Invoke((Action)delegate { Invalidate(); });
                }

                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex)
            {
                MessageBox.Show("Receive error: " + ex.Message);
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}