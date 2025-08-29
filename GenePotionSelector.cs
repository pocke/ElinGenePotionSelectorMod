using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace GenePotionSelector;

internal static class ModInfo
{
  internal const string Guid = "me.pocke.gene-potion-selector";
  internal const string Name = "Gene Potion Selector";
  internal const string Version = "1.0.0";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
internal class GenePotionSelector : BaseUnityPlugin
{
  internal static GenePotionSelector Instance { get; private set; }
  internal static bool DuringGeneMiracle { get; set; }

  public void Awake()
  {
    Instance = this;

    new Harmony(ModInfo.Guid).PatchAll();
  }

  public static void Log(object message)
  {
    Instance.Logger.LogInfo(message);
  }
}
