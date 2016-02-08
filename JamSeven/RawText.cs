// Does not support any control chars. Don't pass them or they'll apear literally

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JamSeven
{


    public class TextLetter
    {
        public int line;
        public bool visible;

        private Vector2 letterSize;
        private char letter;
        private Random ra;

        // effect-specific stuff.
        private Color currentColor;
        private Vector2 offset;
        private float diffX;
        private float diffY;

        public TextLetter(char c, Vector2 ls, Random r)
        {

            letterSize = ls;
            //line = l;
            letter = c;
            ra = r;
            offset = Vector2.Zero;
            currentColor = Color.Black;
            visible = false;
        }

        public void setColor(Color c)
        {
            currentColor = c;
        }

        public void update(double dt)
        {
        }

        public void draw(SpriteBatch spriteBatch, Vector2 drawLocation, Texture2D font)
        {
            spriteBatch.Draw(font, drawLocation + offset,
               new Rectangle((letter - 1) * (int)letterSize.X, 0, (int)letterSize.X, (int)letterSize.Y), currentColor);
        }

    }

    class RawText
    {
        public enum TextEffect { NONE, SHAKE, RAINBOW, RED };

        Texture2D font;
        Vector2 letterSize;
        Vector2 location;
        TextLetter[] letters;

        public RawText(ContentManager Content, string fontName, string t, Vector2 loc)
        {
            font = Content.Load<Texture2D>("Fonts/" + fontName + ".png");
            StreamReader fontMeta = new StreamReader("Content/Fonts/" + fontName + ".png_meta");
            letterSize = new Vector2(int.Parse(fontMeta.ReadLine()), int.Parse(fontMeta.ReadLine()));
            fontMeta.Close();
            location = loc;
            letters = new TextLetter[t.Length];
            char[] ls = t.ToCharArray();
            for (int x = 0; x < t.Length; x++)
            {
                letters[x] = new TextLetter(ls[x], letterSize, null);
            }
        }

        public void changeText(string newText)
        {
            letters = new TextLetter[newText.Length];
            char[] ls = newText.ToCharArray();
            for (int x = 0; x < newText.Length; x++)
            {
                letters[x] = new TextLetter(ls[x], letterSize, null);
            }
        }

        public void setColor(Color c)
        {
            foreach (TextLetter tl in letters)
            {
                tl.setColor(c);
            }
        }

        public void draw(SpriteBatch spriteBatch)
        {
            Vector2 loc = new Vector2(location.X, location.Y);
            foreach (TextLetter tl in letters)
            {
                tl.draw(spriteBatch, loc, font);
                loc.X += letterSize.X;
            }
        }


    }
}
