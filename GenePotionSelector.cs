using BepInEx;
using HarmonyLib;

namespace GenePotionSelector;

internal static class ModInfo
{
  internal const string Guid = "me.pocke.gene-potion-selector";
  internal const string Name = "Gene Potion Selector";
  internal const string Version = "1.0.1";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
internal class GenePotionSelector : BaseUnityPlugin
{
  internal static GenePotionSelector Instance { get; private set; }
  internal static bool IsExecuting { get; set; } = false;

  public void Awake()
  {
    Instance = this;

    var harmony = new Harmony(ModInfo.Guid);
    harmony.PatchAll();
  }

  public static void Log(object message)
  {
    Instance.Logger.LogInfo(message);
  }
}
