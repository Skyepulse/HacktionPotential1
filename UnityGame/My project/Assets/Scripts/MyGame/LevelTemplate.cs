using UnityEngine;

//================================//
public enum TileType
{
    Floor = 0,
    Wall = 1,
    Obstacle = 2,
    Goal = 3,
    Stop = 4,
}

//================================//
public abstract class LevelTemplate : MonoBehaviour
{
    public const int Rows = 10;
    public const int Cols = 15;

    protected TileType[,] grid = new TileType[Rows, Cols];

    public TileType[,] Grid => grid;

    //================================//
    public void Init()
    {
        BuildGrid();
    }

    //================================//
    protected abstract void BuildGrid();

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