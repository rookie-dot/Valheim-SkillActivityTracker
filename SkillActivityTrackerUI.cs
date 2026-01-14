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
    private static bool isDragging = false;
    private static Vector2 dragOffset;
    private static string currentCharacterName = "";
    private static Vector2 basePosition = new Vector2(-20, 60);

    public class SkillUI
    {
        public GameObject root;
        public Text skillText;
        public Image backgroundImage;
        public Image progressBar;
        public float currentLevel;
        public string localizedName;
        public Skills.SkillType? skillType;
        public Vector2 targetPosition;
    }

    public static void CreateUI()
    {
        if (canvasObject != null) return;

        canvasObject = new GameObject("SkillTrackerCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
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
            sUI.root.SetActive(false);

            RectTransform rtRoot = sUI.root.AddComponent<RectTransform>();
            rtRoot.anchorMin = new Vector2(1, 0.5f);
            rtRoot.anchorMax = new Vector2(1, 0.5f);
            rtRoot.pivot = new Vector2(1, 0.5f);
            rtRoot.anchoredPosition = new Vector2(basePosition.x, basePosition.y - i * 30);
            rtRoot.sizeDelta = new Vector2(235, 26);
            
            sUI.targetPosition = new Vector2(basePosition.x, basePosition.y - i * 30);

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sUI.root.transform, false);
            sUI.backgroundImage = bgObj.AddComponent<Image>();
            
            Texture2D gradientTexture = new Texture2D(1, 2);
            gradientTexture.SetPixel(0, 0, new Color(0.85f, 0.6f, 0.2f, 0.95f));
            gradientTexture.SetPixel(0, 1, new Color(1f, 0.85f, 0.4f, 0.95f));
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

            // Progress bar
            GameObject progressObj = new GameObject("ProgressBar");
            progressObj.transform.SetParent(sUI.root.transform, false);
            sUI.progressBar = progressObj.AddComponent<Image>();
            
            // Create gradient texture for progress bar - orange gradient
            Texture2D progressGradient = new Texture2D(1, 2);
            progressGradient.SetPixel(0, 0, new Color(0.9f, 0.4f, 0f, 0.8f));
            progressGradient.SetPixel(0, 1, new Color(1f, 0.6f, 0.1f, 0.8f));
            progressGradient.Apply();
            
            sUI.progressBar.sprite = Sprite.Create(
                progressGradient,
                new Rect(0, 0, 1, 2),
                new Vector2(0.5f, 0.5f)
            );
            
            RectTransform progressRt = progressObj.GetComponent<RectTransform>();
            progressRt.anchorMin = new Vector2(0, 0);
            progressRt.anchorMax = new Vector2(0, 1);
            progressRt.pivot = new Vector2(0, 0.5f);
            progressRt.anchoredPosition = Vector2.zero;
            progressRt.sizeDelta = new Vector2(0, 0);

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

    public static void UpdateUI(List<SkillActivityTrackerLogger.TrackedSkill> skills)
    {
        for (int i = 0; i < skillUIs.Count; i++)
        {
            if (i < skills.Count)
            {
                var s = skills[i];
                int wholeLevel = (int)s.Level;
                float percentFloat = s.PercentProgress;

                string localizedName = !string.IsNullOrEmpty(s.LocalizedName) ? s.LocalizedName : s.Skill.ToString();
                
                skillUIs[i].currentLevel = s.Level;
                skillUIs[i].localizedName = localizedName;
                skillUIs[i].skillType = s.Skill;
                
                string displayText = $"{localizedName}  •  {wholeLevel}  •  ({percentFloat:F2}%)";
                skillUIs[i].skillText.text = displayText;
                
                // Update progress bar
                RectTransform progressRt = skillUIs[i].progressBar.GetComponent<RectTransform>();
                float barWidth = (percentFloat / 100f) * 235f;
                progressRt.sizeDelta = new Vector2(barWidth, 0);
                
                // Set target position
                skillUIs[i].targetPosition = new Vector2(basePosition.x, basePosition.y - i * 30);
                
                skillUIs[i].root.SetActive(true);
            }
            else
            {
                skillUIs[i].root.SetActive(false);
            }
        }
    }

    public static void AnimateUI()
    {
        Player localPlayer = Player.m_localPlayer;
        if (localPlayer == null || localPlayer.GetSkills() == null)
            return;
        
        string playerName = localPlayer.GetPlayerName();
        if (playerName != currentCharacterName)
        {
            currentCharacterName = playerName;
            LoadCharacterPosition();
        }

        HandleDragging();

        var currentSkills = SkillActivityTrackerLogger.GetTrackedSkills();
        
        foreach (var sUI in skillUIs)
        {
            if (!sUI.root.activeSelf) continue;
            
            // Smoothly animate position
            RectTransform rt = sUI.root.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, sUI.targetPosition, Time.deltaTime * 8f);
            
            var trackedSkill = currentSkills.FirstOrDefault(s => s.Skill == sUI.skillType);
            if (trackedSkill != null)
            {
                sUI.currentLevel = trackedSkill.Level;
                sUI.localizedName = trackedSkill.LocalizedName ?? sUI.localizedName;
                
                int wholeLevel = (int)sUI.currentLevel;
                float percentFloat = trackedSkill.PercentProgress;
                
                string displayText = $"{sUI.localizedName}  •  {wholeLevel}  •  ({percentFloat:F2}%)";
                sUI.skillText.text = displayText;
                
                // Animate progress bar
                RectTransform progressRt = sUI.progressBar.GetComponent<RectTransform>();
                float targetWidth = (percentFloat / 100f) * 235f;
                float currentWidth = progressRt.sizeDelta.x;
                float newWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * 5f);
                progressRt.sizeDelta = new Vector2(newWidth, 0);
            }
        }
    }
    
    private static void HandleDragging()
    {
        if (canvasObject == null || skillUIs.Count == 0) return;
        
        bool leftMouseButton = UnityEngine.Input.GetMouseButton(0);
        bool leftMouseButtonDown = UnityEngine.Input.GetMouseButtonDown(0);
        bool leftMouseButtonUp = UnityEngine.Input.GetMouseButtonUp(0);
        
        if (leftMouseButtonDown && !isDragging)
        {
            Vector2 mousePos = UnityEngine.Input.mousePosition;
            foreach (var sUI in skillUIs)
            {
                if (!sUI.root.activeSelf) continue;
                
                RectTransform rt = sUI.root.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null))
                {
                    isDragging = true;
                    RectTransform firstRT = skillUIs[0].root.GetComponent<RectTransform>();
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasObject.GetComponent<RectTransform>(),
                        mousePos,
                        null,
                        out localPoint
                    );
                    dragOffset = firstRT.anchoredPosition - localPoint;
                    break;
                }
            }
        }
        
        if (isDragging && leftMouseButton)
        {
            Vector2 mousePos = UnityEngine.Input.mousePosition;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasObject.GetComponent<RectTransform>(),
                mousePos,
                null,
                out localPoint
            );
            
            Vector2 newPos = localPoint + dragOffset;
            basePosition = newPos;
            
            for (int i = 0; i < skillUIs.Count; i++)
            {
                Vector2 pos = new Vector2(newPos.x, newPos.y - i * 30);
                RectTransform rt = skillUIs[i].root.GetComponent<RectTransform>();
                rt.anchoredPosition = pos;
                skillUIs[i].targetPosition = pos;
            }
        }
        
        if (isDragging && leftMouseButtonUp)
        {
            isDragging = false;
            SaveCharacterPosition();
        }
    }
    
    private static void LoadCharacterPosition()
    {
        if (string.IsNullOrEmpty(currentCharacterName)) return;
        
        basePosition = SkillActivityTrackerConfig.GetCharacterUIPosition(currentCharacterName);
        
        for (int i = 0; i < skillUIs.Count; i++)
        {
            Vector2 newPos = new Vector2(basePosition.x, basePosition.y - i * 30);
            RectTransform rt = skillUIs[i].root.GetComponent<RectTransform>();
            rt.anchoredPosition = newPos;
            skillUIs[i].targetPosition = newPos;
        }
    }
    
    private static void SaveCharacterPosition()
    {
        if (string.IsNullOrEmpty(currentCharacterName)) return;
        
        SkillActivityTrackerConfig.SetCharacterUIPosition(currentCharacterName, basePosition);
    }
}
