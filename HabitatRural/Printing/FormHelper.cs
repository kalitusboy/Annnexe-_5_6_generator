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

    public const double PageWidthMm = 210;
    public const double PageHeightMm = 297;
    public const double PageWidth = PageWidthMm * MM;
    public const double PageHeight = PageHeightMm * MM;

    // Standard margins (mm)
    public const double MarginLeft = 15;
    public const double MarginRight = 15;
    public const double MarginTop = 10;
    public const double MarginBottom = 10;
    public const double ContentWidth = PageWidthMm - MarginLeft - MarginRight; // 180 mm

    // ── Canvas builder ────────────────────────────────────────────────────
    public static Canvas CreateA4Canvas()
    {
        return new Canvas
        {
            Width = PageWidth,
            Height = PageHeight,
            Background = Brushes.White,
            ClipToBounds = true,
        };
    }

    // ── Line drawing ──────────────────────────────────────────────────────
    public static void DrawHLine(Canvas c, double xMm, double yMm, double widthMm,
        double thicknessMm = 0.25, bool bold = false)
    {
        c.Children.Add(new Line
        {
            X1 = xMm * MM, Y1 = yMm * MM,
            X2 = (xMm + widthMm) * MM, Y2 = yMm * MM,
            Stroke = Brushes.Black,
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
            Stroke = Brushes.Black,
            StrokeThickness = thicknessMm * MM,
        });
    }

    public static void DrawRect(Canvas c, double xMm, double yMm, double wMm, double hMm,
        double thicknessMm = 0.25, Brush? fill = null)
    {
        c.Children.Add(new System.Windows.Shapes.Rectangle
        {
            Width = wMm * MM, Height = hMm * MM,
            Stroke = Brushes.Black,
            StrokeThickness = thicknessMm * MM,
            Fill = fill ?? Brushes.Transparent,
            [Canvas.LeftProperty] = xMm * MM,
            [Canvas.TopProperty] = yMm * MM,
        });
    }

    // ── Text drawing ──────────────────────────────────────────────────────
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
            Text = text,
            FontSize = fontPt * PT,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            FontStyle = italic ? FontStyles.Italic : FontStyles.Normal,
            FontFamily = new FontFamily(fontFamily ?? "Arial"),
            Foreground = foreground ?? Brushes.Black,
            TextAlignment = align,
            TextWrapping = wrap,
            TextDecorations = underline ? TextDecorations.Underline : null,
        };
        if (widthMm.HasValue) tb.Width = widthMm.Value * MM;
        Canvas.SetLeft(tb, xMm * MM);
        Canvas.SetTop(tb, yMm * MM);
        c.Children.Add(tb);
        return tb;
    }

    /// <summary>Draw centered text within a box defined by xMm..xMm+wMm</summary>
    public static void AddCenteredText(Canvas c,
        string text, double xMm, double yMm, double wMm,
        double fontPt = 9, bool bold = false)
        => AddText(c, text, xMm, yMm, fontPt, bold, widthMm: wMm, align: TextAlignment.Center);

    /// <summary>Draw an underline field (dotted line for writing)</summary>
    public static void DrawField(Canvas c, double xMm, double yMm, double widthMm,
        string? filledValue = null, double fontPt = 9)
    {
        // Underline
        DrawHLine(c, xMm, yMm + 4.5, widthMm);
        if (!string.IsNullOrEmpty(filledValue))
            AddText(c, filledValue, xMm + 0.5, yMm - 0.5, fontPt, bold: true);
    }

    /// <summary>Draw checkbox (square) optionally with X inside</summary>
    public static void DrawCheckbox(Canvas c, double xMm, double yMm,
        double sizeMm = 4, bool ticked = false)
    {
        DrawRect(c, xMm, yMm, sizeMm, sizeMm, 0.3);
        if (ticked)
        {
            // Draw X
            c.Children.Add(new Line
            {
                X1 = (xMm + 0.5) * MM, Y1 = (yMm + 0.5) * MM,
                X2 = (xMm + sizeMm - 0.5) * MM, Y2 = (yMm + sizeMm - 0.5) * MM,
                Stroke = Brushes.Black, StrokeThickness = 0.4 * MM,
            });
            c.Children.Add(new Line
            {
                X1 = (xMm + sizeMm - 0.5) * MM, Y1 = (yMm + 0.5) * MM,
                X2 = (xMm + 0.5) * MM, Y2 = (yMm + sizeMm - 0.5) * MM,
                Stroke = Brushes.Black, StrokeThickness = 0.4 * MM,
            });
        }
    }

    // ── FixedDocument builder ─────────────────────────────────────────────
    public static FixedDocument WrapCanvasInFixedDocument(Canvas canvas)
    {
        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(PageWidth, PageHeight);

        var pageContent = new PageContent();
        var fixedPage = new FixedPage
        {
            Width = PageWidth,
            Height = PageHeight,
            Background = Brushes.White,
        };
        fixedPage.Children.Add(canvas);
        ((IAddChild)pageContent).AddChild(fixedPage);
        fixedDoc.Pages.Add(pageContent);
        return fixedDoc;
    }

    /// <summary>Format date as DD/MM/YYYY</summary>
    public static string FormatDate(DateTime dt) => dt.ToString("dd/MM/yyyy");

    /// <summary>Format decimal as amount: "420 000,00 DA"</summary>
    public static string FormatAmount(decimal amount)
        => amount.ToString("N2", new CultureInfo("fr-FR")) + " DA";
}
