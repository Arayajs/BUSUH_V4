// ============================================================
// Vista: FrmServidor
// Solo se comunica con ViajeController y SocketController
// No tiene logica de negocio (Principio MVC)
// Composition Root: aqui se construye el grafo de dependencias
// ============================================================
using BUSUH_Servidor.Controllers;
using BUSUH_Servidor.Models;
using BUSUH_Servidor.Repositories;
using BUSUH_Servidor.Services;

namespace BUSUH_Servidor.Views
{
    public class FrmServidor : Form
    {
        // ── Composition Root ─────────────────────────────────
        private readonly ViajeController  _viajeCtrl;
        private readonly SocketController _socketCtrl;

        // ── Controles - Viajes ───────────────────────────────
        private TextBox      txtCodigo    = new TextBox();
        private TextBox      txtDesc      = new TextBox();
        private ComboBox     cboSalida    = new ComboBox();
        private ComboBox     cboLlegada   = new ComboBox();
        private TextBox      txtCapacidad = new TextBox();
        private TextBox      txtCosto     = new TextBox();
        private Label        lblDisp      = new Label();
        private TextBox      txtHostTerms = new TextBox();
        private Button       btnCargar    = new Button();
        private Button       btnGrabar    = new Button();
        private Button       btnEliminar  = new Button();
        private Button       btnLimpiar   = new Button();
        private DataGridView dgvViajes    = new DataGridView();
        private DataGridView dgvAsientos  = new DataGridView();
        private int          intViajeSeleccionado = -1;

        // ── Controles - Socket ───────────────────────────────
        private Label   lblClientes   = new Label();
        private Label   lblEstSocket  = new Label();
        private Button  btnIniciar    = new Button();
        private Button  btnDetener    = new Button();
        private ListBox lstBitacora   = new ListBox();

        public FrmServidor()
        {
            // Construir el grafo de dependencias
            var repo           = new ViajeRepository();
            var viajeSvc       = new ViajeService(repo);
            var terminalClient = new TerminalClientService();
            var socketSvc      = new SocketServerService(viajeSvc);

            _viajeCtrl  = new ViajeController(viajeSvc, terminalClient);
            _socketCtrl = new SocketController(socketSvc);

            // Suscribir eventos del socket a la UI
            _socketCtrl.OnLog += msg =>
                InvSafe(() => lstBitacora.Items.Insert(0, msg));

            _socketCtrl.OnClientesActualizados += n =>
                InvSafe(() => lblClientes.Text = n.ToString());

            // Cuando llega una reserva exitosa, refrescar el grid y el mapa de asientos
            _socketCtrl.OnReservaExitosa += () =>
                InvSafe(() =>
                {
                    RefrescarGridViajes();
                    if (intViajeSeleccionado >= 0)
                        RefrescarMapaAsientos(intViajeSeleccionado);
                });

            InicializarComponentes();
            CargarDatosIniciales();
            RefrescarGridViajes();
        }

        private void CargarDatosIniciales()
        {
            _viajeCtrl.Grabar(1, "SAN JOSE - EL COCO", "SAN JOSE", "EL COCO", 67, 3500);
            _viajeCtrl.Grabar(2, "EL COCO - SAN JOSE", "EL COCO", "SAN JOSE", 67, 4000);
        }

        private void InicializarComponentes()
        {
            Text            = "BUSUH Servidor - Viajes y Socket";
            Size            = new Size(1050, 690);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.WhiteSmoke;

            // ── Titulo ────────────────────────────────────────
            Controls.Add(new Label
            {
                Text      = "Mantenimiento de Viajes",
                Font      = new Font("Arial", 13, FontStyle.Bold),
                Location  = new Point(10, 12),
                AutoSize  = true,
                ForeColor = Color.DarkBlue
            });

            // ── Campos del viaje ──────────────────────────────
            int lx = 10, fx = 185, rh = 36;

            CrearLabel("Codigo Viaje:",     lx, 50);
            CrearLabel("Descripcion:",      lx, 50 + rh);
            CrearLabel("Terminal Salida:",  lx, 50 + rh * 2);
            CrearLabel("Terminal Llegada:", lx, 50 + rh * 3);
            CrearLabel("Capacidad:",        lx, 50 + rh * 4);
            CrearLabel("Costo:",            lx, 50 + rh * 5);
            CrearLabel("Cap. Disponible:",  lx, 50 + rh * 6);

            txtCodigo.Location    = new Point(fx, 48);
            txtCodigo.Size        = new Size(70, 23);

            txtDesc.Location      = new Point(fx, 48 + rh);
            txtDesc.Size          = new Size(370, 23);
            txtDesc.CharacterCasing = CharacterCasing.Upper;

            cboSalida.Location      = new Point(fx, 48 + rh * 2);
            cboSalida.Size          = new Size(200, 23);
            cboSalida.DropDownStyle = ComboBoxStyle.DropDownList;

            cboLlegada.Location      = new Point(fx, 48 + rh * 3);
            cboLlegada.Size          = new Size(200, 23);
            cboLlegada.DropDownStyle = ComboBoxStyle.DropDownList;

            txtCapacidad.Location = new Point(fx, 48 + rh * 4);
            txtCapacidad.Size     = new Size(80, 23);
            txtCapacidad.Text     = "67";

            txtCosto.Location = new Point(fx, 48 + rh * 5);
            txtCosto.Size     = new Size(100, 23);

            lblDisp.Location  = new Point(fx, 52 + rh * 6);
            lblDisp.AutoSize  = true;
            lblDisp.Font      = new Font("Arial", 10, FontStyle.Bold);
            lblDisp.ForeColor = Color.DarkGreen;

            // ── Cargar terminales ─────────────────────────────
            CrearLabel("IP Terminales:", lx, 50 + rh * 7 + 5);
            txtHostTerms.Location = new Point(fx, 50 + rh * 7 + 3);
            txtHostTerms.Size     = new Size(110, 22);
            txtHostTerms.Text     = "127.0.0.1";

            CrearBoton(btnCargar, "Cargar Terminales", fx + 118, 50 + rh * 7 + 3,
                Color.DarkSlateBlue, 145);
            btnCargar.Click += BtnCargar_Click;

            // ── Botones CRUD ──────────────────────────────────
            int by = 50 + rh * 8 + 8;
            CrearBoton(btnGrabar,   "Grabar",   fx,       by, Color.SteelBlue);
            CrearBoton(btnEliminar, "Eliminar", fx + 90,  by, Color.Firebrick);
            CrearBoton(btnLimpiar,  "Limpiar",  fx + 180, by, Color.DimGray);

            btnGrabar.Click   += BtnGrabar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => Limpiar();

            // ── Grid de viajes ────────────────────────────────
            Controls.Add(new Label
            {
                Text     = "Viajes registrados:",
                Location = new Point(10, 375),
                AutoSize = true,
                Font     = new Font("Arial", 9, FontStyle.Bold)
            });

            dgvViajes.Location            = new Point(10, 395);
            dgvViajes.Size                = new Size(510, 248);
            dgvViajes.ReadOnly            = true;
            dgvViajes.AllowUserToAddRows  = false;
            dgvViajes.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            dgvViajes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvViajes.BackgroundColor     = Color.White;
            dgvViajes.CellClick          += DgvViajes_CellClick;

            // ── Grid de asientos ──────────────────────────────
            Controls.Add(new Label
            {
                Text     = "Mapa de asientos (1=disp. 0=ocup.):",
                Location = new Point(530, 375),
                AutoSize = true,
                Font     = new Font("Arial", 9, FontStyle.Bold)
            });

            dgvAsientos.Location            = new Point(530, 395);
            dgvAsientos.Size                = new Size(260, 248);
            dgvAsientos.ReadOnly            = true;
            dgvAsientos.AllowUserToAddRows  = false;
            dgvAsientos.BackgroundColor     = Color.White;

            // ── Panel Socket ──────────────────────────────────
            var panelSocket = new Panel
            {
                Location    = new Point(800, 0),
                Size        = new Size(240, 643),
                BackColor   = Color.FromArgb(28, 28, 28)
            };

            panelSocket.Controls.Add(new Label
            {
                Text      = $"Socket de Comunicacion",
                Font      = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(8, 12),
                AutoSize  = true
            });

            panelSocket.Controls.Add(new Label
            {
                Text      = "Clientes conectados:",
                ForeColor = Color.LightGray,
                Font      = new Font("Arial", 9),
                Location  = new Point(8, 40),
                AutoSize  = true
            });

            lblClientes.Text      = "0";
            lblClientes.ForeColor = Color.LimeGreen;
            lblClientes.Font      = new Font("Arial", 18, FontStyle.Bold);
            lblClientes.Location  = new Point(8, 60);
            lblClientes.AutoSize  = true;

            lblEstSocket.Text      = "Estado: INACTIVO";
            lblEstSocket.ForeColor = Color.Orange;
            lblEstSocket.Font      = new Font("Arial", 9, FontStyle.Bold);
            lblEstSocket.Location  = new Point(8, 100);
            lblEstSocket.AutoSize  = true;

            CrearBotonPanel(btnIniciar, $"▶ Iniciar (Puerto {SocketServerService.PUERTO})",
                8, 125, Color.SeaGreen, 195);
            CrearBotonPanel(btnDetener, "■ Detener", 8, 162, Color.Firebrick, 100);
            btnDetener.Enabled = false;

            btnIniciar.Click += BtnIniciar_Click;
            btnDetener.Click += BtnDetener_Click;

            panelSocket.Controls.Add(new Label
            {
                Text      = "Bitacora:",
                ForeColor = Color.LightGray,
                Font      = new Font("Arial", 9),
                Location  = new Point(8, 198),
                AutoSize  = true
            });

            lstBitacora.Location  = new Point(8, 218);
            lstBitacora.Size      = new Size(224, 410);
            lstBitacora.Font      = new Font("Consolas", 7);
            lstBitacora.BackColor = Color.Black;
            lstBitacora.ForeColor = Color.LimeGreen;

            panelSocket.Controls.Add(lblClientes);
            panelSocket.Controls.Add(lblEstSocket);
            panelSocket.Controls.Add(btnIniciar);
            panelSocket.Controls.Add(btnDetener);
            panelSocket.Controls.Add(lstBitacora);

            Controls.Add(txtCodigo);    Controls.Add(txtDesc);
            Controls.Add(cboSalida);   Controls.Add(cboLlegada);
            Controls.Add(txtCapacidad); Controls.Add(txtCosto);
            Controls.Add(lblDisp);
            Controls.Add(txtHostTerms); Controls.Add(btnCargar);
            Controls.Add(btnGrabar);    Controls.Add(btnEliminar);
            Controls.Add(btnLimpiar);
            Controls.Add(dgvViajes);    Controls.Add(dgvAsientos);
            Controls.Add(panelSocket);
        }

        // ── Eventos ──────────────────────────────────────────

        private void BtnCargar_Click(object? s, EventArgs e)
        {
            var (ok, mensaje, terminales) = _viajeCtrl.CargarTerminales(txtHostTerms.Text.Trim());

            if (!ok)
            {
                MessageBox.Show(mensaje, "Error de conexion",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selSalida  = cboSalida.Text;
            string selLlegada = cboLlegada.Text;

            cboSalida.Items.Clear();
            cboLlegada.Items.Clear();
            foreach (var t in terminales)
            {
                cboSalida.Items.Add(t.Nombre);
                cboLlegada.Items.Add(t.Nombre);
            }

            cboSalida.Text  = selSalida;
            cboLlegada.Text = selLlegada;

            MessageBox.Show(mensaje, "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGrabar_Click(object? s, EventArgs e)
        {
            if (!int.TryParse(txtCodigo.Text, out int cod) ||
                !int.TryParse(txtCapacidad.Text, out int cap) ||
                !double.TryParse(txtCosto.Text, out double costo))
            {
                MessageBox.Show("Codigo, capacidad y costo deben ser numericos.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var (ok, mensaje) = _viajeCtrl.Grabar(
                cod, txtDesc.Text,
                cboSalida.SelectedItem?.ToString() ?? "",
                cboLlegada.SelectedItem?.ToString() ?? "",
                cap, costo);

            MessageBox.Show(mensaje, ok ? "Exito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) { RefrescarGridViajes(); Limpiar(); }
        }

        private void BtnEliminar_Click(object? s, EventArgs e)
        {
            if (!int.TryParse(txtCodigo.Text, out int cod)) return;

            if (MessageBox.Show("¿Eliminar este viaje?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (ok, mensaje) = _viajeCtrl.Eliminar(cod);
            MessageBox.Show(mensaje, ok ? "Exito" : "Error",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            if (ok) { RefrescarGridViajes(); Limpiar(); }
        }

        private void BtnIniciar_Click(object? s, EventArgs e)
        {
            var (ok, mensaje) = _socketCtrl.Iniciar();

            if (ok)
            {
                lblEstSocket.Text      = $"Estado: ACTIVO (Puerto {SocketServerService.PUERTO})";
                lblEstSocket.ForeColor = Color.LimeGreen;
                btnIniciar.Enabled     = false;
                btnDetener.Enabled     = true;
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDetener_Click(object? s, EventArgs e)
        {
            _socketCtrl.Detener();
            lblEstSocket.Text      = "Estado: DETENIDO";
            lblEstSocket.ForeColor = Color.Orange;
            btnIniciar.Enabled     = true;
            btnDetener.Enabled     = false;
        }

        private void DgvViajes_CellClick(object? s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (!int.TryParse(dgvViajes.Rows[e.RowIndex].Cells[0].Value?.ToString(), out int cod))
                return;

            var viaje = _viajeCtrl.BuscarPorCodigo(cod);
            if (viaje == null) return;

            intViajeSeleccionado = cod;

            txtCodigo.Text    = viaje.Codigo.ToString();
            txtDesc.Text      = viaje.Descripcion;
            cboSalida.Text    = viaje.TerminalSalida;
            cboLlegada.Text   = viaje.TerminalLlegada;
            txtCapacidad.Text = viaje.Capacidad.ToString();
            txtCosto.Text     = viaje.Costo.ToString();
            lblDisp.Text      = viaje.CapacidadDisponible.ToString();

            RefrescarMapaAsientos(cod);
        }

        // ── Helpers ──────────────────────────────────────────

        private void RefrescarGridViajes()
        {
            dgvViajes.Rows.Clear();
            dgvViajes.Columns.Clear();
            dgvViajes.Columns.Add("Cod",    "Cod");
            dgvViajes.Columns.Add("Desc",   "Descripcion");
            dgvViajes.Columns.Add("Salida", "Salida");
            dgvViajes.Columns.Add("Llegada","Llegada");
            dgvViajes.Columns.Add("Cap",    "Cap.");
            dgvViajes.Columns.Add("Costo",  "Costo");
            dgvViajes.Columns.Add("Disp",   "Disp.");

            foreach (var v in _viajeCtrl.ObtenerTodos())
                dgvViajes.Rows.Add(v.Codigo, v.Descripcion, v.TerminalSalida,
                    v.TerminalLlegada, v.Capacidad, v.Costo.ToString("N0"),
                    v.CapacidadDisponible);
        }

        private void RefrescarMapaAsientos(int codigoViaje)
        {
            var viaje = _viajeCtrl.BuscarPorCodigo(codigoViaje);
            if (viaje == null) return;

            // Actualizar label de disponibles si el viaje sigue en pantalla
            if (txtCodigo.Text == codigoViaje.ToString())
                lblDisp.Text = viaje.CapacidadDisponible.ToString();

            dgvAsientos.Rows.Clear();
            dgvAsientos.Columns.Clear();
            dgvAsientos.Columns.Add("Fila",     "Fila");
            dgvAsientos.Columns.Add("Asientos", "Asientos");

            for (int f = 0; f < 13; f++)
                dgvAsientos.Rows.Add(f + 1, viaje.GetFilaComoTexto(f));
        }

        private void Limpiar()
        {
            txtCodigo.Text = txtDesc.Text = txtCapacidad.Text = txtCosto.Text = "";
            cboSalida.SelectedIndex = cboLlegada.SelectedIndex = -1;
            lblDisp.Text   = "";
            intViajeSeleccionado = -1;
            dgvAsientos.Rows.Clear();
            dgvAsientos.Columns.Clear();
        }

        private void CrearLabel(string texto, int x, int y) =>
            Controls.Add(new Label
            {
                Text     = texto, Location = new Point(x, y + 3),
                AutoSize = true,  Font     = new Font("Arial", 10, FontStyle.Bold)
            });

        private static void CrearBoton(Button btn, string texto, int x, int y,
                                        Color color, int ancho = 83)
        {
            btn.Text = texto; btn.Location = new Point(x, y);
            btn.Size = new Size(ancho, 28); btn.BackColor = color;
            btn.ForeColor = Color.White; btn.FlatStyle = FlatStyle.Flat;
        }

        private static void CrearBotonPanel(Button btn, string texto, int x, int y,
                                             Color color, int ancho)
        {
            btn.Text = texto; btn.Location = new Point(x, y);
            btn.Size = new Size(ancho, 30); btn.BackColor = color;
            btn.ForeColor = Color.White; btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Arial", 9, FontStyle.Bold);
        }

        private void InvSafe(Action accion)
        {
            if (InvokeRequired) Invoke(accion);
            else accion();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _socketCtrl.Detener();
            base.OnFormClosed(e);
        }
    }
}
