using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.BulkUserManager.Configuration;
using Jellyfin.Plugin.BulkUserManager.Models;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.BulkUserManager.Controllers;

[ApiController]
[Route("BulkUserManager")]
[Authorize(Policy = "RequiresElevation")]
public class UserSettingsController : ControllerBase
{
    private readonly IUserManager _userManager;

    public UserSettingsController(IUserManager userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("Users")]
    public ActionResult<IEnumerable<object>> GetUsers()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var users = _userManager.GetUsers().Select(u =>
        {
            var dto = _userManager.GetUserDto(u, remoteIp);
            return new
            {
                u.Id,
                u.Username,
                IsAdministrator = dto.Policy?.IsAdministrator ?? false,
                IsDisabled = dto.Policy?.IsDisabled ?? false
            };
        });
        return Ok(users);
    }

    [HttpPost("MassUpdate")]
    public async Task<IActionResult> MassUpdate([FromBody] MassUpdateRequest request)
    {
        if (request.UserIds.Count == 0)
        {
            return BadRequest("No users selected.");
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var updated = 0;

        foreach (var userId in request.UserIds)
        {
            var user = _userManager.GetUserById(userId);
            if (user is null) continue;

            var dto = _userManager.GetUserDto(user, remoteIp);
            var config = dto.Configuration ?? new UserConfiguration();

            ApplyToConfiguration(config, request);

            await _userManager.UpdateConfigurationAsync(userId, config).ConfigureAwait(false);
            updated++;
        }

        return Ok(new { Updated = updated });
    }

    [HttpPost("SaveDefaults")]
    public IActionResult SaveDefaults([FromBody] DefaultSettingsRequest request)
    {
        var plugin = Plugin.Instance;
        if (plugin is null) return StatusCode(500, "Plugin not initialized.");

        var defaults = plugin.Configuration.Defaults;

        defaults.OrderedViews = request.OrderedViews;
        defaults.LatestItemsExcludes = request.LatestItemsExcludes;
        defaults.MyMediaExcludes = request.MyMediaExcludes;
        defaults.GroupedFolders = request.GroupedFolders;
        defaults.DisplayMissingEpisodes = request.DisplayMissingEpisodes ?? false;
        defaults.DisplayCollectionsView = request.DisplayCollectionsView ?? false;
        defaults.HidePlayedInLatest = request.HidePlayedInLatest ?? false;
        defaults.PlayDefaultAudioTrack = request.PlayDefaultAudioTrack ?? true;
        defaults.AudioLanguagePreference = request.AudioLanguagePreference;
        defaults.SubtitleLanguagePreference = request.SubtitleLanguagePreference;
        defaults.SubtitleMode = request.SubtitleMode ?? 0;
        defaults.RememberAudioSelections = request.RememberAudioSelections ?? true;
        defaults.RememberSubtitleSelections = request.RememberSubtitleSelections ?? true;
        defaults.EnableNextEpisodeAutoPlay = request.EnableNextEpisodeAutoPlay ?? true;

        plugin.UpdateConfiguration(plugin.Configuration);
        return Ok(new { Ok = true });
    }

    [HttpPost("ApplyDefaultsToAll")]
    public async Task<IActionResult> ApplyDefaultsToAll()
    {
        var plugin = Plugin.Instance;
        if (plugin is null) return StatusCode(500, "Plugin not initialized.");

        var d = plugin.Configuration.Defaults;
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var updated = 0;

        foreach (var user in _userManager.GetUsers().ToList())
        {
            var dto = _userManager.GetUserDto(user, remoteIp);
            var config = dto.Configuration ?? new UserConfiguration();

            config.OrderedViews = d.OrderedViews?.ToArray() ?? [];
            config.LatestItemsExcludes = d.LatestItemsExcludes?.ToArray() ?? [];
            config.MyMediaExcludes = d.MyMediaExcludes?.ToArray() ?? [];
            config.GroupedFolders = d.GroupedFolders?.ToArray() ?? [];
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
            updated++;
        }

        return Ok(new { Updated = updated });
    }

    [HttpGet("ui")]
    [AllowAnonymous]
    public IActionResult GetUi()
    {
        var assembly = typeof(Plugin).Assembly;
        const string resourceName = "Jellyfin.Plugin.BulkUserManager.Configuration.ui.html";
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return NotFound();
        return File(stream, "text/html");
    }

    [HttpPost("MassPolicyUpdate")]
    public async Task<IActionResult> MassPolicyUpdate([FromBody] PolicyUpdateRequest request)
    {
        if (request.UserIds.Count == 0)
        {
            return BadRequest("No users selected.");
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var updated = 0;

        foreach (var userId in request.UserIds)
        {
            var user = _userManager.GetUserById(userId);
            if (user is null) continue;

            var dto = _userManager.GetUserDto(user, remoteIp);
            var policy = dto.Policy;
            if (policy is null) continue;

            ApplyPolicy(policy, request);

            await _userManager.UpdatePolicyAsync(userId, policy).ConfigureAwait(false);
            updated++;
        }

        return Ok(new { Updated = updated });
    }

    [HttpPost("SavePolicyDefaults")]
    public IActionResult SavePolicyDefaults([FromBody] PolicyDefaultsRequest request)
    {
        var plugin = Plugin.Instance;
        if (plugin is null) return StatusCode(500, "Plugin not initialized.");

        var d = plugin.Configuration.PolicyDefaults;

        d.IsAdministrator = request.IsAdministrator;
        d.IsHidden = request.IsHidden;
        d.IsDisabled = request.IsDisabled;
        d.EnableAllFolders = request.EnableAllFolders;
        d.EnabledFolders = request.EnabledFolders ?? new List<Guid>();
        d.EnableContentDownloading = request.EnableContentDownloading;
        d.EnableMediaPlayback = request.EnableMediaPlayback;
        d.EnableAudioPlaybackTranscoding = request.EnableAudioPlaybackTranscoding;
        d.EnableVideoPlaybackTranscoding = request.EnableVideoPlaybackTranscoding;
        d.EnablePlaybackRemuxing = request.EnablePlaybackRemuxing;
        d.EnableLiveTvAccess = request.EnableLiveTvAccess;
        d.EnableLiveTvManagement = request.EnableLiveTvManagement;
        d.EnableRemoteAccess = request.EnableRemoteAccess;
        d.EnableSharedDeviceControl = request.EnableSharedDeviceControl;
        d.EnableSyncTranscoding = request.EnableSyncTranscoding;
        d.EnableSubtitleManagement = request.EnableSubtitleManagement;
        d.EnableLyricManagement = request.EnableLyricManagement;
        d.EnableCollectionManagement = request.EnableCollectionManagement;
        d.EnableUserPreferenceAccess = request.EnableUserPreferenceAccess;

        plugin.UpdateConfiguration(plugin.Configuration);
        return Ok(new { Ok = true });
    }

    [HttpPost("ApplyPolicyDefaultsToAll")]
    public async Task<IActionResult> ApplyPolicyDefaultsToAll()
    {
        var plugin = Plugin.Instance;
        if (plugin is null) return StatusCode(500, "Plugin not initialized.");

        var d = plugin.Configuration.PolicyDefaults;
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var updated = 0;

        foreach (var user in _userManager.GetUsers().ToList())
        {
            var dto = _userManager.GetUserDto(user, remoteIp);
            var policy = dto.Policy;
            if (policy is null) continue;

            policy.IsAdministrator = d.IsAdministrator;
            policy.IsHidden = d.IsHidden;
            policy.IsDisabled = d.IsDisabled;
            policy.EnableAllFolders = d.EnableAllFolders;
            policy.EnabledFolders = d.EnableAllFolders
            ? Array.Empty<Guid>()
            : d.EnabledFolders.ToArray();
            policy.EnableContentDownloading = d.EnableContentDownloading;
            policy.EnableMediaPlayback = d.EnableMediaPlayback;
            policy.EnableAudioPlaybackTranscoding = d.EnableAudioPlaybackTranscoding;
            policy.EnableVideoPlaybackTranscoding = d.EnableVideoPlaybackTranscoding;
            policy.EnablePlaybackRemuxing = d.EnablePlaybackRemuxing;
            policy.EnableLiveTvAccess = d.EnableLiveTvAccess;
            policy.EnableLiveTvManagement = d.EnableLiveTvManagement;
            policy.EnableRemoteAccess = d.EnableRemoteAccess;
            policy.EnableSharedDeviceControl = d.EnableSharedDeviceControl;
            policy.EnableSyncTranscoding = d.EnableSyncTranscoding;
            policy.EnableSubtitleManagement = d.EnableSubtitleManagement;
            policy.EnableLyricManagement = d.EnableLyricManagement;
            policy.EnableCollectionManagement = d.EnableCollectionManagement;
            policy.EnableUserPreferenceAccess = d.EnableUserPreferenceAccess;

            await _userManager.UpdatePolicyAsync(user.Id, policy).ConfigureAwait(false);
            updated++;
        }

        return Ok(new { Updated = updated });
    }

    [HttpGet("Backup")]
    public IActionResult Backup()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var backup = new UserBackup
        {
            ExportedAt = DateTime.UtcNow,
            PluginVersion = typeof(Plugin).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            JellyfinVersion = "12.x",
            Users = _userManager.GetUsers().Select(u =>
            {
                var dto = _userManager.GetUserDto(u, remoteIp);
                return new UserBackupEntry
                {
                    Id = u.Id,
                    Username = u.Username,
                    Configuration = dto.Configuration,
                    Policy = dto.Policy
                };
            }).ToList()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var filename = $"bulk-user-manager-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(bytes, "application/json", filename);
    }

    [HttpPost("RestorePreview")]
    public IActionResult RestorePreview([FromBody] UserBackup backup)
    {
        if (backup?.Users is null || backup.Users.Count == 0)
        {
            return BadRequest("Backup file contains no users.");
        }

        var toRestore = new List<object>();
        var notFound = new List<object>();

        foreach (var entry in backup.Users)
        {
            var user = _userManager.GetUserById(entry.Id);
            if (user is null)
            {
                notFound.Add(new { entry.Id, entry.Username });
            }
            else
            {
                toRestore.Add(new { entry.Id, entry.Username });
            }
        }

        return Ok(new
        {
            ExportedAt = backup.ExportedAt,
            Total = backup.Users.Count,
            ToRestore = toRestore,
            NotFound = notFound
        });
    }

    [HttpPost("Restore")]
    public async Task<IActionResult> Restore([FromBody] UserBackup backup)
    {
        if (backup?.Users is null || backup.Users.Count == 0)
        {
            return BadRequest("Backup file contains no users.");
        }

        var restored = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var entry in backup.Users)
        {
            var user = _userManager.GetUserById(entry.Id);
            if (user is null)
            {
                skipped++;
                continue;
            }

            try
            {
                if (entry.Configuration is not null)
                {
                    await _userManager.UpdateConfigurationAsync(entry.Id, entry.Configuration).ConfigureAwait(false);
                }
                if (entry.Policy is not null)
                {
                    await _userManager.UpdatePolicyAsync(entry.Id, entry.Policy).ConfigureAwait(false);
                }
                restored++;
            }
            catch (Exception ex)
            {
                errors.Add(entry.Username + ": " + ex.Message);
            }
        }

        return Ok(new { Restored = restored, Skipped = skipped, Errors = errors });
    }

    private static void ApplyPolicy(UserPolicy policy, PolicyUpdateRequest req)
    {
        if (req.IsAdministrator.HasValue) policy.IsAdministrator = req.IsAdministrator.Value;
        if (req.IsHidden.HasValue) policy.IsHidden = req.IsHidden.Value;
        if (req.IsDisabled.HasValue) policy.IsDisabled = req.IsDisabled.Value;
        if (req.EnableAllFolders.HasValue) policy.EnableAllFolders = req.EnableAllFolders.Value;
        if (req.EnabledFolders is not null)
        {
            policy.EnabledFolders = req.EnabledFolders.ToArray();
        }
        if (req.EnableContentDownloading.HasValue) policy.EnableContentDownloading = req.EnableContentDownloading.Value;
        if (req.EnableMediaPlayback.HasValue) policy.EnableMediaPlayback = req.EnableMediaPlayback.Value;
        if (req.EnableAudioPlaybackTranscoding.HasValue) policy.EnableAudioPlaybackTranscoding = req.EnableAudioPlaybackTranscoding.Value;
        if (req.EnableVideoPlaybackTranscoding.HasValue) policy.EnableVideoPlaybackTranscoding = req.EnableVideoPlaybackTranscoding.Value;
        if (req.EnablePlaybackRemuxing.HasValue) policy.EnablePlaybackRemuxing = req.EnablePlaybackRemuxing.Value;
        if (req.EnableLiveTvAccess.HasValue) policy.EnableLiveTvAccess = req.EnableLiveTvAccess.Value;
        if (req.EnableLiveTvManagement.HasValue) policy.EnableLiveTvManagement = req.EnableLiveTvManagement.Value;
        if (req.EnableRemoteAccess.HasValue) policy.EnableRemoteAccess = req.EnableRemoteAccess.Value;
        if (req.EnableSharedDeviceControl.HasValue) policy.EnableSharedDeviceControl = req.EnableSharedDeviceControl.Value;
        if (req.EnableSyncTranscoding.HasValue) policy.EnableSyncTranscoding = req.EnableSyncTranscoding.Value;
        if (req.EnableSubtitleManagement.HasValue) policy.EnableSubtitleManagement = req.EnableSubtitleManagement.Value;
        if (req.EnableLyricManagement.HasValue) policy.EnableLyricManagement = req.EnableLyricManagement.Value;
        if (req.EnableCollectionManagement.HasValue) policy.EnableCollectionManagement = req.EnableCollectionManagement.Value;
        if (req.EnableUserPreferenceAccess.HasValue) policy.EnableUserPreferenceAccess = req.EnableUserPreferenceAccess.Value;
    }

    private static void ApplyToConfiguration(UserConfiguration config, MassUpdateRequest request)
    {
        if (request.OrderedViews is not null)
            config.OrderedViews = request.OrderedViews.ToArray();
        if (request.LatestItemsExcludes is not null)
            config.LatestItemsExcludes = request.LatestItemsExcludes.ToArray();
        if (request.MyMediaExcludes is not null)
            config.MyMediaExcludes = request.MyMediaExcludes.ToArray();
        if (request.GroupedFolders is not null)
            config.GroupedFolders = request.GroupedFolders.ToArray();

        if (request.DisplayMissingEpisodes.HasValue)
            config.DisplayMissingEpisodes = request.DisplayMissingEpisodes.Value;
        if (request.DisplayCollectionsView.HasValue)
            config.DisplayCollectionsView = request.DisplayCollectionsView.Value;
        if (request.HidePlayedInLatest.HasValue)
            config.HidePlayedInLatest = request.HidePlayedInLatest.Value;

        if (request.PlayDefaultAudioTrack.HasValue)
            config.PlayDefaultAudioTrack = request.PlayDefaultAudioTrack.Value;
        if (!string.IsNullOrEmpty(request.AudioLanguagePreference))
            config.AudioLanguagePreference = request.AudioLanguagePreference;
        if (request.RememberAudioSelections.HasValue)
            config.RememberAudioSelections = request.RememberAudioSelections.Value;
        if (request.EnableNextEpisodeAutoPlay.HasValue)
            config.EnableNextEpisodeAutoPlay = request.EnableNextEpisodeAutoPlay.Value;

        if (request.SubtitleMode.HasValue)
            config.SubtitleMode = (SubtitlePlaybackMode)request.SubtitleMode.Value;
        if (!string.IsNullOrEmpty(request.SubtitleLanguagePreference))
            config.SubtitleLanguagePreference = request.SubtitleLanguagePreference;
        if (request.RememberSubtitleSelections.HasValue)
            config.RememberSubtitleSelections = request.RememberSubtitleSelections.Value;
    }
}
