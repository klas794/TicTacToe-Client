using Övningstenta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace TicTacToeClient
{
    class SocketClient
    {
        private TcpClient client;
        private NetworkStream ns;

        private const string WELCOME_MESSAGE = "Welcome to Tic Tac Toe Server";
        private const string START_PLAYING_MESSAGE = "Welcome, start playing...";
        private const string PLAYER_JOINED_MESSAGE = "Player joined. Game started...";
        private const string NEW_GAME = "New game";
        private MainWindow _window;

        public SocketClient(MainWindow window)
        {
            _window = window;
        }

        private IPEndPoint RemoteEndPoint(IPAddress address, int port)
        {

            IPEndPoint endPoint = new IPEndPoint(address, port);

            return endPoint;
        }

        public async Task ClientConnectAsync(IPAddress address, int port)
        {
            byte[] data = new byte[1024];
            client = new TcpClient();

            try
            {
                client.Connect(RemoteEndPoint(address, port));
                ns = client.GetStream();

                //int recv = ns.Read(data, 0, data.Length);
                //string response = Encoding.ASCII.GetString(data, 0, recv);

                ClientSend(PLAYER_JOINED_MESSAGE);

                await Task.Run(() => ClientRecieve());
            }
            catch
            {
                throw;
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

        private async Task ClientRecieve()
        {
            while (true)
            {
                var data = new byte[1024];
                int recv = await ns.ReadAsync(data, 0, data.Length);
                string message = Encoding.ASCII.GetString(data, 0, recv);

                try
                {
                    var number = int.Parse(message);

                    Application.Current.Dispatcher.Invoke(() => {

                        foreach (var item in _window.TicTacToe.Children)
                        {
                            if ((item as Button).TabIndex == number)
                            {
                                _window.OpponentClicking = true;
                                (item as Button).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); ;
                                _window.OpponentClicking = false;
                                _window.WaitingForOpponent = false;
                            }
                        }

                    });
                }
                catch
                {
                    if (message == WELCOME_MESSAGE)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _window.ServerAddress.Foreground = new SolidColorBrush(Colors.Green);

                            _window.ConnectionMessage.Text = "Connected";
                            //System.Threading.Thread.Sleep(3000);
                            //ConnectionMessage.Text = "";
                        });
                    }
                    else if (message == PLAYER_JOINED_MESSAGE || message == START_PLAYING_MESSAGE)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (message == START_PLAYING_MESSAGE)
                            {
                                _window.ServerAddress.Foreground = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                _window.CreateNewGame();
                            }

                            _window.ConnectionMessage.Text = message;
                            _window.GameOver = false;
                        });
                    }
                    else if (message == NEW_GAME)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _window.CreateNewGame();
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _window.ConnectionMessage.Text = message;
                        });
                    }
                }
            }

        }

    }
}
