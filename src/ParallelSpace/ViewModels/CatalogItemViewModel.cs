using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ParallelSpace.ViewModels;

/// <summary>
/// One row in the app catalog. Covers both a plain preset (Chrome, Telegram, …)
/// and a Discord release channel, so the page can list them side by side.
/// </summary>
public sealed partial class CatalogItemViewModel : ObservableObject
{
    public required string Key { get; init; }
    public required string Display { get; init; }
    public required string Category { get; init; }
    public required string Glyph { get; init; }
    public required string Accent { get; init; }
    public required string TechniqueLabel { get; init; }
    public required string Note { get; init; }
    public string DownloadUrl { get; init; } = "";

    /// <summary>Set for the three Discord channels; empty for ordinary presets.</summary>
    public string DiscordChannel { get; init; } = "";

    /// <summary>
    /// One space per account, so ordinary presets can be added repeatedly — each
    /// new space gets its own private folder. A Discord channel is a single data
    /// folder, i.e. exactly one account, so adding it twice would gain nothing.
    /// </summary>
    public bool AllowsMultiple => string.IsNullOrEmpty(DiscordChannel);

    public Action<CatalogItemViewModel>? OnActivate { get; init; }

    [ObservableProperty] private bool _isInstalled;
    [ObservableProperty] private bool _isAdded;

    partial void OnIsInstalledChanged(bool value) => RaiseLabels();
    partial void OnIsAddedChanged(bool value) => RaiseLabels();

    private void RaiseLabels()
    {
        OnPropertyChanged(nameof(StatusLine));
        OnPropertyChanged(nameof(ActionLabel));
        OnPropertyChanged(nameof(CanActivate));
    }

    public string StatusLine =>
        !IsInstalled ? "Not installed" :
        IsAdded ? "Added ✓" : "Installed";

    public string ActionLabel =>
        !IsInstalled ? "Get it ↗" :
        !IsAdded ? "Add space" :
        AllowsMultiple ? "Add another" : "Added";

    public bool CanActivate => !IsAdded || AllowsMultiple;

    [RelayCommand]
    private void Activate() => OnActivate?.Invoke(this);
}
