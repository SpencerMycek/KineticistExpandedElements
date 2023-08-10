using CodexLib;
using KineticistElementsExpanded.Components;
using KineticistElementsExpanded.Components.Properties;
using KineticistElementsExpanded.KineticLib;
using AnyRef = CodexLib.AnyRef;
using Helper = CodexLib.Helper;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.Kineticist.ActivatableAbility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature;
using static Kingmaker.UnitLogic.Mechanics.Properties.BlueprintUnitProperty;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using BlueprintCore.Utils;
using System.Linq;
using Kingmaker.UnitLogic.Buffs.Components;

namespace KineticistElementsExpanded.ElementGeneral
{
    class General
    {
        private static readonly KineticistTree Tree = KineticistTree.Instance;

        private static BlueprintBuff EESuperchargeBuff;
        private static BlueprintFeature EEAdditionalEffects7;
        private static BlueprintFeature EEAdditionalEffects13;

        public static void Configure()
        {
            Main.Print("[General][Elemental Engine Additions]");
            CreateElementalEngineAdditions();
        }

        private static void CreateElementalEngineAdditions()
        {
            EESuperchargeBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e0fd464ef0e040a9b609cab6c2580acd"); // ElementalEngineBurnoutSuperchargeBuff
            EEAdditionalEffects7 = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("fd30841e0ede46769708367be0c01a49"); // ElementalEngineAdditionalEffectsFeature7
            EEAdditionalEffects13 = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0e245032368c4cb9bc1a3251c669acf8"); // ElementalEngineAdditionalEffectsFeature13
            Main.Print("[General][EE - Aether]");
            CreateEEAAether();
            Main.Print("[General][EE - Void]");
            CreateEEAVoid();
            Main.Print("[General][EE - Wood]");
            CreateEEAWood();
        }

        private static void CreateEEAAether()
        {
            //+2 AC 50% fort
            //+4 AC Incorp
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath + "/Icons/touchsite.png");
            var fortification50 = new AddFortification
            {
                Bonus=50,
                UseContextValue=false
            };
            var incorporeal = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c4a7f98d743bc784c9d4cf2105852c39"); // Incorporeal

            #region Buff 7
            var buff7 = Helper.CreateBlueprintBuff("ElementalEngineAether7",
                LocalizationTool.GetString("EEA.Aether.7.Name"), LocalizationTool.GetString("EEA.Aether.7.Description"),
                icon).Flags(hidden: false);
            buff7.SetComponents
                (
                    Helper.CreateAddStatBonus(2, StatType.AC, ModifierDescriptor.Armor),
                    fortification50
                );
            var feature7 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsAether7");
            feature7.HideInUI = true;
            feature7.HideInCharacterSheetAndLevelUp = true;
            feature7.HideNotAvailibleInUI = true;
            feature7.SetComponents(new BuffExtraEffects { 
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff), 
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff7)
            });
            #endregion
            #region Buff 13
            var buff13 = Helper.CreateBlueprintBuff("ElementalEngineAether13",
                 LocalizationTool.GetString("EEA.Aether.13.Name"), LocalizationTool.GetString("EEA.Aether.13.Description"),
                 icon).Flags(hidden: false);
            buff13.SetComponents
                (
                    Helper.CreateAddStatBonus(4, StatType.AC, ModifierDescriptor.Armor),
                    Helper.CreateAddFacts(incorporeal)
                );
            var feature13 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsAether13");
            feature13.HideInUI = true;
            feature13.HideInCharacterSheetAndLevelUp = true;
            feature13.HideNotAvailibleInUI = true;
            feature13.SetComponents(new BuffExtraEffects
            {
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff),
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff13)
            });
            #endregion
            EEAdditionalEffects7.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusAether.First, blueprintUnitFact: feature7));
            EEAdditionalEffects13.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusAether.First, blueprintUnitFact: feature13));
        }

        private static void CreateEEAVoid()
        {
            //+2 vDeath 20% conceal
            //+4 vDeath 50% conceal
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath + "/Icons/emptiness.png");
            var miss20 = new SetAttackerMissChance { Value = Helper.CreateContextValue(20) };
            var miss50 = new SetAttackerMissChance { Value = Helper.CreateContextValue(50) };

            #region Buff 7
            var buff7 = Helper.CreateBlueprintBuff("ElementalEngineVoid7",
                LocalizationTool.GetString("EEA.Void.7.Name"), LocalizationTool.GetString("EEA.Void.7.Description"),
                icon).Flags(hidden: false);
            buff7.SetComponents
                (
                    new SavingThrowBonusAgainstDescriptor { Value = 2, SpellDescriptor = SpellDescriptor.Death },
                    miss20
                );
            var feature7 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsVoid7");
            feature7.HideInUI = true;
            feature7.HideInCharacterSheetAndLevelUp = true;
            feature7.HideNotAvailibleInUI = true;
            feature7.SetComponents(new BuffExtraEffects
            {
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff),
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff7)
            });
            #endregion
            #region Buff 13
            var buff13 = Helper.CreateBlueprintBuff("ElementalEngineVoid13",
                 LocalizationTool.GetString("EEA.Void.13.Name"), LocalizationTool.GetString("EEA.Void.13.Description"),
                 icon).Flags(hidden: false);
            buff13.SetComponents
                (
                    new SavingThrowBonusAgainstDescriptor { Value = 4, SpellDescriptor = SpellDescriptor.Death },
                    miss50
                );
            var feature13 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsVoid13");
            feature13.HideInUI = true;
            feature13.HideInCharacterSheetAndLevelUp = true;
            feature13.HideNotAvailibleInUI = true;
            feature13.SetComponents(new BuffExtraEffects
            {
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff),
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff13)
            });
            #endregion
            EEAdditionalEffects7.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusVoid.First, blueprintUnitFact: feature7));
            EEAdditionalEffects13.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusVoid.First, blueprintUnitFact: feature13));
        }

        private static void CreateEEAWood()
        {
            //+2/4 to saves against poison and disease effects, half of kineticist level as N/slashing / half of kineticist level as fast healing
            //PoisonResistance c9022272c87bd66429176ce5c597989c - feature for poison saves from alchemist i.e. But there's many of them
            //MartyrAuraOfHealthFeature caa8099018a01b74298aecb560b21cdc -feature with disease saves
            //ArcanistExploitWoodenFleshAbility 951d71f338f79944494b69d67fe9eff5 - n/slashing
            //MythicPowersFromDLC1Ability cf6133b66ec64f078d76171ba316ce9f - part of this gains fortification and fast healing
            //+2 vPoison/Disease N/slash
            //+4 vPoison/Disease Fast healing
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath + "/Icons/woodblast.png");
            var half_levels = Helper.CreateContextRankConfig(
                ContextRankBaseValueType.ClassLevel, 
                ContextRankProgression.Div2, 
                AbilityRankType.Default, 
                max: 20,
                classes: new BlueprintCharacterClassReference[] { Tree.Class });
            var dr = new AddDamageResistancePhysical 
            { 
                BypassedByForm = true, Form = PhysicalDamageForm.Slashing, 
                Value = Helper.CreateContextValue(AbilityRankType.Default) 
            };
            var healing = new AddEffectFastHealing { Heal = 0, Bonus = Helper.CreateContextValue(AbilityRankType.Default) };
            #region Buff 7
            var buff7 = Helper.CreateBlueprintBuff("ElementalEngineWood7",
                LocalizationTool.GetString("EEA.Wood.7.Name"), LocalizationTool.GetString("EEA.Wood.7.Description"),
                icon).Flags(hidden: false);
            buff7.SetComponents
                (
                    new SavingThrowBonusAgainstDescriptor { Value = 2, SpellDescriptor = SpellDescriptor.Poison },
                    new SavingThrowBonusAgainstDescriptor { Value = 2, SpellDescriptor = SpellDescriptor.Disease },
                    dr
                );
            var feature7 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsWood7");
            feature7.HideInUI = true;
            feature7.HideInCharacterSheetAndLevelUp = true;
            feature7.HideNotAvailibleInUI = true;
            feature7.SetComponents(new BuffExtraEffects
            {
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff),
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff7)
            });
            #endregion
            #region Buff 13
            var buff13 = Helper.CreateBlueprintBuff("ElementalEngineWood13",
                 LocalizationTool.GetString("EEA.Wood.13.Name"), LocalizationTool.GetString("EEA.Wood.13.Description"),
                 icon).Flags(hidden: false);
            buff13.SetComponents
                (
                    new SavingThrowBonusAgainstDescriptor { Value = 4, SpellDescriptor = SpellDescriptor.Poison },
                    new SavingThrowBonusAgainstDescriptor { Value = 2, SpellDescriptor = SpellDescriptor.Disease },
                    healing
                );
            var feature13 = Helper.CreateBlueprintFeature("ElementalEngineAdditionalEffectsWood13");
            feature13.HideInUI = true;
            feature13.HideInCharacterSheetAndLevelUp = true;
            feature13.HideNotAvailibleInUI = true;
            feature13.SetComponents(new BuffExtraEffects
            {
                m_CheckedBuff = AnyRef.ToRef<BlueprintBuffReference>(EESuperchargeBuff),
                m_ExtraEffectBuff = AnyRef.ToRef<BlueprintBuffReference>(buff13)
            });
            #endregion
            EEAdditionalEffects7.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusWood.First, blueprintUnitFact: feature7));
            EEAdditionalEffects13.AddComponents(Helper.CreateAddFeatureIfHasFact(check_BlueprintUnitFact: Tree.FocusWood.First, blueprintUnitFact: feature13));

        }
    }
}
