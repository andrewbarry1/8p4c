using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace JamSeven
{
    class GameStateResults : GameState
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

        ContentManager Content;
        Game1 Game;
        InputHandler ih;

        Texture2D whiteTexture;

        Color[] playerColors;
        int[] scores;

        Sprite[] playerSprites;
        Sprite[] numberSprites;
        Vector2[] numberPositions;

        Sprite results;
        double resultsTimer;

        int slidePlayer;
        double[] slideAmounts;

        double endGameTimer;

        float bgColor;
        bool increasingBG;

        SoundEffect congrats;
        SoundEffect wipe;

        bool playedCongrats;

        public GameStateResults(ContentManager c, Game1 g, Color[] pc, int[] sc)
        {
            Content = c;
            Game = g;
            ih = new InputHandler();

            whiteTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Color[] whiteData = new Color[1] { Color.White };
            whiteTexture.SetData(whiteData);

            playerColors = pc;
            scores = sc;

            playerSprites = new Sprite[8];
            numberSprites = new Sprite[9];
            string[] nsNames = new String[9] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight" };
            numberPositions = new Vector2[9];
            for (int x = 0; x < 8; x++)
            {
                playerSprites[x] = new Sprite("player" + (x+1) + ".png", Content, Vector2.Zero, false);
                numberSprites[x] = new Sprite(nsNames[x] + ".png", Content, Vector2.Zero, false);
                Vector2 nsPos = new Vector2(20,
                    ((((Game1.SCREEN_HEIGHT / 8) * x) + ((Game1.SCREEN_HEIGHT / 8) * (x + 1))) / 2) -
                    numberSprites[x].size.Y / 2);
                numberPositions[x] = nsPos;
            }


            results = new Sprite("results.png", Content, Vector2.Zero, false);
            results.position = new Vector2(
                (Game1.SCREEN_WIDTH / 2) - (results.size.X / 2),
                (Game1.SCREEN_HEIGHT / 2) - (results.size.Y / 2));
            resultsTimer = 5000;

            slidePlayer = -1;
            slideAmounts = new double[8];
            endGameTimer = -1;

            bgColor = 0;
            increasingBG = true;

            congrats = Content.Load<SoundEffect>("congrats");
            wipe = Content.Load<SoundEffect>("resultslide");

            sortPlayers();
        }

        public void sortPlayers()
        {
            for (int i = 1; i < 8; i++)
            {
                int j = i;
                while (j > 0)
                {
                    if (scores[j - 1] > scores[j])
                    {
                        int temp = scores[j - 1];
                        Color tempColor = playerColors[j - 1];
                        Sprite tempSprite = playerSprites[j - 1];
                        scores[j - 1] = scores[j];
                        playerColors[j - 1] = playerColors[j];
                        playerSprites[j - 1] = playerSprites[j];
                        scores[j] = temp;
                        playerColors[j] = tempColor;
                        playerSprites[j] = tempSprite;
                        j--;
                    }
                    else
                        break;
                }
            }
            Array.Reverse(scores);
            Array.Reverse(playerColors);
            Array.Reverse(playerSprites);
        }

        public void update(double dt)
        {
            if (increasingBG)
            {
                bgColor += .5f;
                if (bgColor >= 50) increasingBG = false;
            }
            else
            {
                bgColor -= .5f;
                if (bgColor <= 0) increasingBG = true;
            }
            if (resultsTimer > 0)
            {
                resultsTimer -= dt;
                if (resultsTimer <= 0)
                {
                    slidePlayer = 7;
                    slideAmounts[slidePlayer] = 0;
                }
            }

            if (slidePlayer >= 0)
            {
                slideAmounts[slidePlayer] += dt * 5;
                if (slideAmounts[slidePlayer] >= Game1.SCREEN_WIDTH)
                {
                    wipe.Play();
                    slidePlayer--;
                    if (slidePlayer == -1)
                    {
                        endGameTimer = 1;
                    }
                }
            }

            if (endGameTimer > 0)
            {
                endGameTimer += dt;
                if (endGameTimer > 15000)
                {
                    Game.dropTwoStates();
                }
                if (endGameTimer > 1000 && !playedCongrats)
                {
                    congrats.Play();
                    playedCongrats = true;
                }
            }
            
        }

        public void handleInput()
        {
            ih.update(Keyboard.GetState());
            if (ih.allowSinglePress(Keys.Escape)) Game.Exit();
        }

        public void draw(SpriteBatch spriteBatch) // nothing
        {}

        public void drawUI(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(whiteTexture, position: Vector2.Zero,
                scale: new Vector2(Game1.SCREEN_WIDTH, Game1.SCREEN_HEIGHT), color: new Color(bgColor/255, bgColor/255, bgColor/255));


            if (resultsTimer > 0) results.draw(spriteBatch);
            for (int x = 0; x < 8; x++)
            {
                Vector2 pos = new Vector2(0, (Game1.SCREEN_HEIGHT / 8) * x);
                Vector2 scl = new Vector2((float)slideAmounts[x], (Game1.SCREEN_HEIGHT / 8));
                spriteBatch.Draw(whiteTexture, position: pos, scale: scl, color: playerColors[x]);
                if (slideAmounts[x] >= Game1.SCREEN_WIDTH)
                {
                    numberSprites[scores[x]].draw(spriteBatch, numberPositions[x]);
                    Vector2 psLoc = new Vector2(Game1.SCREEN_WIDTH - playerSprites[x].size.X - 25,
                        ((((Game1.SCREEN_HEIGHT / 8) * x) + ((Game1.SCREEN_HEIGHT / 8) * (x + 1))) / 2) -
                        playerSprites[x].size.Y / 2);
                    playerSprites[x].draw(spriteBatch, psLoc);
                }
            }
        }
    }
}
