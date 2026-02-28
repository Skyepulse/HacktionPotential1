using System;
using UnityEngine;

//================================//
class Level4: LevelTemplate
{
    //================================//
    protected override void BuildGrid()
    {
        FillAll(TileType.Floor);

        // === Border walls ===
        for (int r = 0; r < Rows; r++)
            Set(r, 0, TileType.WallVertical);

        for (int r = 0; r < Rows; r++)
            Set(r, Cols - 1, TileType.WallVertical);

        for (int c = 0; c < Cols; c++)
            Set(0, c, TileType.WallHorizontal);
        
        for (int c = 0; c < Cols; c++)
            Set(Rows - 1, c, TileType.WallHorizontal);

        // === No-go zone: bottom-right block (rows 10-14, cols 15-24) ===
        for (int r = 10; r < 15; r++)
            for (int c = 15; c < 25; c++)
                Set(r, c, TileType.WallHorizontal);

        for (int r = 10; r < 15; r++)
            Set(r, 14, TileType.WallVertical);

        Set(4, 5, TileType.Box); 
        Set(6, 5, TileType.Box); 
        Set(5, 4, TileType.Obstacle);
        Set(5, 6, TileType.Obstacle);

        Set(1, 20, TileType.Stop);

        Set(3, 20, TileType.Box);

        Set(10, 20, TileType.Goal);

        Set(3, 3, TileType.Obstacle);
        Set(2, 10, TileType.Obstacle);
        Set(7, 13, TileType.Obstacle);
        Set(4, 17, TileType.Obstacle);
        Set(8, 3, TileType.Obstacle);
        Set(9, 9, TileType.Obstacle);
    }

    //================================//
    public override Tuple<int, int> GetPlayerStartPosition()
    {
        return Tuple.Create(5, 5);
    }

    //================================//
    public override int GetPlayerStartDirection()
    {
        return (int)Direction.Right;
    }
}