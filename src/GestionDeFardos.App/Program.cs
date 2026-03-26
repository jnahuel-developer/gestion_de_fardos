using GestionDeFardos.Core.Interfaces;
using GestionDeFardos.Infrastructure;

namespace GestionDeFardos.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        IAppLogger logger = new FileAppLogger(AppContext.BaseDirectory);
        RegisterGlobalExceptionLogging(logger);
        string appVersion = Application.ProductVersion;
        logger.Log(AppLogLevel.Info, "APP", $"Aplicacion iniciada. Version={appVersion}.");

        try
        {
            Application.Run(new MainForm(logger));
            logger.Log(AppLogLevel.Info, "APP", $"Aplicacion finalizada. Version={appVersion}.");
        }
        catch (Exception ex)
        {
            logger.Log(AppLogLevel.Error, "APP", $"Fallo fatal: {ex}");
            throw;
        }
    }

    private static void RegisterGlobalExceptionLogging(IAppLogger logger)
    {
        Application.ThreadException += (_, eventArgs) =>
            logger.Log(AppLogLevel.Error, "APP", $"Excepcion UI no controlada: {eventArgs.Exception}");

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception ex)
            {
                logger.Log(AppLogLevel.Error, "APP", $"Excepcion no controlada: {ex}");
            }
            else
            {
                logger.Log(AppLogLevel.Error, "APP", "Excepcion no controlada de tipo desconocido.");
            }
        };
    }
}
