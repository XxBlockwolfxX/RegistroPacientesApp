using RegistroPacientesApp.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RegistroPacientesApp.Forms
{
    public partial class FrmProcedimientos : Form
    {
        TextBox txtBuscar;
        DataGridView dgvPacientes;

        public FrmProcedimientos()
        {
            InicializarComponentesPersonalizados();
            CargarPacientes();
        }

        private void InicializarComponentesPersonalizados()
        {
            this.Text = "Gestión de Procedimientos";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 950;
            this.Height = 600;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            // ======== TÍTULO ========
            Label lblTitulo = new Label()
            {
                Text = "Lista de Pacientes",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 150),
                AutoSize = true,
                Top = 20,
                Left = 350
            };
            Controls.Add(lblTitulo);

            // ======== BÚSQUEDA ========
            Label lblBuscar = new Label()
            {
                Text = "Buscar:",
                Left = 20,
                Top = 70,
                AutoSize = true
            };
            Controls.Add(lblBuscar);

            txtBuscar = new TextBox()
            {
                Left = 90,
                Top = 65,
                Width = 300
            };
            txtBuscar.TextChanged += (s, e) =>
            {
                (dgvPacientes.DataSource as DataTable).DefaultView.RowFilter =
                    $"Nombres LIKE '%{txtBuscar.Text}%' OR Cedula LIKE '%{txtBuscar.Text}%'";
            };
            Controls.Add(txtBuscar);

            // ======== TABLA ========
            dgvPacientes = new DataGridView()
            {
                Left = 20,
                Top = 110,
                Width = 900,
                Height = 400,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvPacientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255);
            dgvPacientes.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPacientes.CellContentClick += DgvPacientes_CellContentClick;
            Controls.Add(dgvPacientes);
        }

        private void CargarPacientes()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT Id, Cedula, Nombres, Edad, EstadoCivil, Direccion, Telefono FROM Paciente", conn);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Agregar botón "Ver detalles"
                DataColumn colBtn = new DataColumn("Acción", typeof(string));
                dt.Columns.Add(colBtn);
                foreach (DataRow row in dt.Rows)
                    row["Acción"] = "Ver detalles";

                dgvPacientes.DataSource = dt;
                dgvPacientes.Columns["Id"].Visible = false;

                // Estilo para el botón
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
                {
                    Text = "🔍 Ver detalles",
                    UseColumnTextForButtonValue = true,
                    HeaderText = "Acción",
                    Name = "btnVer",
                    FlatStyle = FlatStyle.Flat
                };
                dgvPacientes.Columns.Add(btnCol);
            }
        }

        private void DgvPacientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPacientes.Columns[e.ColumnIndex].Name == "btnVer")
            {
                int pacienteId = Convert.ToInt32(dgvPacientes.Rows[e.RowIndex].Cells["Id"].Value);
                FrmDetallePaciente detalle = new FrmDetallePaciente(pacienteId);
                detalle.ShowDialog();
            }
        }
    }
}
