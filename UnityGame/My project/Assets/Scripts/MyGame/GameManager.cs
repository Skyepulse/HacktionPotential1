using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Numerics;
using System;
using System.Collections.Generic;

//================================//
class GameManager: MonoBehaviour
{
    static GameManager instance;

    //================================//
    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private BoardVisuals boardVisuals;

    [SerializeField]
    private LevelTemplate[] levels;

    private float leverTimer = 0f;
    private float buttonTimer = 0f;

    private int currentLevelIndex = 0;

    //================================//
    void InitGame()
    {
        if (levels.Length == 0)
        {
            UnityEngine.Debug.LogError("GameManager: No levels assigned!");
            return;
        }

        levelManager.ChangeLevel(levels[currentLevelIndex]);
    }
    
    //================================//
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.Debug.Log("GameManager initialized");

        InitGame();
    }

    //================================//
    public static void OnLeft()
    {
        if (instance.levelManager.IsMoving)
            return;

        instance.boardVisuals.SetLever(true);
        instance.leverTimer = 0.1f;

        instance.levelManager.Move();
    }

    //================================//
    public static void OnRight()
    {
        if (instance.levelManager.IsMoving)
            return;

        instance.boardVisuals.SetButton(true);
        instance.buttonTimer = 0.3f;

        instance.levelManager.RotatePlayer();
    }

    //================================//
    private void Update()
    {
        if (!instance.levelManager)
            return;

        if (leverTimer > 0f)
        {
            if (instance.levelManager.IsMoving)
            {
                leverTimer = 0.1f;
            }
            else
            {
                leverTimer -= Time.deltaTime;
                if (leverTimer <= 0f)
                {
                    instance.boardVisuals.SetLever(false);
                }
            }
        }

        if (buttonTimer > 0f)
        {
            buttonTimer -= Time.deltaTime;
            if (buttonTimer <= 0f)
            {
                instance.boardVisuals.SetButton(false);
            }
        }

        // On press T
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnRight();
        }

        // On Press M
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnLeft();
        }

        // On Press R
        if (Input.GetKeyDown(KeyCode.R))
        {
            instance.levelManager.ChangeLevel(instance.levelManager.currentLevel);
        }
    }

    //================================//
    static public void SetupCamera(UnityEngine.Vector3 cameraBottomRight)
    {
        float baseScale = instance.boardVisuals.baseScale;
        float sx = instance.levelManager.scaleX * baseScale * 0.95f;
        float sy = instance.levelManager.scaleY * baseScale;
        instance.boardVisuals.transform.localScale = new UnityEngine.Vector3(sx, sy, 1f);

        UnityEngine.Vector3 offsetLocal = instance.boardVisuals.offset.localPosition;
        float pivotX = cameraBottomRight.x - offsetLocal.x * sx;
        float pivotY = cameraBottomRight.y - offsetLocal.y * sy;

        instance.boardVisuals.transform.position = new UnityEngine.Vector3(pivotX, pivotY, 0f);
    }

    //================================//
    static public void OnLevelComplete()
    {
        UnityEngine.Debug.Log("Level Complete!");
        instance.currentLevelIndex++;
        if (instance.currentLevelIndex >= instance.levels.Length)
        {
            instance.currentLevelIndex = 0;
        }
        instance.levelManager.ChangeLevel(instance.levels[instance.currentLevelIndex]);
    }
}