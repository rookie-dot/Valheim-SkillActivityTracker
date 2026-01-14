using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

public static class SkillActivityTrackerLogger
{
    public static ManualLogSource Log;

    private static List<TrackedSkill> trackedSkills = new List<TrackedSkill>();
    private const int MaxSkills = 5;
    private static float lastCheckTime = 0f;

    public static void Init(ManualLogSource logger)
    {
        Log = logger;
    }

    public static void LogSkill(Skills.SkillType skill, float level, float percentProgress, string localizedName = null)
    {
        // Don't track if skill is at level 100 or above
        if (level >= 100f)
        {
            return;
        }

        // Don't track if skill tracking is disabled in config
        if (!SkillActivityTrackerConfig.IsSkillTrackingEnabled(skill))
        {
            return;
        }

        DateTime now = DateTime.Now;

        // Find skill in list
        var existing = trackedSkills.FirstOrDefault(s => s.Skill == skill);
        if (existing != null)
        {
            existing.Uses++;
            existing.Level = level;
            existing.PercentProgress = percentProgress;
            existing.LastUsed = now;
            if (!string.IsNullOrEmpty(localizedName))
                existing.LocalizedName = localizedName;
        }
        else
        {
            trackedSkills.Add(new TrackedSkill
            {
                Skill = skill,
                Uses = 1,
                Level = level,
                PercentProgress = percentProgress,
                LastUsed = now,
                LocalizedName = localizedName ?? skill.ToString()
            });
        }

        // Remove expired skills
        RemoveExpiredSkills();

        // Sort by usage count (descending)
        trackedSkills = trackedSkills.OrderByDescending(s => s.Uses).ToList();

        // Update UI - display only top 5 most used skills
        SkillActivityTrackerUI.UpdateUI(trackedSkills.Take(MaxSkills).ToList());
    }

    // Check for expired skills - called every frame
    public static void CheckExpiredSkills()
    {
        // Check only once per second, not every frame
        if (UnityEngine.Time.time - lastCheckTime < 1f)
            return;

        lastCheckTime = UnityEngine.Time.time;

        if (RemoveExpiredSkills())
        {
            // Re-sort after removal
            trackedSkills = trackedSkills.OrderByDescending(s => s.Uses).ToList();
            
            // Refresh UI
            SkillActivityTrackerUI.UpdateUI(trackedSkills);
        }
    }

    private static bool RemoveExpiredSkills()
    {
        DateTime now = DateTime.Now;
        int countBefore = trackedSkills.Count;
        
        TimeSpan expireTime = SkillActivityTrackerConfig.GetDisplayDuration();
        trackedSkills.RemoveAll(s => (now - s.LastUsed) > expireTime);
        
        return countBefore != trackedSkills.Count;
    }

    public static List<TrackedSkill> GetTrackedSkills()
    {
        return trackedSkills;
    }

    public static void ClearAllSkills()
    {
        trackedSkills.Clear();
        SkillActivityTrackerUI.UpdateUI(trackedSkills);
    }

    public class TrackedSkill
    {
        public Skills.SkillType Skill;
        public int Uses;
        public float Level;
        public float PercentProgress; // Progress 0-100% to next level (calculated)
        public DateTime LastUsed;
        public string LocalizedName;
    }
}
