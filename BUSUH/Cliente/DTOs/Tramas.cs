// ============================================================
// DTOs del Cliente: Tramas del protocolo TCP
// Copia propia (el Cliente es independiente del Servidor)
// TramaRequest:  cliente -> servidor (3 chars)
// TramaResponse: servidor -> cliente (24 chars)
// ViajeInfo:     datos de un viaje recibidos desde el servidor
// ============================================================
namespace BUSUH_Cliente.DTOs
{
    // Representa un viaje disponible, recibido al consultar al servidor
    public class ViajeInfo
    {
        public int    Codigo          { get; set; }
        public string Descripcion     { get; set; } = "";
        public string TerminalSalida  { get; set; } = "";
        public string TerminalLlegada { get; set; } = "";
        public double Costo           { get; set; }

        // Texto que se muestra en el ComboBox del cliente
        public override string ToString() =>
            $"{Codigo:D2} - {Descripcion}  (₡{Costo:N0})";
    }
    public class TramaRequest
    {
        public int CodigoViaje     { get; set; }
        public int CantidadBoletos { get; set; }

        // Serializar a "013" = viaje 01, 3 boletos
        public override string ToString() =>
            $"{CodigoViaje:D2}{CantidadBoletos}";
    }

    public class TramaResponse
    {
        public const int SIN_ASIENTOS    = 0;
        public const int EXITOSA         = 1;
        public const int EXCEDE_CANTIDAD = 3;
        public const int VIAJE_NO_EXISTE = 4;
        public const int FORMATO_INVALIDO= 5;

        public int    Verificador         { get; set; }
        public int    CodigoViaje         { get; set; }
        public int    NumeroFila1         { get; set; }
        public string AsientosFila1       { get; set; } = "00000";
        public int    NumeroFila2         { get; set; }
        public string AsientosFila2       { get; set; } = "00000";
        public int    Monto               { get; set; }
        public int    CapacidadDisponible { get; set; }

        // Parsear la trama de 24 chars recibida del servidor
        public static TramaResponse? Parse(string trama)
        {
            if (string.IsNullOrEmpty(trama) || trama.Length < 24) return null;
            try
            {
                return new TramaResponse
                {
                    Verificador         = int.Parse(trama.Substring(0,  1)),
                    CodigoViaje         = int.Parse(trama.Substring(1,  2)),
                    NumeroFila1         = int.Parse(trama.Substring(3,  2)),
                    AsientosFila1       = trama.Substring(5,  5),
                    NumeroFila2         = int.Parse(trama.Substring(10, 2)),
                    AsientosFila2       = trama.Substring(12, 5),
                    Monto               = int.Parse(trama.Substring(17, 5)),
                    CapacidadDisponible = int.Parse(trama.Substring(22, 2))
                };
            }
            catch { return null; }
        }
    }
}
