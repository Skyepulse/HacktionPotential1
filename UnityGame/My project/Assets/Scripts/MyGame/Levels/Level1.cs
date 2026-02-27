//================================//
class Level1: LevelTemplate
{
    protected override void BuildGrid()
    {
        FillAll(TileType.Floor);

        // Walls around the perimeter
        FillRow(0, TileType.Wall);
        FillRow(Rows - 1, TileType.Wall);
        for (int r = 0; r < Rows; r++)
        {
            Set(r, 0, TileType.Wall);
            Set(r, Cols - 1, TileType.Wall);
        }

        // Some internal obstacles at random
        Set(2, 3, TileType.Obstacle);
        Set(4, 5, TileType.Obstacle);

        // Goal at the bottom right corner
        Set(Rows - 2, Cols - 2, TileType.Goal);

        // A stop somewhere
        Set(5, 2, TileType.Stop);
    }
}