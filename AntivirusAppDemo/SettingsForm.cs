using AntivirusAppDemo.Services;

namespace AntivirusAppDemo;

public class SettingsForm : Form
{
    private readonly SettingsService _settings;
    
    private TextBox txtApiKey = null!;
    private NumericUpDown numInterval = null!;
    private CheckBox chkMinimizeToTray = null!;
    private CheckBox chkStartWithWindows = null!;
    private ListBox lstMonitoredPaths = null!;
    private Button btnAddPath = null!;
    private Button btnRemovePath = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    public SettingsForm(SettingsService settings)
    {
        _settings = settings;
        InitializeComponents();
        LoadCurrentSettings();
    }

    private void InitializeComponents()
    {
        Text = "⚙️ Ayarlar";
        Size = new Size(500, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(30, 30, 46);

        var lblApiKey = new Label
        {
            Text = "VirusTotal API Anahtarı:",
            ForeColor = Color.White,
            Location = new Point(20, 20),
            AutoSize = true
        };

        txtApiKey = new TextBox
        {
            Location = new Point(20, 45),
            Size = new Size(440, 25),
            BackColor = Color.FromArgb(40, 40, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblInterval = new Label
        {
            Text = "Tarama Aralığı (ms):",
            ForeColor = Color.White,
            Location = new Point(20, 85),
            AutoSize = true
        };

        numInterval = new NumericUpDown
        {
            Location = new Point(20, 110),
            Size = new Size(150, 25),
            Minimum = 5000,
            Maximum = 60000,
            Increment = 1000,
            BackColor = Color.FromArgb(40, 40, 56),
            ForeColor = Color.White
        };

        chkMinimizeToTray = new CheckBox
        {
            Text = "Kapatınca sistem tepsisine küçült",
            ForeColor = Color.White,
            Location = new Point(20, 150),
            AutoSize = true
        };

        chkStartWithWindows = new CheckBox
        {
            Text = "Windows ile başlat",
            ForeColor = Color.White,
            Location = new Point(20, 180),
            AutoSize = true
        };

        var lblMonitoredPaths = new Label
        {
            Text = "İzlenen Klasörler (Gerçek Zamanlı Koruma):",
            ForeColor = Color.White,
            Location = new Point(20, 220),
            AutoSize = true
        };

        lstMonitoredPaths = new ListBox
        {
            Location = new Point(20, 245),
            Size = new Size(350, 120),
            BackColor = Color.FromArgb(40, 40, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        btnAddPath = new Button
        {
            Text = "+",
            Location = new Point(380, 245),
            Size = new Size(80, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(87, 242, 135),
            ForeColor = Color.White
        };
        btnAddPath.Click += BtnAddPath_Click;

        btnRemovePath = new Button
        {
            Text = "-",
            Location = new Point(380, 285),
            Size = new Size(80, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(237, 66, 69),
            ForeColor = Color.White
        };
        btnRemovePath.Click += BtnRemovePath_Click;

        btnSave = new Button
        {
            Text = "💾 Kaydet",
            Location = new Point(260, 410),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(88, 101, 242),
            ForeColor = Color.White,
            DialogResult = DialogResult.OK
        };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "İptal",
            Location = new Point(370, 410),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 80),
            ForeColor = Color.White,
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[] {
            lblApiKey, txtApiKey,
            lblInterval, numInterval,
            chkMinimizeToTray, chkStartWithWindows,
            lblMonitoredPaths, lstMonitoredPaths,
            btnAddPath, btnRemovePath,
            btnSave, btnCancel
        });
    }

    private void LoadCurrentSettings()
    {
        txtApiKey.Text = _settings.Settings.VirusTotalApiKey;
        numInterval.Value = _settings.Settings.ScanIntervalMs;
        chkMinimizeToTray.Checked = _settings.Settings.MinimizeToTray;
        chkStartWithWindows.Checked = _settings.Settings.StartWithWindows;
        
        lstMonitoredPaths.Items.Clear();
        foreach (var path in _settings.Settings.MonitoredPaths)
        {
            lstMonitoredPaths.Items.Add(path);
        }
    }

    private void BtnAddPath_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "İzlenecek klasörü seçin"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (!lstMonitoredPaths.Items.Contains(dialog.SelectedPath))
            {
                lstMonitoredPaths.Items.Add(dialog.SelectedPath);
            }
        }
    }

    private void BtnRemovePath_Click(object? sender, EventArgs e)
    {
        if (lstMonitoredPaths.SelectedItem != null)
        {
            lstMonitoredPaths.Items.Remove(lstMonitoredPaths.SelectedItem);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        _settings.Settings.VirusTotalApiKey = txtApiKey.Text;
        _settings.Settings.ScanIntervalMs = (int)numInterval.Value;
        _settings.Settings.MinimizeToTray = chkMinimizeToTray.Checked;
        _settings.Settings.StartWithWindows = chkStartWithWindows.Checked;
        
        _settings.Settings.MonitoredPaths.Clear();
        foreach (var item in lstMonitoredPaths.Items)
        {
            _settings.Settings.MonitoredPaths.Add(item.ToString()!);
        }
        
        _settings.Save();
    }
}
