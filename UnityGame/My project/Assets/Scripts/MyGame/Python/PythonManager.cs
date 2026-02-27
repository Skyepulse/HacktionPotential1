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
    static PythonManager instance;

    //================================//
    private String m_pythonPath;
    private Process m_mainInferenceProcess;

    //================================//
    private StreamInlet inlet;
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

        UnityEngine.Debug.Log("PythonManager initialized with Python path: " + m_pythonPath);
        StartInferenceProcess();
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
    private void Clean()
    {
        if (m_mainInferenceProcess != null && !m_mainInferenceProcess.HasExited)
            m_mainInferenceProcess.Kill();

        if (inlet != null)
        {
            inlet.close_stream();
            inlet = null;
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

        UnityEngine.Debug.Log("LSL stream connected.");
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
}