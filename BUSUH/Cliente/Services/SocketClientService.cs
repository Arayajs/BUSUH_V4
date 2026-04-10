// ============================================================
// Servicio: SocketClientService
// Maneja la conexion TCP con el BUSUH_Servidor
// Principio SRP: solo se encarga de la comunicacion de red
// ============================================================
using System.Net.Sockets;
using System.Text;

namespace BUSUH_Cliente.Services
{
    public class SocketClientService : IDisposable
    {
        public const int PUERTO_SERVIDOR = 30000;

        private TcpClient?     _cliente;
        private NetworkStream? _stream;
        private readonly object _candado = new object();

        public bool EstaConectado => _cliente?.Connected ?? false;

        public (bool ok, string mensaje) Conectar(string host)
        {
            try
            {
                Desconectar();
                _cliente = new TcpClient(host, PUERTO_SERVIDOR);
                _stream  = _cliente.GetStream();
                return (true, $"Conectado a {host}:{PUERTO_SERVIDOR}");
            }
            catch (Exception ex)
            {
                _cliente = null;
                _stream  = null;
                return (false,
                    $"No se pudo conectar al servidor.\n" +
                    $"Verifique que BUSUH_Servidor este corriendo y el socket activo.\n\n" +
                    $"Detalle: {ex.Message}");
            }
        }

        public void Desconectar()
        {
            _stream?.Close();
            _cliente?.Close();
            _stream  = null;
            _cliente = null;
        }

        // Enviar trama y esperar respuesta
        // Retorna string vacio si hay error
        public string EnviarYRecibir(string trama)
        {
            if (!EstaConectado || _stream == null) return "";

            lock (_candado)
            {
                try
                {
                    byte[] datos = Encoding.UTF8.GetBytes(trama);
                    _stream.Write(datos, 0, datos.Length);

                    byte[] buffer = new byte[512];
                    int    n      = _stream.Read(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer, 0, n).Trim();
                }
                catch
                {
                    Desconectar();
                    return "";
                }
            }
        }

        public void Dispose() => Desconectar();
    }
}
