using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeClient
{
    class SocketHelper
    {
        public static Tuple<IPAddress, int> GetDefaultServer()
        {
            IPAddress address = Dns.GetHostAddresses(Dns.GetHostName())
                .First(x => x.AddressFamily == AddressFamily.InterNetwork);

            var tuple = Tuple.Create(address, 8080);
            return tuple;
        }
    }
}
