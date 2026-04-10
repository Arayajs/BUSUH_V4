// ============================================================
// Servicio: TerminalService
// Contiene la logica de negocio para terminales
// Principio SRP: solo maneja reglas de negocio de terminales
// Principio DIP: depende de ITerminalRepository, no de la clase concreta
// ============================================================
using BUSUH_Terminales.Models;
using BUSUH_Terminales.Repositories;

namespace BUSUH_Terminales.Services
{
    public class TerminalService
    {
        private readonly ITerminalRepository _repo;

        public TerminalService(ITerminalRepository repo)
        {
            _repo = repo;
        }

        public (bool ok, string mensaje) Grabar(ClsTerminal terminal)
        {
            if (terminal.Codigo <= 0)
                return (false, "El codigo debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(terminal.Nombre))
                return (false, "El nombre es obligatorio.");

            if (_repo.Existe(terminal.Codigo))
            {
                _repo.Actualizar(terminal);
                return (true, "Terminal actualizada correctamente.");
            }
            else
            {
                _repo.Agregar(terminal);
                return (true, "Terminal agregada correctamente.");
            }
        }

        public (bool ok, string mensaje) Eliminar(int codigo)
        {
            if (!_repo.Existe(codigo))
                return (false, "No existe una terminal con ese codigo.");
            _repo.Eliminar(codigo);
            return (true, "Terminal eliminada correctamente.");
        }

        public ClsTerminal?      BuscarPorCodigo(int codigo) => _repo.BuscarPorCodigo(codigo);
        public List<ClsTerminal> ObtenerTodos()              => _repo.ObtenerTodos();
    }
}
