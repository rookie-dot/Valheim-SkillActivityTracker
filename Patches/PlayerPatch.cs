using HarmonyLib;

// Patch to clear tracked skills on player logout
[HarmonyPatch(typeof(Game), nameof(Game.Logout))]
public static class GameLogoutPatch
{
    static void Prefix()
    {
        // Clear all tracked skills on logout
        SkillActivityTrackerLogger.ClearAllSkills();
    }
}
