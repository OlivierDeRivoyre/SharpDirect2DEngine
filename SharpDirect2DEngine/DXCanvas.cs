using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using System.Diagnostics;
using SharpDX.Direct2D1;
using System.Windows.Forms;
using SharpDX.DirectInput;
using SharpDX;

namespace SharpDirect2DEngine
{

	/// <summary>
	/// A basic Direct2D widows to handle simple game.
	/// 
	/// Currently handle:
	///  - Form Resize: resize the buffer
	///  - Full Screen: on pressing Alt+Enter
	///  - Mouse click: compute the
	///  
	/// Tested on dual screen under Windows 7
	/// </summary>
	public sealed class DXCanvas : IDisposable
	{
		public readonly SharpDX.Windows.RenderForm form;
		/// <summary>
		/// Time to update the game physic
		/// </summary>
		public EventHandler Update;		
		/// <summary>
		/// Time to draw
		/// </summary>
		public EventHandler<DrawEventArgs> Draw;
		public EventHandler<MouseEventArgs> Click;
		public int Width;
		public int Height;
		
		private SharpDX.DXGI.SwapChainDescription swapChainDescription;
		private SharpDX.Direct3D11.Device device;
		private SharpDX.DXGI.SwapChain swapChain;
		private SharpDX.DXGI.Surface backBuffer;
		public SharpDX.Direct2D1.RenderTarget renderTarget;
		private SharpDX.DXGI.Factory factory;
		private SharpDX.DirectInput.DirectInput directInput;
		private SharpDX.DirectInput.Mouse mouse;
		private SharpDX.DirectInput.MouseState lastMouseState;
		private long lastTick;

		/// <summary>
		/// Will need to be notified when resizing screen.
		/// </summary>
		private List<DXImage> existingImages = new List<DXImage>();

		public DXCanvas()
		{
			///Init as in the Katys Coe tutorial
			///http://katyscode.wordpress.com/2013/08/24/c-directx-api-face-off-slimdx-vs-sharpdx-which-should-you-choose/
			this.form = new RenderForm();
			this.swapChainDescription = new SwapChainDescription()
			{
				BufferCount = 1,
				Usage = Usage.RenderTargetOutput,
				OutputHandle = this.form.Handle,
				IsWindowed = true,				
				ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				SampleDescription = new SampleDescription(1, 0),
				Flags = SwapChainFlags.AllowModeSwitch,
				SwapEffect = SwapEffect.Discard			
			};
			/// The BgraSupport flag is needed for Direct2D compatibility otherwise new RenderTarget() will fail!
			SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, this.swapChainDescription, out this.device, out this.swapChain);
			this.factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
			this.factory.MakeWindowAssociation(this.form.Handle, WindowAssociationFlags.IgnoreAll);
			this.form.Size = new System.Drawing.Size(640, 480);		
			this.renderForm_Resize(null, null);
			this.form.KeyDown += this.renderForm_KeyDown;
			this.form.Resize += this.renderForm_Resize;

			this.directInput = new DirectInput();///http://gamedev.stackexchange.com/questions/61729/sharpdx-how-to-detect-if-mouse-button-is-pressed
			this.mouse = new Mouse(this.directInput);
			this.mouse.Acquire();
			this.lastMouseState = mouse.GetCurrentState();		
		}

		public void Dispose()
		{
			foreach (DXImage image in this.existingImages)
			{
				if (image.bitmap != null)
				{
					image.bitmap.Dispose();
				}
			}
			this.mouse.Dispose();
			this.directInput.Dispose();
			this.swapChain.Dispose();
			this.backBuffer.Dispose();
			this.renderTarget.Dispose();
			this.factory.Dispose();
			this.device.Dispose();			
		}

		public void Run()
		{
			this.lastTick = Stopwatch.GetTimestamp();
			RenderLoop.Run(this.form, this.runLoop);
		}

		private static float fixedTimeDuration = 16.6f * Stopwatch.Frequency / 1000;///60Hz = 16.6 ms
		private void runLoop()
		{

			long tick = Stopwatch.GetTimestamp();
			if (tick - this.lastTick < fixedTimeDuration)
			{
				return;
			}
			this.lastTick = tick;
			this.manageMouse();
			this.manageUpdate();
			this.manageDraw();
		}

		#region Update

		
		private void manageUpdate()
		{
			if (this.Update != null)
			{
				this.Update(this, EventArgs.Empty);
			}
		}


		#endregion Update

		#region Draw

	
		private void manageDraw()
		{
			this.renderTarget.BeginDraw();
			this.renderTarget.Transform = SharpDX.Matrix3x2.Identity;
			this.renderTarget.Clear(SharpDX.Color.Black);

			if (this.Draw != null)
			{
				this.Draw(this, new DrawEventArgs(this.renderTarget, this.Width, this.Height));
			}

			this.renderTarget.EndDraw();
			this.swapChain.Present(0, PresentFlags.None);
		}
		

		#endregion Draw

		#region OnClick

	
		private void manageMouse()
		{
			
			mouse.Acquire();
			var mouseState = mouse.GetCurrentState();
			if (mouseState.Buttons[0] && !lastMouseState.Buttons[0])///Left button was just pressed
			{				
				raiseOnClick(MouseButtons.Left);
			}
			if (mouseState.Buttons[1] && !lastMouseState.Buttons[1])///Right button was just pressed
			{
				raiseOnClick(MouseButtons.Right);
			}
			lastMouseState = mouseState;
		}

		private void raiseOnClick(MouseButtons mouseButton)
		{
			var formPoint = form.PointToClient(System.Windows.Forms.Cursor.Position);
			float x = formPoint.X * renderTarget.Size.Width / form.ClientSize.Width;
			float y = formPoint.Y * renderTarget.Size.Height / form.ClientSize.Height;
			if (this.Click != null)
			{
				this.Click(this, new MouseEventArgs(mouseButton, 1, (int)x, (int)y, 0));
			}
		}

		#endregion OnClick

		#region Resize

		private void renderForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				if (e.Alt && e.KeyCode == Keys.Enter)
				{
					if (!swapChain.IsFullScreen)
					{
						Trace.WriteLine("Swtich to FullScreen mode");
						Screen screen = Screen.FromControl(this.form);						
						resize(screen.Bounds.Width, screen.Bounds.Height);///Have the native desktop resolution in fullScreen
						swapChain.SetFullscreenState(true, null);
					}
					else
					{
						Trace.WriteLine("Swtich to  Window mode");
						resize(640, 480);
						swapChain.SetFullscreenState(false, null);
					}					
				}
				else if (e.KeyCode == Keys.Escape)
				{
					Trace.WriteLine("Close on ESC");
					swapChain.SetFullscreenState(false, null);///Return in window mode in order to have the desktop in a clean state
					this.form.Close();
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex);
			}
		}

		private void renderForm_Resize(object sender, EventArgs e)
		{
			try
			{
				///Have the canvas buffer of same size than the windows.
				resize(this.form.ClientSize.Width, this.form.ClientSize.Height);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex);
			}
		}

		private void resize(int width, int height)
		{
			Trace.WriteLine("Resize to " + width + "x" + height);
			if (this.backBuffer != null)
			{
				this.backBuffer.Dispose();
			}
			if (this.renderTarget != null)
			{
				this.renderTarget.Dispose();
			}
			this.swapChain.ResizeBuffers(this.swapChainDescription.BufferCount, width, height, this.swapChainDescription.ModeDescription.Format, this.swapChainDescription.Flags);
			this.Width = width;
			this.Height = height;
			this.initRenderTarget();
			///RenderTarget => need to refresh images
			this.reloadImages();
		}

		private void initRenderTarget()
		{
			this.backBuffer = Surface.FromSwapChain(swapChain, 0);
			using (var factory = new SharpDX.Direct2D1.Factory())
			{
				var dpi = factory.DesktopDpi;
				/// Create bitmap render target from DXGI surface
				renderTarget = new RenderTarget(factory, backBuffer, new RenderTargetProperties()
				{
					DpiX = dpi.Width,
					DpiY = dpi.Height,
					MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
					PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Ignore),
					Type = RenderTargetType.Default,
					Usage = RenderTargetUsage.None
				});
			}
		}

		#endregion Resize

		#region Image



		public DXImage LoadImageFromFile(string filename)
		{			
			DXImage image = new DXImage(this, filename);
			this.existingImages.Add(image);
			return image;
		}

		private void reloadImages()
		{
			foreach (DXImage image in this.existingImages)
			{
				image.rebuildDirectXBitmap();
			}
		}

		internal void unregisterDXImage(DXImage image)
		{
			this.existingImages.Remove(image);
		}

		#endregion Image

	}


	/// <summary>
	/// For Convenience
	/// </summary>
	public class DrawEventArgs : EventArgs
	{
		public readonly SharpDX.Direct2D1.RenderTarget Graphics;
		public readonly int Width;
		public readonly int Height;

		public DrawEventArgs(SharpDX.Direct2D1.RenderTarget graphics, int width, int height)
		{
			this.Graphics = graphics;
			this.Width = width;
			this.Height = height;
		}

	}
}
