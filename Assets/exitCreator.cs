using UnityEngine;
using UnityEngine.UI;

public class exitCreator : MonoBehaviour
{
    public Button button;
    public Canvas canvas;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvas = GetComponentInParent<Canvas>();
    }

        public void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(HideCanvas);
        }
    }

    public void HideCanvas()
    {
        Player.Instance.OnCreatorExit();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }
}
