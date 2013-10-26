using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct2D1;
using SharpDX;

namespace SharpDirect2DEngine
{
	public static class Tools
	{
		public static void DrawBitmap(this RenderTarget renderTarget, DXImage image, RectangleF destinationRectangle, float opacity = 1.0f)
		{
			if (image.bitmap == null)
			{
				throw new ArgumentException("DXImage disposed. Image: '" + image.Filename + "'");
			}
			renderTarget.DrawBitmap(image.bitmap, destinationRectangle, opacity, BitmapInterpolationMode.Linear);
		}
		public static void DrawBitmap(this RenderTarget renderTarget, DXImage image, RectangleF? destinationRectangle, float opacity, RectangleF? sourceRectangle)
		{
			if (image.bitmap == null)
			{
				throw new ArgumentException("DXImage disposed. Image: '" + image.Filename + "'");
			}
			renderTarget.DrawBitmap(image.bitmap, destinationRectangle, opacity, BitmapInterpolationMode.Linear, sourceRectangle);
		}
	}
}
