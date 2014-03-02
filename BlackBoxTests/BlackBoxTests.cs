using System.Threading;

using System.Diagnostics;

namespace BlackBoxTests
{
    /// <summary>
    /// This class runs black-box tests on the server. It is meant to simulate network functionality
    /// like multiple connections, dropped connections, and illicit connections.
    /// </summary>
    public class BlackBoxTests
    {
        /// <summary>
        /// The thread that will be running the tests.
        /// </summary>
        Thread thread = new Thread(new ThreadStart(RunAllTests));

        /// <summary>
        /// Start the tests in a new thread.
        /// </summary>
        public void Start()
        {
            thread.Start();
        }

		private const string FLASH_DEBUGGER_PATH = "E:/Program Files (x86)/FlashDevelop/Tools/flexlibs/runtimes/player/11.3/win/FlashPlayerDebugger.exe";
		private const string GLEBFORGE_PATH = "E:/Coolege/GlebForge/bin/GlebForge.swf";

        /// <summary>
        /// This method launches the game in FlashPlayerDebugger. Note that the file location is machine-dependent.
        /// </summary>
        private static void LaunchGame(uint numInstances)
        {
			Process[] processes = new Process[numInstances];
			for (int i = 0; i < numInstances; i++)
			{
				processes[i] = new Process();
				processes[i].StartInfo = new ProcessStartInfo(FLASH_DEBUGGER_PATH, GLEBFORGE_PATH);
				processes[i].StartInfo.UseShellExecute = false;
				processes[i].Start();
			}
        }

        /// <summary>
        /// The thread start location.
        /// </summary>
        private static void RunAllTests()
        {
			RunFlashTests();
        }

		private static void RunSimulatedTests()
		{
			ServerTests.RunTests();
		}

		private static void RunFlashTests()
		{
			LaunchGame(2);
		}
    }
}
