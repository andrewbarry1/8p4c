using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JamSeven
{
    class Player : Sprite
    {

        public Vector2 velocity
        {
            get
            {
                return gravityVelocity + stickVelocity;
            }
        }

        public Vector2 gravityVelocity;
        public Vector2 stickVelocity;

        public Vector2 lastStickPosition;
        public bool grounded;
        public double fireTime;


        public Player(string filename, ContentManager Content, Vector2 location)
            : base(filename, Content, location)
        {
            stickVelocity = Vector2.Zero;
            gravityVelocity = Vector2.Zero;
            lastStickPosition = Vector2.Zero;
            grounded = false;
            fireTime = 0;
        }

        public void doGravity(double dt)
        {
            gravityVelocity += new Vector2(0f, .23f * (float)dt);
        }
    }
}
