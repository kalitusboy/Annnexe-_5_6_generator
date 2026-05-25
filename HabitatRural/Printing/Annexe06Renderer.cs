using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HabitatRural.Models;
using static HabitatRural.Printing.FormHelper;

namespace HabitatRural.Printing;

/// <summary>
/// Renders ANNEXE N°06 - Procès Verbal de Constat d'Avancement des Travaux
/// All measurements in mm; layout matches the official A4 form.
/// </summary>
public static class Annexe06Renderer
{
    public static FixedDocument Render(ProcesVerbal pv)
    {
        var b = pv.Beneficiaire!;
        var canvas = CreateA4Canvas();
        Draw(canvas, pv, b);
        return WrapCanvasInFixedDocument(canvas);
    }

    private static void Draw(Canvas c, ProcesVerbal pv, Beneficiaire b)
    {
        double lm = MarginLeft;
        double cw = ContentWidth;
        double rm = lm + cw;
        double y = MarginTop;

        // ── Outer border ──────────────────────────────────────────────────
        DrawRect(c, lm, y, cw, 277, 0.3);
        y += 3;

        // ── Header ────────────────────────────────────────────────────────
        // Centered title block
        AddCenteredText(c, "ANNEXE N°06", lm, y, cw, 10, bold: true);
        y += 5;
        AddCenteredText(c, "PROCES VERBAL DE CONSTAT D'AVANCEMENT DES TRAVAUX",
            lm, y, cw, 9, bold: true);
        y += 4.5;
        AddCenteredText(c, "-HABITAT RURAL-", lm, y, cw, 9, bold: true);
        y += 6;

        DrawHLine(c, lm, y, cw, 0.5, bold: true);
        y += 2;

        // Institution header: two columns
        double col1W = 55;
        double col2W = cw - col1W;
        AddText(c, "DIRECTION  DE  ", lm + 2, y, 8);
        AddText(c, "LOGEMENT", lm + 30, y, 8.5, bold: true);
        AddCenteredText(c, "DE LA WILAYA DE", lm + col1W, y, 60, 8);
        AddText(c, "MEDEA", rm - 35, y, 9, bold: true);
        y += 4.5;

        AddText(c, "DAIRA DE  ", lm + 2, y, 8);
        AddText(c, "TABLAT", lm + 20, y, 9, bold: true);
        AddCenteredText(c, "COMMUNE DE :", lm + col1W, y, 50, 8);
        AddText(c, "DEUX BASSINS", lm + col1W + 52, y, 9, bold: true);
        y += 6;

        DrawHLine(c, lm, y, cw, 0.5, bold: true);
        y += 3;

        // JE SOUSSIGNE
        AddText(c, "JE SOUSSIGNE  ", lm + 2, y, 8.5);
        AddText(c, pv.NomSubdivisionnaire, lm + 27, y, 9, bold: true);
        DrawHLine(c, lm + 25, y + 4.5, cw - 27);
        AddText(c, "AGISSANT EN QUALITE DE SUBDIVISIONNAIRE DE", lm + 80, y, 8);
        y += 5.5;
        AddText(c, "LOGEMENT DE LA DAIRA DE TABLAT.", lm + 2, y, 8.5);
        y += 7;

        // ── 1 - CERTIFIE ──────────────────────────────────────────────────
        DrawHLine(c, lm + 1, y, cw - 2, 0.2);
        y += 1.5;
        AddText(c, "1-CERTIFIE AVOIR VISITE CE JOUR: ", lm + 2, y, 8.5, underline: true);
        string dv = pv.DateVisite.HasValue ? FormatDate(pv.DateVisite.Value) : "__________________";
        AddText(c, dv, lm + 70, y, 9, bold: true);
        DrawHLine(c, lm + 70, y + 4.5, 60);

        AddText(c, "  LE PROJET DE :NOUVELLE CONSTRUCTION", lm + 100, y, 8.5);
        y += 5.5;

        AddText(c, "D'UN LOGEMENT RURAL  SITUE : A LA  FRACTION : ", lm + 2, y, 8.5);
        AddText(c, pv.FractionProjet, lm + 96, y, 9, bold: true);
        DrawHLine(c, lm + 94, y + 4.5, cw - 96);
        y += 5.5;

        AddText(c, "CNE :", lm + 2, y, 8.5);
        AddText(c, pv.Commune, lm + 12, y, 9, bold: true);
        AddText(c, "ET  APPARTENANT A MR :", lm + 40, y, 8.5);
        AddText(c, b.NomPrenom, lm + 80, y, 9, bold: true);
        DrawHLine(c, lm + 78, y + 4.5, cw - 80);
        y += 5.5;

        AddText(c, "TITULAIRE DE LA DECISION N° :", lm + 2, y, 8.5);
        AddText(c, b.NumeroDecision, lm + 60, y, 9, bold: true);
        y += 5.5;

        AddText(c, "DU: ", lm + 2, y, 8.5);
        AddText(c, FormatDate(b.DateDecision), lm + 9, y, 9, bold: true); // date decision always set
        AddText(c, "RELATIVE A L'AIDE DE L'ETAT A L'HABITA RURAL", lm + 35, y, 8.5, bold: true);
        y += 5.5;

        AddText(c, "PERMIS DE CONSTRUIRE N° :", lm + 2, y, 8.5);
        AddText(c, pv.NumeroPermis, lm + 52, y, 9, bold: true);
        AddText(c, "DU:", lm + 75, y, 8.5);
        string dp = pv.DatePermis.HasValue ? FormatDate(pv.DatePermis.Value) : "___________";
        AddText(c, dp, lm + 81, y, 9, bold: true);
        AddText(c, "DELIVRE PAR LA COMMUNE  DEUX BASSINS.", lm + 105, y, 8);
        y += 7;

        // ── 2 - ATTESTE ────────────────────────────────────────────────────
        DrawHLine(c, lm + 1, y, cw - 2, 0.2);
        y += 2;
        AddText(c, "2-ATTESTE AVOIR CONSTATE:", lm + 2, y, 8.5, bold: true, underline: true);
        y += 5;

        // Table for rubriques
        y = DrawRubriquesTable(c, pv, lm, rm, cw, y);
        y += 5;

        // ── 3 - DECLARE ────────────────────────────────────────────────────
        DrawHLine(c, lm + 1, y, cw - 2, 0.2);
        y += 2;
        AddText(c, "3-DECLARE QUE LE BENEFICIAIRE OUVRE DROIT AU VERSEMENT DE :",
            lm + 2, y, 8.5, bold: true, underline: true);
        y += 5;

        // Tranche table
        double tW = cw / 2;
        DrawRect(c, lm, y, tW, 7, 0.3);
        DrawRect(c, lm + tW, y, tW, 7, 0.3);
        AddCenteredText(c, "1ERE TRANCHE", lm, y + 1.5, tW, 8.5, bold: true);
        AddCenteredText(c, "2EME TRANCHE", lm + tW, y + 1.5, tW, 8.5, bold: true);
        y += 7;
        DrawRect(c, lm, y, tW, 6, 0.3);
        DrawRect(c, lm + tW, y, tW, 6, 0.3);
        AddCenteredText(c, "60 % DE L'AIDE", lm, y + 1, tW, 8);
        AddCenteredText(c, "40 % DE L'AIDE", lm + tW, y + 1, tW, 8);
        y += 6;
        // Checkbox row
        DrawRect(c, lm, y, tW, 8, 0.3);
        DrawRect(c, lm + tW, y, tW, 8, 0.3);
        // Checkboxes centered in each cell
        DrawCheckbox(c, lm + tW / 2 - 2, y + 2, 4, pv.Tranche1);
        DrawCheckbox(c, lm + tW + tW / 2 - 2, y + 2, 4, pv.Tranche2);
        y += 10;

        AddText(c, "(*) mettre une croix dans la case correspondante.", lm + 2, y, 7.5, italic: true);
        y += 5;

        // ── Observations complémentaires ──────────────────────────────────
        AddText(c, "OBSERVATIONS COMPLEMENTAIRES:", lm + 2, y, 8.5, bold: true);
        y += 5;

        DrawHLine(c, lm + 2, y + 4.5, cw - 4);
        if (!string.IsNullOrEmpty(pv.ObservationsComplementaires))
            AddText(c, pv.ObservationsComplementaires, lm + 2, y, 8.5,
                widthMm: cw - 4, wrap: System.Windows.TextWrapping.Wrap);
        y += 8;
        DrawHLine(c, lm + 2, y + 4.5, cw - 4);
        y += 8;

        // FAIT À
        string dpv = pv.DatePV.HasValue ? FormatDate(pv.DatePV.Value) : "_______________";
        AddCenteredText(c, $"FAIT à {pv.Lieu}  LE : {dpv}",
            lm + cw / 2, y, cw / 2, 8.5);
        y += 8;

        // ── Signatures ────────────────────────────────────────────────────
        double sigColW = cw / 2;
        AddText(c, "NOM ET PRENOM : ", lm + 2, y, 8.5, bold: true);
        AddText(c, pv.NomSignataire, lm + 35, y, 9);
        DrawHLine(c, lm + 33, y + 4.5, sigColW - 34);
        y += 6;

        AddText(c, "QUALITE : Chargé du suivi  à la subdivision", lm + 2, y, 8.5);
        AddText(c, "Visa technique de la direction du", lm + sigColW + 2, y, 8);
        y += 5;
        AddText(c, "Signature :", lm + 2, y, 8.5);
        AddText(c, "Logement ou de l'APC.", lm + sigColW + 2, y, 8, bold: true);
        y += 12;

        DrawHLine(c, lm, y, cw, 0.3, bold: true);
        y += 3;

        // ── Pièce annexe ──────────────────────────────────────────────────
        AddText(c, "Pièce annexe :", lm + 2, y, 8, underline: true);
        y += 5;
        AddText(c, "- Copie du permis de construire pour la première tranche", lm + 4, y, 8);
    }

    private static double DrawRubriquesTable(Canvas c, ProcesVerbal pv,
        double lm, double rm, double cw, double y)
    {
        // Column widths (mm): Rubrique | En Chiffre | En Lettres | Observations
        double[] colW = { 65, 30, 55, 30 };
        double[] colX = { lm, lm + colW[0], lm + colW[0] + colW[1], lm + colW[0] + colW[1] + colW[2] };
        double rowH = 8.0;
        double headerH = 7.0;

        // Header row
        for (int i = 0; i < 4; i++)
            DrawRect(c, colX[i], y, colW[i], headerH, 0.3);

        AddCenteredText(c, "RUBRIQUE",       colX[0], y + 1.5, colW[0], 8.5, bold: true);
        AddCenteredText(c, "EN CHIFRE",      colX[1], y + 1.5, colW[1], 8.5, bold: true);
        AddCenteredText(c, "EN LETTRES",     colX[2], y + 1.5, colW[2], 8.5, bold: true);
        AddCenteredText(c, "OBSERVATIONS",   colX[3], y + 1.5, colW[3], 8.5, bold: true);
        y += headerH;

        // Data rows
        var rubriques = pv.Rubriques.OrderBy(r => r.Ordre).ToList();
        foreach (var rub in rubriques)
        {
            for (int i = 0; i < 4; i++)
                DrawRect(c, colX[i], y, colW[i], rowH, 0.3);

            AddText(c, rub.Rubrique,   colX[0] + 1, y + 1.5, 8.5, bold: true);
            AddCenteredText(c, rub.EnChiffre, colX[1], y + 1.5, colW[1], 9, bold: true);
            AddCenteredText(c, rub.EnLettres, colX[2], y + 1.5, colW[2], 9, bold: true);

            // Observation checkbox / X
            if (rub.HasObservation)
                AddCenteredText(c, "×", colX[3], y + 1, colW[3], 14, bold: true);
            else
                DrawRect(c, colX[3] + colW[3] / 2 - 3, y + 1.5, 6, 5, 0.3);

            y += rowH;
        }

        return y;
    }
}
