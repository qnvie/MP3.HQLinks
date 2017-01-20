using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MP3.HQLinks
{
	internal static class Program
	{
		internal static MainForm MainForm = null;
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Program.MainForm = new HQLinks.MainForm();
			Application.Run(Program.MainForm);
		}
	}
}
