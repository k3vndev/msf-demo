using System;
using System.Collections.Generic;
using UnityEngine;

public class TranslationSystem : MonoBehaviour
{
    public static TranslationSystem I { get; private set; }
    public int CurrentLanguage { get => GameManager.I.Global.Settings.Language; }
    readonly Dictionary<string, string[]> TranslatedTexts = new();

    public static event Action<int> OnLanguageChanged;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        LoadTextsFromCSV();
    }

    /// <summary>
    /// Loads translated texts from a CSV file located in the Resources folder and populates the TranslatedTexts dictionary.
    /// </summary>
    void LoadTextsFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("translated-texts");

        if (csvFile == null)
        {
            Debug.LogError("CSV file not found in Resources!");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\n' }, System.StringSplitOptions.None);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string[] fields = ExtractFields(line);

            // Skip empty lines
            if (fields.Length == 0) continue;

            // Skip lines with no ID
            if (fields.Length < 3 || string.IsNullOrWhiteSpace(fields[0]))
            {
                Debug.LogWarning($"Line {i + 1} is missing ID or has insufficient fields. Recieved [{line}]");
                continue;
            }

            string id = fields[0].Trim();
            string englishText = fields[1].Trim();
            string spanishText = fields[2].Trim();

            if (!TranslatedTexts.ContainsKey(id))
            {
                TranslatedTexts[id] = new string[] { englishText, spanishText };
                continue;
            }
            Debug.LogWarning($"Duplicate ID detected: {id} (line {i + 1})");
        }
    }

    /// <summary>Changes the current language of the game.</summary>
    public void ChangeLanguage(int language)
    {
        if (language != 0 && language != 1)
        {
            throw new IndexOutOfRangeException(
                $"Invalid language selection. Use 0 for English and 1 for Spanish. Received: {language}");
        }

        GameManager.I.Global.Settings.Language = language;
        SaveSystem.I.RequestSave();

        OnLanguageChanged?.Invoke(language);
    }

    /// <summary>Gets the translated text for the given ID based on the current language.</summary>
    public string GetFromId(string id)
    {
        if (TranslatedTexts.TryGetValue(id, out string[] texts))
            return texts[CurrentLanguage];

        throw new KeyNotFoundException($"Translation ID '{id}' not found.");
    }

    string[] ExtractFields(string line)
    {
        string[] fields = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return fields;
    }
}
