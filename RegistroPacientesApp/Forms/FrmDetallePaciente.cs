using Microsoft.VisualBasic;
using RegistroPacientesApp.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RegistroPacientesApp.Services;

namespace RegistroPacientesApp.Forms
{
    public partial class FrmDetallePaciente : Form
    {
        Label lblNombre, lblEdad, lblDireccion, lblEstadoCivil, lblTelefono;
        Panel pnlOdontograma;
        DataGridView dgvProcedimientos;
        private int _pacienteId;
        private readonly ProcedimientoService _service = new ProcedimientoService();

        // ✅ Soporta múltiples prótesis
        private List<(string Tipo, int Inicio, int Fin, string Estado)> protesisLista = new List<(string, int, int, string)>();

        private class PiezaTag
        {
            public int NumeroDiente { get; set; }
            public FaceState Estado { get; set; } = new FaceState();
        }
        private class FaceState
        {
            public Color FillColor = Color.White;
            public Color BorderColor = Color.Gray;
            public string Overlay = "None";

        }
        class OverlayPrótesis : Control
        {
            public Action<Graphics> OnDraw;

            public OverlayPrótesis()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint, true);
                BackColor = Color.Transparent;
                Enabled = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                OnDraw?.Invoke(e.Graphics);
            }
        }
        public FrmDetallePaciente(int pacienteId)
        {
            this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
            _pacienteId = pacienteId;
            InicializarComponentes();
            CrearTablaOdontogramaSiNoExiste();
            CrearTablaProtesisSiNoExiste();
            CargarDatosPaciente(pacienteId);
            CargarProcedimientos(pacienteId);
            GenerarOdontograma();
            CargarOdontograma(pacienteId);
        }

        #region === INICIALIZACIÓN GENERAL ===

        private void InicializarComponentes()
        {
            this.Text = "Detalle del Paciente";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 1200;
            this.Height = 900;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            this.AutoScroll = true;

            // === TÍTULO PRINCIPAL ===
            Label lblTitulo = new Label()
            {
                Text = "Ficha del Paciente",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 150),
                AutoSize = true,
                Top = 15,
                Left = 480
            };
            Controls.Add(lblTitulo);

            // === DATOS DEL PACIENTE ===
            TableLayoutPanel datosPanel = new TableLayoutPanel()
            {
                Left = 40,
                Top = 70,
                Width = 1100,
                Height = 100,
                ColumnCount = 2,
                RowCount = 3
            };
            datosPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            datosPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Controls.Add(datosPanel);

            lblNombre = new Label() { AutoSize = true };
            lblEdad = new Label() { AutoSize = true };
            lblDireccion = new Label() { AutoSize = true };
            lblEstadoCivil = new Label() { AutoSize = true };
            lblTelefono = new Label() { AutoSize = true };

            datosPanel.Controls.Add(lblNombre, 0, 0);
            datosPanel.Controls.Add(lblEdad, 1, 0);
            datosPanel.Controls.Add(lblDireccion, 0, 1);
            datosPanel.Controls.Add(lblEstadoCivil, 1, 1);
            datosPanel.Controls.Add(lblTelefono, 0, 2);

            // === TÍTULO ODONTOGRAMA ===
            Label lblOdonto = new Label()
            {
                Text = "Odontograma:",
                Left = 50,
                Top = datosPanel.Bottom + 20,
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 120)
            };
            Controls.Add(lblOdonto);

            // === PANEL PRINCIPAL ===
            pnlOdontograma = new Panel()
            {
                Left = 20,
                Top = lblOdonto.Bottom + 10,
                Width = this.ClientSize.Width - 60,
                Height = 460,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            Controls.Add(pnlOdontograma);

            // === BOTONES ===
            Button btnProtesis = new Button()
            {
                Text = "🦷 Prótesis Total / Removible",
                Left = 40,
                Width = 250,
                Height = 45,
                Top = pnlOdontograma.Bottom + 20,
                BackColor = Color.FromArgb(80, 150, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnProtesis.FlatAppearance.BorderSize = 0;
            btnProtesis.Click += BtnProtesis_Click;
            Controls.Add(btnProtesis);

            Button btnRecargar = new Button()
            {
                Text = "🔄 Recargar Odontograma",
                Left = 600,
                Top = pnlOdontograma.Bottom + 20,
                Width = 220,
                Height = 40,
                BackColor = Color.FromArgb(100, 150, 220),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRecargar.Click += (s, e) => CargarOdontograma(_pacienteId);
            Controls.Add(btnRecargar);

            Button btnGuardar = new Button()
            {
                Text = "💾 Guardar Odontograma",
                Left = 850,
                Top = pnlOdontograma.Bottom + 20,
                Width = 220,
                Height = 40,
                BackColor = Color.FromArgb(45, 90, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGuardar.Click += (s, e) => GuardarOdontograma(_pacienteId);
            Controls.Add(btnGuardar);

            // === SECCIÓN DE PROCEDIMIENTOS ===
            int offsetY = pnlOdontograma.Bottom + 160;
            Label lblProc = new Label()
            {
                Text = "Procedimientos Realizados:",
                Left = 40,
                Top = offsetY,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(lblProc);

            // === CAMPOS PARA NUEVO PROCEDIMIENTO ===
            Label lblNuevo = new Label()
            {
                Text = "Registrar nuevo procedimiento:",
                Left = 40,
                Top = lblProc.Bottom + 10,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            Controls.Add(lblNuevo);

            Font labelFont = new Font("Segoe UI", 9f);

            Label lblFecha = new Label() { Text = "Fecha:", Left = 40, Top = lblNuevo.Bottom + 12, AutoSize = true, Font = labelFont };
            DateTimePicker dtpFecha = new DateTimePicker()
            {
                Left = 90,
                Top = lblNuevo.Bottom + 8,
                Width = 115,
                Format = DateTimePickerFormat.Short,
                Name = "dtpFecha"
            };
            Controls.Add(lblFecha);
            Controls.Add(dtpFecha);

            Label lblActividad = new Label() { Text = "Actividad:", Left = 220, Top = lblNuevo.Bottom + 12, AutoSize = true, Font = labelFont };
            TextBox txtActividad = new TextBox()
            {
                Left = 290,
                Top = lblNuevo.Bottom + 8,
                Width = 250,
                Name = "txtActividad"
            };
            Controls.Add(lblActividad);
            Controls.Add(txtActividad);

            Label lblValor = new Label() { Text = "Valor ($):", Left = 560, Top = lblNuevo.Bottom + 12, AutoSize = true, Font = labelFont };
            TextBox txtValor = new TextBox()
            {
                Left = 620,
                Top = lblNuevo.Bottom + 8,
                Width = 60,
                Name = "txtValor"
            };
            Controls.Add(lblValor);
            Controls.Add(txtValor);

            Label lblPago = new Label() { Text = "Pago ($):", Left = 690, Top = lblNuevo.Bottom + 12, AutoSize = true, Font = labelFont };
            TextBox txtPago = new TextBox()
            {
                Left = 750,
                Top = lblNuevo.Bottom + 8,
                Width = 60,
                Name = "txtPago"
            };
            Controls.Add(lblPago);
            Controls.Add(txtPago);

            Label lblSaldo = new Label() { Text = "Saldo ($):", Left = 820, Top = lblNuevo.Bottom + 12, AutoSize = true, Font = labelFont };
            TextBox txtSaldo = new TextBox()
            {
                Left = 890,
                Top = lblNuevo.Bottom + 8,
                Width = 60,
                Name = "txtSaldo",
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                TabStop = false
            };
            Controls.Add(lblSaldo);
            Controls.Add(txtSaldo);

            void CalcularSaldo()
            {
                decimal valor = 0, pago = 0;
                decimal.TryParse(txtValor.Text, out valor);
                decimal.TryParse(txtPago.Text, out pago);
                // Evitar saldo negativo
                if (pago > valor)
                {
                    txtSaldo.Text = "0.00";
                    return;
                }
                decimal saldo = valor - pago;
                txtSaldo.Text = saldo.ToString("0.00");
            }

            //Usa Leave o Validated en lugar de TextChanged
            txtValor.Leave += (s, e) => CalcularSaldo();
            txtPago.Leave += (s, e) => CalcularSaldo();
            txtValor.TextChanged += (s, e) => CalcularSaldo();
            txtPago.TextChanged += (s, e) => CalcularSaldo();
            Button btnAgregarProc = new Button()
            {
                Text = "➕ Agregar Procedimiento",
                Left = 1080,
                Top = lblNuevo.Bottom + 8,
                Width = 180,
                Height = 35,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAgregarProc.FlatAppearance.BorderSize = 0;
            btnAgregarProc.Click += (s, e) => AgregarProcedimiento();
            Controls.Add(btnAgregarProc);

            // === TABLA DE PROCEDIMIENTOS DEBAJO ===
            dgvProcedimientos = new DataGridView()
            {
                Left = 40,
                Top = btnAgregarProc.Bottom + 20,
                Width = 1120,
                Height = 250,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgvProcedimientos);

            //Evento para llenar los campos cuando se selecciona una fila
            dgvProcedimientos.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvProcedimientos.Rows[e.RowIndex].DataBoundItem != null)
                {
                    var row = dgvProcedimientos.Rows[e.RowIndex];

                    //Buscar los controles de registro
                    TextBox campoActividad = Controls.Find("txtActividad", true).FirstOrDefault() as TextBox;
                    TextBox campoValor = Controls.Find("txtValor", true).FirstOrDefault() as TextBox;
                    TextBox campoPago = Controls.Find("txtPago", true).FirstOrDefault() as TextBox;
                    TextBox campoSaldo = Controls.Find("txtSaldo", true).FirstOrDefault() as TextBox;
                    DateTimePicker campoFecha = Controls.Find("dtpFecha", true).FirstOrDefault() as DateTimePicker;

                    if (campoActividad == null || campoValor == null || campoPago == null || campoSaldo == null || campoFecha == null)
                        return;

                    //Cargar valores en los campos de registro
                    campoFecha.Value = DateTime.TryParse(row.Cells["Fecha"].Value?.ToString(), out DateTime fecha)
                        ? fecha : DateTime.Today;

                    campoActividad.Text = row.Cells["Actividad"].Value?.ToString() ?? "";
                    campoValor.Text = row.Cells["Valor"].Value?.ToString() ?? "";
                    campoPago.Text = row.Cells["Pago"].Value?.ToString() ?? "";
                    campoSaldo.Text = row.Cells["Saldo"].Value?.ToString() ?? "";
                    dgvProcedimientos.Tag = null;
                }
            };

            // 🧩 Mostrar historial con animación y botón de cierre
            dgvProcedimientos.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var col = dgvProcedimientos.Columns[e.ColumnIndex];
                if (col == null || col.Name != "Actividad") return;

                string actividad = dgvProcedimientos.Rows[e.RowIndex].Cells["Actividad"].Value?.ToString();
                if (string.IsNullOrEmpty(actividad)) return;

                // 🔹 Si ya hay un subgrid abierto, eliminarlo (cerrar anterior)
                var anterior = Controls.Find("panelHistorial", true).FirstOrDefault();
                if (anterior != null)
                {
                    Controls.Remove(anterior);
                    return; // Si se hace clic en otra actividad, cierra el anterior
                }

                var historial = _service.ObtenerHistorial(_pacienteId, actividad);
                if (historial == null || historial.Count == 0)
                {
                    MessageBox.Show("⚠️ No existen registros adicionales para esta actividad.");
                    return;
                }

                // === PANEL CONTENEDOR DEL SUBGRID ===
                Panel panelHistorial = new Panel()
                {
                    Name = "panelHistorial",
                    Width = dgvProcedimientos.Width - 50,
                    Height = 0, // comienza colapsado
                    Left = dgvProcedimientos.Left + 25,
                    Top = dgvProcedimientos.Top + (e.RowIndex + 1) * dgvProcedimientos.RowTemplate.Height + 30,
                    BackColor = Color.FromArgb(245, 248, 255),
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = true
                };

                // === BOTÓN CERRAR ===
                Button btnCerrar = new Button()
                {
                    Text = "❌ Cerrar",
                    Width = 80,
                    Height = 25,
                    Top = 5,
                    Left = panelHistorial.Width - 90,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightGray,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular)
                };
                btnCerrar.FlatAppearance.BorderSize = 0;
                btnCerrar.Click += (s2, e2) =>
                {
                    // Animación de cierre
                    var timer = new Timer() { Interval = 10 };
                    timer.Tick += (s3, e3) =>
                    {
                        panelHistorial.Height -= 15;
                        if (panelHistorial.Height <= 0)
                        {
                            Controls.Remove(panelHistorial);
                            timer.Stop();
                        }
                    };
                    timer.Start();
                };

                // === SUBGRID ===
                DataGridView dgvSub = new DataGridView()
                {
                    Width = panelHistorial.Width - 20,
                    Height = 100,
                    Left = 10,
                    Top = 35,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                dgvSub.DataSource = historial;
                if (dgvSub.Columns.Contains("Id")) dgvSub.Columns["Id"].Visible = false;
                if (dgvSub.Columns.Contains("PacienteId")) dgvSub.Columns["PacienteId"].Visible = false;

                panelHistorial.Controls.Add(dgvSub);
                panelHistorial.Controls.Add(btnCerrar);
                Controls.Add(panelHistorial);
                panelHistorial.BringToFront();

                // === ANIMACIÓN DE DESPLIEGUE ===
                var animTimer = new Timer() { Interval = 10 };
                animTimer.Tick += (s2, e2) =>
                {
                    if (panelHistorial.Height < 160)
                        panelHistorial.Height += 15;
                    else
                        animTimer.Stop();
                };
                animTimer.Start();
            };




            // === BOTONES DE ACCIÓN ===
            var btnEditar = new DataGridViewButtonColumn
            {
                Name = "colEditar",
                HeaderText = "Editar",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgvProcedimientos.Columns.Add(btnEditar);

            var btnEliminar = new DataGridViewButtonColumn
            {
                Name = "colEliminar",
                HeaderText = "Eliminar",
                Text = "🗑️",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgvProcedimientos.Columns.Add(btnEliminar);

        }

        private void DgvProcedimientos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvProcedimientos.Columns[e.ColumnIndex];
            if (col is not DataGridViewButtonColumn) return;

            var row = dgvProcedimientos.Rows[e.RowIndex];
            var idObj = row.Cells["Id"]?.Value;

            if (idObj == null || idObj == DBNull.Value)
            {
                MessageBox.Show("No se pudo obtener el identificador del procedimiento.");
                return;
            }

            int numero = Convert.ToInt32(idObj);

            if (col.Name == "colEditar")
            {
                // === EDITAR ===
                string actividad = row.Cells["Actividad"]?.Value?.ToString() ?? "";
                string valor = row.Cells["Valor"]?.Value?.ToString() ?? "";
                string pago = row.Cells["Pago"]?.Value?.ToString() ?? "";
                string saldo = row.Cells["Saldo"]?.Value?.ToString() ?? "";
                string fecha = row.Cells["Fecha"]?.Value?.ToString() ?? "";

                var txtActividad = Controls.Find("txtActividad", true).FirstOrDefault() as TextBox;
                var txtValor = Controls.Find("txtValor", true).FirstOrDefault() as TextBox;
                var txtPago = Controls.Find("txtPago", true).FirstOrDefault() as TextBox;
                var txtSaldo = Controls.Find("txtSaldo", true).FirstOrDefault() as TextBox;
                var dtpFecha = Controls.Find("dtpFecha", true).FirstOrDefault() as DateTimePicker;

                if (txtActividad == null || txtValor == null || txtPago == null || txtSaldo == null || dtpFecha == null)
                    return;

                txtActividad.Text = actividad;
                txtValor.Text = valor;
                txtPago.Text = pago;
                txtSaldo.Text = saldo;
                if (DateTime.TryParse(fecha, out DateTime parsedFecha))
                    dtpFecha.Value = parsedFecha;

                dgvProcedimientos.Tag = numero;
            }
            else if (col.Name == "colEliminar")
            {
                // === ELIMINAR ===
                string actividad = row.Cells["Actividad"]?.Value?.ToString() ?? "(sin nombre)";
                var confirm = MessageBox.Show(
                    $"¿Seguro que deseas eliminar '{actividad}'?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm != DialogResult.Yes) return;

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using var cmd = new SQLiteCommand("DELETE FROM Procedimiento WHERE Id = @Id", conn);
                    cmd.Parameters.AddWithValue("@Id", numero);

                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                    {
                        MessageBox.Show("🗑️ Procedimiento eliminado correctamente.",
                            "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("⚠️ No se eliminó ningún registro (verifica la columna clave).",
                            "Sin cambios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                CargarProcedimientos(_pacienteId);
                dgvProcedimientos.ClearSelection();
                dgvProcedimientos.Tag = null;
            }
        }
        #endregion

        #region === CARGA DE DATOS ===
        private void CargarDatosPaciente(int pacienteId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Paciente WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", pacienteId);
                SQLiteDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    lblNombre.Text = $"Nombre: {reader["Nombres"]}";
                    lblEdad.Text = $"Edad: {reader["Edad"]}";
                    lblDireccion.Text = $"Dirección: {reader["Direccion"]}";
                    lblEstadoCivil.Text = $"Estado Civil: {reader["EstadoCivil"]}";
                    lblTelefono.Text = $"Teléfono: {reader["Telefono"]}";
                }
            }
        }

        private void CargarProcedimientos(int pacienteId)
        {
            dgvProcedimientos.Columns.Clear();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
            SELECT Id, Dia, Fecha, Actividad, Valor, Pago, Saldo

            FROM Procedimiento 
            WHERE PacienteId = @PacienteId", conn);
                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvProcedimientos.DataSource = dt;
            }

            //Ocultar la columna Id (clave primaria)
            if (dgvProcedimientos.Columns["Id"] != null)
                dgvProcedimientos.Columns["Id"].Visible = false;



            //Añadir columnas de botones DESPUÉS del DataSource
            var btnEditar = new DataGridViewButtonColumn
            {
                Name = "colEditar",
                HeaderText = "Editar",
                Text = "✏️",
                UseColumnTextForButtonValue = true,
                Width = 60
            };
            dgvProcedimientos.Columns.Add(btnEditar);

            var btnEliminar = new DataGridViewButtonColumn
            {
                Name = "colEliminar",
                HeaderText = "Eliminar",
                Text = "🗑️",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgvProcedimientos.Columns.Add(btnEliminar);

            dgvProcedimientos.CellContentClick -= DgvProcedimientos_CellContentClick;
            dgvProcedimientos.CellContentClick += DgvProcedimientos_CellContentClick;

        }
        private void AgregarProcedimiento()
        {
            try
            {
                // Buscar los controles
                DateTimePicker dtpFecha = Controls.Find("dtpFecha", true).FirstOrDefault() as DateTimePicker;
                TextBox txtActividad = Controls.Find("txtActividad", true).FirstOrDefault() as TextBox;
                TextBox txtValor = Controls.Find("txtValor", true).FirstOrDefault() as TextBox;
                TextBox txtPago = Controls.Find("txtPago", true).FirstOrDefault() as TextBox;
                TextBox txtSaldo = Controls.Find("txtSaldo", true).FirstOrDefault() as TextBox;

                if (dtpFecha == null || txtActividad == null) return;

                if (string.IsNullOrWhiteSpace(txtActividad.Text))
                {
                    MessageBox.Show("⚠️ Debe ingresar una actividad.", "Campos obligatorios",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Leer valores
                decimal valor = 0, pago = 0;
                decimal.TryParse(txtValor.Text, out valor);
                decimal.TryParse(txtPago.Text, out pago);

                if (valor <= 0)
                {
                    MessageBox.Show("⚠️ El valor del procedimiento no puede ser cero.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pago < 0)
                {
                    MessageBox.Show("⚠️ El pago no puede ser negativo.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal saldoAnterior = 0;
                bool existeProcedimiento = false;

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    //Buscar el último registro de la misma actividad
                    var checkCmd = new SQLiteCommand(@"
                SELECT Saldo 
                FROM Procedimiento
                WHERE PacienteId = @PacienteId AND Actividad = @Actividad
                ORDER BY Id DESC LIMIT 1", conn);
                    checkCmd.Parameters.AddWithValue("@PacienteId", _pacienteId);
                    checkCmd.Parameters.AddWithValue("@Actividad", txtActividad.Text);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        saldoAnterior = Convert.ToDecimal(result);
                        existeProcedimiento = true;
                    }

                    decimal saldoNuevo = 0;

                    if (existeProcedimiento)
                    {
                        //Calcular nuevo saldo según el saldo anterior
                        saldoNuevo = saldoAnterior - pago;
                        if (saldoNuevo < 0)
                        {
                            MessageBox.Show("⚠️ El pago excede el saldo anterior.", "Error de pago",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        valor = valor == 0 ? saldoAnterior : valor; 
                    }
                    else
                    {
                        //Primer registro del tratamiento
                        saldoNuevo = valor - pago;
                        if (saldoNuevo < 0)
                        {
                            MessageBox.Show("⚠️ El pago no puede superar el valor total.", "Error de pago",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    //Insertar nuevo registro sin eliminar el anterior
                    var insertCmd = new SQLiteCommand(@"
                INSERT INTO Procedimiento (PacienteId, Fecha, Dia, Actividad, Valor, Pago, Saldo)
                VALUES (@PacienteId, @Fecha, @Dia, @Actividad, @Valor, @Pago, @Saldo)", conn);

                    insertCmd.Parameters.AddWithValue("@PacienteId", _pacienteId);
                    insertCmd.Parameters.AddWithValue("@Fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@Dia", dtpFecha.Value.DayOfWeek.ToString());
                    insertCmd.Parameters.AddWithValue("@Actividad", txtActividad.Text);
                    insertCmd.Parameters.AddWithValue("@Valor", valor);
                    insertCmd.Parameters.AddWithValue("@Pago", pago);
                    insertCmd.Parameters.AddWithValue("@Saldo", saldoNuevo);
                    insertCmd.ExecuteNonQuery();

                    txtSaldo.Text = saldoNuevo.ToString("0.00");

                    MessageBox.Show($"✅ Pago registrado correctamente.\nSaldo anterior: {saldoAnterior:C}\nNuevo saldo: {saldoNuevo:C}",
                        "Registro guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //Refrescar tabla
                CargarProcedimientos(_pacienteId);

                //Limpiar campos
                txtActividad.Clear();
                txtValor.Clear();
                txtPago.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al agregar el procedimiento: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarOdontograma(int pacienteId)
        {
            // 🧹 Limpiar estados previos
            foreach (var pieza in pnlOdontograma.Controls.OfType<Panel>())
            {
                if (pieza.Tag is PiezaTag info)
                    info.Estado = new FaceState();

                foreach (var btn in pieza.Controls.OfType<Button>())
                {
                    if (btn.Tag is FaceState st)
                    {
                        st.FillColor = Color.White;
                        st.Overlay = "None";
                        btn.Invalidate();
                    }
                }
                pieza.Invalidate();
            }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // === Cargar datos del odontograma ===
                var cmd = new SQLiteCommand(@"
            SELECT Diente, Cara, Color, Overlay 
            FROM Odontograma
            WHERE PacienteId = @PacienteId", conn);
                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int diente = Convert.ToInt32(reader["Diente"]);
                        string cara = reader["Cara"].ToString();
                        string colorName = reader["Color"].ToString();
                        string overlay = reader["Overlay"].ToString();

                        var pieza = pnlOdontograma.Controls
                            .OfType<Panel>()
                            .FirstOrDefault(p => p.Tag is PiezaTag tag && tag.NumeroDiente == diente);

                        if (pieza == null) continue;

                        //Estado general
                        if (cara == "General")
                        {
                            if (pieza.Tag is PiezaTag info)
                            {
                                info.Estado.Overlay = overlay;
                                try
                                {
                                    info.Estado.BorderColor = Color.FromName(colorName);
                                }
                                catch
                                {
                                    info.Estado.BorderColor = Color.Gray;
                                }
                                pieza.Invalidate();
                            }
                        }
                        else
                        {
                            //Caras individuales
                            foreach (var ctrl in pieza.Controls.OfType<Button>())
                            {
                                if (ctrl.Text == cara && ctrl.Tag is FaceState st)
                                {
                                    try
                                    {
                                        st.FillColor = string.IsNullOrEmpty(colorName)
                                            ? Color.White
                                            : Color.FromName(colorName);
                                    }
                                    catch
                                    {
                                        st.FillColor = Color.White;
                                    }

                                    st.Overlay = overlay;
                                    ctrl.Invalidate();
                                    break;
                                }
                            }
                        }
                    }
                }

                // ===Cargar todas las prótesis del paciente ===
                protesisLista.Clear();
                using (var cmdProt = new SQLiteCommand(
                    "SELECT Tipo, Inicio, Fin, Estado FROM Protesis WHERE PacienteId = @id", conn))
                {
                    cmdProt.Parameters.AddWithValue("@id", pacienteId);
                    using (var rd = cmdProt.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string tipo = rd["Tipo"].ToString();
                            int inicio = Convert.ToInt32(rd["Inicio"]);
                            int fin = Convert.ToInt32(rd["Fin"]);
                            string estado = rd["Estado"]?.ToString() ?? "Realizada";
                            protesisLista.Add((tipo, inicio, fin, estado));

                        }
                    }
                }
                pnlOdontograma.Refresh();

            }

            MessageBox.Show("🔁 Odontograma y prótesis recargados correctamente.",
                "Recarga completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region === ODONTOGRAMA ===
        private void GenerarOdontograma()
        {
            pnlOdontograma.Controls.Clear();
            pnlOdontograma.SuspendLayout();

            int anchoPanel = pnlOdontograma.Width;
            int centroX = anchoPanel / 2;
            int espacioEntreDientes = 58;
            int topInicial = 40;

            // === Arcos adultos ===
            int[] supIzq = { 18, 17, 16, 15, 14, 13, 12, 11 };
            int[] supDer = { 21, 22, 23, 24, 25, 26, 27, 28 };
            int[] infIzq = { 48, 47, 46, 45, 44, 43, 42, 41 };
            int[] infDer = { 31, 32, 33, 34, 35, 36, 37, 38 };

            // === Arcos temporales ===
            int[] supTempIzq = { 55, 54, 53, 52, 51 };
            int[] supTempDer = { 61, 62, 63, 64, 65 };
            int[] infTempIzq = { 85, 84, 83, 82, 81 };
            int[] infTempDer = { 71, 72, 73, 74, 75 };

            // === Coordenadas base centradas ===
            int offsetY = topInicial;
            int alturaFila = 80;
            int espacioEntreArcos = 150;

            int anchoArcoAdulto = supIzq.Length * espacioEntreDientes;
            int inicioIzquierda = centroX - anchoArcoAdulto - 50;
            int inicioDerecha = centroX + 60;

            // === Superior Adultos ===
            CrearFilaArco(supIzq, inicioIzquierda, offsetY);
            CrearFilaArco(supDer, inicioDerecha, offsetY);

            // === Superior Temporales ===
            CrearFilaArco(supTempIzq, inicioIzquierda + 110, offsetY + alturaFila);
            CrearFilaArco(supTempDer, inicioDerecha + 45, offsetY + alturaFila);

            // === Inferior Adultos ===
            CrearFilaArco(infIzq, inicioIzquierda, offsetY + espacioEntreArcos + alturaFila);
            CrearFilaArco(infDer, inicioDerecha, offsetY + espacioEntreArcos + alturaFila);

            // === Inferior Temporales ===
            CrearFilaArco(infTempIzq, inicioIzquierda + 110, offsetY + espacioEntreArcos + (alturaFila * 2));
            CrearFilaArco(infTempDer, inicioDerecha + 45, offsetY + espacioEntreArcos + (alturaFila * 2));


            // === Líneas divisorias ===
            pnlOdontograma.Paint += (s, e) =>
            {
                DibujarProtesis(e.Graphics);
                using (Pen pen = new Pen(Color.LightGray, 1.5f))
                {
                    pen.DashStyle = DashStyle.Dash;

                    int midX = pnlOdontograma.Width / 2;
                    int midY = offsetY + espacioEntreArcos + 50;

                    e.Graphics.DrawLine(pen, midX, 0, midX, pnlOdontograma.Height);
                    e.Graphics.DrawLine(pen, 0, midY, pnlOdontograma.Width, midY);
                }
            };

            pnlOdontograma.ResumeLayout();
        }

        private void DibujarProtesis(Graphics g)
        {
            if (protesisLista.Count == 0)
                return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var p in protesisLista)
            {
                Color colorBase = (p.Estado == "Por Realizar")
                    ? Color.Red
                    : Color.DodgerBlue;

                // Dibuja según el tipo de prótesis
                if (p.Tipo == "Superior Total")
                    DibujarLineaProtesis(g, colorBase, 65);
                else if (p.Tipo == "Inferior Total")
                    DibujarLineaProtesis(g, colorBase, 295);
                else if (p.Tipo == "Removible Parcial" && p.Inicio > 0)
                    DibujarRemovible(g, p.Inicio, p.Fin, colorBase);
            }
        }

        private void DibujarLineaProtesis(Graphics g, Color color, int yCentro)
        {
            int separacion = 6;
            using (Pen p = new Pen(color, 3))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawLine(p, 20, yCentro - separacion, pnlOdontograma.Width - 20, yCentro - separacion);
                g.DrawLine(p, 20, yCentro + separacion, pnlOdontograma.Width - 20, yCentro + separacion);
            }
        }

        private void AgregarTituloCuadrante(string texto, int x, int y)
        {
            Label lbl = new Label()
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 120)
            };
            pnlOdontograma.Controls.Add(lbl);
        }
        private void CrearFilaArco(int[] dientes, int startX, int startY)
        {
            int x = startX;
            foreach (int numero in dientes)
            {
                Panel pieza = new Panel()
                {
                    Width = 48,
                    Height = 48,
                    Left = x,
                    Top = startY,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = new PiezaTag { NumeroDiente = numero }
                };

                //Dibujo del marco del diente
                pieza.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen pen = new Pen(Color.FromArgb(100, 100, 100), 1))
                    {
                        //Borde exterior
                        Rectangle rectExterior = new Rectangle(0, 0, pieza.Width - 1, pieza.Height - 1);
                        e.Graphics.DrawRectangle(pen, rectExterior);

                        //Cuadro interior
                        int margenInterior = (int)(pieza.Width * 0.33);
                        Rectangle rectInterior = new Rectangle(
                            margenInterior,
                            margenInterior,
                            pieza.Width - 2 * margenInterior,
                            pieza.Height - 2 * margenInterior);
                        e.Graphics.DrawRectangle(pen, rectInterior);

                        // Diagonales
                        e.Graphics.DrawLine(pen, rectExterior.Left, rectExterior.Top, rectInterior.Left, rectInterior.Top);
                        e.Graphics.DrawLine(pen, rectExterior.Right, rectExterior.Top, rectInterior.Right, rectInterior.Top);
                        e.Graphics.DrawLine(pen, rectExterior.Left, rectExterior.Bottom, rectInterior.Left, rectInterior.Bottom);
                        e.Graphics.DrawLine(pen, rectExterior.Right, rectExterior.Bottom, rectInterior.Right, rectInterior.Bottom);
                    }

                    //DIBUJAR OVERLAY DE PIEZA COMPLETA (Corona, X, Triángulo)
                    if (pieza.Tag is PiezaTag info && info.Estado.Overlay != "None")
                    {
                        DibujarOverlay(e.Graphics, pieza, info.Estado.Overlay);
                    }
                };


                int margin = 6;
                Rectangle inner = new Rectangle(
                    (int)(pieza.Width * 0.33),
                    (int)(pieza.Height * 0.33),
                    (int)(pieza.Width * 0.34),
                    (int)(pieza.Height * 0.34)
                );

                // 🔹 Crear caras poligonales
                foreach (var cara in new[] { "V", "O", "M", "D", "L" })
                {
                    Button btn = CrearBotonCaraPoligonal(cara, inner, margin, pieza.Width, pieza.Height);
                    pieza.Controls.Add(btn);
                }

                // ⚙️ Menú general (piezas completas) solo si clic derecho en el panel
                pieza.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        if (pieza.GetChildAtPoint(e.Location) == null)
                        {
                            pieza.ContextMenuStrip = CrearMenuContextual(pieza);
                            pieza.ContextMenuStrip.Show(pieza, e.Location);
                        }
                    }
                };


                pnlOdontograma.Controls.Add(pieza);

                // 🔹 Etiqueta con número del diente
                Label lblNum = new Label()
                {
                    Text = numero.ToString(),
                    AutoSize = false,
                    Width = 30,
                    Left = pieza.Left + (pieza.Width - 30) / 2,
                    Top = pieza.Bottom + 2,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 60, 130)
                };
                pnlOdontograma.Controls.Add(lblNum);

                x += 58;
            }
        }
        #endregion

        private void BtnProtesis_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmSeleccionProtesis())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    AplicarProtesis(frm.TipoSeleccionado, frm.Inicio, frm.Fin, frm.EstadoSeleccionado);
                }

            }
        }

        private void AplicarProtesis(string tipo, int inicio, int fin, string estado)
        {
            //Verificar si es una prótesis removible
            if (tipo == "Removible Parcial")
            {
                // Permitir varias removibles con distinto rango
                bool yaExisteMismoRango = protesisLista.Any(p =>
                    p.Tipo == "Removible Parcial" &&
                    p.Inicio == inicio &&
                    p.Fin == fin);

                if (yaExisteMismoRango)
                {
                    // Si ya existe ese mismo rango, solo actualizar estado
                    int idx = protesisLista.FindIndex(p =>
                        p.Tipo == "Removible Parcial" &&
                        p.Inicio == inicio &&
                        p.Fin == fin);

                    protesisLista[idx] = (tipo, inicio, fin, estado);
                    MessageBox.Show($"✅ Estado actualizado para removible ({inicio}-{fin}): {estado}",
                        "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Nueva removible
                    protesisLista.Add((tipo, inicio, fin, estado));
                    MessageBox.Show($"✅ Se registró prótesis removible ({inicio}-{fin}) - {estado}.",
                        "Prótesis agregada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                pnlOdontograma.Refresh();
                return;
            }

            //Para prótesis total (superior/inferior)
            var existente = protesisLista.FirstOrDefault(p => p.Tipo == tipo);

            if (!string.IsNullOrEmpty(existente.Tipo))
            {
                // Actualizar estado si ya existe
                int index = protesisLista.FindIndex(p => p.Tipo == tipo);
                protesisLista[index] = (tipo, inicio, fin, estado);

                pnlOdontograma.Refresh();
                MessageBox.Show($"✅ Estado actualizado: {tipo} ({estado}).",
                    "Actualización", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Nueva prótesis total
            protesisLista.Add((tipo, inicio, fin, estado));
            pnlOdontograma.Refresh();

            MessageBox.Show($"✅ Se registró una {tipo} ({estado}).",
                "Prótesis agregada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SeleccionarRangoRemovible(out int inicio, out int fin)
        {
            inicio = fin = 0;
            string sInicio = Interaction.InputBox("Ingrese el número del primer diente:", "Prótesis Removible", "41");
            string sFin = Interaction.InputBox("Ingrese el número del último diente:", "Prótesis Removible", "48");

            if (int.TryParse(sInicio, out int d1) && int.TryParse(sFin, out int d2))
            {
                inicio = Math.Min(d1, d2);
                fin = Math.Max(d1, d2);
            }
            else
            {
                MessageBox.Show("❌ Números inválidos de dientes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DibujarRemovible(Graphics g, int inicio, int fin, Color color)
        {
            var piezas = pnlOdontograma.Controls.OfType<Panel>()
                .Where(p => p.Tag is PiezaTag tag && tag.NumeroDiente >= inicio && tag.NumeroDiente <= fin)
                .OrderBy(p => ((PiezaTag)p.Tag).NumeroDiente)
                .ToList();

            if (piezas.Count < 2) return;

            var primera = piezas.First();
            var ultima = piezas.Last();
            int y = (primera.Top + ultima.Bottom) / 2;

            using (Pen pen = new Pen(color, 5))
            {
                pen.DashStyle = DashStyle.DashDot;
                g.DrawLine(pen, primera.Left, y, ultima.Right, y);
            }
        }
        private Button CrearBotonCaraPoligonal(string nombre, Rectangle inner, int margin, int w, int h)
        {
            Point[] puntos;

            switch (nombre)
            {
                case "V":
                    puntos = new[]
                    {
                new Point(0, 0),
                new Point(w, 0),
                new Point(inner.Right, inner.Top),
                new Point(inner.Left, inner.Top)
            };
                    break;

                case "O":
                    puntos = new[]
                    {
                new Point(inner.Left, inner.Bottom),
                new Point(inner.Right, inner.Bottom),
                new Point(w, h),
                new Point(0, h)
            };
                    break;

                case "M":
                    puntos = new[]
                    {
                new Point(0, 0),
                new Point(inner.Left, inner.Top),
                new Point(inner.Left, inner.Bottom),
                new Point(0, h)
            };
                    break;

                case "D":
                    puntos = new[]
                    {
                new Point(inner.Right, inner.Top),
                new Point(w, 0),
                new Point(w, h),
                new Point(inner.Right, inner.Bottom)
            };
                    break;

                default:
                    puntos = new[]
                    {
                new Point(inner.Left, inner.Top),
                new Point(inner.Right, inner.Top),
                new Point(inner.Right, inner.Bottom),
                new Point(inner.Left, inner.Bottom)
            };
                    break;
            }

            int minX = puntos.Min(p => p.X);
            int minY = puntos.Min(p => p.Y);
            int maxX = puntos.Max(p => p.X);
            int maxY = puntos.Max(p => p.Y);

            Rectangle bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            Point[] local = puntos.Select(p => new Point(p.X - minX, p.Y - minY)).ToArray();

            Button btn = new Button()
            {
                Bounds = bounds,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Tag = new FaceState(),
                Text = "",
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(local);
            btn.Region = new Region(path);

            //Menú contextual individual
            btn.ContextMenuStrip = CrearMenuContextual(btn);

            //Dibujo de color
            btn.Paint += (s, e) =>
            {
                var st = (FaceState)btn.Tag;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush b = new SolidBrush(st.FillColor == Color.White ? Color.Transparent : st.FillColor))
                    e.Graphics.FillPolygon(b, local);
                if (st.Overlay != "None")
                    DibujarOverlayCara(e.Graphics, btn.ClientRectangle, st.Overlay);
            };

            btn.BringToFront();
            return btn;
        }
        #region === CONTEXTUALES ===

        private ContextMenuStrip CrearMenuContextual(Button btn)
        {
            var st = (FaceState)btn.Tag;
            ContextMenuStrip menu = new ContextMenuStrip();

            // ================================
            // OPCIONES PARA CADA CARA INDIVIDUAL
            // ================================
            menu.Items.Add("Restaurado (Azul)", null, (s, e) =>
            {
                st.FillColor = Color.SkyBlue;
                st.Overlay = "None";
                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            menu.Items.Add("Por Restaurar (Rojo)", null, (s, e) =>
            {
                st.FillColor = Color.IndianRed;
                st.Overlay = "None";
                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            menu.Items.Add(new ToolStripSeparator());

            // ================================
            // OPCIONES DE PIEZA COMPLETA (CORONAS)
            // ================================
            menu.Items.Add("Con Corona (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "CoronaAzul";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Requiere Corona (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "CoronaRojo";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add(new ToolStripSeparator());

            // ================================
            // ESTADOS GENERALES (EXTRACCIONES / ENDODONCIAS)
            // ================================
            menu.Items.Add("Pieza Extraída (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "XBlue";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Por Extraer (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "XRed";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Endodoncia Realizada (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "TriBlue";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Endodoncia por Realizar (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "TriRed";
                    pieza.Invalidate();
                }
            });

            // ================================
            // Recidiva de caries
            // ================================
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Recidiva de Caries", null, (s, e) =>
            {
                st.FillColor = Color.Transparent;
                st.Overlay = "RecidivaCaries";
                btn.Invalidate();
            });


            menu.Items.Add(new ToolStripSeparator());

            // ================================
            // LIMPIAR (RESETEA TODO)
            // ================================
            menu.Items.Add("Limpiar", null, (s, e) =>
            {
                st.FillColor = Color.White;
                st.Overlay = "None";

                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                    info.Estado.Overlay = "None";

                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            return menu;
        }

        private ContextMenuStrip CrearMenuContextual(Panel pieza)
        {
            PiezaTag tag = pieza.Tag as PiezaTag;
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add("Con Corona (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "CoronaAzul";
                pieza.Invalidate();
            });

            menu.Items.Add("Requiere Corona (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "CoronaRojo";
                pieza.Invalidate();
            });

            menu.Items.Add("Pieza Extraída (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "XBlue";
                pieza.Invalidate();
            });

            menu.Items.Add("Por Extraer (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "XRed";
                pieza.Invalidate();
            });

            menu.Items.Add("Endodoncia Realizada (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "TriBlue";
                pieza.Invalidate();
            });

            menu.Items.Add("Endodoncia por Realizar (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "TriRed";
                pieza.Invalidate();
            });

            menu.Items.Add("Limpiar", null, (s, e) =>
            {
                tag.Estado.Overlay = "None";
                pieza.Invalidate();
            });

            return menu;
        }
        #endregion

        #region === DIBUJO ===
        private void DibujarOverlay(Graphics g, Panel pieza, string tipo)
        {
            int w = pieza.Width, h = pieza.Height;

            if (tipo.StartsWith("X"))
            {
                using (Pen p = new Pen(tipo == "XBlue" ? Color.Blue : Color.Red, 2))
                {
                    g.DrawLine(p, 5, 5, w - 5, h - 5);
                    g.DrawLine(p, w - 5, 5, 5, h - 5);
                }
            }
            else if (tipo.StartsWith("Tri"))
            {
                using (SolidBrush br = new SolidBrush(tipo == "TriBlue" ? Color.Blue : Color.Red))
                {
                    Point[] pts = { new Point(w / 2, 8), new Point(6, h - 8), new Point(w - 6, h - 8) };
                    g.FillPolygon(br, pts);
                }
            }
            else if (tipo == "CoronaAzul" || tipo == "CoronaRojo")
            {
                using (Pen p = new Pen(tipo == "CoronaAzul" ? Color.Blue : Color.Red, 6))
                {
                    p.Alignment = PenAlignment.Inset; 
                    g.DrawRectangle(p, 3, 3, w - 6, h - 6);
                }
            }
        }
        #endregion
        private void DibujarOverlayCara(Graphics g, Rectangle bounds, string tipo)
        {
            if (tipo == "XBlue" || tipo == "XRed")
            {
                using (Pen p = new Pen(tipo == "XBlue" ? Color.Blue : Color.Red, 2))
                {
                    g.DrawLine(p, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
                    g.DrawLine(p, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
                }
            }
            else if (tipo == "TriBlue" || tipo == "TriRed")
            {
                using (SolidBrush br = new SolidBrush(tipo == "TriBlue" ? Color.Blue : Color.Red))
                {
                    Point mid = new Point(bounds.Left + bounds.Width / 2, bounds.Top + 4);
                    Point left = new Point(bounds.Left + 4, bounds.Bottom - 4);
                    Point right = new Point(bounds.Right - 4, bounds.Bottom - 4);
                    g.FillPolygon(br, new Point[] { mid, left, right });
                }
            }

            else if (tipo == "RecidivaCaries")
            {
                //Gradiente de borde rojo → centro azul más equilibrado
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddRectangle(bounds);
                    using (PathGradientBrush brush = new PathGradientBrush(path))
                    {
                        // Centro azul más grande y suave
                        brush.CenterColor = Color.FromArgb(200, 100, 170, 255); // Azul translúcido más visible
                        brush.SurroundColors = new Color[] { Color.FromArgb(120, 255, 0, 0) }; // Rojo más tenue

                        // Gradiente más amplio (azul domina más)
                        brush.CenterPoint = new PointF(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                        g.FillRectangle(brush, bounds);
                    }
                }

                //Borde sutil
                using (Pen borde = new Pen(Color.FromArgb(180, 255, 60, 60), 1.2f))
                    g.DrawRectangle(borde, bounds);
            }


        }
        #region === BD Y GUARDADO ===
        private void CrearTablaOdontogramaSiNoExiste()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Odontograma (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PacienteId INTEGER NOT NULL,
                        Diente INTEGER NOT NULL,
                        Cara TEXT NOT NULL,
                        Estado TEXT,
                        Color TEXT,
                        Overlay TEXT,
                        FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (PacienteId) REFERENCES Paciente(Id)
                    );", conn);
                cmd.ExecuteNonQuery();
            }
        }
        private void CrearTablaProtesisSiNoExiste()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
        CREATE TABLE IF NOT EXISTS Protesis (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PacienteId INTEGER NOT NULL,
            Tipo TEXT,
            Inicio INTEGER,
            Fin INTEGER,
            Estado TEXT,
            FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (PacienteId) REFERENCES Paciente(Id)
        );", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                conn.Close();
            }

        }
        private void GuardarOdontograma(int pacienteId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        //Limpiar registros previos
                        var deleteOdonto = new SQLiteCommand("DELETE FROM Odontograma WHERE PacienteId = @id", conn, transaction);
                        deleteOdonto.Parameters.AddWithValue("@id", pacienteId);
                        deleteOdonto.ExecuteNonQuery();

                        var deleteProt = new SQLiteCommand("DELETE FROM Protesis WHERE PacienteId = @id", conn, transaction);
                        deleteProt.Parameters.AddWithValue("@id", pacienteId);
                        deleteProt.ExecuteNonQuery();

                        bool seGuardoAlgo = false;

                        //Guardar odontograma
                        foreach (Control control in pnlOdontograma.Controls)
                        {
                            if (control is Panel pieza && pieza.Tag is PiezaTag info)
                            {
                                int dienteNumero = info.NumeroDiente;

                                //Guardar estados generales (coronas, extracciones, etc.)
                                if (info.Estado != null && info.Estado.Overlay != "None")
                                {
                                    seGuardoAlgo = true;
                                    var cmdGeneral = new SQLiteCommand(@"
                                INSERT INTO Odontograma 
                                (PacienteId, Diente, Cara, Estado, Color, Overlay)
                                VALUES (@PacienteId, @Diente, 'General', 'Pieza Completa', @Color, @Overlay)", conn, transaction);
                                    cmdGeneral.Parameters.AddWithValue("@PacienteId", pacienteId);
                                    cmdGeneral.Parameters.AddWithValue("@Diente", dienteNumero);
                                    cmdGeneral.Parameters.AddWithValue("@Color", info.Estado.BorderColor.Name);
                                    cmdGeneral.Parameters.AddWithValue("@Overlay", info.Estado.Overlay);
                                    cmdGeneral.ExecuteNonQuery();
                                }

                                //Guardar cada cara modificada
                                foreach (Control c in pieza.Controls)
                                {
                                    if (c is Button btn && btn.Tag is FaceState st)
                                    {
                                        if (st.FillColor != Color.White || st.Overlay != "None")
                                        {
                                            seGuardoAlgo = true;
                                            var insertCmd = new SQLiteCommand(@"
                                        INSERT INTO Odontograma 
                                        (PacienteId, Diente, Cara, Estado, Color, Overlay)
                                        VALUES (@PacienteId, @Diente, @Cara, 'Cara Modificada', @Color, @Overlay)", conn, transaction);
                                            insertCmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                                            insertCmd.Parameters.AddWithValue("@Diente", dienteNumero);
                                            insertCmd.Parameters.AddWithValue("@Cara", btn.Text);
                                            insertCmd.Parameters.AddWithValue("@Color", st.FillColor.Name);
                                            insertCmd.Parameters.AddWithValue("@Overlay", st.Overlay);
                                            insertCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }

                        //Guardar prótesis
                       foreach (var p in protesisLista)
                        {
                            var insertProt = new SQLiteCommand(@"
        INSERT INTO Protesis (PacienteId, Tipo, Inicio, Fin, Estado)
        VALUES (@id, @tipo, @inicio, @fin, @estado)", conn, transaction);
                            insertProt.Parameters.AddWithValue("@id", pacienteId);
                            insertProt.Parameters.AddWithValue("@tipo", p.Tipo);
                            insertProt.Parameters.AddWithValue("@inicio", p.Inicio);
                            insertProt.Parameters.AddWithValue("@fin", p.Fin);
                            insertProt.Parameters.AddWithValue("@estado", p.Estado);
                            insertProt.ExecuteNonQuery();
                            seGuardoAlgo = true;
                        }

                        //Confirmar o revertir
                        if (seGuardoAlgo)
                        {
                            transaction.Commit();
                            MessageBox.Show("✅ Odontograma y/o prótesis guardados correctamente.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("⚠️ No se detectaron cambios para guardar.",
                                "Sin cambios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"❌ Error al guardar odontograma: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion
    }
}
