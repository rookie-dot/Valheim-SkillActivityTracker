using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

public static class SkillActivityTrackerUI
{
    private static GameObject canvasObject;
    private static List<SkillUI> skillUIs = new List<SkillUI>();
    private const int MaxSkills = 5;

    public class SkillUI
    {
        public GameObject root;
        public Text skillText;
        public Image backgroundImage;
        public float currentLevel; // Current skill level
        public string localizedName; // Localized name
        public Skills.SkillType? skillType; // Skill type
    }

    public static void CreateUI()
    {
        if (canvasObject != null) return;

        canvasObject = new GameObject("SkillTrackerCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Wysoki priorytet renderowania
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        UnityEngine.Object.DontDestroyOnLoad(canvasObject);

        // Use default Valheim font - try to load from Resources
        Font valheimFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (valheimFont == null)
        {
            valheimFont = Font.CreateDynamicFontFromOSFont("Arial", 13);
        }

        for (int i = 0; i < MaxSkills; i++)
        {
            SkillUI sUI = new SkillUI();
            
            sUI.root = new GameObject("SkillUI" + i);
            sUI.root.transform.SetParent(canvasObject.transform, false);
            sUI.root.SetActive(false); // Hidden until skill is used

            RectTransform rtRoot = sUI.root.AddComponent<RectTransform>();
            // Position on right side of screen, middle vertically
            rtRoot.anchorMin = new Vector2(1, 0.5f);
            rtRoot.anchorMax = new Vector2(1, 0.5f);
            rtRoot.pivot = new Vector2(1, 0.5f);
            rtRoot.anchoredPosition = new Vector2(-20, 60 - i * 30); // 30px spacing between elements
            rtRoot.sizeDelta = new Vector2(235, 26); // Increased by 20%

            // Caramel yellow gradient background (like in Valheim)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sUI.root.transform, false);
            sUI.backgroundImage = bgObj.AddComponent<Image>();
            
            // Create gradient texture - caramel yellow colors
            Texture2D gradientTexture = new Texture2D(1, 2);
            gradientTexture.SetPixel(0, 0, new Color(0.85f, 0.6f, 0.2f, 0.95f)); // Dark caramel at bottom
            gradientTexture.SetPixel(0, 1, new Color(1f, 0.85f, 0.4f, 0.95f));  // Light caramel at top
            gradientTexture.Apply();
            
            sUI.backgroundImage.sprite = Sprite.Create(
                gradientTexture, 
                new Rect(0, 0, 1, 2), 
                new Vector2(0.5f, 0.5f)
            );
            
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Text with name, level and percentage
            GameObject textObj = new GameObject("SkillText");
            textObj.transform.SetParent(sUI.root.transform, false);
            sUI.skillText = textObj.AddComponent<Text>();
            sUI.skillText.font = valheimFont;
            sUI.skillText.fontSize = 13;
            sUI.skillText.alignment = TextAnchor.MiddleLeft;
            sUI.skillText.color = Color.white;
            sUI.skillText.fontStyle = FontStyle.Bold;
            
            // Add outline for better readability
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 1f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            
            RectTransform textRt = sUI.skillText.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 0);
            textRt.offsetMax = new Vector2(-10, 0);

            skillUIs.Add(sUI);
        }
    }

    // UpdateUI - called after each skill change
    public static void UpdateUI(List<SkillActivityTrackerLogger.TrackedSkill> skills)
    {
        for (int i = 0; i < skillUIs.Count; i++)
        {
            if (i < skills.Count)
            {
                var s = skills[i];
                int wholeLevel = (int)s.Level;
                
                // Use calculated percentage
                float percentFloat = s.PercentProgress;

                // Use localized name or fallback to enum name
                string localizedName = !string.IsNullOrEmpty(s.LocalizedName) ? s.LocalizedName : s.Skill.ToString();
                
                // Save data in SkillUI
                skillUIs[i].currentLevel = s.Level;
                skillUIs[i].localizedName = localizedName;
                skillUIs[i].skillType = s.Skill;
                
                // Update text
                string displayText = $"{localizedName} {wholeLevel}      ({percentFloat:F2}%)";
                skillUIs[i].skillText.text = displayText;
                
                skillUIs[i].root.SetActive(true);
            }
            else
            {
                skillUIs[i].root.SetActive(false);
            }
        }
    }

    // Smooth bar animation - called in Update() every frame
    public static void AnimateUI()
    {
        // Get current player (only to check if exists)
        Player localPlayer = Player.m_localPlayer;
        if (localPlayer == null || localPlayer.GetSkills() == null)
            return;

        // Get current skills from logger - they have correct values from SkillsPatch
        var currentSkills = SkillActivityTrackerLogger.GetTrackedSkills();
        
        foreach (var sUI in skillUIs)
        {
            if (!sUI.root.activeSelf) continue;
            
            // Find current skill in list
            var trackedSkill = currentSkills.FirstOrDefault(s => s.Skill == sUI.skillType);
            if (trackedSkill != null)
            {
                // Use values from TrackedSkill - it's updated correctly in SkillsPatch
                sUI.currentLevel = trackedSkill.Level;
                sUI.localizedName = trackedSkill.LocalizedName ?? sUI.localizedName;
                
                int wholeLevel = (int)sUI.currentLevel;
                float percentFloat = trackedSkill.PercentProgress; // Calculated percentage 0-100
                
                // Update text dynamically
                string displayText = $"{sUI.localizedName} {wholeLevel}      ({percentFloat:F2}%)";
                sUI.skillText.text = displayText;
            }
        }
    }
}
