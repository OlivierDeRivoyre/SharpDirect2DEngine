using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDirect2DEngine;

namespace Bombermin
{
	/// <summary>
	/// A Bomberman game.
	/// </summary>
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try
			{
				using (DXCanvas canvas = new DXCanvas())
				{
					Game game = new Game(canvas);
					game.Run();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
