using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ChannelParticipantTile : MonoBehaviour
{
    [SerializeField] public Text PlayerDisplayNameText;
    [SerializeField] public Text PlayerIdText;
    [SerializeField] public Button CopyButton;

    VivoxParticipant parentParticipant;

    private void OnDisable()
    {
        Cleanup();
    }

    public void SetupRosterEntry(VivoxParticipant participant)
    {
        parentParticipant = participant;
        PlayerDisplayNameText.text = participant.DisplayName;
        if (participant.IsSelf)
        {
            PlayerDisplayNameText.text += " (Me)";
        }
        PlayerIdText.text = $"Player ID: {participant.PlayerId}";

        VivoxService.Instance.ParticipantRemovedFromChannel += CheckAndDestroyTile;
        if (!parentParticipant.IsSelf)
        {
            CopyButton.onClick.AddListener(() => TextSampleUIManager.Instance.CopyToClipboard(parentParticipant.PlayerId));
        }
    }

    void CheckAndDestroyTile(VivoxParticipant removedParticipant)
    {
        if (removedParticipant.PlayerId == parentParticipant.PlayerId)
        {
            Cleanup();
        }
    }

    void Cleanup()
    {
        VivoxService.Instance.ParticipantRemovedFromChannel -= CheckAndDestroyTile;
        CopyButton.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
