// ============================================================
// Vista: FrmCliente
// Solo se comunica con ClienteController, sin logica de negocio
// Principio MVC: la vista solo muestra datos y captura eventos
// Composition Root: aqui se instancian las dependencias
// ============================================================
using BUSUH_Cliente.Controllers;
using BUSUH_Cliente.DTOs;
using BUSUH_Cliente.Services;

namespace BUSUH_Cliente.Views
{
    public class FrmCliente : Form
    {
        // ── Composition Root ─────────────────────────────────
        private readonly ClienteController _controller;

        // Lista de viajes cargados desde el servidor
        private List<ViajeInfo> lstViajes = new List<ViajeInfo>();

        // ── Controles - Conexion ──────────────────────────────
        private TextBox txtHost        = new TextBox();
        private Button  btnConectar    = new Button();
        private Button  btnDesconectar = new Button();
        private Label   lblEstado      = new Label();

        // ── Controles - Solicitud ─────────────────────────────
        private ComboBox      cboViaje    = new ComboBox();
        private Button        btnCargar   = new Button();
        private NumericUpDown nudBoletos  = new NumericUpDown();
        private Label         lblTrama    = new Label();
        private Button        btnEnviar   = new Button();
        private Button        btnLimpiar  = new Button();

        // ── Controles - Resultado ─────────────────────────────
        private RichTextBox rtbDetalle  = new RichTextBox();
        private ListBox     lstBitacora = new ListBox();

        public FrmCliente()
        {
            _controller = new ClienteController(new SocketClientService());
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            Text            = "BUSUH Cliente - Peticiones de Boletos";
            Size            = new Size(780, 630);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.White;

            // ── Titulo ────────────────────────────────────────
            Controls.Add(new Label
            {
                Text      = "Peticiones de Cliente - BUSUH",
                Font      = new Font("Arial", 14, FontStyle.Bold),
                Location  = new Point(20, 12),
                AutoSize  = true,
                ForeColor = Color.DarkBlue
            });

            // ── Panel de Conexion ─────────────────────────────
            var panelConn = new Panel
            {
                Location    = new Point(10, 48),
                Size        = new Size(745, 55),
                BackColor   = Color.FromArgb(240, 244, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            panelConn.Controls.Add(new Label
            {
                Text = "IP del Servidor:", Location = new Point(10, 16),
                AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold)
            });

            txtHost.Location = new Point(135, 14);
            txtHost.Size     = new Size(130, 23);
            txtHost.Text     = "127.0.0.1";
            txtHost.Font     = new Font("Consolas", 10);

            CrearBoton(btnConectar,    "Conectar",    275, 13, Color.SteelBlue, 95);
            CrearBoton(btnDesconectar, "Desconectar", 378, 13, Color.Firebrick, 110);
            btnDesconectar.Enabled = false;

            lblEstado.Text      = "● Desconectado";
            lblEstado.Location  = new Point(500, 16);
            lblEstado.AutoSize  = true;
            lblEstado.Font      = new Font("Arial", 9, FontStyle.Bold);
            lblEstado.ForeColor = Color.Red;

            btnConectar.Click    += BtnConectar_Click;
            btnDesconectar.Click += BtnDesconectar_Click;

            panelConn.Controls.Add(txtHost);
            panelConn.Controls.Add(btnConectar);
            panelConn.Controls.Add(btnDesconectar);
            panelConn.Controls.Add(lblEstado);

            // ── Panel de Solicitud ────────────────────────────
            var panelSol = new Panel
            {
                Location    = new Point(10, 112),
                Size        = new Size(745, 88),
                BackColor   = Color.FromArgb(240, 244, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            panelSol.Controls.Add(new Label
            {
                Text = "Viaje:", Location = new Point(10, 14),
                AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold)
            });

            cboViaje.Location      = new Point(65, 12);
            cboViaje.Size          = new Size(310, 23);
            cboViaje.DropDownStyle = ComboBoxStyle.DropDownList;
            cboViaje.SelectedIndexChanged += (s, e) => ActualizarTrama();

            // Boton para cargar los viajes desde el servidor
            CrearBoton(btnCargar, "↺ Cargar Viajes", 385, 12, Color.DarkSlateBlue, 130);
            btnCargar.Enabled = false;
            btnCargar.Click  += BtnCargar_Click;

            panelSol.Controls.Add(new Label
            {
                Text = "Boletos (max 5):", Location = new Point(10, 48),
                AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold)
            });

            nudBoletos.Location = new Point(155, 46);
            nudBoletos.Size     = new Size(55, 23);
            nudBoletos.Minimum  = 1;
            nudBoletos.Maximum  = 5;
            nudBoletos.Value    = 1;
            nudBoletos.ValueChanged += (s, e) => ActualizarTrama();

            panelSol.Controls.Add(new Label
            {
                Text = "Trama:", Location = new Point(225, 48),
                AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold)
            });

            lblTrama.Text      = "---";
            lblTrama.Location  = new Point(280, 47);
            lblTrama.AutoSize  = true;
            lblTrama.Font      = new Font("Consolas", 12, FontStyle.Bold);
            lblTrama.ForeColor = Color.DarkBlue;
            lblTrama.BackColor = Color.LightYellow;

            CrearBoton(btnEnviar, "► Enviar Solicitud", 535, 44, Color.SeaGreen, 160);
            btnEnviar.Font    = new Font("Arial", 10, FontStyle.Bold);
            btnEnviar.Height  = 34;
            btnEnviar.Enabled = false;
            btnEnviar.Click  += BtnEnviar_Click;

            panelSol.Controls.Add(cboViaje);
            panelSol.Controls.Add(btnCargar);
            panelSol.Controls.Add(nudBoletos);
            panelSol.Controls.Add(lblTrama);
            panelSol.Controls.Add(btnEnviar);

            // ── Cabeceras ─────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Detalle de la ultima respuesta:", Location = new Point(10, 213),
                AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold)
            });

            Controls.Add(new Label
            {
                Text = "Historial de solicitudes:", Location = new Point(400, 213),
                AutoSize = true, Font = new Font("Arial", 9, FontStyle.Bold)
            });

            // ── RichTextBox Detalle ───────────────────────────
            rtbDetalle.Location    = new Point(10, 233);
            rtbDetalle.Size        = new Size(380, 345);
            rtbDetalle.ReadOnly    = true;
            rtbDetalle.BackColor   = Color.FromArgb(15, 15, 40);
            rtbDetalle.ForeColor   = Color.Cyan;
            rtbDetalle.Font        = new Font("Consolas", 10);
            rtbDetalle.BorderStyle = BorderStyle.FixedSingle;

            // ── ListBox Bitacora ──────────────────────────────
            lstBitacora.Location  = new Point(400, 233);
            lstBitacora.Size      = new Size(355, 305);
            lstBitacora.Font      = new Font("Consolas", 8);
            lstBitacora.BackColor = Color.Black;
            lstBitacora.ForeColor = Color.LimeGreen;

            CrearBoton(btnLimpiar, "Limpiar", 400, 545, Color.DimGray, 100);
            btnLimpiar.Click += (s, e) =>
            {
                rtbDetalle.Clear();
                lstBitacora.Items.Clear();
            };

            Controls.Add(panelConn);
            Controls.Add(panelSol);
            Controls.Add(rtbDetalle);
            Controls.Add(lstBitacora);
            Controls.Add(btnLimpiar);
        }

        // ── Eventos ──────────────────────────────────────────

        private void BtnConectar_Click(object? s, EventArgs e)
        {
            var (ok, mensaje) = _controller.Conectar(txtHost.Text);
            AplicarEstadoConexion(ok);
            AgregarBitacora(mensaje);

            if (!ok)
            {
                MessageBox.Show(mensaje, "Error de conexion",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Al conectar, cargar automaticamente los viajes disponibles
            CargarViajesDesdeServidor();
        }

        private void BtnDesconectar_Click(object? s, EventArgs e)
        {
            _controller.Desconectar();
            AplicarEstadoConexion(false);
            AgregarBitacora("Desconectado del servidor.");
            cboViaje.Items.Clear();
            lblTrama.Text = "---";
        }

        private void BtnCargar_Click(object? s, EventArgs e)
        {
            CargarViajesDesdeServidor();
        }

        private void BtnEnviar_Click(object? s, EventArgs e)
        {
            if (cboViaje.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar un viaje.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtener el viaje seleccionado
            var viaje   = lstViajes[cboViaje.SelectedIndex];
            int boletos = (int)nudBoletos.Value;

            var (ok, enviada, recibida, detalle) =
                _controller.EnviarSolicitud(viaje.Codigo, boletos, viaje.Descripcion);

            // Bitacora
            AgregarBitacora($"► {enviada}");
            if (!string.IsNullOrEmpty(recibida))
                AgregarBitacora($"◄ {recibida}");
            AgregarBitacora("─────────────────────────");

            // Detalle
            rtbDetalle.Clear();
            rtbDetalle.SelectionColor = Color.White;
            rtbDetalle.AppendText($"Trama enviada  : {enviada}\n");
            if (!string.IsNullOrEmpty(recibida))
                rtbDetalle.AppendText($"Trama recibida : {recibida}\n\n");

            bool exitosa = recibida.Length > 0 && recibida[0] == '1';
            rtbDetalle.SelectionColor = exitosa ? Color.LimeGreen : Color.OrangeRed;
            rtbDetalle.AppendText(detalle);
            rtbDetalle.ScrollToCaret();

            if (!_controller.EstaConectado)
                AplicarEstadoConexion(false);
        }

        // ── Helpers ──────────────────────────────────────────

        // Consultar al servidor la lista de viajes y llenar el ComboBox
        private void CargarViajesDesdeServidor()
        {
            var (ok, mensaje, viajes) = _controller.CargarViajes();

            if (!ok)
            {
                MessageBox.Show(mensaje, "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lstViajes = viajes;
            cboViaje.Items.Clear();
            foreach (var v in lstViajes)
                cboViaje.Items.Add(v.ToString());

            if (cboViaje.Items.Count > 0)
                cboViaje.SelectedIndex = 0;

            AgregarBitacora(mensaje);
        }

        private void ActualizarTrama()
        {
            if (cboViaje.SelectedIndex < 0 || lstViajes.Count == 0)
            {
                lblTrama.Text = "---";
                return;
            }
            int cod     = lstViajes[cboViaje.SelectedIndex].Codigo;
            int boletos = (int)nudBoletos.Value;
            lblTrama.Text = $"{cod:D2}{boletos}";
        }

        private void AplicarEstadoConexion(bool conectado)
        {
            lblEstado.Text         = conectado ? "● Conectado"  : "● Desconectado";
            lblEstado.ForeColor    = conectado ? Color.Green    : Color.Red;
            btnConectar.Enabled    = !conectado;
            btnDesconectar.Enabled =  conectado;
            btnEnviar.Enabled      =  conectado;
            btnCargar.Enabled      =  conectado;
        }

        private void AgregarBitacora(string mensaje) =>
            lstBitacora.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {mensaje}");

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _controller.Desconectar();
            base.OnFormClosing(e);
        }
    }
}
