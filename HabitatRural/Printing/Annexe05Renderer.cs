using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HabitatRural.Models;
using static HabitatRural.Printing.FormHelper;

namespace HabitatRural.Printing;

/// <summary>
/// Renders ANNEXE N°05 - Demande de versement de l'aide à l'habitat rural
/// Layout matches the official A4 form exactly (all measurements in mm).
/// </summary>
public static class Annexe05Renderer
{
    public static FixedDocument Render(DemandeVersement d)
    {
        var b = d.Beneficiaire!;
        var canvas = CreateA4Canvas();
        Draw(canvas, d, b);
        return WrapCanvasInFixedDocument(canvas);
    }

    private static void Draw(Canvas c, DemandeVersement d, Beneficiaire b)
    {
        double lm = MarginLeft;          // left margin 15 mm
        double cw = ContentWidth;        // 180 mm
        double rm = lm + cw;             // right edge 195 mm
        double y = MarginTop;

        // ── Outer border ──────────────────────────────────────────────────
        DrawRect(c, lm, y, cw, 277, 0.3);

        // ── Header section ────────────────────────────────────────────────
        y += 3;

        // CODE BENEFICIAIRE box (top right)
        double codeBoxW = 38;
        double codeBoxX = rm - codeBoxW - 1;
        DrawRect(c, codeBoxX, y, codeBoxW, 18, 0.3);
        AddCenteredText(c, "CODE", codeBoxX, y + 1, codeBoxW, 7, bold: true);
        AddCenteredText(c, "BENEFICIAIRE", codeBoxX, y + 4.5, codeBoxW, 7, bold: true);
        // Value box inside
        DrawRect(c, codeBoxX + 1, y + 9, codeBoxW - 2, 7, 0.3);
        AddCenteredText(c, b.Code, codeBoxX + 1, y + 9.5, codeBoxW - 2, 9, bold: true);

        // Main title block (left of code box)
        double titleW = codeBoxX - lm - 3;
        AddCenteredText(c, "ANNEXE N° 05", lm, y + 0.5, titleW, 10, bold: true);
        AddCenteredText(c, "DEMANDE DE VERSEMENT DE L'AIDE A L'HABITAT RURAL",
            lm, y + 5, titleW, 9, bold: true);
        AddCenteredText(c, "- EMISE PAR LE BENEFICIAIRE -", lm, y + 9.5, titleW, 8);

        y += 21;
        DrawHLine(c, lm, y, cw, 0.3, bold: true);
        y += 3;

        // ── JE SOUSSIGNE ──────────────────────────────────────────────────
        AddText(c, "JE SOUSSIGNE : ", lm + 1, y, 9, bold: true);
        DrawField(c, lm + 36, y, cw - 37, b.NomPrenom, 9);
        y += 8;

        // ── ADRESSE ───────────────────────────────────────────────────────
        AddText(c, "ADRESSE / FRACTION : ", lm + 1, y, 8.5);
        DrawField(c, lm + 43, y, 80, b.Adresse + (b.Fraction.Length > 0 ? "  FRACTION : " + b.Fraction : ""), 8.5);
        AddText(c, "CNE : DEUX BASSINS", lm + 125, y, 8.5, bold: true);
        y += 9;

        // ── BENEFICIAIRE DE LA DECISION ───────────────────────────────────
        DrawHLine(c, lm + 1, y, cw - 2, 0.1);
        y += 2;

        string benefLine = "BENEFICIAIRE DE LA DECISION RELATIVE A L'AIDE DE L'ETAT A L'HABITAT RURAL";
        AddCenteredText(c, benefLine, lm, y, cw, 8.5, bold: true);
        y += 5.5;

        // N° DU: ... D'UN ... DA
        AddText(c, "N° DU : ", lm + 1, y, 8.5);
        AddText(c, b.NumeroDecision, lm + 16, y, 9, bold: true);
        AddText(c, ".", lm + 16 + 18, y, 9);
        AddText(c, "D'UN", lm + 38, y, 8.5);
        AddText(c, FormatAmount(b.Montant), lm + 48, y, 9, bold: true);
        DrawHLine(c, lm + 14, y + 4.5, 22);
        DrawHLine(c, lm + 42, y + 4.5, 60);
        // Date
        AddText(c, FormatDate(b.DateDecision), lm + 56, y, 8.5, bold: true);
        y += 6;

        AddText(c, "relative à  LA CONSTRUCTION D'UNE NOUVELLE HABITATION",
            lm + 1, y, 8.5, bold: false, italic: true);
        y += 6;

        AddText(c, "SISE A ", lm + 1, y, 8.5);
        AddText(c, b.LocalisationProjet, lm + 13, y, 9, bold: true);
        AddText(c, "FRACTION : ", lm + 80, y, 8.5);
        AddText(c, b.FractionProjet, lm + 96, y, 9, bold: true);
        DrawHLine(c, lm + 11, y + 4.5, 66);
        DrawHLine(c, lm + 94, y + 4.5, 40);
        y += 6;

        AddText(c, "COMMUNE : ", lm + 1, y, 8.5);
        AddText(c, b.Commune, lm + 20, y, 9, bold: true);
        y += 6;

        DrawHLine(c, lm + 1, y, cw - 2, 0.1);
        y += 4;

        // ── DEMANDE LE PAIEMENT ───────────────────────────────────────────
        AddText(c, "DEMANDE LE PAIEMENT DE LA", lm + 1, y, 9, bold: true);

        // Checkbox 1ERE
        double cbX = lm + 65;
        DrawCheckbox(c, cbX, y - 0.5, 4, d.NumeroTranche == 1);
        AddText(c, "1ERE", cbX + 5, y, 8.5, bold: d.NumeroTranche == 1);

        // Checkbox 2EME
        cbX = lm + 85;
        DrawCheckbox(c, cbX, y - 0.5, 4, d.NumeroTranche == 2);
        AddText(c, "2EME", cbX + 5, y, 8.5, bold: d.NumeroTranche == 2);

        AddText(c, "TRANCHE", lm + 105, y, 9, bold: true);
        y += 8;

        // DONT LE MONTANT EST DE
        AddText(c, "DONT LE MONTANT EST DE : ", lm + 1, y, 8.5);
        double montantX = lm + 52;
        string montantStr = d.MontantTranche > 0
            ? FormatAmount(d.MontantTranche)
            : FormatAmount(d.MontantCalcule);
        AddText(c, montantStr, montantX, y, 9, bold: true);
        DrawHLine(c, montantX - 1, y + 4.5, 80);
        // EN CHIFFRES label
        AddText(c, "(EN CHIFFRES)", rm - 32, y, 7, italic: true);
        y += 6;

        // EN LETTRES
        if (!string.IsNullOrEmpty(d.MontantEnLettres))
        {
            AddText(c, d.MontantEnLettres, lm + 1, y, 8.5, bold: true,
                widthMm: cw - 2, wrap: System.Windows.TextWrapping.Wrap);
            DrawHLine(c, lm + 1, y + 4.5, cw - 2);
            y += 6;
        }
        else
        {
            DrawHLine(c, lm + 1, y + 4.5, cw - 2);
            y += 6;
        }

        // A VERSER A MON COMPTE
        AddText(c, "A VERSER A MON COMPTE N˚", lm + 1, y, 8.5);
        DrawField(c, lm + 52, y, cw - 53, d.NumeroCompte, 9);
        y += 8;

        // BANQUE
        AddText(c, "BANQUE / AGENCE : ", lm + 1, y, 8.5);
        DrawField(c, lm + 36, y, 60, d.BanqueAgence, 9);
        AddText(c, "(ou CCP)", lm + 100, y, 7, italic: true);
        // BADR box
        DrawRect(c, lm + 115, y - 1, 30, 7, 0.3);
        AddCenteredText(c, "BADR", lm + 115, y, 30, 9, bold: true);
        y += 11;

        DrawHLine(c, lm, y, cw, 0.3, bold: true);
        y += 3;

        // ── Pièces jointes ────────────────────────────────────────────────
        AddText(c, "Pièces jointes (obligatoires)", lm + 2, y, 8.5, bold: true, underline: true);
        y += 5;

        AddText(c, "Pour la première tranche :", lm + 4, y, 8);
        y += 4.5;
        AddText(c, "-  Procès verbal de constat d'avancement des travaux", lm + 8, y, 8);
        y += 4.5;
        AddText(c, "-  Copie du permis de construire", lm + 8, y, 8);
        y += 5.5;

        AddText(c, "Pour la deuxième tranche :", lm + 4, y, 8);
        y += 4.5;
        AddText(c, "-  Procès verbal de constat d'avancement des travaux", lm + 8, y, 8);
        y += 6;

        // (*) Note
        DrawHLine(c, lm + 1, y, cw - 2, 0.1);
        y += 2;
        AddText(c, "(*)Biffer la case correspondante", lm + 2, y, 7.5, italic: true);
        y += 5;

        // Observation paragraph
        string obs = "Observation : la présente demande accompagnée des pièces nécessaires au paiement est déposée " +
                     "auprès de la direction du logement de la wilaya, qui se chargera de la transmettre à la CNL pour exécution.";
        AddText(c, obs, lm + 2, y, 7.5, widthMm: cw - 4,
            wrap: System.Windows.TextWrapping.Wrap);
        y += 13;

        DrawHLine(c, lm + 1, y, cw - 2, 0.1);
        y += 4;

        // ── Signature ─────────────────────────────────────────────────────
        string dateTxt05 = d.DateDemande.HasValue ? FormatDate(d.DateDemande.Value) : "_________________";
        AddText(c, $"FAIT à {d.Lieu}  LE : {dateTxt05}", lm + 2, y, 8.5);
        y += 8;
        AddCenteredText(c, "(SIGNATURE LEGALISEE DU BENEFICIAIRE)", lm + 1, y, cw - 2, 7.5, bold: false);
        y += 20;

        DrawHLine(c, lm + 1, y, cw - 2, 0.3, bold: true);
        y += 3;

        // ── Réception CNL ──────────────────────────────────────────────────
        AddText(c, "REÇUE PAR LA C.N.L", lm + 2, y, 8.5, bold: true);
        y += 6;
        AddText(c, "(NOM LISIBLE ET QUALITE DU SIGNATURE)", lm + 2, y, 7.5);
        DrawHLine(c, lm + 1, y + 4.5, cw - 2);

        if (d.EstRecu)
        {
            AddText(c, d.ReceptionNomPrenom, lm + 2, y, 8.5, bold: true);
            if (!string.IsNullOrEmpty(d.ReceptionQualite))
                AddText(c, d.ReceptionQualite, lm + 2, y + 5.5, 8);
        }
    }
}
