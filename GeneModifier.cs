using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenePotionSelector;

public class GeneModifier
{
  public class Mod
  {
    public Element Feat { get; set; }
    public ActList.Item Ability { get; set; }
    public BodySlot Slot { get; set; }
  }

  Chara chara;
  DNA.Type type;
  List<Mod> mods = new List<Mod>();

  public GeneModifier(Chara chara, DNA.Type type)
  {
    this.chara = chara;
    this.type = type;
  }

  public void AddMod(Mod mod)
  {
    mods.Add(mod);
  }

  public List<Element> featCandidates()
  {
    int geneSlot = mods.Any() ? mods.Max(m => m.Feat?.source?.geneSlot ?? 1) : 1;
    var ret = chara.elements.ListGeneFeats().Where(e => !mods.Any(m => m.Feat?.id == e.id));
    if (geneSlot > 1)
    {
      ret = ret.Where(e => e.source.geneSlot <= 1);
    }
    return ret.ToList();
  }

  public List<ActList.Item> abilityCandidates()
  {
    return chara.ability.list.items.Where(a => !mods.Any(m => m.Ability?.act?.id == a.act.id)).ToList();
  }

  public List<BodySlot> slotCandidates()
  {
    if (mods.Any(m => m.Slot != null))
    {
      return new List<BodySlot>();
    }
    return chara.body.slots;
  }

  public void SpawnGene()
  {
    var t = chara.MakeGene(type);
    Apply(t.c_DNA);
    chara.GiveBirth(t, effect: true);
  }

  void Apply(DNA dna)
  {
    for (int i = dna.vals.Count - 1; 0 <= i; i -= 2)
    {
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

    mods.ForEach(mod =>
    {
      if (mod.Feat != null)
      {
        AddVal(mod.Feat.id, 1, Cost(mod.Feat));
      }
      if (mod.Ability != null)
      {
        AddVal(mod.Ability.act.id, mod.Ability.pt ? -1 : 1, Cost(mod.Ability.act));
      }
      if (mod.Slot != null)
      {
        AddVal(mod.Slot.elementId, 1, Cost(Element.Create(mod.Slot.elementId, 1)));
      }
    });

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
