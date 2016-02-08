using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;

namespace JamSeven
{
    public class Game1 : Game
    {

		public const double FADE_TIME = 1000.0; // fade to white timing
        public static int SCREEN_WIDTH;
        public static int SCREEN_HEIGHT;
        public static float MEDIA_VOLUME;


        public static GraphicsDeviceManager graphics;

        private SpriteBatch spriteBatch;
		public Stack<GameState> states;

		private Texture2D whiteTexture;
		private double whiteFade;
        private GameState queuedState;
        private bool fadingIn;
        public delegate void fadeDelegate();
		public fadeDelegate queuedDelegate;

        int bgColor;

        NetworkInterface ni;

        Song fightMusic;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.Window.Title = "8p4c";
        }

        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
			graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			graphics.ApplyChanges ();

            this.IsMouseVisible = false;

			states = new Stack<GameState> ();
			whiteFade = 0.0;

            MEDIA_VOLUME = .3f;

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = MEDIA_VOLUME;

            base.Initialize();

        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
			whiteTexture = new Texture2D (GraphicsDevice, 1, 1);
			Color[] whiteData = new Color[1];
			whiteData [0] = Color.White;
			whiteTexture.SetData (whiteData);
            SCREEN_WIDTH = GraphicsDevice.DisplayMode.Width;
            SCREEN_HEIGHT = GraphicsDevice.DisplayMode.Height;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.ApplyChanges();

            fightMusic = Content.Load<Song>("combat");
            bgColor = 175;

            ni = new NetworkInterface("ws://8p4c.andrewbarry.me/ws", Content);
            ni.setOnReady(sendHello);
            states.Push(new GameStateEntryMenu(this, Content, ni));
            MediaPlayer.Play(fightMusic);
        }

        public void sendHello()
        {
            ni.sendMessage("r");
        }

		public void startFade(fadeDelegate fDel) {
			whiteFade = 0.01;
			fadingIn = false;
			queuedDelegate = fDel;
		}

		// do addState after FADE_TIME-second white fade
		public void doStateTransition(GameState newState) {
			startFade (addState);
			queuedState = newState;
		}

        public void dropTwoStates()
        {
            startFade(doubleDrop);
        }
        public void doubleDrop()
        {
            states.Pop();
            states.Pop();
            ((GameStateEntryMenu)states.Peek()).reset();
        }

        public void removeState()
        {
            startFade(popState);
        }

        public void popState()
        {
            states.Pop();
        }

		// triggers when white is totally blocking current state draw
		public void whiteFadeTrigger() {
			queuedDelegate ();
            Console.WriteLine("Done doing QD, fadeIn to true");
			fadingIn = true;
		}

		// add GameState to stack and unfade
		private void addState() {
			states.Push (queuedState);
            if (queuedState is GameStateFighting)
            {
                Console.WriteLine("Fight music play");
            }
		}


        protected override void Update(GameTime gameTime)
        {
            if (whiteFade >= 1.0 && !fadingIn) {
                Console.WriteLine("WFT"); // wii fit trainer
				whiteFadeTrigger ();
                return;
			}

			double dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (states.Peek() is GameStateFighting)
            {
                Random ra = new Random(Environment.TickCount);
                int n = ra.Next(-5, 6);
                bgColor = Math.Max(0, Math.Min(255, bgColor + n));
            }

            if (whiteFade > 0.0)
            {
                double plusAlpha = dt / FADE_TIME;
                if (fadingIn)
                {
                    whiteFade -= plusAlpha;
                }
                else
                {
                    whiteFade += plusAlpha;
                }
			}
            else if (states.Count != 0)
            {
                states.Peek().update(dt);
                states.Peek().handleInput();
            }

        }


        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            if (states.Peek() is GameStateFighting)
            {
                spriteBatch.Begin();
                Vector2 bgpos = Vector2.Zero;
                Vector2 bgbr = new Vector2(Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT);
                spriteBatch.Draw(whiteTexture, position: bgpos, scale: bgbr, color: new Color(bgColor, bgColor, bgColor));
                spriteBatch.End();
            }

			if (states.Count != 0 && states.Peek().hasCamera) {
                spriteBatch.Begin(samplerState: SamplerState.LinearClamp, transformMatrix: states.Peek().camera.TranslationMatrix);
				states.Peek ().draw (spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin();
			}
            else
            {
                spriteBatch.Begin();
                if (states.Count != 0)
                {
                    states.Peek().draw(spriteBatch);
                }
            }

            states.Peek().drawUI(spriteBatch);

            if (whiteFade > 0.0)  // do fade-in / fade-out
            {
				Vector2 scale = new Vector2(graphics.GraphicsDevice.PresentationParameters.BackBufferWidth, 
                    graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);
				spriteBatch.Draw (whiteTexture, Vector2.Zero, null, 
				                 Color.White * (float)whiteFade, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}

			spriteBatch.End ();

        }

    }
}

