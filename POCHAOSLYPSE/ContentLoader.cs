using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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
        public static void Initialize(GraphicsDeviceManager device, string contentPath)
        {
            if (instance != null) return;
            lock(sync)
            {
                if (instance == null)
                    instance = new ContentLoader(device, contentPath);
            }
        }
#endregion

        private GraphicsDeviceManager graphics;

        private Dictionary<string, Texture2D> texturesByNameAndFolder = new(); // use: player = texturesByNameAndFolder["Content/Images/player"]
        private Dictionary<string, Song> songsByNameAndFolder = new(); // use: mainMenu = texturesByNameAndFolder["Content/Songs/MainMenu"]
        private Dictionary<string, SoundEffect> soundEffectsByNameAndFolder = new(); // use: playerSFX = texturesByNameAndFolder["Content/SFX/hurt"]

        private ContentLoader(GraphicsDeviceManager device, string contentPath)
        {
            this.graphics = device;
            loadEverything(contentPath);

        }
        private void loadEverything(string contentPath)
        {

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
