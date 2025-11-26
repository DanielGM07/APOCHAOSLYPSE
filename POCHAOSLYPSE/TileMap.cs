using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace POCHAOSLYPSE;
public class TileMap
{
    public Dictionary<Point, Block> blocks {get;} = new(); //TODO: add Block claas
    public bool isCollidable {get;}
    public bool canDraw {get;}
    private int scaleTexture = 16;
    private int tileTexture = 16;
    private int numberOfTilesPerRow = 0;
    public TileMap(bool isCollidable, bool canDraw)
    {
        this.isCollidable = isCollidable;
        this.canDraw = canDraw;
    }
    public void GetBlocks(string filePathj)
    {
        string filePath = ContentLoader.GetExecutingDir(filePathj);
        numberOfTilesPerRow = File.ReadAllLines(filePath).Length; // bastante lento e ineficiente, se lee el archivo dos veces
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
                        GetDestRect(point, x),
                        GetSrcRect(x)
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
        Console.WriteLine($"point: {point} / {numberOfTilesPerRow}");

        return new(
            point,
            new(tileTexture, tileTexture)
        );
    }

    private Rectangle GetDestRect(Point point, int block)
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
        // TODO: make the collision check here
    }
    public void CheckCollisionVertical(Sprite entity)
    {
        // TODO: make the collision check here
    }
    public void Draw(Texture2D texture, GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach(var block in blocks.Values)
        {
            spriteBatch.Draw(texture, block.collider, block.srcRectangle, Color.White);
        }
    }
}