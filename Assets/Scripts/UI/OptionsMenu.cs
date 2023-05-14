using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] tabContents;
    [SerializeField] private OptionsTabButton[] tabButtons;

    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown fullscreenDropdown;
    [SerializeField] private Toggle highQualityToggle;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider tannoyVolumeSlider;

    [SerializeField] private Toggle edgeScrollingToggle;
    [SerializeField] private Slider cameraPanSpeedSlider;
    [SerializeField] private TextMeshProUGUI cameraPanSpeedText;
    [SerializeField] private Slider cameraRotSpeedSlider;
    [SerializeField] private TextMeshProUGUI cameraRotSpeedText;
    [SerializeField] private Toggle orthographicCameraToggle;

    [SerializeField] private TMP_Dropdown colourblindModeDropdown;
    [SerializeField] private Toggle hudEnabledToggle;
    [SerializeField] private Slider hudScaleSlider;
    [SerializeField] private TextMeshProUGUI hudScaleText;
    [SerializeField] private Toggle tannoySubtitlesToggle;

    private int tabCurrent;
    private UnityAction onClose;

    public void Show(UnityAction onClose = null)
    {
        this.onClose = onClose;
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
        ChangeTab(0);
        loadValues();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onClose?.Invoke();
    }
    
    private void loadValues()
    {
        // Populate resolution options
        resolutionDropdown.ClearOptions();
        foreach (Resolution resolution in Screen.resolutions.Reverse())
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(string.Format("{0} x {1} ({2}Hz)", resolution.width, resolution.height, resolution.refreshRate)));
        }
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.value = OptionsManager.Instance.resolution.Value;
        fullscreenDropdown.value = OptionsManager.Instance.fullscreenMode.Value;
        highQualityToggle.isOn = OptionsManager.Instance.highQuality.BoolValue;

        masterVolumeSlider.value = OptionsManager.Instance.masterVolume.Value;
        musicVolumeSlider.value = OptionsManager.Instance.musicVolume.Value;
        sfxVolumeSlider.value = OptionsManager.Instance.sfxVolume.Value;
        tannoyVolumeSlider.value = OptionsManager.Instance.tannoyVolume.Value;

        edgeScrollingToggle.isOn = OptionsManager.Instance.edgeScrolling.BoolValue;
        cameraPanSpeedSlider.value = OptionsManager.Instance.cameraPanSpeed.Value;
        cameraRotSpeedSlider.value = OptionsManager.Instance.cameraRotSpeed.Value;
        orthographicCameraToggle.isOn = OptionsManager.Instance.orthographicCamera.BoolValue;

        colourblindModeDropdown.value = OptionsManager.Instance.colourblindMode.Value;
        hudEnabledToggle.isOn = OptionsManager.Instance.hudEnabled.BoolValue;
        hudScaleSlider.value = OptionsManager.Instance.hudScale.Value;
        tannoySubtitlesToggle.isOn = OptionsManager.Instance.tannoySubtitles.BoolValue;
    }

    public void ChangeTab(int tab)
    {
        tabCurrent = tab;
        for(int i = 0; i < tabContents.Length; i++)
        {
            tabContents[i].SetActive(i == tabCurrent);
            tabButtons[i].SetActive(i == tabCurrent);
        }
    }

    public void RestoreDefaults()
    {
        UIManager.Instance.ConfirmMenu.Show(
            "Are you sure you want to restore all settings to their default values?", false,
            delegate { OptionsManager.Instance.ResetAllToDefault(); loadValues(); }, null
        );
    }

    // UI Callbacks

    public void SetResolutionValue()
    {
        OptionsManager.Instance.resolution.SetValue(resolutionDropdown.value, true);
    }
    
    public void SetFullscreenModeValue()
    {
        OptionsManager.Instance.fullscreenMode.SetValue(fullscreenDropdown.value, true);
    }

    public void SetHighQualityValue()
    {
        OptionsManager.Instance.highQuality.SetValue(highQualityToggle.isOn ? 1 : 0, true);
    }

    public void SetMasterVolumeValue()
    {
        OptionsManager.Instance.masterVolume.SetValue((int) masterVolumeSlider.value, true);
    }

    public void SetMusicVolumeValue()
    {
        OptionsManager.Instance.musicVolume.SetValue((int) musicVolumeSlider.value, true);
    }

    public void SetSfxVolumeValue()
    {
        OptionsManager.Instance.sfxVolume.SetValue((int) sfxVolumeSlider.value, true);
    }

    public void SetTannoyVolumeValue()
    {
        OptionsManager.Instance.tannoyVolume.SetValue((int) tannoyVolumeSlider.value, true);
    }

    public void SetEdgeScrollingValue()
    {
        OptionsManager.Instance.edgeScrolling.SetValue(edgeScrollingToggle.isOn ? 1 : 0, true);
    }

    public void SetCameraPanSpeedValue()
    {
        OptionsManager.Instance.cameraPanSpeed.SetValue((int) cameraPanSpeedSlider.value, true);
        cameraPanSpeedText.text = cameraPanSpeedSlider.value.ToString();
    }

    public void SetCameraRotSpeedValue()
    {
        OptionsManager.Instance.cameraRotSpeed.SetValue((int) cameraRotSpeedSlider.value, true);
        cameraRotSpeedText.text = cameraRotSpeedSlider.value.ToString();
    }

    public void SetOrthographicCameraValue()
    {
        OptionsManager.Instance.orthographicCamera.SetValue(orthographicCameraToggle.isOn ? 1 : 0, true);
    }

    public void SetColourblindModeValue()
    {
        OptionsManager.Instance.colourblindMode.SetValue(colourblindModeDropdown.value, true);
    }

    public void SetHudEnabledValue()
    {
        OptionsManager.Instance.hudEnabled.SetValue(hudEnabledToggle.isOn ? 1 : 0, true);
    }

    public void SetHudScaleValue()
    {
        OptionsManager.Instance.hudScale.SetValue((int) hudScaleSlider.value, true);
        UpdateHudScaleText();
    }

    public void UpdateHudScaleText()
    {
        hudScaleText.text = hudScaleSlider.value.ToString();
    }

    public void SetTannoySubtitlesValue()
    {
        OptionsManager.Instance.tannoySubtitles.SetValue(tannoySubtitlesToggle.isOn ? 1 : 0, true);
    }
}