using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VeraServer;
using System.Collections.Generic;

namespace UnitTests
{
	[TestClass]
	public class PlayerDatabaseTest
	{

		PlayerDatabase der = PlayerDatabase.Instance;
		Player player1, player2;

		[TestInitialize]
		public void Initialize()
		{
			IDictionary<String, Player> players = new Dictionary<String, Player>();
			player1 = new Player(new Position(10, 54), new Velocity(0, 33), "Test", "nutspass");
			player2 = new Player(new Position(-1, 44), new Velocity(-32, 99), "tickles", "ballspass");
			players["Test"] = player1;
			players["tickles"] = player2;
			der.CreateDatabase(players);
		}

		[TestMethod]
		public void CreateDatabaseTest()
		{
			//Not sure how to test this just yet
			Assert.Fail("Method not implemented yet.");
		}


		[TestMethod]
		public void FindPlayerTest()
		{
			//Check when the database is empty
			Assert.IsNull(der.FindPlayer("James"));
			Assert.IsNull(der.FindPlayer("Gleb"));
			Assert.IsNull(der.FindPlayer(""));
			Assert.IsNull(der.FindPlayer("wjkefhawiehfuiw"));

			//Check with two players
			Assert.AreEqual<Player>(der.FindPlayer("tickles"), player2);
			Assert.AreEqual<Player>(der.FindPlayer("Test"), player1);
			Assert.AreNotEqual<Player>(der.FindPlayer("tickles"), der.FindPlayer("Test"));
			Assert.AreNotEqual<Player>(der.FindPlayer("Test"), der.FindPlayer("awuehfiwuf"));
		}

		[TestMethod]
		public void GetNearbyPlayersTest()
		{
			Assert.IsTrue(der.GetNearbyPlayers("Test").Count == 1);
			Assert.IsTrue(der.GetNearbyPlayers("tickles").Count == 1);
			Assert.IsTrue(der.GetNearbyPlayers("Test").Contains(player2));
			Assert.IsTrue(der.GetNearbyPlayers("tickles").Contains(player1));

			der.Reset();
			Assert.IsTrue(der.GetNearbyPlayers("Test").Count == 0);
		}

		[TestMethod]
		public void UpdatePlayerTest()
		{
			Assert.Fail("Method not implemented.");
		}
	}
}
