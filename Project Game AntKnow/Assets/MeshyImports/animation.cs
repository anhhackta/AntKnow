using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private bool isRunning = false; // trạng thái hiện tại

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isRunning", false); // mặc định Idle
    }

    // Hàm này sẽ gắn vào Button OnClick()
    public void ToggleRun()
    {
        isRunning = !isRunning;
        animator.SetBool("isRunning", isRunning);
    }
}
