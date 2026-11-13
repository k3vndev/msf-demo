using System;
using UnityEngine;

public class TranslatedText : TextHandler
{
    [SerializeField] string id;
    public Func<string> ParamGetter;
    public string Id { get => id; }

    /// <summary>
    /// Refreshes the text of the TextMeshProUGUI component based on the current language setting.<br/>
    /// If a parameter getter is provided, it replaces the corresponding placeholder in the text.
    /// </summary>
    public override void Refresh()
    {
        if (string.IsNullOrEmpty(id)) return;

        string text = TranslationSystem.I.GetFromId(id);
        ReplaceWithParam(ref text);

        textComponent.text = text;

        base.Refresh();
    }

    /// <summary>Refreshes the text of the TextMeshProUGUI component with a new ID.</summary>
    public void RefreshId(string newId)
    {
        id = newId.Trim();
        Refresh();
    }

    void ReplaceWithParam(ref string text)
    {
        string param = "";

        if (ParamGetter != null)
        {
            bool isTypingParam = false;

            foreach (char character in text)
            {
                if (character == '<' && !isTypingParam)
                {
                    isTypingParam = true;
                    continue;
                }

                if (isTypingParam)
                {
                    if (character == '>')
                    {
                        isTypingParam = false;
                        break;
                    }
                    param += character;
                }
            }
            text = text.Replace($"<{param}>", ParamGetter.Invoke());
        }

        if (HasUnmatchedBrackets(text))
        {
            Debug.LogWarning($"TextHandler: Text \"{text}\" with ID \"{id}\" at {name} gameobject contains unmatched angle brackets. ");
        }
    }

    bool HasUnmatchedBrackets(string text)
    {
        int open = 0;
        int close = 0;

        foreach (char c in text)
        {
            if (c == '<') open++;
            if (c == '>') close++;
        }

        return open != close;
    }


    void HandlerLanguageChanged(int _) => Refresh();

    protected override void OnEnable()
    {
        base.OnEnable();
        Refresh();
        TranslationSystem.OnLanguageChanged += HandlerLanguageChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        TranslationSystem.OnLanguageChanged -= HandlerLanguageChanged;
    }

    void OnValidate()
    {
        if (id != null) id = id.Trim();
    }
}