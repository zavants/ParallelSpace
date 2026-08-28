using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParallelSpace.Models;

namespace ParallelSpace.ViewModels;

/// <summary>Backs the add/edit space dialog.</summary>
public sealed partial class EditSpaceViewModel : ObservableObject
{
    public Space Model { get; }
    public bool IsExisting { get; }
    public bool IsDiscord { get; }
    public string Title => IsExisting ? "Edit space" : "Add a space";

    public EditSpaceViewModel(Space model, bool isExisting)
    {
        Model = model;
        IsExisting = isExisting;
        IsDiscord = !string.IsNullOrEmpty(model.DiscordChannel);

        _name = model.Name;
        _executablePath = model.ExecutablePath;
        _arguments = model.Arguments;
        _workingDirectory = model.WorkingDirectory;
        _isolatedProfile = model.Isolation == IsolationMode.IsolatedProfile;
        _appendUserDataDir = model.AppendUserDataDir;
        _glyph = string.IsNullOrEmpty(model.Glyph) ? "📦" : model.Glyph;
        _accent = string.IsNullOrEmpty(model.Accent) ? "#5865F2" : model.Accent;
    }

    public IReadOnlyList<string> GlyphChoices { get; } = new[]
    {
        "📦", "🎮", "💬", "🌐", "📨", "🎧", "🗂️", "🎨", "🧭", "🪟", "🔷", "🟣", "🟢", "🟠", "🔴", "⚙️",
    };

    public IReadOnlyList<string> AccentChoices { get; } = new[]
    {
        "#5865F2", "#3BA55D", "#FAA61A", "#ED4245", "#EB459E", "#9B59B6", "#1ABC9C", "#3498DB",
    };

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _executablePath;
    [ObservableProperty] private string _arguments;
    [ObservableProperty] private string _workingDirectory;
    [ObservableProperty] private bool _isolatedProfile;
    [ObservableProperty] private bool _appendUserDataDir;
    [ObservableProperty] private string _glyph;
    [ObservableProperty] private string _accent;

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Name) &&
        (IsDiscord || !string.IsNullOrWhiteSpace(ExecutablePath));

    partial void OnNameChanged(string value) => OnPropertyChanged(nameof(CanSave));
    partial void OnExecutablePathChanged(string value) => OnPropertyChanged(nameof(CanSave));

    [RelayCommand] private void SelectGlyph(string g) => Glyph = g;
    [RelayCommand] private void SelectAccent(string a) => Accent = a;

    /// <summary>Copies the edited values back into the underlying model.</summary>
    public void ApplyToModel()
    {
        Model.Name = Name.Trim();
        Model.Glyph = Glyph;
        Model.Accent = Accent;

        if (!IsDiscord)
        {
            Model.ExecutablePath = ExecutablePath.Trim();
            Model.Arguments = Arguments?.Trim() ?? "";
            Model.WorkingDirectory = WorkingDirectory?.Trim() ?? "";
            Model.Isolation = IsolatedProfile ? IsolationMode.IsolatedProfile : IsolationMode.None;
            Model.AppendUserDataDir = AppendUserDataDir;
        }
    }
}
