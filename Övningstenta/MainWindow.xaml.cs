using System;
using System.Collections.Generic;
using System.Linq;
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
using SocketClientNameSpace;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls.Primitives;

namespace Övningstenta
{
    public enum DiagonalDirections
    {
        TopLeft,
        TopRight
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream ns;

        private string _currentPlayer { get; set; }
        private string _imageUri { get; set; }
        private string _imagePath { get; set; }
        public bool _waitingForOpponent { get; set; }

        private const string WELCOME_MESSAGE = "Welcome to Tic Tac Toe Server";
        private const string START_PLAYING_MESSAGE = "Welcome, start playing...";
        private const string PLAYER_JOINED_MESSAGE = "Player joined. Game started...";
        private const double HIDDEN_BUTTON_OPACITY = .3;

        private int _gridSize;
        private bool _gameOver = true;
        private bool _opponentClicking;

        public MainWindow()
        {
            InitializeComponent();
            _currentPlayer = "o";
            _imageUri = "/images/{0}.png";
            _imagePath = "pack://application:,,," + _imageUri;

            _gridSize = (int)Math.Sqrt(TicTacToe.Children.Count);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if(_gameOver)
            {
                ConnectionMessage.Text = "Game not started";
                return;
            }

            var button = sender as Button;

            var buttonTaken = GetCharacter(button.TabIndex) != null;

            if(!buttonTaken) {

                if (!_opponentClicking && !_waitingForOpponent ||
                    _opponentClicking)
                {
                    SetMarker(button);
                    _currentPlayer = _currentPlayer == "o" ? "x" : "o";
                    LookForWinner();
                }

                if (!_opponentClicking && !_waitingForOpponent) {

                    ClientSend(button.TabIndex.ToString());
                    _waitingForOpponent = true;
                }
                
            }
        }

        private void SetMarker(Button button)
        {
            var image = button.Content as Image;

            image.Source = new BitmapImage(new Uri(String.Format(_imagePath, _currentPlayer)));

        }

        private void SetWinningRow(int rowIndex)
        {
            for (int i = 0; i < _gridSize; i++)
            {
                if(i != rowIndex)
                {
                    for (int j = 0; j < _gridSize; j++)
                    {
                        HideButton(i * _gridSize + j);
                        
                    }
                }
                
            }
        }

        private void SetWinningColumn(int columnIndex)
        {
            for (int i = 0; i < _gridSize; i++)
            {
                if (i != columnIndex)
                {
                    for (int j = 0; j < _gridSize; j++)
                    {
                        HideButton(i + j * _gridSize);
                    }
                }

            }
        }

        private void SetWinningDiagonal(DiagonalDirections direction)
        {
            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    if(i != j)
                    {
                        var index = direction == DiagonalDirections.TopLeft ?
                            i * _gridSize + j : i * _gridSize + (2 - j);
                        HideButton(index);
                    }
                }
            }
        }

        private void HideButton(int buttonIndex)
        {
            var button = TicTacToe.Children[buttonIndex] as Button;
            var image = button.Content as Image;
            image.Opacity = HIDDEN_BUTTON_OPACITY;
        }

        private void LookForWinner()
        {
            // rows
            for (int i = 0; i < _gridSize; i++)
            {
                var addition = i * _gridSize;
                var first = GetCharacter(0 + addition);
                var second = GetCharacter(1 + addition);
                var third = GetCharacter(2 + addition);
                if (  first == second 
                    && second == third
                    && first != null
                    )
                {
                    SetWinningRow(i);
                    AnnounceWinner(first);
                    break;
                }
            }

            // columns
            for (int i = 0; i < _gridSize; i++)
            {
                var addition = i;
                var first = GetCharacter(0 + addition);
                var second = GetCharacter(_gridSize + addition);
                var third = GetCharacter(6 + addition);
                if (first == second
                    && second == third
                    && first != null
                    )
                {
                    SetWinningColumn(i);
                    AnnounceWinner(first);
                    break;
                }
            }

            // diagonal top-left to bottom-right
            if (GetCharacter(0) == GetCharacter(4)
                    && GetCharacter(4) == GetCharacter(8)
                    && GetCharacter(0) != null
                    )
            {
                SetWinningDiagonal(DiagonalDirections.TopLeft);
                AnnounceWinner(GetCharacter(0));
            }

            // diagonal top-right to bottom-left
            if (GetCharacter(2) == GetCharacter(4)
                    && GetCharacter(4) == GetCharacter(6)
                    && GetCharacter(2) != null
                    )
            {
                SetWinningDiagonal(DiagonalDirections.TopRight);
                AnnounceWinner(GetCharacter(2));
            }
        }

        private void AnnounceWinner(string v)
        {
            //MessageBox.Show(String.Format("{0} is the winner!", v));
            _gameOver = true;
            //ClearImages();
        }

        private void ClearImages()
        {
            foreach (var item in TicTacToe.Children)
            {
                var image = (item as Button).Content as Image;
                image.Source = null;
                image.Opacity = 1;
            }
        }

        private string GetCharacter(int index)
        {
            var button = TicTacToe.Children[index] as Button;
            var image = button.Content as Image;
            var source = image.Source as BitmapImage;

            if(source == null )
            {
                return null;
            }

            var uri = source.UriSource;
            var path = uri.LocalPath;

            if (path == String.Format(_imageUri, "o"))
            {
                return "o";
            }

            if (path == String.Format(_imageUri, "x"))
            {
                return "x";
            }

            return null;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ClientSend("New game");
            ClearImages();
            _gameOver = false;
            _waitingForOpponent = false;
            _opponentClicking = false;
            _currentPlayer = "o";
        }

        private void CloseGame_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var address = IPAddress.Parse(ServerAddress.Text);
                ServerAddress.Foreground = new SolidColorBrush(Colors.Black);

                ClientConnectAsync(address);

                
            }
            catch
            {
                ServerAddress.Foreground = new SolidColorBrush(Colors.Red);
            }
        }


        private IPEndPoint RemoteEndPoint(IPAddress address)
        {

            int port = int.Parse("8080");

            IPEndPoint endPoint = new IPEndPoint(address, port);

            return endPoint;
        }

        public async Task ClientConnectAsync(IPAddress address)
        {
            byte[] data = new byte[1024];
            client = new TcpClient();

            try
            {
                client.Connect(RemoteEndPoint(address));
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

                    Dispatcher.Invoke( () => { 

                        foreach (var item in TicTacToe.Children)
                        {
                            if ((item as Button).TabIndex == number)
                            {
                                _opponentClicking = true;
                                (item as Button).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); ;
                                _opponentClicking = false;
                                _waitingForOpponent = false;
                            }
                        }

                    });
                }
                catch
                {
                    if (message == WELCOME_MESSAGE)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ServerAddress.Foreground = new SolidColorBrush(Colors.Green);

                            ConnectionMessage.Text = "Connected";
                            //System.Threading.Thread.Sleep(3000);
                            //ConnectionMessage.Text = "";
                        });
                    }
                    else if (message == PLAYER_JOINED_MESSAGE || message == START_PLAYING_MESSAGE)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if(message == START_PLAYING_MESSAGE)
                            {
                                ServerAddress.Foreground = new SolidColorBrush(Colors.Green);
                            }

                            ConnectionMessage.Text = message;
                            _gameOver = false;
                        });
                    }
                    else
                    {
                        //MessageBox.Show(message);
                        Dispatcher.Invoke(() =>
                        {
                            ConnectionMessage.Text = message;
                        });
                    }
                }
            }

        }

    }
}
