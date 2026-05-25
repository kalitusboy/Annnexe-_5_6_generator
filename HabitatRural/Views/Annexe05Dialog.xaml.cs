using System.Globalization;
using System.Windows;
using HabitatRural.Data;
using HabitatRural.Models;
using HabitatRural.Printing;

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
            Beneficiaire = b,
            NumeroCompte = b.NumeroCompte,
            BanqueAgence = b.BanqueAgence,
            DateDemande = DateTime.Today,
        };
        _d.Beneficiaire = b;
        LoadToForm();
    }

    private void LoadToForm()
    {
        TxtBeneficiaireInfo.Text = $"{_b.NomPrenom}  |  القرار رقم: {_b.NumeroDecision}  |  المبلغ: {_b.Montant:N2} دج";

        RbTranche1.IsChecked = _d.NumeroTranche != 2;
        RbTranche2.IsChecked = _d.NumeroTranche == 2;
        UpdateMontantCalcule();

        decimal mt = _d.MontantTranche > 0 ? _d.MontantTranche : _d.MontantCalcule;
        TxtMontantTranche.Text = mt.ToString("F2", CultureInfo.InvariantCulture);
        TxtMontantEnLettres.Text = _d.MontantEnLettres;
        TxtNumeroCompte.Text = _d.NumeroCompte;
        CbBanque.Text = _d.BanqueAgence;
        TxtLieu.Text = _d.Lieu;
        DpDateDemande.SelectedDate = _d.DateDemande;
        TxtReceptionNom.Text = _d.ReceptionNomPrenom;
        TxtReceptionQualite.Text = _d.ReceptionQualite;
        DpDateReception.SelectedDate = _d.DateReception;
        ChkEstRecu.IsChecked = _d.EstRecu;
    }

    private void UpdateMontantCalcule()
    {
        _d.NumeroTranche = RbTranche2.IsChecked == true ? 2 : 1;
        decimal calc = _d.MontantCalcule;
        TxtMontantCalcule.Text = $"{calc:N2} دج";
        // Pre-fill if not manually changed
        if (TxtMontantTranche != null && string.IsNullOrEmpty(TxtMontantTranche.Text))
            TxtMontantTranche.Text = calc.ToString("F2", CultureInfo.InvariantCulture);
    }

    private void RbTranche_Checked(object sender, RoutedEventArgs e)
    {
        UpdateMontantCalcule();
        if (TxtMontantTranche != null)
            TxtMontantTranche.Text = _d.MontantCalcule.ToString("F2", CultureInfo.InvariantCulture);
    }

    private void TxtMontantTranche_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // allow manual override
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
        _d.MontantTranche = mt > 0 ? mt : _d.MontantCalcule;
        _d.MontantEnLettres = TxtMontantEnLettres.Text.Trim().ToUpper();
        _d.NumeroCompte = TxtNumeroCompte.Text.Trim();
        _d.BanqueAgence = CbBanque.Text.Trim();
        _d.Lieu = TxtLieu.Text.Trim();
        _d.DateDemande = DpDateDemande.SelectedDate ?? DateTime.Today;
        _d.ReceptionNomPrenom = TxtReceptionNom.Text.Trim();
        _d.ReceptionQualite = TxtReceptionQualite.Text.Trim();
        _d.DateReception = DpDateReception.SelectedDate;
        _d.EstRecu = ChkEstRecu.IsChecked == true;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        CollectData();
        try
        {
            _d.Id = DatabaseService.Instance.SaveDemande(_d);
            Saved = true;
            MessageBox.Show("تم الحفظ بنجاح ✓", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
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
        var doc = Annexe05Renderer.Render(_d);
        var preview = new PrintPreviewDialog(doc,
            $"ANNEXE 05 – {_b.NomPrenom} – الشريحة {_d.NumeroTranche}") { Owner = this };
        preview.ShowDialog();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
