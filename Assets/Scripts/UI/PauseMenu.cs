using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Resume()
    {
        PlayerController.Instance.SetPaused(false);
    }

    public void Options()
    {
        UIManager.Instance.OptionsMenu.Show();
    }

    public void Help()
    {
        UIManager.Instance.HelpMenu.Show();
    }

    public void MainMenu()
    {
        PlayerController.Instance.SetPaused(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Hide();
        UIManager.Instance.ConfirmMenu.Show("Are you sure you want to quit to the desktop?", true, Application.Quit, Show);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
