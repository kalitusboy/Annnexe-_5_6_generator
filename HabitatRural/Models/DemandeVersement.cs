namespace HabitatRural.Models;

public class DemandeVersement
{
    public int Id { get; set; }
    public int BeneficiaireId { get; set; }
    public Beneficiaire? Beneficiaire { get; set; }

    public int NumeroTranche { get; set; } = 1;
    public decimal MontantTranche { get; set; }
    public string MontantEnLettres { get; set; } = "";
    public string NumeroCompte { get; set; } = "";
    public string BanqueAgence { get; set; } = "BADR";
    public string Lieu { get; set; } = "DEUX BASSINS";

    // NULL = non renseigné → sera écrit à la main sur le formulaire imprimé
    public DateTime? DateDemande { get; set; }

    public bool EstRecu { get; set; } = false;
    public string ReceptionNomPrenom { get; set; } = "";
    public string ReceptionQualite { get; set; } = "";
    public DateTime? DateReception { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string TrancheLabel => NumeroTranche == 1 ? "1ÈRE" : "2ÈME";
    public decimal MontantCalcule => NumeroTranche == 1
        ? (Beneficiaire?.Montant ?? 700000) * 0.60m
        : (Beneficiaire?.Montant ?? 700000) * 0.40m;
}
