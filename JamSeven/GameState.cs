using System;
using Microsoft.Xna.Framework.Graphics;


namespace JamSeven
{
	public interface GameState
	{
        Camera camera
        {
            get;
            set;
        }

        bool hasCamera
        {
            get;
        }

		void update(double dt);
		void handleInput();
		void draw(SpriteBatch spriteBatch);
        void drawUI(SpriteBatch spriteBatch);

	}
}

