using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.BulkUserManager.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BulkUserManager.Tasks;

public class ApplyDefaultsTask : IScheduledTask
{
    private readonly IUserManager _userManager;
    private readonly ILogger<ApplyDefaultsTask> _logger;

    public ApplyDefaultsTask(IUserManager userManager, ILogger<ApplyDefaultsTask> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public string Name => "Apply defaults to new users";
    public string Key => "BulkUserManagerApplyDefaults";
    public string Description => "Applies the configured default user settings to any user who has not yet customized their layout.";
    public string Category => "Bulk User Manager";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.StartupTrigger
            },
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = TimeSpan.FromMinutes(5).Ticks
            }
        };
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            progress.Report(100);
            return;
        }

        var d = plugin.Configuration.Defaults;
        var alreadyInitialized = new HashSet<Guid>(plugin.Configuration.InitializedUserIds ?? new List<Guid>());
        var remoteIp = "127.0.0.1";
        var applied = 0;
        var users = _userManager.GetUsers().ToList();

        for (var i = 0; i < users.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report((double)i / users.Count * 100);

            var user = users[i];

            // Already processed on a previous run — skip forever.
            if (alreadyInitialized.Contains(user.Id))
            {
                continue;
            }

            var dto = _userManager.GetUserDto(user, remoteIp);
            var config = dto.Configuration ?? new UserConfiguration();

            // If the user has customized their layout themselves, don't touch them
            // and mark as initialized so we never revisit.
            if (config.OrderedViews is { Length: > 0 })
            {
                alreadyInitialized.Add(user.Id);
                continue;
            }

            if (d.OrderedViews is { Count: > 0 })
                config.OrderedViews = d.OrderedViews.ToArray();
            if (d.LatestItemsExcludes is not null)
                config.LatestItemsExcludes = d.LatestItemsExcludes.ToArray();
            if (d.MyMediaExcludes is not null)
                config.MyMediaExcludes = d.MyMediaExcludes.ToArray();
            if (d.GroupedFolders is not null)
                config.GroupedFolders = d.GroupedFolders.ToArray();

            config.DisplayMissingEpisodes = d.DisplayMissingEpisodes;
            config.DisplayCollectionsView = d.DisplayCollectionsView;
            config.HidePlayedInLatest = d.HidePlayedInLatest;
            config.PlayDefaultAudioTrack = d.PlayDefaultAudioTrack;
            config.AudioLanguagePreference = d.AudioLanguagePreference;
            config.SubtitleLanguagePreference = d.SubtitleLanguagePreference;
            config.SubtitleMode = (SubtitlePlaybackMode)d.SubtitleMode;
            config.RememberAudioSelections = d.RememberAudioSelections;
            config.RememberSubtitleSelections = d.RememberSubtitleSelections;
            config.EnableNextEpisodeAutoPlay = d.EnableNextEpisodeAutoPlay;

            await _userManager.UpdateConfigurationAsync(user.Id, config).ConfigureAwait(false);
            alreadyInitialized.Add(user.Id);
            applied++;
        }

        plugin.Configuration.InitializedUserIds = alreadyInitialized.ToList();
        plugin.UpdateConfiguration(plugin.Configuration);

        if (applied > 0)
        {
            _logger.LogInformation("Bulk User Manager: applied defaults to {Count} user(s)", applied);
        }

        progress.Report(100);
    }
}
