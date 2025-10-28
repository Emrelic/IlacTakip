using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Threading;

namespace MedulaOtomasyon
{
    /// <summary>
    /// Akıllı Element Kaydedici - Mouse ile tıklanan tablo satırlarını otomatik tespit eder
    /// </summary>
    public class SmartElementRecorder
    {
        #region Mouse Hook

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelMouseProc? _mouseProc;
        private IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        private bool _isRecording;
        private readonly List<RecordedElement> _recordedElements = new List<RecordedElement>();

        public event EventHandler<ElementRecordedEventArgs>? ElementRecorded;
        public event EventHandler<string>? RecordingStatusChanged;

        /// <summary>
        /// Kayıt modunu başlatır
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording) return;

            _mouseProc = HookCallback;
            _hookID = SetHook(_mouseProc);
            _isRecording = true;

            RecordingStatusChanged?.Invoke(this, "Recording STARTED - Click on table rows to record them");
            LogInfo("📹 Smart Recording başlatıldı. Tablo satırlarına tıklayın...");
        }

        /// <summary>
        /// Kayıt modunu durdurur
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording) return;

            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
            _isRecording = false;

            RecordingStatusChanged?.Invoke(this, "Recording STOPPED");
            LogInfo($"⏹️ Recording durduruldu. {_recordedElements.Count} element kaydedildi.");
        }

        /// <summary>
        /// Kaydedilen elementleri JSON olarak döndürür
        /// </summary>
        public string ExportRecordedElements()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(_recordedElements, options);
        }

        /// <summary>
        /// Kaydedilen elementleri diske yazar
        /// </summary>
        public void SaveRecordedElements(string filePath)
        {
            var json = ExportRecordedElements();
            File.WriteAllText(filePath, json, Encoding.UTF8);
            LogInfo($"Recorded elements saved to {filePath}");
        }

        /// <summary>
        /// Daha önce kaydedilmiş elementleri yükler
        /// </summary>
        public void LoadRecordedElements(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Recording file not found", filePath);
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var loaded = JsonSerializer.Deserialize<List<RecordedElement>>(json, options);
            if (loaded != null)
            {
                _recordedElements.Clear();
                _recordedElements.AddRange(loaded);
                LogInfo($"Loaded {loaded.Count} recorded elements from {filePath}");
            }
        }

        /// <summary>
        /// Kaydedilmiş bir elementi oynatır (tıklar)
        /// </summary>
        public bool PlaybackElement(RecordedElement element)
        {
            try
            {
                LogInfo($"▶️ Playback: {element.Description}");

                // 1. Önce XPath ile dene
                if (TryClickByXPath(element)) return true;

                // 2. Tablo ID + Row Index ile dene
                if (TryClickByTableRowIndex(element)) return true;

                // 3. Text içeriğine göre dene
                if (TryClickByTextContent(element)) return true;

                // 4. Son çare koordinat ile tıkla
                if (TryClickByCoordinates(element)) return true;

                LogError("❌ Element bulunamadı!");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Playback hatası: {ex.Message}");
                return false;
            }
        }

        #region Private Methods - Mouse Hook

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;

            if (curModule == null)
            {
                throw new InvalidOperationException("Current process module could not be resolved.");
            }

            var moduleName = curModule.ModuleName ?? string.Empty;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(moduleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var point = new Point(hookStruct.pt.x, hookStruct.pt.y);

                // Async olarak işle (hook'u bloklamayalım)
                System.Threading.Tasks.Task.Run(() => OnMouseClick(point));
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void OnMouseClick(Point screenPoint)
        {
            try
            {
                LogInfo($"🖱️ Click detected at ({screenPoint.X}, {screenPoint.Y})");
                LogInfo($"═══════════════════════════════════════════════════");

                // UI Automation ile elementi bul
                // System.Drawing.Point'i System.Windows.Point'e çevir
                var windowsPoint = new System.Windows.Point(screenPoint.X, screenPoint.Y);
                var element = AutomationElement.FromPoint(windowsPoint);
                if (element == null)
                {
                    LogWarning("❌ Element bulunamadı (UI Automation)");
                    return;
                }

                LogInfo($"✅ UI Automation Element bulundu:");
                LogInfo($"   Name: {element.Current.Name}");
                LogInfo($"   ClassName: {element.Current.ClassName}");
                LogInfo($"   ControlType: {element.Current.ControlType.ProgrammaticName}");

                if (IsRecorderWindowElement(element))
                {
                    LogInfo("ℹ️ Görev kaydedici arayüzü tıklaması algılandı, yok sayılıyor.");
                    return;
                }

                // Tablo satırı mı kontrol et
                var recordedElement = AnalyzeElement(element, screenPoint);

                if (recordedElement != null)
                {
                    _recordedElements.Add(recordedElement);

                    LogSuccess($"✅ ELEMENT BAŞARIYLA KAYDEDİLDİ!");
                    LogInfo($"═══════════════════════════════════════════════════");
                    LogInfo($"📝 Açıklama: {recordedElement.Description}");
                    LogInfo($"📊 Element Tipi: {recordedElement.ElementType}");

                    if (recordedElement.TableInfo != null)
                    {
                        LogInfo($"📋 Tablo Bilgileri:");
                        LogInfo($"   - TableId: {recordedElement.TableInfo.TableId ?? "N/A"}");
                        LogInfo($"   - RowIndex: {recordedElement.TableInfo.RowIndex}");
                        LogInfo($"   - HtmlTableId: {recordedElement.TableInfo.HtmlTableId ?? "N/A"}");
                        LogInfo($"   - HtmlRowIndex: {recordedElement.TableInfo.HtmlRowIndex}");
                        LogInfo($"   - Cell Count: {recordedElement.TableInfo.CellTexts?.Count ?? 0}");
                        LogInfo($"   - Cell IDs: {recordedElement.TableInfo.CellIds?.Count ?? 0}");

                        if (recordedElement.TableInfo.CellTexts?.Any() == true)
                        {
                            LogInfo($"   - Cell Texts: {string.Join(" | ", recordedElement.TableInfo.CellTexts.Take(3))}");
                        }
                    }

                    LogInfo($"🎯 Selectors:");
                    LogInfo($"   - XPath: {recordedElement.XPath ?? "N/A"}");
                    LogInfo($"   - CSS Selector: {recordedElement.CssSelector ?? "N/A"}");
                    LogInfo($"═══════════════════════════════════════════════════");

                    ElementRecorded?.Invoke(this, new ElementRecordedEventArgs(recordedElement));
                }
                else
                {
                    LogWarning("⚠️ Bu element bir tablo satırı değil veya analiz edilemedi.");
                }
            }
            catch (Exception ex)
            {
                LogError($"❌ Click analiz hatası: {ex.Message}");
                LogError($"   Stack Trace: {ex.StackTrace}");
            }
        }

        #endregion

        #region Private Methods - Element Analysis

        private bool IsRecorderWindowElement(AutomationElement element)
        {
            try
            {
                var window = GetTopLevelWindow(element);
                if (window == null) return false;

                var windowProcessId = window.Current.ProcessId;
                var currentProcessId = Process.GetCurrentProcess().Id;

                if (windowProcessId == currentProcessId)
                {
                    var windowName = window.Current.Name ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(windowName) &&
                        windowName.Contains("Görev Zinciri Kaydedici", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    var automationId = window.Current.AutomationId ?? string.Empty;
                    if (automationId.Equals("TaskChainRecorderForm", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // ignore detection failures
            }

            return false;
        }

        private AutomationElement? GetTopLevelWindow(AutomationElement element)
        {
            var walker = TreeWalker.RawViewWalker;
            var current = element;

            while (current != null)
            {
                try
                {
                    if (current.Current.ControlType == ControlType.Window)
                    {
                        return current;
                    }
                }
                catch
                {
                    break;
                }

                current = walker.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// Elementi analiz eder ve tablo satırı ise bilgilerini çıkarır
        /// </summary>
        private RecordedElement? AnalyzeElement(AutomationElement element, Point clickPoint)
        {
            LogInfo("🔍 === ANALİZ BAŞLIYOR ===");

            // 1. UIA üzerinden tablo satırını bulmayı dene
            LogInfo("1️⃣ UI Automation ile tablo satırı aranıyor...");
            var tableRow = FindTableRow(element);
            var table = tableRow != null ? FindParentTable(tableRow) : null;
            var uiaRowIndex = tableRow != null ? GetRowIndex(tableRow, table) : -1;
            var uiaCellTexts = tableRow != null ? GetCellTexts(tableRow) : new List<string>();

            if (tableRow != null)
            {
                LogSuccess($"✅ UIA Table Row bulundu!");
                LogInfo($"   - Table: {table?.Current.Name ?? "N/A"}");
                LogInfo($"   - Row Index: {uiaRowIndex}");
                LogInfo($"   - Cell Count: {uiaCellTexts.Count}");
            }
            else
            {
                LogWarning("⚠️ UIA ile tablo satırı bulunamadı");
            }

            // 2. HTML içeriğini bul (MSHTML için)
            LogInfo("2️⃣ MSHTML ile HTML bilgisi toplanıyor...");
            var htmlInfo = TryGetHtmlInfo(element, clickPoint);

            if (htmlInfo != null)
            {
                LogSuccess($"✅ HTML bilgisi toplandı!");
                LogInfo($"   - Tag: {htmlInfo.TagName}");
                LogInfo($"   - ID: {htmlInfo.Id ?? "N/A"}");
                LogInfo($"   - Class: {htmlInfo.ClassName ?? "N/A"}");
                LogInfo($"   - TableId: {htmlInfo.TableId ?? "N/A"}");
                LogInfo($"   - TableRowIndex: {htmlInfo.TableRowIndex}");
                LogInfo($"   - Text Content Count: {htmlInfo.TextContent?.Count ?? 0}");
                LogInfo($"   - Cell IDs Count: {htmlInfo.CellIds?.Count ?? 0}");

                if (htmlInfo.TextContent?.Any() == true)
                {
                    LogInfo($"   - First 3 Cells: {string.Join(" | ", htmlInfo.TextContent.Take(3))}");
                }
            }
            else
            {
                LogWarning("⚠️ HTML bilgisi toplanamadı");
            }

            // Eğer UIA satırı bulunamadıysa ve HTML bilgisi varsa tablo satırı gibi davran
            var isHtmlTableRow = htmlInfo != null && string.Equals(htmlInfo.TagName, "tr", StringComparison.OrdinalIgnoreCase);

            LogInfo($"3️⃣ Element tipi belirleniyor...");
            LogInfo($"   - UIA Table Row: {tableRow != null}");
            LogInfo($"   - HTML Table Row: {isHtmlTableRow}");

            if (tableRow == null && !isHtmlTableRow)
            {
                LogWarning("⚠️ Tablo satırı DEĞİL - Genel element olarak kaydediliyor");
                // Tablo satırı değil, genel element olarak kaydet
                return CreateGenericElement(element, clickPoint);
            }

            var cellTexts = htmlInfo?.TextContent?.Any() == true ? htmlInfo.TextContent : uiaCellTexts;
            var htmlRowIndex = htmlInfo?.TableRowIndex ?? -1;
            var effectiveRowIndex = uiaRowIndex >= 0 ? uiaRowIndex : htmlRowIndex;

            LogInfo($"4️⃣ RecordedElement oluşturuluyor...");
            LogInfo($"   - Effective Row Index: {effectiveRowIndex}");
            LogInfo($"   - Cell Texts Count: {cellTexts.Count}");

            // 4. RecordedElement oluştur
            var recordedElement = new RecordedElement
            {
                ElementType = "TableRow",
                Timestamp = DateTime.Now,
                ScreenPoint = clickPoint,

                // UI Automation bilgileri
                AutomationId = tableRow?.Current.AutomationId,
                Name = tableRow?.Current.Name,
                ClassName = tableRow?.Current.ClassName,
                ControlType = tableRow?.Current.ControlType.ProgrammaticName,

                // Tablo bilgileri
                TableInfo = new TableInfo
                {
                    TableId = table?.Current.AutomationId,
                    TableName = table?.Current.Name,
                    RowIndex = effectiveRowIndex,
                    CellTexts = cellTexts,
                    HtmlTableId = htmlInfo?.TableId,
                    HtmlTableClass = htmlInfo?.TableClassName,
                    HtmlRowIndex = htmlRowIndex,
                    CellIds = htmlInfo?.CellIds ?? new List<string>()
                },

                // HTML bilgileri
                HtmlInfo = htmlInfo,

                // XPath ve Selector
                XPath = GenerateXPath(htmlInfo),
                CssSelector = GenerateCssSelector(htmlInfo),

                // Açıklama
                Description = $"Table Row #{effectiveRowIndex}: {string.Join(" | ", cellTexts.Take(3))}"
            };

            LogSuccess($"✅ RecordedElement oluşturuldu!");
            PopulateWindowInfo(recordedElement, element);
            return recordedElement;
        }

        /// <summary>
        /// Parent'lara çıkarak tablo satırını bulur
        /// </summary>
        private AutomationElement? FindTableRow(AutomationElement element)
        {
            var current = element;
            int depth = 0;

            while (current != null && depth < 10)
            {
                try
                {
                    // TR elementi mi kontrol et
                    var className = current.Current.ClassName?.ToLower() ?? "";
                    var localizedControl = current.Current.LocalizedControlType?.ToLower() ?? "";

                    // Tablo satırı olabilecek durumlar:
                    // 1. ClassName'de "row" veya "tr" var
                    // 2. ControlType TableItem veya Custom + parent'ı Table
                    if (className.Contains("row") ||
                        className == "tr" ||
                        localizedControl.Contains("row") ||
                        current.Current.ControlType == ControlType.DataItem ||
                        current.Current.ControlType == ControlType.ListItem)
                    {
                        // Parent'ı tablo mu kontrol et
                        var parent = TreeWalker.RawViewWalker.GetParent(current);
                        if (parent != null && IsTable(parent))
                        {
                            return current;
                        }
                    }

                    current = TreeWalker.RawViewWalker.GetParent(current);
                    depth++;
                }
                catch
                {
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// Bir elementin tablo olup olmadığını kontrol eder
        /// </summary>
        private bool IsTable(AutomationElement element)
        {
            try
            {
                var className = element.Current.ClassName?.ToLower() ?? "";
                var controlType = element.Current.ControlType;
                var localizedControl = element.Current.LocalizedControlType?.ToLower() ?? "";

                return className.Contains("table") ||
                       className.Contains("grid") ||
                       controlType == ControlType.Table ||
                       controlType == ControlType.DataGrid ||
                       localizedControl.Contains("table") ||
                       localizedControl.Contains("grid");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tablo elementini bulur
        /// </summary>
        private AutomationElement? FindParentTable(AutomationElement row)
        {
            var current = TreeWalker.RawViewWalker.GetParent(row);
            int depth = 0;

            while (current != null && depth < 5)
            {
                if (IsTable(current)) return current;
                current = TreeWalker.RawViewWalker.GetParent(current);
                depth++;
            }

            return null;
        }

        /// <summary>
        /// Satırın index'ini bulur
        /// </summary>
        private int GetRowIndex(AutomationElement row, AutomationElement? table)
        {
            if (table == null) return -1;

            try
            {
                int index = 0;
                var walker = TreeWalker.RawViewWalker;
                var sibling = walker.GetFirstChild(table);

                while (sibling != null)
                {
                    if (Automation.Compare(sibling, row))
                        return index;

                    index++;
                    sibling = walker.GetNextSibling(sibling);
                }
            }
            catch { }

            return -1;
        }

        /// <summary>
        /// Satırdaki hücre metinlerini alır
        /// </summary>
        private List<string> GetCellTexts(AutomationElement row)
        {
            var texts = new List<string>();

            try
            {
                var walker = TreeWalker.RawViewWalker;
                var cell = walker.GetFirstChild(row);

                while (cell != null)
                {
                    var text = GetElementText(cell);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        texts.Add(text.Trim());
                    }

                    cell = walker.GetNextSibling(cell);
                }
            }
            catch { }

            return texts;
        }

        /// <summary>
        /// Bir elementin text içeriğini alır
        /// </summary>
        private string GetElementText(AutomationElement element)
        {
            try
            {
                // 1. Name property
                if (!string.IsNullOrWhiteSpace(element.Current.Name))
                    return element.Current.Name;

                // 2. Value Pattern
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern))
                {
                    var value = ((ValuePattern)valuePattern).Current.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                // 3. Text Pattern
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out object textPattern))
                {
                    var text = ((TextPattern)textPattern).DocumentRange.GetText(-1);
                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }

                // 4. Child text'leri topla
                var childTexts = new List<string>();
                var walker = TreeWalker.RawViewWalker;
                var child = walker.GetFirstChild(element);

                while (child != null && childTexts.Count < 10)
                {
                    var childText = child.Current.Name;
                    if (!string.IsNullOrWhiteSpace(childText))
                        childTexts.Add(childText.Trim());

                    child = walker.GetNextSibling(child);
                }

                return string.Join(" ", childTexts);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// MSHTML kullanarak HTML bilgilerini almaya çalışır (dynamic COM interop)
        /// </summary>
        private HtmlInfo? TryGetHtmlInfo(AutomationElement element, Point screenPoint)
        {
            try
            {
                LogInfo($"🌐 MSHTML analizi başlatılıyor...");
                LogInfo($"   Click point: ({screenPoint.X}, {screenPoint.Y})");

                // WebBrowser kontrolünü bul
                var browser = FindWebBrowserControl(element);
                if (browser == null)
                {
                    LogWarning("❌ WebBrowser kontrolü bulunamadı");
                    return null;
                }

                LogSuccess($"✅ WebBrowser kontrolü bulundu");
                LogInfo($"   Browser ClassName: {browser.Current.ClassName}");
                LogInfo($"   Browser PID: {browser.Current.ProcessId}");
                LogInfo($"   Browser HWND: {browser.Current.NativeWindowHandle}");

                // MSHTML Document'i al (dynamic olarak)
                dynamic? doc = GetHtmlDocument(browser);
                if (doc == null)
                {
                    LogWarning("❌ HTML Document bulunamadı");
                    return null;
                }

                LogSuccess($"✅ HTML Document bulundu");

                // Screen koordinatlarını client koordinatlara çevir
                var browserRect = browser.Current.BoundingRectangle;
                int clientX = screenPoint.X - (int)browserRect.Left;
                int clientY = screenPoint.Y - (int)browserRect.Top;

                LogInfo($"   Browser Rect: ({browserRect.Left}, {browserRect.Top})");
                LogInfo($"   Client coordinates (before scroll): ({clientX}, {clientY})");

                ApplyScrollOffsets(doc, ref clientX, ref clientY);

                LogInfo($"   Client coordinates (after scroll): ({clientX}, {clientY})");

                // elementFromPoint ile HTML elementini bul
                dynamic htmlElement = doc.elementFromPoint(clientX, clientY);
                if (htmlElement == null)
                {
                    LogWarning("❌ HTML element bulunamadı (elementFromPoint)");
                    return null;
                }

                string initialTag = htmlElement.tagName?.ToString() ?? "N/A";
                LogInfo($"✅ HTML element bulundu: {initialTag}");

                dynamic rowElement = FindParentTr(htmlElement) ?? htmlElement;
                string tagName = rowElement.tagName?.ToString() ?? htmlElement.tagName?.ToString() ?? string.Empty;
                string id = rowElement.id?.ToString() ?? htmlElement.id?.ToString() ?? string.Empty;
                string className = rowElement.className?.ToString() ?? htmlElement.className?.ToString() ?? string.Empty;

                LogInfo($"📌 Final element: {tagName}");
                LogInfo($"   - ID: {(string.IsNullOrEmpty(id) ? "N/A" : id)}");
                LogInfo($"   - ClassName: {(string.IsNullOrEmpty(className) ? "N/A" : className)}");

                var htmlInfo = new HtmlInfo
                {
                    TagName = tagName.ToLowerInvariant(),
                    Id = id,
                    ClassName = className,
                    Attributes = new Dictionary<string, string>(),
                    BrowserProcessId = browser.Current.ProcessId,
                    BrowserWindowHandle = browser.Current.NativeWindowHandle
                };

                // Tablo bilgisini topla
                LogInfo($"📋 Parent TABLE aranıyor...");
                dynamic? tableElement = FindParentTableElement(rowElement);
                if (tableElement != null)
                {
                    htmlInfo.TableId = tableElement.id?.ToString();
                    htmlInfo.TableClassName = tableElement.className?.ToString();
                    htmlInfo.TableRowIndex = GetHtmlRowIndex(tableElement, rowElement);

                    LogSuccess($"✅ TABLE bulundu!");
                    LogInfo($"   - Table ID: {htmlInfo.TableId ?? "N/A"}");
                    LogInfo($"   - Table Class: {htmlInfo.TableClassName ?? "N/A"}");
                    LogInfo($"   - Row Index: {htmlInfo.TableRowIndex}");
                }
                else
                {
                    LogWarning("⚠️ Parent TABLE bulunamadı");
                }

                // Text ve hücre ID bilgilerini topla
                LogInfo($"📊 Hücre (TD) bilgileri toplanıyor...");
                try
                {
                    var collectedIds = new HashSet<string>(StringComparer.Ordinal);

                    dynamic cells = rowElement.cells;
                    if (cells != null)
                    {
                        int cellCount;
                        try { cellCount = (int)cells.length; }
                        catch { cellCount = 0; }

                        LogInfo($"   Toplam hücre sayısı: {cellCount}");

                        for (int i = 0; i < cellCount; i++)
                        {
                            dynamic cell = cells.item(i);
                            if (cell == null) continue;

                            string cellText = cell.innerText?.ToString().Trim() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(cellText))
                            {
                                htmlInfo.TextContent.Add(cellText);
                                LogInfo($"   Cell[{i}]: {cellText}");
                            }

                            CollectElementIdentifiers(cell, collectedIds);
                        }
                    }
                    else
                    {
                        LogWarning("   ⚠️ rowElement.cells NULL");
                    }

                    if (collectedIds.Count > 0)
                    {
                        htmlInfo.CellIds.Clear();
                        htmlInfo.CellIds.AddRange(collectedIds);
                        LogInfo($"   Toplanan Cell IDs: {string.Join(", ", collectedIds)}");
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"   ❌ Hücre okuma hatası: {ex.Message}");
                }

                // Önemli attribute'ları topla
                try
                {
                    var onclickAttr = rowElement.getAttribute("onclick");
                    if (onclickAttr != null)
                    {
                        htmlInfo.Attributes["onclick"] = onclickAttr.ToString();
                        LogInfo($"   onclick attribute bulundu");
                    }

                    var nameAttr = rowElement.getAttribute("name");
                    if (nameAttr != null)
                    {
                        htmlInfo.Attributes["name"] = nameAttr.ToString();
                        LogInfo($"   name attribute bulundu: {nameAttr}");
                    }
                }
                catch { }

                LogSuccess($"✅ HtmlInfo oluşturuldu!");
                LogInfo($"   - Text Content Count: {htmlInfo.TextContent.Count}");
                LogInfo($"   - Cell IDs Count: {htmlInfo.CellIds.Count}");
                LogInfo($"   - Attributes Count: {htmlInfo.Attributes.Count}");

                return htmlInfo;
            }
            catch (Exception ex)
            {
                LogError($"❌ MSHTML hatası: {ex.Message}");
                LogError($"   Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// WebBrowser/InternetExplorer kontrolünü bulur
        /// </summary>
        private AutomationElement? FindWebBrowserControl(AutomationElement element)
        {
            var current = element;
            int depth = 0;

            while (current != null && depth < 20)
            {
                try
                {
                    var className = current.Current.ClassName?.ToLower() ?? "";
                    if (className.Contains("internet explorer") ||
                        className.Contains("internetexplorer") ||
                        className.Contains("webview") ||
                        className.Contains("browser") ||
                        current.Current.ControlType == ControlType.Document ||
                        current.Current.ControlType == ControlType.Pane)
                    {
                        // Document kontrolü mü kontrol et
                        if (current.Current.ControlType == ControlType.Document)
                        {
                            return current;
                        }
                    }

                    current = TreeWalker.RawViewWalker.GetParent(current);
                    depth++;
                }
                catch
                {
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// AutomationElement'ten MSHTML Document'i alır (dynamic)
        /// </summary>
        private dynamic? GetHtmlDocument(AutomationElement element)
        {
            try
            {
                // ValuePattern ile document interface'ini al
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern))
                {
                    // Bazen ValuePattern içinde document olabiliyor
                }

                // Native interface üzerinden document'e ulaş
                var nativeElement = element.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty);
                if (nativeElement != null && nativeElement is int hwnd && hwnd != 0)
                {
                    return GetHtmlDocumentFromHwnd(hwnd);
                }

                // Alternatif: Root'tan başlayarak IHTMLDocument2 bul
                return FindHtmlDocumentInProcess();
            }
            catch (Exception ex)
            {
                LogError($"GetHtmlDocument error: {ex.Message}");
                return null;
            }
        }

        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(
            int hwnd,
            uint dwObjectID,
            ref Guid riid,
            ref IntPtr ppvObject);

        private const uint OBJID_WINDOW = 0x00000000;

        /// <summary>
        /// Window handle'dan HTML Document interface'ini alır (dynamic)
        /// </summary>
        private dynamic? GetHtmlDocumentFromHwnd(int hwnd)
        {
            try
            {
                Guid IID_IHTMLDocument2 = new Guid("626FC520-A41E-11CF-A731-00A0C9082637");
                IntPtr pDoc = IntPtr.Zero;

                int result = AccessibleObjectFromWindow(hwnd, OBJID_WINDOW, ref IID_IHTMLDocument2, ref pDoc);

                if (pDoc != IntPtr.Zero)
                {
                    dynamic doc = Marshal.GetObjectForIUnknown(pDoc);
                    Marshal.Release(pDoc);
                    return doc;
                }
            }
            catch (Exception ex)
            {
                LogError($"GetHtmlDocumentFromHwnd error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Process'teki açık IE/MSHTML window'lardan document bulur (dynamic COM)
        /// </summary>
        private dynamic? FindHtmlDocumentInProcess(int? targetHwnd = null, int? targetProcessId = null)
        {
            try
            {
                // SHDocVw COM type'ını oluştur
                Type? shellWindowsType = Type.GetTypeFromProgID("Shell.Application");
                if (shellWindowsType == null)
                {
                    LogWarning("Shell.Application COM type not found");
                    return null;
                }

                dynamic shellWindows = Activator.CreateInstance(shellWindowsType)!;
                dynamic windows = shellWindows.Windows();

                // Tüm IE/Browser window'larını kontrol et
                for (int i = 0; i < windows.Count; i++)
                {
                    try
                    {
                        dynamic window = windows.Item(i);
                        dynamic doc = window.Document;

                        if (doc != null)
                        {
                            string url = doc.url?.ToString() ?? "";
                            LogInfo($"Found HTML document: {url}");
                            try
                            {
                                int? windowHandle = null;
                                try { windowHandle = (int)window.HWND; }
                                catch { }

                                if (!targetHwnd.HasValue || (windowHandle.HasValue && windowHandle.Value == targetHwnd.Value))
                                {
                                    return doc;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                LogError($"FindHtmlDocumentInProcess error: {ex.Message}");
            }

            return null;
        }

        private dynamic? GetDocumentForRecordedElement(RecordedElement element)
        {
            try
            {
                var htmlInfo = element.HtmlInfo;
                if (htmlInfo != null)
                {
                    if (htmlInfo.BrowserWindowHandle != 0)
                    {
                        var handleDoc = GetHtmlDocumentFromHwnd(htmlInfo.BrowserWindowHandle);
                        if (handleDoc != null)
                        {
                            LogInfo("Using cached browser window handle for HTML document");
                            return handleDoc;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"GetDocumentForRecordedElement (handle) failed: {ex.Message}");
            }

            int? targetHwnd = null;
            if (element.HtmlInfo?.BrowserWindowHandle > 0)
            {
                targetHwnd = element.HtmlInfo.BrowserWindowHandle;
            }

            int? targetPid = null;
            if (element.HtmlInfo?.BrowserProcessId > 0)
            {
                targetPid = element.HtmlInfo.BrowserProcessId;
            }

            var resolvedDoc = FindHtmlDocumentInProcess(targetHwnd, targetPid);
            if (resolvedDoc != null)
                return resolvedDoc;

            return FindHtmlDocumentInProcess();
        }

        /// <summary>
        /// HTML elementinin parent TR elementini bulur (dynamic)
        /// </summary>
        private dynamic? FindParentTr(dynamic element)
        {
            try
            {
                object? currentObj = element;
                int depth = 0;

                while (currentObj != null && depth < 10)
                {
                    dynamic current = currentObj;
                    string tagName = current.tagName?.ToString() ?? string.Empty;
                    if (string.Equals(tagName, "TR", StringComparison.OrdinalIgnoreCase))
                    {
                        return current;
                    }

                    currentObj = current.parentElement;
                    depth++;
                }
            }
            catch (Exception ex)
            {
                LogError($"FindParentTr error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// HTML elementinin parent TABLE elementini bulur (dynamic)
        /// </summary>
        private dynamic? FindParentTableElement(dynamic element)
        {
            try
            {
                object? currentObj = element;
                int depth = 0;

                while (currentObj != null && depth < 20)
                {
                    dynamic current = currentObj;
                    string tagName = current.tagName?.ToString() ?? string.Empty;
                    if (string.Equals(tagName, "TABLE", StringComparison.OrdinalIgnoreCase))
                    {
                        return current;
                    }

                    currentObj = current.parentElement;
                    depth++;
                }
            }
            catch (Exception ex)
            {
                LogError($"FindParentTableElement error: {ex.Message}");
            }

            return null;
        }

        private int GetHtmlRowIndex(dynamic tableElement, dynamic rowElement)
        {
            try
            {
                dynamic rows = tableElement.rows;
                if (rows == null) return -1;

                int length;
                try { length = (int)rows.length; }
                catch { return -1; }

                int targetSourceIndex = GetSourceIndex(rowElement);

                for (int i = 0; i < length; i++)
                {
                    dynamic candidate = rows.item(i);
                    if (candidate == null) continue;

                    if (ReferenceEquals(candidate, rowElement))
                    {
                        return i;
                    }

                    int candidateIndex = GetSourceIndex(candidate);
                    if (targetSourceIndex >= 0 && candidateIndex == targetSourceIndex)
                    {
                        return i;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"GetHtmlRowIndex failed: {ex.Message}");
            }

            return -1;
        }

        private int GetSourceIndex(dynamic element)
        {
            try
            {
                return (int)element.sourceIndex;
            }
            catch
            {
                return -1;
            }
        }

        private dynamic? GetElementById(dynamic doc, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            try
            {
                return doc.getElementById(id);
            }
            catch (Exception ex)
            {
                LogWarning($"getElementById error: {ex.Message}");
                return null;
            }
        }

        private dynamic? FindElementBySimpleXPath(dynamic doc, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xpath)) return null;

            try
            {
                var trimmed = xpath.Trim();

                var idMatch = Regex.Match(trimmed, @"//\*\[@id='(?<id>[^']+)'\]");
                if (idMatch.Success)
                {
                    return GetElementById(doc, idMatch.Groups["id"].Value);
                }

                var rowMatch = Regex.Match(trimmed, @"//table\[@id='(?<table>[^']+)'\]//tr\[position\(\)=(?<index>\d+)\]");
                if (rowMatch.Success)
                {
                    var tableId = rowMatch.Groups["table"].Value;
                    if (int.TryParse(rowMatch.Groups["index"].Value, out int pos) && pos > 0)
                    {
                        return GetTableRowByIndex(doc, tableId, pos - 1);
                    }
                }

                var classMatch = Regex.Match(trimmed, @"//(?<tag>[a-zA-Z0-9]+)\[@class='(?<class>[^']+)'\]");
                if (classMatch.Success)
                {
                    return FindElementByTagAndClass(doc, classMatch.Groups["tag"].Value, classMatch.Groups["class"].Value);
                }

                return null;
            }
            catch (Exception ex)
            {
                LogWarning($"FindElementBySimpleXPath error: {ex.Message}");
                return null;
            }
        }

        private dynamic? FindRowByHtmlInfo(dynamic doc, HtmlInfo htmlInfo)
        {
            // Table id + index
            if (!string.IsNullOrEmpty(htmlInfo.TableId) && htmlInfo.TableRowIndex >= 0)
            {
                var row = GetTableRowByIndex(doc, htmlInfo.TableId, htmlInfo.TableRowIndex);
                if (row != null) return row;
            }

            // Class + text fallback
            if (htmlInfo.TextContent?.Any() == true)
            {
                var allElements = doc.all;
                if (allElements != null)
                {
                    int count;
                    try { count = (int)allElements.length; }
                    catch { count = 0; }

                    for (int i = 0; i < count; i++)
                    {
                        dynamic candidate = allElements.item(i);
                        if (candidate == null) continue;

                        string tagName = candidate.tagName?.ToString() ?? string.Empty;
                        if (!string.Equals(tagName, "TR", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var cells = candidate.cells;
                        if (cells == null) continue;

                        var texts = new List<string>();
                        int length = 0;
                        try { length = (int)cells.length; }
                        catch { continue; }

                        for (int j = 0; j < length; j++)
                        {
                            dynamic cell = cells.item(j);
                            if (cell == null) continue;
                            string text = cell.innerText?.ToString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                texts.Add(text);
                            }
                        }

                        if (CellTextsMatch(texts, htmlInfo.TextContent))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }

        private dynamic? GetTableRowByIndex(dynamic doc, string tableId, int rowIndex)
        {
            if (string.IsNullOrEmpty(tableId) || rowIndex < 0) return null;

            try
            {
                dynamic table = GetElementById(doc, tableId);
                if (table == null)
                {
                    // Fallback: tüm tablolarda ara
                    var all = doc.all;
                    if (all != null)
                    {
                        int count;
                        try { count = (int)all.length; }
                        catch { count = 0; }

                        for (int i = 0; i < count; i++)
                        {
                            dynamic candidate = all.item(i);
                            if (candidate == null) continue;
                            string tagName = candidate.tagName?.ToString() ?? string.Empty;
                            if (!string.Equals(tagName, "TABLE", StringComparison.OrdinalIgnoreCase))
                                continue;

                            string candidateId = candidate.id?.ToString() ?? string.Empty;
                            if (string.Equals(candidateId, tableId, StringComparison.Ordinal))
                            {
                                table = candidate;
                                break;
                            }
                        }
                    }
                }

                if (table == null) return null;

                dynamic rows = table.rows;
                if (rows == null) return null;

                int countRows;
                try { countRows = (int)rows.length; }
                catch { return null; }

                if (rowIndex >= countRows) return null;

                return rows.item(rowIndex);
            }
            catch (Exception ex)
            {
                LogWarning($"GetTableRowByIndex error: {ex.Message}");
                return null;
            }
        }

        private dynamic? FindElementByTagAndClass(dynamic doc, string tag, string @class)
        {
            try
            {
                var all = doc.all;
                if (all == null) return null;

                int count;
                try { count = (int)all.length; }
                catch { return null; }

                for (int i = 0; i < count; i++)
                {
                    dynamic element = all.item(i);
                    if (element == null) continue;

                    string tagName = element.tagName?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(tag) && !string.Equals(tagName, tag, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string className = element.className?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(className)) continue;

                    var classes = className.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (classes.Any(c => string.Equals(c, @class, StringComparison.OrdinalIgnoreCase)))
                    {
                        return element;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"FindElementByTagAndClass error: {ex.Message}");
            }

            return null;
        }

        private void ApplyScrollOffsets(dynamic doc, ref int x, ref int y)
        {
            try
            {
                int scrollX = GetScrollOffset(doc, axisX: true);
                int scrollY = GetScrollOffset(doc, axisX: false);

                x += scrollX;
                y += scrollY;
            }
            catch (Exception ex)
            {
                LogWarning($"ApplyScrollOffsets failed: {ex.Message}");
            }
        }

        private int GetScrollOffset(dynamic doc, bool axisX)
        {
            try
            {
                dynamic window = doc.parentWindow;
                if (window != null)
                {
                    try
                    {
                        int offset = axisX ? (int)window.pageXOffset : (int)window.pageYOffset;
                        if (offset != 0) return offset;
                    }
                    catch { }

                    try
                    {
                        int offset = axisX ? (int)window.scrollX : (int)window.scrollY;
                        if (offset != 0) return offset;
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                dynamic documentElement = doc.documentElement;
                if (documentElement != null)
                {
                    int offset = axisX ? (int)documentElement.scrollLeft : (int)documentElement.scrollTop;
                    if (offset != 0) return offset;
                }
            }
            catch { }

            try
            {
                dynamic body = doc.body;
                if (body != null)
                {
                    int offset = axisX ? (int)body.scrollLeft : (int)body.scrollTop;
                    if (offset != 0) return offset;
                }
            }
            catch { }

            return 0;
        }

        private void CollectElementIdentifiers(dynamic element, HashSet<string> collectedIds)
        {
            if (collectedIds == null) return;

            try
            {
                string id = element.id?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    collectedIds.Add(id.Trim());
                }
            }
            catch { }

            try
            {
                string name = element.name?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    collectedIds.Add(name.Trim());
                }
            }
            catch { }

            try
            {
                dynamic children = element.children;
                if (children != null)
                {
                    int length = 0;
                    try { length = (int)children.length; }
                    catch { }

                    for (int i = 0; i < length; i++)
                    {
                        dynamic child = children.item(i);
                        if (child == null) continue;
                        CollectElementIdentifiers(child, collectedIds);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Genel element kaydı oluşturur (tablo satırı değilse)
        /// </summary>
        private RecordedElement CreateGenericElement(AutomationElement element, Point clickPoint)
        {
            var recorded = new RecordedElement
            {
                ElementType = "Generic",
                Timestamp = DateTime.Now,
                ScreenPoint = clickPoint,
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ClassName = element.Current.ClassName,
                ControlType = element.Current.ControlType.ProgrammaticName,
                Description = $"Generic: {element.Current.Name ?? element.Current.ClassName}"
            };

            PopulateWindowInfo(recorded, element);
            return recorded;
        }

        private void PopulateWindowInfo(RecordedElement recordedElement, AutomationElement element)
        {
            if (recordedElement == null || element == null)
                return;

            try
            {
                var window = GetTopLevelWindow(element);
                if (window == null)
                    return;

                recordedElement.WindowTitle = window.Current.Name;
                recordedElement.WindowName = window.Current.Name;
                recordedElement.WindowClassName = window.Current.ClassName;
                recordedElement.WindowAutomationId = window.Current.AutomationId;
                recordedElement.WindowProcessId = window.Current.ProcessId;

                try
                {
                    var rect = window.Current.BoundingRectangle;
                    recordedElement.WindowRelativeX = recordedElement.ScreenPoint.X - (int)rect.Left;
                    recordedElement.WindowRelativeY = recordedElement.ScreenPoint.Y - (int)rect.Top;
                }
                catch
                {
                    recordedElement.WindowRelativeX = null;
                    recordedElement.WindowRelativeY = null;
                }

                try
                {
                    recordedElement.WindowHandle = window.Current.NativeWindowHandle;
                }
                catch
                {
                    recordedElement.WindowHandle = 0;
                }

                try
                {
                    var process = Process.GetProcessById(window.Current.ProcessId);
                    recordedElement.WindowProcessName = process.ProcessName;
                }
                catch
                {
                    recordedElement.WindowProcessName = null;
                }
            }
            catch (Exception ex)
            {
                LogWarning($"PopulateWindowInfo failed: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods - XPath and Selector Generation

        private string? GenerateXPath(HtmlInfo? htmlInfo)
        {
            if (htmlInfo == null) return null;

            // ID varsa direkt ID üzerinden XPath dön
            if (!string.IsNullOrEmpty(htmlInfo.Id))
            {
                return $"//*[@id='{EscapeXPath(htmlInfo.Id)}']";
            }

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(htmlInfo.TableId) && htmlInfo.TableRowIndex >= 0)
            {
                // Table id + row index kullan
                builder.Append($"//table[@id='{EscapeXPath(htmlInfo.TableId)}']//tr[position()={htmlInfo.TableRowIndex + 1}]");
                return builder.ToString();
            }

            // Genel fallback
            builder.Append("//");
            builder.Append(string.IsNullOrEmpty(htmlInfo.TagName) ? "tr" : htmlInfo.TagName);

            if (!string.IsNullOrEmpty(htmlInfo.ClassName))
            {
                var firstClass = htmlInfo.ClassName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(firstClass))
                {
                    var escapedClass = EscapeXPath(firstClass);
                    builder.Append($"[contains(concat(' ', normalize-space(@class), ' '), ' {escapedClass} ')]");
                }
            }

            if (htmlInfo.TextContent?.Any() == true)
            {
                var firstText = htmlInfo.TextContent.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                if (!string.IsNullOrEmpty(firstText))
                {
                    builder.Append($"[contains(normalize-space(.), '{EscapeXPath(firstText.Trim())}')]");
                }
            }

            return builder.ToString();
        }

        private string? GenerateCssSelector(HtmlInfo? htmlInfo)
        {
            if (htmlInfo == null) return null;

            if (!string.IsNullOrEmpty(htmlInfo.Id))
            {
                return $"#{htmlInfo.Id}";
            }

            var selector = new StringBuilder();
            selector.Append(string.IsNullOrEmpty(htmlInfo.TagName) ? "tr" : htmlInfo.TagName);

            if (!string.IsNullOrEmpty(htmlInfo.ClassName))
            {
                var classes = htmlInfo.ClassName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cls in classes)
                {
                    selector.Append($".{cls}");
                }
            }

            if (!string.IsNullOrEmpty(htmlInfo.TableId))
            {
                selector.Insert(0, $"table#{htmlInfo.TableId} ");
            }

            return selector.ToString();
        }

        private string EscapeXPath(string text)
        {
            return text.Replace("'", "\\'").Replace("\"", "\\\"");
        }

        #endregion

        #region Private Methods - Playback

        private bool TryClickByXPath(RecordedElement element, string? overrideXPath = null)
        {
            var xpathToUse = overrideXPath ?? element.XPath;
            if (string.IsNullOrEmpty(xpathToUse)) return false;

            try
            {
                LogInfo($"Trying XPath: {xpathToUse}");

                // MSHTML document'i bul
                var doc = GetDocumentForRecordedElement(element);
                if (doc == null) return false;

                dynamic? target = null;

                // Önce ID ile dene
                var htmlInfo = element.HtmlInfo;
                if (!string.IsNullOrEmpty(htmlInfo?.Id) && overrideXPath == null)
                {
                    target = GetElementById(doc, htmlInfo.Id);
                }

                // XPath string'ini çözümle
                if (target == null)
                {
                    target = FindElementBySimpleXPath(doc, xpathToUse);
                }

                // Hücre ID'lerinden TR'ye ulaş
                if (target == null)
                {
                    foreach (var cellId in htmlInfo?.CellIds ?? Enumerable.Empty<string>())
                    {
                        var cellElement = GetElementById(doc, cellId);
                        if (cellElement == null) continue;
                        var parentRow = FindParentTr(cellElement);
                        if (parentRow != null)
                        {
                            target = parentRow;
                            break;
                        }
                    }
                }

                if (target == null && htmlInfo != null)
                {
                    target = FindRowByHtmlInfo(doc, htmlInfo);
                }

                if (target == null)
                {
                    LogWarning("XPath lookup failed");
                    return false;
                }

                ClickHtmlElement(target);
                LogSuccess("✅ Clicked via XPath/HTML lookup");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryExecutePlaywrightSelector(RecordedElement recordedElement, ElementLocatorStrategy strategy)
        {
            try
            {
                if (!strategy.Properties.TryGetValue("SelectorKind", out var kindRaw))
                    return false;

                var kind = kindRaw?.Trim().ToLowerInvariant() ?? string.Empty;

                if (strategy.Properties.TryGetValue("BrowserWindowHandle", out var handleStr) &&
                    int.TryParse(handleStr, out var handle) && handle > 0)
                {
                    recordedElement.HtmlInfo ??= new HtmlInfo();
                    recordedElement.HtmlInfo.BrowserWindowHandle = handle;
                }

                if (strategy.Properties.TryGetValue("BrowserProcessId", out var pidStr) &&
                    int.TryParse(pidStr, out var pid) && pid > 0)
                {
                    recordedElement.HtmlInfo ??= new HtmlInfo();
                    recordedElement.HtmlInfo.BrowserProcessId = pid;
                }

                switch (kind)
                {
                    case "table-row":
                    case "row-index":
                        return TryClickByTableRowIndex(recordedElement) || TryClickHtmlRowByIndex(recordedElement);

                    case "css":
                        if (strategy.Properties.TryGetValue("Selector", out var cssSelector))
                        {
                            if (TryClickByCssSelector(recordedElement, cssSelector))
                                return true;
                        }
                        return TryClickHtmlRowByIndex(recordedElement);

                    case "xpath":
                        if (strategy.Properties.TryGetValue("Selector", out var xpathSelector))
                        {
                            if (TryClickByXPath(recordedElement, xpathSelector))
                                return true;
                        }
                        return TryClickHtmlRowByIndex(recordedElement);

                    case "text":
                        return TryClickByTextContent(recordedElement);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Playwright selector execution failed: {ex.Message}");
                return false;
            }
        }

        private bool TryClickByTableRowIndex(RecordedElement element)
        {
            bool attemptedUia = false;

            if (element.TableInfo != null &&
                !string.IsNullOrEmpty(element.TableInfo.TableId) &&
                element.TableInfo.RowIndex >= 0)
            {
                try
                {
                    attemptedUia = true;
                    LogInfo($"Trying UIA Table Row Index: {element.TableInfo.RowIndex}");

                    // UI Automation ile tabloyu bul
                    var root = AutomationElement.RootElement;
                    var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, element.TableInfo.TableId);
                    var table = root.FindFirst(TreeScope.Descendants, condition);

                    if (table != null)
                    {
                        // Satırı index'e göre bul
                        var walker = TreeWalker.RawViewWalker;
                        var row = walker.GetFirstChild(table);
                        int index = 0;

                        while (row != null)
                        {
                            if (index == element.TableInfo.RowIndex)
                            {
                                // Click pattern ile tıkla
                                if (row.TryGetCurrentPattern(InvokePattern.Pattern, out object pattern))
                                {
                                    ((InvokePattern)pattern).Invoke();
                                    LogSuccess("✅ Clicked via Row Index (UIA)");
                                    return true;
                                }

                                // Alternatif: Mouse event simülasyonu
                                var clickPoint = row.GetClickablePoint();
                                SimulateClick(clickPoint);
                                LogSuccess("✅ Clicked via simulated mouse (UIA)");
                                return true;
                            }

                            index++;
                            row = walker.GetNextSibling(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"TableRowIndex UIA method failed: {ex.Message}");
                }
            }

            // HTML fallback
            if (TryClickHtmlRowByIndex(element))
            {
                return true;
            }

            if (!attemptedUia)
            {
                LogInfo("UIA row index not available, HTML fallback failed");
            }

            return false;
        }

        private bool TryClickByTextContent(RecordedElement element)
        {
            var referenceTexts = element.HtmlInfo?.TextContent?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
            if (referenceTexts == null || referenceTexts.Count == 0)
            {
                referenceTexts = element.TableInfo?.CellTexts?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
            }

            if (referenceTexts == null || referenceTexts.Count == 0)
                return false;

            try
            {
                LogInfo($"Trying Text Content (MSHTML): {string.Join(", ", referenceTexts.Take(3))}");

                // MSHTML document'i bul
                var doc = GetDocumentForRecordedElement(element);
                if (doc == null)
                {
                    LogWarning("HTML Document not found");
                    return false;
                }

                // Tüm TR elementlerini bul
                var allElements = doc.all;
                if (allElements == null) return false;

                int elementCount;
                try { elementCount = (int)allElements.length; }
                catch { return false; }

                for (int i = 0; i < elementCount; i++)
                {
                    try
                    {
                        dynamic htmlElement = allElements.item(i);
                        if (htmlElement == null) continue;

                        string tagName = htmlElement.tagName?.ToString() ?? string.Empty;

                        // TR elementi mi?
                        if (!string.Equals(tagName, "TR", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var trElement = htmlElement;
                        object? cellsObj = trElement == null ? null : trElement.cells;
                        if (cellsObj == null) continue;
                        dynamic cells = cellsObj;

                        // Hücre metinlerini al
                        var cellTexts = new List<string>();
                        int length = 0;
                        try { length = (int)cells.length; }
                        catch { continue; }

                        for (int j = 0; j < length; j++)
                        {
                            dynamic cell = cells.item(j);
                            if (cell != null)
                            {
                                string text = cell.innerText?.ToString()?.Trim() ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    cellTexts.Add(text);
                                }
                            }
                        }

                        // Text'leri karşılaştır
                        if (CellTextsMatch(cellTexts, referenceTexts))
                        {
                            // TR'yi tıkla
                            ClickHtmlElement(htmlElement);
                            LogSuccess("✅ Clicked via Text Content (MSHTML)");
                            return true;
                        }
                    }
                    catch { }
                }

                LogWarning("No matching TR found by text content");
            }
            catch (Exception ex)
            {
                LogWarning($"TextContent method failed: {ex.Message}");
            }

            return false;
        }

        private bool TryClickByCoordinates(RecordedElement element)
        {
            try
            {
                int targetX = element.ScreenPoint.X;
                int targetY = element.ScreenPoint.Y;
                var hwnd = element.WindowHandle != 0 ? new IntPtr(element.WindowHandle) : IntPtr.Zero;

                if (hwnd != IntPtr.Zero)
                {
                    try
                    {
                        if (IsIconic(hwnd))
                        {
                            ShowWindow(hwnd, SW_RESTORE);
                        }

                        SetForegroundWindow(hwnd);
                        Thread.Sleep(150);

                        if (GetWindowRect(hwnd, out var rect) &&
                            element.WindowRelativeX.HasValue &&
                            element.WindowRelativeY.HasValue)
                        {
                            targetX = rect.Left + element.WindowRelativeX.Value;
                            targetY = rect.Top + element.WindowRelativeY.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Coordinate window prep failed: {ex.Message}");
                    }
                }

                if (targetX <= 0 && targetY <= 0)
                {
                    LogWarning("Coordinate fallback aborted: invalid target point");
                    return false;
                }

                SimulateClick(new System.Windows.Point(targetX, targetY));
                LogSuccess("✅ Clicked via coordinate fallback");
                return true;
            }
            catch (Exception ex)
            {
                LogWarning($"Coordinate click failed: {ex.Message}");
                return false;
            }
        }

        private bool TryClickByCssSelector(RecordedElement element, string? cssSelector)
        {
            if (string.IsNullOrWhiteSpace(cssSelector))
                return false;

            try
            {
                var doc = GetDocumentForRecordedElement(element);
                if (doc == null)
                    return false;

                dynamic? match = null;
                try
                {
                    match = doc.querySelector(cssSelector);
                }
                catch (Exception ex)
                {
                    LogWarning($"querySelector failed: {ex.Message}");
                }

                if (match != null)
                {
                    try
                    {
                        var ownerDocumentObj = (object?)match.ownerDocument;
                        if (ownerDocumentObj != null && element.HtmlInfo != null)
                        {
                            dynamic ownerDocument = ownerDocumentObj;
                            dynamic? windowObj = null;
                            try { windowObj = ownerDocument.parentWindow; }
                            catch { windowObj = null; }

                            if (windowObj != null)
                            {
                                try
                                {
                                    int hwnd = 0;
                                    object? handleCandidate = null;

                                    try
                                    {
                                        var type = ((object)windowObj).GetType();
                                        handleCandidate = type.GetProperty("HWND")?.GetValue(windowObj);
                                    }
                                    catch
                                    {
                                        handleCandidate = null;
                                    }

                                    if (handleCandidate != null && int.TryParse(handleCandidate.ToString(), out hwnd) && hwnd != 0)
                                    {
                                        element.HtmlInfo ??= new HtmlInfo();
                                        element.HtmlInfo.BrowserWindowHandle = hwnd;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }

                    ClickHtmlElement(match);
                    LogSuccess("✅ Clicked via CSS selector");
                    return true;
                }

                var rowIndex = ExtractNthOfTypeIndex(cssSelector, "tr");
                if (rowIndex > 0)
                {
                    element.TableInfo ??= new TableInfo();
                    if (element.TableInfo.RowIndex < 0)
                        element.TableInfo.RowIndex = rowIndex - 1;
                    return TryClickHtmlRowByIndex(element);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"CSS selector execution failed: {ex.Message}");
            }

            return false;
        }

        private static int ExtractNthOfTypeIndex(string selector, string tagName)
        {
            if (string.IsNullOrWhiteSpace(selector) || string.IsNullOrWhiteSpace(tagName))
                return -1;

            try
            {
                var pattern = $"{tagName}\\s*:\\s*nth-of-type\\((\\d+)\\)";
                var match = Regex.Match(selector, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var index))
                {
                    return index;
                }
            }
            catch { }

            return -1;
        }

        private bool TryClickHtmlRowByIndex(RecordedElement element)
        {
            var htmlInfo = element.HtmlInfo;
            if (htmlInfo == null && element.TableInfo?.HtmlRowIndex < 0)
                return false;

            try
            {
                var doc = GetDocumentForRecordedElement(element);
                if (doc == null) return false;

                string? tableId = element.TableInfo?.HtmlTableId ?? htmlInfo?.TableId;
                int rowIndex = element.TableInfo?.HtmlRowIndex ?? htmlInfo?.TableRowIndex ?? -1;

                dynamic? row = null;
                if (!string.IsNullOrEmpty(tableId) && rowIndex >= 0)
                {
                    row = GetTableRowByIndex(doc, tableId, rowIndex);
                }

                if (row == null)
                {
                    foreach (var cellId in htmlInfo?.CellIds ?? Enumerable.Empty<string>())
                    {
                        var cell = GetElementById(doc, cellId);
                        if (cell == null) continue;

                        var parentRow = FindParentTr(cell);
                        if (parentRow != null)
                        {
                            row = parentRow;
                            break;
                        }
                    }
                }

                if (row == null && htmlInfo != null)
                {
                    row = FindRowByHtmlInfo(doc, htmlInfo);
                }

                if (row == null && rowIndex >= 0)
                {
                    try
                    {
                        dynamic tables = doc.getElementsByTagName("table");
                        if (tables != null)
                        {
                            int tableCount = 0;
                            try { tableCount = (int)tables.length; }
                            catch { tableCount = 0; }

                            for (int t = 0; t < tableCount && row == null; t++)
                            {
                                dynamic tableCandidate = tables.item(t);
                                if (tableCandidate == null) continue;

                                dynamic rows = tableCandidate.rows;
                                if (rows == null) continue;

                                int rowsCount = 0;
                                try { rowsCount = (int)rows.length; }
                                catch { rowsCount = 0; }

                                if (rowIndex >= 0 && rowIndex < rowsCount)
                                {
                                    row = rows.item(rowIndex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Fallback table scan failed: {ex.Message}");
                    }
                }

                if (row == null) return false;

                ClickHtmlElement(row);
                LogSuccess("✅ Clicked via HTML row lookup");
                return true;
            }
            catch (Exception ex)
            {
                LogWarning($"HTML row lookup failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// HTML elementine tıklama simüle eder (dynamic)
        /// </summary>
        private void ClickHtmlElement(dynamic element)
        {
            try
            {
                // Önce element'in onclick handler'ını tetikle
                element.click();
                LogInfo("Element.click() called");
            }
            catch (Exception ex)
            {
                LogError($"ClickHtmlElement error: {ex.Message}");

                // Alternatif: Focus + Mouse simülasyonu
                try
                {
                    element.focus();
                    Thread.Sleep(100);
                    // Tekrar dene
                    element.click();
                }
                catch { }
            }
        }

        private bool CellTextsMatch(List<string> actual, List<string> expected)
        {
            if (expected == null || expected.Count == 0) return false;

            var normalizedActual = actual
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => NormalizeText(t))
                .Where(t => t.Length > 0)
                .ToList();

            var normalizedExpected = expected
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => NormalizeText(t))
                .Where(t => t.Length > 0)
                .ToList();

            if (normalizedActual.Count == 0 || normalizedExpected.Count == 0) return false;

            foreach (var expectedText in normalizedExpected)
            {
                bool found = normalizedActual.Any(actualText =>
                    actualText.Contains(expectedText, StringComparison.OrdinalIgnoreCase));

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private string NormalizeText(string input)
        {
            return Regex.Replace(input ?? string.Empty, @"\s+", " ").Trim();
        }

        private static string? ExtractTableId(string? selector)
        {
            if (string.IsNullOrWhiteSpace(selector))
                return null;

            var match = Regex.Match(selector, @"table\s*#([A-Za-z0-9_\-:]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        private static string? ExtractTableClass(string? selector)
        {
            if (string.IsNullOrWhiteSpace(selector))
                return null;

            var match = Regex.Match(selector, @"table\.([A-Za-z0-9_\-\.]+)");
            if (match.Success)
            {
                return match.Groups[1].Value.Replace('.', ' ');
            }

            return null;
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private void SimulateClick(System.Windows.Point point)
        {
            var screenPoint = new Point((int)point.X, (int)point.Y);
            Cursor.Position = screenPoint;
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, screenPoint.X, screenPoint.Y, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, screenPoint.X, screenPoint.Y, 0, 0);
        }

        #endregion

        #region Logging

        private void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {message}");
            System.Diagnostics.Debug.WriteLine($"[SmartRecorder] {message}");
        }

        private void LogSuccess(string message)
        {
            Console.WriteLine($"[SUCCESS] {message}");
        }

        private void LogWarning(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
        }

        private void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        #endregion

        #region TaskChain Integration

        /// <summary>
        /// RecordedElement'i UIElementInfo'ya dönüştürür (TaskChain için)
        /// </summary>
        public static UIElementInfo ConvertToUIElementInfo(RecordedElement recordedElement)
        {
            if (recordedElement == null)
                throw new ArgumentNullException(nameof(recordedElement));

            var uiElement = new UIElementInfo
            {
                // UI Automation bilgileri
                AutomationId = recordedElement.AutomationId,
                Name = recordedElement.Name,
                ClassName = recordedElement.ClassName,
                ControlType = recordedElement.ControlType,
                CapturedAt = recordedElement.Timestamp,
                WindowProcessId = recordedElement.HtmlInfo?.BrowserProcessId,

                // HTML bilgileri (varsa)
                HtmlId = recordedElement.HtmlInfo?.Id,
                Tag = recordedElement.HtmlInfo?.TagName,
                TagName = recordedElement.HtmlInfo?.TagName,
                InnerText = recordedElement.HtmlInfo?.TextContent?.FirstOrDefault(),
                HtmlName = recordedElement.HtmlInfo?.Attributes?.GetValueOrDefault("name"),

                // XPath ve selectors
                XPath = recordedElement.XPath,
                CssSelector = recordedElement.CssSelector,

                // Konum
                X = recordedElement.ScreenPoint.X,
                Y = recordedElement.ScreenPoint.Y,

                // Tespit metodu
                DetectionMethod = recordedElement.HtmlInfo != null ? "MSHTML" : "UIAutomation"
            };

            if (recordedElement.WindowHandle > 0)
            {
                uiElement.WindowId = recordedElement.WindowHandle.ToString();
            }

            if (!string.IsNullOrEmpty(recordedElement.WindowTitle))
            {
                uiElement.WindowTitle = recordedElement.WindowTitle;
                uiElement.WindowName = recordedElement.WindowName ?? recordedElement.WindowTitle;
            }

            if (!string.IsNullOrEmpty(recordedElement.WindowClassName))
            {
                uiElement.WindowClassName = recordedElement.WindowClassName;
            }

            if (!string.IsNullOrEmpty(recordedElement.WindowProcessName))
            {
                uiElement.WindowProcessName = recordedElement.WindowProcessName;
            }

            if (recordedElement.WindowProcessId > 0)
            {
                uiElement.WindowProcessId = recordedElement.WindowProcessId;
            }
            else if (recordedElement.HtmlInfo?.BrowserProcessId > 0)
            {
                uiElement.WindowProcessId = recordedElement.HtmlInfo.BrowserProcessId;
            }

            // Tablo bilgilerini ekle (varsa)
            if (recordedElement.TableInfo != null)
            {
                // Tablo satırı için ek bilgiler
                uiElement.OtherAttributes = new Dictionary<string, string>
                {
                    ["TableId"] = recordedElement.TableInfo.TableId ?? "",
                    ["TableRowIndex"] = recordedElement.TableInfo.RowIndex.ToString(),
                    ["HtmlTableId"] = recordedElement.TableInfo.HtmlTableId ?? "",
                    ["HtmlRowIndex"] = recordedElement.TableInfo.HtmlRowIndex.ToString()
                };

                // Hücre metinlerini JSON olarak sakla
                if (recordedElement.TableInfo.CellTexts?.Any() == true)
                {
                    uiElement.TextContent = string.Join(" | ", recordedElement.TableInfo.CellTexts);
                }

                // Hücre ID'lerini sakla
                if (recordedElement.TableInfo.CellIds?.Any() == true)
                {
                    uiElement.DataAttributes = new Dictionary<string, string>
                    {
                        ["cell-ids"] = string.Join(",", recordedElement.TableInfo.CellIds)
                    };
                }
            }

            // HTML attributes
            if (recordedElement.HtmlInfo?.Attributes?.Any() == true)
            {
                uiElement.OtherAttributes ??= new Dictionary<string, string>();
                foreach (var attr in recordedElement.HtmlInfo.Attributes)
                {
                    uiElement.OtherAttributes[attr.Key] = attr.Value;
                }
            }

            if (recordedElement.HtmlInfo?.BrowserWindowHandle > 0)
            {
                uiElement.OtherAttributes ??= new Dictionary<string, string>();
                uiElement.OtherAttributes["BrowserWindowHandle"] = recordedElement.HtmlInfo.BrowserWindowHandle.ToString();
            }

            if (recordedElement.PlaywrightInfo != null)
            {
                uiElement.PlaywrightSelector =
                    recordedElement.PlaywrightInfo.Selectors.GetValueOrDefault("table-row") ??
                    recordedElement.PlaywrightInfo.Selectors.GetValueOrDefault("css") ??
                    recordedElement.PlaywrightInfo.Selectors.Values.FirstOrDefault();

                uiElement.OtherAttributes ??= new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(recordedElement.PlaywrightInfo.DomSelector))
                {
                    uiElement.OtherAttributes["PlaywrightDomSelector"] = recordedElement.PlaywrightInfo.DomSelector;
                }

                if (!string.IsNullOrEmpty(recordedElement.PlaywrightInfo.TableSelector))
                {
                    uiElement.OtherAttributes["PlaywrightTableSelector"] = recordedElement.PlaywrightInfo.TableSelector;
                }

                if (recordedElement.PlaywrightInfo.RowIndex >= 0)
                {
                    uiElement.OtherAttributes["PlaywrightRowIndex"] = recordedElement.PlaywrightInfo.RowIndex.ToString();
                }

                if (recordedElement.PlaywrightInfo.Selectors.Any())
                {
                    uiElement.OtherAttributes["PlaywrightSelectorsJson"] =
                        JsonSerializer.Serialize(recordedElement.PlaywrightInfo.Selectors);
                }

                if (!string.IsNullOrEmpty(recordedElement.PlaywrightInfo.ErrorMessage))
                {
                    uiElement.OtherAttributes["PlaywrightError"] = recordedElement.PlaywrightInfo.ErrorMessage;
                }
            }

            return uiElement;
        }

        public static void ApplyPlaywrightMetadata(RecordedElement recordedElement, PlaywrightSelectorInfo info)
        {
            if (recordedElement == null || info == null)
                return;

            recordedElement.PlaywrightInfo = info;

            if (info.RowIndex >= 0 || !string.IsNullOrEmpty(info.TableSelector))
            {
                recordedElement.ElementType = "TableRow";
                recordedElement.TableInfo ??= new TableInfo();

                if (info.RowIndex >= 0)
                {
                    recordedElement.TableInfo.RowIndex = info.RowIndex;
                    recordedElement.TableInfo.HtmlRowIndex = info.RowIndex;
                }

                var tableId = ExtractTableId(info.TableSelector);
                var tableClass = ExtractTableClass(info.TableSelector);

                if (!string.IsNullOrEmpty(tableId))
                {
                    recordedElement.TableInfo.TableId ??= tableId;
                    recordedElement.TableInfo.HtmlTableId ??= tableId;
                }

                if (!string.IsNullOrEmpty(tableClass))
                {
                    recordedElement.TableInfo.HtmlTableClass ??= tableClass;
                }

                if (info.CellTexts.Any())
                {
                    recordedElement.TableInfo.CellTexts = info.CellTexts.ToList();
                }
            }

            recordedElement.HtmlInfo ??= new HtmlInfo();
            var htmlInfo = recordedElement.HtmlInfo;
            htmlInfo.TagName ??= "tr";
            if (info.RowIndex >= 0)
            {
                htmlInfo.TableRowIndex = info.RowIndex;
            }

            var selectorId = ExtractTableId(info.TableSelector);
            if (!string.IsNullOrEmpty(selectorId))
            {
                htmlInfo.TableId ??= selectorId;
            }

            var selectorClass = ExtractTableClass(info.TableSelector);
            if (!string.IsNullOrEmpty(selectorClass))
            {
                htmlInfo.TableClassName ??= selectorClass;
            }

            if ((htmlInfo.TextContent == null || htmlInfo.TextContent.Count == 0) && info.CellTexts.Any())
            {
                htmlInfo.TextContent = info.CellTexts.ToList();
            }

            if (htmlInfo.Attributes == null)
            {
                htmlInfo.Attributes = new Dictionary<string, string>();
            }

                if (!string.IsNullOrEmpty(info.DomId))
                {
                    htmlInfo.Attributes["id"] = info.DomId;
                    htmlInfo.Id ??= info.DomId;
                }

                if (!string.IsNullOrEmpty(info.DomClass))
                {
                    htmlInfo.Attributes["class"] = info.DomClass;
                    htmlInfo.ClassName ??= info.DomClass;
                }

                if (info.Selectors.TryGetValue("table-row", out var tableSelector) && !string.IsNullOrEmpty(tableSelector))
                {
                    var rowIndex = ExtractNthOfTypeIndex(tableSelector, "tr");
                    if (rowIndex > 0)
                    {
                        htmlInfo.TableRowIndex = rowIndex - 1;
                        recordedElement.TableInfo ??= new TableInfo();
                        recordedElement.TableInfo.RowIndex = rowIndex - 1;
                        recordedElement.TableInfo.HtmlRowIndex = rowIndex - 1;
                    }
                }

                if (!string.IsNullOrEmpty(info.XPath))
                {
                    recordedElement.XPath = info.XPath;
                }

            if (!string.IsNullOrEmpty(info.CssPath))
            {
                recordedElement.CssSelector = info.CssPath;
            }
        }

        /// <summary>
        /// RecordedElement için optimal ElementLocatorStrategy listesi oluşturur
        /// </summary>
        public static List<ElementLocatorStrategy> GenerateLocatorStrategies(RecordedElement recordedElement)
        {
            if (recordedElement == null)
                throw new ArgumentNullException(nameof(recordedElement));

            var strategies = new List<ElementLocatorStrategy>();

            if (recordedElement.PlaywrightInfo?.Selectors?.Any() == true)
            {
                foreach (var kvp in recordedElement.PlaywrightInfo.Selectors)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Value))
                        continue;

                    var properties = new Dictionary<string, string>
                    {
                        ["SelectorKind"] = kvp.Key,
                        ["Selector"] = kvp.Value
                    };

                    if (recordedElement.PlaywrightInfo.RowIndex >= 0)
                    {
                        properties["RowIndex"] = recordedElement.PlaywrightInfo.RowIndex.ToString();
                    }

                    if (!string.IsNullOrEmpty(recordedElement.PlaywrightInfo.TableSelector))
                    {
                        properties["TableSelector"] = recordedElement.PlaywrightInfo.TableSelector;
                    }

                    if (recordedElement.HtmlInfo?.BrowserWindowHandle > 0)
                    {
                        properties["BrowserWindowHandle"] = recordedElement.HtmlInfo.BrowserWindowHandle.ToString();
                    }

                    if (recordedElement.HtmlInfo?.BrowserProcessId > 0)
                    {
                        properties["BrowserProcessId"] = recordedElement.HtmlInfo.BrowserProcessId.ToString();
                    }

                    strategies.Add(new ElementLocatorStrategy
                    {
                        Name = $"Playwright {kvp.Key}",
                        Description = $"Playwright selector ({kvp.Key}): {kvp.Value}",
                        Type = LocatorType.PlaywrightSelector,
                        Properties = properties,
                        RecordedElement = recordedElement
                    });
                }
            }

            // 1. HTML ID (en güvenilir - varsa)
            if (!string.IsNullOrEmpty(recordedElement.HtmlInfo?.Id))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "HTML ID",
                    Description = $"HTML element ID: {recordedElement.HtmlInfo.Id}",
                    Type = LocatorType.HtmlId,
                    Properties = new Dictionary<string, string>
                    {
                        ["HtmlId"] = recordedElement.HtmlInfo.Id
                    },
                    RecordedElement = recordedElement
                });
            }

            // 2. XPath (HTML için)
            if (!string.IsNullOrEmpty(recordedElement.XPath))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "XPath",
                    Description = $"XPath: {recordedElement.XPath}",
                    Type = LocatorType.XPath,
                    Properties = new Dictionary<string, string>
                    {
                        ["XPath"] = recordedElement.XPath
                    },
                    RecordedElement = recordedElement
                });
            }

            // 3. Table Row Index (tablo satırı için en iyi)
            if (recordedElement.TableInfo != null && recordedElement.TableInfo.RowIndex >= 0)
            {
                var tableId = recordedElement.TableInfo.HtmlTableId ?? recordedElement.TableInfo.TableId;
                if (!string.IsNullOrEmpty(tableId))
                {
                    strategies.Add(new ElementLocatorStrategy
                    {
                        Name = "Table Row Index",
                        Description = $"Table: {tableId}, Row: {recordedElement.TableInfo.RowIndex}",
                        Type = LocatorType.TableRowIndex,
                        Properties = new Dictionary<string, string>
                        {
                            ["TableId"] = tableId,
                            ["RowIndex"] = recordedElement.TableInfo.RowIndex.ToString()
                        },
                        RecordedElement = recordedElement
                    });
                }
            }

            // 4. Text Content (hücre metinleri ile)
            if (recordedElement.HtmlInfo?.TextContent?.Any() == true)
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "Text Content",
                    Description = $"Cell texts: {string.Join(", ", recordedElement.HtmlInfo.TextContent.Take(3))}",
                    Type = LocatorType.TextContent,
                    Properties = new Dictionary<string, string>
                    {
                        ["TextContent"] = string.Join("|", recordedElement.HtmlInfo.TextContent)
                    },
                    RecordedElement = recordedElement
                });
            }

            // 5. CSS Selector
            if (!string.IsNullOrEmpty(recordedElement.CssSelector))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "CSS Selector",
                    Description = $"CSS: {recordedElement.CssSelector}",
                    Type = LocatorType.CssSelector,
                    Properties = new Dictionary<string, string>
                    {
                        ["CssSelector"] = recordedElement.CssSelector
                    },
                    RecordedElement = recordedElement
                });
            }

            // 6. AutomationId (UI Automation için)
            if (!string.IsNullOrEmpty(recordedElement.AutomationId))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "AutomationId",
                    Description = $"AutomationId: {recordedElement.AutomationId}",
                    Type = LocatorType.AutomationId,
                    Properties = new Dictionary<string, string>
                    {
                        ["AutomationId"] = recordedElement.AutomationId
                    },
                    RecordedElement = recordedElement
                });
            }

            // 7. Name + ControlType
            if (!string.IsNullOrEmpty(recordedElement.Name) && !string.IsNullOrEmpty(recordedElement.ControlType))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "Name + ControlType",
                    Description = $"Name: {recordedElement.Name}, Type: {recordedElement.ControlType}",
                    Type = LocatorType.NameAndControlType,
                    Properties = new Dictionary<string, string>
                    {
                        ["Name"] = recordedElement.Name,
                        ["ControlType"] = recordedElement.ControlType
                    },
                    RecordedElement = recordedElement
                });
            }

            // 8. Coordinates (fallback - en son çare)
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "Coordinates",
                Description = $"Screen position: ({recordedElement.ScreenPoint.X}, {recordedElement.ScreenPoint.Y})",
                Type = LocatorType.Coordinates,
                Properties = new Dictionary<string, string>
                {
                    ["X"] = recordedElement.ScreenPoint.X.ToString(),
                    ["Y"] = recordedElement.ScreenPoint.Y.ToString()
                },
                RecordedElement = recordedElement
            });

            return strategies;
        }

        /// <summary>
        /// ElementLocatorStrategy kullanarak element bulur ve tıklar
        /// </summary>
        public bool ExecuteLocatorStrategy(ElementLocatorStrategy strategy)
        {
            if (strategy?.RecordedElement == null)
                return false;

            try
            {
                LogInfo($"🎯 Executing strategy: {strategy.Name}");

                if (strategy.Type == LocatorType.PlaywrightSelector)
                {
                    if (TryExecutePlaywrightSelector(strategy.RecordedElement, strategy))
                        return true;
                }

                // RecordedElement üzerindeki playback metotlarını kullan
                return PlaybackElement(strategy.RecordedElement);
            }
            catch (Exception ex)
            {
                LogError($"Strategy execution failed: {ex.Message}");
                return false;
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Kaydedilmiş element bilgileri
    /// </summary>
    public class RecordedElement
    {
        public string ElementType { get; set; } = ""; // "TableRow", "Button", "Generic"
        public DateTime Timestamp { get; set; }
        public Point ScreenPoint { get; set; }
        public string? WindowTitle { get; set; }
        public string? WindowName { get; set; }
        public string? WindowClassName { get; set; }
        public string? WindowAutomationId { get; set; }
        public string? WindowProcessName { get; set; }
        public int WindowProcessId { get; set; } = -1;
        public int WindowHandle { get; set; }
        public int? WindowRelativeX { get; set; }
        public int? WindowRelativeY { get; set; }

        // UI Automation Properties
        public string? AutomationId { get; set; }
        public string? Name { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }

        // Tablo bilgileri
        public TableInfo? TableInfo { get; set; }

        // HTML bilgileri
        public HtmlInfo? HtmlInfo { get; set; }

        // Selector'lar
        public string? XPath { get; set; }
        public string? CssSelector { get; set; }
        public PlaywrightSelectorInfo? PlaywrightInfo { get; set; }

        // Açıklama
        public string Description { get; set; } = "";

        public override string ToString()
        {
            return $"[{ElementType}] {Description}";
        }
    }

    public class TableInfo
    {
        public string? TableId { get; set; }
        public string? TableName { get; set; }
        public int RowIndex { get; set; }
        public List<string> CellTexts { get; set; } = new List<string>();
        public string? HtmlTableId { get; set; }
        public string? HtmlTableClass { get; set; }
        public int HtmlRowIndex { get; set; } = -1;
        public List<string> CellIds { get; set; } = new List<string>();
    }

    public class HtmlInfo
    {
        public string? TagName { get; set; }
        public string? Id { get; set; }
        public string? ClassName { get; set; }
        public List<string> TextContent { get; set; } = new List<string>();
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public string? TableId { get; set; }
        public string? TableClassName { get; set; }
        public int TableRowIndex { get; set; } = -1;
        public List<string> CellIds { get; set; } = new List<string>();
        public int BrowserProcessId { get; set; } = -1;
        public int BrowserWindowHandle { get; set; } = 0;
    }

    public class ElementRecordedEventArgs : EventArgs
    {
        public RecordedElement Element { get; }

        public ElementRecordedEventArgs(RecordedElement element)
        {
            ArgumentNullException.ThrowIfNull(element);
            Element = element;
        }
    }

    #endregion
}
