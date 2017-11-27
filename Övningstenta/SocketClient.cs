using Övningstenta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SocketClientNameSpace
{
    public class SocketClient
    {
        TcpClient client;
        NetworkStream ns;
        private MainWindow _window;

        private IPEndPoint RemoteEndPoint(IPAddress address)
        {
            
            int port = int.Parse("8080");

            IPEndPoint endPoint = new IPEndPoint(address, port);

            return endPoint;
        }

        public void Connect(IPAddress address)
        {
            byte[] data = new byte[1024];
            client = new TcpClient();

            try
            {
                client.Connect(RemoteEndPoint(address));
                ns = client.GetStream();

                int recv = ns.Read(data, 0, data.Length);
                string response = Encoding.ASCII.GetString(data, 0, recv);

                //Task task = new Task(() => ClientRecieve());
                //task.Start();

                ClientSend("Client connecting");

                ClientRecieve();

            }
            catch
            {
                throw ;
            }
        }

        public void ClientSend(string message)
        {
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
            }
            catch
            {
                MessageBox.Show("Servern stängde anslutningen");
            }
        }

        private void ClientRecieve()
        {
            while (true)
            {
                var data = new byte[1024];
                int recv = ns.Read(data, 0, data.Length);
                string message = Encoding.ASCII.GetString(data, 0, recv);
                MessageBox.Show("Recieved: " + message);

                try
                {
                    var number = int.Parse(message);

                    foreach (var item in _window.TicTacToe.Children)
                    {
                        if((item as Button).TabIndex == number)
                        {
                            _window._waitingForOpponent = false;
                            (item as Button).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); ;
                        }
                    }
                }
                catch
                {

                }
            }

        }
    }
}
