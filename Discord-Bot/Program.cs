/*
 * Class: Program
 *
 * Purpose: main class, calls client to begin bot.
 */

namespace Discord_Bot
{
	class Program
	{	
		public static void Main(string[] args)
		{
			//we cannot make main asynchronous, so will pass execution to asynchronous method
			var client = new Client();
			client.MainAsync().Wait();
		}
	}
}