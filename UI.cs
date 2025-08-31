using System;
using System.Collections.Generic;
using System.Linq;
using YKF;

namespace GenePotionSelector;

public class SelectionLayerData
{
  public List<Element> Feats { get; }
  public List<ActList.Item> Abilities { get; }
  public List<BodySlot> Slots { get; }

  public Action<GeneModifier.Mod> OnSelect { get; }

  public SelectionLayerData(List<Element> feats, List<ActList.Item> abilities, List<BodySlot> slots, Action<GeneModifier.Mod> onSelect)
  {
    Feats = feats;
    Abilities = abilities;
    Slots = slots;
    OnSelect = onSelect;
  }
}

public class SelectorLayer : YKLayer<SelectionLayerData>
{
  public override void OnLayout()
  {
    CreateTab<SelectorTab>("遺伝子に付与したい特性を選択"._("Select traits to add to the gene"), $"{ModInfo.Guid}.selector-tab");
  }
}

public class SelectorTab : YKLayout<SelectionLayerData>
{
  public override void OnLayout()
  {
    if (Layer.Data.Feats.Count > 0)
    {
      HeaderSmall("フィート"._("Feats"));
      Layer.Data.Feats.ForEach(ele =>
      {
        var group = Horizontal();
        group.Button("選択"._("Select"), () =>
        {
          Layer.Close();
          Layer.Data.OnSelect(new GeneModifier.Mod { Feat = ele });
        });
        group.Text(ele.Name);
      });
    }

    if (Layer.Data.Abilities.Count > 0)
    {
      HeaderSmall("アビリティ"._("Abilities"));
      Layer.Data.Abilities.ForEach(act =>
      {
        var group = Horizontal();
        group.Button("選択"._("Select"), () =>
        {
          Layer.Close();
          Layer.Data.OnSelect(new GeneModifier.Mod { Ability = act });
        });
        group.Text(act.act.Name);
      });
    }

    if (Layer.Data.Slots.Count > 0)
    {
      HeaderSmall("部位"._("Body parts"));
      Layer.Data.Slots.GroupBy(s => s.name).Select(s => s.First()).ToList().ForEach(slot =>
      {
        var group = Horizontal();
        group.Button("選択"._("Select"), () =>
        {
          Layer.Close();
          Layer.Data.OnSelect(new GeneModifier.Mod { Slot = slot });
        });
        group.Text(slot.name);
      });
    }
  }
}
