// ============================================================
// Controlador: ClienteController
// Conecta la Vista (FrmCliente) con el SocketClientService
// Principio MVC: el controlador coordina, no tiene logica de UI
// Principio SRP: solo maneja la comunicacion y la interpretacion
// ============================================================
using BUSUH_Cliente.DTOs;
using BUSUH_Cliente.Services;

namespace BUSUH_Cliente.Controllers
{
    public class ClienteController : IDisposable
    {
        private readonly SocketClientService _socketService;

        public bool EstaConectado => _socketService.EstaConectado;

        public ClienteController(SocketClientService socketService)
        {
            _socketService = socketService;
        }

        // ── Conexion ─────────────────────────────────────────

        public (bool ok, string mensaje) Conectar(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return (false, "Debe ingresar la IP del servidor.");

            return _socketService.Conectar(host.Trim());
        }

        public void Desconectar() => _socketService.Desconectar();

        // ── Cargar viajes desde el servidor ──────────────────

        // Enviar comando "VIAJES" al servidor y parsear la lista recibida
        // Formato: "cod|desc|salida|llegada|costo;cod|desc|salida|llegada|costo"
        public (bool ok, string mensaje, List<ViajeInfo> viajes) CargarViajes()
        {
            if (!EstaConectado)
                return (false, "No hay conexion activa.", new List<ViajeInfo>());

            string respuesta = _socketService.EnviarYRecibir("VIAJES");

            if (string.IsNullOrEmpty(respuesta))
                return (false, "Sin respuesta del servidor.", new List<ViajeInfo>());

            if (respuesta == "VACIO")
                return (false, "El servidor no tiene viajes registrados.", new List<ViajeInfo>());

            var lista = new List<ViajeInfo>();
            foreach (string parte in respuesta.Split(';'))
            {
                string[] campos = parte.Split('|');
                if (campos.Length >= 5 &&
                    int.TryParse(campos[0], out int cod) &&
                    double.TryParse(campos[4], out double costo))
                {
                    lista.Add(new ViajeInfo
                    {
                        Codigo          = cod,
                        Descripcion     = campos[1],
                        TerminalSalida  = campos[2],
                        TerminalLlegada = campos[3],
                        Costo           = costo
                    });
                }
            }

            return (true, $"Se cargaron {lista.Count} viajes.", lista);
        }

        // ── Enviar solicitud ────────────────────────────────── ──────────────────────────────────

        public (bool ok, string tramaEnviada, string tramaRecibida, string detalle)
            EnviarSolicitud(int codigoViaje, int boletos, string descViaje = "")
        {
            if (!EstaConectado)
                return (false, "", "", "No hay conexion activa con el servidor.");

            if (boletos < 1 || boletos > 5)
                return (false, "", "", "La cantidad debe ser entre 1 y 5 boletos.");

            // Armar y enviar la trama
            var req   = new TramaRequest { CodigoViaje = codigoViaje, CantidadBoletos = boletos };
            string enviada  = req.ToString();
            string recibida = _socketService.EnviarYRecibir(enviada);

            if (string.IsNullOrEmpty(recibida))
                return (false, enviada, "", "Sin respuesta del servidor. La conexion pudo perderse.");

            // Parsear e interpretar la respuesta
            var resp = TramaResponse.Parse(recibida);
            if (resp == null)
                return (false, enviada, recibida, $"Respuesta inesperada: [{recibida}]");

            return (true, enviada, recibida, InterpretarRespuesta(resp, descViaje));
        }

        // Convertir la trama de respuesta a texto legible para el usuario
        // strDescViaje: descripcion del viaje para mostrar (viene del ComboBox)
        public static string InterpretarRespuesta(TramaResponse r, string strDescViaje = "")
        {
            string estado = r.Verificador switch
            {
                TramaResponse.EXITOSA          => "TRANSACCION EXITOSA",
                TramaResponse.SIN_ASIENTOS     => "SIN ASIENTOS DISPONIBLES",
                TramaResponse.EXCEDE_CANTIDAD  => "EXCEDE LA CANTIDAD (max 5 boletos)",
                TramaResponse.VIAJE_NO_EXISTE  => "CODIGO DE VIAJE NO EXISTE",
                TramaResponse.FORMATO_INVALIDO => "FORMATO INVALIDO",
                _                              => $"Codigo desconocido: {r.Verificador}"
            };

            if (r.Verificador != TramaResponse.EXITOSA)
                return estado;

            // Usar la descripcion real del viaje si viene, sino el codigo
            string ruta = string.IsNullOrEmpty(strDescViaje)
                ? $"Viaje {r.CodigoViaje:D2}"
                : strDescViaje;

            // Mostrar solo los asientos asignados (ignorar los '0' de relleno)
            string asientos1 = string.Concat(
                r.AsientosFila1.Where(c => c != '0').Select(c => $"[{c}]"));
            string asientos2 = r.NumeroFila2 > 0
                ? string.Concat(r.AsientosFila2.Where(c => c != '0').Select(c => $"[{c}]"))
                : "N/A";

            return $"{estado}\n" +
                   $"─────────────────────────────\n" +
                   $"Viaje    : {r.CodigoViaje:D2} - {ruta}\n" +
                   $"Fila 1   : {r.NumeroFila1:D2}  Asientos: {asientos1}\n" +
                   $"Fila 2   : {(r.NumeroFila2 > 0 ? r.NumeroFila2.ToString("D2") : "N/A")}  Asientos: {asientos2}\n" +
                   $"Monto    : {r.Monto:N0} colones\n" +
                   $"Disponibles restantes: {r.CapacidadDisponible}\n" +
                   $"─────────────────────────────";
        }

        public void Dispose() => _socketService.Dispose();
    }
}
