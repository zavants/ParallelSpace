using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ParallelSpace.ViewModels;

namespace ParallelSpace.Views;

public partial class EditSpaceWindow : Window
{
    public EditSpaceWindow()
    {
        InitializeComponent();
    }

    private EditSpaceViewModel? Vm => DataContext as EditSpaceViewModel;

    private async void OnBrowseExe(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Pick the program",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Programs") { Patterns = new[] { "*.exe" } },
                new("All files") { Patterns = new[] { "*" } },
            },
        });

        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (Vm is not null && !string.IsNullOrEmpty(path))
        {
            Vm.ExecutablePath = path;
            if (string.IsNullOrWhiteSpace(Vm.Name))
                Vm.Name = Path.GetFileNameWithoutExtension(path);
        }
    }

    private async void OnBrowseWorkDir(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Pick a working folder",
            AllowMultiple = false,
        });

        var path = folders.FirstOrDefault()?.TryGetLocalPath();
        if (Vm is not null && !string.IsNullOrEmpty(path))
            Vm.WorkingDirectory = path;
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        if (Vm is { CanSave: true })
        {
            Vm.ApplyToModel();
            Close(true);
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
