// ============================================================
// Controladores del Servidor
// Conectan las Vistas con los Servicios
// Principio MVC: los controladores no tienen logica de negocio
// Principio SRP: cada controlador maneja solo su area
// ============================================================
using BUSUH_Servidor.Models;
using BUSUH_Servidor.Services;

namespace BUSUH_Servidor.Controllers
{
    // ── ViajeController ──────────────────────────────────────
    public class ViajeController
    {
        private readonly ViajeService          _viajeService;
        private readonly TerminalClientService _terminalClient;

        public ViajeController(ViajeService viajeService,
                               TerminalClientService terminalClient)
        {
            _viajeService   = viajeService;
            _terminalClient = terminalClient;
        }

        public (bool ok, string mensaje) Grabar(int codigo, string descripcion,
            string salida, string llegada, int capacidad, double costo)
        {
            var viaje = new ClsViaje
            {
                Codigo          = codigo,
                Descripcion     = descripcion.Trim().ToUpper(),
                TerminalSalida  = salida,
                TerminalLlegada = llegada,
                Capacidad       = capacidad,
                Costo           = costo
            };
            return _viajeService.Grabar(viaje);
        }

        public (bool ok, string mensaje) Eliminar(int codigo) =>
            _viajeService.Eliminar(codigo);

        public ClsViaje?      BuscarPorCodigo(int codigo) =>
            _viajeService.BuscarPorCodigo(codigo);

        public List<ClsViaje> ObtenerTodos() =>
            _viajeService.ObtenerTodos();

        // Pedir la lista de terminales al programa BUSUH_Terminales
        public (bool ok, string mensaje, List<ClsTerminal> terminales)
            CargarTerminales(string host)
        {
            var lista = _terminalClient.ObtenerTerminales(host);
            if (lista.Count == 0)
                return (false,
                    $"No se pudo conectar a BUSUH_Terminales en {host}:{TerminalClientService.PUERTO}.\n" +
                    "Verifique que el programa este corriendo y el servicio activo.",
                    lista);

            return (true, $"Se cargaron {lista.Count} terminales.", lista);
        }
    }

    // ── SocketController ─────────────────────────────────────
    public class SocketController
    {
        private readonly SocketServerService _socketService;

        public bool EstaActivo         => _socketService.EstaActivo;
        public int  ClientesConectados => _socketService.ClientesConectados;

        public event Action<string>? OnLog
        {
            add    => _socketService.OnLog += value;
            remove => _socketService.OnLog -= value;
        }

        public event Action<int>? OnClientesActualizados
        {
            add    => _socketService.OnClientesActualizados += value;
            remove => _socketService.OnClientesActualizados -= value;
        }

        public event Action? OnReservaExitosa
        {
            add    => _socketService.OnReservaExitosa += value;
            remove => _socketService.OnReservaExitosa -= value;
        }

        public SocketController(SocketServerService socketService)
        {
            _socketService = socketService;
        }

        public (bool ok, string mensaje) Iniciar() => _socketService.Iniciar();
        public void Detener()                       => _socketService.Detener();
    }
}
