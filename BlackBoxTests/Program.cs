using VeraServer;

namespace BlackBoxTests
{
	class Program
	{
		static void Main(string[] args)
		{
			//Objects are references, meaning that as long as we do not make a new object and clone the old one
			//we will always have a reference to the original, even if it is changed by another thread.
			/*
			PlayerDatabase db = PlayerDatabase.Instance;
			db.CreateTestDatabase();
			var players = db.GetPlayers();
			Player derp = players[0];
			System.Console.WriteLine(derp.Name);

			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(dothingy));
			thread.Start();
			System.Threading.Thread.Sleep(3000);

			//players = db.GetPlayers();
			
			System.Console.WriteLine(derp.Name);
			*/

            BlackBoxTests test = new BlackBoxTests();
            test.Start();

			ListenServer listenServer = new ListenServer();
			listenServer.Run();
		}

		/*
		static void dothingy()
		{
			PlayerDatabase db = PlayerDatabase.Instance;

			System.Collections.Generic.IList<Player> list = db.GetPlayers();
			list[0].Name = "Sonichu";
		}
		*/
	}
}
