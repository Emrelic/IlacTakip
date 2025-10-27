using System.Text;
using System.IO;

namespace MedulaOtomasyon;

/// <summary>
/// Debug log'larını hem Debug Output'a hem de dosyaya yazan logger
/// </summary>
public static class DebugLogger
{
    private static readonly object _lockObject = new object();
    private static readonly string _logFilePath;
    private static bool _isInitialized = false;

    static DebugLogger()
    {
        // Log dosyası: proje klasöründe ElementLocator.log
        var projectDir = AppDomain.CurrentDomain.BaseDirectory;
        _logFilePath = Path.Combine(projectDir, "ElementLocator.log");
    }

    /// <summary>
    /// Log dosyasını temizler ve yeni bir oturum başlatır
    /// </summary>
    public static void StartNewSession()
    {
        lock (_lockObject)
        {
            try
            {
                var header = new StringBuilder();
                header.AppendLine("=".PadRight(80, '='));
                header.AppendLine($"ELEMENT LOCATOR DEBUG LOG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                header.AppendLine("=".PadRight(80, '='));
                header.AppendLine();

                File.WriteAllText(_logFilePath, header.ToString());
                _isInitialized = true;

                System.Diagnostics.Debug.WriteLine($"[DebugLogger] Log dosyası başlatıldı: {_logFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DebugLogger] HATA: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log mesajı yazar (hem Debug Output'a hem dosyaya)
    /// </summary>
    public static void Log(string message)
    {
        if (!_isInitialized)
        {
            StartNewSession();
        }

        lock (_lockObject)
        {
            try
            {
                // Debug Output'a yaz
                System.Diagnostics.Debug.WriteLine(message);

                // Dosyaya yaz (timestamp ekle)
                var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch
            {
                // Log yazma hatası - sessizce devam et
            }
        }
    }

    /// <summary>
    /// Birden fazla log mesajı yazar
    /// </summary>
    public static void LogMultiple(params string[] messages)
    {
        foreach (var message in messages)
        {
            Log(message);
        }
    }

    /// <summary>
    /// Boş satır ekler
    /// </summary>
    public static void LogBlankLine()
    {
        Log("");
    }

    /// <summary>
    /// Separator ekler
    /// </summary>
    public static void LogSeparator(char c = '-', int length = 60)
    {
        Log(new string(c, length));
    }

    /// <summary>
    /// Log dosyasının yolunu döndürür
    /// </summary>
    public static string GetLogFilePath()
    {
        return _logFilePath;
    }

    /// <summary>
    /// Log dosyasını okur ve içeriği döndürür
    /// </summary>
    public static string ReadLogFile()
    {
        lock (_lockObject)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    return File.ReadAllText(_logFilePath);
                }
                return "Log dosyası henüz oluşturulmamış.";
            }
            catch (Exception ex)
            {
                return $"Log dosyası okunamadı: {ex.Message}";
            }
        }
    }
}
