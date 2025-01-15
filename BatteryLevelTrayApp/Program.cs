namespace BatteryLevelTrayApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new TrayApplicationContext());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Application crashed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}