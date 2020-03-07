using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Selenium
{
    /// <summary>
    /// Selenium's entry point class
    /// </summary>
    internal class Bot
    {
        #region serilog setup

        // logger output template
        private const string OutputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public static readonly Logger Logger = new LoggerConfiguration()
            // log **everything**
            .MinimumLevel.Verbose()

            // obvs write to console
            .WriteTo.Console()
            // async file writing for the file logger
            .WriteTo.Async(wt => wt.File(
                // serilog automatically sticks the timestamp on the end of the file name
                "logs/selenium-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Verbose,
                outputTemplate: OutputTemplate))

            // build logger
            .CreateLogger();

        #endregion

        private static readonly Dictionary<LogSeverity, LogEventLevel> SerilogLevelMap =
            new Dictionary<LogSeverity, LogEventLevel>
            {
                [LogSeverity.Verbose] = LogEventLevel.Verbose,
                [LogSeverity.Debug] = LogEventLevel.Debug,
                [LogSeverity.Info] = LogEventLevel.Information,
                [LogSeverity.Warning] = LogEventLevel.Warning,
                [LogSeverity.Error] = LogEventLevel.Error,
                [LogSeverity.Critical] = LogEventLevel.Fatal
            };

        private static DiscordShardedClient _client;
        private static CommandService _commands;
        private static ServiceProvider _services;
        private static Database _database;

        private static void Main()
        {
            try
            {
                MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "A fatal error has occurred.");
            }

            Logger.Dispose();
        }

        private static async Task MainAsync()
        {
            Logger.Debug("loading config");

            Config config;

            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load configuration.");
                goto die;
            }

            Logger.Information("Selenium snapshot {Snapshot} starting up now.", config.Snapshot);

            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                TotalShards = 1
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                IgnoreExtraArgs = true,
                DefaultRunMode = RunMode.Async
            });
            _services = new ServiceCollection()
                // add instances
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(Logger)
                .AddSingleton(Database.Instance)
                .AddSingleton(config)
                .BuildServiceProvider();

            _client.Log += msg =>
            {
                var actualLevel = SerilogLevelMap[msg.Severity];

                // ReSharper disable once AccessToDisposedClosure
                Logger.Write(actualLevel, "[{Source}] {Message}", msg.Source, msg.Message);

                return Task.CompletedTask;
            };

            _client.ShardReady += shard =>
            {
                Logger.Information("Shard {ShardId} is up and ready, handling {Guilds} guilds.",
                    shard.ShardId,
                    shard.Guilds.Count);
                return Task.CompletedTask;
            };

            _client.MessageReceived += HandleMessage;

            _database = _services.GetService<Database>();

            await _database.Initialize();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _client.LoginAsync(TokenType.Bot, config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);

            // should only get here in severe cases
            die:
            _client?.Dispose();

            throw new Exception("An unknown fatal error has occurred that triggered the 'die' label.");
        }

        private static async Task HandleMessage(IDeletable messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;
            if (message.Author.IsBot) return;

            var ctx = new ShardedCommandContext(_client, message);
            // don't handle anything in dms
            if (ctx.IsPrivate) return;

            var guildSettings = await _database.GetSettings(ctx.Guild.Id);

            // automod

            // command handling
            var argPos = 0;

            if (!(
                    // in case people forget
                    message.HasMentionPrefix(_client.CurrentUser, ref argPos)
                    // custom prefix
                    || message.HasStringPrefix(guildSettings.Prefix, ref argPos)
                )
            ) return;

            Logger.Debug("Processing command {Command} from {GuildId}", message.Content, ctx.Guild.Id);
            var result = await _commands.ExecuteAsync(ctx, argPos, _services);

            if (!result.IsSuccess && result.ErrorReason != "Unknown command.")
                await ctx.Channel.SendMessageAsync(
                    LocalizationProvider.Instance._("bot.error.errorOccurred", guildSettings.Language,
                        Constants.ErrorEmoji, result.ErrorReason)
                    + "\n"
                    + LocalizationProvider.Instance._("bot.error.docs", guildSettings.Language,
                        Constants.InfoEmoji, "notarealdomain://selenium.bot/docs/"));
        }
    }
}