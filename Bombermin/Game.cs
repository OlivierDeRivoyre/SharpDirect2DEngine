using System;
using System.Collections.Generic;
using System.Text;
using SharpDirect2DEngine;
using SharpDX;


namespace Bombermin
{
	/// <summary>
	/// Bombermin manager
	/// </summary>
	public class Game
	{
		private DXCanvas canvas;
		public static Random rand = new Random();

		public Player[] players;
		public int[,] walls;

		private const int GROUND = 0;
		private const int WALL = 1;
		private const int UNBREAKABLE_WALL = 2;
		private DXImage imageGround;
		private DXImage imageWall;
		private DXImage imageUnbreakable;

		public Game(DXCanvas canvas)
		{
			this.canvas = canvas;
			this.imageGround = canvas.LoadImageFromFile("Images/ground.png");
			this.imageWall = canvas.LoadImageFromFile("Images/wall.png");
			this.imageUnbreakable = canvas.LoadImageFromFile("Images/unbreakable.png");
			
			canvas.Draw += this.canvas_Draw;
			StartNewGame();
		}

		public void Run()
		{
			this.canvas.Run();
		}

		public void StartNewGame()
		{
			buildWalls();
		}

		private void buildWalls()
		{
			this.walls = new int[16, 9];
			for (int i = 0; i < this.walls.GetLength(0); i++)
			{
				for (int j = 0; j < this.walls.GetLength(1); j++)
				{
					if (i % 2 == 1 && j % 2 == 1)
					{
						this.walls[i, j] = UNBREAKABLE_WALL;
					}
					else
					{
						if (rand.Next(100) < 50)
						{
							this.walls[i, j] = WALL;
						}
					}
				}
			}
		}

		private DXImage getWallImage(int i, int j)
		{
			int wall = this.walls[i, j];
			switch (wall)
			{
				case GROUND: return this.imageGround;
				case WALL: return this.imageWall;
				case UNBREAKABLE_WALL: return this.imageUnbreakable;
				default: throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Prevent to have black line between tiles due to rounding problems.
		/// </summary>
		private const float PIXELROUNDING = 1f;

		private void canvas_Draw(object sender, DrawEventArgs e)
		{
			e.Graphics.Transform = Tools.GetTransformForMyBound(this.walls.GetLength(0) * 64, this.walls.GetLength(1) * 64, this.canvas);
			
			for (int i = 0; i < this.walls.GetLength(0); i++)
			{
				for (int j = 0; j < this.walls.GetLength(1); j++)
				{
					e.Graphics.DrawImage(getWallImage(i, j),
					   new RectangleF(i * 64 - PIXELROUNDING, j * 64 - PIXELROUNDING, 
						   i * 64 + 64 + PIXELROUNDING, j * 64 + 64 + PIXELROUNDING));
				}
			}
		}


		
	}
}
