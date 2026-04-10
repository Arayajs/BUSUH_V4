// ============================================================
// Vista: FrmTerminales
// Solo se comunica con el TerminalController, nunca con los servicios directamente
// Principio MVC: la vista no tiene logica de negocio
// Composition Root: aqui se instancian todas las dependencias
// ============================================================
using BUSUH_Terminales.Controllers;
using BUSUH_Terminales.Repositories;
using BUSUH_Terminales.Services;

namespace BUSUH_Terminales.Views
{
    public class FrmTerminales : Form
    {
        // ── Composition Root (inyeccion manual de dependencias) ──────────
        private readonly TerminalController _controller;

        // ── Controles ────────────────────────────────────────────────────
        private TextBox      txtCodigo      = new TextBox();
        private TextBox      txtNombre      = new TextBox();
        private TextBox      txtDescripcion = new TextBox();
        private Button       btnGrabar      = new Button();
        private Button       btnEliminar    = new Button();
        private Button       btnLimpiar     = new Button();
        private Button       btnSalir       = new Button();
        private Button       btnIniciar     = new Button();
        private Button       btnDetener     = new Button();
        private Label        lblEstado      = new Label();
        private DataGridView dgvTerminales  = new DataGridView();
        private ListBox      lstLog         = new ListBox();

        public FrmTerminales()
        {
            // Construir el grafo de dependencias
            var repo           = new TerminalRepository();
            var terminalSvc    = new TerminalService(repo);
            var socketSvc      = new TerminalSocketService(terminalSvc);
            _controller        = new TerminalController(terminalSvc, socketSvc);

            // Suscribir el log del socket a la lista de actividad
            _controller.OnLog += msg => InvSafe(() =>
            {
                lstLog.Items.Insert(0, msg);
            });

            InicializarComponentes();
            CargarDatosIniciales();
            RefrescarGrid();
        }

        private void CargarDatosIniciales()
        {
            _controller.Grabar(1, "SAN JOSE", "Terminal Central San Jose");
            _controller.Grabar(2, "EL COCO",  "Terminal Playas del Coco");
        }

        private void InicializarComponentes()
        {
            Text            = "Mantenimiento de Terminales - BUSUH";
            Size            = new Size(750, 600);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.White;

            // Titulo
            var lblTitulo = new Label
            {
                Text      = "Mantenimiento de Terminales",
                Font      = new Font("Arial", 14, FontStyle.Bold),
                Location  = new Point(20, 15),
                AutoSize  = true,
                ForeColor = Color.DarkBlue
            };

            // Labels y campos
            CrearLabel("Codigo:",      20, 65);
            CrearLabel("Nombre:",      20, 100);
            CrearLabel("Descripcion:", 20, 135);

            txtCodigo.Location  = new Point(130, 63);
            txtCodigo.Size      = new Size(80, 23);

            txtNombre.Location  = new Point(130, 98);
            txtNombre.Size      = new Size(260, 23);
            txtNombre.CharacterCasing = CharacterCasing.Upper;

            txtDescripcion.Location  = new Point(130, 133);
            txtDescripcion.Size      = new Size(420, 23);

            // Botones CRUD
            CrearBoton(btnGrabar,   "Grabar",   130, 170, Color.SteelBlue);
            CrearBoton(btnEliminar, "Eliminar", 220, 170, Color.Firebrick);
            CrearBoton(btnLimpiar,  "Limpiar",  310, 170, Color.DimGray);
            CrearBoton(btnSalir,    "Salir",    400, 170, Color.DimGray);

            btnGrabar.Click   += BtnGrabar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => Limpiar();
            btnSalir.Click    += (s, e) => Close();

            // Seccion servicio TCP
            var lblServicio = new Label
            {
                Text     = $"Servicio TCP (Puerto {TerminalSocketService.PUERTO}):",
                Location = new Point(20, 215),
                AutoSize = true,
                Font     = new Font("Arial", 9, FontStyle.Bold)
            };

            CrearBoton(btnIniciar, "Iniciar Servicio", 20, 237, Color.SeaGreen, 130);
            CrearBoton(btnDetener, "Detener",          160, 237, Color.Firebrick, 80);
            btnDetener.Enabled = false;

            btnIniciar.Click += BtnIniciar_Click;
            btnDetener.Click += BtnDetener_Click;

            lblEstado.Text      = "Estado: INACTIVO";
            lblEstado.Location  = new Point(255, 242);
            lblEstado.AutoSize  = true;
            lblEstado.Font      = new Font("Arial", 9, FontStyle.Bold);
            lblEstado.ForeColor = Color.Red;

            // Grid de terminales
            var lblGrid = new Label
            {
                Text     = "Terminales registradas:",
                Location = new Point(20, 278),
                AutoSize = true,
                Font     = new Font("Arial", 9, FontStyle.Bold)
            };

            dgvTerminales.Location            = new Point(20, 298);
            dgvTerminales.Size                = new Size(440, 250);
            dgvTerminales.ReadOnly            = true;
            dgvTerminales.AllowUserToAddRows  = false;
            dgvTerminales.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            dgvTerminales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTerminales.BackgroundColor     = Color.White;
            dgvTerminales.CellClick          += DgvTerminales_CellClick;

            // Log de actividad
            var lblLog = new Label
            {
                Text     = "Log de actividad:",
                Location = new Point(475, 278),
                AutoSize = true,
                Font     = new Font("Arial", 9, FontStyle.Bold)
            };

            lstLog.Location  = new Point(475, 298);
            lstLog.Size      = new Size(250, 250);
            lstLog.Font      = new Font("Consolas", 8);
            lstLog.BackColor = Color.Black;
            lstLog.ForeColor = Color.LimeGreen;

            Controls.Add(lblTitulo);
            Controls.Add(txtCodigo);   Controls.Add(txtNombre);
            Controls.Add(txtDescripcion);
            Controls.Add(btnGrabar);   Controls.Add(btnEliminar);
            Controls.Add(btnLimpiar);  Controls.Add(btnSalir);
            Controls.Add(lblServicio); Controls.Add(btnIniciar);
            Controls.Add(btnDetener);  Controls.Add(lblEstado);
            Controls.Add(lblGrid);     Controls.Add(dgvTerminales);
            Controls.Add(lblLog);      Controls.Add(lstLog);
        }

        // ── Eventos ──────────────────────────────────────────────────────

        private void BtnGrabar_Click(object? s, EventArgs e)
        {
            if (!int.TryParse(txtCodigo.Text, out int codigo))
            {
                MessageBox.Show("El codigo debe ser un numero.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var (ok, mensaje) = _controller.Grabar(codigo, txtNombre.Text, txtDescripcion.Text);
            MessageBox.Show(mensaje, ok ? "Exito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) { RefrescarGrid(); Limpiar(); }
        }

        private void BtnEliminar_Click(object? s, EventArgs e)
        {
            if (!int.TryParse(txtCodigo.Text, out int codigo)) return;

            if (MessageBox.Show("¿Desea eliminar esta terminal?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (ok, mensaje) = _controller.Eliminar(codigo);
            MessageBox.Show(mensaje, ok ? "Exito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) { RefrescarGrid(); Limpiar(); }
        }

        private void BtnIniciar_Click(object? s, EventArgs e)
        {
            var (ok, mensaje) = _controller.IniciarServicio();

            if (ok)
            {
                lblEstado.Text      = $"Estado: ACTIVO (Puerto {TerminalSocketService.PUERTO})";
                lblEstado.ForeColor = Color.Green;
                btnIniciar.Enabled  = false;
                btnDetener.Enabled  = true;
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDetener_Click(object? s, EventArgs e)
        {
            _controller.DetenerServicio();
            lblEstado.Text      = "Estado: INACTIVO";
            lblEstado.ForeColor = Color.Red;
            btnIniciar.Enabled  = true;
            btnDetener.Enabled  = false;
        }

        private void DgvTerminales_CellClick(object? s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            txtCodigo.Text      = dgvTerminales.Rows[e.RowIndex].Cells[0].Value?.ToString();
            txtNombre.Text      = dgvTerminales.Rows[e.RowIndex].Cells[1].Value?.ToString();
            txtDescripcion.Text = dgvTerminales.Rows[e.RowIndex].Cells[2].Value?.ToString();
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private void RefrescarGrid()
        {
            dgvTerminales.Rows.Clear();
            dgvTerminales.Columns.Clear();
            dgvTerminales.Columns.Add("Codigo",      "Codigo");
            dgvTerminales.Columns.Add("Nombre",      "Nombre");
            dgvTerminales.Columns.Add("Descripcion", "Descripcion");

            foreach (var t in _controller.ObtenerTodos())
                dgvTerminales.Rows.Add(t.Codigo, t.Nombre, t.Descripcion);
        }

        private void Limpiar()
        {
            txtCodigo.Text = txtNombre.Text = txtDescripcion.Text = "";
            txtCodigo.Focus();
        }

        private void CrearLabel(string texto, int x, int y)
        {
            Controls.Add(new Label
            {
                Text     = texto,
                Location = new Point(x, y + 3),
                AutoSize = true,
                Font     = new Font("Arial", 10, FontStyle.Bold)
            });
        }

        private static void CrearBoton(Button btn, string texto, int x, int y,
                                        Color color, int ancho = 83)
        {
            btn.Text      = texto;
            btn.Location  = new Point(x, y);
            btn.Size      = new Size(ancho, 28);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
        }

        private void InvSafe(Action accion)
        {
            if (InvokeRequired) Invoke(accion);
            else accion();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _controller.DetenerServicio();
            base.OnFormClosed(e);
        }
    }
}
