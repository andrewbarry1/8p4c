using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JamSeven
{
    class GameStateEntryMenu : GameState
    {

        public Camera camera
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool hasCamera
        {
            get { return false; }
        }

        Game1 Game;
        ContentManager Content;
        InputHandler ih;

        Sprite instructions;

        Texture2D whiteTexture;

        Color[] playerColors;
        Sprite[] numberSprites;
        bool[] playersReady;
        float[] playerSlides;

        Sprite getReady;
        double readyWait;

        NetworkInterface net;

        SoundEffect ready;
        SoundEffect entry;
        bool playReadySF;

        public GameStateEntryMenu(Game1 g, ContentManager c, NetworkInterface ni)
        {
            Game = g;
            Content = c;
            ih = new InputHandler();
            net = ni;

            instructions = new Sprite("instructions.png", Content, new Vector2(0, 0), false);
            float insSclA = (2 * Game1.SCREEN_HEIGHT / 3) / instructions.size.Y;
            float insMidX = (Game1.SCREEN_WIDTH / 2) - ((instructions.size.X * insSclA) / 2);
            instructions.move(new Vector2(insMidX, 0));

            playersReady = new bool[8];

            playerSlides = new float[8];

            readyWait = 3000;
            getReady = new Sprite("ready.png", Content, Vector2.Zero, false);
            getReady.move(new Vector2(
                (Game1.SCREEN_WIDTH / 2) - (getReady.size.X / 2),
                (Game1.SCREEN_HEIGHT / 2) - (getReady.size.Y / 2)));

            numberSprites = new Sprite[8];
            string[] nsNames = new String[8] {"one","two","three","four","five","six","seven","eight"};
            Vector2 firstLocation = new Vector2(Game1.SCREEN_WIDTH / 16, 0);
            for (int x = 0; x < 8; x++)
            {
                Vector2 loc = firstLocation + new Vector2((Game1.SCREEN_WIDTH / 8) * x, 
                    (Game1.SCREEN_HEIGHT + (instructions.size.Y * insSclA)) / 2);
                numberSprites[x] = new Sprite(nsNames[x] + ".png", Content, loc, false);
                numberSprites[x].move(new Vector2(-1 * (numberSprites[x].size.X / 2), 0));
            }

            playerColors = new Color[8];
            Random ra = new Random(Environment.TickCount);
            for (int x = 0; x < 8; x++)
            {
                playerColors[x] = new Color(ra.Next(255), ra.Next(255), ra.Next(255));
            }
            whiteTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Color[] whiteData = new Color[1] {Color.White};
            whiteTexture.SetData(whiteData);

            playReadySF = true;
            ready = Content.Load<SoundEffect>("ready");
            entry = Content.Load<SoundEffect>("playerjoin");
        }


        public void update(double dt)
        {
            if (playersReady[0] && playersReady[1] &&
                playersReady[2] && playersReady[3] &&
                playersReady[4] && playersReady[5] &&
                playersReady[6] && playersReady[7])
            {
                if (playReadySF)
                {
                    ready.Play();
                    playReadySF = false;
                }
                readyWait -= dt;

                if (readyWait < 0)
                {
                    GameStateFighting gsf = new GameStateFighting(Content, Game, playerColors, net);
                    net.setGSF(gsf);
                    Game.doStateTransition(gsf);
                }
            }
            
            for (int x = 0; x < 8; x++) {
                if (playersReady[x]) playerSlides[x] += (float)dt * 4;
            }
        }

        public void reset()
        {
            playReadySF = true;
            Random ra = new Random(Environment.TickCount);
            for (int x = 0; x < 8; x++)
            {
                playerColors[x] = new Color(ra.Next(255), ra.Next(255), ra.Next(255));
                playersReady[x] = false;
                playerSlides[x] = 0f;
            }
            readyWait = 3000;
        }

        public void handleInput()
        {
            ih.update(Keyboard.GetState());

            if (ih.allowSinglePress(Keys.Escape))
            {
                Game.Exit();
            }

            if (ih.allowSinglePress(Keys.Space)) // debug
            {
                for (int x = 0; x < 8; x++) playersReady[x] = true;
            }

            GamePadState gps1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gps2 = GamePad.GetState(PlayerIndex.Two);
            GamePadState gps3 = GamePad.GetState(PlayerIndex.Three);
            GamePadState gps4 = GamePad.GetState(PlayerIndex.Four);
            ih.updateGamepads(gps1, gps2, gps3, gps4);

            for (int x = 0; x < 4; x++)
            {
                if (ih.allowSingleGPpress(1, 'j')) { playersReady[0] = true; entry.Play(); }
                if (ih.allowSingleGPpress(1, 'k')) { playersReady[1] = true; entry.Play(); }
                if (ih.allowSingleGPpress(2, 'j')) { playersReady[2] = true; entry.Play(); }
                if (ih.allowSingleGPpress(2, 'k')) { playersReady[3] = true; entry.Play(); }
                if (ih.allowSingleGPpress(3, 'j')) { playersReady[4] = true; entry.Play(); }
                if (ih.allowSingleGPpress(3, 'k')) { playersReady[5] = true; entry.Play(); }
                if (ih.allowSingleGPpress(4, 'j')) { playersReady[6] = true; entry.Play(); }
                if (ih.allowSingleGPpress(4, 'k')) { playersReady[7] = true; entry.Play(); }
            }

        }

        public void draw(SpriteBatch spriteBatch) // nothing happens in here because lolnocamera
        {}

        public void drawUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(whiteTexture, position: new Vector2(0, 0), scale: new Vector2(Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT));

            float insSclA = (2 * Game1.SCREEN_HEIGHT / 3) / instructions.size.Y;
            Vector2 insScl = new Vector2(insSclA, insSclA);

            spriteBatch.Draw(whiteTexture, position: new Vector2(0, 0), 
                scale: new Vector2(Game1.SCREEN_WIDTH, 2 * Game1.SCREEN_HEIGHT/3), color: Color.Black);
            instructions.drawSize(spriteBatch, insScl);

            for (int x = 0; x < 8; x++)
            {
                if (playersReady[x])
                {
                    Vector2 colPos = new Vector2((Game1.SCREEN_WIDTH / 8) * x, 2 * Game1.SCREEN_HEIGHT / 3);
                    Vector2 colScale = new Vector2(Game1.SCREEN_WIDTH / 8, (float)Math.Min(playerSlides[x], 2 * Game1.SCREEN_HEIGHT / 3));
                    spriteBatch.Draw(whiteTexture, position: colPos, scale: colScale, color: playerColors[x]);
                    if (Math.Min(playerSlides[x], 2 * Game1.SCREEN_HEIGHT / 3) >= 2 * Game1.SCREEN_HEIGHT / 3)
                    {
                        numberSprites[x].draw(spriteBatch);
                    }
                }
            }
            if (readyWait < 3000)
            {
                getReady.draw(spriteBatch);
            }
        }
    }
}
