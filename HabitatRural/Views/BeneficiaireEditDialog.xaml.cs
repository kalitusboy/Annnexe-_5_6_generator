using System.Windows;
using HabitatRural.Data;
using HabitatRural.Models;

namespace HabitatRural.Views;

public partial class BeneficiaireEditDialog : Window
{
    private readonly Beneficiaire _b;
    public bool Saved { get; private set; }

    public BeneficiaireEditDialog(Beneficiaire? beneficiaire = null)
    {
        InitializeComponent();
        _b = beneficiaire ?? new Beneficiaire();
        LoadToForm();
    }

    private void LoadToForm()
    {
        TxtCode.Text = _b.Code;
        TxtNomPrenom.Text = _b.NomPrenom;
        TxtAdresse.Text = _b.Adresse;
        TxtFraction.Text = _b.Fraction;
        TxtNumeroDecision.Text = _b.NumeroDecision;
        DpDateDecision.SelectedDate = _b.DateDecision == default ? DateTime.Today : _b.DateDecision;
        TxtMontant.Text = _b.Montant.ToString("F2");
        TxtCommune.Text = _b.Commune;
        TxtLocalisationProjet.Text = _b.LocalisationProjet;
        TxtFractionProjet.Text = _b.FractionProjet;
        TxtNumeroCompte.Text = _b.NumeroCompte;
        CbBanqueAgence.Text = _b.BanqueAgence;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNomPrenom.Text))
        {
            MessageBox.Show("الرجاء إدخال اسم ولقب المستفيد", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(TxtMontant.Text.Replace(",", "."), out var montant))
        {
            MessageBox.Show("مبلغ المساعدة غير صحيح", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _b.Code = TxtCode.Text.Trim();
        _b.NomPrenom = TxtNomPrenom.Text.Trim();
        _b.Adresse = TxtAdresse.Text.Trim();
        _b.Fraction = TxtFraction.Text.Trim();
        _b.NumeroDecision = TxtNumeroDecision.Text.Trim();
        _b.DateDecision = DpDateDecision.SelectedDate ?? DateTime.Today;
        _b.Montant = montant;
        _b.Commune = TxtCommune.Text.Trim();
        _b.LocalisationProjet = TxtLocalisationProjet.Text.Trim();
        _b.FractionProjet = TxtFractionProjet.Text.Trim();
        _b.NumeroCompte = TxtNumeroCompte.Text.Trim();
        _b.BanqueAgence = CbBanqueAgence.Text.Trim();

        try
        {
            _b.Id = DatabaseService.Instance.SaveBeneficiaire(_b);
            Saved = true;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحفظ:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
