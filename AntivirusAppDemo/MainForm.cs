using System.Diagnostics;
using AntivirusAppDemo.Engine;
using AntivirusAppDemo.Models;
using AntivirusAppDemo.Scanners;
using AntivirusAppDemo.Services;

namespace AntivirusAppDemo;

public partial class MainForm : Form
{
    private readonly Queue<string> _scanQueue = new();
    private readonly AntivirusEngine _engine = new();
    private readonly DatabaseService _database;
    private readonly SettingsService _settings;
    private readonly QuarantineService _quarantine;
    private readonly RealTimeProtectionService _realTimeProtection;
    private VirusTotalScanner? _virusTotalScanner;
    
    private int _totalFiles;
    private int _scannedFiles;
    private bool _isScanning;
    private bool _isPaused;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainForm()
    {
        InitializeComponent();
        
        _database = new DatabaseService();
        _settings = new SettingsService();
        _quarantine = new QuarantineService();
        _realTimeProtection = new RealTimeProtectionService();
        
        InitializeEngine();
        SetupRealTimeProtection();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        LoadStatistics();
        LoadSettings();
        ShowDashboard();
    }

    private void InitializeEngine()
    {
        _virusTotalScanner = new VirusTotalScanner(_settings.Settings.VirusTotalApiKey);
        _engine.AddScanner(_virusTotalScanner);
    }

    private void SetupRealTimeProtection()
    {
        _realTimeProtection.FileDetected += (s, e) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => AddToScanQueue(e.FilePath));
            }
            else
            {
                AddToScanQueue(e.FilePath);
            }
        };
    }

    private void LoadStatistics()
    {
        var stats = _database.GetStatistics();
        UpdateStatsDisplay(stats);
    }

    private void UpdateStatsDisplay(ScanStatistics stats)
    {
        lblTotalScans.Text = $"📊 Toplam Tarama: {stats.TotalScans}";
        lblThreatsFound.Text = $"⚠️ Tehdit: {stats.ThreatsFound}";
        lblQuarantined.Text = $"🔒 Karantina: {stats.FilesQuarantined}";
    }

    private void LoadSettings()
    {
        chkRealTimeProtection.Checked = _settings.Settings.RealTimeProtectionEnabled;
        tmrScanner.Interval = _settings.Settings.ScanIntervalMs;
    }

    // ========== SIDEBAR NAVIGATION ==========
    private void btnDashboard_Click(object sender, EventArgs e) => ShowDashboard();
    private void btnScan_Click(object sender, EventArgs e) => ShowScanView();
    private void btnQuarantine_Click(object sender, EventArgs e) => ShowQuarantineView();
    private void btnHistory_Click(object sender, EventArgs e) => ShowHistoryView();
    private void btnSettings_Click(object sender, EventArgs e) => ShowSettingsDialog();

    private void HighlightSidebarButton(Button activeBtn)
    {
        var buttons = new[] { btnDashboard, btnScan, btnQuarantine, btnHistory, btnSettings };
        foreach (var btn in buttons)
        {
            btn.BackColor = btn == activeBtn 
                ? Color.FromArgb(88, 101, 242) 
                : Color.FromArgb(30, 30, 46);
        }
    }

    private void ShowDashboard()
    {
        HighlightSidebarButton(btnDashboard);
        dgvResults.Rows.Clear();
        lblStatus.Text = "Dashboard - Son tarama sonuçları";
        
        var history = _database.GetScanHistory(50);
        foreach (var entry in history)
        {
            AddResultToGrid(new ScanResult
            {
                FilePath = entry.FilePath,
                IsThreat = entry.IsThreat,
                ScannerName = entry.ScannerName,
                Message = entry.Message ?? "",
                ThreatScore = entry.ThreatScore
            });
        }
    }

    private void ShowScanView()
    {
        HighlightSidebarButton(btnScan);
        dgvResults.Rows.Clear();
        lblStatus.Text = "Tarama için dosya veya klasör seçin";
    }

    private async void ShowQuarantineView()
    {
        HighlightSidebarButton(btnQuarantine);
        dgvResults.Rows.Clear();
        lblStatus.Text = "Karantina - İzole edilmiş dosyalar";

        var quarantined = await _quarantine.GetQuarantinedFilesAsync();
        foreach (var entry in quarantined)
        {
            var rowIndex = dgvResults.Rows.Add();
            var row = dgvResults.Rows[rowIndex];
            row.Cells["colFileName"].Value = entry.FileName;
            row.Cells["colFilePath"].Value = entry.OriginalPath;
            row.Cells["colIsThreat"].Value = "🔒 Karantina";
            row.Cells["colScannerName"].Value = "-";
            row.Cells["colMessage"].Value = entry.Reason;
            row.Cells["colThreatScore"].Value = "-";
            row.Tag = entry.Id;
        }
    }

    private void ShowHistoryView()
    {
        HighlightSidebarButton(btnHistory);
        dgvResults.Rows.Clear();
        lblStatus.Text = "Tarama Geçmişi";

        var history = _database.GetScanHistory(200);
        foreach (var entry in history)
        {
            AddResultToGrid(new ScanResult
            {
                FilePath = entry.FilePath,
                IsThreat = entry.IsThreat,
                ScannerName = entry.ScannerName,
                Message = entry.Message ?? "",
                ThreatScore = entry.ThreatScore,
                ScanDate = entry.ScanDate
            });
        }
    }

    private void ShowSettingsDialog()
    {
        using var settingsForm = new SettingsForm(_settings);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            _virusTotalScanner?.UpdateApiKey(_settings.Settings.VirusTotalApiKey);
            tmrScanner.Interval = _settings.Settings.ScanIntervalMs;
        }
    }

    // ========== SCAN CONTROLS ==========
    private void btnSelectFile_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Taranacak dosyaları seçin",
            Filter = "Tüm Dosyalar (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            foreach (var file in dialog.FileNames)
                _scanQueue.Enqueue(file);
            
            StartScanning();
        }
    }

    private void btnSelectFolder_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Taranacak klasörü seçin",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var files = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
                _scanQueue.Enqueue(file);
            
            StartScanning();
        }
    }

    private void btnQuickScan_Click(object sender, EventArgs e)
    {
        var quickScanPaths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        };

        foreach (var path in quickScanPaths)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                    _scanQueue.Enqueue(file);
            }
        }

        if (_scanQueue.Count > 0)
            StartScanning();
        else
            MessageBox.Show("Taranacak dosya bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnPauseScan_Click(object sender, EventArgs e)
    {
        _isPaused = !_isPaused;
        btnPauseScan.Text = _isPaused ? "▶️ Devam" : "⏸️ Duraklat";
        btnPauseScan.BackColor = _isPaused ? Color.FromArgb(87, 242, 135) : Color.FromArgb(250, 166, 26);
        lblStatus.Text = _isPaused ? "Tarama duraklatıldı" : "Tarama devam ediyor...";
    }

    private void btnStopScan_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _scanQueue.Clear();
        tmrScanner.Enabled = false;
        _isScanning = false;
        _isPaused = false;
        
        btnPauseScan.Enabled = false;
        btnStopScan.Enabled = false;
        btnPauseScan.Text = "⏸️ Duraklat";
        
        lblStatus.Text = "Tarama durduruldu";
        progressBar.Value = 0;
        lblProgress.Text = "0%";
        
        LoadStatistics();
    }

    private void AddToScanQueue(string filePath)
    {
        _scanQueue.Enqueue(filePath);
        if (!tmrScanner.Enabled)
            StartScanning();
    }

    private void StartScanning()
    {
        if (_scanQueue.Count == 0) return;

        _totalFiles = _scanQueue.Count;
        _scannedFiles = 0;
        _isPaused = false;
        _cancellationTokenSource = new CancellationTokenSource();
        
        btnPauseScan.Enabled = true;
        btnStopScan.Enabled = true;
        
        UpdateProgress();

        if (!tmrScanner.Enabled)
        {
            tmrScanner.Enabled = true;
            _ = ProcessNextFile();
        }
    }

    private async void tmrScanner_Tick(object sender, EventArgs e)
    {
        if (_isScanning || _isPaused) return;
        await ProcessNextFile();
    }

    private async Task ProcessNextFile()
    {
        if (_scanQueue.Count == 0)
        {
            CompleteScan();
            return;
        }

        if (_cancellationTokenSource?.Token.IsCancellationRequested == true)
            return;

        _isScanning = true;
        var filePath = _scanQueue.Dequeue();
        _scannedFiles++;
        
        lblStatus.Text = $"Taranıyor: {Path.GetFileName(filePath)}";
        UpdateProgress();

        try
        {
            var results = await _engine.ScanFileAsync(filePath);
            foreach (var result in results)
            {
                AddResultToGrid(result);
                _database.SaveScanResult(result, result.FileHash);
                
                // Tehdit bulunursa bildirim göster
                if (result.IsThreat)
                {
                    ShowThreatNotification(result);
                }
            }
        }
        catch (Exception ex)
        {
            AddResultToGrid(new ScanResult
            {
                FilePath = filePath,
                Message = $"Hata: {ex.Message}",
                ScannerName = "System"
            });
        }

        _isScanning = false;
        LoadStatistics();
    }

    private void CompleteScan()
    {
        tmrScanner.Enabled = false;
        _isScanning = false;
        
        btnPauseScan.Enabled = false;
        btnStopScan.Enabled = false;
        
        lblStatus.Text = "✅ Tarama tamamlandı!";
        progressBar.Value = 100;
        lblProgress.Text = "100%";
        
        LoadStatistics();
        
        notifyIcon.ShowBalloonTip(3000, "Tarama Tamamlandı", 
            $"{_scannedFiles} dosya tarandı.", ToolTipIcon.Info);
    }

    private void UpdateProgress()
    {
        var progress = _totalFiles > 0 ? (_scannedFiles * 100) / _totalFiles : 0;
        progressBar.Value = Math.Min(progress, 100);
        lblProgress.Text = $"{progress}%";
    }

    private void AddResultToGrid(ScanResult result)
    {
        if (dgvResults.IsDisposed || !dgvResults.IsHandleCreated) return;
        
        try
        {
            var rowIndex = dgvResults.Rows.Add();
            var row = dgvResults.Rows[rowIndex];

            row.Cells["colFileName"].Value = result.FileName;
            row.Cells["colFilePath"].Value = result.FilePath;
            row.Cells["colIsThreat"].Value = result.IsThreat ? "⚠️ Tehdit" : "✅ Temiz";
            row.Cells["colScannerName"].Value = result.ScannerName;
            row.Cells["colMessage"].Value = result.Message;
            row.Cells["colThreatScore"].Value = result.ThreatScore;
            row.Tag = result;

            if (result.IsThreat)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(60, 30, 30);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 150, 150);
            }

            if (dgvResults.Rows.Count > 0)
                dgvResults.FirstDisplayedScrollingRowIndex = dgvResults.Rows.Count - 1;
        }
        catch (InvalidOperationException)
        {
            // Grid henüz hazır değil, atla
        }
    }

    private void ShowThreatNotification(ScanResult result)
    {
        notifyIcon.ShowBalloonTip(5000, "⚠️ Tehdit Tespit Edildi!", 
            $"{result.FileName}\n{result.Message}", ToolTipIcon.Warning);
    }

    // ========== REAL-TIME PROTECTION ==========
    private void chkRealTimeProtection_CheckedChanged(object sender, EventArgs e)
    {
        if (chkRealTimeProtection.Checked)
        {
            _realTimeProtection.Enable(_settings.Settings.MonitoredPaths);
            lblProtectionStatus.Text = "🟢 Koruma Aktif";
            lblProtectionStatus.ForeColor = Color.FromArgb(87, 242, 135);
            chkRealTimeProtection.Text = "🟢 Gerçek Zamanlı Koruma";
        }
        else
        {
            _realTimeProtection.Disable();
            lblProtectionStatus.Text = "🔴 Koruma Pasif";
            lblProtectionStatus.ForeColor = Color.FromArgb(237, 66, 69);
            chkRealTimeProtection.Text = "🔴 Gerçek Zamanlı Koruma";
        }
        
        _settings.UpdateRealTimeProtection(chkRealTimeProtection.Checked);
    }

    // ========== CONTEXT MENU ==========
    private async void menuQuarantine_Click(object sender, EventArgs e)
    {
        if (dgvResults.SelectedRows.Count == 0) return;
        
        var row = dgvResults.SelectedRows[0];
        var filePath = row.Cells["colFilePath"].Value?.ToString();
        
        if (string.IsNullOrEmpty(filePath)) return;

        var result = await _quarantine.QuarantineFileAsync(filePath, "Manuel karantina");
        if (result)
        {
            _database.IncrementQuarantineCount();
            dgvResults.Rows.Remove(row);
            LoadStatistics();
            MessageBox.Show("Dosya karantinaya alındı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Dosya karantinaya alınamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void menuOpenLocation_Click(object sender, EventArgs e)
    {
        if (dgvResults.SelectedRows.Count == 0) return;
        
        var filePath = dgvResults.SelectedRows[0].Cells["colFilePath"].Value?.ToString();
        if (string.IsNullOrEmpty(filePath)) return;

        var directory = Path.GetDirectoryName(filePath);
        if (Directory.Exists(directory))
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
    }

    private void menuRescan_Click(object sender, EventArgs e)
    {
        if (dgvResults.SelectedRows.Count == 0) return;
        
        var filePath = dgvResults.SelectedRows[0].Cells["colFilePath"].Value?.ToString();
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            _scanQueue.Enqueue(filePath);
            StartScanning();
        }
    }

    private void menuDelete_Click(object sender, EventArgs e)
    {
        if (dgvResults.SelectedRows.Count == 0) return;
        
        var filePath = dgvResults.SelectedRows[0].Cells["colFilePath"].Value?.ToString();
        if (string.IsNullOrEmpty(filePath)) return;

        var confirm = MessageBox.Show($"Bu dosyayı kalıcı olarak silmek istediğinize emin misiniz?\n\n{filePath}", 
            "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        
        if (confirm == DialogResult.Yes)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                
                dgvResults.Rows.Remove(dgvResults.SelectedRows[0]);
                MessageBox.Show("Dosya silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya silinemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void dgvResults_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != dgvResults.Columns["colActions"].Index) return;
        
        var filePath = dgvResults.Rows[e.RowIndex].Cells["colFilePath"].Value?.ToString();
        if (!string.IsNullOrEmpty(filePath))
        {
            menuQuarantine_Click(sender, e);
        }
    }

    // ========== FORM EVENTS ==========
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_settings.Settings.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            notifyIcon.ShowBalloonTip(2000, "Antivirus", "Uygulama arka planda çalışmaya devam ediyor.", ToolTipIcon.Info);
        }
        else
        {
            _realTimeProtection.Dispose();
            _database.Dispose();
            notifyIcon.Dispose();
        }
    }

    private void notifyIcon_DoubleClick(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }
}
