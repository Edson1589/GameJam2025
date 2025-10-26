using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text textField;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HideText();
    }

    public void ShowText(string msg)
    {
        if (panel != null) panel.SetActive(true);
        if (textField != null) textField.text = msg;
    }

    public void HideText()
    {
        if (panel != null) panel.SetActive(false);
    }
}
