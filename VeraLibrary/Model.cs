using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace VeraLibrary
{
	public enum TerrainType
	{
		Ore, Trees, Water, Fertile
	}

	public interface Model
	{
		TerrainType RandomTerrain();
	}

	public class Game : Model
	{
		Random rand = new Random();

		TerrainType[] terrainTypes = { TerrainType.Ore, TerrainType.Trees, TerrainType.Water, TerrainType.Fertile };

		GameData gameData;

		public Game()
		{
			LoadGame();
		}

		private void LoadGame()
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(GameData));
			XmlTextReader stream = new XmlTextReader(File.OpenText("GameData.xml"));
			gameData = serializer.ReadObject(stream) as GameData;
		}

		private void SaveGame()
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(GameData));
			StreamWriter fs = File.CreateText("GameData.xml");
			XmlTextWriter writer = new XmlTextWriter(fs);
			writer.Formatting = Formatting.Indented;
			gameData = new GameData();
			gameData.terrain.Add(TerrainType.Fertile, new Tuple<int, int>(0, 0));
			serializer.WriteObject(writer, gameData);
			writer.Close();
		}

		[DataContract(Name="Ballz")]
		public class GameData
		{
			[DataMember(Name="Derp")]
			public Dictionary<TerrainType, Tuple<int, int>> terrain = new Dictionary<TerrainType, Tuple<int, int>>();
		}

		public TerrainType RandomTerrain()
		{
			return terrainTypes[rand.Next(terrainTypes.Length)];
		}
	}
}

