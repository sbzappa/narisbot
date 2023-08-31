using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using NarisBot.Core;
using NarisBot.IO;
using NarisBot.Messages;

namespace NarisBot.Tasks
{
    public class ResetWeekly
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public DiscordShardedClient Discord { get; set; }
        public Weekly Weekly { get; set; }
        public Config Config { get; set; }
        public TimeSpan Interval { get; set; }

        public async Task StartAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await TryResetWeekly();
                    await Task.Delay(WeeklyUtils.GetRemainingWeeklyDuration(Weekly.WeekNumber), _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task TryResetWeekly()
        {
            var guilds = Discord.ShardClients
                .Select(kvp => kvp.Value)
                .SelectMany(shard => shard.Guilds)
                .Select(kvp => kvp.Value);
            
            var previousWeek = Weekly.WeekNumber;
            var currentWeek = WeeklyUtils.GetWeekNumber();
            var backupAndResetWeekly = previousWeek != currentWeek;

            // Make a backup of the previous week's weekly and create a new
            // weekly for the current week.
            if (!backupAndResetWeekly)
                return;
               
            foreach (var guild in guilds)
            {
                await CommandUtils.RevokeAllRolesAsync(guild, new[]
                {
                    Config.WeeklyCompletedRole,
                    Config.WeeklyForfeitedRole
                });
                
                await CommandUtils.SendToChannelAsync(
                    guild,
                    Config.WeeklyChannel,
                    Display.LeaderboardEmbed(Weekly, false));
            } 
            
            // Backup weekly settings to json before overriding.
            await WeeklyIO.StoreWeeklyAsync(Weekly, $"weekly.{previousWeek}.json");

            // Set weekly to blank with a fresh leaderboard.
            Weekly.Load(Weekly.Blank);
            await WeeklyIO.StoreWeeklyAsync(Weekly);
        }

        public void StopAsync()
        {
            _cts.Cancel();
        }
        
    }
}