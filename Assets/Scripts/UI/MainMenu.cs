using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator animTitle;
    [SerializeField] private Animator animButtons;

    void Awake()
    {
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        animTitle.SetTrigger("Open");
        animButtons.SetTrigger("Open");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("KianScene");
    }

    public void Options()
    {
        Hide();
        UIManager.Instance.OptionsMenu.Show(Show);
    }

    public void Help()
    {
        Hide();
        UIManager.Instance.HelpMenu.Show(Show);
    }

    public void Credits()
    {
        Hide();
        UIManager.Instance.CreditsMenu.Show(Show);
    }

    public void Quit()
    {
        Hide();
        UIManager.Instance.ConfirmMenu.Show("Are you sure you want to quit to the desktop?", true, Application.Quit, Show);
    }
}
