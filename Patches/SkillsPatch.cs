using HarmonyLib;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// Patch to track skill raises and calculate progress percentage
[HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
public static class SkillsRaisePatch
{
    private static FieldInfo skillDataField = null;

    static void Prefix(Skills __instance, Skills.SkillType skillType)
    {
        // Initialize reflection on first call
        if (skillDataField == null)
        {
            skillDataField = typeof(Skills).GetField("m_skillData", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    static void Postfix(Skills __instance, Skills.SkillType skillType, float factor = 1f)
    {
        // Wait one frame to ensure skill value is updated
        SkillActivityTracker.Instance.StartCoroutine(UpdateSkillAfterFrame(__instance, skillType));
    }

    private static System.Collections.IEnumerator UpdateSkillAfterFrame(Skills skills, Skills.SkillType skillType)
    {
        yield return null; // Wait one frame
        
        // Get updated skill data
        if (skillDataField == null)
        {
            skillDataField = typeof(Skills).GetField("m_skillData", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        if (skillDataField != null)
        {
            var skillData = skillDataField.GetValue(skills) as Dictionary<Skills.SkillType, Skills.Skill>;
            if (skillData != null && skillData.ContainsKey(skillType))
            {
                var skill = skillData[skillType];
                
                // Use GetLevelPercentage() method from Skills.Skill class
                float percentProgress = 0f;
                try
                {
                    var getLevelPercentageMethod = skill.GetType().GetMethod("GetLevelPercentage", BindingFlags.Public | BindingFlags.Instance);
                    if (getLevelPercentageMethod != null)
                    {
                        float rawPercentage = (float)getLevelPercentageMethod.Invoke(skill, null);
                        // GetLevelPercentage() returns 0-1, multiply by 100
                        percentProgress = rawPercentage * 100f;
                    }
                    else
                    {
                        // Fallback: use fractional part of level
                        float level = skill.m_level;
                        int wholeLevel = (int)level;
                        percentProgress = (level - wholeLevel) * 100f;
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error getting skill percentage: {ex.Message}");
                }
                
                string localizedName = GetLocalizedSkillName(skillType);
                
                // Pass level and calculated percentage
                SkillActivityTrackerLogger.LogSkill(skillType, skill.m_level, percentProgress, localizedName);
            }
        }
    }

    private static string GetLocalizedSkillName(Skills.SkillType skillType)
    {
        try
        {
            // Use Localization from Valheim
            string skillKey = "$skill_" + skillType.ToString().ToLower();
            
            // Find Localization class in assemblies
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var localizationType = assembly.GetType("Localization");
                if (localizationType != null)
                {
                    var instanceProp = localizationType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                    if (instanceProp != null)
                    {
                        var instance = instanceProp.GetValue(null);
                        if (instance != null)
                        {
                            var localizeMethod = localizationType.GetMethod("Localize", new[] { typeof(string) });
                            if (localizeMethod != null)
                            {
                                string result = localizeMethod.Invoke(instance, new object[] { skillKey }) as string;
                                if (!string.IsNullOrEmpty(result) && result != skillKey)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"Failed to localize skill {skillType}: {ex.Message}");
        }
        
        // Fallback to enum name
        return skillType.ToString();
    }
}
