using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// Task Chain çalıştırma motoru
/// Play, Pause, Stop, Debug özellikleri ile task chain execution
/// </summary>
public class TaskChainExecutor
{
    private TaskChain? _currentChain;
    private ExecutionRecord? _currentRecord;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isPaused;
    private int _currentStepIndex = -1;
    private readonly ExecutionHistoryDatabase _historyDb;
    private readonly ConditionEvaluator _conditionEvaluator;
    private readonly SmartElementRecorder _smartElementPlayer;

    // Settings
    public ExecutionSpeed Speed { get; set; } = ExecutionSpeed.Normal;
    public bool DebugMode { get; set; } = false;
    public bool ScreenshotEnabled { get; set; } = false;
    public ErrorAction DefaultErrorAction { get; set; } = ErrorAction.Stop;

    // Events
    public event EventHandler<ExecutionEventArgs>? ExecutionStarted;
    public event EventHandler<ExecutionEventArgs>? ExecutionCompleted;
    public event EventHandler<ExecutionEventArgs>? ExecutionFailed;
    public event EventHandler<ExecutionEventArgs>? ExecutionPaused;
    public event EventHandler<ExecutionEventArgs>? ExecutionResumed;
    public event EventHandler<ExecutionEventArgs>? ExecutionStopped;

    public event EventHandler<StepExecutionEventArgs>? StepStarted;
    public event EventHandler<StepExecutionEventArgs>? StepCompleted;
    public event EventHandler<StepExecutionEventArgs>? StepFailed;
    public event EventHandler<StepExecutionEventArgs>? StepSkipped;

    public event EventHandler<string>? LogMessage;
    public event Func<TaskStep, string, ErrorAction>? ErrorOccurred; // Kullanıcıya sor

    // State
    public bool IsRunning => _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
    public bool IsPaused => _isPaused;
    public ExecutionRecord? CurrentRecord => _currentRecord;
    public int CurrentStepIndex => _currentStepIndex;

    public TaskChainExecutor()
    {
        _historyDb = new ExecutionHistoryDatabase();
        _conditionEvaluator = new ConditionEvaluator();
        _smartElementPlayer = new SmartElementRecorder();

        // Debug logger'ı başlat (element arama sorunlarını tespit için)
        DebugLogger.StartNewSession();
    }

    /// <summary>
    /// Task chain'i başlat
    /// </summary>
    public async Task<ExecutionRecord> ExecuteAsync(TaskChain chain)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Zaten bir task chain çalışıyor!");
        }

        _currentChain = chain;
        _currentStepIndex = -1;
        _isPaused = false;
        _cancellationTokenSource = new CancellationTokenSource();

        // Execution record oluştur
        _currentRecord = new ExecutionRecord
        {
            ChainName = chain.Name,
            StartTime = DateTime.Now,
            Status = ExecutionStatus.Running,
            Speed = Speed,
            DebugMode = DebugMode,
            ScreenshotEnabled = ScreenshotEnabled,
            TotalSteps = chain.Steps.Count
        };

        Log($"Task chain başlatılıyor: {chain.Name}");
        ExecutionStarted?.Invoke(this, new ExecutionEventArgs(_currentRecord));

        try
        {
            int i = 0;
            bool isLooping = false;
            int loopCount = 0;

            // Her adımı sırayla çalıştır (döngü desteği ile)
            while (i < chain.Steps.Count)
            {
                // Cancellation check
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _currentRecord.Status = ExecutionStatus.Stopped;
                    Log("Task chain kullanıcı tarafından durduruldu.");
                    ExecutionStopped?.Invoke(this, new ExecutionEventArgs(_currentRecord));
                    break;
                }

                // Pause check
                while (_isPaused)
                {
                    await Task.Delay(100, _cancellationTokenSource.Token);
                }

                _currentStepIndex = i;
                var step = chain.Steps[i];

                // Adımı çalıştır
                var stepRecord = await ExecuteStepAsync(step, _cancellationTokenSource.Token);
                _currentRecord.StepRecords.Add(stepRecord);

                // Hata durumu kontrolü
                if (stepRecord.Status == StepExecutionStatus.Failed)
                {
                    // Kullanıcıya sor: Durdur / Tekrar dene / Atla
                    var action = await HandleStepErrorAsync(step, stepRecord.ErrorMessage ?? "Bilinmeyen hata");

                    if (action == ErrorAction.Stop)
                    {
                        _currentRecord.Status = ExecutionStatus.Failed;
                        Log("Task chain hata nedeniyle durduruldu.");
                        ExecutionFailed?.Invoke(this, new ExecutionEventArgs(_currentRecord, stepRecord));
                        break;
                    }
                    else if (action == ErrorAction.Retry)
                    {
                        // Aynı adımı tekrar dene
                        stepRecord.RetryCount++;
                        Log($"Adım {step.StepNumber} tekrar deneniyor... (Deneme: {stepRecord.RetryCount})");
                        continue;
                    }
                    else if (action == ErrorAction.Skip)
                    {
                        stepRecord.Status = StepExecutionStatus.Skipped;
                        Log($"Adım {step.StepNumber} atlandı.");
                        StepSkipped?.Invoke(this, new StepExecutionEventArgs(stepRecord, step));
                    }
                    // Continue: Bir sonraki adıma geç
                }

                // Debug mode: Her adımda bekle
                if (DebugMode)
                {
                    await Task.Delay(GetStepDelay(), _cancellationTokenSource.Token);
                }

                // Döngüsel görev kontrolü
                if (chain.IsLooped && i == chain.LoopEndIndex)
                {
                    isLooping = true;
                    loopCount++;

                    // Döngü sonlanma kontrolü
                    if (chain.LoopConditionStep != null)
                    {
                        Log($"🔄 Döngü sonlanma kontrolü yapılıyor... (Döngü: {loopCount})");

                        // Döngü sonlanma koşulunu kontrol et
                        var loopCheckResult = await CheckLoopConditionAsync(chain.LoopConditionStep, _cancellationTokenSource.Token);

                        if (loopCheckResult)
                        {
                            Log("✓ Döngü sonlanma koşulu sağlandı. Döngü tamamlanıyor...");
                            i++; // Sonraki adıma geç (döngüden çık)
                        }
                        else
                        {
                            Log($"↩️ Döngü devam ediyor. {chain.LoopStartIndex + 1}. adıma dönülüyor...");
                            i = chain.LoopStartIndex; // Döngü başlangıcına dön
                            continue;
                        }
                    }
                    else
                    {
                        // Döngü sonlanma kontrolü yoksa, döngüyü maksimum sayıda çalıştır
                        int maxLoopCount = chain.MaxLoopCount > 0 ? chain.MaxLoopCount : 100; // Varsayılan 100
                        if (loopCount >= maxLoopCount)
                        {
                            Log($"⚠️ Maksimum döngü sayısına ({maxLoopCount}) ulaşıldı. Döngü sonlandırılıyor.");
                            i++;
                        }
                        else
                        {
                            Log($"↩️ Döngü {loopCount}. kez çalışıyor. {chain.LoopStartIndex + 1}. adıma dönülüyor... (Maks: {maxLoopCount})");
                            i = chain.LoopStartIndex; // Döngü başlangıcına dön
                            continue;
                        }
                    }
                }
                else
                {
                    i++; // Bir sonraki adıma geç
                }
            }

            // Execution tamamlandı
            if (_currentRecord.Status == ExecutionStatus.Running)
            {
                _currentRecord.Status = ExecutionStatus.Completed;
                Log("Task chain başarıyla tamamlandı!");
                if (isLooping)
                {
                    Log($"📊 Toplam döngü sayısı: {loopCount}");
                }
                ExecutionCompleted?.Invoke(this, new ExecutionEventArgs(_currentRecord));
            }
        }
        catch (OperationCanceledException)
        {
            _currentRecord.Status = ExecutionStatus.Stopped;
            Log("Task chain iptal edildi.");
            ExecutionStopped?.Invoke(this, new ExecutionEventArgs(_currentRecord));
        }
        catch (Exception ex)
        {
            _currentRecord.Status = ExecutionStatus.Failed;
            Log($"Beklenmeyen hata: {ex.Message}");
            ExecutionFailed?.Invoke(this, new ExecutionEventArgs(_currentRecord, message: ex.Message));
        }
        finally
        {
            // İstatistikleri güncelle
            _currentRecord.EndTime = DateTime.Now;
            _currentRecord.CalculateDuration();
            _currentRecord.UpdateStatistics();

            // Geçmişe kaydet
            _historyDb.Add(_currentRecord);

            // Cleanup
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        return _currentRecord;
    }

    /// <summary>
    /// Bir adımı çalıştır
    /// </summary>
    private async Task<StepExecutionRecord> ExecuteStepAsync(TaskStep step, CancellationToken cancellationToken)
    {
        var stepRecord = new StepExecutionRecord
        {
            StepNumber = step.StepNumber,
            StepDescription = step.Description,
            Status = StepExecutionStatus.Running,
            StartTime = DateTime.Now
        };

        Log($"Adım {step.StepNumber} başlatılıyor: {step.Description}");
        StepStarted?.Invoke(this, new StepExecutionEventArgs(stepRecord, step));

        try
        {
            // Step type'a göre execute et
            switch (step.StepType)
            {
                case StepType.TargetSelection:
                    await ExecuteTargetSelectionAsync(step, cancellationToken);
                    break;

                case StepType.UIElementAction:
                    await ExecuteUIElementActionAsync(step, cancellationToken);
                    break;

                case StepType.ConditionalBranch:
                    await ExecuteConditionalBranchAsync(step, cancellationToken);
                    break;

                case StepType.LoopOrEnd:
                    // TODO: Implement loop
                    throw new NotImplementedException("Loop henüz implement edilmedi.");

                default:
                    throw new NotSupportedException($"Bilinmeyen step type: {step.StepType}");
            }

            // Screenshot (eğer aktifse)
            if (ScreenshotEnabled)
            {
                stepRecord.ScreenshotPath = await CaptureScreenshotAsync(step);
            }

            stepRecord.Status = StepExecutionStatus.Success;
            stepRecord.EndTime = DateTime.Now;
            stepRecord.CalculateDuration();

            Log($"✓ Adım {step.StepNumber} tamamlandı ({stepRecord.DurationMs}ms)");
            StepCompleted?.Invoke(this, new StepExecutionEventArgs(stepRecord, step));

            // Adımlar arası bekleme (Speed'e göre)
            await Task.Delay(GetStepDelay(), cancellationToken);
        }
        catch (Exception ex)
        {
            stepRecord.Status = StepExecutionStatus.Failed;
            stepRecord.ErrorMessage = ex.Message;
            stepRecord.EndTime = DateTime.Now;
            stepRecord.CalculateDuration();

            Log($"✗ Adım {step.StepNumber} başarısız: {ex.Message}");
            StepFailed?.Invoke(this, new StepExecutionEventArgs(stepRecord, step, ex.Message));
        }

        return stepRecord;
    }

    /// <summary>
    /// Target Selection adımını çalıştır
    /// </summary>
    private async Task ExecuteTargetSelectionAsync(TaskStep step, CancellationToken cancellationToken)
    {
        if (step.Target == null)
        {
            throw new InvalidOperationException("Target bilgisi boş!");
        }

        Log($"Hedef pencere aranıyor: {step.Target.WindowTitle}");

        // Pencereyi bul
        var window = await Task.Run(() => FindWindowByTarget(step.Target), cancellationToken);

        if (window == null)
        {
            throw new Exception($"Hedef pencere bulunamadı: {step.Target.WindowTitle}");
        }

        // Pencereyi aktif et (SetFocus)
        try
        {
            window.SetFocus();
            await Task.Delay(500, cancellationToken);
            Log($"✓ Pencere aktif edildi: {step.Target.WindowTitle}");
        }
        catch
        {
            // SetFocus başarısız olursa devam et
        }
    }

    /// <summary>
    /// UI Element Action adımını çalıştır
    /// </summary>
    private async Task ExecuteUIElementActionAsync(TaskStep step, CancellationToken cancellationToken)
    {
        if (step.UIElement == null || step.SelectedStrategy == null)
        {
            throw new InvalidOperationException("UIElement veya SelectedStrategy boş!");
        }

        Log($"Element aranıyor: {step.SelectedStrategy.Name}");

        // Akıllı stratejiler için önce SmartElementRecorder ile dene
        if (step.SelectedStrategy.RecordedElement != null)
        {
            var smartSuccess = await Task.Run(() =>
            {
                try
                {
                    return _smartElementPlayer.ExecuteLocatorStrategy(step.SelectedStrategy);
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[TaskChainExecutor] Smart strategy execution error: {ex.Message}");
                    return false;
                }
            }, cancellationToken);

            if (smartSuccess)
            {
                Log("✓ Akıllı strateji ile element etkileşimi başarıyla gerçekleştirildi.");
                return;
            }

            Log("⚠ Akıllı strateji element etkileşimi başarısız oldu, klasik aramaya dönülüyor.");
        }

        // Elementi bul - cancellation token ile timeout uygula
        AutomationElement? element = null;
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            // Güçlü stratejiler için daha uzun timeout, zayıf stratejiler için kısa timeout
            var timeoutSeconds = IsStrongStrategy(step.SelectedStrategy) ? 15 : 10;
            linkedCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                var elementInfo = step.UIElement ??
                                  (step.SelectedStrategy.RecordedElement != null
                                      ? SmartElementRecorder.ConvertToUIElementInfo(step.SelectedStrategy.RecordedElement)
                                      : null);

                if (elementInfo == null)
                {
                    throw new InvalidOperationException("UIElement bilgisi oluşturulamadı.");
                }

                // Eksik UIElement bilgisi varsa, oluşturduğumuz değeri sakla (ileride tekrar kullanım için)
                if (step.UIElement == null)
                {
                    step.UIElement = elementInfo;
                }

                element = await Task.Run(() => FindElementByStrategy(elementInfo, step.SelectedStrategy), linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw; // Kullanıcı iptal etti
                }
                else
                {
                    throw new TimeoutException($"Element arama işlemi {timeoutSeconds} saniye içinde tamamlanamadı.");
                }
            }
        }

        if (element == null)
        {
            throw new Exception($"Element bulunamadı: {step.SelectedStrategy.Name}");
        }

        Log($"✓ Element bulundu: {element.Current.Name}");

        // Action'ı uygula
        await Task.Run(() => ExecuteActionOnElement(element, step), cancellationToken);
    }

    /// <summary>
    /// Target'a göre window bul
    /// </summary>
    private AutomationElement? FindWindowByTarget(TargetInfo target)
    {
        var windows = AutomationElement.RootElement.FindAll(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
        );

        foreach (AutomationElement window in windows)
        {
            try
            {
                if (target.WindowTitle != null && window.Current.Name.Contains(target.WindowTitle))
                {
                    return window;
                }

                if (target.ProcessId.HasValue && window.Current.ProcessId == target.ProcessId.Value)
                {
                    return window;
                }
            }
            catch { }
        }

        return null;
    }

    /// <summary>
    /// Strateji ile element bul
    /// </summary>
    private AutomationElement? FindElementByStrategy(UIElementInfo elementInfo, ElementLocatorStrategy strategy)
    {
        // ElementLocatorTester'dan fonksiyonu kullan (elementInfo ile hızlandırma)
        return ElementLocatorTester.FindElementByStrategy(strategy, elementInfo);
    }

    /// <summary>
    /// Element üzerinde action gerçekleştir
    /// </summary>
    private void ExecuteActionOnElement(AutomationElement element, TaskStep step)
    {
        switch (step.Action)
        {
            case ActionType.LeftClick:
                ClickElement(element);
                break;

            case ActionType.RightClick:
                RightClickElement(element);
                break;

            case ActionType.DoubleClick:
                DoubleClickElement(element);
                break;

            case ActionType.MouseWheel:
                MouseWheelElement(element, step.MouseWheelDelta ?? 120);
                break;

            case ActionType.TypeText:
                TypeText(element, step.TextToType ?? "");
                break;

            case ActionType.KeyPress:
                PressKeys(element, step.KeysToPress ?? "");
                break;

            default:
                throw new NotSupportedException($"Action type desteklenmiyor: {step.Action}");
        }
    }

    /// <summary>
    /// Element'e tıkla
    /// </summary>
    private void ClickElement(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? pattern) &&
            pattern is InvokePattern invokePattern)
        {
            invokePattern.Invoke();
        }
        else
        {
            // Fallback: Mouse click
            var rect = element.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);
            MedulaAutomation.MouseClick(centerX, centerY);
        }
    }

    /// <summary>
    /// Element'e double-click
    /// </summary>
    private void DoubleClickElement(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseDoubleClick(centerX, centerY);
    }

    /// <summary>
    /// Element'e sağ tık
    /// </summary>
    private void RightClickElement(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseRightClick(centerX, centerY);
    }

    /// <summary>
    /// Element üzerinde mouse tekerlek
    /// </summary>
    private void MouseWheelElement(AutomationElement element, int delta)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseWheel(centerX, centerY, delta);
    }

    /// <summary>
    /// Element'e text yaz
    /// </summary>
    private void TypeText(AutomationElement element, string text)
    {
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern) &&
            pattern is ValuePattern valuePattern)
        {
            valuePattern.SetValue(text);
        }
        else
        {
            // Fallback: SendKeys
            element.SetFocus();
            System.Windows.Forms.SendKeys.SendWait(text);
        }
    }

    /// <summary>
    /// Klavye tuşlarına bas
    /// </summary>
    private void PressKeys(AutomationElement element, string keys)
    {
        element.SetFocus();
        System.Windows.Forms.SendKeys.SendWait(keys);
    }

    /// <summary>
    /// Screenshot çek
    /// </summary>
    private async Task<string> CaptureScreenshotAsync(TaskStep step)
    {
        return await Task.Run(() =>
        {
            try
            {
                var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");
                Directory.CreateDirectory(screenshotsDir);

                var fileName = $"step_{step.StepNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(screenshotsDir, fileName);

                var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                if (primaryScreen == null)
                {
                    return "";
                }

                using var bitmap = new Bitmap(
                    primaryScreen.Bounds.Width,
                    primaryScreen.Bounds.Height
                );
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                bitmap.Save(filePath, ImageFormat.Png);

                return filePath;
            }
            catch
            {
                return "";
            }
        });
    }

    /// <summary>
    /// Hata durumunda kullanıcıya sor
    /// </summary>
    private async Task<ErrorAction> HandleStepErrorAsync(TaskStep step, string errorMessage)
    {
        // Event ile kullanıcıya sor
        if (ErrorOccurred != null)
        {
            return await Task.Run(() => ErrorOccurred.Invoke(step, errorMessage));
        }

        // Default action
        return DefaultErrorAction;
    }

    /// <summary>
    /// Speed'e göre bekleme süresi
    /// </summary>
    private int GetStepDelay()
    {
        return Speed switch
        {
            ExecutionSpeed.Slow => 2000,
            ExecutionSpeed.Normal => 1000,
            ExecutionSpeed.Fast => 0,
            _ => 1000
        };
    }

    /// <summary>
    /// Çalıştırmayı duraklat
    /// </summary>
    public void Pause()
    {
        if (IsRunning && !_isPaused)
        {
            _isPaused = true;
            if (_currentRecord != null)
            {
                _currentRecord.Status = ExecutionStatus.Paused;
                Log("Task chain duraklatıldı.");
                ExecutionPaused?.Invoke(this, new ExecutionEventArgs(_currentRecord));
            }
        }
    }

    /// <summary>
    /// Çalıştırmaya devam et
    /// </summary>
    public void Resume()
    {
        if (IsRunning && _isPaused)
        {
            _isPaused = false;
            if (_currentRecord != null)
            {
                _currentRecord.Status = ExecutionStatus.Running;
                Log("Task chain devam ediyor.");
                ExecutionResumed?.Invoke(this, new ExecutionEventArgs(_currentRecord));
            }
        }
    }

    /// <summary>
    /// Çalıştırmayı durdur
    /// </summary>
    public void Stop()
    {
        if (IsRunning)
        {
            _cancellationTokenSource?.Cancel();
            Log("Task chain durduruluyor...");
        }
    }

    /// <summary>
    /// Conditional Branch adımını çalıştır (Tip 3)
    /// </summary>
    private async Task ExecuteConditionalBranchAsync(TaskStep step, CancellationToken cancellationToken)
    {
        if (step.Condition == null)
        {
            throw new InvalidOperationException("Condition bilgisi boş!");
        }

        Log($"Koşul değerlendiriliyor: {step.Condition.PageIdentifier ?? "Sayfa kontrolü"}");

        try
        {
            // Koşulları değerlendir
            string result = _conditionEvaluator.EvaluateConditions(step.Condition);
            Log($"Koşul sonucu: {result}");

            // DÖNGÜ SONLANMA MODU KONTROLÜ
            if (step.Condition.IsLoopTerminationMode)
            {
                Log("🔄 Döngü Sonlanma Modu aktif");

                if (result.ToLower() == "true")
                {
                    // Koşul TRUE → Programı sonlandır
                    Log("✅ Döngü sonlanma koşulu sağlandı → Program sonlandırılıyor");
                    _cancellationTokenSource?.Cancel();
                    return;
                }
                else
                {
                    // Koşul FALSE → Döngü devam eder, belirtilen adıma git
                    Log("➰ Döngü sonlanma koşulu sağlanmadı → Döngü devam ediyor");

                    // FALSE durumu için hedef dalı bul
                    string targetBranch = _conditionEvaluator.GetTargetBranch(step.Condition, result);

                    if (!string.IsNullOrEmpty(targetBranch))
                    {
                        Log($"↩ Geri dönüş hedefi: Adım {targetBranch}");
                        step.NextStepId = targetBranch;
                    }
                    else
                    {
                        Log($"⚠ Uyarı: FALSE durumu için hedef dal bulunamadı.");
                    }
                }
            }
            else
            {
                // NORMAL KOŞULLU DALLANMA MODU
                // Hangi dala gideceğini bul
                string targetBranch = _conditionEvaluator.GetTargetBranch(step.Condition, result);

                if (string.IsNullOrEmpty(targetBranch))
                {
                    Log($"⚠ Uyarı: Koşul sonucu '{result}' için hedef dal bulunamadı. Varsayılan akış devam ediyor.");
                    return;
                }

                Log($"✓ Dallanma hedefi: Adım {targetBranch}");
                step.NextStepId = targetBranch;
            }

            await Task.Delay(500, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Koşul değerlendirme hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Log mesajı
    /// </summary>
    /// <summary>
    /// Döngü sonlanma koşulunu kontrol et
    /// </summary>
    private async Task<bool> CheckLoopConditionAsync(TaskStep loopConditionStep, CancellationToken cancellationToken)
    {
        try
        {
            Log("Döngü sonlanma koşulu kontrol ediliyor...");

            // Gelişmiş kontrol: Koşul değerlendirici kullan
            if (loopConditionStep.Condition != null && _conditionEvaluator != null)
            {
                // Koşul değerlendirici ile kontrol et
                var conditionResult = _conditionEvaluator.EvaluateConditions(loopConditionStep.Condition);
                Log($"Koşul değerlendirme sonucu: {conditionResult}");

                // "true" döndürüldüyse döngü sonlanır
                return conditionResult.ToLower() == "true";
            }

            // Basit bir örnek: Kullanıcıya sor
            var result = await Task.Run(() =>
            {
                var dialogResult = MessageBox.Show(
                    "Döngü sonlandırılsın mı?\n\n" +
                    "Evet: Döngü sonlanır ve görev tamamlanır\n" +
                    "Hayır: Döngü devam eder",
                    "Döngü Sonlanma Kontrolü",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                return dialogResult == DialogResult.Yes;
            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            Log($"Döngü koşulu kontrolünde hata: {ex.Message}");
            return true; // Hata durumunda döngüyü sonlandır
        }
    }

    private void Log(string message)
    {
        LogMessage?.Invoke(this, message);
    }

    /// <summary>
    /// StepId'ye göre adım indexini bul
    /// </summary>
    private int FindStepIndexByStepId(string stepId)
    {
        if (_currentChain == null) return -1;

        for (int i = 0; i < _currentChain.Steps.Count; i++)
        {
            if (_currentChain.Steps[i].StepId == stepId)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Stratejinin güçlü/unique olup olmadığını kontrol eder
    /// </summary>
    private bool IsStrongStrategy(ElementLocatorStrategy strategy)
    {
        return strategy.Type switch
        {
            LocatorType.AutomationId => true,
            LocatorType.AutomationIdAndControlType => true,
            LocatorType.TreePath => true,
            LocatorType.ElementPath => true,
            LocatorType.XPath => true,
            LocatorType.HtmlId => true,
            LocatorType.PlaywrightSelector => true,
            LocatorType.Coordinates => true,
            _ => false
        };
    }
}
