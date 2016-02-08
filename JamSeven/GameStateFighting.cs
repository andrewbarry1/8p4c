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
    class GameStateFighting : GameState
    {


        public Camera camera
        {
            get;
            set;
        }
        bool GameState.hasCamera
        {
            get { return true; }
        }

        ContentManager Content;
        Game1 Game;
        InputHandler ih;

        Sprite go;
        double goTimer;

        Sprite winner;
        Color winnerColor;
        double winnerTime;

        public Sprite platform;
        Player playerOne;
        Sprite p1Field;
        int playerOneNumber;
        Player playerTwo;
        Sprite p2Field;
        int playerTwoNumber;

        Color[] playerColors;
        int[] scores;

        Vector2[] matchList;
        int currentMatch;

        public bool invertControls;
        public bool offline;
        public float cfAmount;

        Dictionary<int, int> controllerNumbers;

        List<Sprite> extraFields;
        List<float> extraFieldTimes;
        List<Color> extraFieldColors;

        NetworkInterface net;

        Sprite suddenDeath;
        double sdTimer;

        public Vector2 shakeAmnt;

        public float fieldMultiplier;

        Random ra;

        SoundEffect sdSF;
        SoundEffect gameSF;
        SoundEffect goSF;
        SoundEffect jumpSF;
        SoundEffect fieldSF;

        bool playedGSE;
        bool sentNet;

        public GameStateFighting(ContentManager c, Game1 g, Color[] pColors, NetworkInterface ni) {

            net = ni;

            shakeAmnt = Vector2.Zero;

            ra = new Random(Environment.TickCount);

            Content = c;
            Game = g;
            ih = new InputHandler();
            camera = new Camera();
            camera.setPosition(Vector2.Zero);

            go = new Sprite("go.png", Content, Vector2.Zero, false);
            go.move(new Vector2(
                (Game1.SCREEN_WIDTH / 2) - (go.size.X / 2),
                (Game1.SCREEN_HEIGHT / 2) - (go.size.Y / 2)));
            goTimer = 250;

            winner = new Sprite("game.png", Content, Vector2.Zero, false);
            winner.move(new Vector2(
                (Game1.SCREEN_WIDTH / 2) - (winner.size.X / 2),
                (Game1.SCREEN_HEIGHT / 2) - (winner.size.Y / 2)));
            winnerTime = 751;

            platform = new Sprite("platform.png", Content, Vector2.Zero, false);
            playerOne = new Player("player.png", Content, Vector2.Zero);
            p1Field = new Sprite("force.png", Content, Vector2.Zero, false);
            playerOneNumber = 0;
            playerTwo = new Player("player.png", Content, Vector2.Zero);
            p2Field = new Sprite("force.png", Content, Vector2.Zero, false);
            playerTwoNumber = 0;

            extraFields = new List<Sprite>();
            extraFieldTimes = new List<float>();
            extraFieldColors = new List<Color>();

            playerColors = pColors;
            scores = new int[8];

            matchList = new Vector2[28] {
                mv(0,7), mv(1,6), mv(2,5), mv(3,4),
                mv(0,6), mv(5,7), mv(4,1), mv(3,2),
                mv(0,5), mv(6,4), mv(7,3), mv(1,2),
                mv(0,4), mv(3,5), mv(2,6), mv(1,7),
                mv(0,3), mv(4,2), mv(5,1), mv(6,7),
                mv(0,2), mv(1,3), mv(7,4), mv(6,5),
                mv(0,1), mv(2,7), mv(3,6), mv(4,5)
            };
            matchList = shuffle(matchList);
            currentMatch = -1;

            fieldMultiplier = 1f;

            suddenDeath = new Sprite("suddendeath.png", Content, Vector2.Zero, false);
            suddenDeath.position = new Vector2(
                (Game1.SCREEN_WIDTH / 2) - (suddenDeath.size.X / 2), 50);

            playedGSE = false;

            controllerNumbers = new Dictionary<int, int>();
            controllerNumbers.Add(0, 1);
            controllerNumbers.Add(1, 1);
            controllerNumbers.Add(2, 2);
            controllerNumbers.Add(3, 2);
            controllerNumbers.Add(4, 3);
            controllerNumbers.Add(5, 3);
            controllerNumbers.Add(6, 4);
            controllerNumbers.Add(7, 4);

            camera.move(new Vector2(-350, (Game1.SCREEN_WIDTH / 5)));

            goSF = Content.Load<SoundEffect>("go");
            sdSF = Content.Load<SoundEffect>("suddendeath");
            gameSF = Content.Load<SoundEffect>("game");
            jumpSF = Content.Load<SoundEffect>("jump");
            fieldSF = Content.Load<SoundEffect>("field");

            prepareMatch();
        }

        // macro
        Vector2 mv(int x, int y)
        {
            return new Vector2(x, y);
        }
        Vector2[] shuffle(Vector2[] vecs)
        {
            Random ra = new Random(Environment.TickCount);
            List<KeyValuePair<int, Vector2>> list = new List<KeyValuePair<int, Vector2>>();
            foreach (Vector2 v in vecs)
            {
                list.Add(new KeyValuePair<int, Vector2>(ra.Next(), v));
            }
            var sorted = from item in list
                         orderby item.Key
                         select item;
            Vector2[] result = new Vector2[vecs.Length];
            int index = 0;
            foreach (KeyValuePair<int, Vector2> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            return result;
        }

        void prepareMatch()
        {
            currentMatch++;
            if (currentMatch >= matchList.Length)
            {
                net.sendMessage("s");
                Game.doStateTransition(new GameStateResults(Content, Game, playerColors, scores));
                return;
            }
            winnerTime = 751;
            goTimer = 250;
            sdTimer = 0;
            net.sendMessage("s");
            camera.resetZoom();

            extraFields = new List<Sprite>();
            extraFieldTimes = new List<float>();
            extraFieldColors = new List<Color>();

            playedGSE = false;
            sentNet = false;
            invertControls = false;

            shakeAmnt = Vector2.Zero;
            fieldMultiplier = 1f;
            cfAmount = 0f;

            camera.setPosition(new Vector2(350, -1 * (Game1.SCREEN_WIDTH / 5)));
            camera.resetRotation();

            platform.position = Vector2.Zero;

            playerOne.stickVelocity = Vector2.Zero;
            playerOne.gravityVelocity = Vector2.Zero;
            playerOne.lastStickPosition = Vector2.Zero;
            playerOneNumber = (int)matchList[currentMatch].X;
            playerOne.fireTime = 0;
            playerOne.position = new Vector2(25, -100);

            playerTwo.stickVelocity = Vector2.Zero;
            playerTwo.gravityVelocity = Vector2.Zero;
            playerTwo.lastStickPosition = Vector2.Zero;
            playerTwoNumber = (int)matchList[currentMatch].Y;
            playerTwo.fireTime = 0;
            playerTwo.position = new Vector2(633, -100);
            playerTwo.facingLeft = true;
        }

        double getPlayerDistance()
        {
            return getDistance(playerOne, playerTwo);
        }

        double getDistance(Sprite one, Sprite two)
        {
            Point p1c = one.boundingRectangle.Center;
            Point p2c = two.boundingRectangle.Center;
            return Math.Sqrt(Math.Pow(p1c.X - p2c.X, 2) + Math.Pow(p1c.Y - p2c.Y, 2));
        }

        public void createTempExplosion()
        {
            Sprite field = new Sprite("explosion.png", Content, new Vector2(ra.Next(-250, 850), ra.Next(-500, 100)), false);
            extraFields.Add(field);
            extraFieldTimes.Add(0);
            extraFieldColors.Add(new Color(ra.Next(255), ra.Next(255), ra.Next(255)));
        }

        public void update(double dt)
        {
            if (goTimer > 0)
            {
                goTimer -= dt;
                if (!playedGSE)
                {
                    goSF.Play();
                    playedGSE = true;
                }
            }
            if (winnerTime > 0 && winnerTime <= 750) winnerTime -= dt;
            if (winnerTime <= 0) prepareMatch();
            sdTimer += dt;
            if (sdTimer >= 5000)
            {
                if (!offline && !sentNet)
                {
                    net.sendMessage("g");
                    sentNet = true;
                }
                else if (offline)
                {
                    int message = ra.Next(8);
                    if (message == 0) // shake the camera
                    {
                        shakeAmnt += new Vector2(.1f, .1f);
                    }
                    else if (message == 1) // increase field power
                    {
                        fieldMultiplier += 0.5f;
                    }
                    else if (message == 2) // zoom out
                    {
                        camera.adjustZoom(-0.01f);
                    }
                    else if (message == 3) // lower the platform
                    {
                        platform.move(new Vector2(0f, 1f));
                    }
                    else if (message == 4) // invert controls
                    {
                        invertControls = !invertControls;
                    }
                    else if (message == 5) // flip screen
                    {
                        camera.adjustRotation(.1f);
                    }
                    else if (message == 6) // create center force
                    {
                        cfAmount += 0.5f;
                    }
                    else if (message == 7) // create explosions
                    {
                        createTempExplosion();
                    }
                }
            }

            for (int x = 0; x < extraFields.Count; x++)
            {
                extraFieldTimes[x] += (float)dt;
                if (extraFieldTimes[x] > 1000)
                {
                    extraFields.RemoveAt(x);
                    extraFieldTimes.RemoveAt(x);
                    extraFieldColors.RemoveAt(x);
                }
                else
                {
                    if (getDistance(playerOne, extraFields[x]) < 125)
                    {
                        if (playerOne.position.X > extraFields[x].position.X) playerOne.gravityVelocity.X += (2 * fieldMultiplier);
                        else playerOne.gravityVelocity.X -= (2 * fieldMultiplier);
                    }
                    if (getDistance(playerTwo, extraFields[x]) < 125)
                    {
                        if (playerTwo.position.X > extraFields[x].position.X) playerTwo.gravityVelocity.X += (2 * fieldMultiplier);
                        else playerTwo.gravityVelocity.X -= (2 * fieldMultiplier);
                    }
                }
            }

            if (shakeAmnt.X > 0 || shakeAmnt.Y > 0)
            {
                double mx = ra.NextDouble() * shakeAmnt.X;
                if (ra.NextDouble() > 0.5f) mx *= -1;
                double my = ra.NextDouble() * shakeAmnt.Y;
                if (ra.NextDouble() > 0.5f) my *= -1;
                camera.move(new Vector2((float)mx, (float)my));
            }

            playerOne.update(dt);
            playerTwo.update(dt);
            platform.update(dt);

            if (playerOne.fireTime > 0) playerOne.fireTime -= dt;
            if (playerTwo.fireTime > 0) playerTwo.fireTime -= dt;

            if (cfAmount > 0 && playerOne.position.X > platform.size.X / 2) playerOne.gravityVelocity.X += cfAmount;
            else if (cfAmount > 0 && playerOne.position.X < platform.size.X / 2) playerOne.gravityVelocity.X -= cfAmount;
            if (cfAmount > 0 && playerTwo.position.X > platform.size.X / 2) playerTwo.gravityVelocity.X += cfAmount;
            else if (cfAmount > 0 && playerTwo.position.X < platform.size.X / 2) playerTwo.gravityVelocity.X -= cfAmount;

            playerOne.doGravity(dt);
            if (playerOne.velocity.Y != 0) playerOne.grounded = false;
            playerTwo.doGravity(dt);
            if (playerTwo.velocity.Y != 0) playerTwo.grounded = false;

            if (playerOne.boundingRectangle.Intersects(platform.boundingRectangle) &&
                playerOne.position.Y + playerOne.size.Y - 5 > platform.position.Y)
            {
                playerOne.position.Y -= 100;
            }
            if (playerTwo.boundingRectangle.Intersects(platform.boundingRectangle) &&
                playerTwo.position.Y + playerOne.size.Y - 5 > platform.position.Y)
            {
                playerTwo.position.Y -= 100;
            }

            Rectangle ppoy = new Rectangle((int)playerOne.position.X,
                (int)playerOne.position.Y + (int)playerOne.velocity.Y,
                (int)playerOne.size.X, (int)playerOne.size.Y);
            Rectangle ppty = new Rectangle((int)playerTwo.position.X,
                (int)playerTwo.position.Y + (int)playerTwo.velocity.Y,
                (int)playerTwo.size.X, (int)playerTwo.size.Y);

            if (ppoy.Intersects(platform.boundingRectangle))
            {
                playerOne.gravityVelocity.Y = 0;
                playerOne.grounded = true;
                playerOne.animHandler.setAnimation(0, true);
            }
            if (ppty.Intersects(platform.boundingRectangle))
            {
                playerTwo.gravityVelocity.Y = 0;
                playerTwo.grounded = true;
                playerTwo.animHandler.setAnimation(0, true);
            }

            double playerDist = getPlayerDistance();
            if (playerOne.fireTime > 1000 && playerDist < 125)
            {
                if (playerTwo.position.X > playerOne.position.X) playerTwo.gravityVelocity.X += (2 * fieldMultiplier);
                else playerTwo.gravityVelocity.X -= (2 * fieldMultiplier);
            }
            if (playerTwo.fireTime > 1000 && playerDist < 125)
            {
                if (playerOne.position.X > playerTwo.position.X) playerOne.gravityVelocity.X += (2 * fieldMultiplier);
                else playerOne.gravityVelocity.X -= (2 * fieldMultiplier);
            }

            playerOne.position += playerOne.velocity;
            playerTwo.position += playerTwo.velocity;

            if (playerOne.position.Y > 5000 && winnerTime > 750)
            {
                winnerTime = 750;
                winnerColor = playerColors[playerTwoNumber];
                scores[playerTwoNumber]++;
                gameSF.Play();
            }
            else if (playerTwo.position.Y > 5000 && winnerTime > 750)
            {
                winnerTime = 750;
                winnerColor = playerColors[playerOneNumber];
                scores[playerOneNumber]++;
                gameSF.Play();
            }

            if (playerOne.fireTime > 1000)
            {
                p1Field.position = new Vector2(playerOne.boundingRectangle.Center.X - (p1Field.size.X / 2),
    playerOne.boundingRectangle.Center.Y - (p1Field.size.Y / 2));
            }
            if (playerTwo.fireTime > 1000)
            {
                p2Field.position = new Vector2(playerTwo.boundingRectangle.Center.X - (p2Field.size.X / 2),
    playerTwo.boundingRectangle.Center.Y - (p2Field.size.Y / 2));
            }

        }

        public void handleInput()
        {
            ih.update(Keyboard.GetState());
            if (ih.allowSinglePress(Keys.Escape)) Game.Exit();
            if (ih.allowSinglePress(Keys.T))
            {
                playerOne.gravityVelocity.X = -99;
            }
            if (ih.allowSinglePress(Keys.E))
            {
                createTempExplosion();
            }

            if (goTimer > 0) return;

            GamePadState gp1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gp2 = GamePad.GetState(PlayerIndex.Two);
            GamePadState gp3 = GamePad.GetState(PlayerIndex.Three);
            GamePadState gp4 = GamePad.GetState(PlayerIndex.Four);

            ih.updateGamepads(gp1, gp2, gp3, gp4);

            Vector2 p1stick = ih.sticks[playerOneNumber];
            Vector2 p2stick = ih.sticks[playerTwoNumber];
            if (invertControls)
            {
                p1stick *= -1;
                p2stick *= -1;
            }

            if (Math.Abs(playerOne.gravityVelocity.X) > 0)
            {
                if (playerOne.gravityVelocity.X > 0)
                {
                    playerOne.gravityVelocity.X += p1stick.X;
                    if (playerOne.gravityVelocity.X < 0) playerOne.gravityVelocity.X = 0;
                }
                else
                {
                    playerOne.gravityVelocity.X += p1stick.X;
                    if (playerOne.gravityVelocity.X > 0) playerOne.gravityVelocity.X = 0;
                }
            }
            playerOne.stickVelocity.X += (p1stick.X - playerOne.lastStickPosition.X) * 10;
            playerOne.lastStickPosition = p1stick;

            if (Math.Abs(playerTwo.gravityVelocity.X) > 0)
            {
                if (playerTwo.gravityVelocity.X > 0)
                {
                    playerTwo.gravityVelocity.X += p2stick.X;
                    if (playerTwo.gravityVelocity.X < 0) playerTwo.gravityVelocity.X = 0;
                }
                else
                {
                    playerTwo.gravityVelocity.X += p2stick.X;
                    if (playerTwo.gravityVelocity.X > 0) playerTwo.gravityVelocity.X = 0;
                }
            }
            playerTwo.stickVelocity.X += (p2stick.X - playerTwo.lastStickPosition.X) * 10;
            playerTwo.lastStickPosition = p2stick;

            if (playerOne.velocity.X < -0.1) playerOne.facingLeft = true;
            else if (playerOne.velocity.X > 0.1) playerOne.facingLeft = false;
            if (playerTwo.velocity.X < -0.1) playerTwo.facingLeft = true;
            else if (playerTwo.velocity.X > 0.1) playerTwo.facingLeft = false;

            if (playerOneNumber % 2 == 0) // left side of the controller
            {
                int p1cn = controllerNumbers[playerOneNumber];
                if (ih.allowSingleGPpress(p1cn, 'l') && playerOne.grounded)
                {
                    playerOne.gravityVelocity.Y = -60;
                    jumpSF.Play();
                    playerOne.animHandler.setAnimation(1, true);
                }
                if (ih.allowSingleGPpress(p1cn, 'j') && playerOne.fireTime <= 0)
                {
                    fieldSF.Play();
                    playerOne.fireTime = 2000;
                    p1Field.position = new Vector2(playerOne.boundingRectangle.Center.X - (p1Field.size.X / 2),
                        playerOne.boundingRectangle.Center.Y - (p1Field.size.Y / 2));
                }
            }
            else
            {
                int p1cn = controllerNumbers[playerOneNumber];
                if (ih.allowSingleGPpress(p1cn, 'r') && playerOne.grounded)
                {
                    playerOne.gravityVelocity.Y = -60;
                    jumpSF.Play();
                    playerOne.animHandler.setAnimation(1, true);
                }
                if (ih.allowSingleGPpress(p1cn, 'k') && playerOne.fireTime <= 0)
                {
                    fieldSF.Play();
                    playerOne.fireTime = 2000;
                    p1Field.position = new Vector2(playerOne.boundingRectangle.Center.X - (p1Field.size.X / 2),
                        playerOne.boundingRectangle.Center.Y - (p1Field.size.Y / 2));
                }
            }

            if (playerTwoNumber % 2 == 0) // left side of the controller
            {
                int p2cn = controllerNumbers[playerTwoNumber];
                if (ih.allowSingleGPpress(p2cn, 'l') && playerTwo.grounded)
                {
                    playerTwo.gravityVelocity.Y = -60;
                    jumpSF.Play();
                    playerTwo.animHandler.setAnimation(1, true);
                }
                if (ih.allowSingleGPpress(p2cn, 'j') && playerTwo.fireTime <= 0)
                {
                    fieldSF.Play();
                    playerTwo.fireTime = 2000;
                    p2Field.position = new Vector2(playerTwo.boundingRectangle.Center.X - (p2Field.size.X / 2),
                        playerTwo.boundingRectangle.Center.Y - (p2Field.size.Y / 2));
                }
            }
            else
            {
                int p2cn = controllerNumbers[playerTwoNumber];
                if (ih.allowSingleGPpress(p2cn, 'r') && playerTwo.grounded)
                {
                    playerTwo.gravityVelocity.Y = -60;
                    jumpSF.Play();
                    playerTwo.animHandler.setAnimation(1, true);
                }
                if (ih.allowSingleGPpress(p2cn, 'k') && playerTwo.fireTime <= 0)
                {
                    fieldSF.Play();
                    playerTwo.fireTime = 2000;
                    p2Field.position = new Vector2(playerTwo.boundingRectangle.Center.X - (p2Field.size.X / 2),
                        playerTwo.boundingRectangle.Center.Y - (p2Field.size.Y / 2));
                }
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {

            platform.draw(spriteBatch);
            playerOne.draw(spriteBatch, playerColors[playerOneNumber]);
            playerTwo.draw(spriteBatch, playerColors[playerTwoNumber]);
            if (playerOne.fireTime > 1000) p1Field.draw(spriteBatch);
            if (playerTwo.fireTime > 1000) p2Field.draw(spriteBatch);
            for (int x = 0; x < extraFields.Count; x++ )
            {
                extraFields[x].draw(spriteBatch, extraFieldColors[x]);
            }
        }

        public void drawUI(SpriteBatch spriteBatch)
        {
            if (goTimer > 0) go.draw(spriteBatch);
            if (winnerTime > 0 && winnerTime <= 750) winner.draw(spriteBatch, winnerColor);
            if (sdTimer > 5000 && sdTimer % 250 < 125) suddenDeath.draw(spriteBatch);
        }
    }
}
