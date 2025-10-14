using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AntKnow.Menu
{
    /// <summary>
    /// SIÊU ĐƠN GIẢN - Chỉ load DemoScene khi click button
    /// Attach script này vào button, DONE!
    /// </summary>
    public class DemoButton : MonoBehaviour
    {
        private void Start()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => {
                    Debug.Log("[DemoButton] Loading DemoScene...");
                    SceneManager.LoadScene("DemoScene");
                });
            }
        }
    }
}
