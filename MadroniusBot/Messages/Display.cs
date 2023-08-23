using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using MadroniusBot.Core;

namespace MadroniusBot.Messages
{
    /// <summary>
    /// Display message to Discord.
    /// </summary>
    public static class Display
    {
        const int kMinNumberOfElementsPerColumn = 4;
        const int kMaxNumberOfColumns = 3;

        public const string kValidCommandEmoji = ":white_check_mark:";
        public const string kInvalidCommandEmoji = ":no_entry_sign:";

        public static readonly string[] kRankingEmoijs = new []
        {
            ":first_place:",
            ":second_place:",
            ":third_place:"
        };

        /// <summary>
        /// Displays the leaderboard for specified weekly.
        /// </summary>
        /// <param name="weekly">Weekly settings.</param>
        /// <param name="preventSpoilers">Hide potential spoilers.</param>
        /// <returns>Returns an embed builder.</returns>
        public static DiscordEmbedBuilder LeaderboardEmbed(IReadOnlyWeekly weekly, bool preventSpoilers)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Leaderboard",
                Color = DiscordColor.Yellow
            };

            embed
                .AddField("Weekly Seed", $"Week #{weekly.WeekNumber}");

            IEnumerable<KeyValuePair<string, TimeSpan>> leaderboard = weekly.Leaderboard;

            // To avoid giving away any ranking, avoid sorting the leaderboard when preventing spoilers.
            if (!preventSpoilers)
            {
                leaderboard = leaderboard
                    .OrderBy(kvp => kvp.Value);
            }

            var rankStrings = String.Empty;
            var userStrings = String.Empty;
            var timeStrings = String.Empty;

            var rank = 0;
            var rankTreshold = TimeSpan.MinValue;
            foreach (var entry in leaderboard)
            {
                if (entry.Value > rankTreshold)
                {
                    ++rank;
                    rankTreshold = entry.Value;
                }
                rankStrings += $"{(rank <= 3 ? kRankingEmoijs[rank - 1] : CommandUtils.IntegerToOrdinal(rank))}\n";

                userStrings += $"{entry.Key}\n";
                timeStrings += $"{(entry.Value.Equals(TimeSpan.MaxValue) ? "DNF" : entry.Value.ToString())}\n";
            }

            if (preventSpoilers)
            {
                timeStrings = Formatter.Spoiler(timeStrings);
            }

            if (!preventSpoilers)
                embed.AddField("\u200B", rankStrings, true);

            embed.AddField("User", userStrings, true);
            embed.AddField("Time", timeStrings, true);

            return embed;
        }
    }
}