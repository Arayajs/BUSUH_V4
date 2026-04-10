// BUSUH_Cliente - Ejecutable #3
// Iniciar ULTIMO (despues de BUSUH_Terminales y BUSUH_Servidor)
using BUSUH_Cliente.Views;

namespace BUSUH_Cliente
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmCliente());
        }
    }
}
