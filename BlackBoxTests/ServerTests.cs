using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

using VeraServer;
using System.Collections.Generic;
using System.Text;

namespace BlackBoxTests
{
	class ServerTests
	{
		private const uint NUM_CONNECTIONS = 10;
		private const int WAIT_BEFORE_DROP = 5000;
		private const double FAKE_PLAYER_PROBABILITY = 0.2;
		private const int MAX_NAME_LENGTH = 20;

		public static void RunTests()
		{
			ConnectThenDrop();
			ConnectThenDrop();
		}


		/// <summary>
		/// Spawn NUM_CONNECTIONS, some of which represent players that exist in the database,
		/// some of which represent players that do not exist. The threads connect and run until
		/// WAIT_BEFORE_DROP milliseconds pass, then all of them drop unexpectedly.
		/// </summary>
		public static void ConnectThenDrop()
		{
			Thread[] threads = new Thread[NUM_CONNECTIONS];
			PlayerThread[] pt = new PlayerThread[NUM_CONNECTIONS];
			Random rand = new Random();
			PlayerDatabase players = PlayerDatabase.Instance;
			players.CreateTestDatabase();
			int i = 0;
			for (; i < NUM_CONNECTIONS; i++)
			{
				Player player;
				
				if (rand.NextDouble() < FAKE_PLAYER_PROBABILITY)
				{
					byte[] fakeName = new byte[rand.Next(MAX_NAME_LENGTH)];
					byte[] fakePassword = new byte[128];
					rand.NextBytes(fakeName);
					rand.NextBytes(fakePassword);
					player = new Player(new Position((float)(rand.NextDouble() * 800.0), (float)(rand.NextDouble() * 600.0)),
						new Velocity((float)(rand.NextDouble() * 4.0 - 2.0), (float)(rand.NextDouble() * 4.0 - 2.0)),
						ASCIIEncoding.ASCII.GetString(fakeName), ASCIIEncoding.ASCII.GetString(fakePassword));
				}
				else
				{
					IList<Player> list = players.GetPlayers();
					player = list[rand.Next(list.Count)];
				}
				
				pt[i] = new PlayerThread(player, false);
				threads[i] = new Thread(new ThreadStart(pt[i].SingleConnection));
				threads[i].Start();
			}

			//Set a boolean in each thread to true, causing the thread to return
			Thread.Sleep(WAIT_BEFORE_DROP);
			foreach (var p in pt)
				p.done = true;
		}
	}

	class PlayerThread
	{
		private Player player;

		public bool done = false;

		private static Random rand = new Random();

		private const int WAIT_MILLIS_LENGTH_MAX = 200;
		private const float WAIT_PROBABILITY = 0.001f;


		private bool networkDebug = true;

		public PlayerThread(Player player, bool networkDebug)
		{
			this.player = player;
			this.networkDebug = networkDebug;
		}

		public void SingleConnection()
		{
			try
			{
				Run();
			}
			catch (IOException e)
			{
				if (e.InnerException.GetType() == typeof(SocketException))
					Console.WriteLine("Testing thread {0} threw exception: " + e.Message, player.Name);
				else
					throw e;
			}
		}
		private void Run()
		{

			TcpClient tcp = new TcpClient();
			IAsyncResult ar = tcp.BeginConnect("127.0.0.1", 11000, null, null);
			System.Threading.WaitHandle wh = ar.AsyncWaitHandle;

			if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
			{
				tcp.Close();
				throw new TimeoutException("Authentication thread timed out.");
			}

			tcp.EndConnect(ar);


			MaybeWait();

			//System.Threading.Thread.Sleep(2000);
			NetworkStream stream = tcp.GetStream();
			byte[] buffer = BitConverter.GetBytes(390458);
			stream.Write(buffer, 0, 4);

			MaybeWait();

			stream.Read(buffer, 0, 4);
			long value = BitConverter.ToInt32(buffer, 0);

			MaybeWait();

			buffer = System.Text.Encoding.UTF8.GetBytes(player.Name);
			stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
			MaybeWait();
			stream.Write(buffer, 0, buffer.Length);

			MaybeWait();

			buffer = System.Text.Encoding.UTF8.GetBytes(player.Password);
			//Will always be 128 characters long
			stream.Write(buffer, 0, buffer.Length);

			while (!done)
			{
				float x = player.Position.x + (float)(rand.NextDouble() * 4.0 - 2.0);

				if (x > 800.0f)
					x = 800.0f;
				else if (x < 0.0f)
					x = 0.0f;

				float y = player.Position.y + (float)(rand.NextDouble() * 4.0 - 2.0);

				if (y > 600.0f)
					y = 600.0f;
				else if (y < 0.0f)
					y = 0.0f;

				player.Position = new Position(x, y);

				stream.Write(BitConverter.GetBytes(player.Position.x), 0, 4);
				MaybeWait();
				stream.Write(BitConverter.GetBytes(player.Position.y), 0, 4);
				MaybeWait();

				buffer = new byte[4];
				stream.Read(buffer, 0, 4);
				int numNearbyPlayers = BitConverter.ToInt32(buffer, 0);
				int numBytes = numNearbyPlayers * 8;

				MaybeWait();

				buffer = new byte[numBytes];
				stream.Read(buffer, 0, numBytes);

				MaybeWait();

				//float a = BitConverter.ToSingle(buffer, 0);
				//float b = BitConverter.ToSingle(buffer, 4);
			}
		}

		/// <summary>
		/// Wait for WAIT_MILLIS_LENGTH_MAX with WAIT_PROBABILITY as long as networkDebug is true.
		/// </summary>
		private void MaybeWait()
		{
			if (rand.NextDouble() > WAIT_PROBABILITY || !networkDebug)
				return;

			int millisToWait = rand.Next(WAIT_MILLIS_LENGTH_MAX);
			Thread.Sleep(millisToWait);
			//Console.WriteLine("Sleeping for {0} millis...", millisToWait);
		}
	}
}
