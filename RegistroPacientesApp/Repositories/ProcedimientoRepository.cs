using System;
using System.Collections.Generic;
using System.Data.SQLite;
using RegistroPacientesApp.Data;
using RegistroPacientesApp.Models;

namespace RegistroPacientesApp.Repositories
{
    public class ProcedimientoRepository
    {
        public List<Procedimiento> ObtenerPorActividad(int pacienteId, string actividad)
        {
            var lista = new List<Procedimiento>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
                    SELECT Id, Fecha, Dia, Actividad, Valor, Pago, Saldo
                    FROM Procedimiento
                    WHERE PacienteId = @PacienteId AND Actividad = @Actividad
                    ORDER BY Id ASC", conn);

                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                cmd.Parameters.AddWithValue("@Actividad", actividad);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Procedimiento
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Fecha = reader["Fecha"].ToString(),
                            Dia = reader["Dia"].ToString(),
                            Actividad = reader["Actividad"].ToString(),
                            Valor = Convert.ToDecimal(reader["Valor"]),
                            Pago = Convert.ToDecimal(reader["Pago"]),
                            Saldo = Convert.ToDecimal(reader["Saldo"])
                        });
                    }
                }
            }
            return lista;
        }

        public decimal? ObtenerSaldoAnterior(int pacienteId, string actividad)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
                    SELECT Saldo FROM Procedimiento
                    WHERE PacienteId = @PacienteId AND Actividad = @Actividad
                    ORDER BY Id DESC LIMIT 1", conn);

                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                cmd.Parameters.AddWithValue("@Actividad", actividad);

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : (decimal?)null;
            }
        }
    }
}
