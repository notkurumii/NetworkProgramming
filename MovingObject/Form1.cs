using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Linq;

namespace MovingObject
{
    public partial class Form1 : Form
    {
        private Socket serverSocket;
        private List<Socket> clientSockets = new List<Socket>();

        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;
            Text = "SERVER";
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 11111));
                serverSocket.Listen(10);
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Server startup error: " + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                Socket handler = serverSocket.EndAccept(AR);
                clientSockets.Add(handler);
                // Terus mendengarkan koneksi baru
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex)
            {
                MessageBox.Show("Accept error: " + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            back();
            rect.X += slide;
            Invalidate();
            
            // Mengirim posisi objek ke semua klien
            PushDataToClients(rect.X, rect.Y);
        }

        private void PushDataToClients(int x, int y)
        {
            try
            {
                byte[] data = BitConverter.GetBytes(x).Concat(BitConverter.GetBytes(y)).ToArray();
                List<Socket> disconnectedClients = new List<Socket>();

                foreach (Socket client in clientSockets)
                {
                    try
                    {
                        client.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, client);
                    }
                    catch (SocketException)
                    {
                        disconnectedClients.Add(client);
                    }
                }

                // Hapus klien yang terputus
                foreach (Socket client in disconnectedClients)
                {
                    clientSockets.Remove(client);
                    client.Close();
                }
            }
            catch (ObjectDisposedException) { }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                Socket current = (Socket)AR.AsyncState;
                current.EndSend(AR);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex)
            {
                // Klien terputus, akan ditangani pada PushDataToClients
            }
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else
            if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}