using System.IO;
using Microsoft.Data.Sqlite;
using HabitatRural.Models;

namespace HabitatRural.Data;

public class DatabaseService
{
    private static DatabaseService? _instance;
    public static DatabaseService Instance => _instance ??= new DatabaseService();

    private readonly string _dbPath;
    private readonly string _connStr;

    private DatabaseService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HabitatRural");
        Directory.CreateDirectory(folder);
        _dbPath = Path.Combine(folder, "habitat_rural.db");
        _connStr = $"Data Source={_dbPath}";
    }

    public string DbPath => _dbPath;

    // ─── Schema ──────────────────────────────────────────────────────────────
    public void Initialize()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            PRAGMA journal_mode=WAL;
            PRAGMA foreign_keys=ON;

            CREATE TABLE IF NOT EXISTS beneficiaires (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL DEFAULT '',
                nom_prenom TEXT NOT NULL DEFAULT '',
                adresse TEXT DEFAULT '',
                fraction TEXT DEFAULT '',
                numero_decision TEXT DEFAULT '',
                date_decision TEXT DEFAULT '',
                montant REAL DEFAULT 700000,
                localisation_projet TEXT DEFAULT '',
                fraction_projet TEXT DEFAULT '',
                commune TEXT DEFAULT 'DEUX BASSINS',
                numero_compte TEXT DEFAULT '',
                banque_agence TEXT DEFAULT 'BADR',
                created_at TEXT DEFAULT (datetime('now')),
                updated_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS demandes_versement (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                beneficiaire_id INTEGER NOT NULL REFERENCES beneficiaires(id) ON DELETE CASCADE,
                numero_tranche INTEGER DEFAULT 1,
                montant_tranche REAL DEFAULT 0,
                montant_en_lettres TEXT DEFAULT '',
                numero_compte TEXT DEFAULT '',
                banque_agence TEXT DEFAULT 'BADR',
                lieu TEXT DEFAULT 'DEUX BASSINS',
                date_demande TEXT DEFAULT '',
                est_recu INTEGER DEFAULT 0,
                reception_nom_prenom TEXT DEFAULT '',
                reception_qualite TEXT DEFAULT '',
                date_reception TEXT DEFAULT '',
                created_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS proces_verbaux (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                beneficiaire_id INTEGER NOT NULL REFERENCES beneficiaires(id) ON DELETE CASCADE,
                nom_subdivisionnaire TEXT DEFAULT '',
                direction_logement TEXT DEFAULT 'MEDEA',
                daira TEXT DEFAULT 'TABLAT',
                commune TEXT DEFAULT 'DEUX BASSINS',
                date_visite TEXT DEFAULT '',
                fraction_projet TEXT DEFAULT '',
                numero_permis TEXT DEFAULT '',
                date_permis TEXT DEFAULT '',
                tranche1 INTEGER DEFAULT 0,
                tranche2 INTEGER DEFAULT 0,
                observations_complementaires TEXT DEFAULT '',
                nom_signataire TEXT DEFAULT '',
                lieu TEXT DEFAULT 'DEUX BASSINS',
                date_pv TEXT DEFAULT '',
                created_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS rubriques_constat (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                proces_verbal_id INTEGER NOT NULL REFERENCES proces_verbaux(id) ON DELETE CASCADE,
                rubrique TEXT DEFAULT '',
                en_chiffre TEXT DEFAULT '',
                en_lettres TEXT DEFAULT '',
                has_observation INTEGER DEFAULT 0,
                ordre INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS ingenieurs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nom TEXT DEFAULT '',
                prenom TEXT DEFAULT '',
                qualite TEXT DEFAULT 'Chargé du suivi à la subdivision',
                actif INTEGER DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS parametres (
                cle TEXT PRIMARY KEY,
                valeur TEXT DEFAULT ''
            );

            INSERT OR IGNORE INTO parametres(cle,valeur) VALUES('commune','DEUX BASSINS');
            INSERT OR IGNORE INTO parametres(cle,valeur) VALUES('daira','TABLAT');
            INSERT OR IGNORE INTO parametres(cle,valeur) VALUES('wilaya','MEDEA');
        ";
        cmd.ExecuteNonQuery();
    }

    // ─── Beneficiaires ───────────────────────────────────────────────────────
    public List<Beneficiaire> GetAllBeneficiaires()
    {
        var list = new List<Beneficiaire>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM beneficiaires ORDER BY updated_at DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(ReadBeneficiaire(r));
        return list;
    }

    public Beneficiaire? GetBeneficiaire(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM beneficiaires WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? ReadBeneficiaire(r) : null;
    }

    public int SaveBeneficiaire(Beneficiaire b)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        if (b.Id == 0)
        {
            cmd.CommandText = @"
                INSERT INTO beneficiaires
                (code,nom_prenom,adresse,fraction,numero_decision,date_decision,
                 montant,localisation_projet,fraction_projet,commune,numero_compte,banque_agence)
                VALUES(@c,@n,@a,@fr,@nd,@dd,@m,@lp,@fp,@co,@nc,@ba);
                SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"
                UPDATE beneficiaires SET
                code=@c,nom_prenom=@n,adresse=@a,fraction=@fr,
                numero_decision=@nd,date_decision=@dd,montant=@m,
                localisation_projet=@lp,fraction_projet=@fp,commune=@co,
                numero_compte=@nc,banque_agence=@ba,
                updated_at=datetime('now')
                WHERE id=@id;
                SELECT @id;";
            cmd.Parameters.AddWithValue("@id", b.Id);
        }
        AddBeneficiaireParams(cmd, b);
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public void DeleteBeneficiaire(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM beneficiaires WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static void AddBeneficiaireParams(SqliteCommand cmd, Beneficiaire b)
    {
        cmd.Parameters.AddWithValue("@c", b.Code);
        cmd.Parameters.AddWithValue("@n", b.NomPrenom);
        cmd.Parameters.AddWithValue("@a", b.Adresse);
        cmd.Parameters.AddWithValue("@fr", b.Fraction);
        cmd.Parameters.AddWithValue("@nd", b.NumeroDecision);
        cmd.Parameters.AddWithValue("@dd", b.DateDecision.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@m", (double)b.Montant);
        cmd.Parameters.AddWithValue("@lp", b.LocalisationProjet);
        cmd.Parameters.AddWithValue("@fp", b.FractionProjet);
        cmd.Parameters.AddWithValue("@co", b.Commune);
        cmd.Parameters.AddWithValue("@nc", b.NumeroCompte);
        cmd.Parameters.AddWithValue("@ba", b.BanqueAgence);
    }

    private static Beneficiaire ReadBeneficiaire(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        Code = r["code"]?.ToString() ?? "",
        NomPrenom = r["nom_prenom"]?.ToString() ?? "",
        Adresse = r["adresse"]?.ToString() ?? "",
        Fraction = r["fraction"]?.ToString() ?? "",
        NumeroDecision = r["numero_decision"]?.ToString() ?? "",
        DateDecision = DateTime.TryParse(r["date_decision"]?.ToString(), out var dd) ? dd : DateTime.Today,
        Montant = Convert.ToDecimal(r["montant"]),
        LocalisationProjet = r["localisation_projet"]?.ToString() ?? "",
        FractionProjet = r["fraction_projet"]?.ToString() ?? "",
        Commune = r["commune"]?.ToString() ?? "DEUX BASSINS",
        NumeroCompte = r["numero_compte"]?.ToString() ?? "",
        BanqueAgence = r["banque_agence"]?.ToString() ?? "BADR",
        CreatedAt = DateTime.TryParse(r["created_at"]?.ToString(), out var ca) ? ca : DateTime.Now,
        UpdatedAt = DateTime.TryParse(r["updated_at"]?.ToString(), out var ua) ? ua : DateTime.Now,
    };

    // ─── DemandeVersement ────────────────────────────────────────────────────
    public List<DemandeVersement> GetDemandesForBeneficiaire(int beneficiaireId)
    {
        var list = new List<DemandeVersement>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM demandes_versement WHERE beneficiaire_id=@bid ORDER BY created_at DESC";
        cmd.Parameters.AddWithValue("@bid", beneficiaireId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(ReadDemande(r));
        return list;
    }

    public DemandeVersement? GetDemande(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM demandes_versement WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        var d = ReadDemande(r);
        d.Beneficiaire = GetBeneficiaire(d.BeneficiaireId);
        return d;
    }

    public int SaveDemande(DemandeVersement d)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        if (d.Id == 0)
        {
            cmd.CommandText = @"
                INSERT INTO demandes_versement
                (beneficiaire_id,numero_tranche,montant_tranche,montant_en_lettres,
                 numero_compte,banque_agence,lieu,date_demande,est_recu,
                 reception_nom_prenom,reception_qualite,date_reception)
                VALUES(@bid,@nt,@mt,@ml,@nc,@ba,@li,@dd,@er,@rn,@rq,@dr);
                SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"
                UPDATE demandes_versement SET
                numero_tranche=@nt,montant_tranche=@mt,montant_en_lettres=@ml,
                numero_compte=@nc,banque_agence=@ba,lieu=@li,date_demande=@dd,
                est_recu=@er,reception_nom_prenom=@rn,reception_qualite=@rq,date_reception=@dr
                WHERE id=@id;
                SELECT @id;";
            cmd.Parameters.AddWithValue("@id", d.Id);
        }
        cmd.Parameters.AddWithValue("@bid", d.BeneficiaireId);
        cmd.Parameters.AddWithValue("@nt", d.NumeroTranche);
        cmd.Parameters.AddWithValue("@mt", (double)d.MontantTranche);
        cmd.Parameters.AddWithValue("@ml", d.MontantEnLettres);
        cmd.Parameters.AddWithValue("@nc", d.NumeroCompte);
        cmd.Parameters.AddWithValue("@ba", d.BanqueAgence);
        cmd.Parameters.AddWithValue("@li", d.Lieu);
        // ✅ التصحيح: السماح بقيمة فارغة لتاريخ الطلب
        cmd.Parameters.AddWithValue("@dd", d.DateDemande?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("@er", d.EstRecu ? 1 : 0);
        cmd.Parameters.AddWithValue("@rn", d.ReceptionNomPrenom);
        cmd.Parameters.AddWithValue("@rq", d.ReceptionQualite);
        // ✅ التصحيح: السماح بقيمة فارغة لتاريخ الاستلام (بالفعل كان صحيحاً لكن نؤكد)
        cmd.Parameters.AddWithValue("@dr", d.DateReception?.ToString("yyyy-MM-dd") ?? "");
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void DeleteDemande(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM demandes_versement WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static DemandeVersement ReadDemande(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        BeneficiaireId = r.GetInt32(r.GetOrdinal("beneficiaire_id")),
        NumeroTranche = r.GetInt32(r.GetOrdinal("numero_tranche")),
        MontantTranche = Convert.ToDecimal(r["montant_tranche"]),
        MontantEnLettres = r["montant_en_lettres"]?.ToString() ?? "",
        NumeroCompte = r["numero_compte"]?.ToString() ?? "",
        BanqueAgence = r["banque_agence"]?.ToString() ?? "BADR",
        Lieu = r["lieu"]?.ToString() ?? "DEUX BASSINS",
        // ✅ التصحيح: إذا كانت القيمة فارغة في قاعدة البيانات، اجعل DateDemande = null
        DateDemande = r["date_demande"] == DBNull.Value ? null : DateTime.Parse(r["date_demande"].ToString()!),
        EstRecu = r.GetInt32(r.GetOrdinal("est_recu")) == 1,
        ReceptionNomPrenom = r["reception_nom_prenom"]?.ToString() ?? "",
        ReceptionQualite = r["reception_qualite"]?.ToString() ?? "",
        // ✅ التصحيح: السماح بتاريـخ استلام فارغ
        DateReception = r["date_reception"] == DBNull.Value ? null : DateTime.Parse(r["date_reception"].ToString()!),
        CreatedAt = DateTime.TryParse(r["created_at"]?.ToString(), out var ca) ? ca : DateTime.Now,
    };

    // ─── ProcesVerbal ────────────────────────────────────────────────────────
    public List<ProcesVerbal> GetPVsForBeneficiaire(int beneficiaireId)
    {
        var list = new List<ProcesVerbal>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM proces_verbaux WHERE beneficiaire_id=@bid ORDER BY created_at DESC";
        cmd.Parameters.AddWithValue("@bid", beneficiaireId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var pv = ReadPV(r);
            pv.Rubriques = GetRubriquesForPV(conn, pv.Id);
            list.Add(pv);
        }
        return list;
    }

    public ProcesVerbal? GetPV(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM proces_verbaux WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        var pv = ReadPV(r);
        pv.Beneficiaire = GetBeneficiaire(pv.BeneficiaireId);
        pv.Rubriques = GetRubriquesForPV(conn, pv.Id);
        return pv;
    }

    public int SavePV(ProcesVerbal pv)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = conn.CreateCommand();
            if (pv.Id == 0)
            {
                cmd.CommandText = @"
                    INSERT INTO proces_verbaux
                    (beneficiaire_id,nom_subdivisionnaire,direction_logement,daira,commune,
                     date_visite,fraction_projet,numero_permis,date_permis,
                     tranche1,tranche2,observations_complementaires,nom_signataire,lieu,date_pv)
                    VALUES(@bid,@ns,@dl,@da,@co,@dv,@fp,@np,@dp,@t1,@t2,@oc,@ns2,@li,@dpv);
                    SELECT last_insert_rowid();";
            }
            else
            {
                cmd.CommandText = @"
                    UPDATE proces_verbaux SET
                    nom_subdivisionnaire=@ns,direction_logement=@dl,daira=@da,commune=@co,
                    date_visite=@dv,fraction_projet=@fp,numero_permis=@np,date_permis=@dp,
                    tranche1=@t1,tranche2=@t2,observations_complementaires=@oc,
                    nom_signataire=@ns2,lieu=@li,date_pv=@dpv
                    WHERE id=@id;
                    SELECT @id;";
                cmd.Parameters.AddWithValue("@id", pv.Id);
            }
            AddPVParams(cmd, pv);
            pv.Id = Convert.ToInt32(cmd.ExecuteScalar());

            // Delete existing rubriques and re-insert
            using var delCmd = conn.CreateCommand();
            delCmd.CommandText = "DELETE FROM rubriques_constat WHERE proces_verbal_id=@pvid";
            delCmd.Parameters.AddWithValue("@pvid", pv.Id);
            delCmd.ExecuteNonQuery();

            foreach (var rub in pv.Rubriques)
            {
                using var rubCmd = conn.CreateCommand();
                rubCmd.CommandText = @"
                    INSERT INTO rubriques_constat
                    (proces_verbal_id,rubrique,en_chiffre,en_lettres,has_observation,ordre)
                    VALUES(@pvid,@r,@ec,@el,@ho,@o)";
                rubCmd.Parameters.AddWithValue("@pvid", pv.Id);
                rubCmd.Parameters.AddWithValue("@r", rub.Rubrique);
                rubCmd.Parameters.AddWithValue("@ec", rub.EnChiffre);
                rubCmd.Parameters.AddWithValue("@el", rub.EnLettres);
                rubCmd.Parameters.AddWithValue("@ho", rub.HasObservation ? 1 : 0);
                rubCmd.Parameters.AddWithValue("@o", rub.Ordre);
                rubCmd.ExecuteNonQuery();
            }
            tx.Commit();
            return pv.Id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void DeletePV(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM proces_verbaux WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static void AddPVParams(SqliteCommand cmd, ProcesVerbal pv)
    {
        cmd.Parameters.AddWithValue("@bid", pv.BeneficiaireId);
        cmd.Parameters.AddWithValue("@ns", pv.NomSubdivisionnaire);
        cmd.Parameters.AddWithValue("@dl", pv.DirectionLogement);
        cmd.Parameters.AddWithValue("@da", pv.Daira);
        cmd.Parameters.AddWithValue("@co", pv.Commune);
        // ✅ التصحيح: السماح بتاريخ معاينة فارغ
        cmd.Parameters.AddWithValue("@dv", pv.DateVisite?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("@fp", pv.FractionProjet);
        cmd.Parameters.AddWithValue("@np", pv.NumeroPermis);
        // ✅ التصحيح: السماح بتاريخ رخصة بناء فارغ
        cmd.Parameters.AddWithValue("@dp", pv.DatePermis?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("@t1", pv.Tranche1 ? 1 : 0);
        cmd.Parameters.AddWithValue("@t2", pv.Tranche2 ? 1 : 0);
        cmd.Parameters.AddWithValue("@oc", pv.ObservationsComplementaires);
        cmd.Parameters.AddWithValue("@ns2", pv.NomSignataire);
        cmd.Parameters.AddWithValue("@li", pv.Lieu);
        // ✅ التصحيح: السماح بتاريخ محضر فارغ
        cmd.Parameters.AddWithValue("@dpv", pv.DatePV?.ToString("yyyy-MM-dd") ?? "");
    }

    private static ProcesVerbal ReadPV(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        BeneficiaireId = r.GetInt32(r.GetOrdinal("beneficiaire_id")),
        NomSubdivisionnaire = r["nom_subdivisionnaire"]?.ToString() ?? "",
        DirectionLogement = r["direction_logement"]?.ToString() ?? "MEDEA",
        Daira = r["daira"]?.ToString() ?? "TABLAT",
        Commune = r["commune"]?.ToString() ?? "DEUX BASSINS",
        // ✅ التصحيح: السماح بقراءة قيمة فارغة -> null
        DateVisite = r["date_visite"] == DBNull.Value ? null : DateTime.Parse(r["date_visite"].ToString()!),
        FractionProjet = r["fraction_projet"]?.ToString() ?? "",
        NumeroPermis = r["numero_permis"]?.ToString() ?? "",
        DatePermis = r["date_permis"] == DBNull.Value ? null : DateTime.Parse(r["date_permis"].ToString()!),
        Tranche1 = r.GetInt32(r.GetOrdinal("tranche1")) == 1,
        Tranche2 = r.GetInt32(r.GetOrdinal("tranche2")) == 1,
        ObservationsComplementaires = r["observations_complementaires"]?.ToString() ?? "",
        NomSignataire = r["nom_signataire"]?.ToString() ?? "",
        Lieu = r["lieu"]?.ToString() ?? "DEUX BASSINS",
        DatePV = r["date_pv"] == DBNull.Value ? null : DateTime.Parse(r["date_pv"].ToString()!),
        CreatedAt = DateTime.TryParse(r["created_at"]?.ToString(), out var ca) ? ca : DateTime.Now,
    };

    private static List<RubriqueConstat> GetRubriquesForPV(SqliteConnection conn, int pvId)
    {
        var list = new List<RubriqueConstat>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM rubriques_constat WHERE proces_verbal_id=@pvid ORDER BY ordre";
        cmd.Parameters.AddWithValue("@pvid", pvId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new RubriqueConstat
            {
                Id = r.GetInt32(r.GetOrdinal("id")),
                ProcesVerbalId = pvId,
                Rubrique = r["rubrique"]?.ToString() ?? "",
                EnChiffre = r["en_chiffre"]?.ToString() ?? "",
                EnLettres = r["en_lettres"]?.ToString() ?? "",
                HasObservation = r.GetInt32(r.GetOrdinal("has_observation")) == 1,
                Ordre = r.GetInt32(r.GetOrdinal("ordre")),
            });
        }
        return list;
    }

    // ─── Ingenieurs ──────────────────────────────────────────────────────────
    public List<Ingenieur> GetIngenieurs(bool actifOnly = true)
    {
        var list = new List<Ingenieur>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = actifOnly
            ? "SELECT * FROM ingenieurs WHERE actif=1 ORDER BY nom"
            : "SELECT * FROM ingenieurs ORDER BY nom";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(ReadIngenieur(r));
        return list;
    }

    public int SaveIngenieur(Ingenieur ing)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        if (ing.Id == 0)
        {
            cmd.CommandText = @"INSERT INTO ingenieurs(nom,prenom,qualite,actif)
                VALUES(@n,@p,@q,@a); SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"UPDATE ingenieurs SET nom=@n,prenom=@p,qualite=@q,actif=@a
                WHERE id=@id; SELECT @id;";
            cmd.Parameters.AddWithValue("@id", ing.Id);
        }
        cmd.Parameters.AddWithValue("@n", ing.Nom);
        cmd.Parameters.AddWithValue("@p", ing.Prenom);
        cmd.Parameters.AddWithValue("@q", ing.Qualite);
        cmd.Parameters.AddWithValue("@a", ing.Actif ? 1 : 0);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void DeleteIngenieur(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ingenieurs WHERE id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static Ingenieur ReadIngenieur(SqliteDataReader r) => new()
    {
        Id      = r.GetInt32(r.GetOrdinal("id")),
        Nom     = r["nom"]?.ToString()    ?? "",
        Prenom  = r["prenom"]?.ToString() ?? "",
        Qualite = r["qualite"]?.ToString() ?? "",
        Actif   = r.GetInt32(r.GetOrdinal("actif")) == 1,
    };

    // ─── Parametres ───────────────────────────────────────────────────────────
    public AppParametres GetParametres()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var dict = new Dictionary<string, string>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT cle,valeur FROM parametres";
        using var r = cmd.ExecuteReader();
        while (r.Read()) dict[r["cle"].ToString()!] = r["valeur"]?.ToString() ?? "";
        return new AppParametres
        {
            Commune = dict.GetValueOrDefault("commune", "DEUX BASSINS"),
            Daira   = dict.GetValueOrDefault("daira",   "TABLAT"),
            Wilaya  = dict.GetValueOrDefault("wilaya",  "MEDEA"),
        };
    }

    public void SaveParametres(AppParametres p)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        foreach (var (cle, val) in new[] {
            ("commune", p.Commune),
            ("daira",   p.Daira),
            ("wilaya",  p.Wilaya),
        })
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO parametres(cle,valeur) VALUES(@c,@v)";
            cmd.Parameters.AddWithValue("@c", cle);
            cmd.Parameters.AddWithValue("@v", val);
            cmd.ExecuteNonQuery();
        }
    }
}
