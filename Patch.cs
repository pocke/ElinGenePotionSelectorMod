using System;
using System.Security.Principal;
using HarmonyLib;
using UnityEngine;

namespace GenePotionSelector;

[HarmonyPatch]
public static class Patch
{
  [HarmonyPrefix, HarmonyPatch(typeof(ActEffect), nameof(ActEffect.GeneMiracle))]
  public static void ActEffect_GeneMiracle_Prefix()
  {
    // TODO: Display the selector

    GenePotionSelector.Log("GeneMiracle before");
    GenePotionSelector.DuringGeneMiracle = true;
  }

  [HarmonyPostfix, HarmonyPatch(typeof(ActEffect), nameof(ActEffect.GeneMiracle))]
  public static void ActEffect_GeneMiracle_Postfix()
  {
    GenePotionSelector.Log("GeneMiracle after");
    GenePotionSelector.DuringGeneMiracle = false;
  }

  [HarmonyPostfix, HarmonyPatch(typeof(DNA), nameof(DNA.Generate))]
  public static void DNA_Generate_Postfix(DNA __instance, DNA.Type _type)
  {
    if (!GenePotionSelector.DuringGeneMiracle)
    {
      return;
    }
    GenePotionSelector.Log("DNA.Generate during GeneMiracle");

    var dna = __instance;

    for (int i = dna.vals.Count - 1; 0 <= i; i -= 2)
    {
      GenePotionSelector.Log($"Creating Element from {i}");
      var ele = Element.Create(dna.vals[i - 1], dna.vals[i]);

      switch (ele.source.category)
      {
        case "slot":
        case "feat":
        case "ability":
          RemoveVal(i - 1, Cost(ele));
          break;
      }
    }

    // TODO: Add specified slot, feat, or ability
    //       The following code is just an example
    var addedEle = Element.Create(6603, 1);
    AddVal(6603, 1, Cost(addedEle));


    dna.CalcCost();
    dna.CalcSlot();

    void AddVal(int id, int v, int cost)
    {
      dna.vals.Add(id);
      dna.vals.Add(v);
      dna.cost += Mathf.Max(0, cost);
    }

    void RemoveVal(int i, int cost)
    {
      dna.vals.RemoveAt(i);
      dna.vals.RemoveAt(i);
      dna.cost -= cost;
    }

    int Cost(Element ele)
    {
      switch (ele.source.category)
      {
        case "slot":
          return 20;
        case "feat":
          return ele.source.cost[0] * 5;
        case "ability":
          return 8 + ele.source.cost[0] / 10 * 2;
        default:
          throw new Exception("Unexpected category: " + ele.source.category);
      }
    }
  }
}
