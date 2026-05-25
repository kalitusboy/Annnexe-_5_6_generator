using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HabitatRural.Printing;

/// <summary>
/// Helpers for drawing official forms on a WPF Canvas at exact mm dimensions.
/// A4 = 210 × 297 mm   (at 96 DPI: 793.7 × 1122.5 px)
/// </summary>
public static class FormHelper
{
    // ── Unit conversion ────────────────────────────────────────────────────
    public const double MM = 96.0 / 25.4;   // px per mm  (~3.7795)
    public const double PT = 96.0 / 72.0;   // px per pt  (~1.3333)

    public const double PageWidthMm  = 210;
    public const double PageHeightMm = 297;
    public const double PageWidth    = PageWidthMm  * MM;
    public const double PageHeight   = PageHeightMm * MM;

    public const double MarginLeft    = 15;
    public const double MarginRight   = 15;
    public const double MarginTop     = 10;
    public const double MarginBottom  = 10;
    public const double ContentWidth  = PageWidthMm - MarginLeft - MarginRight; // 180 mm

    // ── Canvas builder ────────────────────────────────────────────────────
    public static Canvas CreateA4Canvas() => new()
    {
        Width        = PageWidth,
        Height       = PageHeight,
        Background   = Brushes.White,
        ClipToBounds = true,
    };

    // ── Line helpers ──────────────────────────────────────────────────────
    public static void DrawHLine(Canvas c, double xMm, double yMm, double widthMm,
        double thicknessMm = 0.25, bool bold = false)
    {
        c.Children.Add(new Line
        {
            X1 = xMm * MM,             Y1 = yMm * MM,
            X2 = (xMm + widthMm) * MM, Y2 = yMm * MM,
            Stroke          = Brushes.Black,
            StrokeThickness = (bold ? 0.5 : thicknessMm) * MM,
        });
    }

    public static void DrawVLine(Canvas c, double xMm, double yMm, double heightMm,
        double thicknessMm = 0.25)
    {
        c.Children.Add(new Line
        {
            X1 = xMm * MM, Y1 = yMm * MM,
            X2 = xMm * MM, Y2 = (yMm + heightMm) * MM,
            Stroke          = Brushes.Black,
            StrokeThickness = thicknessMm * MM,
        });
    }

    /// <summary>
    /// Draw a rectangle. Uses Canvas.SetLeft/SetTop (NOT object-initializer indexer,
    /// which doesn't work with WPF attached DependencyProperties).
    /// </summary>
    public static void DrawRect(Canvas c, double xMm, double yMm, double wMm, double hMm,
        double thicknessMm = 0.25, Brush? fill = null)
    {
        var rect = new Rectangle
        {
            Width           = wMm * MM,
            Height          = hMm * MM,
            Stroke          = Brushes.Black,
            StrokeThickness = thicknessMm * MM,
            Fill            = fill ?? Brushes.Transparent,
        };
        Canvas.SetLeft(rect, xMm * MM);
        Canvas.SetTop (rect, yMm * MM);
        c.Children.Add(rect);
    }

    // ── Text helpers ──────────────────────────────────────────────────────
    public static TextBlock AddText(Canvas c,
        string text, double xMm, double yMm,
        double fontPt = 9,
        bool bold = false, bool italic = false,
        double? widthMm = null,
        TextAlignment align = TextAlignment.Left,
        string? fontFamily = null,
        Brush? foreground = null,
        bool underline = false,
        TextWrapping wrap = TextWrapping.NoWrap)
    {
        var tb = new TextBlock
        {
            Text           = text,
            FontSize       = fontPt * PT,
            FontWeight     = bold   ? FontWeights.Bold   : FontWeights.Normal,
            FontStyle      = italic ? FontStyles.Italic  : FontStyles.Normal,
            FontFamily     = new FontFamily(fontFamily ?? "Arial"),
            Foreground     = foreground ?? Brushes.Black,
            TextAlignment  = align,
            TextWrapping   = wrap,
            TextDecorations = underline ? TextDecorations.Underline : null,
        };
        if (widthMm.HasValue) tb.Width = widthMm.Value * MM;
        Canvas.SetLeft(tb, xMm * MM);
        Canvas.SetTop (tb, yMm * MM);
        c.Children.Add(tb);
        return tb;
    }

    /// <summary>Draw text centered inside a box of width wMm starting at xMm.</summary>
    public static void AddCenteredText(Canvas c,
        string text, double xMm, double yMm, double wMm,
        double fontPt = 9, bool bold = false)
        => AddText(c, text, xMm, yMm, fontPt, bold, widthMm: wMm, align: TextAlignment.Center);

    /// <summary>Draw an underline field, optionally pre-filled.</summary>
    public static void DrawField(Canvas c, double xMm, double yMm, double widthMm,
        string? filledValue = null, double fontPt = 9)
    {
        DrawHLine(c, xMm, yMm + 4.5, widthMm);
        if (!string.IsNullOrEmpty(filledValue))
            AddText(c, filledValue, xMm + 0.5, yMm - 0.5, fontPt, bold: true);
    }

    /// <summary>Draw a checkbox square, optionally ticked with an X.</summary>
    public static void DrawCheckbox(Canvas c, double xMm, double yMm,
        double sizeMm = 4, bool ticked = false)
    {
        DrawRect(c, xMm, yMm, sizeMm, sizeMm, 0.3);
        if (!ticked) return;

        c.Children.Add(new Line
        {
            X1 = (xMm + 0.5) * MM,          Y1 = (yMm + 0.5) * MM,
            X2 = (xMm + sizeMm - 0.5) * MM, Y2 = (yMm + sizeMm - 0.5) * MM,
            Stroke = Brushes.Black, StrokeThickness = 0.4 * MM,
        });
        c.Children.Add(new Line
        {
            X1 = (xMm + sizeMm - 0.5) * MM, Y1 = (yMm + 0.5) * MM,
            X2 = (xMm + 0.5) * MM,          Y2 = (yMm + sizeMm - 0.5) * MM,
            Stroke = Brushes.Black, StrokeThickness = 0.4 * MM,
        });
    }

    // ── FixedDocument builder ─────────────────────────────────────────────
    public static FixedDocument WrapCanvasInFixedDocument(Canvas canvas)
    {
        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(PageWidth, PageHeight);

        var pageContent = new PageContent();
        var fixedPage   = new FixedPage
        {
            Width      = PageWidth,
            Height     = PageHeight,
            Background = Brushes.White,
        };
        fixedPage.Children.Add(canvas);
        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
        fixedDoc.Pages.Add(pageContent);
        return fixedDoc;
    }

    // ── Formatters ────────────────────────────────────────────────────────
    public static string FormatDate(DateTime dt)   => dt.ToString("dd/MM/yyyy");
    public static string FormatAmount(decimal amt) =>
        amt.ToString("N2", new CultureInfo("fr-FR")) + " DA";
}
