using System.Windows;
using HabitatRural.Data;

namespace HabitatRural;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Initialize database on first run
        DatabaseService.Instance.Initialize();
    }
}
