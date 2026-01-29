using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NightBlade.Addons;

public class DungeonSeed : MonoBehaviour
{
    private void OnEnable()
    {
        // Wir warten einen Frame, falls der Singleton gerade erst initialisiert wird
        Invoke(nameof(Subscribe), 0.1f);
    }

    private void Subscribe()
    {
        if (ProceduralGenerationManager.Singleton != null)
        {
            // Wir registrieren uns für zukünftige Änderungen
            ProceduralGenerationManager.Singleton.OnWorldSeedReceived += UpdateText;

            // Falls der Seed schon existiert (weil wir nach dem Laden joinen), direkt anzeigen
            if (ProceduralGenerationManager.Singleton.CurrentInstanceSeed != 0)
            {
                UpdateText(ProceduralGenerationManager.Singleton.CurrentInstanceSeed);
            }
        }
    }

    private void OnDisable()
    {
        if (ProceduralGenerationManager.Singleton != null)
            ProceduralGenerationManager.Singleton.OnWorldSeedReceived -= UpdateText;
    }

    private void UpdateText(int seed)
    {
        // Automatische Erkennung ob TMP oder Standard Text
        var tmp = GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = "Seed: " + seed;

        var legacy = GetComponent<Text>();
        if (legacy != null) legacy.text = "Seed: " + seed;
    }
}