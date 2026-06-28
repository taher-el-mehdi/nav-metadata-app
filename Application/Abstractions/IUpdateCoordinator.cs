namespace NAVMetadata.Abstractions;

/// <summary>Orchestrates update checks, settings, and user prompts.</summary>
public interface IUpdateCoordinator
{
    Task CheckOnStartupAsync(IWin32Window owner, CancellationToken cancellationToken = default);

    Task CheckManuallyAsync(IWin32Window owner, CancellationToken cancellationToken = default);
}
