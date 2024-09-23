namespace webpfy
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (!Directory.Exists(Core.outputDir))
                {
                    Directory.CreateDirectory(Core.outputDir);
                }
            }catch (Exception e) {
                MessageBox.Show(e.Message);
            }
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}