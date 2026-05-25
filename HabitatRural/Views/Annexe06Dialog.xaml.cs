using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HabitatRural.Data;
using HabitatRural.Export;
using HabitatRural.Models;
using HabitatRural.Printing;
using Microsoft.Win32;

namespace HabitatRural.Views;

public partial class Annexe06Dialog : Window
{
    private readonly ProcesVerbal _pv;
    private readonly Beneficiaire _b;
    private readonly ObservableCollection<RubriqueConstat> _rubriques;
    public bool Saved { get; private set; }

    public Annexe06Dialog(Beneficiaire b, ProcesVerbal? existing = null)
    {
        InitializeComponent();
        _b = b;

        var param = DatabaseService.Instance.GetParametres();
        _pv = existing ?? new ProcesVerbal
        {
            BeneficiaireId = b.Id,
            Beneficiaire   = b,
            FractionProjet = b.FractionProjet,
            DirectionLogement = param.Wilaya,
            Daira   = param.Daira,
            Commune = param.Commune,
        };
        _pv.Beneficiaire = b;

        _rubriques = new ObservableCollection<RubriqueConstat>(_pv.Rubriques);
        IcRubriques.ItemsSource = _rubriques;

        LoadIngenieurs();
        LoadToForm();
    }

    private void LoadIngenieurs()
    {
        var ingenieurs = DatabaseService.Instance.GetIngenieurs();
        CbIngenieur.ItemsSource   = ingenieurs;
        CbIngenieur.DisplayMemberPath = "NomComplet";

        // Pre-select if already set
        if (!string.IsNullOrEmpty(_pv.NomSubdivisionnaire))
        {
            var match = ingenieurs.FirstOrDefault(i =>
                i.NomComplet == _pv.NomSubdivisionnaire.Trim().ToUpper());
            if (match != null) CbIngenieur.SelectedItem = match;
            else CbIngenieur.Text = _pv.NomSubdivisionnaire;
        }
    }

    private void LoadToForm()
    {
        TxtBeneficiaireInfo.Text =
            $"{_b.NomPrenom}  |  القرار: {_b.NumeroDecision}  |  البلدية: {_b.Commune}";

        TxtDirectionLogement.Text = _pv.DirectionLogement;
        TxtDaira.Text    = _pv.Daira;
        TxtCommune.Text  = _pv.Commune;
        DpDateVisite.SelectedDate = _pv.DateVisite;
        TxtFractionProjet.Text = _pv.FractionProjet;
        TxtNumeroPermis.Text   = _pv.NumeroPermis;
        DpDatePermis.SelectedDate = _pv.DatePermis;
        ChkTranche1.IsChecked  = _pv.Tranche1;
        ChkTranche2.IsChecked  = _pv.Tranche2;
        TxtObservations.Text   = _pv.ObservationsComplementaires;
        TxtNomSignataire.Text  = _pv.NomSignataire;
        TxtLieu.Text = _pv.Lieu;
        DpDatePV.SelectedDate  = _pv.DatePV;
    }

    private void CbIngenieur_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CbIngenieur.SelectedItem is Ingenieur ing)
        {
            TxtNomSignataire.Text = ing.NomComplet;
        }
    }

    private void BtnAddRubrique_Click(object sender, RoutedEventArgs e)
    {
        _rubriques.Add(new RubriqueConstat
        {
            Rubrique = "", EnChiffre = "", EnLettres = "",
            HasObservation = false, Ordre = _rubriques.Count + 1,
        });
    }

    private void BtnDeleteRubrique_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is RubriqueConstat rub)
        {
            if (MessageBox.Show("هل تريد حذف هذا السطر؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                _rubriques.Remove(rub);
        }
    }

    private bool Validate()
    {
        if (CbIngenieur.SelectedItem == null && string.IsNullOrWhiteSpace(TxtNomSignataire.Text))
        {
            MessageBox.Show("الرجاء تحديد المهندس المتابع", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (_rubriques.Count == 0)
        {
            MessageBox.Show("الرجاء إضافة سطر واحد على الأقل في جدول الإنجازات", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void CollectData()
    {
        // Engineer
        if (CbIngenieur.SelectedItem is Ingenieur ing)
        {
            _pv.NomSubdivisionnaire      = ing.NomComplet;
            _pv.QualiteSubdivisionnaire  = ing.Qualite;
        }
        else
        {
            _pv.NomSubdivisionnaire = TxtNomSignataire.Text.Trim().ToUpper();
        }

        _pv.DirectionLogement = TxtDirectionLogement.Text.Trim().ToUpper();
        _pv.Daira    = TxtDaira.Text.Trim().ToUpper();
        _pv.Commune  = TxtCommune.Text.Trim().ToUpper();
        _pv.DateVisite = DpDateVisite.SelectedDate;
        _pv.FractionProjet = TxtFractionProjet.Text.Trim().ToUpper();
        _pv.NumeroPermis   = TxtNumeroPermis.Text.Trim();
        _pv.DatePermis     = DpDatePermis.SelectedDate;
        _pv.Tranche1 = ChkTranche1.IsChecked == true;
        _pv.Tranche2 = ChkTranche2.IsChecked == true;
        _pv.ObservationsComplementaires = TxtObservations.Text.Trim();
        _pv.NomSignataire = TxtNomSignataire.Text.Trim().ToUpper();
        _pv.Lieu   = TxtLieu.Text.Trim().ToUpper();
        _pv.DatePV = DpDatePV.SelectedDate;

        int i = 1;
        foreach (var r in _rubriques)
        {
            r.Ordre    = i++;
            r.Rubrique = r.Rubrique?.Trim().ToUpper() ?? "";
            r.EnChiffre = r.EnChiffre?.Trim().ToUpper() ?? "";
            r.EnLettres = r.EnLettres?.Trim().ToUpper() ?? "";
        }
        _pv.Rubriques    = _rubriques.ToList();
        _pv.Beneficiaire = _b;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        CollectData();
        try
        {
            _pv.Id = DatabaseService.Instance.SavePV(_pv);
            Saved  = true;
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
        var doc = Annexe06Renderer.Render(_pv);
        var preview = new PrintPreviewDialog(doc,
            $"ANNEXE 06 – {_b.NomPrenom}") { Owner = this };
        preview.ShowDialog();
    }

    private void BtnExportWord_Click(object sender, RoutedEventArgs e)
    {
        CollectData();
        var dlg = new SaveFileDialog
        {
            Filter   = "Word Document (*.docx)|*.docx",
            FileName = $"Annexe06_{_b.Code}_{_b.NomPrenom}.docx"
                .Replace(" ", "_").Replace("/", "-"),
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            string folder = System.IO.Path.GetDirectoryName(dlg.FileName)!;
            var path = WordExporter.ExportAnnexe06(_pv, folder);
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
