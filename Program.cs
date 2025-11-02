using System.IO;

namespace MedulaOtomasyon;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Global exception handler'ları ekle
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        var exception = e.Exception;
        LogException("UI Thread Exception", exception);

        try
        {
            MessageBox.Show(
                $"GLOBAL UI THREAD EXCEPTION:\n\n" +
                $"Type: {exception.GetType().Name}\n" +
                $"Message: {exception.Message}\n\n" +
                $"StackTrace:\n{exception.StackTrace}\n\n" +
                $"Log yazıldı: crash_log.txt",
                "FATAL ERROR - UI Thread",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch { }
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogException("Unhandled Exception", exception);

        try
        {
            MessageBox.Show(
                $"GLOBAL UNHANDLED EXCEPTION:\n\n" +
                $"Type: {exception?.GetType().Name ?? "Unknown"}\n" +
                $"Message: {exception?.Message ?? "Unknown"}\n\n" +
                $"StackTrace:\n{exception?.StackTrace ?? "Unknown"}\n\n" +
                $"IsTerminating: {e.IsTerminating}\n\n" +
                $"Log yazıldı: crash_log.txt",
                "FATAL ERROR - Unhandled",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch { }
    }

    private static void LogException(string source, Exception? exception)
    {
        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            var logMessage = $"\n\n{'=',-80}\n" +
                           $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {source}\n" +
                           $"{'=',-80}\n" +
                           $"Type: {exception?.GetType().FullName ?? "Unknown"}\n" +
                           $"Message: {exception?.Message ?? "Unknown"}\n" +
                           $"StackTrace:\n{exception?.StackTrace ?? "Unknown"}\n" +
                           $"InnerException: {exception?.InnerException?.ToString() ?? "None"}\n";

            File.AppendAllText(logFile, logMessage);
        }
        catch { }
    }
}