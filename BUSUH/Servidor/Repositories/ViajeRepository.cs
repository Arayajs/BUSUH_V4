// ============================================================
// Repositorio: ViajeRepository
// Almacena los viajes en memoria con arreglo de objetos
// Principio SRP: solo persiste datos, no tiene logica de negocio
// ============================================================
using BUSUH_Servidor.Models;

namespace BUSUH_Servidor.Repositories
{
    public interface IViajeRepository
    {
        void         Agregar(ClsViaje viaje);
        void         Actualizar(ClsViaje viaje);
        void         Eliminar(int codigo);
        ClsViaje?    BuscarPorCodigo(int codigo);
        List<ClsViaje> ObtenerTodos();
        bool         Existe(int codigo);
    }

    public class ViajeRepository : IViajeRepository
    {
        private readonly ClsViaje[] arrViajes = new ClsViaje[50];
        private int intCantidad = 0;

        public void Agregar(ClsViaje viaje)
        {
            arrViajes[intCantidad] = viaje;
            intCantidad++;
        }

        public void Actualizar(ClsViaje viaje)
        {
            int pos = BuscarPosicion(viaje.Codigo);
            if (pos >= 0) arrViajes[pos] = viaje;
        }

        public void Eliminar(int codigo)
        {
            int pos = BuscarPosicion(codigo);
            if (pos < 0) return;
            for (int i = pos; i < intCantidad - 1; i++)
                arrViajes[i] = arrViajes[i + 1];
            arrViajes[intCantidad - 1] = null!;
            intCantidad--;
        }

        public ClsViaje? BuscarPorCodigo(int codigo)
        {
            int pos = BuscarPosicion(codigo);
            return pos >= 0 ? arrViajes[pos] : null;
        }

        public List<ClsViaje> ObtenerTodos()
        {
            var lista = new List<ClsViaje>();
            for (int i = 0; i < intCantidad; i++)
                lista.Add(arrViajes[i]);
            return lista;
        }

        public bool Existe(int codigo) => BuscarPosicion(codigo) >= 0;

        private int BuscarPosicion(int codigo)
        {
            for (int i = 0; i < intCantidad; i++)
                if (arrViajes[i].Codigo == codigo) return i;
            return -1;
        }
    }
}
