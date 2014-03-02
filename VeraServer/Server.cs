using System.Threading;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
				StreamWriter writer = new StreamWriter(stream);
				StreamReader reader = new StreamReader(stream);

				string message = reader.ReadLine();
				Console.WriteLine("Request: \"" + message + "\"");
				if (message == "Map plz")
				{
					Console.WriteLine("Responded");
					writer.WriteLine("Here it is");
				}
				else
					Console.WriteLine("Wrong request");
				writer.Close();
				//closeServer(this);
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
	}
}
