using System;
using System.Collections.Generic;
using RegistroPacientesApp.Models;
using RegistroPacientesApp.Repositories;

namespace RegistroPacientesApp.Services
{
    public class ProcedimientoService
    {
        private readonly ProcedimientoRepository _repo;

        public ProcedimientoService()
        {
            _repo = new ProcedimientoRepository();
        }

        public List<Procedimiento> ObtenerHistorial(int pacienteId, string actividad)
        {
            return _repo.ObtenerPorActividad(pacienteId, actividad);
        }

        public decimal CalcularNuevoSaldo(decimal saldoAnterior, decimal nuevoPago)
        {
            decimal saldo = saldoAnterior - nuevoPago;
            if (saldo < 0)
                throw new Exception("El pago excede el saldo anterior.");
            return saldo;
        }

        public decimal? ObtenerSaldoAnterior(int pacienteId, string actividad)
        {
            return _repo.ObtenerSaldoAnterior(pacienteId, actividad);
        }
    }
}
