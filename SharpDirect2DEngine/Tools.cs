using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct2D1;
using SharpDX;

namespace SharpDirect2DEngine
{
	public static class Tools
	{
		public static void DrawImage(this RenderTarget renderTarget, DXImage image, RectangleF destinationRectangle, float opacity = 1.0f)
		{
			if (image.bitmap == null)
			{
				throw new ArgumentException("DXImage disposed. Image: '" + image.Filename + "'");
			}
			renderTarget.DrawBitmap(image.bitmap, destinationRectangle, opacity, BitmapInterpolationMode.Linear);
		}
		public static void DrawImage(this RenderTarget renderTarget, DXImage image, RectangleF? destinationRectangle, RectangleF? sourceRectangle, float opacity = 1.0f)
		{
			if (image.bitmap == null)
			{
				throw new ArgumentException("DXImage disposed. Image: '" + image.Filename + "'");
			}			
			renderTarget.DrawBitmap(image.bitmap, destinationRectangle, opacity, BitmapInterpolationMode.Linear, sourceRectangle);
		}

		/// <summary>
		/// Compute a transform-matrix to stretch to the screen my fixed-size zone.
		/// 
		/// If the ratio does not fit the screen, it will add a black band in order to not deform the image.
		/// 
		/// The result must be apply to SharpDX.Direct2D1.RenderTarget.Transform
		/// </summary>
		public static SharpDX.Matrix3x2 GetTransformForMyBound(int desiredWidth, int desiredHeight, DXCanvas canvas)
		{
			int screenWidth = canvas.Width;
			int screenHeight = canvas.Height;

			///Assuming we want to emulate a screen size of 1x1
			float widthScale = ((float)screenWidth) / desiredWidth;
			float heightScale = ((float)screenHeight) / desiredHeight;
			///Does the screen is larger or longer?
			bool isLargerThanLonger = widthScale > heightScale;
			float offerSetX = 0;
			float offerSetY = 0;
			float scale;
			if (isLargerThanLonger)
			{
				///We scale until the Height take full space
				scale = heightScale;
				/// We will have vertical black-bar (at left and right)
				offerSetX = (screenWidth - desiredWidth * scale) / 2;
			}
			else
			{
				///Horizontal bar (at top and bottom)
				scale = widthScale;
				offerSetY = (screenHeight - desiredHeight * scale) / 2;
			}

			///http://msdn.microsoft.com/en-us/library/windows/desktop/dd756655(v=vs.85).aspx
			SharpDX.Matrix3x2 transform = new Matrix3x2();
			transform.M11 = scale;
			transform.M22 = scale;
			transform.M31 = offerSetX;
			transform.M32 = offerSetY;
			return transform;
		}
	}
}
