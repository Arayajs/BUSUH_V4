// ============================================================
// DTOs: Tramas del protocolo TCP
// TramaRequest:  cliente -> servidor (3 chars)
// TramaResponse: servidor -> cliente (24 chars)
// ============================================================
namespace BUSUH_Servidor.DTOs
{
    public class TramaRequest
    {
        public int CodigoViaje    { get; set; }
        public int CantidadBoletos{ get; set; }

        // Parsear "013" -> viaje=01, boletos=3
        public static TramaRequest? Parse(string trama)
        {
            if (trama.Length != 3) return null;
            if (!int.TryParse(trama.Substring(0, 2), out int viaje))   return null;
            if (!int.TryParse(trama.Substring(2, 1), out int boletos)) return null;
            return new TramaRequest { CodigoViaje = viaje, CantidadBoletos = boletos };
        }
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

        // Serializar a 24 chars segun el protocolo del enunciado
        public override string ToString() =>
            $"{Verificador}{CodigoViaje:D2}{NumeroFila1:D2}{AsientosFila1}" +
            $"{NumeroFila2:D2}{AsientosFila2}{Monto:D5}{CapacidadDisponible:D2}";

        public static TramaResponse Error(int verificador, int codigoViaje = 0) => new()
        {
            Verificador = verificador,
            CodigoViaje = codigoViaje
        };
    }
}
