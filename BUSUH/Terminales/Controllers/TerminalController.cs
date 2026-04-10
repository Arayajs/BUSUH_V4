// ============================================================
// Controlador: TerminalController
// Conecta la Vista (FrmTerminales) con los Servicios
// Principio SRP: solo coordina, no tiene logica de negocio
// Principio MVC: el controlador es el intermediario
// ============================================================
using BUSUH_Terminales.Models;
using BUSUH_Terminales.Services;

namespace BUSUH_Terminales.Controllers
{
    public class TerminalController
    {
        private readonly TerminalService       _terminalService;
        private readonly TerminalSocketService _socketService;

        public bool ServicioActivo => _socketService.EstaActivo;

        // Eventos para que la vista pueda suscribirse al log del socket
        public event Action<string>? OnLog
        {
            add    => _socketService.OnLog += value;
            remove => _socketService.OnLog -= value;
        }

        public TerminalController(TerminalService terminalService,
                                  TerminalSocketService socketService)
        {
            _terminalService = terminalService;
            _socketService   = socketService;
        }

        public (bool ok, string mensaje) Grabar(int codigo, string nombre, string descripcion)
        {
            var terminal = new ClsTerminal(codigo, nombre.Trim().ToUpper(), descripcion.Trim());
            return _terminalService.Grabar(terminal);
        }

        public (bool ok, string mensaje) Eliminar(int codigo) =>
            _terminalService.Eliminar(codigo);

        public (bool ok, string mensaje) IniciarServicio() =>
            _socketService.Iniciar();

        public void DetenerServicio() =>
            _socketService.Detener();

        public ClsTerminal?      BuscarPorCodigo(int codigo) =>
            _terminalService.BuscarPorCodigo(codigo);

        public List<ClsTerminal> ObtenerTodos() =>
            _terminalService.ObtenerTodos();
    }
}
