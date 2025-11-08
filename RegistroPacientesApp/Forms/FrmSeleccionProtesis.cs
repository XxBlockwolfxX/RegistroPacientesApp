using System;
using System.Drawing;
using System.Windows.Forms;

namespace RegistroPacientesApp.Forms
{
    public partial class FrmSeleccionProtesis : Form
    {
        public string TipoSeleccionado { get; private set; } = "";
        public int Inicio { get; private set; } = 0;
        public int Fin { get; private set; } = 0;
        public string EstadoSeleccionado { get; private set; } = "Realizada";

        private RadioButton rbRealizada;
        private RadioButton rbPorRealizar;

        public FrmSeleccionProtesis()
        {
            InicializarUI();
        }

        private void InicializarUI()
        {
            Text = "Seleccionar tipo de prótesis";
            Size = new Size(320, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.WhiteSmoke;

            // === Botón prótesis superior ===
            Button btnSup = new Button()
            {
                Text = "🦷 Prótesis superior",
                Width = 220,
                Height = 40,
                Top = 20,
                Left = 45,
                BackColor = Color.FromArgb(80, 150, 240),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSup.FlatAppearance.BorderSize = 0;
            btnSup.Click += (s, e) =>
            {
                TipoSeleccionado = "Superior Total";
                ConfirmarSeleccion();
            };
            Controls.Add(btnSup);

            // === Botón prótesis inferior ===
            Button btnInf = new Button()
            {
                Text = "🦷 Prótesis inferior",
                Width = 220,
                Height = 40,
                Top = 70,
                Left = 45,
                BackColor = Color.FromArgb(240, 100, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnInf.FlatAppearance.BorderSize = 0;
            btnInf.Click += (s, e) =>
            {
                TipoSeleccionado = "Inferior Total";
                ConfirmarSeleccion();
            };
            Controls.Add(btnInf);

            // === Sección ESTADO ===
            Label lblEstado = new Label()
            {
                Text = "Estado de la prótesis:",
                Top = 125,
                Left = 30,
                Width = 250,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Controls.Add(lblEstado);

            rbRealizada = new RadioButton()
            {
                Text = "Realizada",
                Top = 150,
                Left = 60,
                Checked = true
            };
            Controls.Add(rbRealizada);

            rbPorRealizar = new RadioButton()
            {
                Text = "Por realizar",
                Top = 175,
                Left = 60
            };
            Controls.Add(rbPorRealizar);

            // === Línea divisoria ===
            Label divider = new Label()
            {
                BorderStyle = BorderStyle.Fixed3D,
                Width = 250,
                Height = 2,
                Top = 205,
                Left = 30
            };
            Controls.Add(divider);

            // === Etiqueta removible ===
            Label lblRem = new Label()
            {
                Text = "🧩 Prótesis removible",
                AutoSize = false,
                Width = 250,
                TextAlign = ContentAlignment.MiddleCenter,
                Top = 220,
                Left = 30,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Controls.Add(lblRem);

            // === Campos de rango ===
            Label lblDesde = new Label()
            {
                Text = "Desde:",
                Left = 60,
                Top = 255,
                AutoSize = true
            };
            Controls.Add(lblDesde);

            TextBox txtDesde = new TextBox()
            {
                Name = "txtDesde",
                Left = 110,
                Top = 250,
                Width = 50
            };
            Controls.Add(txtDesde);

            Label lblHasta = new Label()
            {
                Text = "Hasta:",
                Left = 170,
                Top = 255,
                AutoSize = true
            };
            Controls.Add(lblHasta);

            TextBox txtHasta = new TextBox()
            {
                Name = "txtHasta",
                Left = 220,
                Top = 250,
                Width = 50
            };
            Controls.Add(txtHasta);

            // === Botón confirmar removible ===
            Button btnRem = new Button()
            {
                Text = "Registrar Removible",
                Width = 220,
                Height = 40,
                Top = 295,
                Left = 45,
                BackColor = Color.FromArgb(100, 180, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRem.FlatAppearance.BorderSize = 0;
            btnRem.Click += (s, e) =>
            {
                if (int.TryParse(txtDesde.Text, out int d1) && int.TryParse(txtHasta.Text, out int d2))
                {
                    Inicio = Math.Min(d1, d2);
                    Fin = Math.Max(d1, d2);
                    TipoSeleccionado = "Removible Parcial";
                    ConfirmarSeleccion();
                }
                else
                {
                    MessageBox.Show("Ingrese valores numéricos válidos para el rango.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            Controls.Add(btnRem);
        }

        private void ConfirmarSeleccion()
        {
            EstadoSeleccionado = rbPorRealizar.Checked ? "Por Realizar" : "Realizada";
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
