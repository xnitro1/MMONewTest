using DunGen;
using DunGen.Analysis;
using DunGen.Demo;
using NightBlade.Addons;
using System;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SeedUIDisplay : MonoBehaviour
{

    public RuntimeDungeon DungeonGenerator;
    public Action<StringBuilder> GetAdditionalText;

    private readonly StringBuilder infoText = new StringBuilder();
    private IDemoInputBridge inputBridge;
    private bool showStats = true;
    private float keypressDelay = 0.1f;
    private float timeSinceLastPress;
    private bool allowHold;
    private bool isKeyDown;
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
        inputBridge = new DemoInputBridge();

        if (DungeonGenerator == null)
        {
        DungeonGenerator = GetComponentInChildren<RuntimeDungeon>();
        
        }
        DungeonGenerator.Generator.Seed = seed;

        GenerateRandom();
    }


    public void GenerateRandom() => DungeonGenerator.Generate();

    private void Update()
    {
    } 

}