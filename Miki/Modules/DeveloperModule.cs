﻿using Discord;
using Discord.WebSocket;
using IA;
using IA.Events;
using IA.SDK;
using IA.SDK.Events;
using IA.SDK.Extensions;
using IA.SDK.Interfaces;
using Miki.Accounts;
using Miki.Accounts.Achievements;
using Miki.Languages;
using Miki.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Miki.Modules
{
    internal class DeveloperModule
    {
        public async Task LoadEvents(Bot bot)
        {
            // TODO: Change to SDK
            await new RuntimeModule(module =>
            {
                module.Name = "Experimental";
                module.Events = new List<ICommandEvent>()
                {
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "dumpshards";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            EmbedBuilder embed = new EmbedBuilder();
                            embed.Title = "Shards";

                            foreach(DiscordSocketClient c in bot.Client.Shards)
                            {
                                embed.AddField(f =>
                                {
                                    f.Name = "Shard " + c.ShardId;
                                    f.Value = $"State:  {c.ConnectionState}\nPing:   {c.Latency}\nGuilds: {c.Guilds.Count}";
                                    f.IsInline = true;
                                });
                            }

                            await e.Channel.SendMessage(new RuntimeEmbed(embed));
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "changeavatar";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            Image s = new Image(new FileStream("./" + e.arguments, FileMode.Open));

                            await bot.Client.GetShard(e.message.Discord.ShardId).CurrentUser.ModifyAsync(z =>
                            {
                                z.Avatar = new Optional<Image?>(s);
                            });
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "changename";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await bot.Client.GetShard(e.message.Discord.ShardId).CurrentUser.ModifyAsync(z =>
                             {
                                 z.Username = e.arguments;
                             });
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "setexp";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            using(var context = new MikiContext())
                            {
                                LocalExperience u = await context.Experience.FindAsync(e.Guild.Id.ToDbLong(), e.message.MentionedUserIds.First().ToDbLong());
                                if(u == null)
                                {
                                    return;
                                }
                                u.Experience = int.Parse(e.arguments.Split(' ')[1]);
                                await context.SaveChangesAsync();
                            }
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "setgame";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await e.message.Discord.SetGameAsync(e.arguments);
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "setstream";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await e.message.Discord.SetGameAsync(e.arguments, "https://www.twitch.tv/velddev");
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "testnotification";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await Notification.SendPM(e.Author.Id, e.arguments);
                        };
                    }),
                    new RuntimeCommandEvent("commandsystemtest")
                        .SetAccessibility(EventAccessibility.DEVELOPERONLY)
                        .Default(async (e) =>
                        {
                            await Utils.Embed.SetColor(IA.SDK.Color.GetColor(IAColor.ORANGE)).SetDescription("This is the default command param").SendToChannel(e.Channel.Id);
                        })
                        .On("?", async (e) =>
                        {
                            await Utils.Embed.SetDescription("? was triggered").SendToChannel(e.Channel.Id);
                        })
                        .On("say", async (e) =>
                        {
                            await Utils.Embed.SetTitle("SAY").SetDescription(e.arguments).SendToChannel(e.Channel.Id);
                        }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "stats";
                        x.ProcessCommand = async (e) =>
                        {
                            //int servers = bot.Client.Guilds.Count;
                            //int channels = bot.Client.Guilds.Sum(a => a.Channels.Count);
                            //int members = bot.Client.Guilds.Sum(a => a.Channels.Sum(b => b.Users.Count));

                            TimeSpan timeSinceStart = DateTime.Now.Subtract(Program.timeSinceStartup);

                            IDiscordEmbed embed = new RuntimeEmbed(new EmbedBuilder());
                            embed.Title = "⚙️ Miki stats";
                            embed.Description = "General realtime stats about miki!";
                            embed.Color = new IA.SDK.Color(0.3f, 0.8f, 1);

                            //embed.AddField(f =>
                            //{
                            //    f.Name = "🖥️ Servers";
                            //    f.Value = servers.ToString();
                            //    f.IsInline = true;
                            //});

                            //embed.AddField(f =>
                            //{
                            //    f.Name = "📺 Channels";
                            //    f.Value = channels.ToString();
                            //    f.IsInline = true;
                            //});

                            //embed.AddField(f =>
                            //{
                            //    f.Name = "👤 Users";
                            //    f.Value = members.ToString();
                            //    f.IsInline = true;
                            //});

                            //embed.AddField(f =>
                            //{
                            //    f.Name = "🐏 Ram";
                            //    f.Value = (memsize / 1024 / 1024).ToString() + "MB";
                            //    f.IsInline = true;
                            //});

                            //embed.AddField(f =>
                            //{
                            //    f.Name = "👷 Threads";
                            //    f.Value = threads.ToString();
                            //    f.IsInline = true;
                            //});

                            embed.AddField(f =>
                            {
                                f.Name = "💬 Commands";
                                f.Value = bot.Events.CommandsUsed().ToString();
                                f.IsInline = true;
                            });

                            embed.AddField(f =>
                            {
                                f.Name = "⏰ Uptime";
                                f.Value = timeSinceStart.ToTimeString();
                                f.IsInline = true;
                            });

                            await e.Channel.SendMessage(embed);
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "unload";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await bot.Addons.Unload(bot, e.arguments);
                            await e.Channel.SendMessage($"Unloaded Add-On \"{e.arguments}\" successfully");
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "load";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await bot.Addons.LoadSpecific(bot, e.arguments);
                            await e.Channel.SendMessage($"Loaded Add-On \"{e.arguments}\" successfully");
                        };
                    }),
                    new RuntimeCommandEvent("cmdtest")
                        .SetAccessibility(EventAccessibility.DEVELOPERONLY)
                        .Default(DoCmdTest),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "reload";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await bot.Addons.Unload(bot, e.arguments);
                            await bot.Addons.LoadSpecific(bot, e.arguments);
                            await e.Channel.SendMessage($"Reloaded {e.arguments} successfully!");
                        };
                    }),
                    new CommandEvent(x =>
                    {
                        x.Name = "queryembed";
                        x.ProcessCommand = async (e) =>
                        {
                            await e.Channel.SendMessage(new RuntimeEmbed(new EmbedBuilder()).Query(e.arguments));
                        };
                    }),
                    new RuntimeCommandEvent(x =>
                    {
                        x.Name = "mtou";
                        x.Accessibility = EventAccessibility.DEVELOPERONLY;
                        x.ProcessCommand = async (e) =>
                        {
                            await e.Channel.SendMessage(e.message.RemoveMentions());
                        };
                    })
                };
            }).InstallAsync(bot);
        }

        private async Task DoCmdTest(EventContext message)
        {
            CommandHandler c = new CommandHandlerBuilder()
                .AddPrefix(">")
                .DisposeInSeconds(20)
                .SetOwner(message.message)
                .AddCommand(
                    new RuntimeCommandEvent("yes")
                        .Default(async (e) => 
                        {
                            await e.Channel.SendMessage("you picked yes!");
                            e.commandHandler.RequestDispose();
                        }))
                .AddCommand(
                    new RuntimeCommandEvent("no")
                        .Default(async (e) => 
                        {
                            await e.Channel.SendMessage("you picked no!");
                            e.commandHandler.RequestDispose();
                        }))
                .Build();

            Bot.instance.Events.AddPrivateCommandHandler(message.message, c);
            await message.Channel.SendMessage("OK!");
        }
    }
}