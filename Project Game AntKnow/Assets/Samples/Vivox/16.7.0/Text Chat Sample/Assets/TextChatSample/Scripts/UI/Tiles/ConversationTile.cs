using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ConversationTile : MonoBehaviour
{
    public Sprite DMIcon;
    public Sprite ChannelIcon;
    public Sprite PlusIcon;
    public Image TileIconImage;
    public Text TileNameText;
    public Text PlayerIdText;
    public Button JoinConversationButton;

    public void SetupExisitingConversationTile(VivoxConversation conversationInfo)
    {
        SetupExisitingConversationTile(conversationInfo.ConversationType, conversationInfo.Name);
    }

    public void SetupExisitingConversationTile(ConversationType conversationType, string id, string displayName = null)
    {
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
            {
                // We don't need the ID portion if it's a channel conversation.
                Destroy(PlayerIdText.gameObject);
                TileNameText.text = id;
                TileIconImage.sprite = ChannelIcon;
                JoinConversationButton.onClick.AddListener(async () => await SignalJoinConversation(id));
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                TileIconImage.sprite = DMIcon;
                TileNameText.text = id == VivoxService.Instance.SignedInPlayerId ? $"{TextSampleUIManager.Instance.SignedInPlayerDisplayName} (Me)" : displayName;
                PlayerIdText.text = $"Player ID: {id}";
                JoinConversationButton.onClick.AddListener(async () => await SignalJoinConversation(id));
            }
            break;
            default:
                break;
        }
    }

    public void SetupCreateNewConversationTile(ConversationType conversationType)
    {
        Destroy(PlayerIdText.gameObject);
        TileIconImage.sprite = PlusIcon;
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
            {
                TileNameText.text = "Create New Channel";
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                TileNameText.text = "Create New Message";
            }
            break;
            default:
                break;
        }
    }
    async Task SignalJoinConversation(string conversationId)
    {
        await TextSampleUIManager.Instance.JoinConversation(conversationId);
    }

}
