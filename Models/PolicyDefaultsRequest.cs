using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.BulkUserManager.Models;

public class PolicyDefaultsRequest
{
    public bool IsAdministrator { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDisabled { get; set; }
    public bool EnableAllFolders { get; set; } = true;
    public List<Guid>? EnabledFolders { get; set; }
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
