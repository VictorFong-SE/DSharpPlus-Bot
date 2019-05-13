/*
 * Class: Commands
 *
 * Purpose: Holds and executes commands triggered by users.
 * by: Victor Fong, vfong3, 665878537

 */


using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

namespace Discord_Bot
{
   public class CommandsUngrouped : BaseCommandModule
   {
      //attributes
      [Command("ping")]                              //define method
      [Description("pongs when pinged.")]            //describe command
      [Aliases("pong", "ping-pong")]                 //any other words for command
      public async Task ping(CommandContext context) //no additional arguments are taken
      {
         await context.TriggerTypingAsync(); //show in discord as typing

         var emoji = DiscordEmoji.FromName(context.Client, ":ping_pong:");     //creat emoji just cuz
         await context.RespondAsync($"{emoji} pong! {context.Client.Ping}ms"); //respond
      }

      [Command("ping")] //overloads allowed in 4.x
      public async Task ping(CommandContext context, [RemainingText] [Description("string to be copied.")]
                             string copy)
      {
         await context.TriggerTypingAsync(); //show in discord as typing

         var emoji = DiscordEmoji.FromName(context.Client,
                                           ":ping_pong:");                                         //creat emoji just cuz
         await context.RespondAsync($"{emoji} ponging back your string! {context.Client.Ping}ms"); //respond
         await context.RespondAsync(copy);                                                         //respond
      }

      [Command("roll")]
      [Description("rolls a die of selected size. (Currently supports a single 6, or 20 sided die.")]
      [Aliases("r")]
      public async Task Random(CommandContext context, [Description("type of die to be rolled.")]
                               string die)
      {
         await context.TriggerTypingAsync(); //show in discord as typing
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


   [Group("admin")]
   [Description("Administrative commands.")]
   [Hidden]
   [RequirePermissions(Permissions.ManageChannels)] //only members who can manage channels have these perms
   public class CommandsAdmin : BaseCommandModule
   {
      // all the commands will need to be executed as <prefix>admin <command> <arguments>

      // this command will be only executable by the bot's owner
      [Command("sudo")]
      [Description("Executes a command as another user.")]
      [Hidden]
      [RequireOwner]
      public async Task SudoAsync(CommandContext context, [Description("Member to execute as.")] DiscordMember member,
                                  [RemainingText, Description("Command text to execute.")]
                                  string commandString)
      {
         // note the [RemainingText] attribute on the argument.
         // it will capture all the text passed to the command

         // let's trigger a typing indicator to let
         // users know we're working
         await context.TriggerTypingAsync();

         var command = context.CommandsNext.FindCommand(commandString, out var args);
         if (command == null)
            throw new CommandNotFoundException(commandString);

         var fakeContext = context.CommandsNext.CreateFakeContext(member, context.Channel, commandString,
                                                                  context.Prefix,
                                                                  command, args);
         await context.CommandsNext.ExecuteCommandAsync(fakeContext).ConfigureAwait(false);
      }


      [Command("nick")]
      [Aliases("nickname")]
      [Description("Changes a persons nickname.")]
      [RequirePermissions(Permissions.ManageNicknames)]
      public async Task NicknameAsync(CommandContext context,
                                      [Description("Name of current members name")]
                                      DiscordMember discordMember,
                                      [RemainingText] [Description("New nickname for member.")]
                                      string newNickname)
      {
         await context.TriggerTypingAsync();
         try
         {
            // let's change the nickname, and tell the 
            // audit logs who did it.
            await discordMember.ModifyAsync(givenMember =>
                                            {
                                               givenMember.Nickname = newNickname;
                                               givenMember.AuditLogReason =
                                                  string.Concat("Edited by ", context.User.Username, "#",
                                                                context.User.Discriminator, " (",
                                                                context.User.Id, ")");
                                            }).ConfigureAwait(false);

            // let's make a simple response.
            var emoji = DiscordEmoji.FromName(context.Client, ":+1:");

            // and respond with it.
            await context.RespondAsync(emoji);
         }
         catch (Exception exception)
         {
            // oh no, something failed, let the invoker now
            var emoji = DiscordEmoji.FromName(context.Client, ":-1:");
            await context.RespondAsync(emoji);
         }
      }


      [Command("messageWarn")]
      [Description(
         "Used by moderators to delete a message, log the message in the audit log, and DM the user a warning")]
      [Hidden]
      [RequirePermissions(Permissions.ManageMessages)]
      public async Task messageWarn(CommandContext                                       context,
                                    [Description("Message ID")]           DiscordMessage badMessage,
                                    [Description("Warning to give user")] string         warningMessage)
      {
         await context.TriggerTypingAsync();

         await badMessage.DeleteAsync();
      }
   }
}