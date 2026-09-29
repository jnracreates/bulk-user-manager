using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Users;

namespace Jellyfin.Plugin.BulkUserManager.Models;

public class UserBackup
{
    public DateTime ExportedAt { get; set; }
    public string PluginVersion { get; set; } = "";
    public string JellyfinVersion { get; set; } = "";
    public List<UserBackupEntry> Users { get; set; } = new();
}

public class UserBackupEntry
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public UserConfiguration? Configuration { get; set; }
    public UserPolicy? Policy { get; set; }
}
