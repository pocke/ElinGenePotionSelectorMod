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
    public Element Skill { get; set; }
    public Element Attribute { get; set; }
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

  public bool CanChoose()
  {
    if (Remaining() <= 0)
    {
      return false;
    }

    return featCandidates().Any() || abilityCandidates().Any() || slotCandidates().Any() || skillCandidates().Any() || attributeCandidates().Any();
  }

  public int Remaining()
  {
    return maxMods() - mods.Count;
  }

  public List<Element> featCandidates()
  {
    if (!canAddSpecial())
    {
      return new List<Element>();
    }

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
    if (!canAddSpecial())
    {
      return new List<ActList.Item>();
    }

    return chara.ability.list.items
      // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/7517ec09aaec867bffa504b0064b37675851a609/Elin/DNA.cs#L337-L338
      .Where(a => a.act.source.category == "ability")
      .Where(a => !mods.Any(m => m.Ability?.act?.id == a.act.id)).ToList();
  }

  public List<BodySlot> slotCandidates()
  {
    if (!canAddSpecial())
    {
      return new List<BodySlot>();
    }

    if (mods.Any(m => m.Slot != null))
    {
      return new List<BodySlot>();
    }
    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/7517ec09aaec867bffa504b0064b37675851a609/Elin/DNA.cs#L357
    return chara.body.slots.Where(s => s.elementId != 40 && s.elementId != 44).ToList();
  }

  public List<Element> skillCandidates()
  {
    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L298
    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L419
    var skills = chara.elements.ListBestSkills();
    return skills.Take(6).ToList();
  }

  public List<Element> attributeCandidates()
  {
    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L297
    // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L359
    return chara.elements.ListBestAttributes().Take(3).ToList();
  }

  public int maxMods()
  {
    return type == DNA.Type.Default ? 4 : 7;
  }

  public void SpawnGene()
  {
    var t = chara.MakeGene(type);
    Apply(t.c_DNA);
    chara.GiveBirth(t, effect: true);
  }

  private bool canAddSpecial()
  {
    int n = 0;
    mods.ForEach(mod =>
    {
      if (mod.Ability != null || mod.Slot != null || mod.Feat != null)
      {
        n++;
      }
    });
    return n < (type == DNA.Type.Default ? 1 : 2);
  }

  void Apply(DNA dna)
  {
    Reset();

    mods.ForEach(mod =>
    {
      if (mod.Feat != null)
      {
        AddVal(mod.Feat.id, 1, _ => mod.Feat.source.cost[0] * 5);
      }
      if (mod.Ability != null)
      {
        AddVal(mod.Ability.act.id, mod.Ability.pt ? -1 : 1, _ => 8 + mod.Ability.act.source.cost[0] / 10 * 2);
      }
      if (mod.Slot != null)
      {
        AddVal(mod.Slot.elementId, 1, _ => 20);
      }
      if (mod.Skill != null)
      {
        // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L420
        AddVal(mod.Skill.id, EClass.rndHalf(mod.Skill.ValueWithoutLink / 2), v => v / 5 + 1);
      }
      if (mod.Attribute != null)
      {
        // https://github.com/Elin-Modding-Resources/Elin-Decompiled/blob/dc99f554451266f493eec814d9f0f9aa55cdf170/Elin/DNA.cs#L360
        AddVal(mod.Attribute.id, EClass.rndHalf(mod.Attribute.ValueWithoutLink / 2), v => v / 5 + 1);
      }
    });

    dna.CalcCost();
    dna.CalcSlot();

    void AddVal(int id, int v, Func<int, int> funcCost)
    {
      var costBase = CurveWrapper.curve(v, 20, 10, 90);
      if (v < -100)
      {
        costBase = CurveWrapper.curve(Mathf.Abs(v + 100), 20, 10, 90);
      }

      v = CurveValue(v);
      for (int k = 0; k < dna.vals.Count; k += 2)
      {
        if (dna.vals[k] == id)
        {
          v /= 2;
          costBase /= 2;
          dna.vals[k + 1] += v;
          dna.cost += Mathf.Max(0, funcCost(costBase));
          return;
        }
      }

      if (v != 0)
      {
        dna.vals.Add(id);
        dna.vals.Add(v);
        dna.cost += Mathf.Max(0, funcCost(costBase));
      }
    }

    void Reset()
    {
      dna.vals.Clear();
      dna.cost = 0;
      dna.slot = 0;
    }
  }

  public int CurveValue(int v)
  {
    return CurveWrapper.curve(v, 20, 10, 80);
  }
}
