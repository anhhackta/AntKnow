using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{

    [SerializeField]
    float m_rotateSpeed = 100.0f;
    [SerializeField]
    Transform m_spinner;

    // Update is called once per frame
    void Update()
    {
        m_spinner.Rotate(Vector3.forward, -m_rotateSpeed * Time.deltaTime);
    }

    public void SetActive(bool active, string reason = "")
    {
        gameObject.SetActive(active);
    }
}
