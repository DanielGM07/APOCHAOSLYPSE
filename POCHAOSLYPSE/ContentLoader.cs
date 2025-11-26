using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace POCHAOSLYPSE
{
    public class ContentLoader
    {
#region Singleton
        private static ContentLoader instance;
        public static ContentLoader Instance
        {
            get
            {
                if (instance == null)
                  {throw new System.InvalidOperationException("ContentLoader has not been initialized");}
                return instance;
            }
        }
        private static readonly object sync = new();
        public static void Initialize(GraphicsDeviceManager device, ContentManager content, string contentPath)
        {
            if (instance != null) return;
            lock(sync)
            {
                if (instance == null)
                    instance = new ContentLoader(device, content, contentPath);
            }
        }
#endregion

        public GraphicsDeviceManager graphics {get;}

        public Dictionary<string, Texture2D> texturesByNameAndFolder = new(); // use: player = texturesByNameAndFolder["Content/Images/player"]
        public Dictionary<string, Song> songsByNameAndFolder = new(); // use: mainMenu = texturesByNameAndFolder["Content/Songs/MainMenu"]
        public Dictionary<string, SoundEffect> soundEffectsByNameAndFolder = new(); // use: playerSFX = texturesByNameAndFolder["Content/SFX/hurt"]
        public SpriteFont font;

        private ContentLoader(GraphicsDeviceManager device, ContentManager content, string contentPath)
        {
            this.graphics = device;
            font = content.Load<SpriteFont>("font");
            loadEverything(contentPath);
        }
        private void loadEverything(string contentPath)
        {
          //TODO: make it load what you need, idk
        }

        public Texture2D LoadImage(string path)
        {
            return Texture2D.FromFile(
                graphics.GraphicsDevice,
                GetExecutingDir(path),
                DefaultColorProcessors.PremultiplyAlpha
            );
        }

#region Utils
        private static string GetExecutingDir(string v)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dirInfo = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 3 && dirInfo.Parent != null; i++)
            {
                dirInfo = dirInfo.Parent;
            }
            baseDirectory = dirInfo.FullName;
            return Path.Combine(baseDirectory, v);
        }
#endregion
    }
}
