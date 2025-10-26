using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Drawing;

namespace MedulaOtomasyon;

public class MedulaAutomation
{
    private const int DefaultRetryDelay = 600;
    private const int DefaultRetryCount = 5;
    private readonly Action<string> _logCallback;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    public MedulaAutomation(Action<string> logCallback)
    {
        _logCallback = logCallback;
    }

    private void Log(string message)
    {
        _logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public async Task RunAGrubuAsync()
    {
        try
        {
            Log("A Grubu otomasyonu başlatılıyor...");

            // 0. Hazırlık - MEDULA penceresini bul ve aktif hale getir
            var medulaWindow = await FindMedulaWindowAsync();
            if (medulaWindow == null)
            {
                Log("HATA: MEDULA penceresi bulunamadı!");
                return;
            }

            Log("MEDULA penceresi bulundu, aktif hale getiriliyor...");
            ActivateWindow(medulaWindow);
            await Task.Delay(2000); // IE tam yüklenmesi için bekle

            // 1. Reçete Listesi butonuna tıkla
            Log("Adım 1: Reçete Listesi butonuna tıklanıyor...");
            await ClickReceteListesiAsync(medulaWindow);
            await Task.Delay(2000);

            // 2. ComboBox'tan A Grubu seç ve Sorgula butonuna bas
            Log("Adım 2: ComboBox'tan 'A Grubu' seçiliyor...");
            await SelectAGrubuFromComboBoxAsync(medulaWindow);
            await Task.Delay(600);

            Log("Adım 2.1: Sorgula butonuna basılıyor...");
            await ClickSorgulaButtonAsync(medulaWindow);
            await Task.Delay(600);

            // 3. '1' numaralı satıra tıkla
            Log("Adım 3: '1' numaralı satıra tıklanıyor...");
            await ClickElementAsync(medulaWindow, "Name", "1", ControlType.Text);
            await Task.Delay(600);

            // Döngü - 4,5,6,7,8,9 adımları
            int prescriptionCount = 0;
            while (true)
            {
                prescriptionCount++;
                Log($"\n--- Reçete #{prescriptionCount} işleniyor ---");

                // 4. İlaç butonu
                Log("Adım 4: İlaç butonuna tıklanıyor...");
                await ClickElementAsync(medulaWindow, "AutomationId", "f:buttonIlacListesi", ControlType.Button);
                await Task.Delay(600);

                // 5. Y (Yazdır) butonu
                Log("Adım 5: Y (Yazdır) butonuna tıklanıyor...");
                await ClickElementAsync(medulaWindow, "AutomationId", "btnPrint", ControlType.Button);
                await Task.Delay(600);

                // 6. Bizden Alınmayanları Seç butonu
                Log("Adım 6: Bizden Alınmayanları Seç butonuna tıklanıyor...");
                await ClickElementAsync(medulaWindow, "AutomationId", "btnRaporlulariSec", ControlType.Button);
                await Task.Delay(600);

                // 7. Checkbox kontrolü ve dallanma
                Log("Adım 7: Checkbox'lar kontrol ediliyor...");
                var checkboxes = await FindCheckedCheckboxesAsync(medulaWindow);

                if (checkboxes.Count == 0)
                {
                    // 7A: Hiçbir checkbox işaretli değil
                    Log("7A: Hiçbir checkbox işaretli değil, pencere kapatılıyor...");
                    await CloseIlacListesiWindowAsync();
                }
                else
                {
                    // 7B: En az bir checkbox işaretli
                    Log($"7B: {checkboxes.Count} adet checkbox işaretli bulundu");

                    // 7B1: İlk işaretli checkbox'a sağ tık
                    Log("7B1: İlk checkbox'a sağ tık yapılıyor...");
                    await RightClickElementAsync(checkboxes[0]);
                    await Task.Delay(400);

                    // 7B2: Takip Et'e tıkla
                    Log("7B2: Takip Et seçeneğine tıklanıyor...");
                    await ClickTakipEtAsync();
                    await Task.Delay(400);

                    // 7B3: İlaç listesi penceresini kapat
                    Log("7B3: İlaç listesi penceresi kapatılıyor...");
                    await CloseIlacListesiWindowAsync();
                }

                await Task.Delay(600);

                // 8. Geri Dön butonu
                Log("Adım 8: Geri Dön butonuna tıklanıyor...");
                await ClickElementAsync(medulaWindow, "AutomationId", "form1:buttonGeriDon", ControlType.Button);
                await Task.Delay(600);

                // 9. Sonraki (İleri) butonu
                Log("Adım 9: Sonraki butonuna tıklanıyor...");
                await ClickElementAsync(medulaWindow, "AutomationId", "btnSonraki", ControlType.Button);
                await Task.Delay(600);

                // 10. Döngü bitişi kontrolü
                Log("Adım 10: Reçete kaydı kontrolü yapılıyor...");
                var endMessage = await FindElementAsync(medulaWindow, "Name", "Reçete kaydı bulunamadı.", ControlType.Text);
                if (endMessage != null)
                {
                    Log("\n✅ BAŞARILI: Tüm reçeteler işlendi! Toplam: " + prescriptionCount);
                    break;
                }
            }

            Log("\nA Grubu otomasyonu tamamlandı!");
        }
        catch (Exception ex)
        {
            Log($"HATA: {ex.Message}");
            Log($"Stack Trace: {ex.StackTrace}");
        }
    }

    private async Task<AutomationElement?> FindMedulaWindowAsync()
    {
        var currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

        for (int i = 0; i < DefaultRetryCount; i++)
        {
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
            );

            foreach (AutomationElement window in windows)
            {
                try
                {
                    var name = window.Current.Name;
                    var processId = window.Current.ProcessId;

                    // MEDULA ile başlayan VE bizim programımız olmayan pencereyi bul
                    // "MEDULA 2.1" formatında olmalı, "Medula Reçete Otomasyonu" değil
                    if (name.StartsWith("MEDULA", StringComparison.Ordinal) && // Büyük harfle başlamalı
                        processId != currentProcessId && // Bizim process değil
                        name.Contains("2.")) // Versiyon numarası içermeli
                    {
                        Log($"MEDULA penceresi bulundu: {name} (ProcessId: {processId})");
                        return window;
                    }
                }
                catch { }
            }

            if (i < DefaultRetryCount - 1)
            {
                await Task.Delay(DefaultRetryDelay);
            }
        }

        return null;
    }

    private void ActivateWindow(AutomationElement window)
    {
        var hwnd = new IntPtr(window.Current.NativeWindowHandle);
        ShowWindow(hwnd, SW_RESTORE);
        SetForegroundWindow(hwnd);
    }

    private async Task ClickReceteListesiAsync(AutomationElement window)
    {
        Log("Reçete Listesi butonu aranıyor...");

        // TreeScope.Descendants çok yavaş, FindAll kullanarak tüm butonları çek
        Log("Tüm butonlar listeleniyor...");

        AutomationElement? button = null;

        await Task.Run(() =>
        {
            try
            {
                var buttons = window.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)
                );

                Log($"{buttons.Count} buton bulundu, 'Reçete Listesi' aranıyor...");

                foreach (AutomationElement btn in buttons)
                {
                    try
                    {
                        var name = btn.Current.Name;

                        // "    Reçete Listesi" veya "Reçete Listesi"
                        if (name != null &&
                            (name == "    Reçete Listesi" ||
                             name.Trim() == "Reçete Listesi"))
                        {
                            button = btn;
                            Log($"✓ Buton bulundu: '{name}'");
                            break;
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log($"Buton arama hatası: {ex.Message}");
            }
        });

        if (button == null)
        {
            Log("HATA: Reçete Listesi butonu bulunamadı!");
            throw new Exception("Reçete Listesi butonu bulunamadı! Lütfen MEDULA'da doğru sayfada olduğunuzdan emin olun.");
        }

        Log($"✓ Reçete Listesi butonu bulundu!");
        Log($"  Name: '{button.Current.Name}'");
        Log($"  AutomationId: '{button.Current.AutomationId}'");
        Log($"  FrameworkId: '{button.Current.FrameworkId}'");
        Log($"  BoundingRectangle: {button.Current.BoundingRectangle}");
        Log($"  IsOffscreen: {button.Current.IsOffscreen}");

        InvokeElement(button);
    }

    private async Task SelectAGrubuFromComboBoxAsync(AutomationElement window)
    {
        Log("ComboBox aranıyor...");

        // 1. ComboBox'ı bul
        AutomationElement? comboBox = null;
        await Task.Run(() =>
        {
            try
            {
                var comboBoxes = window.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox)
                );

                Log($"{comboBoxes.Count} ComboBox bulundu");

                if (comboBoxes.Count > 0)
                {
                    comboBox = comboBoxes[0] as AutomationElement;
                    Log($"✓ ComboBox bulundu!");
                    Log($"  Name: '{comboBox.Current.Name}'");
                    Log($"  BoundingRectangle: {comboBox.Current.BoundingRectangle}");
                }
            }
            catch (Exception ex)
            {
                Log($"ComboBox arama hatası: {ex.Message}");
            }
        });

        if (comboBox == null)
        {
            throw new Exception("ComboBox bulunamadı!");
        }

        // 2. ComboBox'ı aç - ExpandCollapsePattern ile dene
        Log("ComboBox açılıyor...");
        bool opened = false;

        if (comboBox.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandPattern) &&
            expandPattern is ExpandCollapsePattern ecp)
        {
            try
            {
                var currentState = ecp.Current.ExpandCollapseState;
                Log($"ExpandCollapseState: {currentState}");

                if (currentState == ExpandCollapseState.Collapsed)
                {
                    Log("ExpandCollapsePattern.Expand() çağrılıyor...");
                    ecp.Expand();
                    opened = true;
                    await Task.Delay(800);
                    Log("✓ ComboBox ExpandCollapsePattern ile açıldı");
                }
                else
                {
                    Log("✓ ComboBox zaten açık");
                    opened = true;
                }
            }
            catch (Exception ex)
            {
                Log($"ExpandCollapsePattern hatası: {ex.Message}");
            }
        }

        // 3. Eğer ExpandCollapsePattern çalışmazsa, ComboBox'a tıkla
        if (!opened)
        {
            Log("ComboBox'a mouse click yapılıyor...");
            var rect = comboBox.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);
            Log($"Mouse click pozisyonu: ({centerX}, {centerY})");
            MouseClick(centerX, centerY);
            await Task.Delay(800);
            Log("✓ ComboBox mouse click ile açıldı");
        }

        // 4. A Grubu liste öğesini bul
        Log("A Grubu liste öğesi aranıyor...");
        AutomationElement? aGrubuItem = null;
        int attemptCount = 0;
        int maxAttempts = 5;

        while (aGrubuItem == null && attemptCount < maxAttempts)
        {
            attemptCount++;
            Log($"A Grubu arama denemesi {attemptCount}/{maxAttempts}...");

            await Task.Run(() =>
            {
                try
                {
                    // MEDULA process'ine ait tüm ListItem'ları ara
                    var medulaProcessId = window.Current.ProcessId;
                    var allElements = AutomationElement.RootElement.FindAll(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem)
                    );

                    Log($"{allElements.Count} liste öğesi bulundu (tüm sistem)");

                    foreach (AutomationElement elem in allElements)
                    {
                        try
                        {
                            // Sadece MEDULA process'ine ait olanları kontrol et
                            if (elem.Current.ProcessId == medulaProcessId)
                            {
                                var name = elem.Current.Name;
                                if (name == "A Grubu" || name.Trim() == "A Grubu")
                                {
                                    aGrubuItem = elem;
                                    Log($"✓ A Grubu bulundu!");
                                    Log($"  Name: '{name}'");
                                    Log($"  BoundingRectangle: {elem.Current.BoundingRectangle}");
                                    Log($"  IsOffscreen: {elem.Current.IsOffscreen}");
                                    Log($"  hwnd: 0x{elem.Current.NativeWindowHandle:X}");
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Arama hatası: {ex.Message}");
                }
            });

            if (aGrubuItem == null && attemptCount < maxAttempts)
            {
                Log($"A Grubu bulunamadı, {400 * attemptCount}ms bekleyip tekrar deneniyor...");
                await Task.Delay(400 * attemptCount); // 400ms, 800ms, 1200ms, 1600ms, 2000ms
            }
        }

        if (aGrubuItem == null)
        {
            throw new Exception("A Grubu liste öğesi bulunamadı! ComboBox açık olmayabilir veya A Grubu listede yok.");
        }

        // 5. A Grubu öğesini seç - SelectionItemPattern öncelikli
        Log("A Grubu seçiliyor...");
        bool selected = false;

        // Yöntem 1: SelectionItemPattern (önerilen)
        if (aGrubuItem.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionPattern) &&
            selectionPattern is SelectionItemPattern sip)
        {
            try
            {
                Log($"SelectionItemPattern kullanılarak seçiliyor... (IsSelected: {sip.Current.IsSelected})");

                // Zaten seçili değilse seç
                if (!sip.Current.IsSelected)
                {
                    sip.Select();
                    Log("✓ SelectionItemPattern ile seçildi");
                }
                else
                {
                    Log("✓ A Grubu zaten seçili");
                }

                selected = true;
            }
            catch (Exception ex)
            {
                Log($"SelectionItemPattern hatası: {ex.Message}");
            }
        }

        // Yöntem 2: InvokePattern
        if (!selected && aGrubuItem.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokePattern) &&
            invokePattern is InvokePattern ip)
        {
            try
            {
                Log("InvokePattern kullanılarak seçiliyor...");
                ip.Invoke();
                selected = true;
                Log("✓ InvokePattern ile seçildi");
            }
            catch (Exception ex)
            {
                Log($"InvokePattern hatası: {ex.Message}");
            }
        }

        // Yöntem 3: MouseClick (fallback)
        if (!selected)
        {
            Log("MouseClick kullanılarak seçiliyor...");

            // ScrollIntoView dene
            if (aGrubuItem.TryGetCurrentPattern(ScrollItemPattern.Pattern, out object? scrollPattern) &&
                scrollPattern is ScrollItemPattern scrollItem)
            {
                try
                {
                    scrollItem.ScrollIntoView();
                    await Task.Delay(300);
                }
                catch { }
            }

            var rect = aGrubuItem.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);
            Log($"Mouse click pozisyonu: ({centerX}, {centerY})");
            MouseClick(centerX, centerY);
            Log("✓ MouseClick ile seçildi");
        }

        await Task.Delay(1200); // Seçim yapılması ve sayfa yüklenmesi için bekle
        Log("✓ A Grubu seçim işlemi tamamlandı");
    }

    private async Task ClickSorgulaButtonAsync(AutomationElement window)
    {
        Log("Sorgula butonu aranıyor...");

        AutomationElement? button = null;

        // Yöntem 1: ElementPath ile ara
        try
        {
            Log("ElementPath ile deneniyor...");
            const string sorgulaPath = "Pane[Medula Eczane]/Table/Custom/Table/Custom/Table/Custom/Table/Custom/Button[Sorgula]";
            button = FindElementByPath(window, sorgulaPath);

            if (button != null)
            {
                Log("✓ ElementPath ile Sorgula butonu bulundu!");
            }
        }
        catch (Exception ex)
        {
            Log($"ElementPath hatası: {ex.Message}");
        }

        // Yöntem 2: AutomationId ile ara
        if (button == null)
        {
            Log("AutomationId ile deneniyor...");
            button = await FindElementAsync(window, "AutomationId", "form1:buttonSonlandirilmamisReceteler", ControlType.Button, retryCount: 2);

            if (button != null)
            {
                Log("✓ AutomationId ile Sorgula butonu bulundu!");
            }
        }

        // Yöntem 3: Name ile ara (tüm butonları tara)
        if (button == null)
        {
            Log("Name ile FindAll yapılıyor...");
            button = await FindElementByNameWithFindAll(window, "Sorgula", ControlType.Button);

            if (button != null)
            {
                Log("✓ Name ile Sorgula butonu bulundu!");
            }
        }

        if (button == null)
        {
            throw new Exception("Sorgula butonu bulunamadı! Lütfen MEDULA'da doğru sayfada olduğunuzdan emin olun.");
        }

        Log($"Sorgula butonuna basılıyor...");
        Log($"  Name: '{button.Current.Name}'");
        Log($"  BoundingRectangle: {button.Current.BoundingRectangle}");
        InvokeElement(button);
        Log("✓ Sorgula butonuna basıldı");
    }

    private async Task<AutomationElement?> FindElementAsync(
        AutomationElement parent,
        string propertyName,
        string propertyValue,
        ControlType controlType,
        int retryCount = DefaultRetryCount)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                var condition = propertyName switch
                {
                    "AutomationId" => new AndCondition(
                        new PropertyCondition(AutomationElement.AutomationIdProperty, propertyValue),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
                    ),
                    "Name" => new AndCondition(
                        new PropertyCondition(AutomationElement.NameProperty, propertyValue),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
                    ),
                    _ => throw new ArgumentException($"Unsupported property: {propertyName}")
                };

                var element = parent.FindFirst(TreeScope.Descendants, condition);
                if (element != null)
                {
                    return element;
                }
            }
            catch (Exception ex)
            {
                Log($"FindElement hatası (deneme {i + 1}/{retryCount}): {ex.Message}");
            }

            if (i < retryCount - 1)
            {
                await Task.Delay(DefaultRetryDelay);
            }
        }

        return null;
    }

    private void InvokeElement(AutomationElement element)
    {
        var frameworkId = element.Current.FrameworkId;
        var name = element.Current.Name;

        // Internet Explorer butonları için direkt mouse click kullan
        if (frameworkId == "InternetExplorer")
        {
            Log($"IE butonu tespit edildi, mouse click kullanılıyor: {name}");

            // ScrollIntoView dene
            if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out object? scrollPattern) && scrollPattern is ScrollItemPattern sip)
            {
                try
                {
                    sip.ScrollIntoView();
                    Thread.Sleep(200);
                }
                catch { }
            }

            var rect = element.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);

            Log($"Mouse click yapılıyor: ({centerX}, {centerY})");
            MouseClick(centerX, centerY);
            return;
        }

        // Diğer butonlar için InvokePattern dene
        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? pattern) && pattern is InvokePattern invokePattern)
        {
            Log($"InvokePattern kullanılıyor: {name}");
            invokePattern.Invoke();
        }
        else
        {
            // Fallback: Mouse click
            Log($"Fallback: Mouse click kullanılıyor: {name}");
            var rect = element.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);
            MouseClick(centerX, centerY);
        }
    }

    private async Task DoubleClickElementAsync(AutomationElement parent, string propertyName, string propertyValue, ControlType controlType)
    {
        Log($"Double-click için element aranıyor: {propertyName}='{propertyValue}', ControlType={controlType.ProgrammaticName}");

        // Önce hızlı FindElementAsync dene
        var element = await FindElementAsync(parent, propertyName, propertyValue, controlType, retryCount: 2);

        // Bulamazsa FindAll ile dene
        if (element == null)
        {
            Log($"FindElementAsync bulamadı, FindAll ile deneniyor...");
            element = await FindElementByNameWithFindAll(parent, propertyValue, controlType);
        }

        if (element == null)
        {
            throw new Exception($"Element bulunamadı: {propertyName}={propertyValue}");
        }

        Log($"✓ Element bulundu: '{element.Current.Name}'");
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);

        Log($"Double-click yapılıyor: ({centerX}, {centerY})");
        MouseDoubleClick(centerX, centerY);
    }

    private async Task ClickElementAsync(AutomationElement parent, string propertyName, string propertyValue, ControlType controlType)
    {
        Log($"Click için element aranıyor: {propertyName}='{propertyValue}', ControlType={controlType.ProgrammaticName}");

        // Önce hızlı FindElementAsync dene
        var element = await FindElementAsync(parent, propertyName, propertyValue, controlType, retryCount: 2);

        // Bulamazsa FindAll ile dene
        if (element == null)
        {
            Log($"FindElementAsync bulamadı, FindAll ile deneniyor...");
            element = await FindElementByNameWithFindAll(parent, propertyValue, controlType);
        }

        if (element == null)
        {
            throw new Exception($"Element bulunamadı: {propertyName}={propertyValue}");
        }

        Log($"✓ Element bulundu: '{element.Current.Name}'");
        InvokeElement(element);
    }

    private async Task<AutomationElement?> FindElementByNameWithFindAll(AutomationElement parent, string nameToFind, ControlType controlType)
    {
        AutomationElement? foundElement = null;

        await Task.Run(() =>
        {
            try
            {
                var elements = parent.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
                );

                Log($"{elements.Count} element bulundu (ControlType={controlType.ProgrammaticName}), '{nameToFind}' aranıyor...");

                foreach (AutomationElement elem in elements)
                {
                    try
                    {
                        var name = elem.Current.Name;

                        if (name != null &&
                            (name == nameToFind || name.Trim() == nameToFind.Trim()))
                        {
                            foundElement = elem;
                            Log($"✓ Element bulundu: '{name}'");
                            break;
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log($"FindElementByNameWithFindAll hatası: {ex.Message}");
            }
        });

        return foundElement;
    }

    private async Task<List<AutomationElement>> FindCheckedCheckboxesAsync(AutomationElement window)
    {
        var checkedItems = new List<AutomationElement>();

        try
        {
            // İlaç listesi penceresini bul
            var ilacWindow = await FindIlacListesiWindowAsync();
            if (ilacWindow == null)
            {
                Log("İlaç listesi penceresi bulunamadı");
                return checkedItems;
            }

            // "Seçim satır" ile başlayan tüm dataitem'ları bul
            var allItems = ilacWindow.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem)
            );

            foreach (AutomationElement item in allItems)
            {
                try
                {
                    var name = item.Current.Name;
                    if (name.StartsWith("Seçim satır", StringComparison.OrdinalIgnoreCase))
                    {
                        // Checkbox'ın seçili olup olmadığını kontrol et
                        if (IsCheckboxChecked(item))
                        {
                            checkedItems.Add(item);
                        }
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Log($"Checkbox arama hatası: {ex.Message}");
        }

        return checkedItems;
    }

    private bool IsCheckboxChecked(AutomationElement element)
    {
        try
        {
            // Value pattern kontrolü
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern) && valuePattern is ValuePattern vp)
            {
                return vp.Current.Value.Contains("Seçili", StringComparison.OrdinalIgnoreCase);
            }

            // Toggle pattern kontrolü
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togglePattern) && togglePattern is TogglePattern tp)
            {
                return tp.Current.ToggleState == ToggleState.On;
            }

            // SelectionItem pattern kontrolü
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionPattern) && selectionPattern is SelectionItemPattern sp)
            {
                return sp.Current.IsSelected;
            }
        }
        catch { }

        return false;
    }

    private async Task<AutomationElement?> FindIlacListesiWindowAsync()
    {
        for (int i = 0; i < DefaultRetryCount; i++)
        {
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
            );

            foreach (AutomationElement window in windows)
            {
                try
                {
                    var name = window.Current.Name;
                    if (name.Contains("İlaç Listesi", StringComparison.OrdinalIgnoreCase))
                    {
                        return window;
                    }
                }
                catch { }
            }

            if (i < DefaultRetryCount - 1)
            {
                await Task.Delay(200);
            }
        }

        return null;
    }

    private Task RightClickElementAsync(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);

        MouseRightClick(centerX, centerY);
        return Task.CompletedTask;
    }

    private async Task ClickTakipEtAsync()
    {
        await Task.Delay(100); // Context menu açılması için kısa bekle

        var desktop = AutomationElement.RootElement;

        // "Takip Et" menu item'ı bul
        var takipEt = desktop.FindFirst(
            TreeScope.Descendants,
            new AndCondition(
                new PropertyCondition(AutomationElement.NameProperty, "Takip Et"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem)
            )
        );

        if (takipEt == null)
        {
            throw new Exception("Takip Et menü öğesi bulunamadı!");
        }

        InvokeElement(takipEt);
    }

    private async Task CloseIlacListesiWindowAsync()
    {
        var ilacWindow = await FindIlacListesiWindowAsync();
        if (ilacWindow == null)
        {
            Log("İlaç listesi penceresi zaten kapalı");
            return;
        }

        // Pencereyi kapat - Window pattern kullan
        if (ilacWindow.TryGetCurrentPattern(WindowPattern.Pattern, out object? pattern) && pattern is WindowPattern windowPattern)
        {
            windowPattern.Close();
        }
        else
        {
            // Fallback: Alt+F4 gönder
            var hwnd = new IntPtr(ilacWindow.Current.NativeWindowHandle);
            SetForegroundWindow(hwnd);
            await Task.Delay(100);
            SendKeys.SendWait("%{F4}");
        }

        await Task.Delay(300);
    }

    #region Mouse Operations

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    private void MouseClick(int x, int y)
    {
        SetCursorPos(x, y);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    private void MouseDoubleClick(int x, int y)
    {
        SetCursorPos(x, y);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    private void MouseRightClick(int x, int y)
    {
        SetCursorPos(x, y);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
    }

    #endregion

    private AutomationElement? FindElementByPath(AutomationElement root, string elementPath)
    {
        try
        {
            Log($"ElementPath ile arama başlıyor: {elementPath}");

            // Path'i parse et: "Pane[Medula Eczane]/Table/Custom/Table/Custom/Button[Sorgula]"
            var parts = elementPath.Split('/');
            AutomationElement current = root;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                // Part'ı parse et: "Pane[Medula Eczane]" veya "Table"
                string controlTypeName;
                string? elementName = null;

                if (part.Contains('['))
                {
                    var bracketIndex = part.IndexOf('[');
                    controlTypeName = part.Substring(0, bracketIndex);
                    elementName = part.Substring(bracketIndex + 1, part.Length - bracketIndex - 2); // "[" ve "]" çıkar
                }
                else
                {
                    controlTypeName = part;
                }

                // ControlType'ı bul
                ControlType? controlType = controlTypeName switch
                {
                    "Pane" => ControlType.Pane,
                    "Table" => ControlType.Table,
                    "Custom" => ControlType.Custom,
                    "Button" => ControlType.Button,
                    "ComboBox" => ControlType.ComboBox,
                    "List" => ControlType.List,
                    "ListItem" => ControlType.ListItem,
                    "Text" => ControlType.Text,
                    _ => null
                };

                if (controlType == null)
                {
                    Log($"Bilinmeyen ControlType: {controlTypeName}");
                    return null;
                }

                // Element'i bul
                Condition condition;
                if (elementName != null)
                {
                    condition = new AndCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, controlType),
                        new PropertyCondition(AutomationElement.NameProperty, elementName)
                    );
                    Log($"Aranan: {controlTypeName}[{elementName}]");
                }
                else
                {
                    condition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
                    Log($"Aranan: {controlTypeName}");
                }

                var found = current.FindFirst(TreeScope.Children, condition);
                if (found == null)
                {
                    Log($"Bulunamadı: {part}");
                    return null;
                }

                Log($"✓ Bulundu: {found.Current.ControlType.ProgrammaticName} - {found.Current.Name}");
                current = found;
            }

            Log($"✓ ElementPath başarıyla çözüldü!");
            return current;
        }
        catch (Exception ex)
        {
            Log($"ElementPath arama hatası: {ex.Message}");
            return null;
        }
    }

    #region Debug Methods

    public void DebugListWindows(Action<string> log)
    {
        log("=== TÜM PENCERELER ===");

        var windows = AutomationElement.RootElement.FindAll(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
        );

        int count = 0;
        foreach (AutomationElement window in windows)
        {
            try
            {
                var name = window.Current.Name;
                var className = window.Current.ClassName;
                var processId = window.Current.ProcessId;

                log($"{++count}. Pencere: \"{name}\"");
                log($"   Class: {className}");
                log($"   ProcessId: {processId}");
                log($"   IsOffscreen: {window.Current.IsOffscreen}");
                log("");
            }
            catch (Exception ex)
            {
                log($"{++count}. Pencere okuma hatası: {ex.Message}\n");
            }
        }

        log($"Toplam {count} pencere bulundu.\n");
    }

    public void DebugFindButtons(Action<string> log)
    {
        try
        {
            var currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

            // Önce MEDULA penceresini bul
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
            );

            AutomationElement? medulaWindow = null;
            foreach (AutomationElement window in windows)
            {
                try
                {
                    var name = window.Current.Name;
                    var processId = window.Current.ProcessId;

                    if (name.StartsWith("MEDULA", StringComparison.Ordinal) &&
                        processId != currentProcessId &&
                        name.Contains("2."))
                    {
                        medulaWindow = window;
                        log($"✓ MEDULA penceresi bulundu: \"{name}\" (ProcessId: {processId})\n");
                        break;
                    }
                }
                catch { }
            }

            if (medulaWindow == null)
            {
                log("✗ MEDULA penceresi bulunamadı!\n");
                return;
            }

            // MEDULA penceresindeki tüm butonları listele
            log("=== MEDULA PENCERESİNDEKİ TÜM BUTONLAR ===");

            var buttons = medulaWindow.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)
            );

            int count = 0;
            foreach (AutomationElement button in buttons)
            {
                try
                {
                    count++;
                    var name = button.Current.Name;
                    var automationId = button.Current.AutomationId;
                    var className = button.Current.ClassName;
                    var isOffscreen = button.Current.IsOffscreen;

                    log($"{count}. Buton:");
                    log($"   Name: \"{name}\"");
                    log($"   AutomationId: \"{automationId}\"");
                    log($"   ClassName: \"{className}\"");
                    log($"   IsOffscreen: {isOffscreen}");

                    // Reçete Listesi butonunu özel olarak işaretle
                    if (name.Contains("Reçete", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Listesi", StringComparison.OrdinalIgnoreCase) ||
                        automationId.Contains("Recete", StringComparison.OrdinalIgnoreCase) ||
                        automationId.Contains("Liste", StringComparison.OrdinalIgnoreCase))
                    {
                        log("   >>> OLASI REÇETE LİSTESİ BUTONU <<<");
                    }

                    log("");

                    if (count >= 20) // İlk 20 butonu göster
                    {
                        log($"... ve daha fazla buton (toplam gösterilmedi)\n");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    log($"{count}. Buton okuma hatası: {ex.Message}\n");
                }
            }

            log($"Toplam {buttons.Count} buton bulundu (ilk {Math.Min(count, 20)} gösterildi).\n");
        }
        catch (Exception ex)
        {
            log($"Buton arama hatası: {ex.Message}\n");
        }
    }

    #endregion
}
