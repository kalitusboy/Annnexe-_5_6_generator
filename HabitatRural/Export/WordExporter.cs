using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HabitatRural.Models;
using HabitatRural.Utils;

namespace HabitatRural.Export;

/// <summary>Exports Annexe 05 and 06 as editable .docx Word files.</summary>
public static class WordExporter
{
    // ── Annexe 05 ─────────────────────────────────────────────────────────
    public static string ExportAnnexe05(DemandeVersement d, string outputFolder)
    {
        var b = d.Beneficiaire!;
        string dateTxt = d.DateDemande.HasValue
            ? d.DateDemande.Value.ToString("dd/MM/yyyy") : "_______________";
        string montant = d.MontantTranche > 0 ? d.MontantTranche : d.MontantCalcule;
        string montantLettres = string.IsNullOrEmpty(d.MontantEnLettres)
            ? NumberToWordsFr.Convert(montant) : d.MontantEnLettres;

        string fileName = $"Annexe05_{b.Code}_{b.NomPrenom}_T{d.NumeroTranche}.docx"
            .Replace(" ", "_").Replace("/", "-");
        string path = Path.Combine(outputFolder, fileName);

        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Page setup A4
        body.AppendChild(new SectionProperties(
            new PageSize { Width = 11906, Height = 16838 }, // A4 in twentieths of a point
            new PageMargin { Top = 720, Bottom = 720, Left = 1080, Right = 720 }));

        // ── Title ──
        body.AppendChild(Para("ANNEXE N° 05", bold: true, center: true, size: 24));
        body.AppendChild(Para("DEMANDE DE VERSEMENT DE L'AIDE A L'HABITAT RURAL",
            bold: true, center: true, size: 22));
        body.AppendChild(Para("- EMISE PAR LE BENEFICIAIRE -", center: true, size: 20));
        body.AppendChild(HRule());

        // ── Code bénéficiaire ──
        body.AppendChild(ParaRight($"CODE BENEFICIAIRE : {b.Code}", bold: true));

        // ── Identité ──
        body.AppendChild(Para($"JE SOUSSIGNE : {b.NomPrenom}", bold: true));
        body.AppendChild(Para($"ADRESSE / FRACTION : {b.Adresse}  {b.Fraction}     CNE : {b.Commune}"));

        body.AppendChild(HRule());
        body.AppendChild(Para("BENEFICIAIRE DE LA DECISION RELATIVE A L'AIDE DE L'ETAT A L'HABITAT RURAL",
            bold: true, center: true));
        body.AppendChild(Para(
            $"N° DU : {b.NumeroDecision}  .  " +
            $"D'UN  {b.Montant:N2} DA   " +
            $"DATE : {b.DateDecision:dd/MM/yyyy}"));
        body.AppendChild(Para("relative à  LA CONSTRUCTION D'UNE NOUVELLE HABITATION", italic: true));
        body.AppendChild(Para(
            $"SISE A : {b.LocalisationProjet}    FRACTION : {b.FractionProjet}    COMMUNE : {b.Commune}"));

        body.AppendChild(HRule());

        // ── Tranche ──
        string t1 = d.NumeroTranche == 1 ? "☒" : "☐";
        string t2 = d.NumeroTranche == 2 ? "☒" : "☐";
        body.AppendChild(Para(
            $"DEMANDE LE PAIEMENT DE LA  {t1} 1ERE      {t2} 2EME   TRANCHE", bold: true));
        body.AppendChild(Para($"DONT LE MONTANT EST DE : {montant:N2} DA", bold: true));
        body.AppendChild(Para($"                          {montantLettres}"));
        body.AppendChild(Para($"A VERSER A MON COMPTE N° : {d.NumeroCompte}"));
        body.AppendChild(Para($"BANQUE / AGENCE : {d.BanqueAgence}"));

        body.AppendChild(HRule());

        // ── Pièces jointes ──
        body.AppendChild(Para("Pièces jointes (obligatoires) :", bold: true));
        body.AppendChild(Para("Pour la première tranche :"));
        body.AppendChild(Para("    -  Procès verbal de constat d'avancement des travaux"));
        body.AppendChild(Para("    -  Copie du permis de construire"));
        body.AppendChild(Para("Pour la deuxième tranche :"));
        body.AppendChild(Para("    -  Procès verbal de constat d'avancement des travaux"));

        body.AppendChild(HRule());
        body.AppendChild(Para("(*) Biffer la case correspondante", italic: true, size: 18));
        body.AppendChild(Para(
            "Observation : la présente demande accompagnée des pièces nécessaires au paiement est " +
            "déposée auprès de la direction du logement de la wilaya, qui se chargera de la " +
            "transmettre à la CNL pour exécution.", size: 18));

        body.AppendChild(HRule());
        body.AppendChild(Para($"FAIT à {d.Lieu}  LE : {dateTxt}"));
        body.AppendChild(Para(""));
        body.AppendChild(Para("(SIGNATURE LEGALISEE DU BENEFICIAIRE)", center: true, italic: true));

        body.AppendChild(HRule());
        body.AppendChild(Para("REÇUE PAR LA C.N.L", bold: true));
        if (d.EstRecu)
        {
            body.AppendChild(Para($"NOM : {d.ReceptionNomPrenom}    QUALITE : {d.ReceptionQualite}"));
        }
        else
        {
            body.AppendChild(Para("NOM ET QUALITE : ___________________________________"));
        }

        mainPart.Document.Save();
        return path;
    }

    // ── Annexe 06 ─────────────────────────────────────────────────────────
    public static string ExportAnnexe06(ProcesVerbal pv, string outputFolder)
    {
        var b = pv.Beneficiaire!;
        string dateVisite = pv.DateVisite.HasValue
            ? pv.DateVisite.Value.ToString("dd/MM/yyyy") : "_______________";
        string datePermis = pv.DatePermis.HasValue
            ? pv.DatePermis.Value.ToString("dd/MM/yyyy") : "_______________";
        string datePV = pv.DatePV.HasValue
            ? pv.DatePV.Value.ToString("dd/MM/yyyy") : "_______________";

        string fileName = $"Annexe06_{b.Code}_{b.NomPrenom}.docx"
            .Replace(" ", "_").Replace("/", "-");
        string path = Path.Combine(outputFolder, fileName);

        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        body.AppendChild(new SectionProperties(
            new PageSize { Width = 11906, Height = 16838 },
            new PageMargin { Top = 720, Bottom = 720, Left = 1080, Right = 720 }));

        // ── Header ──
        body.AppendChild(Para("ANNEXE N°06", bold: true, center: true, size: 24));
        body.AppendChild(Para("PROCES VERBAL DE CONSTAT D'AVANCEMENT DES TRAVAUX",
            bold: true, center: true, size: 22));
        body.AppendChild(Para("-HABITAT RURAL-", bold: true, center: true, size: 22));
        body.AppendChild(HRule());

        body.AppendChild(Para(
            $"DIRECTION DE LOGEMENT  DE LA WILAYA DE {pv.DirectionLogement}     " +
            $"DAIRA DE {pv.Daira}     COMMUNE DE : {pv.Commune}"));
        body.AppendChild(HRule());

        // ── 1 - Certifie ──
        body.AppendChild(Para($"JE SOUSSIGNE  {pv.NomSubdivisionnaire}   " +
            $"AGISSANT EN QUALITE DE SUBDIVISIONNAIRE DE LOGEMENT DE LA DAIRA DE {pv.Daira}."));

        body.AppendChild(Para(
            $"1-CERTIFIE AVOIR VISITE CE JOUR : {dateVisite}   " +
            "LE PROJET DE : NOUVELLE CONSTRUCTION D'UN LOGEMENT RURAL",
            bold: true, underline: true));
        body.AppendChild(Para($"SITUE : A LA FRACTION :  {pv.FractionProjet}     CNE : {pv.Commune}"));
        body.AppendChild(Para($"APPARTENANT A MR : {b.NomPrenom}"));
        body.AppendChild(Para($"TITULAIRE DE LA DECISION N° : {b.NumeroDecision}     " +
            $"DU : {b.DateDecision:dd/MM/yyyy}     RELATIVE A L'AIDE DE L'ETAT A L'HABITA RURAL"));
        body.AppendChild(Para($"PERMIS DE CONSTRUIRE N° : {pv.NumeroPermis}     " +
            $"DU : {datePermis}     DELIVRE PAR LA COMMUNE {pv.Commune}."));

        body.AppendChild(HRule());

        // ── 2 - Table rubriques ──
        body.AppendChild(Para("2-ATTESTE AVOIR CONSTATE :", bold: true, underline: true));
        body.AppendChild(BuildRubriquesTable(pv.Rubriques, mainPart));

        body.AppendChild(HRule());

        // ── 3 - Tranche ──
        string t1 = pv.Tranche1 ? "☒" : "☐";
        string t2 = pv.Tranche2 ? "☒" : "☐";
        body.AppendChild(Para(
            "3-DECLARE QUE LE BENEFICIAIRE OUVRE DROIT AU VERSEMENT DE :",
            bold: true, underline: true));
        body.AppendChild(Para(
            $"  {t1} 1ERE TRANCHE – 60% DE L'AIDE          {t2} 2EME TRANCHE – 40% DE L'AIDE",
            bold: true));

        body.AppendChild(HRule());
        body.AppendChild(Para("OBSERVATIONS COMPLEMENTAIRES :", bold: true));
        body.AppendChild(Para(string.IsNullOrEmpty(pv.ObservationsComplementaires)
            ? "___________________________________________________________________________"
            : pv.ObservationsComplementaires));

        body.AppendChild(HRule());
        body.AppendChild(Para($"FAIT à {pv.Lieu}  LE : {datePV}", center: true));
        body.AppendChild(Para(""));
        body.AppendChild(Para($"NOM ET PRENOM : {pv.NomSignataire}", bold: true));
        body.AppendChild(Para($"QUALITE : {pv.QualiteSubdivisionnaire}"));
        body.AppendChild(Para("Signature :"));
        body.AppendChild(Para(""));
        body.AppendChild(Para("Pièce annexe :", underline: true));
        body.AppendChild(Para("- Copie du permis de construire pour la première tranche"));

        mainPart.Document.Save();
        return path;
    }

    // ── Helper: rubriques table ───────────────────────────────────────────
    private static Table BuildRubriquesTable(List<RubriqueConstat> rubriques,
        MainDocumentPart mainPart)
    {
        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder    { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder   { Val = BorderValues.Single, Size = 4 },
                new RightBorder  { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder   { Val = BorderValues.Single, Size = 4 }),
            new TableWidth { Width = "9000", Type = TableWidthUnitValues.Dxa }));

        // Header row
        var headerRow = new TableRow();
        foreach (var hdr in new[] { "RUBRIQUE", "EN CHIFFRE", "EN LETTRES", "OBSERVATIONS" })
        {
            headerRow.AppendChild(Cell(hdr, bold: true, width: hdr == "RUBRIQUE" ? 3600
                : hdr == "EN CHIFFRE" ? 1500 : hdr == "EN LETTRES" ? 2700 : 1200));
        }
        table.AppendChild(headerRow);

        // Data rows
        foreach (var rub in rubriques.OrderBy(r => r.Ordre))
        {
            var row = new TableRow();
            row.AppendChild(Cell(rub.Rubrique, width: 3600));
            row.AppendChild(Cell(rub.EnChiffre, center: true, bold: true, width: 1500));
            row.AppendChild(Cell(rub.EnLettres, center: true, bold: true, width: 2700));
            row.AppendChild(Cell(rub.HasObservation ? "×" : "", center: true, width: 1200));
            table.AppendChild(row);
        }
        return table;
    }

    // ── OpenXml helpers ───────────────────────────────────────────────────
    private static Paragraph Para(string text,
        bool bold = false, bool italic = false, bool underline = false,
        bool center = false, int size = 20)
    {
        var run = new Run();
        var rpr = new RunProperties();
        if (bold)      rpr.AppendChild(new Bold());
        if (italic)    rpr.AppendChild(new Italic());
        if (underline) rpr.AppendChild(new Underline { Val = UnderlineValues.Single });
        rpr.AppendChild(new FontSize { Val = size.ToString() });
        run.AppendChild(rpr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        var ppr = new ParagraphProperties();
        if (center) ppr.AppendChild(new Justification { Val = JustificationValues.Center });

        var p = new Paragraph();
        p.AppendChild(ppr);
        p.AppendChild(run);
        return p;
    }

    private static Paragraph ParaRight(string text, bool bold = false)
    {
        var run = new Run();
        var rpr = new RunProperties();
        if (bold) rpr.AppendChild(new Bold());
        rpr.AppendChild(new FontSize { Val = "20" });
        run.AppendChild(rpr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        var ppr = new ParagraphProperties();
        ppr.AppendChild(new Justification { Val = JustificationValues.Right });

        var p = new Paragraph();
        p.AppendChild(ppr);
        p.AppendChild(run);
        return p;
    }

    private static Paragraph HRule()
    {
        var p = new Paragraph();
        var ppr = new ParagraphProperties();
        ppr.AppendChild(new ParagraphBorders(
            new BottomBorder { Val = BorderValues.Single, Size = 4 }));
        p.AppendChild(ppr);
        return p;
    }

    private static TableCell Cell(string text,
        bool bold = false, bool center = false, int width = 2000)
    {
        var run = new Run();
        var rpr = new RunProperties();
        if (bold) rpr.AppendChild(new Bold());
        rpr.AppendChild(new FontSize { Val = "18" });
        run.AppendChild(rpr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        var ppr = new ParagraphProperties();
        if (center) ppr.AppendChild(new Justification { Val = JustificationValues.Center });

        var para = new Paragraph();
        para.AppendChild(ppr);
        para.AppendChild(run);

        var cell = new TableCell();
        cell.AppendChild(new TableCellProperties(
            new TableCellWidth { Width = width.ToString(), Type = TableWidthUnitValues.Dxa }));
        cell.AppendChild(para);
        return cell;
    }

    // Convenience: decimal → string format
    private static string FormatAmt(decimal d) =>
        d.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " DA";
}

// Extension for body to accept Table
file static class BodyExtensions
{
    public static Table AppendChild(this Body body, Table table)
    {
        body.Append(table);
        return table;
    }
}
