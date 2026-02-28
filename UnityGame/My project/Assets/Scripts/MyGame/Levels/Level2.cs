using System;
using UnityEngine;

//================================//
class Level2: LevelTemplate
{
    //================================//
    protected override void BuildGrid()
    {
        FillAll(TileType.WallHorizontal);

        for (int r = 1; r < 9; r++)
            for (int c = 1; c < 12; c++)
                Set(r, c, TileType.Floor);

        Set(1, 4, TileType.Obstacle);
        Set(7, 3, TileType.Obstacle);
        Set(6, 12, TileType.Goal);

        // Some more randoms
        Set(2, 2, TileType.Obstacle);
        Set(3, 5, TileType.Obstacle);
        Set(5, 7, TileType.Obstacle);
        Set(8, 9, TileType.Obstacle);
    }

    //================================//
    public override Tuple<int, int> GetPlayerStartPosition()
    {
        return Tuple.Create(1, 1);
    }

    //================================//
    public override int GetPlayerStartDirection()
    {
        return (int)Direction.Right;
    }
}