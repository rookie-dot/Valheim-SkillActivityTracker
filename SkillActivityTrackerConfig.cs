using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillActivityTrackerConfig
{
    // Dictionary to store if skill should be tracked (true = display, false = hide)
    public static Dictionary<Skills.SkillType, ConfigEntry<bool>> SkillTrackingEnabled = new Dictionary<Skills.SkillType, ConfigEntry<bool>>();
    
    // Display duration in minutes
    public static ConfigEntry<float> DisplayDurationMinutes;
    
    // Per-character UI positions
    private static Dictionary<string, ConfigEntry<string>> characterPositions = new Dictionary<string, ConfigEntry<string>>();
    private static ConfigFile configFile;

    public static void Initialize(ConfigFile config)
    {
        configFile = config;
        
        // Display duration setting
        DisplayDurationMinutes = config.Bind(
            "General",
            "DisplayDurationMinutes",
            2.0f,
            "How long (in minutes) skills should be displayed after last use before disappearing"
        );

        // All Valheim skills - set to true by default (display all)
        var allSkills = (Skills.SkillType[])Enum.GetValues(typeof(Skills.SkillType));
        
        foreach (var skill in allSkills)
        {
            // Skip "None" and "All" if they exist
            if (skill == Skills.SkillType.None || skill == Skills.SkillType.All)
                continue;

            SkillTrackingEnabled[skill] = config.Bind(
                "Skill Tracking",
                skill.ToString(),
                true,
                $"Enable tracking for {skill} skill (true = display, false = hide)"
            );
        }
    }

    // Check if skill should be tracked
    public static bool IsSkillTrackingEnabled(Skills.SkillType skill)
    {
        if (SkillTrackingEnabled.ContainsKey(skill))
        {
            return SkillTrackingEnabled[skill].Value;
        }
        return true; // Default to true if not found
    }

    // Get display duration as TimeSpan
    public static TimeSpan GetDisplayDuration()
    {
        return TimeSpan.FromMinutes(DisplayDurationMinutes.Value);
    }
    
    // Get UI position for specific character
    public static Vector2 GetCharacterUIPosition(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
            return new Vector2(-20, 60); // Default position
        
        if (!characterPositions.ContainsKey(characterName))
        {
            characterPositions[characterName] = configFile.Bind(
                "Character Positions",
                characterName,
                "-20,60",
                $"UI position for character {characterName} (format: x,y)"
            );
        }
        
        string posStr = characterPositions[characterName].Value;
        string[] parts = posStr.Split(',');
        
        if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
        {
            return new Vector2(x, y);
        }
        
        return new Vector2(-20, 60); // Default position if parsing fails
    }
    
    // Save UI position for specific character
    public static void SetCharacterUIPosition(string characterName, Vector2 position)
    {
        if (string.IsNullOrEmpty(characterName))
            return;
        
        if (!characterPositions.ContainsKey(characterName))
        {
            characterPositions[characterName] = configFile.Bind(
                "Character Positions",
                characterName,
                "-20,60",
                $"UI position for character {characterName} (format: x,y)"
            );
        }
        
        characterPositions[characterName].Value = $"{position.x},{position.y}";
        configFile.Save();
    }
}
