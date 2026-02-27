using System;
using UnityEngine;

//================================//
class Level1: LevelTemplate
{
    //================================//
    protected override void BuildGrid()
    {
        FillAll(TileType.WallHorizontal);

        for (int r = 1; r < 10; r++)
            Set(r, 9, TileType.Floor);

        for (int c = 1; c < 9; c++)
            Set(9, c, TileType.Floor);

        Set(9, 0, TileType.Goal);
    }

    //================================//
    public override Tuple<int, int> GetPlayerStartPosition()
    {
        return Tuple.Create(1, 9);
    }

    //================================//
    public override int GetPlayerStartDirection()
    {
        return (int)Direction.Down;
    }
}