/*
 * Class: Program
 *
 * Purpose: main class, calls client to begin bot.
 *
 * by: Victor Fong, vfong3, 665878537
 */

namespace Discord_Bot
{
	internal static class Program
	{	
		public static void Main(string[] args)
		{
			//we cannot make main asynchronous, so will pass execution to asynchronous method
			var discordClient = new Client();
			discordClient
				.MainAsync()
				.GetAwaiter()
				.GetResult();
		}
	}
}