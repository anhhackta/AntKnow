using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditMessageModal : MonoBehaviour
{
    UnityAction<string> m_saveButtonAction;

    public Button CancelButton;
    public Button SaveButton;
    public InputField EditMessageinputField;

    public void Show(string originalMessage, UnityAction cancelButtonAction, UnityAction<string> saveButtonAction)
    {
        EditMessageinputField.text = originalMessage;
        EditMessageinputField.Select();
        EditMessageinputField.ActivateInputField();
        m_saveButtonAction = saveButtonAction;
        CancelButton.onClick.AddListener(cancelButtonAction);
        SaveButton.onClick.AddListener(() => saveButtonAction?.Invoke(EditMessageinputField.text));
        gameObject.SetActive(true);
    }

    void OnDisable()
    {
        CancelButton.onClick.RemoveAllListeners();
        SaveButton.onClick.RemoveAllListeners();
    }

    void Update()
    {
        SaveButton.interactable = !string.IsNullOrEmpty(EditMessageinputField.text);
    }
}
