// ============================================================
// Modelo: ClsTerminal
// Representa una terminal de buses (origen o destino)
// Principio SRP: solo contiene los datos de la terminal
// ============================================================
namespace BUSUH_Terminales.Models
{
    public class ClsTerminal
    {
        public int    Codigo      { get; set; }
        public string Nombre      { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public ClsTerminal() { }

        public ClsTerminal(int codigo, string nombre, string descripcion)
        {
            Codigo      = codigo;
            Nombre      = nombre;
            Descripcion = descripcion;
        }

        public override string ToString() => Nombre;
    }
}
