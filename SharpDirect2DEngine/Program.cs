using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;

namespace SharpDirect2DEngine
{
	/// <summary>
	/// To check the DXCanvas work a little
	/// </summary>
	static class Program
	{
		public static Random rand = new Random();
		/// <summary>
		/// Moving points
		/// </summary>
		private static List<Dot> dots = new List<Dot>();

		/// <summary>
		/// Basic usage of DXCanvas
		/// </summary>
		[STAThread]
		public static void Main()
		{
			try
			{
				using (DXCanvas canvas = new DXCanvas())
				{
					canvas.Click += canvas_Click;
					canvas.Update += canvas_Update;
					canvas.Draw += canvas_Draw;

					canvas.Run();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// User has click => create a "moving dot"
		/// </summary>		
		private static void canvas_Click(object o, MouseEventArgs e)
		{
			float maxSpeed = 3f;
			dots.Add(new Dot
			{
				X = e.X - 5,
				Y = e.Y - 5,
				DX = ((float)rand.NextDouble() - 0.5f) * maxSpeed,
				DY = ((float)rand.NextDouble() - 0.5f) * maxSpeed,
				/// 0xFF the alpha
				///Color = new Color(0xFFFF0000)//Blue
				///Color = new Color(0xFF00FF00)//Green
				///Color = new Color(0xFF0000FF)//Red				
				Color = new Color(rand.Next())
			});
		}

		/// <summary>
		/// Move stuff
		/// </summary>		
		private static void canvas_Update(object sender, EventArgs e)
		{
			DXCanvas canvas = (DXCanvas) sender;
			foreach (var dot in dots)
			{
				dot.X = incrementModulus(dot.X, dot.DX, canvas.Width);
				dot.Y = incrementModulus(dot.Y, dot.DY, canvas.Height);
			}
		}

		/// <summary>
		/// Paint
		/// </summary>
		private static void canvas_Draw(object sender, DrawEventArgs e)
		{
			e.Graphics.Clear(Color.White);
			using (var brush = new SharpDX.Direct2D1.SolidColorBrush(e.Graphics, Color.LightSlateGray))
			{
				for (int x = 0; x < e.Width; x += 10)
					e.Graphics.DrawLine(new Vector2(x, 0), new Vector2(x, e.Height), brush, 0.5f);
				for (int y = 0; y < e.Height; y += 10)
					e.Graphics.DrawLine(new Vector2(0, y), new Vector2(e.Width, y), brush, 0.5f);
			}
			foreach (var dot in dots)
			{
				using (var brush = new SharpDX.Direct2D1.SolidColorBrush(e.Graphics, dot.Color))
				{
					e.Graphics.FillRectangle(new RectangleF(dot.X, dot.Y, dot.X + 10, dot.Y + 10), brush);
				}
			}
		}

		/// <summary>
		/// Stay in the windows
		/// </summary>
		private static float incrementModulus(float x, float dx, float width)
		{
			x += dx;
			if (x >= width)
			{
				x -= width;
			}
			if (x < 0)
			{
				x += width;
			}
			return x;
		}

		/// <summary>
		/// A dot that move
		/// </summary>
		private class Dot
		{
			public float X;
			public float Y;
			public float DX;
			public float DY;
			public SharpDX.Color Color;

		}
	}
}
