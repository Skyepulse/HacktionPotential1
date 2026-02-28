using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

//================================//
enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

//================================//
public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public LevelTemplate currentLevel;

    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;

    [Header("Tile Assets — index must match TileType enum order")]
    [SerializeField] private TileBase[] tileAssets;

    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    public Camera mainCamera;
    public float TileSize = 1f;
    private bool isBuilt = false;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private float resizeTimer = -1f;
    private const float ResizeDelay = 0.3f;
    [HideInInspector]
    public float scaleX = 1f;
    [HideInInspector]
    public float scaleY = 1f;

    //================================//
    private Tuple<int, int> playerPosition;
    private GameObject playerInstance;
    private Direction playerDirection = Direction.Up;
    private bool isMoving = false;
    public bool IsMoving => isMoving;

    private Tuple<int, int> moveStartPos;
    private Tuple<int, int> moveTargetPos;

    public float moveSpeed = 7f; // Tiles per second
    [HideInInspector]
    public float moveStartTime;

    Dictionary<Tuple<int, int>, TileType> hiddenTiles = new Dictionary<Tuple<int, int>, TileType>();

    private bool willWin = false;

    //================================//
    TileType GetHiddenTile(int r, int c)
    {
        Tuple<int, int> pos = Tuple.Create(r, c);
        return hiddenTiles.ContainsKey(pos) ? hiddenTiles[pos] : TileType.Floor;
    }

    //================================//
    bool CanPushBox(int boxR, int boxC, int dr, int dc)
    {
        int destR = boxR + dr;
        int destC = boxC + dc;

        if (destR < 0 || destR >= LevelTemplate.Rows || destC < 0 || destC >= LevelTemplate.Cols)
            return false;

        TileType destTile = currentLevel.Grid[destR, destC];

        if (destTile == TileType.WallHorizontal || destTile == TileType.WallVertical
            || destTile == TileType.Obstacle || destTile == TileType.Box)
            return false;

        return true;
    }

    //================================//
    void PushBox(int boxR, int boxC, int dr, int dc)
    {
        int destR = boxR + dr;
        int destC = boxC + dc;

        Tuple<int, int> oldPos = Tuple.Create(boxR, boxC);
        Tuple<int, int> newPos = Tuple.Create(destR, destC);

        TileType restoredTile = GetHiddenTile(boxR, boxC);
        currentLevel.Grid[boxR, boxC] = restoredTile;
        hiddenTiles.Remove(oldPos);

        TileType destOriginal = currentLevel.Grid[destR, destC];
        hiddenTiles[newPos] = destOriginal;

        currentLevel.Grid[destR, destC] = TileType.Box;

        Vector3Int oldVis = new Vector3Int(boxC, LevelTemplate.Rows - 1 - boxR, 0);
        Vector3Int newVis = new Vector3Int(destC, LevelTemplate.Rows - 1 - destR, 0);
        tilemap.SetTile(oldVis, tileAssets[(int)restoredTile]);
        tilemap.SetTile(newVis, tileAssets[(int)TileType.Box]);
    }

    //================================//
    private void LateUpdate()
    {
        if (!isBuilt)
        {
            if (currentLevel == null)
            {
                return;
            }

            playerInstance = Instantiate(playerPrefab);

            currentLevel.Init();
            BuildTilemap();
            SetupCamera();
            lastScreenHeight = Screen.height;
            lastScreenWidth = Screen.width;
            isBuilt = true;
        }

        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            resizeTimer = ResizeDelay;
        }

        if (resizeTimer > 0f)
        {
            resizeTimer -= Time.unscaledDeltaTime;
            if (resizeTimer <= 0f)
            {
                SetupCamera();
            }
        }
    }

    //================================//
    private void Update()
    {
        if (isMoving)
        {
            int numMoveTiles = Mathf.Max(Mathf.Abs(moveTargetPos.Item1 - moveStartPos.Item1), Mathf.Abs(moveTargetPos.Item2 - moveStartPos.Item2));
            float totalMoveTime = numMoveTiles / moveSpeed;
            float t = (Time.time - moveStartTime) / totalMoveTime;

            Tuple<float, float> startWorldPos = GetTileWorldPosition(moveStartPos);
            Tuple<float, float> targetWorldPos = GetTileWorldPosition(moveTargetPos);

            float newX = Mathf.Lerp(startWorldPos.Item1, targetWorldPos.Item1, t);
            float newY = Mathf.Lerp(startWorldPos.Item2, targetWorldPos.Item2, t);

            playerInstance.transform.position = new Vector3(newX, newY, playerInstance.transform.position.z);

            if (t >= 1f)
            {
                playerInstance.transform.position = new Vector3(targetWorldPos.Item1, targetWorldPos.Item2, playerInstance.transform.position.z);
                isMoving = false;

                if (willWin)
                {
                    willWin = false;
                    GameManager.OnLevelComplete();
                }
            }
        }
    }

    //================================//
    public void BuildTilemap()
    {
        tilemap.ClearAllTiles();
        hiddenTiles.Clear();

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
        playerPosition = currentLevel.GetPlayerStartPosition();
        playerDirection = (Direction)currentLevel.GetPlayerStartDirection();

        Tuple<float, float> playerWorldPos = GetTileWorldPosition(playerPosition);
        playerInstance.transform.position = new Vector3(playerWorldPos.Item1, playerWorldPos.Item2, playerInstance.transform.position.z);
        playerInstance.transform.rotation = Quaternion.Euler(0, 0, -90f * (int)playerDirection);
    }

    //================================//
    private void SetupCamera()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                UnityEngine.Debug.LogError("LevelManager: No camera found!");
                return;
            }
        }

        float gridLocalWidth  = LevelTemplate.Cols * TileSize;
        float gridLocalHeight = LevelTemplate.Rows * TileSize;

        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth  = camHeight * mainCamera.aspect;

        scaleX = camWidth / gridLocalWidth;
        scaleY = camHeight / gridLocalHeight;

        Transform gridParent = tilemap.transform.parent;
        if (gridParent != null)
        {
            gridParent.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        else
        {
            tilemap.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        float camLeft   = mainCamera.transform.position.x - camWidth / 2f;
        float camBottom = mainCamera.transform.position.y - camHeight / 2f;
        float camRight  = camLeft + camWidth;

        float gridOriginX = camLeft;
        float gridOriginY = camBottom;  

        if (gridParent != null)
            gridParent.position = new Vector3(gridOriginX, gridOriginY, 0f);
        else
            tilemap.transform.position = new Vector3(gridOriginX, gridOriginY, 0f);

        if (playerInstance != null && playerPosition != null)
        {
            Tuple<float, float> playerWorldPos = GetTileWorldPosition(playerPosition);
            playerInstance.transform.position = new Vector3(playerWorldPos.Item1, playerWorldPos.Item2, playerInstance.transform.position.z);
        }

        GameManager.SetupCamera(new Vector3(camRight, camBottom, 0f));
    }

    //================================//
    public Tuple<float, float> GetTileWorldPosition(Tuple<int, int> tilePos)
    {
        int r = tilePos.Item1;
        int c = tilePos.Item2;

        float localX = (c + 0.5f) * TileSize;
        float localY = (LevelTemplate.Rows - 1 - r + 0.5f) * TileSize;

        Transform gridParent = tilemap.transform.parent;
        Vector3 origin = (gridParent != null) ? gridParent.position : tilemap.transform.position;

        float worldX = origin.x + localX * scaleX;
        float worldY = origin.y + localY * scaleY;

        return Tuple.Create(worldX, worldY);
    }

    //================================//
    public void RotatePlayer()
    {
        if (isMoving)
            return;

        playerDirection = (Direction)(((int)playerDirection + 1) % 4);
        playerInstance.transform.rotation = Quaternion.Euler(0, 0, -90f * (int)playerDirection);
    }

    //================================//
    public void Move()
    {
        if (isMoving)
            return;

        int dr = 0, dc = 0;
        switch (playerDirection)
        {
            case Direction.Up:    dr = -1; break;
            case Direction.Down:  dr = 1;  break;
            case Direction.Left:  dc = -1; break;
            case Direction.Right: dc = 1;  break;
        }

        moveStartPos = playerPosition;

        int tilesMoved = 0;
        int currentR = playerPosition.Item1;
        int currentC = playerPosition.Item2;
        int SafeBreak = 100;

        while (tilesMoved < SafeBreak)
        {
            int nextR = currentR + dr;
            int nextC = currentC + dc;

            if (nextR < 0 || nextR >= LevelTemplate.Rows || nextC < 0 || nextC >= LevelTemplate.Cols)
            {
                throw new Exception("Player moved out of bounds! This should never happen if levels are designed correctly.");
            }

            TileType nextTile = currentLevel.Grid[nextR, nextC];

            if (nextTile == TileType.WallHorizontal || nextTile == TileType.WallVertical || nextTile == TileType.Obstacle)
                break;

            if (nextTile == TileType.Box)
            {
                if (tilesMoved != 0)
                    break;

                if (!CanPushBox(nextR, nextC, dr, dc))
                {
                    UnityEngine.Debug.Log("Cannot push box — destination blocked!");
                    break;
                }

                PushBox(nextR, nextC, dr, dc);

                currentR = nextR;
                currentC = nextC;
                tilesMoved++;

                break;
            }

            currentR = nextR;
            currentC = nextC;
            tilesMoved++;

            if (nextTile == TileType.Stop)
                break;

            if (nextTile == TileType.Goal)
            {
                willWin = true;
                break;
            }
        }

        if (tilesMoved == 0)
        {
            UnityEngine.Debug.Log("Cannot move");
            return;
        }

        moveTargetPos = Tuple.Create(currentR, currentC);
        playerPosition = moveTargetPos;
        moveStartTime = Time.time;
        isMoving = true;

        UnityEngine.Debug.Log($"Moving from ({moveStartPos.Item1}, {moveStartPos.Item2}) to ({moveTargetPos.Item1}, {moveTargetPos.Item2}) over {tilesMoved} tiles.");
    }

    //================================//
    public void Cleanup()
    {
        isMoving = false;
        playerDirection = Direction.Up;
        hiddenTiles.Clear();
        if (playerInstance)
            Destroy(playerInstance);

        tilemap.ClearAllTiles();
    }

    //================================//
    public void ChangeLevel(LevelTemplate newLevel)
    {
        Cleanup();
        currentLevel = newLevel;
        isBuilt = false;
    }

    //================================//
    public void Restart()
    {
        if (!GameManager.instance.InMainMenu)
        {
            ChangeLevel(currentLevel);
        }
    }
}