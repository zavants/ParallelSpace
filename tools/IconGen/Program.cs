using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// Generates the Parallel Space app icon (multi-size .ico) and a header logo (.png).
// Motif: two overlapping app "windows" on a blurple tile — parallel copies.

var outDir = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ParallelSpace", "Assets"));
Directory.CreateDirectory(outDir);

int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };

// Build ICO from PNG-encoded entries (Vista+ format).
var pngs = sizes.ToDictionary(s => s, s => EncodePng(Render(s)));
WriteIco(Path.Combine(outDir, "appicon.ico"), pngs);

// Header logo at 256.
File.WriteAllBytes(Path.Combine(outDir, "logo.png"), EncodePng(Render(256)));

Console.WriteLine($"Wrote appicon.ico + logo.png to {outDir}");

static Bitmap Render(int s)
{
    var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.Clear(Color.Transparent);

    float S = s;
    float M = S * 0.055f;                      // outer margin
    var tile = new RectangleF(M, M, S - 2 * M, S - 2 * M);
    float tileR = S * 0.215f;

    // Blurple tile with a soft diagonal gradient.
    using (var path = Rounded(tile, tileR))
    using (var brush = new LinearGradientBrush(
               new PointF(M, M), new PointF(S - M, S - M),
               ColorFromHex("#6A76F5"), ColorFromHex("#4148B8")))
    {
        g.FillPath(brush, path);
    }

    // Card geometry (fractions of S).
    float cw = S * 0.44f, ch = S * 0.40f, cr = S * 0.075f;
    var back = new RectangleF(S * 0.34f, S * 0.17f, cw, ch);   // upper-right, translucent
    var front = new RectangleF(S * 0.21f, S * 0.31f, cw, ch);  // lower-left, solid

    // Subtle drop shadow under the front card.
    using (var shadow = Rounded(new RectangleF(front.X + S * 0.015f, front.Y + S * 0.02f, cw, ch), cr))
    using (var sb = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
        g.FillPath(sb, shadow);

    // Back card: frosted white.
    using (var path = Rounded(back, cr))
    using (var b = new SolidBrush(Color.FromArgb(115, 255, 255, 255)))
        g.FillPath(b, path);

    // Front card: solid white with a blurple title bar + two dots.
    using (var path = Rounded(front, cr))
    using (var b = new SolidBrush(Color.FromArgb(255, 250, 250, 252)))
        g.FillPath(b, path);

    float barH = ch * 0.26f;
    var bar = new RectangleF(front.X, front.Y, cw, barH);
    using (var clip = Rounded(front, cr))
    {
        var state = g.Save();
        g.SetClip(clip);
        using (var bb = new LinearGradientBrush(
                   new PointF(bar.X, bar.Y), new PointF(bar.Right, bar.Y),
                   ColorFromHex("#5865F2"), ColorFromHex("#4752C4")))
            g.FillRectangle(bb, bar);
        g.Restore(state);
    }

    // Two window dots on the title bar (only when large enough to read).
    if (s >= 32)
    {
        float dotR = barH * 0.17f;
        float dy = bar.Y + barH / 2f - dotR;
        float dx = front.X + cw * 0.10f;
        using var dot = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
        g.FillEllipse(dot, dx, dy, dotR * 2, dotR * 2);
        g.FillEllipse(dot, dx + dotR * 3.2f, dy, dotR * 2, dotR * 2);
    }

    return bmp;
}

static GraphicsPath Rounded(RectangleF r, float radius)
{
    float d = radius * 2f;
    var p = new GraphicsPath();
    p.AddArc(r.X, r.Y, d, d, 180, 90);
    p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
    p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
    p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
    p.CloseFigure();
    return p;
}

static Color ColorFromHex(string hex)
{
    hex = hex.TrimStart('#');
    return Color.FromArgb(
        Convert.ToInt32(hex.Substring(0, 2), 16),
        Convert.ToInt32(hex.Substring(2, 2), 16),
        Convert.ToInt32(hex.Substring(4, 2), 16));
}

static byte[] EncodePng(Bitmap bmp)
{
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}

static void WriteIco(string path, Dictionary<int, byte[]> images)
{
    using var fs = new FileStream(path, FileMode.Create);
    using var w = new BinaryWriter(fs);
    var entries = images.OrderBy(kv => kv.Key).ToList();

    // ICONDIR
    w.Write((ushort)0);              // reserved
    w.Write((ushort)1);              // type = icon
    w.Write((ushort)entries.Count);  // count

    int offset = 6 + entries.Count * 16;
    foreach (var (size, data) in entries)
    {
        w.Write((byte)(size >= 256 ? 0 : size)); // width
        w.Write((byte)(size >= 256 ? 0 : size)); // height
        w.Write((byte)0);            // color count
        w.Write((byte)0);            // reserved
        w.Write((ushort)1);          // planes
        w.Write((ushort)32);         // bit count
        w.Write(data.Length);        // bytes in resource
        w.Write(offset);             // image offset
        offset += data.Length;
    }
    foreach (var (_, data) in entries)
        w.Write(data);
}
