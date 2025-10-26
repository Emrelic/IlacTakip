using System.IO;
using System.Text.Json.Serialization;

namespace MedulaOtomasyon;

/// <summary>
/// Execution durumu
/// </summary>
public enum ExecutionStatus
{
    NotStarted = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Stopped = 5
}

/// <summary>
/// Step execution durumu
/// </summary>
public enum StepExecutionStatus
{
    Pending = 0,
    Running = 1,
    Success = 2,
    Failed = 3,
    Skipped = 4,
    Retrying = 5
}

/// <summary>
/// Execution hız seçenekleri
/// </summary>
public enum ExecutionSpeed
{
    Slow = 0,      // Her adım arası 2000ms bekle
    Normal = 1,    // Her adım arası 1000ms bekle
    Fast = 2       // Bekleme yok (sadece gerekli sistem beklemeleri)
}

/// <summary>
/// Hata durumunda kullanıcı seçimi
/// </summary>
public enum ErrorAction
{
    Stop = 0,      // Çalıştırmayı durdur
    Retry = 1,     // Adımı tekrar dene
    Skip = 2,      // Adımı atla ve devam et
    Continue = 3   // Hatayı yoksay ve devam et
}

/// <summary>
/// Bir adımın execution kaydı
/// </summary>
public class StepExecutionRecord
{
    public int StepNumber { get; set; }
    public string? StepDescription { get; set; }
    public StepExecutionStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public string? ScreenshotPath { get; set; } // Debug için screenshot

    /// <summary>
    /// Adımın süresini hesapla
    /// </summary>
    public int CalculateDuration()
    {
        if (EndTime.HasValue)
        {
            DurationMs = (int)(EndTime.Value - StartTime).TotalMilliseconds;
        }
        return DurationMs;
    }
}

/// <summary>
/// Task chain execution geçmişi
/// </summary>
public class ExecutionRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChainName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalDurationMs { get; set; }
    public ExecutionStatus Status { get; set; }
    public List<StepExecutionRecord> StepRecords { get; set; } = new();

    // Execution settings
    public ExecutionSpeed Speed { get; set; }
    public bool DebugMode { get; set; }
    public bool ScreenshotEnabled { get; set; }

    // Statistics
    public int TotalSteps { get; set; }
    public int SuccessfulSteps { get; set; }
    public int FailedSteps { get; set; }
    public int SkippedSteps { get; set; }

    /// <summary>
    /// Toplam süreyi hesapla
    /// </summary>
    public int CalculateDuration()
    {
        if (EndTime.HasValue)
        {
            TotalDurationMs = (int)(EndTime.Value - StartTime).TotalMilliseconds;
        }
        return TotalDurationMs;
    }

    /// <summary>
    /// İstatistikleri güncelle
    /// </summary>
    public void UpdateStatistics()
    {
        TotalSteps = StepRecords.Count;
        SuccessfulSteps = StepRecords.Count(s => s.Status == StepExecutionStatus.Success);
        FailedSteps = StepRecords.Count(s => s.Status == StepExecutionStatus.Failed);
        SkippedSteps = StepRecords.Count(s => s.Status == StepExecutionStatus.Skipped);
    }
}

/// <summary>
/// Execution geçmişini yöneten database
/// </summary>
public class ExecutionHistoryDatabase
{
    private readonly string _dbFilePath;
    private List<ExecutionRecord> _records = new();

    public ExecutionHistoryDatabase(string? filePath = null)
    {
        _dbFilePath = filePath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "execution_history.json"
        );

        LoadAll();
    }

    /// <summary>
    /// Tüm geçmişi yükle
    /// </summary>
    public List<ExecutionRecord> LoadAll()
    {
        try
        {
            if (File.Exists(_dbFilePath))
            {
                var json = File.ReadAllText(_dbFilePath);
                _records = System.Text.Json.JsonSerializer.Deserialize<List<ExecutionRecord>>(json)
                    ?? new List<ExecutionRecord>();
            }
        }
        catch
        {
            _records = new List<ExecutionRecord>();
        }

        return _records;
    }

    /// <summary>
    /// Geçmişi kaydet
    /// </summary>
    public void SaveAll()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_records, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_dbFilePath, json);
        }
        catch
        {
            // Hata durumunda sessiz kal
        }
    }

    /// <summary>
    /// Yeni execution kaydı ekle
    /// </summary>
    public void Add(ExecutionRecord record)
    {
        _records.Add(record);
        SaveAll();
    }

    /// <summary>
    /// Execution kaydını güncelle
    /// </summary>
    public void Update(ExecutionRecord record)
    {
        var existing = _records.FirstOrDefault(r => r.Id == record.Id);
        if (existing != null)
        {
            _records.Remove(existing);
            _records.Add(record);
            SaveAll();
        }
    }

    /// <summary>
    /// Belirli bir chain için geçmişi getir
    /// </summary>
    public List<ExecutionRecord> GetByChainName(string chainName)
    {
        return _records
            .Where(r => r.ChainName == chainName)
            .OrderByDescending(r => r.StartTime)
            .ToList();
    }

    /// <summary>
    /// En son N kaydı getir
    /// </summary>
    public List<ExecutionRecord> GetRecent(int count = 10)
    {
        return _records
            .OrderByDescending(r => r.StartTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Başarılı execution'ları getir
    /// </summary>
    public List<ExecutionRecord> GetSuccessful()
    {
        return _records
            .Where(r => r.Status == ExecutionStatus.Completed)
            .OrderByDescending(r => r.StartTime)
            .ToList();
    }

    /// <summary>
    /// Başarısız execution'ları getir
    /// </summary>
    public List<ExecutionRecord> GetFailed()
    {
        return _records
            .Where(r => r.Status == ExecutionStatus.Failed)
            .OrderByDescending(r => r.StartTime)
            .ToList();
    }
}

/// <summary>
/// Execution event arguments
/// </summary>
public class ExecutionEventArgs : EventArgs
{
    public ExecutionRecord Record { get; set; }
    public StepExecutionRecord? CurrentStep { get; set; }
    public string? Message { get; set; }

    public ExecutionEventArgs(ExecutionRecord record, StepExecutionRecord? currentStep = null, string? message = null)
    {
        Record = record;
        CurrentStep = currentStep;
        Message = message;
    }
}

/// <summary>
/// Step execution event arguments
/// </summary>
public class StepExecutionEventArgs : EventArgs
{
    public StepExecutionRecord StepRecord { get; set; }
    public TaskStep Step { get; set; }
    public string? Message { get; set; }

    public StepExecutionEventArgs(StepExecutionRecord stepRecord, TaskStep step, string? message = null)
    {
        StepRecord = stepRecord;
        Step = step;
        Message = message;
    }
}
