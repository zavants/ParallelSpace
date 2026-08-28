using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParallelSpace.Models;

namespace ParallelSpace.ViewModels;

/// <summary>A theme card on the Appearance page, showing a live palette preview.</summary>
public sealed partial class ThemeOptionViewModel : ObservableObject
{
    public AppTheme Theme { get; }
    public Action<ThemeOptionViewModel>? OnActivate { get; init; }

    public ThemeOptionViewModel(AppTheme theme) => Theme = theme;

    public string Key => Theme.Key;
    public string Name => Theme.Name;
    public string Blurb => Theme.Blurb;
    public string Kind => Theme.IsLight ? "Light" : "Dark";

    // Swatches for the preview strip.
    public string AppBackground => Theme.AppBackground;
    public string Panel => Theme.Panel;
    public string Card => Theme.Card;
    public string Accent => Theme.Accent;
    public string Text => Theme.Text;
    public string Ok => Theme.Ok;

    [ObservableProperty] private bool _isSelected;

    [RelayCommand]
    private void Activate() => OnActivate?.Invoke(this);
}
