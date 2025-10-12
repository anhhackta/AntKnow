using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeleteConfirmationModal : MonoBehaviour
{
    public Button DeleteConfirmationButton;
    public Button CancelButton;

    public void Show(UnityAction deleteAction, UnityAction cancelAction = null)
    {
        CancelButton.onClick.AddListener(cancelAction);
        DeleteConfirmationButton.onClick.AddListener(deleteAction);
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        CancelButton.onClick.RemoveAllListeners();
        DeleteConfirmationButton.onClick.RemoveAllListeners();
    }
}
