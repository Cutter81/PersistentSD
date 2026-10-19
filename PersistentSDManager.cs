using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

internal static class Program
{
    [STAThread] private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

internal sealed class DiskChoice
{
    public string Id, Label;
    public override string ToString() { return Label; }
}

internal sealed class MainForm : Form
{
    private const string TaskName = "Persistent SD";
    private readonly ComboBox devices = new ComboBox();
    private readonly ComboBox letters = new ComboBox();
    private readonly Label status = new Label();
    private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private string TemplatePath { get { return Path.Combine(baseDir, "PersistentSD.ps1"); } }
    private string ScriptPath { get { return Path.Combine(GetSelectedDestination(), "PersistentSD.ps1"); } }
    private readonly RadioButton profileLocation = new RadioButton();
    private readonly RadioButton documentsLocation = new RadioButton();
    private string discoveredTaskScript = null;
    
    private string GetSelectedDestination()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (documentsLocation.Checked) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scripts");
        return Path.Combine(home, "Scripts");
    }

    public MainForm()
    {
        Text = "PersistentSD"; Width = 691; Height = 500; MinimumSize = new Size(691, 500);
        StartPosition = FormStartPosition.CenterScreen;
        try { Icon = new Icon(Path.Combine(baseDir, "RazorBlade.ico")); } catch { }
        BackColor = Color.FromArgb(30, 32, 36); ForeColor = Color.White;
        var sub = new Label { Text = "Choose the storage device and the drive letter it should keep.", Left = 26, Top = 20, Width = 630, Height = 25, Font = new Font("Segoe UI", 9), ForeColor = Color.Gainsboro };
        var dl = new Label { Text = "Storage device", Left = 26, Top = 55, Width = 220, Height = 22, ForeColor = Color.White };
        devices.Left = 26; devices.Top = 80; devices.Width = 623; devices.DropDownWidth = 623; devices.IntegralHeight = false; devices.MaxDropDownItems = 14; devices.DropDownStyle = ComboBoxStyle.DropDownList;
        // Use the native ComboBox text renderer. Owner-drawing separate columns clipped
        // the first glyph on some Windows display scales (e.g. the R in Realtek).
        // A single native-rendered string prevents any overlap between Disk N and the name.
        var refresh = MakeButton("Refresh Devices", 26, 119, 145, 32); refresh.Click += (s,e) => LoadDevices();
        var ll = new Label { Text = "Preferred drive letter", Left = 195, Top = 124, Width = 170, Height = 22, ForeColor = Color.White };
        letters.Left = 365; letters.Top = 119; letters.Width = 75; letters.DropDownStyle = ComboBoxStyle.DropDownList;
        for (char c = 'D'; c <= 'Z'; c++) letters.Items.Add(c.ToString() + ":");
        letters.SelectedItem = "D:";
        var destinationLabel = new Label { Text = "Generated script location", Left = 26, Top = 160, Width = 220, Height = 22, ForeColor = Color.White };
        profileLocation.Text = "%USERPROFILE%\\Scripts"; profileLocation.Left = 26; profileLocation.Top = 184; profileLocation.Width = 220; profileLocation.Checked = true; profileLocation.ForeColor = Color.White; profileLocation.BackColor = BackColor;
        documentsLocation.Text = "My Documents\\Scripts"; documentsLocation.Left = 260; documentsLocation.Top = 184; documentsLocation.Width = 200; documentsLocation.Checked = false; documentsLocation.ForeColor = Color.White; documentsLocation.BackColor = BackColor;
        
        LoadLocationPreference();
        var install = MakeButton("Save / Install / Update", 26, 225, 210, 40); install.Click += InstallTask;
        var check = MakeButton("Check Status", 247, 225, 125, 40); check.Click += (s,e) => RefreshStatus();
        var remove = MakeButton("Remove Task", 383, 225, 110, 40); remove.Click += RemoveTask;
        var scheduler = MakeButton("Task Scheduler", 504, 225, 145, 40); scheduler.Click += (s,e) => { try { Process.Start(new ProcessStartInfo("taskschd.msc") { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show(this, "Could not open Task Scheduler: " + ex.Message, TaskName, MessageBoxButtons.OK, MessageBoxIcon.Error); } };
        status.Left = 27; status.Top = 275; status.Width = 622; status.Height = 78; status.ForeColor = Color.LightGreen; status.Font = new Font("Segoe UI", 9); status.AutoSize = false; status.BorderStyle = BorderStyle.FixedSingle;
        var note = new Label { Text = "New scripts go in the selected Scripts folder. Existing task actions are inspected first. The boot task runs as SYSTEM; the selected letter may be reassigned from another volume.", Left = 26, Top = 365, Width = 622, Height = 50, ForeColor = Color.Silver, Font = new Font("Segoe UI", 8) };
        Controls.AddRange(new Control[] { sub, dl, devices, refresh, ll, letters, destinationLabel, profileLocation, documentsLocation, install, check, remove, scheduler, status, note });
        LoadDevices(); RefreshStatus();
    }

    private Button MakeButton(string text, int x, int y, int w, int h) { return new Button { Text = text, Left = x, Top = y, Width = w, Height = h, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 59, 66), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }; }

    private void LoadDevices()
    {
        DiskChoice selectedBefore = devices.SelectedItem as DiskChoice;
        string previous = selectedBefore == null ? null : selectedBefore.Id;
        devices.Items.Clear();
        try
        {
            string script = "$ErrorActionPreference='Stop'; Get-Disk | ForEach-Object { $id=[string]$_.UniqueId; $name=([string]$_.FriendlyName -replace '[|\r\n]',' '); $serial=([string]$_.SerialNumber -replace '[|\r\n]',' ').Trim(); Write-Output ($_.Number.ToString()+'|'+$id+'|'+$name+'|'+$serial+'|'+$_.Size) }";
            var r = Run("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command " + Quote(script), false);
            foreach (string line in r.Output.Split(new[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var a = line.Split('|'); if (a.Length < 5) continue;
                string id = a[1].Trim(); if (id.Length == 0) continue;
                long size = 0; long.TryParse(a[4], out size);
                var choice = new DiskChoice { Id = id, Label = "Disk " + a[0] + ": " + a[2] + (a[3].Length > 0 ? " (" + a[3] + ")" : "") + " | " + (size / (1024d*1024*1024)).ToString("0.0") + " GB" };
                devices.Items.Add(choice); if (id == previous) devices.SelectedItem = choice;
            }
            if (devices.SelectedIndex < 0 && devices.Items.Count > 0) devices.SelectedIndex = 0;
            if (devices.Items.Count == 0) status.Text = "No disks detected. Run as Administrator and click Refresh Devices.";
        }
        catch (Exception ex) { status.Text = "Could not enumerate disks: " + ex.Message; }
        LoadSavedPreferences();
    }

    private string PreferencePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PersistentSD", "preferences.txt"); } }

    private string FindExistingScript()
    {
        discoveredTaskScript = null;
        // The task action is authoritative; inspect its XML rather than assuming a filename.
        string[] names = new[] { "Persistent SD", "PersistentSD" };
        foreach (string name in names)
        {
            var r = Run("schtasks.exe", "/Query /TN " + Quote("\\" + name) + " /XML", false);
            if (r.ExitCode != 0) continue;
            try
            {
                var doc = new System.Xml.XmlDocument(); doc.LoadXml(ExtractXml(r.Output));
                var ns = new System.Xml.XmlNamespaceManager(doc.NameTable); ns.AddNamespace("t", "http://schemas.microsoft.com/windows/2004/02/mit/task");
                var args = doc.SelectSingleNode("//t:Actions/t:Exec/t:Arguments", ns);
                if (args != null)
                {
                    string a = args.InnerText;
                    var m = System.Text.RegularExpressions.Regex.Match(a, @"(?i)(?:-File|/File)\s+(?:""([^""]+\.ps1)""|([^\s]+\.ps1))");
                    if (m.Success) { string candidate = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value; if (Path.IsPathRooted(candidate) && File.Exists(candidate)) { discoveredTaskScript = candidate; return candidate; } }
                }
            } catch { }
        }
        // Fallback: look in both standard Scripts folders and their roots for an existing script.
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string[] candidates = new[] { Path.Combine(home,"Scripts","PersistentSD.ps1"), Path.Combine(docs,"Scripts","PersistentSD.ps1"), Path.Combine(home,"PersistentSD.ps1"), Path.Combine(docs,"PersistentSD.ps1") };
        foreach (string candidate in candidates) if (File.Exists(candidate)) { discoveredTaskScript = candidate; return candidate; }
        // Finally scan the two roots for any .ps1; only accept a unique match.
        var found = new System.Collections.Generic.List<string>();
        foreach (string dir in new[] { home, docs, Path.Combine(home,"Scripts"), Path.Combine(docs,"Scripts") })
            try { if (Directory.Exists(dir)) foreach (string f in Directory.GetFiles(dir,"*.ps1",SearchOption.TopDirectoryOnly)) if (!found.Contains(f)) found.Add(f); } catch { }
        if (found.Count == 1) { discoveredTaskScript = found[0]; return found[0]; }
        discoveredTaskScript = null;
        return null;
    }

    private bool ExistingTask(out string name)
    {
        foreach (string n in new[] { "Persistent SD", "PersistentSD" })
            if (Run("schtasks.exe", "/Query /TN " + Quote("\\" + n), false).ExitCode == 0) { name = n; return true; }
        name = null; return false;
    }
    private void LoadLocationPreference()
    {
        try
        {
            string p = PreferencePath;
            if (!File.Exists(p)) return;
            // Preferences are stored as three lines: location, device ID, and drive letter.
            // Read only the location line so the saved device and letter do not affect this choice.
            string[] values = File.ReadAllLines(p);
            string location = values.Length > 0 ? values[0].Trim() : "Profile";
            documentsLocation.Checked = location == "Documents";
            profileLocation.Checked = location != "Documents";
        }
        catch { }
    }
    private void LoadSavedPreferences()
    {
        try {
            string p = PreferencePath; if (!File.Exists(p)) return;
            string[] a = File.ReadAllLines(p);
            if (a.Length >= 3) {
                foreach (DiskChoice d in devices.Items) if (d.Id == a[1]) { devices.SelectedItem = d; break; }
                string l = a[2].Trim().TrimEnd(':').ToUpperInvariant() + ":"; if (letters.Items.Contains(l)) letters.SelectedItem = l;
            }
        } catch { }
    }
    private void SavePreferences(string deviceId, string letter)
    {
        string dir = Path.GetDirectoryName(PreferencePath); Directory.CreateDirectory(dir);
        File.WriteAllLines(PreferencePath, new[] { documentsLocation.Checked ? "Documents" : "Profile", deviceId, letter }, new UTF8Encoding(false));
    }

    private bool TaskExists() { string n; return ExistingTask(out n); }

    private void InstallTask(object sender, EventArgs e)
    {
        var disk = devices.SelectedItem as DiskChoice;
        if (disk == null) { MessageBox.Show(this, "Select a storage device first.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (letters.SelectedItem == null) { MessageBox.Show(this, "Select a drive letter first.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        string selectedLetter = letters.SelectedItem.ToString().Trim().ToUpperInvariant();
        if (selectedLetter.Length != 2 || selectedLetter[1] != ':' || selectedLetter[0] < 'D' || selectedLetter[0] > 'Z') { MessageBox.Show(this, "Only drive letters D: through Z: are allowed. A:, B:, and C: are prohibited.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (String.IsNullOrWhiteSpace(disk.Id)) { MessageBox.Show(this, "The selected device does not expose a stable Windows disk UniqueId. It cannot be configured safely.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!File.Exists(TemplatePath)) { MessageBox.Show(this, "PersistentSD.ps1 template must be beside PersistentSD.exe.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        string existingName; bool existingTask = ExistingTask(out existingName);
        string existingScript = existingTask ? FindExistingScript() : null;
        if (existingTask) {
            string details = existingScript == null ? "The existing task was found, but its script path could not be resolved safely." : "Existing task: " + existingName + "\nScript: " + existingScript;
            var answer = MessageBox.Show(this, details + "\n\nYes = update this task to use the selected settings\nNo = leave the existing task and script untouched\nCancel = do nothing", TaskName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) { status.Text = answer == DialogResult.No ? "Existing task left untouched." : "No changes made."; return; }
            if (existingScript != null && !String.Equals(existingScript, ScriptPath, StringComparison.OrdinalIgnoreCase)) {
                var useExisting = MessageBox.Show(this, "The existing task points to a script outside the selected destination.\n\nYes = preserve and update the existing script at its current path\nNo = use the selected destination and update the task path", TaskName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (useExisting == DialogResult.Cancel) return;
                if (useExisting == DialogResult.Yes) { discoveredTaskScript = existingScript; }
                else { discoveredTaskScript = null; }
            }
        }
        try {
            int matchingIds = 0;
            foreach (DiskChoice item in devices.Items) if (item.Id == disk.Id) matchingIds++;
            if (matchingIds != 1) { MessageBox.Show(this, "The selected device ID is ambiguous. Refresh devices and select a uniquely identified disk.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string targetScript = (existingTask && discoveredTaskScript != null && File.Exists(discoveredTaskScript)) ? discoveredTaskScript : ScriptPath;
            string destination = Path.GetDirectoryName(targetScript);
            Directory.CreateDirectory(destination);
            string template = File.ReadAllText(TemplatePath);
            string generated = template.Replace("__DEVICE_UNIQUE_ID_BASE64__", Convert.ToBase64String(Encoding.UTF8.GetBytes(disk.Id)))
                                       .Replace("__PREFERRED_LETTER__", selectedLetter.TrimEnd(':'));
            // Validate that the template placeholders were all consumed before touching an existing file.
            if (generated.Contains("__DEVICE_UNIQUE_ID_BASE64__") || generated.Contains("__PREFERRED_LETTER__"))
                throw new InvalidOperationException("The script template contains unresolved configuration placeholders. No files were changed.");
            string backupPath = null;
            string backupDir = Path.Combine(destination, "PersistentSD-Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            // Always create a recovery point before replacing an existing task, even when
            // its script path cannot be resolved or the new target file does not exist yet.
            if (existingTask || File.Exists(targetScript) || (existingScript != null && File.Exists(existingScript)))
            {
                Directory.CreateDirectory(backupDir);
                if (File.Exists(targetScript))
                {
                    backupPath = Path.Combine(backupDir, Path.GetFileName(targetScript) + ".bak");
                    File.Copy(targetScript, backupPath, false);
                }
                // If switching destinations, back up the old task's actual script too. Never remove it here.
                if (existingScript != null && File.Exists(existingScript) && !String.Equals(Path.GetFullPath(existingScript), Path.GetFullPath(targetScript), StringComparison.OrdinalIgnoreCase))
                {
                    string oldScriptBackup = Path.Combine(backupDir, Path.GetFileName(existingScript) + ".previous.bak");
                    File.Copy(existingScript, oldScriptBackup, false);
                    if (backupPath == null) backupPath = oldScriptBackup;
                }
                if (existingTask)
                {
                    var oldXml = Run("schtasks.exe", "/Query /TN " + Quote("\\" + existingName) + " /XML", false);
                    if (oldXml.ExitCode != 0 || oldXml.Output.IndexOf("<Task", StringComparison.OrdinalIgnoreCase) < 0)
                        throw new InvalidOperationException("Could not back up the existing scheduled task XML. The task was not replaced. " + oldXml.Output);
                    File.WriteAllText(Path.Combine(backupDir, "ScheduledTask.xml"), ExtractXml(oldXml.Output), new UTF8Encoding(false));
                }
            }
            File.WriteAllText(targetScript, generated, new UTF8Encoding(false));
            SavePreferences(disk.Id, letters.SelectedItem.ToString().TrimEnd(':'));
            string xml = BuildTaskXml(targetScript);
            string temp = Path.Combine(Path.GetTempPath(), "PersistentSD-" + Guid.NewGuid().ToString("N") + ".xml"); File.WriteAllText(temp, xml, new UTF8Encoding(false));
            var p = Run("schtasks.exe", "/Create /TN " + Quote("\\" + (existingTask ? existingName : TaskName)) + " /XML " + Quote(temp) + " /F", false); try { File.Delete(temp); } catch { }
            if (p.ExitCode == 0) {
                string verified = FindExistingScript();
                if (verified != null && String.Equals(Path.GetFullPath(verified), Path.GetFullPath(targetScript), StringComparison.OrdinalIgnoreCase)) {
                    status.Text = "Installed and verified. Task: " + (existingTask ? existingName : TaskName) + " | Letter: " + letters.SelectedItem + (backupPath == null ? "" : " | Backup: " + backupPath);
                    status.ForeColor = Color.LightGreen;
                } else {
                    status.Text = "Task registration returned success, but script path verification failed. Backup retained: " + (backupPath ?? "none");
                    status.ForeColor = Color.Khaki;
                    MessageBox.Show(this, status.Text + "\n\nReview Task Scheduler before relying on this setup.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else MessageBox.Show(this, "Task creation failed. The generated script was saved, but the task was not confirmed.\n\n" + p.Output + "\n\nRun this utility as Administrator.", TaskName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshStatus();
        } catch (Exception ex) { MessageBox.Show(this, ex.Message, TaskName, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private string BuildTaskXml(string path)
    {
        string p = SecurityElementEscape(path);
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">\r\n" +
" <RegistrationInfo><Author>PersistentSD</Author><URI>\\Persistent SD</URI></RegistrationInfo>\r\n" + " <Triggers><BootTrigger><Enabled>true</Enabled></BootTrigger></Triggers>\r\n" +
" <Principals><Principal id=\"Author\"><UserId>S-1-5-18</UserId><RunLevel>HighestAvailable</RunLevel></Principal></Principals>\r\n" +
" <Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>true</AllowHardTerminate><StartWhenAvailable>true</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden><RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>7</Priority></Settings>\r\n" +
" <Actions Context=\"Author\"><Exec><Command>PowerShell.exe</Command><Arguments>-NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File &quot;" + p + "&quot;</Arguments></Exec></Actions>\r\n</Task>";
    }
    private string ExtractXml(string text)
    {
        int start = text.IndexOf("<Task", StringComparison.OrdinalIgnoreCase);
        if (start < 0) return text;
        return text.Substring(start).Trim();
    }
    private string SecurityElementEscape(string s) { return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;"); }
    private string Quote(string s) { return "\"" + s.Replace("\"", "\\\"") + "\""; }
    private void RemoveTask(object sender, EventArgs e) { string task; if (!ExistingTask(out task)) { status.Text = "No PersistentSD task found."; return; } if (MessageBox.Show(this, "Remove the scheduled task '" + task + "'? The script will not be deleted.", TaskName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return; var p = Run("schtasks.exe", "/Delete /TN " + Quote("\\" + task) + " /F", false); status.Text = p.ExitCode == 0 ? "Scheduled task removed. Script and configuration were left alone." : "Could not remove task: " + p.Output; RefreshStatus(); }
    private void RefreshStatus() {
        try {
            string name; bool exists = ExistingTask(out name);
            string foundScript = FindExistingScript();
            var lines = new System.Collections.Generic.List<string>();
            lines.Add("Scheduled task: " + (exists ? "FOUND — " + name : "NOT FOUND (checked 'Persistent SD' and 'PersistentSD')"));
            if (exists && foundScript != null) lines.Add("Script full path: " + Path.GetFullPath(foundScript));
            else if (discoveredTaskScript != null) lines.Add("Script full path (fallback match): " + Path.GetFullPath(discoveredTaskScript));
            else lines.Add("Script full path: NOT RESOLVED — checked task action and standard profile/Documents locations.");
            lines.Add("Status check time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            status.Text = String.Join(Environment.NewLine, lines.ToArray());
            status.ForeColor = exists && foundScript != null ? Color.LightGreen : Color.Khaki;
        } catch (Exception ex) {
            status.Text = "Status check failed: " + ex.Message;
            status.ForeColor = Color.OrangeRed;
            MessageBox.Show(this, status.Text, TaskName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    private ProcessResult Run(string exe, string args, bool show = true) {
        try {
            var psi = new ProcessStartInfo(exe, args) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = !show };
            using (var p = Process.Start(psi)) {
                if (p == null) return new ProcessResult { ExitCode = -1, Output = "Windows did not start: " + exe };
                string o = p.StandardOutput.ReadToEnd(); string er = p.StandardError.ReadToEnd(); p.WaitForExit();
                return new ProcessResult { ExitCode = p.ExitCode, Output = (o + Environment.NewLine + er).Trim() };
            }
        } catch (Exception ex) { return new ProcessResult { ExitCode = -1, Output = "Could not start '" + exe + "': " + ex.Message }; }
    }
    private sealed class ProcessResult { public int ExitCode; public string Output; }
}
