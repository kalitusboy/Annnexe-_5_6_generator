using System.Windows;
using System.Windows.Controls;
using HabitatRural.Data;
using HabitatRural.Models;

namespace HabitatRural.Views;

public partial class ParametresDialog : Window
{
    public ParametresDialog()
    {
        InitializeComponent();
        LoadAll();
    }

    private void LoadAll()
    {
        var p = DatabaseService.Instance.GetParametres();
        TxtCommune.Text = p.Commune;
        TxtDaira.Text   = p.Daira;
        TxtWilaya.Text  = p.Wilaya;
        DgIngenieurs.ItemsSource = DatabaseService.Instance.GetIngenieurs(actifOnly: false);
    }

    private void BtnSaveParams_Click(object sender, RoutedEventArgs e)
    {
        DatabaseService.Instance.SaveParametres(new AppParametres
        {
            Commune = TxtCommune.Text.Trim().ToUpper(),
            Daira   = TxtDaira.Text.Trim().ToUpper(),
            Wilaya  = TxtWilaya.Text.Trim().ToUpper(),
        });
        MessageBox.Show("تم حفظ الإعدادات ✓", "تم",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnAddIngenieur_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtIngNom.Text))
        {
            MessageBox.Show("الرجاء إدخال لقب المهندس", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var ing = new Ingenieur
        {
            Nom     = TxtIngNom.Text.Trim().ToUpper(),
            Prenom  = TxtIngPrenom.Text.Trim().ToUpper(),
            Qualite = TxtIngQualite.Text.Trim(),
            Actif   = true,
        };
        DatabaseService.Instance.SaveIngenieur(ing);
        TxtIngNom.Clear(); TxtIngPrenom.Clear();
        TxtIngQualite.Text = "Chargé du suivi à la subdivision";
        DgIngenieurs.ItemsSource = DatabaseService.Instance.GetIngenieurs(actifOnly: false);
    }

    private void BtnDeleteIngenieur_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Ingenieur ing)
        {
            if (MessageBox.Show($"حذف المهندس {ing.NomComplet}؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DatabaseService.Instance.DeleteIngenieur(ing.Id);
                DgIngenieurs.ItemsSource = DatabaseService.Instance.GetIngenieurs(actifOnly: false);
            }
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
