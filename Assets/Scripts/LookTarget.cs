using TMPro;
using UnityEngine;

// Put this on any object (with a Collider) that should trigger the UI when looked at.
public class LookTarget : MonoBehaviour
{
    public string promptMessage = "Press E to interact";
    [SerializeField] private Canvas prompt;
    [SerializeField] private TextMeshProUGUI promptText;

    void Start()
    {
        promptText.text = promptMessage;
        prompt.gameObject.SetActive(false);
    }

    public void ShowMessage(bool show)
    {
        prompt.gameObject.SetActive(show);
    }
}