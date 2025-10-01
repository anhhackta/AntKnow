using UnityEngine;

namespace AntKnow.Auth
{
    /// <summary>
    /// Handles click events on 3D character models
    /// </summary>
    public class ModelClickHandler : MonoBehaviour
    {
        private SelectCharacterController controller;
        private string gender;

        public void Initialize(SelectCharacterController controller, string gender)
        {
            this.controller = controller;
            this.gender = gender;
        }

        private void OnMouseDown()
        {
            if (controller != null)
            {
                controller.SelectCharacter(gender);
            }
        }

        private void OnMouseEnter()
        {
            // Add hover effect if needed
            Debug.Log($"Hovering over {gender} character");
        }

        private void OnMouseExit()
        {
            // Remove hover effect if needed
        }
    }
}
