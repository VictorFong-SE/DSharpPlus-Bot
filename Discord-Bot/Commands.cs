/*
 * Class: Commands
 *
 * Purpose: Holds and executes commands triggered by users.
 */


using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;


namespace Discord_Bot
{
	public class UngroupedCommands
	{
		[Command("ping")] //define method
		[Description("pongs when pinged")] //describe command
		[Aliases("pong", "ping-pong")] //any other words for command
		public async Task ping(CommandContext context) //no additional arguments are taken
		{
			await context.TriggerTypingAsync(); //show in discord as typing

			var emoji = DiscordEmoji.FromName(context.Client, ":ping_pong:");	//creat emoji just cuz
			await context.RespondAsync($"{emoji} pong! {context.Client.Ping}ms");	//respond
		}
	}
}