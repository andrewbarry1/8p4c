using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace JamSeven
{
    class NetworkInterface
    {
        private WebSocket sock;
        private ContentManager Content;

        public bool connected;

        private Game1.fadeDelegate onReady;

        GameStateFighting game;


        public NetworkInterface(string address, ContentManager c)
        {

            sock = new WebSocket(address);
            Content = c;
            connected = false;
            sock.OnMessage += (sender, e) =>
            {
                byte[] r = e.RawData;
                string s = System.Text.Encoding.UTF8.GetString(r);
                doNetworkInput(s);
            };
            sock.OnOpen += (sender, e) =>
            {
                connected = true;
                Console.WriteLine("Connected");
                onReady();
            };
            sock.OnClose += (sender, e) =>
            {
                Console.WriteLine("Ran onclose");
                connected = false;
            };
            sock.OnError += (sender, e) =>
            {
                if (e.Message == "An error has occurred while connecting.")
                {
                    connected = false;
                    onReady();
                }
            };
            sock.ConnectAsync();
        }

        public void setGSF(GameStateFighting gsf)
        {
            game = gsf;
            game.offline = !connected;
        }

        public void setOnReady(Game1.fadeDelegate del)
        {
            onReady = del;
        }

        public void sendMessage(string msg)
        {
            byte[] mData = System.Text.Encoding.UTF8.GetBytes(msg.ToCharArray());
            sock.SendAsync(mData, null);
        }

        private void doNetworkInput(string message)
        {
            if (message == "1") // shake the camera
            {
                game.shakeAmnt += new Vector2(.1f, .1f);
            }
            else if (message == "2") // increase field power
            {
                game.fieldMultiplier += 0.5f;
            }
            else if (message == "3") // zoom out
            {
                game.camera.adjustZoom(-0.01f);
            }
            else if (message == "4") // lower the platform
            {
                game.platform.move(new Vector2(0f, 3f));
            }
            else if (message == "5") // invert controls
            {
                game.invertControls = !game.invertControls;
            }
            else if (message == "6") // rotate camera
            {
                game.camera.adjustRotation(.1f);
            }
            else if (message == "7") // create force
            {
                game.cfAmount += .05f;
            }
            else if (message == "8") // create explosions
            {
                game.createTempExplosion();
            }
        }


        
    }
}
