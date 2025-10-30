using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace MedulaOtomasyon;

/// <summary>
/// Görev zincirlerini JSON dosyasında saklayan basit veritabanı
/// </summary>
public class TaskChainDatabase
{
    private readonly string _dbFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public TaskChainDatabase(string? dbFilePath = null)
    {
        // Varsayılan: uygulama klasörü altında taskchains.json
        _dbFilePath = dbFilePath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "taskchains.json"
        );

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Tüm görev zincirlerini yükler
    /// </summary>
    public List<TaskChain> LoadAll()
    {
        try
        {
            if (!File.Exists(_dbFilePath))
            {
                return new List<TaskChain>();
            }

            var json = File.ReadAllText(_dbFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<TaskChain>();
            }

            var chains = JsonSerializer.Deserialize<List<TaskChain>>(json, _jsonOptions);
            return chains ?? new List<TaskChain>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Görev zincirleri yüklenirken hata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tüm görev zincirlerini kaydeder
    /// </summary>
    public void SaveAll(List<TaskChain> chains)
    {
        try
        {
            var json = JsonSerializer.Serialize(chains, _jsonOptions);
            File.WriteAllText(_dbFilePath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Görev zincirleri kaydedilirken hata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Yeni bir görev zinciri ekler
    /// </summary>
    public void Add(TaskChain chain)
    {
        var chains = LoadAll();

        // Aynı isimde varsa güncelle, yoksa ekle
        var existing = chains.FirstOrDefault(c => c.Name == chain.Name);
        if (existing != null)
        {
            chains.Remove(existing);
        }

        chain.LastModifiedDate = DateTime.Now;
        chains.Add(chain);
        SaveAll(chains);
    }

    /// <summary>
    /// İsme göre görev zinciri getirir
    /// </summary>
    public TaskChain? Get(string name)
    {
        var chains = LoadAll();
        return chains.FirstOrDefault(c => c.Name == name);
    }

    /// <summary>
    /// Tüm görev zincirlerini getir
    /// </summary>
    public List<TaskChain> GetAll()
    {
        return LoadAll();
    }

    /// <summary>
    /// İsme göre görev zincirini getir (alias for Get)
    /// </summary>
    public TaskChain? GetByName(string name)
    {
        return Get(name);
    }

    /// <summary>
    /// Görev zincirini siler
    /// </summary>
    public void Delete(string name)
    {
        var chains = LoadAll();
        chains.RemoveAll(c => c.Name == name);
        SaveAll(chains);
    }

    /// <summary>
    /// Mevcut bir görev zincirini güncelle
    /// </summary>
    public void Update(TaskChain chain)
    {
        var chains = LoadAll();
        var index = chains.FindIndex(c => c.Name == chain.Name);

        if (index >= 0)
        {
            chain.LastModifiedDate = DateTime.Now;
            chains[index] = chain;
            SaveAll(chains);
        }
        else
        {
            throw new ArgumentException($"'{chain.Name}' adlı görev zinciri bulunamadı.");
        }
    }

    /// <summary>
    /// Veritabanı dosya yolunu döndürür
    /// </summary>
    public string GetDatabasePath() => _dbFilePath;
}
