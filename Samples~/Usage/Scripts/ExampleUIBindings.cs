using System;
using TMPro;
using UnityEngine;

public class ExampleUIBindings : MonoBehaviour
{
    [field: SerializeField]public TMP_InputField PromptField { get; private set; }
    [field: SerializeField]public TMP_Text AIChatBox { get; private set; }
    [field: SerializeField]public IntegrationExample Integration { get; private set; }

    public void Submit()
    {
        var prompt = PromptField.text;
        PromptField.SetTextWithoutNotify(string.Empty);
        if(!string.IsNullOrWhiteSpace(prompt)) Integration.Prompt(prompt);
    }

    private void Update()
    {
        while(Integration.TextResponseQueue.Count > 0)
        {
            var part = Integration.TextResponseQueue.Dequeue();
            AIChatBox.text += part;
            if (AIChatBox.fontSize < 60) AIChatBox.text = AIChatBox.text.Substring(Mathf.FloorToInt(AIChatBox.text.Length * 0.25f), AIChatBox.text.Length - Mathf.FloorToInt(AIChatBox.text.Length * 0.25f));
        }
    }
}
