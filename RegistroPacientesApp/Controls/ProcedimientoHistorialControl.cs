using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RegistroPacientesApp.Services;

namespace RegistroPacientesApp.Controls
{
    public class ProcedimientoHistorialControl : UserControl
    {
        private readonly ProcedimientoService _service = new ProcedimientoService();
        private readonly int _pacienteId;
        private readonly string _actividad;

        private DataGridView dgvHistorial;
        private Button btnCerrar;

        public event EventHandler Cerrado;

        public ProcedimientoHistorialControl(int pacienteId, string actividad)
        {
            _pacienteId = pacienteId;
            _actividad = actividad;
            Inicializar();
            CargarHistorial();
        }

        private void Inicializar()
        {
            this.Width = 1000;
            this.Height = 0;
            this.BackColor = Color.FromArgb(245, 248, 255);
            this.BorderStyle = BorderStyle.FixedSingle;

            dgvHistorial = new DataGridView()
            {
                Left = 10,
                Top = 35,
                Width = this.Width - 20,
                Height = 100,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(dgvHistorial);

            btnCerrar = new Button()
            {
                Text = "❌ Cerrar",
                Width = 80,
                Height = 25,
                Top = 5,
                Left = this.Width - 90,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => CerrarConAnimacion();
            this.Controls.Add(btnCerrar);
        }

        private void CargarHistorial()
        {
            var historial = _service.ObtenerHistorial(_pacienteId, _actividad);
            if (historial == null || historial.Count == 0)
            {
                MessageBox.Show("⚠️ No existen registros adicionales para esta actividad.");
                this.Dispose();
                return;
            }

            dgvHistorial.DataSource = historial;
            if (dgvHistorial.Columns.Contains("Id")) dgvHistorial.Columns["Id"].Visible = false;
            if (dgvHistorial.Columns.Contains("PacienteId")) dgvHistorial.Columns["PacienteId"].Visible = false;

            // Animación de despliegue
            var timer = new Timer() { Interval = 10 };
            timer.Tick += (s, e) =>
            {
                if (this.Height < 160)
                    this.Height += 15;
                else
                    timer.Stop();
            };
            timer.Start();
        }

        private void CerrarConAnimacion()
        {
            var timer = new Timer() { Interval = 10 };
            timer.Tick += (s, e) =>
            {
                this.Height -= 15;
                if (this.Height <= 0)
                {
                    this.Parent?.Controls.Remove(this);
                    Cerrado?.Invoke(this, EventArgs.Empty);
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}
