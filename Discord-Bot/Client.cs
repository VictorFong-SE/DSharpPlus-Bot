/*
 * Class: Client
 *
 * Purpose: main async class. Initializes bot and begins async operations.
 *
 * by: Victor Fong, vfong3, 665878537
 */

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;

namespace Discord_Bot
{
    public class Client
    {
        public DiscordClient          client        { get; set; } // discord client itself
        public InteractivityExtension interactivity { get; set; } //module to handle interactible commands
        public CommandsNextExtension  commands      { get; set; } // module to overwatch general commands


        public async Task MainAsync()
        {
            // init config
            if (!File.Exists("config.json"))
            {
                #region !! config unavailable !!

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;

                centerWrite("▒▒▒▒▒▒▒▒▒▄▄▄▄▒▒▒▒▒▒▒", 2);
                centerWrite("▒▒▒▒▒▒▄▀▀▓▓▓▀█▒▒▒▒▒▒");
                centerWrite("▒▒▒▒▄▀▓▓▄██████▄▒▒▒▒");
                centerWrite("▒▒▒▄█▄█▀░░▄░▄░█▀▒▒▒▒");
                centerWrite("▒▒▄▀░██▄░░▀░▀░▀▄▒▒▒▒");
                centerWrite("▒▒▀▄░░▀░▄█▄▄░░▄█▄▒▒▒");
                centerWrite("▒▒▒▒▀█▄▄░░▀▀▀█▀▒▒▒▒▒");
                centerWrite("▒▒▒▄▀▓▓▓▀██▀▀█▄▀▀▄▒▒");
                centerWrite("▒▒█▓▓▄▀▀▀▄█▄▓▓▀█░█▒▒");
                centerWrite("▒▒▀▄█░░░░░█▀▀▄▄▀█▒▒▒");
                centerWrite("▒▒▒▄▀▀▄▄▄██▄▄█▀▓▓█▒▒");
                centerWrite("▒▒█▀▓█████████▓▓▓█▒▒");
                centerWrite("▒▒█▓▓██▀▀▀▒▒▒▀▄▄█▀▒▒");
                centerWrite("▒▒▒▀▀▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
                Console.BackgroundColor = ConsoleColor.Yellow;
                centerWrite("WARNING", 3);
                Console.ResetColor();
                centerWrite("Thank you Mario!", 1);
                centerWrite("But our config.json is in another castle!");
                centerWrite("(Please place config.json within working directory with working values.)", 2);
                centerWrite("Press any key to exit..",                                                  1);
                Console.SetCursorPosition(0, 0);
                Console.ReadKey();

                #endregion

                Environment.Exit(0);
            }

            string json;
            using (var fileLoc = File.OpenRead("config.json"))
                using (var streamReader = new StreamReader(fileLoc, new UTF8Encoding(false)))
                    json = await streamReader.ReadToEndAsync();

            // load values from json file
            var jsonConfig = JsonConvert.DeserializeObject<JsonConfig>(json);
            var config = new DiscordConfiguration
                         {
                             Token     = jsonConfig.Token, //set token
                             TokenType = TokenType.Bot,

                             AutoReconnect         = true, // try to reconnect if connection lost
                             UseInternalLogHandler = true,
                             LogLevel              = LogLevel.Debug //show log in terminal
                         };

            client = new DiscordClient(config);

            //setup client debugging
            client.Ready          += discordClientReady;
            client.GuildAvailable += discordClientHasGuild;
            client.ClientErrored  += discordClientError;
            
            //initialize interactivity
            client.UseInteractivity(new InteractivityConfiguration
                                    {
                                        //by default, commands should ignore reactions
                                        PaginationBehaviour = PaginationBehaviour.Ignore,
                                        //default timeout
                                        Timeout = TimeSpan.FromMinutes(2)
                                    });
            
            //initialize commands
            var commandConfig = new CommandsNextConfiguration
                                {
                                    StringPrefixes      = jsonConfig.CommandPrefixes,
                                    EnableDms           = true,
                                    EnableMentionPrefix = true
                                };
            commands = client.UseCommandsNext(commandConfig);

            //setup command debugging
            commands.CommandExecuted += commandExecuted;
            commands.CommandErrored  += commandError;

            //register commands
            commands.RegisterCommands<Commands>();
            commands.RegisterCommands<InteractiveCommands>();


            //choose help formatter (default built into api)
            commands.SetHelpFormatter<DefaultHelpFormatter>();


            await client.ConnectAsync(); //connect bot

            //If doing something with bot here is the place to do it

            await Task.Delay(-1); //prevent death
        }


        /*#######---Helper Functions---#######*/
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
            events.Client.DebugLogger.LogMessage(LogLevel.Info, "Patto", $"is in server: {events.Guild.Name}",
                                                 DateTime.Now);

            //if a method is not async, a Task must be returned
            return Task.CompletedTask;
        }

        private Task discordClientError(ClientErrorEventArgs events)
        {
            //log error details
            events.Client.DebugLogger.LogMessage(LogLevel.Error, "Patto",
                                                 $"Exception occured: {events.Exception.GetType()}: {events.Exception.Message}",
                                                 DateTime.Now);

            //if a method is not async, a Task must be returned
            return Task.CompletedTask;
        }


        private Task commandExecuted(CommandExecutionEventArgs events)
        {
            // log command details
            events.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "Patto",
                                                         $"{events.Context.User.Username} executed command '{events.Command.QualifiedName}'",
                                                         DateTime.Now);

            //if a method is not async, a Task must be returned
            return Task.CompletedTask;
        }

        private async Task commandError(CommandErrorEventArgs events)
        {
            // log error details
            events.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "Patto",
                                                         $"{events.Context.User.Username} tried executing '{events.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {events.Exception.GetType()}: {events.Exception.Message ?? "<no message>"}",
                                                         DateTime.Now);

            // check for proper perms
            if (events.Exception is ChecksFailedException)
            {
                var emoji = DiscordEmoji.FromName(events.Context.Client, ":no_entry:");

                //wrap response into an embed
                var embed = new DiscordEmbedBuilder
                            {
                                Title       = "Access denied",
                                Description = $"{emoji} Maybe when you're older... (incorrect permissions)",
                                Color       = new DiscordColor(0xFF0000) // choose color (red)
                            };
                await events.Context.RespondAsync("", embed: embed);
            }
        }


        private static void centerWrite(string buffer, int linesToSkip = 0) // writes to center of console
        {
            for (var i = 0; i < linesToSkip; i++)
                Console.WriteLine();

            Console.SetCursorPosition((Console.WindowWidth - buffer.Length) / 2, Console.CursorTop);
            Console.WriteLine(buffer);
        }
    }


    public struct JsonConfig // struct created to hold token
    {
        [JsonProperty("token")] public string Token { get; private set; }

        [JsonProperty("commandPrefixes")] public string[] CommandPrefixes { get; private set; }
    }
}