using RegistroPacientesApp.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RegistroPacientesApp.Forms
{
    public partial class FrmPacientes : Form
    {
        TextBox txtBuscar;
        Label lblBuscar;

        DataGridView dgv;
        TextBox txtCedula, txtNombres, txtFechaNac, txtEdad, txtDireccion, txtTelefono, txtOcupacion, txtAntecedentes;
        ComboBox cmbEstadoCivil;
        Button btnGuardar, btnActualizar;
        Label lblTitulo;

        public FrmPacientes()
        {
            InicializarComponentesPersonalizados();
            CargarPacientes();
        }

        private void InicializarComponentesPersonalizados()
        {
            // ======== CONFIGURACIÓN GENERAL DEL FORMULARIO ========
            this.Text = "Gestión de Pacientes";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 950;
            this.Height = 650;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            // ======== TÍTULO ========
            lblTitulo = new Label()
            {
                Text = "Registro de Pacientes",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 150),
                AutoSize = true,
                Top = 15,
                Left = 330
            };
            Controls.Add(lblTitulo);

            int y = 70;
            int labelX = 40;
            int textX = 160;

            // ======== FILA 1 ========
            Controls.Add(new Label() { Text = "Cédula:", Left = labelX, Top = y + 5 });
            txtCedula = new TextBox() { Left = textX, Top = y, Width = 220, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtCedula);

            Controls.Add(new Label() { Text = "Nombres:", Left = 420, Top = y + 5 });
            txtNombres = new TextBox() { Left = 520, Top = y, Width = 350, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtNombres);

            // ======== FILA 2 ========
            y += 40;
            Controls.Add(new Label() { Text = "F. Nacimiento:", Left = labelX, Top = y + 5 });

            // === Reemplazamos TextBox por DateTimePicker ===
            DateTimePicker dtpFechaNac = new DateTimePicker()
            {
                Left = textX,
                Top = y,
                Width = 220,
                Format = DateTimePickerFormat.Short
            };
            dtpFechaNac.ValueChanged += (s, e) =>
            {
                // Calcular edad automáticamente
                int edad = DateTime.Today.Year - dtpFechaNac.Value.Year;
                if (DateTime.Today < dtpFechaNac.Value.AddYears(edad))
                    edad--;
                txtEdad.Text = edad.ToString();

                // Guardar valor para la base de datos
                txtFechaNac.Text = dtpFechaNac.Value.ToString("dd/MM/yyyy");
            };
            Controls.Add(dtpFechaNac);

            // Campo oculto (para guardar en BD)
            txtFechaNac = new TextBox() { Visible = false };
            Controls.Add(txtFechaNac);

            Controls.Add(new Label() { Text = "Edad:", Left = 420, Top = y + 5 });
            txtEdad = new TextBox() { Left = 520, Top = y, Width = 50, BorderStyle = BorderStyle.FixedSingle, ReadOnly = true };
            Controls.Add(txtEdad);

            // ======== FILA 3 ========
            y += 40;
            Controls.Add(new Label() { Text = "E. Civil:", Left = labelX, Top = y + 5 });

            cmbEstadoCivil = new ComboBox()
            {
                Left = textX,
                Top = y,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoCivil.Items.AddRange(new string[]
            {
    "Soltero",
    "Casado",
    "Divorciado",
    "Unión libre",
    "Viudo"
            });
            Controls.Add(cmbEstadoCivil);

           
            Controls.Add(new Label() { Text = "Dirección:", Left = 420, Top = y + 5 });
            txtDireccion = new TextBox()
            {
                Left = 520,
                Top = y,
                Width = 350,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtDireccion);



            // ======== FILA 4 ========
            y += 40;
            Controls.Add(new Label() { Text = "Teléfono:", Left = labelX, Top = y + 5 });
            txtTelefono = new TextBox() { Left = textX, Top = y, Width = 220, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtTelefono);

            Controls.Add(new Label() { Text = "Ocupación:", Left = 420, Top = y + 5 });
            txtOcupacion = new TextBox() { Left = 520, Top = y, Width = 350, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtOcupacion);

            // ======== FILA 5 ========
            y += 40;
            Controls.Add(new Label() { Text = "Antecedentes:", Left = labelX, Top = y + 5 });
            txtAntecedentes = new TextBox()
            {
                Left = textX,
                Top = y,
                Width = 710,
                Height = 60,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtAntecedentes);

            // ======== BOTONES ========
            y += 80;
            btnGuardar = new Button()
            {
                Text = "💾 Guardar",
                Left = 640,
                Top = y,
                Width = 120,
                BackColor = Color.FromArgb(60, 130, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            btnActualizar = new Button()
            {
                Text = "✏️ Actualizar",
                Left = 780,
                Top = y,
                Width = 120,
                BackColor = Color.FromArgb(100, 170, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += BtnActualizar_Click;
            Controls.Add(btnActualizar);

            // ======== BÚSQUEDA ========
            lblBuscar = new Label()
            {
                Text = "Buscar:",
                Left = 20,
                Top = y + 20,
                AutoSize = true
            };
            Controls.Add(lblBuscar);

            txtBuscar = new TextBox()
            {
                Left = 90,
                Top = y + 15,
                Width = 300
            };
            txtBuscar.TextChanged += (s, e) =>
            {
                (dgv.DataSource as DataTable).DefaultView.RowFilter =
                    $"Nombres LIKE '%{txtBuscar.Text}%' OR Cedula LIKE '%{txtBuscar.Text}%'";
            };
            Controls.Add(txtBuscar);

            y += 40; // espacio antes del DataGridView


            // ======== TABLA ========
            y += 60;
            dgv = new DataGridView()
            {
                Left = 20,
                Top = y,
                Width = 880,
                Height = 300,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.CellClick += Dgv_CellClick;
            Controls.Add(dgv);
        }


        private void CargarPacientes()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Paciente", conn);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgv.DataSource = dt;
                dgv.Columns["Id"].Visible = false;
                dgv.Columns["Nombres"].DisplayIndex = 0;
                dgv.Columns["Cedula"].DisplayIndex = 1;
                dgv.AutoResizeColumns();

            }
        }



        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // VALIDAR CAMPOS OBLIGATORIOS
            if (string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNac.Text))
            {
                MessageBox.Show("Por favor complete los campos obligatorios: Cédula, Nombres y Fecha de Nacimiento.",
                                "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"INSERT INTO Paciente 
            (Cedula, Nombres, FechaNacimiento, Edad, EstadoCivil, Direccion, Telefono, Ocupacion, Antecedentes)
            VALUES (@Cedula, @Nombres, @FechaNacimiento, @Edad, @EstadoCivil, @Direccion, @Telefono, @Ocupacion, @Antecedentes)", conn);

                cmd.Parameters.AddWithValue("@Cedula", txtCedula.Text);
                cmd.Parameters.AddWithValue("@Nombres", txtNombres.Text);
                cmd.Parameters.AddWithValue("@FechaNacimiento", txtFechaNac.Text);
                cmd.Parameters.AddWithValue("@Edad", txtEdad.Text);
                cmd.Parameters.AddWithValue("@EstadoCivil", cmbEstadoCivil.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text);
                cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text);
                cmd.Parameters.AddWithValue("@Ocupacion", txtOcupacion.Text);
                cmd.Parameters.AddWithValue("@Antecedentes", txtAntecedentes.Text);
                cmd.ExecuteNonQuery();
            }
            CargarPacientes();
            MessageBox.Show("Paciente registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"UPDATE Paciente SET 
                    Cedula=@Cedula, Nombres=@Nombres, FechaNacimiento=@FechaNacimiento, Edad=@Edad, EstadoCivil=@EstadoCivil,
                    Direccion=@Direccion, Telefono=@Telefono, Ocupacion=@Ocupacion, Antecedentes=@Antecedentes 
                    WHERE Id=@Id", conn);

                cmd.Parameters.AddWithValue("@Cedula", txtCedula.Text);
                cmd.Parameters.AddWithValue("@Nombres", txtNombres.Text);
                cmd.Parameters.AddWithValue("@FechaNacimiento", txtFechaNac.Text);
                cmd.Parameters.AddWithValue("@Edad", txtEdad.Text);
                cmd.Parameters.AddWithValue("@EstadoCivil", cmbEstadoCivil.SelectedItem?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text);
                cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text);
                cmd.Parameters.AddWithValue("@Ocupacion", txtOcupacion.Text);
                cmd.Parameters.AddWithValue("@Antecedentes", txtAntecedentes.Text);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            CargarPacientes();
            MessageBox.Show("Registro actualizado correctamente.", "Actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                txtCedula.Text = row.Cells["Cedula"].Value.ToString();
                txtNombres.Text = row.Cells["Nombres"].Value.ToString();
                txtFechaNac.Text = row.Cells["FechaNacimiento"].Value.ToString();
                txtEdad.Text = row.Cells["Edad"].Value.ToString();
                cmbEstadoCivil.SelectedItem = row.Cells["EstadoCivil"].Value.ToString();
                txtDireccion.Text = row.Cells["Direccion"].Value.ToString();
                txtTelefono.Text = row.Cells["Telefono"].Value.ToString();
                txtOcupacion.Text = row.Cells["Ocupacion"].Value.ToString();
                txtAntecedentes.Text = row.Cells["Antecedentes"].Value.ToString();
            }
        }
    }
}
