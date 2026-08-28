using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using ParallelSpace.Models;

namespace ParallelSpace.Services;

/// <summary>
/// Applies a theme by replacing the brushes in Application.Resources.
/// The views bind these with DynamicResource, so a swap repaints live.
///
/// A theme only defines ten base colours; the secondary surfaces below are mixed
/// from them, which is what lets a light theme work without a second palette.
/// </summary>
public static class ThemeManager
{
    public static AppTheme Current { get; private set; } = AppThemes.Default;

    public static AppTheme Resolve(string? key) =>
        AppThemes.All.FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase))
        ?? AppThemes.Default;

    public static void Apply(Application app, AppTheme theme)
    {
        Current = theme;

        var bg = Color.Parse(theme.AppBackground);
        var panel = Color.Parse(theme.Panel);
        var card = Color.Parse(theme.Card);
        var cardHover = Color.Parse(theme.CardHover);
        var text = Color.Parse(theme.Text);
        var muted = Color.Parse(theme.MutedText);

        var r = app.Resources;
        r["AppBackgroundBrush"] = new SolidColorBrush(bg);
        r["PanelBrush"] = new SolidColorBrush(panel);
        r["CardBrush"] = new SolidColorBrush(card);
        r["CardHoverBrush"] = new SolidColorBrush(cardHover);
        r["AccentBrush"] = new SolidColorBrush(Color.Parse(theme.Accent));
        r["AccentHoverBrush"] = new SolidColorBrush(Color.Parse(theme.AccentHover));
        r["TextBrush"] = new SolidColorBrush(text);
        r["MutedTextBrush"] = new SolidColorBrush(muted);
        r["OkBrush"] = new SolidColorBrush(Color.Parse(theme.Ok));
        r["WarnBrush"] = new SolidColorBrush(Color.Parse(theme.Warn));
        r["AccentColor"] = Color.Parse(theme.Accent);

        // Derived surfaces — mixing toward the text colour keeps these readable
        // whether the theme is light or dark.
        r["SubtleBrush"] = new SolidColorBrush(Mix(cardHover, text, 0.00));
        r["SubtleHoverBrush"] = new SolidColorBrush(Mix(cardHover, text, 0.14));
        r["NavHoverBrush"] = new SolidColorBrush(Mix(panel, text, 0.07));
        r["TileBrush"] = new SolidColorBrush(Mix(card, text, 0.10));
        r["FaintTextBrush"] = new SolidColorBrush(Mix(muted, panel, 0.40));
        r["BorderBrush"] = new SolidColorBrush(Mix(card, text, 0.20));

        // Let Fluent's own controls (text boxes, checkboxes, scrollbars) match.
        app.RequestedThemeVariant = theme.IsLight ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    /// <summary>Linear blend of two colours; amount 0 = a, 1 = b.</summary>
    private static Color Mix(Color a, Color b, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);
        return Color.FromRgb(
            (byte)Math.Round(a.R + (b.R - a.R) * amount),
            (byte)Math.Round(a.G + (b.G - a.G) * amount),
            (byte)Math.Round(a.B + (b.B - a.B) * amount));
    }
}
