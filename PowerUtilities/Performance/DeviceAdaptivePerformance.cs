using Jint;
using Jint.Runtime.Interop;
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.UI;

public class DeviceAdaptivePerformance : MonoBehaviour
{
    public Slider uiTemperatureLevel;
    public Text uiFpsText;

    public float temperatureLevel;
    public float curFrameTime;

    IAdaptivePerformance adapter;
    UnityEngine.AdaptivePerformance.FrameTiming frameTiming;

    // Start is called before the first frame update
    void Start()
    {
        adapter = Holder.Instance;

        if (adapter == null)
            return;

        adapter.ThermalStatus.ThermalEvent -= ThermalStatus_ThermalEvent;
        adapter.ThermalStatus.ThermalEvent += ThermalStatus_ThermalEvent;

        adapter.PerformanceStatus.PerformanceBottleneckChangeEvent -= PerformanceStatus_PerformanceBottleneckChangeEvent;
        adapter.PerformanceStatus.PerformanceBottleneckChangeEvent += PerformanceStatus_PerformanceBottleneckChangeEvent;
    }

    private void PerformanceStatus_PerformanceBottleneckChangeEvent(PerformanceBottleneckChangeEventArgs e)
    {
        
    }

    private void Update()
    {
        if (!adapter.Active)
            return;

        temperatureLevel = adapter.ThermalStatus.ThermalMetrics.TemperatureLevel;
        frameTiming = adapter.PerformanceStatus.FrameTiming;

        curFrameTime = frameTiming.CurrentFrameTime;

        if (uiTemperatureLevel != null)
        {
            uiTemperatureLevel.value = temperatureLevel;
        }

        if(uiFpsText != null)
        {
            uiFpsText.text = (1f/curFrameTime).ToString();
        }
    }

    private void ThermalStatus_ThermalEvent(ThermalMetrics ev)
    {
        temperatureLevel = ev.TemperatureLevel;
    }

}
