using UnityEngine;
using UnityEngine.Tilemaps;
using System;

//================================//
public enum TileType
{
    Floor = 0,
    WallHorizontal = 1,
    WallVertical = 2,
    Obstacle = 3,
    Box = 4,
    Goal = 5,
    Stop = 6,
}

//================================//
public abstract class LevelTemplate : MonoBehaviour
{
    public const int Rows = 15;
    public const int Cols = 25;

    protected TileType[,] grid = new TileType[Rows, Cols];

    public TileType[,] Grid => grid;

    //================================//
    public void Init()
    {
        BuildGrid();
    }

    //================================//
    protected abstract void BuildGrid();
    public abstract Tuple<int, int> GetPlayerStartPosition(); 
    public abstract int GetPlayerStartDirection();

    //================================//
    protected void Set(int row, int col, TileType type)
    {
        grid[row, col] = type;
    }

    //================================//
    protected void FillRow(int row, TileType type)
    {
        for (int c = 0; c < Cols; c++)
            grid[row, c] = type;
    }

    //================================//
    protected void FillAll(TileType type)
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                grid[r, c] = type;
    }
}