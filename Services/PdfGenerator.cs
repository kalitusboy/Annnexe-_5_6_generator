using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using RuralHousingApp.Models;
using System;
using System.IO;

namespace RuralHousingApp.Services
{
    public static class PdfGenerator
    {
        private const float MM_TO_PT = 2.8346f;
        private static readonly PdfFont DefaultFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        private static readonly PdfFont BoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        public static void GenerateAnnexe05(Beneficiary b, string outputPath)
        {
            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(12 * MM_TO_PT, 12 * MM_TO_PT, 12 * MM_TO_PT, 12 * MM_TO_PT);
            doc.SetFontSize(9.5f);
            doc.SetFont(DefaultFont);

            // العنوان
            doc.Add(new Paragraph("ANNEXE N° 05").SetFontSize(11f).SetFont(BoldFont).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(2));
            doc.Add(new Paragraph("DEMANDE DE VERSEMENT DE L'AIDE A L'HABITAT RURAL - EMISE PAR LE BENEFICIAIRE -")
                .SetFontSize(9.5f).SetFont(BoldFont).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(8));

            // جدول الكود (16 خانة)
            var codeTable = new Table(16).UseAllAvailableWidth();
            var code = b.CodeBeneficiaire.PadRight(16, ' ').Substring(0, 16);
            for (int i = 0; i < 16; i++)
                codeTable.AddCell(new Cell().Add(new Paragraph(code[i].ToString())).SetTextAlignment(TextAlignment.Center).SetPadding(1).SetBorder(new iText.Layout.Borders.SolidBorder(0.5f)));
            doc.Add(new Paragraph("CODE BENEFICIAIRE").SetFont(BoldFont).SetMarginBottom(2));
            doc.Add(codeTable.SetMarginBottom(8));

            // البيانات الأساسية
            doc.Add(new Paragraph("JE SOUSSIGNE:").SetFont(BoldFont).SetMarginBottom(1));
            doc.Add(new Paragraph(b.NomBeneficiaire).SetFontSize(10f).SetMarginBottom(4));
            doc.Add(new Paragraph($"ADRESSE/ FRACTION: CNE: {b.Commune}").SetFont(BoldFont).SetMarginBottom(1));
            doc.Add(new Paragraph(b.Fraction).SetFontSize(10f).SetMarginBottom(6));

            // نص القرار
            doc.Add(new Paragraph("BENEFICIAIRE DE LA DECISION RELATIVE A L'AIDE DE L'ETAT A L'HABITAT RURAL").SetFont(BoldFont).SetMarginBottom(1));
            doc.Add(new Paragraph($"N° DU: {b.DecisionNumero}    D'UN: {b.Montant:N2} DA    relative à LA CONSTRUCTION D'UNE NOUVELLE HABITATION").SetMarginBottom(1));
            doc.Add(new Paragraph($"DATE: {b.DecisionDate:dd/MM/yyyy}").SetMarginBottom(6));
            doc.Add(new Paragraph($"SISE A localisation du projet FRACTION: {b.Fraction}    COMMUNE: {b.Commune}").SetMarginBottom(8));

            // خانة الترانش
            var reqTable = new Table(4).UseAllAvailableWidth();
            reqTable.AddHeaderCell("DEMANDE LE PAIEMENT DE LA");
            reqTable.AddCell(new Paragraph(b.IsFirstTranche ? "☑ 1ERE" : "☐ 1ERE").SetTextAlignment(TextAlignment.Center));
            reqTable.AddCell(new Paragraph(b.IsFirstTranche ? "☐ 2EME" : "☑ 2EME").SetTextAlignment(TextAlignment.Center));
            reqTable.AddCell(new Paragraph("TRANCHE").SetTextAlignment(TextAlignment.Center));
            doc.Add(reqTable.SetMarginBottom(4));

            // المبلغ
            var amtTable = new Table(2).UseAllAvailableWidth();
            amtTable.AddHeaderCell("DONT LE MONTANT EST DE:");
            amtTable.AddCell(new Paragraph($"(EN CHIFFRES)\n{b.Montant:N2}"));
            amtTable.AddCell(new Paragraph("(EN LETTRES)\n" + FrenchNumberConverter.ToFrenchWords(b.Montant)));
            doc.Add(amtTable.SetMarginBottom(8));

            // الحساب البنكي
            var bankTable = new Table(2).UseAllAvailableWidth();
            bankTable.AddHeaderCell("A VERSER A MON COMPTE N°");
            bankTable.AddCell(new Paragraph($"BANQUE/AGENCE ou ccp: {b.Banque}    N°: {b.CompteNumero}"));
            doc.Add(bankTable.SetMarginBottom(8));

            // مرفقات
            doc.Add(new Paragraph("Pièces jointes(obligatoires)").SetFont(BoldFont).SetFontSize(9f).SetMarginBottom(1));
            doc.Add(new Paragraph("Pour la première tranche: -Procès verbal de constat d'avancement des travaux -Copie du permis de construire\nPour la deuxième tranche: - Procès verbal de constat d'avancement des travaux").SetFontSize(8.5f).SetMarginBottom(10));

            // التوقيع والتاريخ
            doc.Add(new Paragraph($"FAIT à {b.Commune} LE: {DateTime.Now:dd/MM/yyyy}").SetTextAlignment(TextAlignment.RIGHT).SetMarginBottom(2));
            doc.Add(new Paragraph("(SIGNATURE LEGALISEE DU BENEFICIAIRE)").SetTextAlignment(TextAlignment.RIGHT).SetMarginBottom(6));
            doc.Add(new Paragraph("REÇUE PAR LA C.N.L (NOM LISIBLE ET QUALITE DU SIGNATURE)").SetFontSize(8.5f));

            doc.Close();
        }

        public static void GenerateAnnexe06(Beneficiary b, string outputPath)
        {
            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.SetMargins(12 * MM_TO_PT, 12 * MM_TO_PT, 12 * MM_TO_PT, 12 * MM_TO_PT);
            doc.SetFontSize(9.5f);
            doc.SetFont(DefaultFont);

            // العنوان
            doc.Add(new Paragraph("ANNEXE N°06").SetFontSize(11f).SetFont(BoldFont).SetTextAlignment(TextAlignment.CENTER));
            doc.Add(new Paragraph("PROCES VERBAL DE CONSTAT D'AVANCEMENT DES TRAVAUX - HABITAT RURAL -").SetFontSize(10f).SetFont(BoldFont).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(4));

            // الرأسية الإدارية            doc.Add(new Paragraph($"DIRECTION DE LOGEMENT DE LA WILAYA DE {b.Wilaya.ToUpper()}    DAIRA DE {b.Daira.ToUpper()}").SetFont(BoldFont).SetMarginBottom(1));
            doc.Add(new Paragraph($"JE SOUSSIGNE COMMUNE DE: {b.Commune.ToUpper()} AGISSANT EN QUALITE DE SUBDIVISIONNAIRE DE LOGEMENT DE LA DAIRA DE {b.Daira.ToUpper()}.")
                .SetMarginBottom(6));

            // القسم 1
            doc.Add(new Paragraph("1- CERTIFIE AVOIR VISITE CE JOUR:.........................").SetMarginBottom(1));
            doc.Add(new Paragraph("LE PROJET DE: NOUVELLE CONSTRUCTION D'UN LOGEMENT RURAL").SetMarginBottom(1));
            doc.Add(new Paragraph($"SITUE: A LA FRACTION: {b.Fraction}    APPARTENANT A MR: {b.NomBeneficiaire}").SetMarginBottom(1));
            doc.Add(new Paragraph($"CNE: {b.Commune}    ET TITULAIRE DE LA DECISION N°: {b.DecisionNumero} DU: {b.DecisionDate:dd/MM/yyyy}").SetMarginBottom(1));
            doc.Add(new Paragraph($"PERMIS DE CONSTRUIRE N°: {b.PermisNumero} DU: {b.PermisDate:dd/MM/yyyy}    DELIVRE PAR LA COMMUNE {b.Commune}.").SetMarginBottom(6));

            // القسم 2: الجدول
            doc.Add(new Paragraph("2- ATTESTE AVOIR CONSTATE:").SetFont(BoldFont).SetMarginBottom(3));
            var progTable = new Table(3).UseAllAvailableWidth();
            progTable.SetBorder(new iText.Layout.Borders.SolidBorder(0.5f));
            progTable.AddHeaderCell("RUBRIQUE");
            progTable.AddHeaderCell("ACHEVEMENT LA PLATE FORME");
            progTable.AddHeaderCell("ACHEVEMENT DES POTEAUX");

            progTable.AddCell(new Paragraph("EN CHIFRE").SetFontSize(8.5f));
            progTable.AddCell(new Paragraph(b.AvancementPlateforme).SetTextAlignment(TextAlignment.Center));
            progTable.AddCell(new Paragraph(b.AvancementPoteaux).SetTextAlignment(TextAlignment.Center));

            progTable.AddCell(new Paragraph("EN LETTRES").SetFontSize(8.5f));
            progTable.AddCell(new Paragraph("CENT POUR CENT").SetTextAlignment(TextAlignment.Center));
            progTable.AddCell(new Paragraph("CENT POUR CENT").SetTextAlignment(TextAlignment.Center));

            progTable.AddCell(new Paragraph("OBSERVATIONS").SetFontSize(8.5f));
            progTable.AddCell(new Paragraph(b.Observations).SetColspan(2).SetFontSize(8.5f));
            doc.Add(progTable.SetMarginBottom(8));

            // القسم 3: الاستحقاق
            doc.Add(new Paragraph("3- DECLARE QUE LE BENEFICIAIRE OUVRE DROIT AU VERSEMENT DE:").SetFont(BoldFont).SetMarginBottom(3));
            var trancheTable = new Table(2).UseAllAvailableWidth();
            trancheTable.AddHeaderCell(b.IsFirstTranche ? "☑ 1ERE TRANCHE\n60% DE L'AIDE" : "☐ 1ERE TRANCHE\n60% DE L'AIDE");
            trancheTable.AddHeaderCell(b.IsFirstTranche ? "☐ 2EME TRANCHE\n40% DE L'AIDE" : "☑ 2EME TRANCHE\n40% DE L'AIDE");
            doc.Add(trancheTable.SetMarginBottom(6));

            // ملاحظات وتوقيع
            doc.Add(new Paragraph("OBSERVATIONS COMPLEMENTAIRES:").SetFont(BoldFont).SetMarginBottom(2));
            doc.Add(new Paragraph(b.Observations).SetBorder(new iText.Layout.Borders.SolidBorder(0.5f)).SetPadding(4).SetHeight(35));

            doc.Add(new Paragraph($"FAIT à {b.Commune} LE: {DateTime.Now:dd/MM/yyyy}").SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(6));
            
            var sigTable = new Table(3).UseAllAvailableWidth();
            sigTable.AddHeaderCell("NOM ET PRENOM:").SetFontSize(8.5f);
            sigTable.AddHeaderCell("QUALITE:").SetFontSize(8.5f);
            sigTable.AddHeaderCell("SIGNATURE:").SetFontSize(8.5f);
            sigTable.AddCell(new Paragraph(b.NomResponsable).SetFontSize(9f));
            sigTable.AddCell(new Paragraph(b.QualiteResponsable).SetFontSize(9f));            sigTable.AddCell(new Paragraph(""));
            doc.Add(sigTable.SetMarginBottom(6));

            doc.Add(new Paragraph("Visa technique de la direction du Logement ou de l'APC:").SetFontSize(8.5f));
            doc.Add(new Paragraph("Pièce annexe: - Copie du permis de construire pour la première tranche").SetFontSize(8f));

            doc.Close();
        }
    }
}