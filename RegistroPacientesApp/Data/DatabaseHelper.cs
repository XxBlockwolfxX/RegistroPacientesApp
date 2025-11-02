using System;
using System.Data.SQLite;
using System.IO;

namespace RegistroPacientesApp.Data
{
    public static class DatabaseHelper
    {
        private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "registro.db");

        // 🔹 Se añade BusyTimeout al connection string
        private static readonly string connectionString =
            $"Data Source={dbPath};Version=3;BusyTimeout=5000;";

        public static SQLiteConnection GetConnection()
        {
            if (!File.Exists(dbPath))
                CreateDatabase();

            // 🔹 Devuelve una conexión con tiempo de espera habilitado
            var conn = new SQLiteConnection(connectionString);
            return conn;
        }

        private static void CreateDatabase()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            SQLiteConnection.CreateFile(dbPath);

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                new SQLiteCommand("PRAGMA journal_mode=WAL;", conn).ExecuteNonQuery();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Paciente (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Cedula TEXT,
                        Nombres TEXT,
                        FechaNacimiento TEXT,
                        Edad INTEGER,
                        EstadoCivil TEXT,
                        Direccion TEXT,
                        Telefono TEXT,
                        Ocupacion TEXT,
                        Antecedentes TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Procedimiento (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PacienteId INTEGER,
                        Numero INTEGER,
                        Dia TEXT,
                        Fecha TEXT,
                        Actividad TEXT,
                        Valor REAL,
                        Saldo REAL,
                        Constancia TEXT,
                        FOREIGN KEY(PacienteId) REFERENCES Paciente(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Odontograma (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PacienteId INTEGER NOT NULL,
                        Diente INTEGER NOT NULL,
                        Cara TEXT NOT NULL,
                        Estado TEXT NOT NULL,
                        Color TEXT,
                        Overlay TEXT,
                        FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (PacienteId) REFERENCES Paciente(Id)
                    );
                        CREATE TABLE IF NOT EXISTS Protesis (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PacienteId INTEGER NOT NULL,
    Tipo TEXT,
    Inicio INTEGER,
    Fin INTEGER,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PacienteId) REFERENCES Paciente(Id)
);

                ";
                new SQLiteCommand(sql, conn).ExecuteNonQuery();
            }
        }
    }
}
