namespace HabitatRural.Models;

public class Ingenieur
{
    public int    Id      { get; set; }
    public string Nom     { get; set; } = "";
    public string Prenom  { get; set; } = "";
    public string Qualite { get; set; } = "Chargé du suivi à la subdivision";
    public bool   Actif   { get; set; } = true;

    public string NomComplet => $"{Nom} {Prenom}".Trim().ToUpper();
    public override string ToString() => NomComplet;
}
