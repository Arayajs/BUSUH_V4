// ============================================================
// Modelos del Servidor
// Principio SRP: cada clase tiene una sola responsabilidad
// ============================================================
namespace BUSUH_Servidor.Models
{
    // Modelo Terminal (copia local, recibida desde BUSUH_Terminales via TCP)
    public class ClsTerminal
    {
        public int    Codigo      { get; set; }
        public string Nombre      { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public override string ToString() => Nombre;
    }

    // Modelo Viaje con arreglo de asientos [13 filas x 7 columnas]
    // Filas 1-12: 5 asientos c/u | Fila 13: 7 asientos | Total: 67
    public class ClsViaje
    {
        public int    Codigo              { get; set; }
        public string Descripcion         { get; set; } = "";
        public string TerminalSalida      { get; set; } = "";
        public string TerminalLlegada     { get; set; } = "";
        public int    Capacidad           { get; set; }
        public double Costo               { get; set; }
        public int    CapacidadDisponible { get; set; }

        // Arreglo de asientos: 1=disponible, 0=ocupado
        public int[,] Asientos { get; private set; } = new int[13, 7];

        // Llenar la matriz al momento de grabar el viaje
        public void InicializarAsientos()
        {
            for (int fila = 0; fila < 13; fila++)
            {
                int maxCols = (fila == 12) ? 7 : 5;
                for (int col = 0; col < 7; col++)
                    Asientos[fila, col] = col < maxCols ? 1 : 0;
            }
            CapacidadDisponible = Capacidad;
        }

        public int AsientosLibresEnFila(int fila)
        {
            int max = (fila == 12) ? 7 : 5;
            int libres = 0;
            for (int col = 0; col < max; col++)
                if (Asientos[fila, col] == 1) libres++;
            return libres;
        }

        public string GetFilaComoTexto(int fila)
        {
            string texto = "";
            for (int col = 0; col < 7; col++)
                texto += Asientos[fila, col];
            return texto;
        }
    }
}
