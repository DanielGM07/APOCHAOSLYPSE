using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace POCHAOSLYPSE;
public class TileMap
{
    public Dictionary<Point, Block> blocks {get;} = new();
    public bool isCollidable {get;}
    public bool canDraw {get;}
    private int scaleTexture = 16;
    private int tileTexture = 16;
    private int numberOfTilesPerRow = 13;


    public TileMap(bool isCollidable, bool canDraw)
    {
        this.isCollidable = isCollidable;
        this.canDraw = canDraw;
    }

    public void GetBlocks(string filePathj)
    {
        string filePath = ContentLoader.GetExecutingDir(filePathj);
        StreamReader reader = new(filePath);
        string line;
        int y = 0;
        while((line = reader.ReadLine()) != null)
        {
            string[] parts = line.Split(',');
            for(int x = 0; x < parts.Length; x++)
            {
                if(int.TryParse(parts[x], out int block))
                {
                    if(block < 0) continue;
                    Point point = new(x,y);
                    blocks[point] = new Collisionblock(
                        GetDestRect(point),
                        GetSrcRect(block)
                    );
                }
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

        return new(
            point,
            new(tileTexture, tileTexture)
        );
    }

    private Rectangle GetDestRect(Point point)
    {
        return new(
            point.X * scaleTexture,
            point.Y * scaleTexture,
            scaleTexture,
            scaleTexture
        );
    }
    public void CheckCollisionHorizontal(Sprite entity)
    {
        
    }
    public void CheckCollisionVertical(Sprite entity)
    {
    }
    public void Draw(Texture2D texture, GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach(var block in blocks.Values)
        {
          //Console.WriteLine($"srcRect: {block.srcRectangle} / destRect: {block.collider}");
            spriteBatch.Draw(texture, block.collider, block.srcRectangle, Color.White);
        }
    }
}
