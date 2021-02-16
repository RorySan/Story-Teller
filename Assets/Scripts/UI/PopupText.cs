using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
    [SerializeField] private Text popupText;
    private void Start()
    {
        gameObject.SetActive(false);
    }
    public void DisplayPopupText(string text)
    {
        gameObject.SetActive(true);
        popupText.text = text;
    }

    public void HidePopupText()
    {
        gameObject.SetActive(false);
    }
}
