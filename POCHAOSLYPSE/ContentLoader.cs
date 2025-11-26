using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace POCHAOSLYPSE
{
    public class ContentLoader
    {
        private static ContentLoader instance;
        public static ContentLoader Instance
        {
            get
            {
                if (instance == null) throw new System.InvalidOperationException("ContentLoader has not been initialized");
                return instance;
            }
        }
        private static readonly object sync = new();
        public static void Initialize(GraphicsDeviceManager device)
        {
            if (instance != null) return;
            lock(sync)
            {
                if (instance == null)
                    instance = new ContentLoader(device);
            }
        }
        private GraphicsDeviceManager graphics;
        private ContentLoader(GraphicsDeviceManager device)
        {
            this.graphics = device;
        }
        public Texture2D LoadImage(string path)
        {
            return Texture2D.FromFile(
                graphics.GraphicsDevice,
                GetExecutingDir(path),
                DefaultColorProcessors.PremultiplyAlpha
            );
        }
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
    }
}