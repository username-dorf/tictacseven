using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class UITitleView: MonoBehaviour
    {
        [SerializeField] private TMP_Text title;

        public void Initialize(string message)
        {
            title.text = message;
        }
    }
}