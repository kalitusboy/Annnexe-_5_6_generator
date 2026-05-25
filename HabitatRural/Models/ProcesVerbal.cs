namespace HabitatRural.Models;

public class ProcesVerbal
{
    public int Id { get; set; }
    public int BeneficiaireId { get; set; }
    public Beneficiaire? Beneficiaire { get; set; }

    public string NomSubdivisionnaire { get; set; } = "";
    public string DirectionLogement { get; set; } = "MEDEA";
    public string Daira { get; set; } = "TABLAT";
    public string Commune { get; set; } = "DEUX BASSINS";

    public DateTime DateVisite { get; set; } = DateTime.Today;
    public string FractionProjet { get; set; } = "";

    public string NumeroPermis { get; set; } = "";
    public DateTime DatePermis { get; set; } = DateTime.Today;

    // Rubriques du constat
    public List<RubriqueConstat> Rubriques { get; set; } = new()
    {
        new RubriqueConstat { Rubrique = "ACHEVEMENT LA PLATE FORME", Ordre = 1 },
        new RubriqueConstat { Rubrique = "ACHEVEMENT DES POTEAUX", Ordre = 2 },
    };

    // Tranches autorisées (une ou les deux peuvent être cochées)
    public bool Tranche1 { get; set; } = false;
    public bool Tranche2 { get; set; } = false;

    public string ObservationsComplementaires { get; set; } = "";
    public string NomSignataire { get; set; } = "";
    public string Lieu { get; set; } = "DEUX BASSINS";
    public DateTime DatePV { get; set; } = DateTime.Today;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class RubriqueConstat
{
    public int Id { get; set; }
    public int ProcesVerbalId { get; set; }
    public string Rubrique { get; set; } = "";
    public string EnChiffre { get; set; } = "";   // ex: "100 %"
    public string EnLettres { get; set; } = "";   // ex: "CENT POUR CENT"
    public bool HasObservation { get; set; } = false;
    public int Ordre { get; set; } = 0;
}
