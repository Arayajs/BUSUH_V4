// ============================================================
// Servicios del Servidor
// Cada clase tiene una sola responsabilidad (Principio SRP)
// ============================================================
using System.Net;
using System.Net.Sockets;
using System.Text;
using BUSUH_Servidor.DTOs;
using BUSUH_Servidor.Models;
using BUSUH_Servidor.Repositories;

namespace BUSUH_Servidor.Services
{
    // ──────────────────────────────────────────────────────────
    // ViajeService: logica de negocio de viajes y reservas
    // Thread-safe: usa lock para proteger el arreglo de asientos
    // cuando varios clientes reservan al mismo tiempo
    // ──────────────────────────────────────────────────────────
    public class ViajeService
    {
        private readonly IViajeRepository _repo;
        private readonly object _candado = new object();

        public ViajeService(IViajeRepository repo) => _repo = repo;

        public (bool ok, string mensaje) Grabar(ClsViaje viaje)
        {
            if (viaje.Codigo <= 0)                               return (false, "Codigo invalido.");
            if (string.IsNullOrWhiteSpace(viaje.Descripcion))   return (false, "Descripcion obligatoria.");
            if (string.IsNullOrWhiteSpace(viaje.TerminalSalida))return (false, "Seleccione terminal de salida.");
            if (string.IsNullOrWhiteSpace(viaje.TerminalLlegada))return (false, "Seleccione terminal de llegada.");
            if (viaje.TerminalSalida == viaje.TerminalLlegada)  return (false, "Salida y llegada no pueden ser iguales.");
            if (viaje.Capacidad <= 0)                           return (false, "La capacidad debe ser mayor a cero.");
            if (viaje.Costo <= 0)                               return (false, "El costo debe ser mayor a cero.");

            if (_repo.Existe(viaje.Codigo))
            {
                _repo.Actualizar(viaje);
                return (true, "Viaje actualizado correctamente.");
            }
            else
            {
                viaje.InicializarAsientos(); // llenar la matriz al grabar
                _repo.Agregar(viaje);
                return (true, "Viaje grabado correctamente.");
            }
        }

        public (bool ok, string mensaje) Eliminar(int codigo)
        {
            if (!_repo.Existe(codigo)) return (false, "No existe el viaje.");
            _repo.Eliminar(codigo);
            return (true, "Viaje eliminado.");
        }

        public ClsViaje?      BuscarPorCodigo(int codigo) => _repo.BuscarPorCodigo(codigo);
        public List<ClsViaje> ObtenerTodos()              => _repo.ObtenerTodos();

        // Procesar reserva de boletos con thread-safety
        public TramaResponse ProcesarReserva(TramaRequest req)
        {
            if (req.CantidadBoletos < 1 || req.CantidadBoletos > 5)
                return TramaResponse.Error(TramaResponse.EXCEDE_CANTIDAD, req.CodigoViaje);

            var viaje = _repo.BuscarPorCodigo(req.CodigoViaje);
            if (viaje == null)
                return TramaResponse.Error(TramaResponse.VIAJE_NO_EXISTE, req.CodigoViaje);

            lock (_candado)
            {
                if (viaje.CapacidadDisponible < req.CantidadBoletos)
                    return TramaResponse.Error(TramaResponse.SIN_ASIENTOS, req.CodigoViaje);

                return AsignarAsientos(viaje, req.CantidadBoletos);
            }
        }

        private TramaResponse AsignarAsientos(ClsViaje viaje, int cantidad)
        {
            int    fila1 = 0, fila2 = 0;
            string asientos1 = "00000", asientos2 = "00000";

            // Primero buscar una fila con espacio suficiente
            for (int f = 0; f < 13; f++)
            {
                if (viaje.AsientosLibresEnFila(f) >= cantidad)
                {
                    fila1     = f + 1;
                    asientos1 = OcuparEnFila(viaje, f, cantidad);
                    goto Calcular;
                }
            }

            // Si no caben en una sola fila, repartir en dos
            int restante = cantidad;
            for (int f = 0; f < 13 && restante > 0; f++)
            {
                int libres = viaje.AsientosLibresEnFila(f);
                if (libres == 0) continue;
                int asignar = Math.Min(libres, restante);
                if (fila1 == 0) { fila1 = f + 1; asientos1 = OcuparEnFila(viaje, f, asignar); }
                else            { fila2 = f + 1; asientos2 = OcuparEnFila(viaje, f, asignar); }
                restante -= asignar;
            }

            Calcular:
            viaje.CapacidadDisponible -= cantidad;
            _repo.Actualizar(viaje);

            return new TramaResponse
            {
                Verificador         = TramaResponse.EXITOSA,
                CodigoViaje         = viaje.Codigo,
                NumeroFila1         = fila1,
                AsientosFila1       = asientos1,
                NumeroFila2         = fila2,
                AsientosFila2       = asientos2,
                Monto               = (int)(viaje.Costo * cantidad),
                CapacidadDisponible = viaje.CapacidadDisponible
            };
        }

        private static string OcuparEnFila(ClsViaje viaje, int fila, int cantidad)
        {
            int    max      = (fila == 12) ? 7 : 5;
            char[] resultado = { '0', '0', '0', '0', '0' };
            int    pos      = 0;
            for (int col = 0; col < max && pos < cantidad; col++)
            {
                if (viaje.Asientos[fila, col] == 1)
                {
                    viaje.Asientos[fila, col] = 0;
                    resultado[pos++] = (char)('1' + col);
                }
            }
            return new string(resultado);
        }
    }

    // ──────────────────────────────────────────────────────────
    // TerminalClientService: consulta terminales a BUSUH_Terminales
    // Se conecta al puerto 30001 y pide la lista
    // ──────────────────────────────────────────────────────────
    public class TerminalClientService
    {
        public const int PUERTO = 30001;

        public List<ClsTerminal> ObtenerTerminales(string host)
        {
            try
            {
                using var tcp    = new TcpClient(host, PUERTO);
                using var stream = tcp.GetStream();

                stream.Write(Encoding.UTF8.GetBytes("LIST"));

                byte[] buffer = new byte[4096];
                int n = stream.Read(buffer, 0, buffer.Length);
                string respuesta = Encoding.UTF8.GetString(buffer, 0, n).Trim();

                if (respuesta == "VACIO" || string.IsNullOrEmpty(respuesta))
                    return new List<ClsTerminal>();

                return respuesta.Split(';')
                    .Select(parte =>
                    {
                        var campos = parte.Split('|');
                        return campos.Length >= 3
                            ? new ClsTerminal
                            {
                                Codigo      = int.Parse(campos[0]),
                                Nombre      = campos[1],
                                Descripcion = campos[2]
                            }
                            : null;
                    })
                    .Where(t => t != null)
                    .Cast<ClsTerminal>()
                    .ToList();
            }
            catch
            {
                return new List<ClsTerminal>();
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    // SocketServerService: servidor TCP en puerto 30000
    // Atiende las reservas de los clientes (BUSUH_Cliente)
    // ──────────────────────────────────────────────────────────
    public class SocketServerService
    {
        public const int PUERTO = 30000;

        private TcpListener?    _listener;
        private bool            _activo = false;
        private int             _clientes = 0;
        private readonly ViajeService _viajeService;

        public bool EstaActivo         => _activo;
        public int  ClientesConectados => _clientes;

        public event Action<string>? OnLog;
        public event Action<int>?    OnClientesActualizados;
        public event Action?         OnReservaExitosa;  // notifica a la vista para refrescar el grid

        public SocketServerService(ViajeService viajeService)
        {
            _viajeService = viajeService;
        }

        public (bool ok, string mensaje) Iniciar()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PUERTO);
                _listener.Start();
                _activo = true;

                Thread hilo = new Thread(AceptarClientes);
                hilo.IsBackground = true;
                hilo.Start();

                return (true, $"Servidor activo en puerto {PUERTO}.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al iniciar socket: {ex.Message}");
            }
        }

        public void Detener()
        {
            _activo = false;
            _listener?.Stop();
            Log("Servidor detenido.");
        }

        private void AceptarClientes()
        {
            while (_activo)
            {
                try
                {
                    TcpClient cliente = _listener!.AcceptTcpClient();
                    _clientes++;
                    OnClientesActualizados?.Invoke(_clientes);
                    Log($"Cliente conectado: {cliente.Client.RemoteEndPoint}");

                    Thread hilo = new Thread(() => AtenderCliente(cliente));
                    hilo.IsBackground = true;
                    hilo.Start();
                }
                catch { break; }
            }
        }

        private void AtenderCliente(TcpClient cliente)
        {
            string ep = cliente.Client.RemoteEndPoint?.ToString() ?? "?";
            try
            {
                NetworkStream stream = cliente.GetStream();
                byte[] buffer = new byte[256];

                while (_activo)
                {
                    int n = stream.Read(buffer, 0, buffer.Length);
                    if (n == 0) break;

                    string trama = Encoding.UTF8.GetString(buffer, 0, n).Trim();
                    Log($"[{ep}] Recibido: {trama}");

                    string respuesta = ProcesarTrama(trama);
                    Log($"[{ep}] Enviado: {respuesta}");

                    stream.Write(Encoding.UTF8.GetBytes(respuesta));

                    // Si la transaccion fue exitosa, avisar a la vista para refrescar
                    if (respuesta.Length > 0 && respuesta[0] == '1')
                        OnReservaExitosa?.Invoke();
                }
            }
            catch { }
            finally
            {
                cliente.Close();
                _clientes--;
                OnClientesActualizados?.Invoke(_clientes);
                Log($"[{ep}] Desconectado.");
            }
        }

        private string ProcesarTrama(string trama)
        {
            // Comando especial: el cliente pide la lista de viajes disponibles
            // Formato respuesta: "cod|descripcion|salida|llegada|costo;cod|..."
            if (trama == "VIAJES")
                return SerializarViajes();

            var req = TramaRequest.Parse(trama);
            if (req == null)
                return TramaResponse.Error(TramaResponse.FORMATO_INVALIDO).ToString();

            return _viajeService.ProcesarReserva(req).ToString();
        }

        // Serializar la lista de viajes para enviarla al cliente
        private string SerializarViajes()
        {
            var lista = _viajeService.ObtenerTodos();
            if (lista.Count == 0) return "VACIO";
            return string.Join(";",
                lista.Select(v => $"{v.Codigo}|{v.Descripcion}|{v.TerminalSalida}|{v.TerminalLlegada}|{v.Costo}"));
        }

        private void Log(string msg) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");
    }
}
