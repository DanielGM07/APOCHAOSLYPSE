using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public enum EnemyKind
    {
        Light,   // id 10
        Medium,  // id 11
        Heavy    // id 12
    }

    public class EnemySpawn
    {
        public EnemyKind Kind;
        public Vector2   Position;

        public EnemySpawn(EnemyKind kind, Vector2 position)
        {
            Kind     = kind;
            Position = position;
        }
    }

    public class TileMap
    {
        public List<Block>     blocks      { get; } = new();
        public List<EnemySpawn> EnemySpawns { get; } = new();

        public bool isCollidable { get; }
        public bool canDraw      { get; }

        private int scaleTexture       = 16;
        private int tileTexture        = 16;
        private int numberOfTilesPerRow = 13;

        public TileMap(bool isCollidable, bool canDraw)
        {
            this.isCollidable = isCollidable;
            this.canDraw      = canDraw;
        }

        public void GetBlocks(string filePathj)
        {
            string filePath = ContentLoader.GetExecutingDir(filePathj);
            using StreamReader reader = new(filePath);

            string line;
            int y = 0;

            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');

                for (int x = 0; x < parts.Length; x++)
                {
                    if (!int.TryParse(parts[x], out int blockId))
                        continue;

                    if (blockId < 0)
                        continue;

                    Point     point = new(x, y);
                    Rectangle dest  = GetDestRect(point);
                    Rectangle src   = GetSrcRect(blockId);

                    // 🔹 IDs especiales 10/11/12 → spawns de enemigos, NO bloques
                    if (blockId == 10 || blockId == 11 || blockId == 12)
                    {
                        EnemyKind kind = EnemyKind.Light;
                        if (blockId == 11) kind = EnemyKind.Medium;
                        if (blockId == 12) kind = EnemyKind.Heavy;

                        // Centro del tile como posición del spawn
                        EnemySpawns.Add(new EnemySpawn(kind, dest.Center.ToVector2()));
                        // No agregamos Block, este tile solo sirve para spawnear
                        continue;
                    }

                    Block block = null;

                    switch (blockId)
                    {
                        case 14:
                            // Bloque sólido
                            block = new Collisionblock(dest, src);
                            break;

                        case 112:
                            // Plataforma one-way
                            block = new OneWayBlock(dest, src);
                            break;

                        default:
                            // Decorativo, sin colisión
                            block = new DecoBlock(dest, src);
                            break;
                    }
                    if (block != null)
                        blocks.Add(block);
                }
                y++;
            }
        }

        private Rectangle GetSrcRect(int block)
        {
            Point point = new(
                (block % numberOfTilesPerRow) * tileTexture,
                (block / numberOfTilesPerRow) * tileTexture
            );

            return new Rectangle(
                point,
                new Point(tileTexture, tileTexture)
            );
        }

        private Rectangle GetDestRect(Point point)
        {
            return new Rectangle(
                point.X * scaleTexture,
                point.Y * scaleTexture,
                scaleTexture,
                scaleTexture
            );
        }

        public void CheckCollisionHorizontal(Entity entity)
        {
            foreach (var block in blocks)
            {
                if (block.collider.Intersects(entity.destinationRectangle))
                {
                    block.horizontalAction(entity);
                }
            }
        }

        public void CheckCollisionVertical(Entity entity)
        {
            foreach (var block in blocks)
            {
                if (block.collider.Intersects(entity.destinationRectangle))
                {
                    block.verticalActions(entity);
                }
            }
        }

        public void Draw(Texture2D texture, GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!canDraw && false) // de momento ignoramos canDraw
                return;

            foreach (var block in blocks)
            {
                spriteBatch.Draw(texture, block.collider, block.srcRectangle, Color.White);
            }
        }

        public void HandleProjectileCollisions(List<Projectile> projectiles, List<Explosion> explosions)
        {
            if (projectiles == null || projectiles.Count == 0)
                return;

            for (int i = 0; i < projectiles.Count; i++)
            {
                var p = projectiles[i];
                if (!p.IsAlive)
                    continue;

                foreach (var block in blocks)
                {
                    if (block.collider.Intersects(p.recPosition))
                    {
                        if (p.IsExplosive && explosions != null)
                        {
                            explosions.Add(new Explosion(
                                p.recPosition.Location.ToVector2(),
                                p.ExplosionRadius,
                                0.18f,
                                Color.Red
                            ));

                            Camera.Instance.Shake(20f, 0.2f);
                        }
                        p.Lifetime = 0f;
                        break;
                    }
                }
            }
        }
    }
}
