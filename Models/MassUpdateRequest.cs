using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.BulkUserManager.Models;

public class MassUpdateRequest
{
    public List<Guid> UserIds { get; set; } = [];

    public List<Guid>? OrderedViews { get; set; }
    public List<Guid>? LatestItemsExcludes { get; set; }
    public List<Guid>? MyMediaExcludes { get; set; }
    public List<Guid>? GroupedFolders { get; set; }

    public bool? DisplayMissingEpisodes { get; set; }
    public bool? DisplayCollectionsView { get; set; }
    public bool? HidePlayedInLatest { get; set; }
    public bool? PlayDefaultAudioTrack { get; set; }
    public string? AudioLanguagePreference { get; set; }
    public string? SubtitleLanguagePreference { get; set; }
    public int? SubtitleMode { get; set; }
    public bool? RememberAudioSelections { get; set; }
    public bool? RememberSubtitleSelections { get; set; }
    public bool? EnableNextEpisodeAutoPlay { get; set; }
    public List<string>? HomeSections { get; set; }
}
