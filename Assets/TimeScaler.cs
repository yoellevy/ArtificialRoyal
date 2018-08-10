using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaler : MonoBehaviour {

    [SerializeField]
    private float timeScale = 10;

    [SerializeField]
    private float maxTimeScale = 20;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Text timeScaleText;

    // Use this for initialization
    void Start () {
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        SetTimeScale((int)timeScale);
    }

    // Invoked when the value of the slider changes.
    public void ValueChangeCheck()
    {
        int newValue = (int)(1 + (slider.value * (maxTimeScale - 1) + 0.5f));
        SetTimeScale(newValue);
    }

    private void SetTimeScale(int num)
    {
        lock (this)
        {
            float sliderCorrection = ((float)num - 1) / (maxTimeScale - 1);
            slider.value = sliderCorrection;
            timeScale = num;
            Time.timeScale = timeScale;
            timeScaleText.text = num.ToString();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
    }
}
