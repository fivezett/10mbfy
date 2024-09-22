using System.Diagnostics;

namespace webpfy
{
    public partial class MainForm : Form
    {
        private Core core = new();
        private string latestOutputDir = "";
        private string originalTitleName = "";
        public MainForm()
        {
            InitializeComponent();
        }

        private async void convertByClipBoard_Click(object sender, EventArgs e)
        {

            var files = Clipboard.GetFileDropList();
            string[] stringArray = new string[files.Count];
            files.CopyTo(stringArray, 0);
            var li = Pickup.pickup(stringArray);
            if (li.Count() == 0)
            {
                copyButton.Enabled = false;
                return;
            }
            convertByClipBoard.Enabled = false;
            latestOutputDir = await core.ConvertToWebp(li.ToArray());
            convertByClipBoard.Enabled = true;
            copyButton.Enabled = true;
            this.Text = originalTitleName;
        }

        private void forceFrontCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = forceFrontCheckBox.Checked;
        }

        private void openOutputDir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(latestOutputDir))
            {
                Process.Start("explorer.exe", Core.outputDir);
            }
            else
            {
                Process.Start("explorer.exe", latestOutputDir);
            }
        }

        private void openSettingsButton_Click(object sender, EventArgs e)
        {
            //new SettingsForm().ShowDialog();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            var ie_files = new DirectoryInfo(latestOutputDir).EnumerateFiles("*", SearchOption.AllDirectories);
            List<String> clipboardList = new List<String>();
            foreach (FileInfo f in ie_files)
            {
                clipboardList.Add(f.FullName);
            }
            IDataObject data = new DataObject(DataFormats.FileDrop, clipboardList.ToArray());
            Clipboard.SetDataObject(data, true);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            originalTitleName = this.Text;
            core.ProgressEventHandler += (int cnt, int total) =>
            {
                this.Invoke(() =>
                {
                    Debug.WriteLine(total + " " + cnt);
                    progressBar1.Maximum = total;
                    progressBar1.Value = cnt;
                    this.Text = originalTitleName + $" {cnt}/{total}";
                });
            };
        }
    }
}
