using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private const string TaskName = "Persistent SD";
    private readonly TextBox scriptPath = new TextBox();
    private readonly Label status = new Label();
    private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;

    public MainForm()
    {
        Text = "Persistent SD";
        Width = 660; Height = 370; MinimumSize = new Size(660, 370);
        StartPosition = FormStartPosition.CenterScreen;
        try { Icon = new Icon(Path.Combine(baseDir, "RazorBlade.ico")); } catch { }
        BackColor = Color.FromArgb(30, 32, 36); ForeColor = Color.White;

        var heading = new Label { Text = "PERSISTENT SD", Left = 24, Top = 18, Width = 580, Height = 36, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White };
        var sub = new Label { Text = "Keep the Realtek SD card mapped to R: at every boot and card insertion.", Left = 26, Top = 59, Width = 590, Height = 26, Font = new Font("Segoe UI", 9), ForeColor = Color.Gainsboro };
        var pathLabel = new Label { Text = "RDrivePersistentSD.ps1 location", Left = 26, Top = 105, Width = 300, Height = 22, ForeColor = Color.White };
        scriptPath.Left = 26; scriptPath.Top = 130; scriptPath.Width = 485; scriptPath.Height = 26;
        scriptPath.Text = Path.Combine(baseDir, "RDrivePersistentSD.ps1");
        var browse = MakeButton("Browse…", 520, 128, 100, 30); browse.Click += BrowseScript;
        var install = MakeButton("Install / Update Task", 26, 180, 190, 40); install.Click += InstallTask;
        var check = MakeButton("Check Status", 226, 180, 130, 40); check.Click += (s,e) => RefreshStatus();
        var remove = MakeButton("Remove Task", 366, 180, 120, 40); remove.Click += RemoveTask;
        var scheduler = MakeButton("Task Scheduler", 496, 180, 124, 40); scheduler.Click += (s,e) => Run("taskschd.msc", "");
        status.Left = 27; status.Top = 242; status.Width = 590; status.Height = 52; status.ForeColor = Color.LightGreen; status.Font = new Font("Segoe UI", 10);
        var note = new Label { Text = "Existing task detected? You will be asked before it is overwritten. Choosing No leaves it untouched.", Left = 26, Top = 300, Width = 600, Height = 32, ForeColor = Color.Silver, Font = new Font("Segoe UI", 8) };
        Controls.AddRange(new Control[] { heading, sub, pathLabel, scriptPath, browse, install, check, remove, scheduler, status, note });
        RefreshStatus();
    }

    private Button MakeButton(string text, int x, int y, int w, int h)
    {
        return new Button { Text = text, Left = x, Top = y, Width = w, Height = h, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 59, 66), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
    }

    private void BrowseScript(object sender, EventArgs e)
    {
        using (var d = new OpenFileDialog())
        {
            d.Title = "Select RDrivePersistentSD.ps1"; d.Filter = "PowerShell scripts (*.ps1)|*.ps1|All files (*.*)|*.*";
            d.FileName = Path.GetFileName(scriptPath.Text);
            if (d.ShowDialog(this) == DialogResult.OK) scriptPath.Text = d.FileName;
        }
    }

    private bool TaskExists()
    {
        var p = Run("schtasks.exe", "/Query /TN " + Quote("\\" + TaskName), false);
        return p.ExitCode == 0;
    }

    private void InstallTask(object sender, EventArgs e)
    {
        string path = scriptPath.Text.Trim().Trim('"');
        if (!File.Exists(path)) { MessageBox.Show(this, "The selected PowerShell script does not exist. Browse to RDrivePersistentSD.ps1 first.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        bool exists = TaskExists();
        if (exists)
        {
            var answer = MessageBox.Show(this, "The scheduled task 'Persistent SD' already exists.\n\nYes = overwrite it with this configuration\nNo = leave it exactly as it is\nCancel = do nothing", TaskName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) { status.Text = answer == DialogResult.No ? "Existing task left untouched." : "No changes made."; return; }
        }
        try
        {
            string xml = BuildTaskXml(path);
            string temp = Path.Combine(Path.GetTempPath(), "PersistentSD-" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(temp, xml, new UTF8Encoding(false));
            var p = Run("schtasks.exe", "/Create /TN " + Quote("\\" + TaskName) + " /XML " + Quote(temp) + (exists ? " /F" : ""), false);
            try { File.Delete(temp); } catch { }
            if (p.ExitCode == 0) { status.Text = "Installed: task 'Persistent SD' runs at system boot."; status.ForeColor = Color.LightGreen; }
            else MessageBox.Show(this, "Task creation failed.\n\n" + p.Output + "\n\nRun this utility as Administrator.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshStatus();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, TaskName, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private string BuildTaskXml(string path)
    {
        string p = SecurityElementEscape(path);
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
"<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">\r\n" +
" <RegistrationInfo><Author>Persistent SD</Author><URI>\\Persistent SD</URI></RegistrationInfo>\r\n" +
" <Triggers><BootTrigger><Enabled>true</Enabled></BootTrigger></Triggers>\r\n" +
" <Principals><Principal id=\"Author\"><UserId>S-1-5-18</UserId><RunLevel>HighestAvailable</RunLevel></Principal></Principals>\r\n" +
" <Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>true</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>true</StopIfGoingOnBatteries><AllowHardTerminate>true</AllowHardTerminate><StartWhenAvailable>false</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><IdleSettings><Duration>PT10M</Duration><WaitTimeout>PT1H</WaitTimeout><StopOnIdleEnd>true</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden><RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT72H</ExecutionTimeLimit><Priority>7</Priority></Settings>\r\n" +
" <Actions Context=\"Author\"><Exec><Command>PowerShell.exe</Command><Arguments>-NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File &quot;" + p + "&quot;</Arguments></Exec></Actions>\r\n" +
"</Task>";
    }

    private string SecurityElementEscape(string s) { return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;"); }
    private string Quote(string s) { return "\"" + s.Replace("\"", "\\\"") + "\""; }

    private void RemoveTask(object sender, EventArgs e)
    {
        if (!TaskExists()) { status.Text = "Task 'Persistent SD' is not installed."; return; }
        if (MessageBox.Show(this, "Remove the scheduled task 'Persistent SD'? The PowerShell script itself will not be deleted.", TaskName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var p = Run("schtasks.exe", "/Delete /TN " + Quote("\\" + TaskName) + " /F", false);
        status.Text = p.ExitCode == 0 ? "Scheduled task removed. Script file was left alone." : "Could not remove task: " + p.Output;
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        bool exists = TaskExists();
        status.Text = exists ? "Status: scheduled task 'Persistent SD' exists." : "Status: scheduled task 'Persistent SD' is not installed.";
        status.ForeColor = exists ? Color.LightGreen : Color.Khaki;
    }

    private ProcessResult Run(string exe, string args, bool show = true)
    {
        var psi = new ProcessStartInfo(exe, args) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = !show };
        using (var p = Process.Start(psi)) { string o = p.StandardOutput.ReadToEnd(); string er = p.StandardError.ReadToEnd(); p.WaitForExit(); return new ProcessResult { ExitCode = p.ExitCode, Output = o + er }; }
    }
    private sealed class ProcessResult { public int ExitCode; public string Output; }
}
