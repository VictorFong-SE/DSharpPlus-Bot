/*
 * Class: Commands
 *
 * Purpose: Holds and executes commands triggered by users.
 * by: Victor Fong, vfong3, 665878537

 */


using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;


namespace Discord_Bot
{
	public class Commands
	{
		[Command("ping")] //define method
		[Description("pongs when pinged.")] //describe command
		[Aliases("pong", "ping-pong")] //any other words for command
		public async Task ping(CommandContext context) //no additional arguments are taken
		{
			await context.TriggerTypingAsync(); //show in discord as typing

			var emoji = DiscordEmoji.FromName(context.Client, ":ping_pong:");	//creat emoji just cuz
			await context.RespondAsync($"{emoji} pong! {context.Client.Ping}ms");	//respond
		}
		
		
		[Command("roll")]
		[Description("rolls a die of selected size. (Currently supports a single 6, or 20 sided die.")]
		[Aliases("r")]
		public async Task Random(CommandContext context, string die)
		{
			var random = new Random();
			switch (die)
			{
				case "d6":
				{
					await context.RespondAsync($"🎲 You rolled a : {random.Next(1, 7)}");
					break;
				}
				case "d20":
				{
					await context.RespondAsync($"🎲 You rolled a : {random.Next(1, 21)}");
					break;
				}
				default:
				{
					await context.RespondAsync("you don't seem to have this kind of die, go buy one!");
					break;
				}
			}
		}
	}
}