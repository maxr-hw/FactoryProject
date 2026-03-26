using UnityEngine;
using UnityEngine.UI;
using Factory.Core;

namespace Factory.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private void Start()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClick();
            }
        }
    }
}
