using TMPro;
using UnityEngine;

public class LookTarget : MonoBehaviour
{
    public string promptMessage = "Press E to interact";
    [SerializeField] private Canvas prompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float lookDistance = 10f;
    public float LookDistance => lookDistance;

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