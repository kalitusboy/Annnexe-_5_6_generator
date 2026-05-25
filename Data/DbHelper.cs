using Dapper;
using RuralHousingApp.Models;
using System.Data.SQLite;

namespace RuralHousingApp.Data
{
    public static class DbHelper
    {
        private const string DbPath = "habitat_rural.db";

        public static void Initialize()
        {
            using var conn = new SQLiteConnection($"Data Source={DbPath}");
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS Beneficiaries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CodeBeneficiaire TEXT, NomBeneficiaire TEXT, Fraction TEXT,
                    Wilaya TEXT, Daira TEXT, Commune TEXT,
                    DecisionNumero TEXT, DecisionDate TEXT, Montant REAL,
                    IsFirstTranche BOOLEAN, Banque TEXT, CompteNumero TEXT,
                    PermisNumero TEXT, PermisDate TEXT,
                    AvancementPlateforme TEXT, AvancementPoteaux TEXT,
                    Observations TEXT, NomResponsable TEXT, QualiteResponsable TEXT,
                    DateCreation TEXT
                )");
        }

        public static List<Beneficiary> GetAll(string filter = "")
        {
            using var conn = new SQLiteConnection($"Data Source={DbPath}");
            var sql = string.IsNullOrWhiteSpace(filter)
                ? "SELECT * FROM Beneficiaries ORDER BY Id DESC"
                : "SELECT * FROM Beneficiaries WHERE NomBeneficiaire LIKE @f OR CodeBeneficiaire LIKE @f ORDER BY Id DESC";
            return conn.Query<Beneficiary>(sql, new { f = $"%{filter}%" }).ToList();
        }

        public static void Save(Beneficiary b)
        {
            using var conn = new SQLiteConnection($"Data Source={DbPath}");
            if (b.Id == 0)
                conn.Execute(@"INSERT INTO Beneficiaries (CodeBeneficiaire, NomBeneficiaire, Fraction, Wilaya, Daira, Commune,
                                DecisionNumero, DecisionDate, Montant, IsFirstTranche, Banque, CompteNumero,
                                PermisNumero, PermisDate, AvancementPlateforme, AvancementPoteaux, Observations,
                                NomResponsable, QualiteResponsable, DateCreation)
                              VALUES (@CodeBeneficiaire, @NomBeneficiaire, @Fraction, @Wilaya, @Daira, @Commune,
                                      @DecisionNumero, @DecisionDate, @Montant, @IsFirstTranche, @Banque, @CompteNumero,
                                      @PermisNumero, @PermisDate, @AvancementPlateforme, @AvancementPoteaux, @Observations,
                                      @NomResponsable, @QualiteResponsable, @DateCreation)", b);
            else
                conn.Execute(@"UPDATE Beneficiaries SET CodeBeneficiaire=@CodeBeneficiaire, NomBeneficiaire=@NomBeneficiaire,
                                Fraction=@Fraction, Wilaya=@Wilaya, Daira=@Daira, Commune=@Commune,
                                DecisionNumero=@DecisionNumero, DecisionDate=@DecisionDate, Montant=@Montant,
                                IsFirstTranche=@IsFirstTranche, Banque=@Banque, CompteNumero=@CompteNumero,
                                PermisNumero=@PermisNumero, PermisDate=@PermisDate,
                                AvancementPlateforme=@AvancementPlateforme, AvancementPoteaux=@AvancementPoteaux,
                                Observations=@Observations, NomResponsable=@NomResponsable, QualiteResponsable=@QualiteResponsable
                              WHERE Id=@Id", b);
        }

        public static void Delete(int id)
        {
            using var conn = new SQLiteConnection($"Data Source={DbPath}");
            conn.Execute("DELETE FROM Beneficiaries WHERE Id=@Id", new { id });
        }
    }
}