// ============================================================
// Servicio: TerminalSocketService
// Expone la lista de terminales al BUSUH_Servidor via TCP puerto 30001
// Principio SRP: solo maneja la comunicacion de red
// ============================================================
using System.Net;
using System.Net.Sockets;
using System.Text;
using BUSUH_Terminales.Services;

namespace BUSUH_Terminales.Services
{
    public class TerminalSocketService
    {
        public const int PUERTO = 30001;

        private TcpListener?        _listener;
        private bool                _activo = false;
        private readonly TerminalService _terminalService;

        public bool   EstaActivo => _activo;
        public event Action<string>? OnLog;

        public TerminalSocketService(TerminalService terminalService)
        {
            _terminalService = terminalService;
        }

        public (bool ok, string mensaje) Iniciar()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PUERTO);
                _listener.Start();
                _activo = true;

                Thread hilo = new Thread(EsperarConexiones);
                hilo.IsBackground = true;
                hilo.Start();

                return (true, $"Servicio iniciado en puerto {PUERTO}.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al iniciar servicio: {ex.Message}");
            }
        }

        public void Detener()
        {
            _activo = false;
            _listener?.Stop();
            Log("Servicio detenido.");
        }

        private void EsperarConexiones()
        {
            while (_activo)
            {
                try
                {
                    TcpClient cliente = _listener!.AcceptTcpClient();
                    Thread hiloCliente = new Thread(() => AtenderCliente(cliente));
                    hiloCliente.IsBackground = true;
                    hiloCliente.Start();
                }
                catch { break; }
            }
        }

        private void AtenderCliente(TcpClient cliente)
        {
            try
            {
                NetworkStream stream = cliente.GetStream();
                byte[] buffer = new byte[256];
                int n = stream.Read(buffer, 0, buffer.Length);
                string solicitud = Encoding.UTF8.GetString(buffer, 0, n).Trim();

                if (solicitud == "LIST")
                {
                    string respuesta = SerializarTerminales();
                    stream.Write(Encoding.UTF8.GetBytes(respuesta));
                    Log($"Lista enviada ({_terminalService.ObtenerTodos().Count} terminales).");
                }
                cliente.Close();
            }
            catch { }
        }

        // Formato: "codigo|nombre|descripcion;codigo|nombre|descripcion"
        private string SerializarTerminales()
        {
            var lista = _terminalService.ObtenerTodos();
            if (lista.Count == 0) return "VACIO";
            return string.Join(";", lista.Select(t => $"{t.Codigo}|{t.Nombre}|{t.Descripcion}"));
        }

        private void Log(string mensaje) =>
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
    }
}
