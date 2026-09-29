using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.BulkUserManager.Models;

public class PolicyUpdateRequest
{
    public List<Guid> UserIds { get; set; } = [];
    public bool? IsAdministrator { get; set; }
    public bool? IsHidden { get; set; }
    public bool? IsDisabled { get; set; }
    public bool? EnableAllFolders { get; set; }
    public List<Guid>? EnabledFolders { get; set; }
    public bool? EnableContentDownloading { get; set; }
    public bool? EnableMediaPlayback { get; set; }
    public bool? EnableAudioPlaybackTranscoding { get; set; }
    public bool? EnableVideoPlaybackTranscoding { get; set; }
    public bool? EnablePlaybackRemuxing { get; set; }
    public bool? EnableLiveTvAccess { get; set; }
    public bool? EnableLiveTvManagement { get; set; }
    public bool? EnableRemoteAccess { get; set; }
    public bool? EnableSharedDeviceControl { get; set; }
    public bool? EnableSyncTranscoding { get; set; }
    public bool? EnableSubtitleManagement { get; set; }
    public bool? EnableLyricManagement { get; set; }
    public bool? EnableCollectionManagement { get; set; }
    public bool? EnableUserPreferenceAccess { get; set; }
}
