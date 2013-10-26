using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDirect2DEngine;
using SharpDX;

namespace TestImage
{
	static class Program
	{
		private static DXImage image;

		/// <summary>
		/// Test the display of an image
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				using (DXCanvas canvas = new DXCanvas())
				{
					image = canvas.LoadImageFromFile("HappyFace.png");
					
					canvas.Draw += canvas_Draw;

					canvas.Run();					
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private static void canvas_Draw(object sender, DrawEventArgs e)
		{
			e.Graphics.Clear(SharpDX.Color.White);
			///Placement
			e.Graphics.DrawBitmap(image,
				new RectangleF(50, 50, 50 + image.Width, 50 + image.Height),
				0.7f);

			///For TileSet
			e.Graphics.DrawBitmap(image,
				new RectangleF(50, 300, 50 + 64, 300 + 64),
				1f,
				new RectangleF(30, 70, 30 + 30, 70 + 30)
				);

			///Zoom
			e.Graphics.DrawBitmap(image,
				new RectangleF(250, 250, e.Width, e.Height),
				0.5f);			
			
		}
	}
}
