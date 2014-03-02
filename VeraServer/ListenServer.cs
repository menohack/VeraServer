using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace VeraServer
{
	/// <summary>
	/// This class accepts connections and spawns AuthenticationServer threads.
	/// </summary>
	public class ListenServer
	{
		/// <summary>
		/// The list of server threads. Each thread talks to one player.
		/// </summary>
		private IList<Server> servers;

		/// <summary>
		/// The database of players.
		/// </summary>
		private static PlayerDatabase players = PlayerDatabase.Instance;

		/// <summary>
		/// The length of time, in milliseconds, that the ListenServer listens for a connection.
		/// </summary>
		private const int LISTEN_LENGTH = 20000;

		/// <summary>
		/// The maximum number of connected players.
		/// </summary>
		public const int MAX_PLAYERS = 8;

		public delegate void CloseServerDelegate(Server server);

		/// <summary>
		/// Callback function for when a Server thread exits.
		/// </summary>
		/// <param name="server">The server that is no longer running.</param>
		public void CloseServer(Server server)
		{
			servers.Remove(server);
			uint numPlayersLoggedIn = 0;
			foreach (var p in players.GetPlayers())
				if (p.LoggedIn)
					numPlayersLoggedIn++;
			Console.WriteLine("Removed server. {0} remaining servers, {1} players logged in.", servers.Count, numPlayersLoggedIn);
		}

		/// <summary>
		/// Constructs the ListenServer.
		/// </summary>
		public ListenServer()
		{
			servers = new List<Server>();
			players = PlayerDatabase.Instance;
			players.CreateTestDatabase();
			players.SaveDatabase();
		}

		/// <summary>
		/// Runs the ListenServer.
		/// </summary>
		public void Run()
		{
			String ip = "127.0.0.1";
			//String ip = "128.220.251.35";
			//String ip = "128.220.70.65";
			IPAddress ipAddress = IPAddress.Parse(ip);
			IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 11000);
			try
			{
				TcpListener listener = new TcpListener(ipLocalEndPoint);
				listener.Start();

				while (true)
				{
					while (servers.Count < MAX_PLAYERS)
					{
						Console.WriteLine("Waiting for connections...");
						TcpClient client = listener.AcceptTcpClient();
						//var task = listener.AcceptTcpClientAsync();
						
						//if (!task.Wait(LISTEN_LENGTH))
						//	continue;

						//TcpClient client = task.Result;

						Server server = new Server(client, new ListenServer.CloseServerDelegate(CloseServer));
						servers.Add(server);
						//int playerID = servers.IndexOf(server);
						//server.playerID = playerID;
						Console.WriteLine("{0} players connected", servers.Count);
						server.Start();
					}
					//System.Threading.Thread.Sleep(1000);
				}
			}
			catch (SocketException e)
			{
				//Continue if there is a socket exception
				Console.WriteLine(e.ToString());
			}
			catch (Exception e)
			{
				//Crash if there is any other exception
				Console.WriteLine(e.ToString());
				Console.Read();
			}
		}
	}
}
