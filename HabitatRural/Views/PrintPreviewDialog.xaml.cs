using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace HabitatRural.Views;

public partial class PrintPreviewDialog : Window
{
    private readonly FixedDocument _document;

    public PrintPreviewDialog(FixedDocument document, string title = "")
    {
        InitializeComponent();
        _document = document;
        if (!string.IsNullOrEmpty(title)) Title = $"معاينة – {title}";
        DocViewer.Document = _document;
    }

    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
        {
            dlg.PrintDocument(_document.DocumentPaginator, Title);
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
