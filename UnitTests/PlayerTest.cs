using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VeraServer;

namespace UnitTests
{
    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        public void Player()
        {
            Player player = new Player();
            Assert.AreEqual<String>(player.Name, "NoName");
            Assert.AreEqual<Position>(player.Position, new Position());
            Assert.AreEqual<Velocity>(player.Velocity, new Velocity());

            Position pos = new Position(10.0f, 15.0f);
            Velocity vel = new Velocity(-1.0f, 4.0f);
            String name = "hello thar";
			String pass = "buttsacks";
            Player player2 = new Player(pos, vel, name, pass);
            Assert.AreEqual<String>(player2.Name, name);
            Assert.AreEqual<Position>(player2.Position, pos);
            Assert.AreEqual<Velocity>(player2.Velocity, vel);
        }
    }
}
