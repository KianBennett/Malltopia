using UnityEngine;
using UnityEngine.Events;

public class StandardMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private UnityAction onClose;

    public virtual void Show(UnityAction onClose = null)
    {
        this.onClose = onClose;
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        onClose?.Invoke();
    }
}
