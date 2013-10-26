using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

namespace SharpDirect2DEngine
{
	/// <summary>
	/// Wrap SharpDX.Direct2D1.Bitmap in order to survive the Form Resize.
	/// 
	/// By default, we have an:
	///  SharpDX.SharpDXException: HRESULT: [0x88990012], Module: [SharpDX.Direct2D1], ApiCode: [D2DERR_WRONG_FACTORY/WrongFactory], Message: Unknown
	///  
	/// SharpDX.Direct2D1.Bitmap need to be reloaded when the canvas is resized. This class manage this rebuild.
	/// </summary>
	public class DXImage : IDisposable
	{
		/// <summary>
		/// A nice to have for debug.
		/// </summary>
		public readonly string Filename;
		public readonly int Width;
		public readonly int Height;
		private int[] pixels;
		internal SharpDX.Direct2D1.Bitmap bitmap;
		private DXCanvas canvas;

		internal DXImage(DXCanvas canvas, string filename)
		{
			this.Filename = filename;
			this.canvas = canvas;
			/// <remarks>
			/// Copy/paste and modified from:
			/// https://github.com/sharpdx/SharpDX/blob/master/Samples/Direct2D1/BitmapApp/Program.cs
			/// I suppose I must paste the license here:
			// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
			// 
			// Permission is hereby granted, free of charge, to any person obtaining a copy
			// of this software and associated documentation files (the "Software"), to deal
			// in the Software without restriction, including without limitation the rights
			// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
			// copies of the Software, and to permit persons to whom the Software is
			// furnished to do so, subject to the following conditions:
			// 
			// The above copyright notice and this permission notice shall be included in
			// all copies or substantial portions of the Software.
			// 
			// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
			// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
			// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
			// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
			// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
			// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
			// THE SOFTWARE.
			/// </remarks>
			using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filename))
			{
				this.Width = bitmap.Width;
				this.Height = bitmap.Height;
				var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
				var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
				this.pixels = new int[this.Width * this.Height];
				for (int y = 0; y < bitmap.Height; y++)
				{
					int offset = bitmapData.Stride * y;
					for (int x = 0; x < bitmap.Width; x++)
					{
						// Not optimized 
						byte B = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte G = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte R = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
						byte A = System.Runtime.InteropServices.Marshal.ReadByte(bitmapData.Scan0, offset++);
						int rgba = R | (G << 8) | (B << 16) | (A << 24);
						this.pixels[y * this.Width + x] = rgba;
					}
				}
				bitmap.UnlockBits(bitmapData);
			}
			this.rebuildDirectXBitmap();
		}
		/// <summary>
		/// Must refresh when canvas.renderTarget has changed.
		/// </summary>
		internal void rebuildDirectXBitmap()
		{
			if (this.bitmap != null)
			{
				this.bitmap.Dispose();
			}
			
			using (var tempStream = new SharpDX.DataStream(this.Height * this.Width * 4, true, true))
			{
				for (int i = 0; i < this.pixels.Length; i++)
				{
					tempStream.Write(this.pixels[i]);
				}
				tempStream.Position = 0;
				var size = new SharpDX.DrawingSize(this.Width, this.Height);
				var bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
				this.bitmap = new SharpDX.Direct2D1.Bitmap(this.canvas.renderTarget, size, tempStream, this.Width * 4, bitmapProperties);
			}
		}

		public void Dispose()
		{
			if (this.bitmap != null)
			{
				this.bitmap.Dispose();
				this.bitmap = null;
			}
			this.pixels = null;
			this.canvas.unregisterDXImage(this);
		}
	}
}
