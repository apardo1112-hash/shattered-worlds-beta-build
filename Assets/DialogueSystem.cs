using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public Canvas dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public Transform optionContainer;
    public GameObject optionButtonPrefab;

    private Action<int> currentCallback;
    private readonly List<Button> spawnedButtons = new List<Button>();

    public bool IsOpen => dialogueCanvas != null && dialogueCanvas.gameObject.activeSelf;

    public void ShowDialogue(string[] lines, string[] options, Action<int> onOptionSelected)
    {
        EnsureUi();
        dialogueCanvas.gameObject.SetActive(true);
        dialogueText.text = string.Join("\n", lines);

        ClearButtons();
        currentCallback = onOptionSelected;

        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            GameObject buttonObject = Instantiate(optionButtonPrefab, optionContainer);
            Button button = buttonObject.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = options[i];
            }

            button.onClick.AddListener(() => SelectOption(index));
            spawnedButtons.Add(button);
        }
    }

    public void HideDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }

        ClearButtons();
        currentCallback = null;
    }

    private void SelectOption(int index)
    {
        HideDialogue();
        currentCallback?.Invoke(index);
    }

    private void ClearButtons()
    {
        for (int i = spawnedButtons.Count - 1; i >= 0; i--)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private void EnsureUi()
    {
        if (dialogueCanvas == null)
        {
            GameObject canvasObject = new GameObject("DialogueCanvas");
            dialogueCanvas = canvasObject.AddComponent<Canvas>();
            dialogueCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (dialogueText == null)
        {
            GameObject panelObject = new GameObject("DialoguePanel");
            panelObject.transform.SetParent(dialogueCanvas.transform, false);

            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500f, 260f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);

            GameObject textObject = new GameObject("DialogueText");
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(20f, 80f);
            textRect.offsetMax = new Vector2(-20f, -20f);
            dialogueText = textObject.AddComponent<TextMeshProUGUI>();
            dialogueText.fontSize = 24f;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
        }

        if (optionContainer == null)
        {
            GameObject containerObject = new GameObject("OptionContainer");
            containerObject.transform.SetParent(dialogueText.transform.parent, false);
            RectTransform containerRect = containerObject.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(1f, 0f);
            containerRect.sizeDelta = new Vector2(0f, 60f);
            containerRect.anchoredPosition = new Vector2(0f, 20f);
            optionContainer = containerObject.transform;
        }

        if (optionButtonPrefab == null)
        {
            GameObject buttonObject = new GameObject("OptionButtonPrefab");
            buttonObject.transform.SetParent(optionContainer, false);
            Button button = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.3f, 0.7f, 1f);
            button.targetGraphic = buttonImage;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = Color.gray,
                pressedColor = Color.white,
                selectedColor = Color.white,
                disabledColor = Color.gray,
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            GameObject textObject = new GameObject("ButtonText");
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);
            TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontSize = 20f;
            optionButtonPrefab = buttonObject;
        }

        dialogueCanvas.gameObject.SetActive(false);
    }
}
