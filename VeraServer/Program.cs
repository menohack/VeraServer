namespace VeraServer
{
	class Program
	{
		static void Main(string[] args)
		{
			//Console.WriteLine("isLittleEndian: {0}", BitConverter.IsLittleEndian);
			ListenServer listenServer = new ListenServer();
			listenServer.Run();
		}
	}
}
