using RuralHousingApp.UI;

namespace RuralHousingApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Data.DbHelper.Initialize();
            Application.Run(new MainForm());
        }
    }
}