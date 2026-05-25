namespace HabitatRural.Models;

public class Beneficiaire
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string NomPrenom { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string Fraction { get; set; } = "";
    public string NumeroDecision { get; set; } = "";
    public DateTime DateDecision { get; set; } = DateTime.Today;
    public decimal Montant { get; set; } = 700000;
    public string LocalisationProjet { get; set; } = "";
    public string FractionProjet { get; set; } = "";
    public string Commune { get; set; } = "DEUX BASSINS";
    public string NumeroCompte { get; set; } = "";
    public string BanqueAgence { get; set; } = "BADR";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public override string ToString() => $"{Code} - {NomPrenom}";
}
