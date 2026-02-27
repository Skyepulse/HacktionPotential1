using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

//================================//
public class LevelManager : MonoBehaviour
{
    [Header("Assign the current level (GameObject with a LevelTemplate subclass)")]
    [SerializeField] private LevelTemplate currentLevel;

    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;

    [Header("Tile Assets — index must match TileType enum order")]
    [SerializeField] private TileBase[] tileAssets;

    private bool isBuilt = false;

    //================================//
    private void LateUpdate()
    {
        if (isBuilt)
            return;

        if (currentLevel == null)
        {
            UnityEngine.Debug.LogError("LevelManager: No level assigned!");
            isBuilt = true;
            return;
        }

        UnityEngine.Debug.Log("Initializing level: " + currentLevel.name);
        currentLevel.Init();
        BuildTilemap();
        isBuilt = true;
        UnityEngine.Debug.Log("Level built successfully");
    }

    //================================//
    public void BuildTilemap()
    {
        tilemap.ClearAllTiles();

        TileType[,] grid = currentLevel.Grid;

        Vector3Int[] positions = new Vector3Int[LevelTemplate.Rows * LevelTemplate.Cols];
        TileBase[] tiles = new TileBase[positions.Length];

        int i = 0;
        for (int r = 0; r < LevelTemplate.Rows; r++)
        {
            for (int c = 0; c < LevelTemplate.Cols; c++)
            {
                TileType type = grid[r, c];
                int index = (int)type;

                positions[i] = new Vector3Int(c, LevelTemplate.Rows - 1 - r, 0);
                tiles[i] = (index >= 0 && index < tileAssets.Length) ? tileAssets[index] : null;
                i++;
            }
        }

        tilemap.SetTiles(positions, tiles);
        UnityEngine.Debug.Log($"Tilemap bounds: {tilemap.cellBounds}, tile count: {tilemap.GetTilesRangeCount(tilemap.cellBounds.min, tilemap.cellBounds.max)}");
    }
}