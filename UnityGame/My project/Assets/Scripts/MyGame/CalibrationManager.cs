using System;
using UnityEngine;

//================================//
class CalibrationManager: MonoBehaviour
{
    private bool runningCalibration = false;

    public int totalCalibrations = 10;
    private int rightHandCalibrations = 5;
    private int leftHandCalibrations = 5;

    private float showTimer = 0f;
    private float hideTimer = 0f;

    private float startTimer = 4f;

    [SerializeField ]
    private float showDuration = 2f;
    [SerializeField ]
    private float hideDuration = 1f;

    [SerializeField]
    public GameObject leftHandVisuals;
    [SerializeField]
    public GameObject rightHandVisuals;

    [SerializeField]
    public GameObject calibrationMenu;
    [SerializeField]
    public GameObject Title;

    int currentCalibration = -1;

    //================================//
    public void StartCalibration()
    {
        runningCalibration = true;

        rightHandCalibrations = 10;
        leftHandCalibrations = 10;

        calibrationMenu.SetActive(true);
        Title.SetActive(true);

        leftHandVisuals.SetActive(false);
        rightHandVisuals.SetActive(false);

        startTimer = 4f;
    }

    //================================//
    public void StopCalibration()
    {
        if (!runningCalibration) return;

        runningCalibration = false;
        currentCalibration = -1;

        calibrationMenu.SetActive(false);
        leftHandVisuals.SetActive(false);
        rightHandVisuals.SetActive(false);

        hideTimer = 0f;
        showTimer = 0f;
    }

    //================================//
    public void Update()
    {
        if (!runningCalibration) return;

        if (startTimer > 0f)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0f)
            {
                Title.SetActive(false);
                hideTimer = hideDuration;
            }
            return;
        }

        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0f)
            {
                if (currentCalibration == 0)
                {
                    OnHideLeftHand();
                }
                else if (currentCalibration == 1)
                {
                    OnHideRightHand();
                }
                hideTimer = hideDuration;
            }
        }

        else if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                // Choose at random left or right
                bool showRight = UnityEngine.Random.value > 0.5f;
                if (rightHandCalibrations > 0 && (showRight || leftHandCalibrations == 0))
                {
                    OnShowRightHand();
                    rightHandCalibrations--;
                    showTimer = showDuration;
                    currentCalibration = 1;
                }
                else if (leftHandCalibrations > 0)
                {
                    OnShowLeftHand();
                    leftHandCalibrations--;
                    showTimer = showDuration;
                    currentCalibration = 0;
                }
                else
                {
                    GameManager.instance.ReturnToMainMenu();
                }
            }
        }
    }

    //================================//
    public void OnShowLeftHand()
    {
        leftHandVisuals.SetActive(true);
        PythonManager.instance.StartLeftHandMovement();
    }

    //================================//
    public void OnShowRightHand()
    {
        rightHandVisuals.SetActive(true);
        PythonManager.instance.StartRightHandMovement();
    }

    //================================//
    public void OnHideLeftHand()
    {
        leftHandVisuals.SetActive(false);
        PythonManager.instance.StopLeftHandMovement();
    }

    //================================//
    public void OnHideRightHand()
    {
        rightHandVisuals.SetActive(false);
        PythonManager.instance.StopRightHandMovement();
    }
}