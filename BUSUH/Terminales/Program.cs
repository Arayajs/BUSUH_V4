// BUSUH_Terminales - Ejecutable #1
// Iniciar PRIMERO y activar el servicio TCP antes de abrir BUSUH_Servidor
using BUSUH_Terminales.Views;

namespace BUSUH_Terminales
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmTerminales());
        }
    }
}
