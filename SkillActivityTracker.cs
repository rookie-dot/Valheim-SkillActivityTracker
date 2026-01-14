using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections; // dla IEnumerator

[BepInPlugin("pl.rookie.skillactivitytracker", "Skill Activity Tracker", "1.0.0")]
public class SkillActivityTracker : BaseUnityPlugin
{
    public static SkillActivityTracker Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        // Inicjalizacja konfiguracji
        SkillActivityTrackerConfig.Initialize(Config);
        
        // Inicjalizacja loggera
        SkillActivityTrackerLogger.Init(Logger);

        // Patch Harmony
        var harmony = new Harmony("pl.rookie.skillactivitytracker");
        harmony.PatchAll();

        Logger.LogInfo("SkillActivityTracker loaded");
    }

    private void Start()
    {
        // Odroczone utworzenie UI (2 frame’y)
        StartCoroutine(DelayedCreateUI());
    }

    private IEnumerator DelayedCreateUI()
    {
        yield return null;
        yield return null;
        SkillActivityTrackerUI.CreateUI();
    }

    private void Update()
    {
        SkillActivityTrackerUI.AnimateUI();
        SkillActivityTrackerLogger.CheckExpiredSkills();
    }
}
