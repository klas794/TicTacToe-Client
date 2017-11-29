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
using TicTacToeClient;
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
        private string _homeMarker { get; set; }
        private string _imageUri { get; set; }
        private string _imagePath { get; set; }

        private int _gridSize;

        public bool WaitingForOpponent { get; set; }

        private const string NEW_GAME = "New game";
        private const double HIDDEN_BUTTON_OPACITY = .3;
        private const int _filterSize = 3;

        private SocketClient _socketClient;
        public bool GameOver = true;
        public bool OpponentClicking;
        private bool _firstHomeMove = true;

        public MainWindow()
        {
            InitializeComponent();
            _currentPlayer = "o";
            _imageUri = "/images/{0}.png";
            _imagePath = "pack://application:,,," + _imageUri;

            _gridSize = (int)Math.Sqrt(TicTacToe.Children.Count);

            _socketClient = new SocketClient(this);

            var defaultServer = SocketHelper.GetDefaultServer();
            ServerAddress.Text = defaultServer.Item1.ToString();
            ServerPort.Text = defaultServer.Item2.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if(GameOver)
            {
                ConnectionMessage.Text = "Game not started";
                return;
            }

            var button = sender as Button;

            var buttonTaken = GetCharacter(0, button.TabIndex) != null;

            if(!buttonTaken) {

                if (!OpponentClicking && !WaitingForOpponent ||
                    OpponentClicking)
                {
                    SetMarker(button);
                    _currentPlayer = _currentPlayer == "o" ? "x" : "o";
                    LookForWinner();
                }

                if (!OpponentClicking && !WaitingForOpponent) {

                    if(_firstHomeMove)
                    {
                        ConnectionMessage.Text = "";
                        _homeMarker = _currentPlayer;
                        _firstHomeMove = false;
                    }

                    _socketClient.ClientSend(button.TabIndex.ToString());
                    WaitingForOpponent = true;
                }
                
            }
        }

        private void SetMarker(Button button)
        {
            var image = button.Content as Image;

            image.Source = new BitmapImage(new Uri(String.Format(_imagePath, _currentPlayer)));

        }

        private void SetWinningRow(int filterStartIndex, int rowIndex)
        {
            for (int i = 0; i < _filterSize; i++)
            {
                if(i == rowIndex)
                {
                    for (int j = 0; j < _filterSize; j++)
                    {
                        HideButton(filterStartIndex + i * _gridSize + j);
                        
                    }
                }
                
            }
        }

        private void SetWinningColumn(int filterStartIndex, int columnIndex)
        {
            for (int i = 0; i < _filterSize; i++)
            {
                if (i == columnIndex)
                {
                    for (int j = 0; j < _filterSize; j++)
                    {
                        HideButton(filterStartIndex + i + j * _gridSize);
                    }
                }

            }
        }

        private void SetWinningDiagonal(int filterStartIndex, DiagonalDirections direction)
        {
            for (int i = 0; i < _filterSize; i++)
            {
                for (int j = 0; j < _filterSize; j++)
                {
                    if(i == j)
                    {
                        var index = direction == DiagonalDirections.TopLeft ?
                            i * _gridSize + j : i * _gridSize + (2 - j);
                        index += filterStartIndex;
                        HideButton(index);
                    }
                }
            }
        }

        private void ShowButton(int buttonIndex)
        {
            var button = TicTacToe.Children[buttonIndex] as Button;
            var image = button.Content as Image;
            image.Opacity = 1;
        }

        private void HideButton(int buttonIndex)
        {
            var button = TicTacToe.Children[buttonIndex] as Button;
            var image = button.Content as Image;
            image.Opacity = HIDDEN_BUTTON_OPACITY;
        }

        private void HideAllButtons()
        {
            for (int i = 0; i < _gridSize; i++)
            {
                HideButton(i);
            }
        }

        private void ShowAllButtons()
        {
            for (int i = 0; i < _gridSize; i++)
            {
                ShowButton(i);
            }
        }

        private void LookForWinner()
        {
            var cursor = 0;

            for (int cursorX = 0; cursorX <= _gridSize - _filterSize; cursorX++)
            {
                for (int cursorY = 0; cursorY <= _gridSize - _filterSize; cursorY++)
                {
                    cursor = cursorX + _gridSize * cursorY;
                    // rows
                    for (int i = 0; i < _filterSize; i++)
                    {
                        var addition = i * _gridSize;
                        var first = GetCharacter(cursor, 0 + addition);
                        var second = GetCharacter(cursor, 1 + addition);
                        var third = GetCharacter(cursor, 2 + addition);
                        if (  first == second 
                            && second == third
                            && first != null
                            )
                        {
                            SetWinningRow(cursor, i);
                            AnnounceWinner(first);
                            break;
                        }
                    }

                    // columns
                    for (int i = 0; i < _filterSize; i++)
                    {
                        var addition = i;
                        var first = GetCharacter(cursor, 0 + addition);
                        var second = GetCharacter(cursor, _gridSize + addition);
                        var third = GetCharacter(cursor, _gridSize * 2 + addition);
                        if (first == second
                            && second == third
                            && first != null
                            )
                        {
                            SetWinningColumn(cursor, i);
                            AnnounceWinner(first);
                            break;
                        }
                    }

                    // diagonal top-left to bottom-right
                    if (GetCharacter(cursor, 0) != null
                            && GetCharacter(cursor, 0) == GetCharacter(cursor, _gridSize + 1)
                            && GetCharacter(cursor, _gridSize + 1) == GetCharacter(cursor, _gridSize * 2 + 2)
                            )
                    {
                        SetWinningDiagonal(cursor, DiagonalDirections.TopLeft);
                        AnnounceWinner(GetCharacter(cursor, 0));
                    }

                    // diagonal top-right to bottom-left
                    if (GetCharacter(cursor, 2) != null 
                        && GetCharacter(cursor, 2) == GetCharacter(cursor, _gridSize + 1)
                            && GetCharacter(cursor, _gridSize + 1) == GetCharacter(cursor, _gridSize * 2) 
                            )
                    {
                        SetWinningDiagonal(cursor, DiagonalDirections.TopRight);
                        AnnounceWinner(GetCharacter(cursor, 2));
                    }

                }
            }

        }

        private void AnnounceWinner(string v)
        {
            //MessageBox.Show(String.Format("{0} is the winner!", v));
            GameOver = true;
            ConnectionMessage.Text = v == _homeMarker ? "You lost.": "You won!";
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

        private string GetCharacter(int cursor, int index)
        {
            var button = TicTacToe.Children[cursor + index] as Button;
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
            _socketClient.ClientSend(NEW_GAME);
            CreateNewGame();
        }

        public void CreateNewGame()
        {
            ClearImages();
            GameOver = false;
            WaitingForOpponent = false;
            OpponentClicking = false;
            _currentPlayer = "o";
            _firstHomeMove = true;
            ConnectionMessage.Text = "";
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
                var port = int.Parse(ServerPort.Text);
                ServerAddress.Foreground = new SolidColorBrush(Colors.Black);

                _socketClient.ClientConnectAsync(address, port);

                
            }
            catch
            {
                ServerAddress.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
       
    }
}
