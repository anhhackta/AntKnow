using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] public Button LogoutButton;
    [SerializeField] public Button ShowChannelListButton;
    [SerializeField] public Button ShowDirectMessageListButton;
    [SerializeField] public Button ViewChannelConversationsButton;
    [SerializeField] public Button ViewDMConversationsButton;
    [SerializeField] public GameObject LoginPanel;
    [SerializeField] public GameObject LobbyPanel;
    [SerializeField] public GameObject RosterPanel;
    [SerializeField] public GameObject DirectMessagePanel;

    public Text SignedInPlayerName;
    public Text SignedInPlayerId;
    public Button CopyButton;

    private Coroutine slideCoroutine = null;
    static public bool isCanShowProfileUI = false;

    public GameObject ProfilePanelUI;

    void Start()
    {
        LogoutButton.onClick.AddListener(async () => await TextSampleUIManager.Instance.RunTaskWithLoadingUI(LogoutOfVivox()));
        ViewChannelConversationsButton.onClick.AddListener(() => { TextSampleUIManager.Instance.SwitchConversationContext(ConversationType.ChannelConversation); });
        ViewDMConversationsButton.onClick.AddListener(() => { TextSampleUIManager.Instance.SwitchConversationContext(ConversationType.DirectedMessageConversation); });
    }
    private void OnEnable()
    {
        SignedInPlayerName.text = TextSampleUIManager.Instance.SignedInPlayerDisplayName;
        SignedInPlayerId.text = $"Player ID: {VivoxService.Instance.SignedInPlayerId}";
        CopyButton.onClick.AddListener(() => TextSampleUIManager.Instance.CopyToClipboard(VivoxService.Instance.SignedInPlayerId));
        ProfilePanelUI.SetActive(true);

        switch (TextSampleUIManager.Instance.CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
            {
                ViewChannelConversationsButton.Select();
            }
            break;
            case ConversationType.DirectedMessageConversation:
            {
                ViewDMConversationsButton.Select();
            }
            break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        CopyButton.onClick.RemoveAllListeners();
        ProfilePanelUI.SetActive(false);
    }

    private async Task LogoutOfVivox()
    {
        await VivoxService.Instance.LogoutAsync();
#if AUTH_PACKAGE_PRESENT
        AuthenticationService.Instance.SignOut();
#endif
    }

    public void ShowProfileUI(bool bo)
    {
        Vector2 showPos = new Vector2(0, GetComponent<RectTransform>().anchoredPosition.y);
        Vector2 hidePos = new Vector2(-Screen.width, GetComponent<RectTransform>().anchoredPosition.y);

        var destPos = (bo == true ? showPos : hidePos);

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlidePanel(destPos));
    }

    IEnumerator SlidePanel(Vector2 destPos)
    {
        float time = 0f;
        float duration = 0.15f;
        var startPos = GetComponent<RectTransform>().anchoredPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, destPos, t);
            yield return null;
        }

        slideCoroutine = null;
    }

    IEnumerator DetectSlide()
    {
        Vector3 lastPos = Vector3.zero;
        Vector3 currPos = Vector3.zero;

        var rectProfilePanel = GetComponent<RectTransform>();
        var rectBackgroundPanel = transform.Find("BackgroundPanel").GetComponent<RectTransform>();

        while (true)
        {
            yield return null;

            if (Input.GetMouseButtonDown(0))
            {
                lastPos = Input.mousePosition;

                if (RectTransformUtility.RectangleContainsScreenPoint(rectProfilePanel, lastPos) == true &&
                   RectTransformUtility.RectangleContainsScreenPoint(rectBackgroundPanel, lastPos) == false)
                {
                    ShowProfileUI(false);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                currPos = Input.mousePosition;
                var offsetX = currPos.x - lastPos.x;

                if (Math.Abs(offsetX) > 200 && CanShowProfilePanel() == true)
                {
                    var bo = (offsetX > 0 ? true : false);
                    ShowProfileUI(bo);
                }
            }
        }
    }

    bool CanShowProfilePanel()
    {
        if (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0)
            return false;

        if (Input.mousePosition.x >= Screen.width || Input.mousePosition.y >= Screen.height)
            return false;

        var rect = LoginPanel.GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition) == true)
            return false;

        rect = RosterPanel.GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition) == true)
            return false;

        rect = DirectMessagePanel.GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition) == true)
            return false;

        if (ViewChannelConversationsButton.transform.Find("CreateChannelPanel").gameObject.activeSelf == true)
            return false;

        if (ViewDMConversationsButton.transform.Find("CreateDirectMessagePanel").gameObject.activeSelf == true)
            return false;

        if (LobbyPanel.GetComponent<CanvasGroup>().interactable == false)
            return false;

        if (GetComponent<CanvasGroup>().interactable == false)
            return false;

        return true;
    }
}
