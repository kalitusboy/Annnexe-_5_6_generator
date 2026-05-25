using System.Globalization;
using System.Windows;
using HabitatRural.Data;
using HabitatRural.Export;
using HabitatRural.Models;
using HabitatRural.Printing;
using HabitatRural.Utils;
using Microsoft.Win32;

namespace HabitatRural.Views;

public partial class Annexe05Dialog : Window
{
    private readonly DemandeVersement _d;
    private readonly Beneficiaire _b;
    public bool Saved { get; private set; }

    public Annexe05Dialog(Beneficiaire b, DemandeVersement? existing = null)
    {
        InitializeComponent();
        _b = b;
        _d = existing ?? new DemandeVersement
        {
            BeneficiaireId = b.Id,
            Beneficiaire   = b,
            NumeroCompte   = b.NumeroCompte,
            BanqueAgence   = b.BanqueAgence,
        };
        _d.Beneficiaire = b;
        LoadToForm();
    }

    private void LoadToForm()
    {
        TxtBeneficiaireInfo.Text =
            $"{_b.NomPrenom}  |  القرار: {_b.NumeroDecision}  |  المبلغ: {_b.Montant:N2} دج";

        RbTranche1.IsChecked = _d.NumeroTranche != 2;
        RbTranche2.IsChecked = _d.NumeroTranche == 2;
        UpdateMontantCalcule();

        decimal mt = _d.MontantTranche > 0 ? _d.MontantTranche : _d.MontantCalcule;
        TxtMontantTranche.Text  = mt.ToString("F2", CultureInfo.InvariantCulture);
        TxtMontantEnLettres.Text = string.IsNullOrEmpty(_d.MontantEnLettres)
            ? NumberToWordsFr.Convert(mt) : _d.MontantEnLettres;
        TxtNumeroCompte.Text  = _d.NumeroCompte;
        CbBanque.Text         = _d.BanqueAgence;
        TxtLieu.Text          = _d.Lieu;
        DpDateDemande.SelectedDate = _d.DateDemande; // null = vide
    }

    private void UpdateMontantCalcule()
    {
        _d.NumeroTranche = RbTranche2.IsChecked == true ? 2 : 1;
        TxtMontantCalcule.Text = $"{_d.MontantCalcule:N2} دج";
    }

    private void RbTranche_Checked(object sender, RoutedEventArgs e)
    {
        UpdateMontantCalcule();
        if (TxtMontantTranche != null)
        {
            TxtMontantTranche.Text = _d.MontantCalcule.ToString("F2", CultureInfo.InvariantCulture);
            AutoFillLettres();
        }
    }

    private void TxtMontantTranche_TextChanged(object sender,
        System.Windows.Controls.TextChangedEventArgs e) => AutoFillLettres();

    private void AutoFillLettres()
    {
        if (TxtMontantTranche == null || TxtMontantEnLettres == null) return;
        if (decimal.TryParse(TxtMontantTranche.Text.Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var mt) && mt > 0)
            TxtMontantEnLettres.Text = NumberToWordsFr.Convert(mt);
    }

    private bool Validate()
    {
        if (!decimal.TryParse(TxtMontantTranche.Text.Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var mt) || mt <= 0)
        {
            MessageBox.Show("الرجاء إدخال مبلغ صحيح للشريحة", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void CollectData()
    {
        _d.NumeroTranche = RbTranche2.IsChecked == true ? 2 : 1;
        decimal.TryParse(TxtMontantTranche.Text.Replace(",", "."),
            NumberStyles.Any, CultureInfo.InvariantCulture, out var mt);
        _d.MontantTranche    = mt > 0 ? mt : _d.MontantCalcule;
        _d.MontantEnLettres  = TxtMontantEnLettres.Text.Trim().ToUpper();
        _d.NumeroCompte      = TxtNumeroCompte.Text.Trim();
        _d.BanqueAgence      = CbBanque.Text.Trim();
        _d.Lieu              = TxtLieu.Text.Trim();
        _d.DateDemande       = DpDateDemande.SelectedDate; // nullable
        _d.EstRecu           = ChkEstRecu.IsChecked == true;
        _d.ReceptionNomPrenom = TxtReceptionNom.Text.Trim();
        _d.ReceptionQualite  = TxtReceptionQualite.Text.Trim();
        _d.DateReception     = DpDateReception.SelectedDate;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        CollectData();
        try
        {
            _d.Id = DatabaseService.Instance.SaveDemande(_d);
            Saved = true;
            MessageBox.Show("تم الحفظ بنجاح ✓", "تم",
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحفظ:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPreview_Click(object sender, RoutedEventArgs e)
    {
        CollectData();
        _d.Beneficiaire = _b;
        var doc     = Annexe05Renderer.Render(_d);
        var preview = new PrintPreviewDialog(doc,
            $"ANNEXE 05 – {_b.NomPrenom} – الشريحة {_d.NumeroTranche}") { Owner = this };
        preview.ShowDialog();
    }

    private void BtnExportWord_Click(object sender, RoutedEventArgs e)
    {
        CollectData();
        _d.Beneficiaire = _b;
        var dlg = new SaveFileDialog
        {
            Filter   = "Word Document (*.docx)|*.docx",
            FileName = $"Annexe05_{_b.Code}_{_b.NomPrenom}_T{_d.NumeroTranche}.docx"
                .Replace(" ", "_").Replace("/", "-"),
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            string folder = System.IO.Path.GetDirectoryName(dlg.FileName)!;
            string name   = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
            var path = WordExporter.ExportAnnexe05(_d, folder);
            // rename if needed
            if (!path.Equals(dlg.FileName, StringComparison.OrdinalIgnoreCase))
            {
                if (System.IO.File.Exists(dlg.FileName)) System.IO.File.Delete(dlg.FileName);
                System.IO.File.Move(path, dlg.FileName);
            }
            MessageBox.Show($"تم التصدير بنجاح ✓\n{dlg.FileName}", "تصدير Word",
                MessageBoxButton.OK, MessageBoxImage.Information);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = dlg.FileName, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
