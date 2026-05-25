namespace HabitatRural.Models;

public class ProcesVerbal
{
    public int Id { get; set; }
    public int BeneficiaireId { get; set; }
    public Beneficiaire? Beneficiaire { get; set; }

    public string NomSubdivisionnaire { get; set; } = "";
    public string QualiteSubdivisionnaire { get; set; } = "Chargé du suivi à la subdivision";
    public string DirectionLogement { get; set; } = "MEDEA";
    public string Daira { get; set; } = "TABLAT";
    public string Commune { get; set; } = "DEUX BASSINS";

    // NULL = laissé en blanc pour écriture manuelle
    public DateTime? DateVisite { get; set; }

    public string FractionProjet { get; set; } = "";
    public string NumeroPermis { get; set; } = "";
    public DateTime? DatePermis { get; set; }

    public List<RubriqueConstat> Rubriques { get; set; } = new()
    {
        new RubriqueConstat { Rubrique = "ACHEVEMENT LA PLATE FORME", Ordre = 1 },
        new RubriqueConstat { Rubrique = "ACHEVEMENT DES POTEAUX",    Ordre = 2 },
    };

    public bool Tranche1 { get; set; } = false;
    public bool Tranche2 { get; set; } = false;

    public string ObservationsComplementaires { get; set; } = "";
    public string NomSignataire { get; set; } = "";
    public string Lieu { get; set; } = "DEUX BASSINS";
    public DateTime? DatePV { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class RubriqueConstat
{
    public int Id { get; set; }
    public int ProcesVerbalId { get; set; }
    public string Rubrique { get; set; } = "";
    public string EnChiffre { get; set; } = "";
    public string EnLettres { get; set; } = "";
    public bool HasObservation { get; set; } = false;
    public int Ordre { get; set; } = 0;
}
