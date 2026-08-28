using System.Collections.Generic;

namespace ParallelSpace.Models;

/// <summary>
/// A colour theme. Ten base colours; the rest of the palette is mixed from these
/// by ThemeManager so a light theme needs no special-casing.
/// </summary>
public sealed record AppTheme(
    string Key,
    string Name,
    string Blurb,
    bool IsLight,
    string AppBackground,
    string Panel,
    string Card,
    string CardHover,
    string Accent,
    string AccentHover,
    string Text,
    string MutedText,
    string Ok,
    string Warn);

/// <summary>
/// The shipped theme pack. Designed in Magic Patterns and checked against WCAG:
/// Text is at least 7:1 on every surface, MutedText at least 4.5:1, and white on
/// Accent at least 4.5:1.
/// </summary>
public static class AppThemes
{
    public static IReadOnlyList<AppTheme> All { get; } =
    [
        new("midnight", "Midnight", "Deep neutral dark with a calm indigo accent", false,
            "#131417", "#1B1D22", "#24262D", "#2E313A",
            "#4F5BD5", "#414CBE", "#F4F5F7", "#A3A9B8", "#3BA55D", "#E0A32E"),

        new("nord", "Nord", "Cool desaturated blue-grey with icy highlights", false,
            "#262B33", "#2E3440", "#3B4252", "#454E60",
            "#4C6E96", "#3E5C80", "#ECEFF4", "#B8C2D2", "#5A9E6F", "#D08F42"),

        new("carbon", "Carbon", "Near-black, high contrast, crisp separations", false,
            "#000000", "#0B0B0C", "#16171A", "#24262A",
            "#D62828", "#B81F1F", "#FFFFFF", "#9CA0A8", "#33A867", "#E0A32E"),

        new("ember", "Ember", "Warm charcoal surfaces lit by amber", false,
            "#17120F", "#1F1916", "#2A211C", "#382C25",
            "#B45309", "#93420A", "#F7F1EB", "#B3A398", "#4F9D5F", "#D9932B"),

        new("daylight", "Daylight", "Clean white workspace with a confident blue", true,
            "#EDF0F4", "#F8FAFC", "#FFFFFF", "#E7EBF1",
            "#2563EB", "#1D4FC4", "#1A1D24", "#5A6472", "#17803D", "#A15C07"),

        new("matrix", "Matrix", "Terminal black with phosphor-green glow", false,
            "#050806", "#0A100C", "#101812", "#17231A",
            "#1B7A3D", "#14602F", "#E6F4EA", "#8FB59B", "#35B65C", "#C9A227"),
    ];

    public static AppTheme Default => All[0];
}
