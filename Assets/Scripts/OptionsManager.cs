using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class OptionsManager : Singleton<OptionsManager>
{
    public class Option
    {
        private string key;
        private int value;
        private int defaultValue;

        private Action<int> onValueChange;

        public int Value { get { return value; } }
        public bool BoolValue { get { return value > 0; } }

        public Option(string key, int defaultValue, Action<int> onValueChange)
        {
            this.key = key;
            value = this.defaultValue = defaultValue;
            this.onValueChange = onValueChange;
            LoadFromPlayerPrefs();
            Instance.allOptions.Add(this);
        }

        public void SetValue(int value, bool save)
        {
            this.value = value;
            onValueChange(value);
            if (save) SaveToPlayerPrefs();
        }

        public void ResetToDefault()
        {
            SetValue(defaultValue, true);
        }

        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey(key))
            {
                SetValue(PlayerPrefs.GetInt(key), false);
            }
            else
            {
                SetValue(defaultValue, false);
            }
        }
    }

    public Option resolution;
    public Option fullscreenMode;
    public Option highQuality;

    public Option masterVolume;
    public Option musicVolume;
    public Option sfxVolume;
    public Option tannoyVolume;

    public Option edgeScrolling;
    public Option cameraPanSpeed;
    public Option cameraRotSpeed;
    public Option orthographicCamera;

    public Option colourblindMode;
    public Option hudEnabled;
    public Option hudScale;
    public Option tannoySubtitles;

    // Default windowed resolution
    [SerializeField] private int screenInitX, screenInitY;

    private List<Option> allOptions;

    protected override void Awake()
    {
        base.Awake();

        allOptions = new();
    }

    void Start()
    {
        resolution = new Option("resolution", 0, onChangeResolution);
        fullscreenMode = new Option("fullscreenMode", 0, onChangeFullscreenMode);
        highQuality = new Option("highQuality", 1, onChangeHighQuality);

        masterVolume = new Option("masterVolume", 8, onChangeMasterVolume);
        musicVolume = new Option("musicVolume", 8, onChangeMusicVolume);
        sfxVolume = new Option("sfxVolume", 8, onChangeSfxVolume);
        tannoyVolume = new Option("tannoyVolume", 8, onChangeTannoyVolume);

        edgeScrolling = new Option("edgeScrolling", 1, onChangeEdgeScrolling);
        cameraPanSpeed = new Option("cameraPanSpeed", 6, onChangeCameraPanSpeed);
        cameraRotSpeed = new Option("cameraRotSpeed", 6, onChangeCameraRotSpeed);
        orthographicCamera = new Option("orthographicCamera", 1, onChangeOrthographicCamera);

        colourblindMode = new Option("colourblindMode", 0, onChangeColourblindMode);
        hudEnabled = new Option("hudEnabled", 1, onChangeHudEnabled);
        hudScale = new Option("hudScale", 7, onChangeHudScale);
        tannoySubtitles = new Option("tannoySubtitles", 0, onChangeTannoySubtitles);
    }

    public void ResetAllToDefault()
    {
        foreach(Option option in allOptions)
        {
            option.ResetToDefault();
        }
    }

    private void onChangeResolution(int value)
    {
        Resolution res = Screen.resolutions.Reverse().ToArray()[value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    private void onChangeFullscreenMode(int value)
    {
        switch (value)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // Borderless
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    private void onChangeHighQuality(int value)
    {
        if (CameraController.Instance)
        {
            CameraController.Instance.SetPostProcessingComponentActive<Bloom>(value == 1);
            CameraController.Instance.SetAntialiasingEnabled(value == 1);
            CameraController.Instance.SetAmbientOcclusionEnabled(value == 1);
        }
    }

    private void onChangeMasterVolume(int value)
    {
        AkSoundEngine.SetRTPCValue("masterVolume", (float) value);
    }

    private void onChangeMusicVolume(int value)
    {
        AkSoundEngine.SetRTPCValue("musicVolume", (float) value);
    }

    private void onChangeSfxVolume(int value)
    {
        AkSoundEngine.SetRTPCValue("soundVolume", (float) value);
    }

    private void onChangeTannoyVolume(int value)
    {
        AkSoundEngine.SetRTPCValue("tannoyVolume", (float) value);
    }

    private void onChangeEdgeScrolling(int value)
    {
    }

    private void onChangeCameraPanSpeed(int value)
    {
    }

    private void onChangeCameraRotSpeed(int value)
    {
    }

    private void onChangeOrthographicCamera(int value)
    {
        CameraController.Instance.SetOrthographicEnabled(value == 1);
    }

    private void onChangeColourblindMode(int value)
    {
        CameraController.Instance.SetColourblindMode(value);
    }

    private void onChangeHudEnabled(int value)
    {
        if(HUD.Instance)
        {
            HUD.Instance.SetHudEnabled(value == 1);
        }
    }

    private void onChangeHudScale(int value)
    {
        UIManager.Instance.SetCanvasScale(1.0f * (value / 10f));
    }

    private void onChangeTannoySubtitles(int value)
    {
    }
}
