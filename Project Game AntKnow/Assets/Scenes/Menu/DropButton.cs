using UnityEngine;

public class DropdownMenu : MonoBehaviour
{
    public GameObject subMenu;   // Kéo thả SubMenu vào đây

    public void ToggleSubMenu()
    {
        subMenu.SetActive(!subMenu.activeSelf);
    }

    public void HideSubMenu()
    {
        subMenu.SetActive(false);
    }
}