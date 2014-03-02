using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace VeraServer
{
	/// <summary>
	/// The PlayerDatabase class contains the in-memory player data as well as the functionality for
	/// writing the data to an XML file. In the future this will connect to an SQL database rather
	/// than writing to file. Every thread running in the model communicates with this singleton
	/// class to keep the player data coordinated.
	/// </summary>
	public class PlayerDatabase
	{
		/// <summary>
		/// This Dictionary contains all of the player data in memory, indexed by name.
		/// </summary>
		private ConcurrentDictionary<String, Player> dict;

		/// <summary>
		/// The Linq XML Document that contains the database.
		/// </summary>
		private XElement database;

		/// <summary>
		/// The filename used to load and save the database.
		/// </summary>
		private const String DATABASE_FILE_NAME = "database.xml";

		/// <summary>
		/// The static instance of the PlayerDatabase.
		/// </summary>
		private static PlayerDatabase instance;

		/// <summary>
		/// Singleton constructor. It initializes the instance on the first get.
		/// </summary>
		public static PlayerDatabase Instance
		{
			get
			{
				if (instance == null)
					instance = new PlayerDatabase();
				return instance; 
			}
			private set { }
		}

		/// <summary>
		/// The default constructor loads the database from a file.
		/// </summary>
		private PlayerDatabase()
		{
			//if (System.IO.File.Exists(DATABASE_FILE_NAME))
			//	LoadDatabase();
			dict = new ConcurrentDictionary<string, Player>();
			database = new XElement("Players");
		}

		/// <summary>
		/// Reset the database.
		/// </summary>
		public void Reset()
		{
			dict = new ConcurrentDictionary<string, Player>();
			database = new XElement("Players");
		}

		/// <summary>
		/// Create a test database with two players.
		/// </summary>
		public void CreateTestDatabase()
		{
			IDictionary<String, Player> players = new ConcurrentDictionary<String, Player>();
			players.Add("James", new Player(new Position(200, 200), new Velocity(0, 0), "James", "jamespass"));
			players.Add("Gleb", new Player(new Position(400, 300), new Velocity(0, 0), "Gleb", "glebpass"));

			CreateDatabase(players);
		}

		/// <summary>
		/// Creates a database from the List of players.
		/// </summary>
		public void CreateDatabase(IDictionary<String, Player> players)
		{
			dict = new ConcurrentDictionary<String, Player>(players);

			foreach (var player in players)
			{
				Player value = player.Value;
				XElement p = new XElement("Player");
				p.Add(new XElement("Name", value.Name));
				p.Add(new XElement("Password", value.Password));

				XElement position = new XElement("Position");
				position.Add(new XElement("X", value.Position.x));
				position.Add(new XElement("Y", value.Position.y));
				p.Add(position);

				XElement velocity = new XElement("Velocity");
				velocity.Add(new XElement("X", value.Velocity.x));
				velocity.Add(new XElement("Y", value.Velocity.y));
				p.Add(velocity);

				database.Add(p); 
			}
		}

		/// <summary>
		/// Saves the database to a file.
		/// </summary>
		public void SaveDatabase()
		{
			database.Save(DATABASE_FILE_NAME);
		}

		/// <summary>
		/// Loads the database from a file.
		/// </summary>
		/// <returns>Returns a list of the players loaded from the database.</returns>
		public IList<Player> LoadDatabase()
		{
			database = XElement.Load(DATABASE_FILE_NAME);

			IList<Player> players = new List<Player>();

			foreach (var player in database.Descendants("Player"))
				player.Elements("Player");
			return players;
		}

		/// <summary>
		/// Returns a list of all of the players in the database.
		/// </summary>
		/// <returns>A list of all the players in the database.</returns>
		public IList<Player> GetPlayers()
		{
			List<Player> list = new List<Player>();
			foreach (var p in dict)
				list.Add(p.Value);
			return list;
		}

		/// <summary>
		/// Returns a list of players near the player with name name. Currently this returns all players.
		/// </summary>
		/// <param name="name">The name of the player to search nearby.</param>
		/// <returns>A list of nearby players.</returns>
		public List<Player> GetNearbyPlayers(String name)
		{
			List<Player> nearby = new List<Player>();
			foreach (var p in dict)
				if (p.Value.Name != name)
					nearby.Add(p.Value);
			return nearby;
		}

		/// <summary>
		/// Searches for a player by name.
		/// </summary>
		/// <param name="name">The name of the player for which to search.</param>
		/// <returns>The Player if it is found, null otherwise.</returns>
		public Player FindPlayer(String name)
		{
			Player player;
			if (dict.TryGetValue(name, out player))
				return player;
			else
				return null;
		}

		/// <summary>
		/// Updates player data.
		/// </summary>
		/// <param name="player"></param>
		public void UpdatePlayer(Player player)
		{
			Player result;
			if (dict.TryGetValue(player.Name, out result))
				dict.TryUpdate(player.Name, player, result);
		}

		public bool TryUpdate(string key, Player newValue, Player comparisonValue)
		{
			return dict.TryUpdate(key, newValue, comparisonValue);
		}

		class PlayerDoesNotExistException : ApplicationException
		{
		}

		public bool IsLoggedIn(string name)
		{
			Player player;
			if (!dict.TryGetValue(name, out player))
				throw new PlayerDoesNotExistException();

			return player.LoggedIn;
		}

		public bool LogIn(string name)
		{
			Player player, updatedPlayer;
			if (!dict.TryGetValue(name, out player))
				throw new PlayerDoesNotExistException();

			if (player.LoggedIn)
				return false;
			else
			{
				updatedPlayer = new Player(player);
				updatedPlayer.LoggedIn = true;
				//If the update failed then the player has been modified (we are assuming
				//that LoggedIn was changed to false
				if (!dict.TryUpdate(name, updatedPlayer, player))
					return false;
				else
					return true;

			}
		}

	}
}
