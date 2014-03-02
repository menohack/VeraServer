using System.Threading;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace VeraServer
{
	/// <summary>
	/// The Server class encapsulates a thread that communicates with a single player. Each Server has
	/// access to the PlayerDatabase Singleton data structure, which facilitates thread-safe data access.
	/// The Server uses the State Pattern to simplify message logic.
	/// </summary>
	public class Server
	{
		/// <summary>
		/// The TcpClient with which we connected to the client.
		/// </summary>
		private TcpClient client;

		/// <summary>
		/// The thread that will run the server. Each server has its own thread. We should probably
		/// use a ThreadPool in the future.
		/// </summary>
		private Thread thread;

		/// <summary>
		/// The NetworkStream for reading and writing to the client.
		/// </summary>
		private NetworkStream stream;

		/// <summary>
		/// The Singleton instance of the data structure containing the player information.
		/// </summary>
		private static PlayerDatabase players = PlayerDatabase.Instance;

		/// <summary>
		/// The current player that this server represents.
		/// </summary>
		private Player player;

		/// <summary>
		/// The time, in milliseconds, to wait before throwing a TimeoutException and closing the thread.
		/// </summary>
		private const int TIMEOUT_TIME = 15000;

		/// <summary>
		/// A delegate to the method that must be invoked when the Server closes.
		/// </summary>
		private ListenServer.CloseServerDelegate closeServer;

		/// <summary>
		/// Construct the Server.
		/// </summary>
		/// <param name="client">The accepted TcpClient from the ListenServer.</param>
		/// <param name="closeServer">A delegate to the method that should be called once the server closes.</param>
		public Server(TcpClient client, ListenServer.CloseServerDelegate closeServer)
		{
			this.client = client;
			thread = new Thread(new ThreadStart(Run));
			this.closeServer = closeServer;
		}

		/// <summary>
		/// Start running the thread.
		/// </summary>
		public void Start()
		{
			if (thread != null)
				thread.Start();
		}

		/// <summary>
		/// This method runs the server by continually executing state transitions.
		/// </summary>
		private void Run()
		{
			try
			{
				stream = client.GetStream();
				StateContext context = new StateContext(this);

				while (true)
					context.Next();
			}
			catch (Exception e)
			{
				//IOExceptions and TimeoutExceptions have the same behavior
				Console.WriteLine(e.Message);
				client.Close();


				Player updatedPlayer = new Player(player);
				updatedPlayer.LoggedIn = false;
				if (player != null && player.LoggedIn)
					if (!players.TryUpdate(player.Name, updatedPlayer, player))
						throw new Exception("Fuck you");

				//if (player != null && player.LoggedIn)
				//	player.LoggedIn = false;
				closeServer(this);
				return;
			}
		}

		/// <summary>
		/// Reads length bytes. If the read takes longer than TIMEOUT_TIME it throws an exception. The thread
		/// blocks until the data is read or TIMEOUT_TIME has elapsed. Should I be calling EndRead()?
		/// </summary>
		/// <param name="length">The length, in bytes, to be read.</param>
		/// <returns>Returns a byte array of the data read.</returns>
		private byte[] ReadAsync(uint length)
		{
			byte[] buffer = new byte[length];

			//This will immediately return if the client closes.
			IAsyncResult result = stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback((x) => { /*(x.AsyncState as NetworkStream).EndRead(x);*/ }), stream);
			WaitHandle waitHandle = result.AsyncWaitHandle;
			bool completed = waitHandle.WaitOne(TIMEOUT_TIME);
			//int bytesRead = stream.EndRead(result);
			if (!completed)
				throw new TimeoutException(String.Format("Timed out while trying to read {0} bytes.", length));
			return buffer;
		}

		/// <summary>
		/// Reads a 32-bit signed integer asynchronously.
		/// </summary>
		/// <returns>Returns the integer that was read.</returns>
		private int ReadInt()
		{
			byte[] buffer = ReadAsync(4);

			return BitConverter.ToInt32(buffer, 0);
		}

		/// <summary>
		/// Reads a length byte string asynchronously.
		/// </summary>
		/// <param name="length">The length, in bytes, to read.</param>
		/// <returns>Returns the string that was read.</returns>
		private String ReadString(uint length)
		{
			byte[] buffer = ReadAsync(length);
			return Encoding.UTF8.GetString(buffer, 0, (int)length);
		}

		/// <summary>
		/// Reads a 32-bit float asynchronously.
		/// </summary>
		/// <returns>Returns the fload that was read.</returns>
		private float ReadSingle()
		{
			byte[] buffer = ReadAsync(4);
			return BitConverter.ToSingle(buffer, 0);
		}

		/// <summary>
		/// This is temporary. It should be asynchronous in the future.
		/// </summary>
		/// <param name="buffer"></param>
		private void Write(byte[] buffer, int offset, int size)
		{
			stream.Write(buffer, 0, size);
		}

		/// <summary>
		/// The StateContext class is used to facilitate state transitions.
		/// </summary>
		class StateContext
		{
			/// <summary>
			/// The current state, starting with AuthenticationState.
			/// </summary>
			private NetworkState state = new AuthenticationState();

			/// <summary>
			/// A reference to the server that holds this state.
			/// </summary>
			private Server server;

			/// <summary>
			/// Default constructor.
			/// </summary>
			/// <param name="server">The server that holds this state.</param>
			public StateContext(Server server)
			{
				this.server = server;
			}

			/// <summary>
			/// Transition to the next state.
			/// </summary>
			public void Next()
			{
				state.Receive(server);
				state = state.Send(server);
			}
		}

		/// <summary>
		/// The NetworkState interface specifies the two methods of every State class.
		/// </summary>
		interface NetworkState
		{
			NetworkState Send(Server server);
			void Receive(Server server);
		}

		/// <summary>
		/// The AuthenticationState is the first state, when the client and server confirm that
		/// they are the right game.
		/// </summary>
		class AuthenticationState : NetworkState
		{
			private const int AUTHENTICATION_CLIENT_VALUE = 390458;
			private const int AUTHENTICATION_SERVER_VALUE = -283947;

			public NetworkState Send(Server server)
			{
				server.Write(BitConverter.GetBytes(AUTHENTICATION_SERVER_VALUE), 0, 4);
				//Console.WriteLine("Authentication successful!");

				return new UsernameAndPasswordState();
			}
			public void Receive(Server server)
			{
				int value = server.ReadInt();

				Console.WriteLine("Player authenticated with {0}", value);
				if (value != AUTHENTICATION_CLIENT_VALUE)
					throw new ApplicationException("Player authenticated with invalid value.");
			}
		}

		/// <summary>
		/// The UsernameAndPasswordState is the second state, when the client submits a username and
		/// password which the server confirms or denies. We should log the number of attempts in the
		/// future.
		/// </summary>
		class UsernameAndPasswordState : NetworkState
		{
			private const int LOGIN_SUCCESSFUL_VALUE = 1337;
			private bool loggedIn = false;

			public NetworkState Send(Server server)
			{
				if (loggedIn)
				{
					Console.WriteLine("{0} successfully logged in with {1}", server.player.Name, server.player.Password);
					server.Write(BitConverter.GetBytes(LOGIN_SUCCESSFUL_VALUE), 0, 4);
					return new PositionState();
				}
				else
					//This should retry in the future, rather than closing the server
					throw new ApplicationException("Login failed");
			}

			public void Receive(Server server) 
			{
				try
				{
					int length = server.ReadInt();
					if (length > Player.MAX_PLAYER_NAME_LENGTH)
						throw new ApplicationException(String.Format("Name too long: {0} characters", length));

					String name = server.ReadString((uint)length);
					Console.WriteLine("Player attempted to join with name: " + name);

					server.player = players.FindPlayer(name);

					if (server.player == null)
						throw new ApplicationException("Player not found");
					if (server.player.LoggedIn)
						throw new ApplicationException("Player " + server.player.Name + " already logged in");

					String password = server.ReadString(128);
					if (!server.player.Password.Equals(password))
						throw new ApplicationException(String.Format("Player gave invalid password {0}.", password));

					
					Player updatedPlayer = new Player(server.player);
					updatedPlayer.LoggedIn = true;
					if (players.TryUpdate(name, updatedPlayer, server.player))
						loggedIn = true;
				}
				catch (ApplicationException e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}

		/// <summary>
		/// The PositionState is the third state, when the position information is transmitted.
		/// This is the primary state. It will be expanded in the future. Each player tells the
		/// server its position then reads the position of nearby players other than itself
		/// from the server. We will also need to check the logic in the future.
		/// </summary>
		class PositionState : NetworkState
		{
			public NetworkState Send(Server server)
			{
				//Find the first other player.
				List<Player> otherPlayers = players.GetNearbyPlayers(server.player.Name);
				int numNearbyPlayers = otherPlayers.Count;

				//Write the number of players for which we are going to send data
				server.Write(BitConverter.GetBytes(numNearbyPlayers), 0, 4);

				WritePlayerData(server, otherPlayers);

				return this;
			}

			public void Receive(Server server)
			{
				Position newPos;
				newPos.x = server.ReadSingle();
				newPos.y = server.ReadSingle();
				server.player.Position = newPos;
				//Console.WriteLine("Read position ({0},{1})",newPos.x, newPos.y);
			}

			private void WritePlayerData(Server server, IList<Player> otherPlayers)
			{
				byte[] buffer = new byte[otherPlayers.Count * 8];
				int index = 0;
				//Write the data for each player into a buffer
				foreach (Player p in otherPlayers)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(p.Position.x), 0, buffer, index, 4);
					Buffer.BlockCopy(BitConverter.GetBytes(p.Position.y), 0, buffer, index + 4, 4);
					//Console.Write("({0}, {1})",p.Position.x, p.Position.y);

					index += 8;
					//Check for buffer full
				}

				//Write the buffer of player data
				server.Write(buffer, 0, index);
			}
		}
	}
}
