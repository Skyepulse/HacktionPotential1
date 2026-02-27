using System;
using UnityEngine;

//================================//
class Level3: LevelTemplate
{
    //================================//
    protected override void BuildGrid()
    {
        FillAll(TileType.WallHorizontal);

        for (int r = 1; r < 9; r++)
            for (int c = 1; c < 12; c++)
                Set(r, c, TileType.Floor);

        Set(1, 4, TileType.Obstacle);
        Set(4, 3, TileType.Stop);
        Set(4, 6, TileType.Stop);

        Set(8, 3, TileType.Obstacle);
        Set(7, 11, TileType.Obstacle);
        Set(0, 10, TileType.Goal);
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