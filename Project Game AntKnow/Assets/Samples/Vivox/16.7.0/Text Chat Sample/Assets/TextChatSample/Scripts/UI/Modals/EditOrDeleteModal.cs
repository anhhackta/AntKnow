using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditOrDeleteModal : MonoBehaviour
{
    public Button EditButton;
    public Button DeleteButton;

    public void Show(UnityAction deleteButtonAction, UnityAction editButtonAction)
    {
        EditButton.onClick.AddListener(editButtonAction);
        DeleteButton.onClick.AddListener(deleteButtonAction);
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        EditButton.onClick.RemoveAllListeners();
        DeleteButton.onClick.RemoveAllListeners();
    }
}
