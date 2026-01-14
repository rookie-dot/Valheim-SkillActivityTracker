using BepInEx.Configuration;
using System;
using System.Collections.Generic;

public static class SkillActivityTrackerConfig
{
    // Dictionary to store if skill should be tracked (true = display, false = hide)
    public static Dictionary<Skills.SkillType, ConfigEntry<bool>> SkillTrackingEnabled = new Dictionary<Skills.SkillType, ConfigEntry<bool>>();
    
    // Display duration in minutes
    public static ConfigEntry<float> DisplayDurationMinutes;

    public static void Initialize(ConfigFile config)
    {
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
}
