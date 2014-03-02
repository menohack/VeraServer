using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VeraServer
{
	public struct Position
	{
		public float x;
		public float y;

		public Position(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public struct Velocity
	{
		public float x;
		public float y;

		public Velocity(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class Player
	{
		public Position Position { get; set; }

		public Velocity Velocity { get; private set; }

		public const int MAX_PLAYER_NAME_LENGTH = 32;

		public String Name { get; private set; }

		public String Password { get; private set; }

		private bool loggedIn = false;

		public bool LoggedIn
		{
			get
			{
				return loggedIn;
			}
			set
			{
				Console.WriteLine("{0} set to logged in", Name);
				loggedIn = value;
			}
		}


		public Player(Position position, Velocity velocity, String name, String password)
		{
			this.Position = position;
			this.Velocity = velocity;
			this.Name = name;
			
			this.Password = password.PadRight(128);
		}

		public Player()
		{
			Position = new Position();
			Velocity = new Velocity();
			Name = "NoName";
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="copy">The Player object to copy into the new one.</param>
		public Player(Player copy)
		{
			this.Position = copy.Position;
			this.Velocity = copy.Velocity;
			this.Name = String.Copy(copy.Name);
			this.Password = String.Copy(copy.Password);
			this.loggedIn = copy.loggedIn;
		}
	}
}
