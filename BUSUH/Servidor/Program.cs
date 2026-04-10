// BUSUH_Servidor - Ejecutable #2
// Iniciar SEGUNDO (despues de BUSUH_Terminales)
// Presionar "Cargar Terminales" y luego "Iniciar" el socket
using BUSUH_Servidor.Views;

namespace BUSUH_Servidor
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmServidor());
        }
    }
}
