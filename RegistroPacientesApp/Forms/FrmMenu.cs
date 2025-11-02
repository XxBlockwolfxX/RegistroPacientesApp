using System;
using System.Windows.Forms;

namespace RegistroPacientesApp.Forms
{
    public partial class FrmMenu : Form
    {
        public FrmMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Registro de Pacientes - Menú Principal";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 400;
            this.Height = 300;

            Label lblTitulo = new Label()
            {
                Text = "MENÚ PRINCIPAL",
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                Top = 20,
                Left = 100
            };
            Controls.Add(lblTitulo);

            Button btnPacientes = new Button()
            {
                Text = "Registrar Pacientes",
                Width = 200,
                Height = 40,
                Top = 80,
                Left = 90
            };
            btnPacientes.Click += (s, e) => new FrmPacientes().ShowDialog();
            Controls.Add(btnPacientes);

            Button btnProcedimientos = new Button()
            {
                Text = "Registrar Procedimientos",
                Width = 200,
                Height = 40,
                Top = 140,
                Left = 90
            };
            btnProcedimientos.Click += (s, e) => new FrmProcedimientos().ShowDialog();
            Controls.Add(btnProcedimientos);

            Button btnSalir = new Button()
            {
                Text = "Salir",
                Width = 200,
                Height = 40,
                Top = 200,
                Left = 90
            };
            btnSalir.Click += (s, e) => this.Close();
            Controls.Add(btnSalir);
        }
    }
}
