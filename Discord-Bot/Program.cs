/*
 * Class: Program
 *
 * Purpose: main class. Initializes bot and begins async operations.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace Discord_Bot
{
	class Program
	{
		public DiscordClient discordClient { get; set; }	// discord client itself
		public CommandsNextModule commands { get; set; }	// module to overwatch commands
		
		public static void Main(string[] args)
		{
			//we cannot make main asynchronous, so will pass execution to asynchronous method
			var program = new Program();
			
			program.MainAsync()
				.GetAwaiter()
				.GetResult();
		}


		public async Task MainAsync()
		{
			// init config
			var json = "";
			using (var fileLoc = File.OpenRead("config.json"))
			using (var streamReader = new StreamReader(fileLoc, new UTF8Encoding(false)))
				json = await streamReader.ReadToEndAsync();
			
			// load values from json file
			var jsonConfig = JsonConvert.DeserializeObject<JsonConfig>(json);
			var config = new DiscordConfiguration
			{
				Token = jsonConfig.Token,	//set token
				TokenType = TokenType.Bot,
				
				AutoReconnect = true,	// try to reconnect if connection lost
				UseInternalLogHandler = true,
				LogLevel = LogLevel.Debug	//show log in terminal
			};
			
			discordClient = new DiscordClient(config);
			
			//setup client debugging
			discordClient.Ready += discordClientReady;
			discordClient.GuildAvailable += discordClientHasGuild;
			discordClient.ClientErrored += discordClientError;
			
			

			//initialize commands
			var commandConfig = new CommandsNextConfiguration
			{
				StringPrefix = jsonConfig.CommandPrefix,
				EnableDms = true,
				EnableMentionPrefix = true
			};
			commands = discordClient.UseCommandsNext(commandConfig);
			
			//setup command debugging
			commands.CommandExecuted += commandExecuted;
			commands.CommandErrored += commandError;	

			
			//register commands
			commands.RegisterCommands<UngroupedCommands>();
			
			//choose help formatter (default built into api)
			commands.SetHelpFormatter<DefaultHelpFormatter>();
			
			await discordClient.ConnectAsync();	//connect bot
			await Task.Delay(-1);	//prevent death
		}
		
		
		
		private Task discordClientReady(ReadyEventArgs events)
		{
			// let's log the fact that this event occured
			events.Client.DebugLogger.LogMessage(LogLevel.Info, "Patto", "is ready to roll!", DateTime.Now);

			//if a method is not async, a Task must be returned
			return Task.CompletedTask;
		}

		private Task discordClientHasGuild(GuildCreateEventArgs events)
		{
			// log name of server requesting bot
			events.Client.DebugLogger.LogMessage(LogLevel.Info, "Patto", $"is in server: {events.Guild.Name}", DateTime.Now);

			//if a method is not async, a Task must be returned
			return Task.CompletedTask;
		}		
		
		private Task discordClientError(ClientErrorEventArgs events)
		{
			//log error details
			events.Client.DebugLogger.LogMessage(LogLevel.Error, "Patto", $"Exception occured: {events.Exception.GetType()}: {events.Exception.Message}", DateTime.Now);

			//if a method is not async, a Task must be returned
			return Task.CompletedTask;
		}
		
		
		private Task commandExecuted(CommandExecutionEventArgs events)
		{
			// log command details
			events.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "Patto", $"{events.Context.User.Username} executed command '{events.Command.QualifiedName}'", DateTime.Now);

			//if a method is not async, a Task must be returned
			return Task.CompletedTask;
		}

		private async Task commandError(CommandErrorEventArgs events)
		{
			// log error details
			events.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "Patto", $"{events.Context.User.Username} tried executing '{events.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {events.Exception.GetType()}: {events.Exception.Message ?? "<no message>"}", DateTime.Now);

			// check for proper perms
			if (events.Exception is ChecksFailedException exception)
			{
				var emoji = DiscordEmoji.FromName(events.Context.Client, ":no_entry:");

				//wrap response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} Maybe when you're older... (incorrect permissions)",
					Color = new DiscordColor(0xFF0000) // choose color (red)

				};
				await events.Context.RespondAsync("", embed: embed);
			}
		}
	}

	
	public struct JsonConfig	// struct created to hold token
	{
		[JsonProperty("token")]
		public string Token { get; private set; }
		
		[JsonProperty("commandPrefix")]
		public string CommandPrefix { get; private set; }
	}
}