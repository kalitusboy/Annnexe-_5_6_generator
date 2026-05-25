namespace RuralHousingApp.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public string CodeBeneficiaire { get; set; } = "";
        public string NomBeneficiaire { get; set; } = "";
        public string Fraction { get; set; } = "";
        public string Wilaya { get; set; } = "MEDEA";
        public string Daira { get; set; } = "TABLAT";
        public string Commune { get; set; } = "DEUX BASSINS";
        public string DecisionNumero { get; set; } = "";
        public DateTime DecisionDate { get; set; } = DateTime.Now;
        public decimal Montant { get; set; } = 700000m;
        public bool IsFirstTranche { get; set; } = true;
        public string Banque { get; set; } = "BADR";
        public string CompteNumero { get; set; } = "";
        public string PermisNumero { get; set; } = "";
        public DateTime PermisDate { get; set; } = DateTime.Now;
        public string AvancementPlateforme { get; set; } = "100%";
        public string AvancementPoteaux { get; set; } = "100%";
        public string Observations { get; set; } = "";
        public string NomResponsable { get; set; } = "HAMZI IMENE";
        public string QualiteResponsable { get; set; } = "Charge du suivi";
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}