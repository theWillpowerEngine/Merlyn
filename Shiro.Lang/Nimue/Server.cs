using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shiro.Guts;

namespace Shiro.Nimue
{
    internal static class Server
    {
        internal static bool Serving = false;

        internal static ConnectionType ConType
        {
            get { return Connection.ConType; }
            set { Connection.ConType = value; }
        }

        internal static class Locks
        {
            public static readonly object ConnectionsLock = new object();
            public static readonly object ShiroLock = new object();
        }

        private static List<Connection> Connections = new List<Connection>();
        private static int Port = 4676;

        private static Token HandlerToken, ConnectHandlerToken;
        private static Interpreter Merp;

        private static void Listener()
        {
            IPAddress localAdd = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(localAdd, Port);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            listener.Start();

            while (Serving)
            {
                //Blocking
                TcpClient client = listener.AcceptTcpClient();

                var conn = new Connection(client);
                conn.ConnectionId = Guid.NewGuid();

                lock (Locks.ConnectionsLock)
                {
                    Connections.Add(conn);
                }

                if (ConnectHandlerToken != null)
                {
                    lock (Locks.ShiroLock)
                    {
                        EvaluateTokenWithLet(conn, ConnectHandlerToken);
                    }
                }
            }

            listener.Stop();
        }

        private static void UpdateConnections()
        {
            var newConns = new List<Connection>();
            lock (Locks.ConnectionsLock)
            {
                foreach (var con in Connections)
                {
                    if (!con.ShouldBeNuked)
                    {
                        newConns.Add(con);
                        con.CheckForInput();
                        if (con.HasFullCommand)
                        {
							var result = EvaluateTokenWithLet(con, HandlerToken);

                            if (ConType == ConnectionType.HTTP)
                            {
                                SendTo(con, HttpHelper.WrapInHttpResponse(result.ToString()));
                                con.CleanUp();
                                newConns.Remove(con);
                            }
                        }
                    }
                    else
                    {
                        con.CleanUp();
                    }
                }

                Connections = newConns;
            }
        }

        private static Token EvaluateTokenWithLet(Connection con, Token handler)
        {
            lock (Locks.ShiroLock)
            {
				Guid letId = Guid.NewGuid();
				try
				{
					Merp.Symbols.Let(Symbols.AutoVars.ConnectionId, new Token(con.ConnectionId.ToString()), letId);

					if (ConType == ConnectionType.MUD)
						Merp.Symbols.Let(Symbols.AutoVars.TelnetInput, new Token(con.GetCommand()), letId);
					else
					{
						var request = HttpHelper.ParseRequest(con.GetCommand());
						var token = HttpHelper.RequestToToken(request);
						Merp.Symbols.Let(Symbols.AutoVars.HttpRequest, token, letId);
					}

					var retVal = handler.Eval(Merp);
					return retVal;
				}
				catch (ApplicationException aex)
				{
					Merp.Eval($"print 'Server error: {aex.Message}'");
				}
				finally
				{
					Merp.Symbols.ClearLetId(letId);
				}
			}

			return Token.Nil;
        }

        internal static void SendTo(Guid conId, string msg)
        {
            Connection conn = null;
            lock (Locks.ConnectionsLock)
            {
                conn = Connections.FirstOrDefault(c => c.ConnectionId == conId);
            }

            SendTo(conn, msg);
        }
        internal static void SendTo(Connection conn, string msg)
        {
            if (conn != null)
                conn.Send(msg);
            else
                Interpreter.Error($"Attempt to send on non-existent socket, data '{msg}'");
        }
        internal static void SendToAll(string msg)
        {
            lock (Locks.ConnectionsLock)
            {
                foreach (var conn in Connections)
                {
                    try
                    {
                        conn.Send(msg);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        internal static void ListenForTelnet(Interpreter merp, Token commandHandler, int port = 4676, Token connectHandler = null)
        {
            ConType = ConnectionType.MUD;
            Port = port;
            HandlerToken = commandHandler;
            ConnectHandlerToken = connectHandler;

            lock(Locks.ShiroLock)
                Merp = merp;

            Serving = true;
            
            //Listen thread
            var ts = new ThreadStart(Listener);
            var thread = new Thread(ts);
            thread.Start();

            //Receive thread
            var ts2 = new ThreadStart(() =>
            {
                while (Serving)
                {
                    //Receive loop
                    UpdateConnections();
                    Thread.Sleep(50);
                }

                lock (Locks.ConnectionsLock)
                {
                    foreach (var con in Connections)
                        con.CleanUp(false);
                    Connections.Clear();
                }

            });
            var thread2 = new Thread(ts2);
            thread2.Start();

            Result = null;
            while (Serving)
            {
                Thread.Sleep(50);
            }
        }

        internal static void ListenForHttp(Interpreter merp, Token commandHandler, int port = 8088)
        {
            ConType = ConnectionType.HTTP;
            Port = port;
            HandlerToken = commandHandler;
            ConnectHandlerToken = null;

            lock (Locks.ShiroLock)
                Merp = merp;

            Serving = true;

            //Listen thread
            var ts = new ThreadStart(Listener);
            var thread = new Thread(ts);
            thread.Start();

            //Receive thread
            var ts2 = new ThreadStart(() =>
            {
                while (Serving)
                {
                    //Receive loop
                    UpdateConnections();
                    Thread.Sleep(50);
                }

                lock (Locks.ConnectionsLock)
                {
                    foreach (var con in Connections)
                        con.CleanUp(false);
                    Connections.Clear();
                }

            });
            var thread2 = new Thread(ts2);
            thread2.Start();

            Result = null;
            while (Serving)
            {
                Thread.Sleep(50);
            }
        }

        internal static Token Result = null;
    }
}
