using System.Diagnostics;
using System.IO;
using LSL;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

//================================//
class PythonManager: MonoBehaviour
{
    public static PythonManager instance;

    //================================//
    private String m_pythonPath;
    private Process m_mainInferenceProcess;
    private Process m_calibrationProcess;

    //================================//
    private StreamInlet inlet;
    private StreamOutlet leftHandOutlet;
    private StreamOutlet rightHandOutlet;
    private int[] sample;
    private double timestamp;

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

        string[] lines = File.ReadAllLines("python_config.toml");
        foreach (var line in lines)
        {
            if (line.StartsWith("executable"))
            {
                m_pythonPath = line.Split('=')[1].Trim().Trim('"');
            }
        }
    }

    //================================//
    public void StartInferenceProcess()
    {
        Clean();

        m_mainInferenceProcess = new Process();
        m_mainInferenceProcess.StartInfo.FileName = m_pythonPath;
        m_mainInferenceProcess.StartInfo.Arguments = "inference_loop.py";
        m_mainInferenceProcess.StartInfo.WorkingDirectory = "";
        m_mainInferenceProcess.StartInfo.UseShellExecute = false;
        m_mainInferenceProcess.StartInfo.CreateNoWindow = true;
        m_mainInferenceProcess.StartInfo.RedirectStandardOutput = true;
        m_mainInferenceProcess.StartInfo.RedirectStandardError = true;

        m_mainInferenceProcess.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.Log("[PY] " + e.Data);
        };

        m_mainInferenceProcess.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            if (e.Data.StartsWith("Traceback") ||
                e.Data.Contains("Error") ||
                e.Data.Contains("Exception"))
            {
                UnityEngine.Debug.LogError("[PY] " + e.Data);
            }
            else
            {
                UnityEngine.Debug.Log("[PY] " + e.Data);
            }
        };

        m_mainInferenceProcess.Start();
        m_mainInferenceProcess.BeginOutputReadLine();
        m_mainInferenceProcess.BeginErrorReadLine();

        StartCoroutine(LookForStreams("InputStream"));
    }

    //================================//
    public void StartCalibrationProcess()
    {
       CreateOutlets();

        m_calibrationProcess = new Process();
        m_calibrationProcess.StartInfo.FileName = m_pythonPath;
        m_calibrationProcess.StartInfo.Arguments = "calibration_loop.py";
        m_calibrationProcess.StartInfo.WorkingDirectory = "";
        m_calibrationProcess.StartInfo.UseShellExecute = false;
        m_calibrationProcess.StartInfo.CreateNoWindow = true;
        m_calibrationProcess.StartInfo.RedirectStandardOutput = true;
        m_calibrationProcess.StartInfo.RedirectStandardError = true;

        m_calibrationProcess.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.Log("[PY] " + e.Data);
        };

        m_calibrationProcess.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            if (e.Data.StartsWith("Traceback") ||
                e.Data.Contains("Error") ||
                e.Data.Contains("Exception"))
            {
                UnityEngine.Debug.LogError("[PY] " + e.Data);
            }
            else
            {
                UnityEngine.Debug.Log("[PY] " + e.Data);
            }
        };

        m_calibrationProcess.Start();
        m_calibrationProcess.BeginOutputReadLine();
        m_calibrationProcess.BeginErrorReadLine(); 
    }

    //================================//
    public void Clean()
    {
        if (m_mainInferenceProcess != null && !m_mainInferenceProcess.HasExited)
            m_mainInferenceProcess.Kill();

        if (m_calibrationProcess != null && !m_calibrationProcess.HasExited)
            m_calibrationProcess.Kill();

        if (inlet != null)
        {
            inlet.close_stream();
            inlet = null;
        }

        if (leftHandOutlet != null)
        {
            leftHandOutlet = null;
        }

        if (rightHandOutlet != null)
        {
            rightHandOutlet = null;
        }
    }

    //================================//
    void OnApplicationQuit()
    {
        Clean();
    }

    //================================//
    void OnDestroy()
    {
        Clean();
    }

    //================================//
    IEnumerator LookForStreams(String streamName)
    {
        // wait for the python subprocess to be launched
        yield return new WaitForSeconds(1);

        // Resolve LSL streams of type "Markers"
        UnityEngine.Debug.Log("Looking for LSL stream...");
        StreamInfo[] results = LSL.LSL.resolve_stream("name", streamName, 1, 5.0);

        if (results.Length == 0)
        {
            UnityEngine.Debug.LogError("No LSL stream found.");
            yield break;
        }

        inlet = new StreamInlet(results[0]);
        sample = new int[1];
    }

    //================================//
    public void CreateOutlets()
    {
        StreamInfo info = new StreamInfo(
            "Left hand movement",   // stream name
            "Markers",         // stream type
            1,                 // one channel
            0,                 // irregular sampling rate
            channel_format_t.cf_int32,
            "unity_left_hand"
        );
        leftHandOutlet = new StreamOutlet(info);
        
        StreamInfo rightHandInfo = new StreamInfo(
            "Right hand movement",   // stream name
            "Markers",         // stream type
            1,                 // one channel
            0,                 // irregular sampling rate
            channel_format_t.cf_int32,
            "unity_right_hand"
        );
        rightHandOutlet = new StreamOutlet(rightHandInfo);
    }

    //================================//
    void Update()
    {
        if (inlet == null)
            return;

        timestamp = inlet.pull_sample(sample, 0.0f);

        if (timestamp != 0.0)
        {
            int value = sample[0];
            UnityEngine.Debug.Log("Received LSL message: " + value + " at time " + timestamp);

            if (value == 0)
            {
                GameManager.OnLeft();
            }
            else if (value == 1)
            {
                GameManager.OnRight();
            }
        }
    }

    //================================//
    public void StartLeftHandMovement()
    {
        if (leftHandOutlet != null)
        {
            int[] startMsg = new int[] { 1 };
            leftHandOutlet.push_sample(startMsg);
        }
    }

    //================================//
    public void StartRightHandMovement()
    {
        if (rightHandOutlet != null)
        {
            int[] startMsg = new int[] { 1 };
            rightHandOutlet.push_sample(startMsg);
        }
    }

    //================================//
    public void StopLeftHandMovement()
    {
        if (leftHandOutlet != null)
        {
            int[] stopMsg = new int[] { 0 };
            leftHandOutlet.push_sample(stopMsg);
        }
    }

    //================================//
    public void StopRightHandMovement()
    {
        if (rightHandOutlet != null)
        {
            int[] stopMsg = new int[] { 0 };
            rightHandOutlet.push_sample(stopMsg);
        }
    }
}