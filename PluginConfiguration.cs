using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.BulkUserManager.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public UserDefaultSettings Defaults { get; set; } = new();
    public PolicyDefaults PolicyDefaults { get; set; } = new();
    public List<Guid> InitializedUserIds { get; set; } = new();
}

public class UserDefaultSettings
{
    public List<Guid>? OrderedViews { get; set; }
    public List<Guid>? LatestItemsExcludes { get; set; }
    public List<Guid>? MyMediaExcludes { get; set; }
    public List<Guid>? GroupedFolders { get; set; }
    public bool DisplayMissingEpisodes { get; set; }
    public bool DisplayCollectionsView { get; set; }
    public bool HidePlayedInLatest { get; set; }
    public bool PlayDefaultAudioTrack { get; set; } = true;
    public string? AudioLanguagePreference { get; set; }
    public string? SubtitleLanguagePreference { get; set; }
    public int SubtitleMode { get; set; }
    public bool RememberAudioSelections { get; set; } = true;
    public bool RememberSubtitleSelections { get; set; } = true;
    public bool EnableNextEpisodeAutoPlay { get; set; } = true;
}

public class PolicyDefaults
{
    public bool IsAdministrator { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDisabled { get; set; }
    public bool EnableAllFolders { get; set; } = true;
    public List<Guid> EnabledFolders { get; set; } = new();
    public bool EnableContentDownloading { get; set; } = true;
    public bool EnableMediaPlayback { get; set; } = true;
    public bool EnableAudioPlaybackTranscoding { get; set; } = true;
    public bool EnableVideoPlaybackTranscoding { get; set; } = true;
    public bool EnablePlaybackRemuxing { get; set; } = true;
    public bool EnableLiveTvAccess { get; set; } = true;
    public bool EnableLiveTvManagement { get; set; }
    public bool EnableRemoteAccess { get; set; } = true;
    public bool EnableSharedDeviceControl { get; set; } = true;
    public bool EnableSyncTranscoding { get; set; } = true;
    public bool EnableSubtitleManagement { get; set; }
    public bool EnableLyricManagement { get; set; }
    public bool EnableCollectionManagement { get; set; }
    public bool EnableUserPreferenceAccess { get; set; } = true;
}
