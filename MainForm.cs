using System.Diagnostics;

namespace webpfy
{
    public partial class MainForm : Form
    {
        private Core core = new();
        private string latestOutputDir = "";
        private string originalTitleName = "";
        private CancellationTokenSource? cancellationTokenSource = null;
        private bool isBusy
        {
            get
            {
                return convertByClipBoard.Text.IndexOf("キャンセル") != -1;
            }
            set
            {
                convertByClipBoard.Text = value ? "キャンセル" : "クリップボードから変換";
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private async void convertByClipBoard_Click(object sender, EventArgs e)
        {
            if (isBusy)
            {
                cancellationTokenSource?.Cancel();
                this.convertByClipBoard.Enabled = false;
                convertByClipBoard.Text = "キャンセル中...";
                return;
            }
            if (cancellationTokenSource == null) { cancellationTokenSource = new(); }
            var files = Clipboard.GetFileDropList();
            string[] stringArray = new string[files.Count];
            files.CopyTo(stringArray, 0);
            var filelist = Pickup.pickup(stringArray).ToArray();
            if (filelist.Count() == 0)
            {
                MessageBox.Show("クリップボードにファイルがありません");
            }
            await startConvert(filelist, cancellationTokenSource.Token);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        private async void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (isBusy)
            {
                MessageBox.Show("実行中です");
                return;
            }
            if (cancellationTokenSource == null) { cancellationTokenSource = new(); }
            string[] filelist = Pickup.pickup((string[])e.Data.GetData(DataFormats.FileDrop, false)).ToArray();
            if (filelist.Count() == 0)
            {
                MessageBox.Show("ファイルがありません");
            }
            await startConvert(Pickup.pickup(filelist).ToArray(), cancellationTokenSource.Token);
        }

        private async Task startConvert(string[] paths, CancellationToken cancellationToken)
        {
            Exception ex = null;
            if (paths.Count() == 0)
            {
                copyButton.Enabled = false;
                return;
            }
            isBusy = true;
            try
            {
                latestOutputDir = await core.ConvertToWebp(paths, cancellationToken);
            }
            catch (Exception e)
            {
                ex = e;
            }
            isBusy = false;
            copyButton.Enabled = true;
            convertByClipBoard.Enabled = true;
            this.Text = originalTitleName;
            if (ex is TaskCanceledException)
            {
                this.Text += " キャンセル";
                progressBar1.Value = 0;
                copyButton.Enabled = false;
            }
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
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

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(latestOutputDir)) { MessageBox.Show("変換されたファイルがありません"); return; }
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
