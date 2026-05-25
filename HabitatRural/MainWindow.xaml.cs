using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HabitatRural.Data;
using HabitatRural.Models;
using HabitatRural.Printing;
using HabitatRural.Views;

namespace HabitatRural;

public partial class MainWindow : Window
{
    private ObservableCollection<Beneficiaire> _allBeneficiaires = new();
    private Beneficiaire? SelectedBeneficiaire =>
        LstBeneficiaires.SelectedItem as Beneficiaire;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TxtDbPath.Text = $"  |  قاعدة البيانات: {DatabaseService.Instance.DbPath}";
        LoadBeneficiaires();
    }

    // ── Data loading ──────────────────────────────────────────────────────
    private void LoadBeneficiaires()
    {
        _allBeneficiaires = new ObservableCollection<Beneficiaire>(
            DatabaseService.Instance.GetAllBeneficiaires());
        FilterBeneficiaires(TxtSearch.Text);
        UpdateStatus($"إجمالي المستفيدين: {_allBeneficiaires.Count}");
    }

    private void FilterBeneficiaires(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            LstBeneficiaires.ItemsSource = _allBeneficiaires;
        }
        else
        {
            var q = query.Trim().ToLower();
            LstBeneficiaires.ItemsSource = _allBeneficiaires
                .Where(b => b.NomPrenom.ToLower().Contains(q)
                         || b.Code.ToLower().Contains(q)
                         || b.NumeroDecision.ToLower().Contains(q))
                .ToList();
        }
    }

    private void LoadDocumentsForSelected()
    {
        if (SelectedBeneficiaire == null)
        {
            DgAnnexe05.ItemsSource = null;
            DgAnnexe06.ItemsSource = null;
            return;
        }
        DgAnnexe05.ItemsSource = DatabaseService.Instance
            .GetDemandesForBeneficiaire(SelectedBeneficiaire.Id);
        DgAnnexe06.ItemsSource = DatabaseService.Instance
            .GetPVsForBeneficiaire(SelectedBeneficiaire.Id);
    }

    private void UpdateStatus(string msg) => TxtStatus.Text = msg;

    // ── Search ────────────────────────────────────────────────────────────
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        => FilterBeneficiaires(TxtSearch.Text);

    // ── Beneficiaire actions ──────────────────────────────────────────────
    private void LstBeneficiaires_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => LoadDocumentsForSelected();

    private void BtnNewBeneficiaire_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new BeneficiaireEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadBeneficiaires();
    }

    private void BtnEditBeneficiaire_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBeneficiaire == null)
        {
            MessageBox.Show("الرجاء تحديد مستفيد من القائمة", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new BeneficiaireEditDialog(SelectedBeneficiaire) { Owner = this };
        if (dlg.ShowDialog() == true) LoadBeneficiaires();
    }

    private void BtnDeleteBeneficiaire_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBeneficiaire == null) return;
        var res = MessageBox.Show(
            $"هل تريد حذف المستفيد:\n{SelectedBeneficiaire.NomPrenom}\n\n⚠  سيتم حذف جميع وثائقه أيضاً.",
            "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (res != MessageBoxResult.Yes) return;
        DatabaseService.Instance.DeleteBeneficiaire(SelectedBeneficiaire.Id);
        LoadBeneficiaires();
    }

    // ── Annexe 05 actions ─────────────────────────────────────────────────
    private void BtnNewAnnexe05_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureBeneficiaireSelected()) return;
        var dlg = new Annexe05Dialog(SelectedBeneficiaire!) { Owner = this };
        if (dlg.ShowDialog() == true) LoadDocumentsForSelected();
    }

    private void BtnEditAnnexe05_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DemandeVersement d)
        {
            var b = DatabaseService.Instance.GetBeneficiaire(d.BeneficiaireId)!;
            var dlg = new Annexe05Dialog(b, d) { Owner = this };
            if (dlg.ShowDialog() == true) LoadDocumentsForSelected();
        }
    }

    private void BtnPrintAnnexe05_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DemandeVersement d)
        {
            d.Beneficiaire = DatabaseService.Instance.GetBeneficiaire(d.BeneficiaireId);
            var doc = Annexe05Renderer.Render(d);
            var preview = new PrintPreviewDialog(doc,
                $"ANNEXE 05 – {d.Beneficiaire?.NomPrenom} – الشريحة {d.NumeroTranche}")
            { Owner = this };
            preview.ShowDialog();
        }
    }

    private void BtnDeleteAnnexe05_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DemandeVersement d)
        {
            if (MessageBox.Show($"حذف الطلب – الشريحة {d.NumeroTranche}؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DatabaseService.Instance.DeleteDemande(d.Id);
                LoadDocumentsForSelected();
            }
        }
    }

    // ── Annexe 06 actions ─────────────────────────────────────────────────
    private void BtnNewAnnexe06_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureBeneficiaireSelected()) return;
        var dlg = new Annexe06Dialog(SelectedBeneficiaire!) { Owner = this };
        if (dlg.ShowDialog() == true) LoadDocumentsForSelected();
    }

    private void BtnEditAnnexe06_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ProcesVerbal pv)
        {
            var fullPV = DatabaseService.Instance.GetPV(pv.Id)!;
            fullPV.Beneficiaire = DatabaseService.Instance.GetBeneficiaire(fullPV.BeneficiaireId);
            var dlg = new Annexe06Dialog(fullPV.Beneficiaire!, fullPV) { Owner = this };
            if (dlg.ShowDialog() == true) LoadDocumentsForSelected();
        }
    }

    private void BtnPrintAnnexe06_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ProcesVerbal pv)
        {
            var fullPV = DatabaseService.Instance.GetPV(pv.Id)!;
            fullPV.Beneficiaire = DatabaseService.Instance.GetBeneficiaire(fullPV.BeneficiaireId);
            var doc = Annexe06Renderer.Render(fullPV);
            var preview = new PrintPreviewDialog(doc,
                $"ANNEXE 06 – {fullPV.Beneficiaire?.NomPrenom}")
            { Owner = this };
            preview.ShowDialog();
        }
    }

    private void BtnDeleteAnnexe06_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ProcesVerbal pv)
        {
            if (MessageBox.Show("حذف محضر المعاينة؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DatabaseService.Instance.DeletePV(pv.Id);
                LoadDocumentsForSelected();
            }
        }
    }

    // ── Toolbar ───────────────────────────────────────────────────────────
    private void BtnParametres_Click(object sender, RoutedEventArgs e)
    {
        new ParametresDialog { Owner = this }.ShowDialog();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadBeneficiaires();
        LoadDocumentsForSelected();
    }

    private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var folder = Path.GetDirectoryName(DatabaseService.Instance.DbPath)!;
        Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
    }

    // ── Helper ────────────────────────────────────────────────────────────
    private bool EnsureBeneficiaireSelected()
    {
        if (SelectedBeneficiaire != null) return true;
        MessageBox.Show("الرجاء تحديد مستفيد من القائمة أولاً", "تنبيه",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }
}
