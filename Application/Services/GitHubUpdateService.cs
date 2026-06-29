using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NAVMetadata.Abstractions;
using NAVMetadata.Constants;
using NAVMetadata.Helpers;
using NAVMetadata.Models;

namespace NAVMetadata.Services;

/// <summary>
/// Fetches the latest release from the GitHub Releases API and compares versions with SemVer.
/// </summary>
public sealed class GitHubUpdateService : IUpdateService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerService _logger;

    public GitHubUpdateService(ILoggerService logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{AppConstants.AppName}/{CurrentVersion}");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public string CurrentVersion => AppVersion.Current;

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfo($"Checking for updates (current version {CurrentVersion})");

            using var response = await _httpClient.GetAsync(AppConstants.GitHubLatestReleaseApiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = $"GitHub API returned {(int)response.StatusCode} {response.ReasonPhrase}";
                _logger.LogWarning(message);
                return UpdateCheckResult.Failed(CurrentVersion, message);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<GitHubReleaseResponse>(
                stream,
                cancellationToken: cancellationToken);

            if (payload is null || string.IsNullOrWhiteSpace(payload.TagName))
            {
                const string message = "GitHub release response did not contain a version tag.";
                _logger.LogWarning(message);
                return UpdateCheckResult.Failed(CurrentVersion, message);
            }

            var latestVersion = SemanticVersionComparer.Normalize(payload.TagName);
            var release = MapRelease(payload, latestVersion);

            if (SemanticVersionComparer.IsNewer(latestVersion, CurrentVersion))
            {
                _logger.LogInfo($"Update available: {CurrentVersion} -> {latestVersion}");
                return UpdateCheckResult.UpdateAvailable(CurrentVersion, release);
            }

            _logger.LogInfo($"Application is up to date ({CurrentVersion})");
            return UpdateCheckResult.UpToDate(CurrentVersion, release);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            const string message = "The update check timed out. Check your internet connection and try again.";
            _logger.LogWarning(message);
            _logger.LogError("Update check timed out", ex);
            return UpdateCheckResult.Failed(CurrentVersion, message);
        }
        catch (Exception ex)
        {
            const string message = "Unable to check for updates right now.";
            _logger.LogError("Update check failed", ex);
            return UpdateCheckResult.Failed(CurrentVersion, message);
        }
    }

    public Task OpenReleasePageAsync(ReleaseInfo release, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);

        try
        {
            _logger.LogInfo($"Opening release page for version {release.Version}");
            Process.Start(new ProcessStartInfo(release.ReleasePageUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to open release page: {release.ReleasePageUrl}", ex);
            throw;
        }

        return Task.CompletedTask;
    }

    public void Dispose() => _httpClient.Dispose();

    private static ReleaseInfo MapRelease(GitHubReleaseResponse payload, string version) => new()
    {
        Version = version,
        ReleaseNotes = string.IsNullOrWhiteSpace(payload.Body)
            ? "No release notes were provided for this version."
            : payload.Body.Trim(),
        ReleasePageUrl = string.IsNullOrWhiteSpace(payload.HtmlUrl)
            ? AppConstants.GitHubReleasesPageUrl
            : payload.HtmlUrl,
        DownloadUrl = payload.Assets?
            .Where(a => !string.IsNullOrWhiteSpace(a.BrowserDownloadUrl) && !string.IsNullOrWhiteSpace(a.Name))
            .OrderByDescending(a => a.Name!.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(a => a.Name!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(a => a.Name!.Contains("win-x64", StringComparison.OrdinalIgnoreCase))
            .Select(a => a.BrowserDownloadUrl)
            .FirstOrDefault()
    };

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset>? Assets { get; set; }
    }

    private sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
