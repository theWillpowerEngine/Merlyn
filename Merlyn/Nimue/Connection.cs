using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Merlyn.Nimue
{
    internal enum ConnectionType
    {
        MUD = 0,
        HTTP = 1
    }


    internal class Connection
    {
        internal static ConnectionType ConType = ConnectionType.HTTP;
        
        #region Properties and Pass Throughs

        internal Guid ConnectionId = Guid.Empty;

        private readonly TcpClient _client;

        private string _inputBuffer = "";
        internal string InputBuffer
        {
            get => _inputBuffer;
        }

        private string _ipAddress = "Unk";
        internal string IPAddress
        {
            get => _ipAddress;
        }

        internal bool ShouldBeNuked
        {
            get => _client.GetState() != TcpState.Established;
        }

        internal bool HasFullCommand
        {
            get
            {
                if(ConType == ConnectionType.MUD)
                    return InputBuffer.Contains("\r") || InputBuffer.Contains("\n");

                if (ConType == ConnectionType.HTTP)
                    return InputBuffer.Contains("\r") || InputBuffer.Contains("\n");        //TODO:  Something better (this just tells you if the headers are there)

                return false;
            }
        }

        #endregion

        protected string ProcessBackspaces(string s)
        {
            string ret = "";

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '\b')
                {
                    if (ret.Length > 0)
                        ret = ret.Substring(0, ret.Length - 1);
                }
                else
                    ret += c;
            }

            return ret;
        }

        internal void CheckForInput()
        {
            NetworkStream nwStream = _client.GetStream();
            if (nwStream.CanRead && nwStream.DataAvailable)
            {
                byte[] buffer = new byte[_client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(buffer, 0, _client.ReceiveBufferSize);

                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                _inputBuffer += dataReceived;
                _inputBuffer = ProcessBackspaces(_inputBuffer);
            }
        }

        internal void CleanUp(bool throwOnFail = true)
        {
            try
            {
                _client.Close();
            }
            catch (Exception ex)
            {
                if(throwOnFail)
                    Merpreter.Error($"Exception in Connection.CleanUp: {ex.Message}.");
            }
        }

        public string GetCommand()
        {
            if (!HasFullCommand)
                return null;

            if (ConType == ConnectionType.MUD)
            {
                var eles = InputBuffer.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                var retVal = eles[0];

                _inputBuffer = "";
                for (var i = 1; i < eles.Length; i++)
                    _inputBuffer += Environment.NewLine + eles[i];

                _inputBuffer = _inputBuffer.TrimStart('\r', '\n');

                return retVal;
            }
            else
            {
                var retVal = InputBuffer;
                _inputBuffer = "";
                return retVal;
            }
        }

        public void Send(string msg)
        {
            NetworkStream nwStream = _client.GetStream();
            var bytes = Encoding.ASCII.GetBytes(msg);
            nwStream.Write(bytes, 0, bytes.Length);
        }

        public Connection(TcpClient tcp)
        {
            _client = tcp;
            _ipAddress = _client.Client.RemoteEndPoint.ToString();
        }
    }
}
