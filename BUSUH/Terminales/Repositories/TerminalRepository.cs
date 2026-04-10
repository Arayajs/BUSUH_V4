// ============================================================
// Repositorio: TerminalRepository
// Maneja el almacenamiento de terminales en memoria
// Principio SRP: solo se encarga de guardar y recuperar datos
// Principio DIP: el servicio depende de esta interfaz
// ============================================================
using BUSUH_Terminales.Models;

namespace BUSUH_Terminales.Repositories
{
    public interface ITerminalRepository
    {
        void              Agregar(ClsTerminal terminal);
        void              Actualizar(ClsTerminal terminal);
        void              Eliminar(int codigo);
        ClsTerminal?      BuscarPorCodigo(int codigo);
        List<ClsTerminal> ObtenerTodos();
        bool              Existe(int codigo);
    }

    // Implementacion en memoria con arreglo de objetos
    // (no usamos archivos planos ni base de datos segun el enunciado)
    public class TerminalRepository : ITerminalRepository
    {
        private readonly ClsTerminal[] arrTerminales = new ClsTerminal[100];
        private int intCantidad = 0;

        public void Agregar(ClsTerminal terminal)
        {
            arrTerminales[intCantidad] = terminal;
            intCantidad++;
        }

        public void Actualizar(ClsTerminal terminal)
        {
            int pos = BuscarPosicion(terminal.Codigo);
            if (pos >= 0) arrTerminales[pos] = terminal;
        }

        public void Eliminar(int codigo)
        {
            int pos = BuscarPosicion(codigo);
            if (pos < 0) return;
            // Correr elementos hacia atras para llenar el hueco
            for (int i = pos; i < intCantidad - 1; i++)
                arrTerminales[i] = arrTerminales[i + 1];
            arrTerminales[intCantidad - 1] = null!;
            intCantidad--;
        }

        public ClsTerminal? BuscarPorCodigo(int codigo)
        {
            int pos = BuscarPosicion(codigo);
            return pos >= 0 ? arrTerminales[pos] : null;
        }

        public List<ClsTerminal> ObtenerTodos()
        {
            var lista = new List<ClsTerminal>();
            for (int i = 0; i < intCantidad; i++)
                lista.Add(arrTerminales[i]);
            return lista;
        }

        public bool Existe(int codigo) => BuscarPosicion(codigo) >= 0;

        private int BuscarPosicion(int codigo)
        {
            for (int i = 0; i < intCantidad; i++)
                if (arrTerminales[i].Codigo == codigo) return i;
            return -1;
        }
    }
}
