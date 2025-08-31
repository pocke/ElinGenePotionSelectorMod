using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using YKF;

namespace GenePotionSelector;

[HarmonyPatch]
public static class Patch
{
  [HarmonyPrefix, HarmonyPatch(typeof(ActEffect), nameof(ActEffect.GeneMiracle))]
  public static bool ActEffect_GeneMiracle_Prefix(Chara tc, DNA.Type type)
  {
    if (type != DNA.Type.Default && type != DNA.Type.Superior)
    {
      return true;
    }

    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/72332a1390e68a8de62bca4acbd6ebbaab92257b/Elin/ActEffect.cs#L2362-L2366
    if (EClass._zone.IsUserZone && !tc.IsPCFactionOrMinion)
    {
      Msg.SayNothingHappen();
      return false;
    }

    int n = type == DNA.Type.Default ? 1 : 2;
    var m = new GeneModifier(tc, type);

    chooseMod(n, m, tc);

    return false;
  }

  private static void chooseMod(int n, GeneModifier m, Chara tc)
  {
    GenePotionSelector.IsExecuting = true;

    Action<GeneModifier.Mod> onSelect = mod =>
    {
      m.AddMod(mod);
      n--;

      if (n == 0)
      {
        // TODO: call tc.Say
        m.SpawnGene();
        GenePotionSelector.IsExecuting = false;
      }
      else
      {
        chooseMod(n, m, tc);
      }
    };

    // TODO: onCancel
    var layerData = new SelectionLayerData(m.featCandidates(), m.abilityCandidates(), m.slotCandidates(), onSelect);
    YK.CreateLayer<SelectorLayer, SelectionLayerData>(layerData);
  }

  // Prevent updating Act.TC static field during ActThrow.Throw.
  // Opening a new layer causes mouse move events and they triggers Act.CanPerform.
  // This method updates Act.TC, and it breaks the Throw method.
  [HarmonyPrefix, HarmonyPatch(typeof(Act), nameof(Act.CanPerform), new Type[] { typeof(Chara), typeof(Card), typeof(Point) })]
  public static bool Act_CanPerform_Prefix()
  {
    return !GenePotionSelector.IsExecuting;
  }
}
