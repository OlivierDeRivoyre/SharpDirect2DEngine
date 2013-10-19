SharpDirect2DEngine
===================

A basic Direct2D widows to handle simple game in Windows desktop in C#.

It's in .Net 2.0 with SharpDX to handle Direct X. The code is done with Visual Studio 2012.

The method 'Run()' start a windows that contains a surface on witch you can draw.

The idea is hide the DX complexity by having the following usages of this engine:

using (DXCanvas canvas = new DXCanvas())
{
	canvas.Click += canvas_Click;
	canvas.Update += canvas_Update;
	canvas.Draw += canvas_Draw;

	canvas.Run();
}

Currently, it handle:
 - the loop game.
 - the resizing.
 - the switch from windows mode to fullscreen.
 - the mouse.

Have a look on Program.cs to see what you can do with it. The program should start a windows where colored dots appears (and moves) on user's clicks.


Please note that I'm a complete noob in DirectX stuff (it's my first day). I currently do not understand all the parameters used here.
Feels free to copy/paste this code. I'm quite sure I will not support it very long.
