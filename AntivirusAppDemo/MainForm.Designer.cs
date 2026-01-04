namespace AntivirusAppDemo;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        
        // Main containers
        pnlSidebar = new Panel();
        pnlMain = new Panel();
        pnlStats = new Panel();
        pnlContent = new Panel();
        
        // Sidebar buttons
        btnDashboard = new Button();
        btnScan = new Button();
        btnQuarantine = new Button();
        btnHistory = new Button();
        btnSettings = new Button();
        lblAppTitle = new Label();
        
        // Stats panel controls
        lblTotalScans = new Label();
        lblThreatsFound = new Label();
        lblQuarantined = new Label();
        lblProtectionStatus = new Label();
        
        // Scan controls
        pnlScanControls = new Panel();
        btnSelectFile = new Button();
        btnSelectFolder = new Button();
        btnQuickScan = new Button();
        btnStopScan = new Button();
        btnPauseScan = new Button();
        chkRealTimeProtection = new CheckBox();
        
        // Progress
        progressBar = new ProgressBar();
        lblStatus = new Label();
        lblProgress = new Label();
        
        // Results grid
        dgvResults = new DataGridView();
        colFilePath = new DataGridViewTextBoxColumn();
        colFileName = new DataGridViewTextBoxColumn();
        colIsThreat = new DataGridViewTextBoxColumn();
        colScannerName = new DataGridViewTextBoxColumn();
        colMessage = new DataGridViewTextBoxColumn();
        colThreatScore = new DataGridViewTextBoxColumn();
        colActions = new DataGridViewButtonColumn();
        
        // Context menu
        contextMenuStrip = new ContextMenuStrip(components);
        menuQuarantine = new ToolStripMenuItem();
        menuOpenLocation = new ToolStripMenuItem();
        menuDelete = new ToolStripMenuItem();
        menuRescan = new ToolStripMenuItem();
        
        // Timer & Tray
        tmrScanner = new System.Windows.Forms.Timer(components);
        notifyIcon = new NotifyIcon(components);
        trayContextMenu = new ContextMenuStrip(components);
        
        // Initialize
        ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
        pnlSidebar.SuspendLayout();
        pnlMain.SuspendLayout();
        pnlStats.SuspendLayout();
        pnlContent.SuspendLayout();
        pnlScanControls.SuspendLayout();
        contextMenuStrip.SuspendLayout();
        SuspendLayout();

        // ========== SIDEBAR ==========
        pnlSidebar.BackColor = Color.FromArgb(30, 30, 46);
        pnlSidebar.Dock = DockStyle.Left;
        pnlSidebar.Width = 200;
        pnlSidebar.Padding = new Padding(10);
        pnlSidebar.Controls.Add(btnSettings);
        pnlSidebar.Controls.Add(btnHistory);
        pnlSidebar.Controls.Add(btnQuarantine);
        pnlSidebar.Controls.Add(btnScan);
        pnlSidebar.Controls.Add(btnDashboard);
        pnlSidebar.Controls.Add(lblAppTitle);

        // App Title
        lblAppTitle.Text = "🛡️ Antivirus";
        lblAppTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblAppTitle.ForeColor = Color.White;
        lblAppTitle.Dock = DockStyle.Top;
        lblAppTitle.Height = 60;
        lblAppTitle.TextAlign = ContentAlignment.MiddleCenter;

        // Sidebar Buttons Style
        var sidebarBtnStyle = new Action<Button, string, int>((btn, text, top) =>
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(30, 30, 46);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11F);
            btn.Size = new Size(180, 45);
            btn.Location = new Point(10, top);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
        });

        sidebarBtnStyle(btnDashboard, "📊  Dashboard", 70);
        sidebarBtnStyle(btnScan, "🔍  Tara", 120);
        sidebarBtnStyle(btnQuarantine, "🔒  Karantina", 170);
        sidebarBtnStyle(btnHistory, "📜  Geçmiş", 220);
        sidebarBtnStyle(btnSettings, "⚙️  Ayarlar", 270);

        btnDashboard.Click += btnDashboard_Click;
        btnScan.Click += btnScan_Click;
        btnQuarantine.Click += btnQuarantine_Click;
        btnHistory.Click += btnHistory_Click;
        btnSettings.Click += btnSettings_Click;

        // ========== MAIN PANEL ==========
        pnlMain.BackColor = Color.FromArgb(24, 24, 37);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Controls.Add(pnlContent);
        pnlMain.Controls.Add(pnlStats);

        // ========== STATS PANEL ==========
        pnlStats.BackColor = Color.FromArgb(24, 24, 37);
        pnlStats.Dock = DockStyle.Top;
        pnlStats.Height = 100;
        pnlStats.Padding = new Padding(20, 15, 20, 15);
        pnlStats.Controls.Add(lblProtectionStatus);
        pnlStats.Controls.Add(lblQuarantined);
        pnlStats.Controls.Add(lblThreatsFound);
        pnlStats.Controls.Add(lblTotalScans);

        var statLabelStyle = new Action<Label, string, int>((lbl, text, left) =>
        {
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 10F);
            lbl.ForeColor = Color.FromArgb(180, 180, 200);
            lbl.AutoSize = true;
            lbl.Location = new Point(left, 20);
        });

        statLabelStyle(lblTotalScans, "📊 Toplam Tarama: 0", 20);
        statLabelStyle(lblThreatsFound, "⚠️ Tehdit: 0", 200);
        statLabelStyle(lblQuarantined, "🔒 Karantina: 0", 350);
        statLabelStyle(lblProtectionStatus, "🟢 Koruma Aktif", 500);

        // ========== CONTENT PANEL ==========
        pnlContent.BackColor = Color.FromArgb(24, 24, 37);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Padding = new Padding(10);
        pnlContent.Controls.Add(dgvResults);
        pnlContent.Controls.Add(pnlScanControls);

        // ========== SCAN CONTROLS ==========
        pnlScanControls.BackColor = Color.FromArgb(30, 30, 46);
        pnlScanControls.Dock = DockStyle.Top;
        pnlScanControls.Height = 110;
        pnlScanControls.Padding = new Padding(10);
        pnlScanControls.Controls.Add(chkRealTimeProtection);
        pnlScanControls.Controls.Add(lblProgress);
        pnlScanControls.Controls.Add(progressBar);
        pnlScanControls.Controls.Add(lblStatus);
        pnlScanControls.Controls.Add(btnStopScan);
        pnlScanControls.Controls.Add(btnPauseScan);
        pnlScanControls.Controls.Add(btnQuickScan);
        pnlScanControls.Controls.Add(btnSelectFolder);
        pnlScanControls.Controls.Add(btnSelectFile);

        var actionBtnStyle = new Action<Button, string, Color, int>((btn, text, color, left) =>
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Size = new Size(110, 35);
            btn.Location = new Point(left, 15);
            btn.Cursor = Cursors.Hand;
        });

        actionBtnStyle(btnSelectFile, "📄 Dosya", Color.FromArgb(88, 101, 242), 15);
        actionBtnStyle(btnSelectFolder, "📁 Klasör", Color.FromArgb(88, 101, 242), 135);
        actionBtnStyle(btnQuickScan, "⚡ Hızlı Tara", Color.FromArgb(87, 242, 135), 255);
        actionBtnStyle(btnPauseScan, "⏸️ Duraklat", Color.FromArgb(250, 166, 26), 375);
        actionBtnStyle(btnStopScan, "⏹️ Durdur", Color.FromArgb(237, 66, 69), 495);

        btnSelectFile.Click += btnSelectFile_Click;
        btnSelectFolder.Click += btnSelectFolder_Click;
        btnQuickScan.Click += btnQuickScan_Click;
        btnPauseScan.Click += btnPauseScan_Click;
        btnStopScan.Click += btnStopScan_Click;

        btnPauseScan.Enabled = false;
        btnStopScan.Enabled = false;

        // Real-time protection checkbox
        chkRealTimeProtection.Text = "🔴 Gerçek Zamanlı Koruma";
        chkRealTimeProtection.ForeColor = Color.White;
        chkRealTimeProtection.Font = new Font("Segoe UI", 9F);
        chkRealTimeProtection.Location = new Point(620, 22);
        chkRealTimeProtection.AutoSize = true;
        chkRealTimeProtection.CheckedChanged += chkRealTimeProtection_CheckedChanged;

        // Status label
        lblStatus.Text = "Hazır";
        lblStatus.ForeColor = Color.FromArgb(180, 180, 200);
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.Location = new Point(15, 60);
        lblStatus.AutoSize = true;

        // Progress bar
        progressBar.Location = new Point(15, 85);
        progressBar.Size = new Size(600, 20);
        progressBar.Style = ProgressBarStyle.Continuous;

        // Progress label
        lblProgress.Text = "0%";
        lblProgress.ForeColor = Color.White;
        lblProgress.Font = new Font("Segoe UI", 9F);
        lblProgress.Location = new Point(625, 85);
        lblProgress.AutoSize = true;

        // ========== DATA GRID ==========
        dgvResults.BackgroundColor = Color.FromArgb(30, 30, 46);
        dgvResults.BorderStyle = BorderStyle.None;
        dgvResults.Dock = DockStyle.Fill;
        dgvResults.AllowUserToAddRows = false;
        dgvResults.AllowUserToDeleteRows = false;
        dgvResults.ReadOnly = true;
        dgvResults.RowHeadersVisible = false;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvResults.ColumnHeadersHeight = 35;
        dgvResults.RowTemplate.Height = 30;
        dgvResults.EnableHeadersVisualStyles = false;
        dgvResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 56);
        dgvResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvResults.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 46);
        dgvResults.DefaultCellStyle.ForeColor = Color.White;
        dgvResults.DefaultCellStyle.SelectionBackColor = Color.FromArgb(88, 101, 242);
        dgvResults.GridColor = Color.FromArgb(50, 50, 70);
        dgvResults.ContextMenuStrip = contextMenuStrip;
        dgvResults.CellClick += dgvResults_CellClick;
        dgvResults.ScrollBars = ScrollBars.Both;

        dgvResults.Columns.AddRange(new DataGridViewColumn[] { 
            colFileName, colFilePath, colIsThreat, colScannerName, colMessage, colThreatScore, colActions 
        });

        colFileName.HeaderText = "Dosya";
        colFileName.Name = "colFileName";
        colFileName.FillWeight = 15;

        colFilePath.HeaderText = "Yol";
        colFilePath.Name = "colFilePath";
        colFilePath.FillWeight = 25;

        colIsThreat.HeaderText = "Durum";
        colIsThreat.Name = "colIsThreat";
        colIsThreat.FillWeight = 10;

        colScannerName.HeaderText = "Tarayıcı";
        colScannerName.Name = "colScannerName";
        colScannerName.FillWeight = 10;

        colMessage.HeaderText = "Mesaj";
        colMessage.Name = "colMessage";
        colMessage.FillWeight = 20;

        colThreatScore.HeaderText = "Skor";
        colThreatScore.Name = "colThreatScore";
        colThreatScore.FillWeight = 8;

        colActions.HeaderText = "İşlem";
        colActions.Name = "colActions";
        colActions.Text = "Karantina";
        colActions.UseColumnTextForButtonValue = true;
        colActions.FillWeight = 12;

        // ========== CONTEXT MENU ==========
        contextMenuStrip.Items.AddRange(new ToolStripItem[] { menuQuarantine, menuOpenLocation, menuRescan, menuDelete });
        
        menuQuarantine.Text = "🔒 Karantinaya Al";
        menuQuarantine.Click += menuQuarantine_Click;
        
        menuOpenLocation.Text = "📂 Dosya Konumunu Aç";
        menuOpenLocation.Click += menuOpenLocation_Click;
        
        menuRescan.Text = "🔄 Tekrar Tara";
        menuRescan.Click += menuRescan_Click;
        
        menuDelete.Text = "🗑️ Kalıcı Sil";
        menuDelete.Click += menuDelete_Click;

        // ========== TIMER ==========
        tmrScanner.Interval = 15000;
        tmrScanner.Tick += tmrScanner_Tick;

        // ========== NOTIFY ICON ==========
        notifyIcon.Text = "Antivirus";
        notifyIcon.Visible = true;
        notifyIcon.DoubleClick += notifyIcon_DoubleClick;
        notifyIcon.ContextMenuStrip = trayContextMenu;

        var trayOpen = new ToolStripMenuItem("Aç", null, (s, e) => { Show(); WindowState = FormWindowState.Normal; });
        var trayExit = new ToolStripMenuItem("Çıkış", null, (s, e) => { Application.Exit(); });
        trayContextMenu.Items.AddRange(new ToolStripItem[] { trayOpen, trayExit });

        // ========== MAIN FORM ==========
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(24, 24, 37);
        ClientSize = new Size(1100, 650);
        Controls.Add(pnlMain);
        Controls.Add(pnlSidebar);
        MinimumSize = new Size(900, 500);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "🛡️ Modular Antivirus";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;

        ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
        pnlSidebar.ResumeLayout(false);
        pnlMain.ResumeLayout(false);
        pnlStats.ResumeLayout(false);
        pnlStats.PerformLayout();
        pnlContent.ResumeLayout(false);
        pnlScanControls.ResumeLayout(false);
        pnlScanControls.PerformLayout();
        contextMenuStrip.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    // Panels
    private Panel pnlSidebar;
    private Panel pnlMain;
    private Panel pnlStats;
    private Panel pnlContent;
    private Panel pnlScanControls;

    // Sidebar
    private Label lblAppTitle;
    private Button btnDashboard;
    private Button btnScan;
    private Button btnQuarantine;
    private Button btnHistory;
    private Button btnSettings;

    // Stats
    private Label lblTotalScans;
    private Label lblThreatsFound;
    private Label lblQuarantined;
    private Label lblProtectionStatus;

    // Scan controls
    private Button btnSelectFile;
    private Button btnSelectFolder;
    private Button btnQuickScan;
    private Button btnPauseScan;
    private Button btnStopScan;
    private CheckBox chkRealTimeProtection;
    private ProgressBar progressBar;
    private Label lblStatus;
    private Label lblProgress;

    // Grid
    private DataGridView dgvResults;
    private DataGridViewTextBoxColumn colFileName;
    private DataGridViewTextBoxColumn colFilePath;
    private DataGridViewTextBoxColumn colIsThreat;
    private DataGridViewTextBoxColumn colScannerName;
    private DataGridViewTextBoxColumn colMessage;
    private DataGridViewTextBoxColumn colThreatScore;
    private DataGridViewButtonColumn colActions;

    // Context menu
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem menuQuarantine;
    private ToolStripMenuItem menuOpenLocation;
    private ToolStripMenuItem menuDelete;
    private ToolStripMenuItem menuRescan;

    // Timer & Tray
    private System.Windows.Forms.Timer tmrScanner;
    private NotifyIcon notifyIcon;
    private ContextMenuStrip trayContextMenu;
}
