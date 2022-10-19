using BlueprintCore.Utils;
using CodexLib;
using KineticistElementsExpanded.Components;
using KineticistElementsExpanded.Components.Properties;
using KineticistElementsExpanded.KineticLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.TempMapCode.Ambush;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
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
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Kingmaker.Blueprints.BlueprintUnitTemplate;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using System;

namespace KineticistElementsExpanded.ElementWood
{
    class Wood
    {
        // Idea to bring Ghoran race as a dryad or something
        
        public static KineticistTree Tree = KineticistTree.Instance;

        public static KineticistTree.Infusion PositiveAdmixture = new();

        public static void Configure()  
        {
            BlueprintFeatureBase wood_class_skills = CreateWoodClassSkills();

            CreateInfusions();

            CreateWoodBlastsSelection();
            CreateCompositeBlasts();

            Kineticist.AddElementsToInfusion(Tree.Spore, Tree.Wood, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);
            Kineticist.AddElementsToInfusion(Tree.Toxic, Tree.Wood, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);
            Kineticist.AddElementsToInfusion(Tree.ToxicGreater, Tree.Wood, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);

            BlueprintFeature flesh_of_wood_feature = CreateFleshofWood();

            Kineticist.AddElementalDefenseIsPrereqFor(Tree.Positive.BlastFeature, Tree.Positive.Blade.Feature, flesh_of_wood_feature);
            Kineticist.AddElementalDefenseIsPrereqFor(Tree.Wood.BlastFeature, Tree.Wood.Blade.Feature, flesh_of_wood_feature);

            Kineticist.ElementsBlastSetup(Tree.Positive, Tree.Wood, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);

            EntanglePushInfusions(Tree.Wood, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);
            DazzleInfusion(Tree.Positive, Tree.Composite_Verdant);
            FoxfireInfusions(Tree.Positive);

            Kineticist.AddCompositeToBuff(Tree, Tree.Composite_Autumn, Tree.Wood, Tree.Earth);
            Kineticist.AddCompositeToBuff(Tree, Tree.Composite_Spring, Tree.Wood, Tree.Air);
            Kineticist.AddCompositeToBuff(Tree, Tree.Composite_Summer, Tree.Wood, Tree.Fire);
            Kineticist.AddCompositeToBuff(Tree, Tree.Composite_Winter, Tree.Wood, Tree.Cold);

            Kineticist.AddAdmixtureToBuff(Tree, PositiveAdmixture, Tree.Positive, true, true, false);
            Kineticist.AddBladesToKineticWhirlwind(Tree.Wood, Tree.Positive, Tree.Composite_Verdant, Tree.Composite_Autumn, Tree.Composite_Spring, Tree.Composite_Summer, Tree.Composite_Winter);


            CreateWoodElementalFocus(wood_class_skills, flesh_of_wood_feature);
            CreateKineticKnightWoodFocus(wood_class_skills, flesh_of_wood_feature);
            CreateSecondElementWood();
            CreateThirdElementWood();

            CreateWoodWildTalents(flesh_of_wood_feature);
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateWoodClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("WoodClassSkills", LocalizationTool.GetString("Wood.Skills.Name"),
                LocalizationTool.GetString("Wood.Skills.Description"), null, 0)
                .SetComponents
                (
                Helper.CreateAddClassSkill(StatType.SkillLoreNature),
                Helper.CreateAddClassSkill(StatType.SkillKnowledgeWorld)
                );
            return feature;
        }

        private static void CreateWoodBlastsSelection()
        {
            CreatePositiveBlast();
            CreateWoodBlast();

            var selection = Helper.CreateBlueprintFeatureSelection("WoodBlastSelection", LocalizationTool.GetString("Wood.Selection.Name"),
                LocalizationTool.GetString("Wood.Selection.Description"), null, FeatureGroup.None, SelectionMode.Default);
            selection.IsClassFeature = true;

            Helper.AppendAndReplace<BlueprintFeatureReference>(ref selection.m_AllFeatures,
                AnyRef.ToAny(Tree.Positive.Progession),
                AnyRef.ToAny(Tree.Wood.Progession));

            Tree.Positive.Selection = AnyRef.ToAny(selection);
            Tree.Wood.Selection = Tree.Positive.Selection;
        }

        private static void EntanglePushInfusions(params KineticistTree.Element[] elements)
        {
            var pushingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("fbb97f35a41b71c4cbc36c5f3995b892"); // PushingInfusionFeature
            var pushingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f795bede8baefaf4d9d7f404ede960ba"); // PushingInfusionBuff
            var entanglingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("607539d018d03454aaac0a2c1522f7ac"); // EntanglingInfusionFeature
            var entanglingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("738120aad01eedb4f891eca5b784646a"); // EntanglingInfusionBuff

            foreach (var element in elements)
            {
                var prereq = pushingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);
                prereq = entanglingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);

                var applicable = pushingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);
                applicable = entanglingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);

                var trigger = pushingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
                trigger = entanglingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
            }
        }
        private static void DazzleInfusion(params KineticistTree.Element[] elements)
        {
            var dazzlingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("037460f7ae3e21943b237007f2b1a5d5"); // DazzlingInfusionFeature
            var dazzlingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("ee8d9f5631c53684d8d627d715eb635c"); // DazzlingInfusionBuff

            foreach (var element in elements)
            {
                var prereq = dazzlingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);

                var applicable = dazzlingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);

                var trigger = dazzlingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
            }
        }
        private static void FoxfireInfusions(params KineticistTree.Element[] elements)
        {
            var foxfireInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ae21c5369252ec74aa1fee89f1bc1b21"); // FoxfireInfusionFeature
            var foxfireInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e671f173fcb75bf4aa78a4078d075792"); // FoxfireInfusionBuff

            foreach (var element in elements)
            {
                var prereq = foxfireInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);

                var applicable = foxfireInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);

                var trigger = foxfireInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
            }
        }
        #endregion

        #region Elemental Focus Selection

        private static void CreateWoodElementalFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase flesh_of_wood)
        {
            var progression = Helper.CreateBlueprintProgression("ElementalFocusWood", LocalizationTool.GetString("Wood"),
                LocalizationTool.GetString("Wood.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Kineticist.ref_blood_kineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any Wood basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Tree.Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(2, flesh_of_wood);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateKineticKnightWoodFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            var progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusWood", LocalizationTool.GetString("Wood"),
                LocalizationTool.GetString("Wood.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Tree.Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(4, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateSecondElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("SecondaryElementWood", LocalizationTool.GetString("Wood"),
                LocalizationTool.GetString("Wood.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusWood.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusWood.Knight))
                        ),
                    new GameAction[] {
                        Helper.CreateAddFact(new FactOwner(), AnyRef.ToAny(Tree.Positive.BlastFeature)),
                        Helper.CreateAddFact(new FactOwner(), AnyRef.ToAny(Tree.Wood.BlastFeature)),
                        Helper.CreateAddFact(new FactOwner(),  AnyRef.ToAny(Tree.Composite_Verdant.BlastFeature))
                    }
                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(7, Tree.Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateThirdElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementWood", LocalizationTool.GetString("Wood"),
                LocalizationTool.GetString("Wood.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusWood.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusWood.Knight))),
                    new GameAction[] {
                        Helper.CreateAddFact(new FactOwner(), AnyRef.ToAny(Tree.Composite_Verdant.BlastFeature))
                    }
                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff)),
                Helper.CreatePrerequisiteNoFeature(AnyRef.ToAny(Tree.FocusWood.Second))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(15, Tree.Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        #endregion

        #region Flesh of Wood

        public static BlueprintFeature CreateFleshofWood()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("5b77d7cc65b8ab74688e74a37fc2f553"); // Barkskin

            #region Resource

            var resource = Helper.CreateBlueprintAbilityResource("FleshofWoodResource", "Flesh of Wood Resource",
                LocalizationTool.GetString("Wood.Defense.Description"), null, min: 0, max: 6, baseValue: 1, levelDiv: 3, levelMult: 1, startLevel: 2,
                classes: new BlueprintCharacterClassReference[] { Tree.Class });

            #endregion
            #region Effect Feature

            var effect_feature = Helper.CreateBlueprintFeature("FlashofWoodEffectFeature",
                null, null, icon, FeatureGroup.None);
            effect_feature.Ranks = 7;
            effect_feature.HideInUI = true;
            effect_feature.HideInCharacterSheetAndLevelUp = true;
            effect_feature.IsClassFeature = true;
            effect_feature.SetComponents
                (
                new AddFacts { }
                );

            #endregion
            #region Effect Buff

            var effect_buff = Helper.CreateBlueprintBuff("FleshofWoodEffectBuff", null,
                null, icon);
            effect_buff.Flags(hidden: true, stayOnDeath: true);
            effect_buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;
            effect_buff.Stacking = StackingType.Stack;
            effect_buff.IsClassFeature = true;
            effect_buff.SetComponents
                (
                Helper.CreateAddFacts(effect_feature.ToRef2())
                );

            #endregion
            #region Buff

            var buff = Helper.CreateBlueprintBuff("FleshofWoodBuff", null,
               null, icon);
            buff.Flags(hidden: true, stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.SetComponents
                (
                new AddContextStatBonus
                {
                    Descriptor = ModifierDescriptor.NaturalArmorEnhancement,
                    Stat = StatType.AC,
                    Multiplier = 1,
                    HasMinimal = true,
                    Minimal = 1,
                    Value = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.Default
                    }
                },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, progression: ContextRankProgression.BonusValue, stepLevel: 1, min: 1, max: 7, feature: effect_feature.ToRef()),
                Helper.CreateRecalculateOnFactsChange(effect_feature.ToRef2())
                );

            #endregion
            #region Ability

            var ability = Helper.CreateBlueprintAbility("FleshofWoodAbility", LocalizationTool.GetString("Wood.Defense.Button"),
                LocalizationTool.GetString("Wood.Defense.Description"), icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityResourceLogic(AnyRef.ToAny(resource), 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var feature = Helper.CreateBlueprintFeature("FleshofWoodFeature", LocalizationTool.GetString("Wood.Defense"),
                LocalizationTool.GetString("Wood.Defense.Description"), icon, FeatureGroup.None);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreateAddFacts(buff.ToRef2(), ability.ToRef2()),
                // Prereqs Positive/Wood Feature, Respective Blade features
                Helper.CreatePrerequisiteFeature(Tree.Positive.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Positive.Blade.Feature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Wood.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Wood.Blade.Feature, any: true),
                Helper.CreateAddAbilityResources(AnyRef.ToAny(resource))
                );

            return feature;
        }

        #endregion

        #region Positive Blast

        public static void CreatePositiveBlast()
        {
            // Variants
            var standard = CreatePositiveBlastVariant_base();
            //var blade = CreatePositiveBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Positive", "Positive", isComposite: false,
                "d739a9e236ba6164ab854b356bfb6ed5", Resource.Projectile.SunBeam00,
                Helper.CreateSprite(Main.ModPath + "/Icons/positiveBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/positiveBlast.png"),
                e: DamageEnergyType.PositiveEnergy);
            var extended = CreatePositiveBlastVariant_extended();
            var spindle = CreatePositiveBlastVariant_spindle();
            var wall = CreatePositiveBlastVariant_wall();
            // Ability
            CreatePositiveBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreatePositiveBlastFeature();
            // Progression
            CreatePositiveBlastProgression();

        }

        #region Positive Variants

        private static BlueprintAbility CreatePositiveBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/positiveBlast.png");

            var ability = Helper.CreateBlueprintAbility("PositiveBlastAbility", LocalizationTool.GetString("Wood.Positive.Name"),
                LocalizationTool.GetString("Wood.Positive.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.SunBeam00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreatePositiveBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangePositiveBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.SunBeam00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Positive.BaseAbility;

            return ability;
        }
        private static BlueprintAbility CreatePositiveBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindlePositiveBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.SunBeam00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.SunBeam00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Positive.BaseAbility;

            return ability;
        }
        private static BlueprintAbility CreatePositiveBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.DamageBonus
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Positive", "098a29fefbbc4564281afa5a6887cd2c", e: DamageEnergyType.PositiveEnergy),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallPositiveBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.SpellResistance = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Positive.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreatePositiveBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/positiveBlast.png");

            var ability = Helper.CreateBlueprintAbility("PositiveBlastBase", LocalizationTool.GetString("Wood.Positive.Name"),
                LocalizationTool.GetString("Wood.Positive.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreatePositiveBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("PositiveBlastFeature", LocalizationTool.GetString("Wood.Positive.Name"),
                LocalizationTool.GetString("Wood.Positive.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Positive.BaseAbility))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        public static void CreatePositiveBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("PositiveBlastProgression", LocalizationTool.GetString("Wood.Positive.Name"),
                LocalizationTool.GetString("Wood.Positive.Description"), null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Positive.Blade.Feature)),
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Positive.BlastFeature))
                );

            var entry = Helper.CreateLevelEntry(1, Tree.Positive.BlastFeature);
            Helper.AddEntries(progression, entry);
        }

        #endregion

        #region Wood Blast

        public static void CreateWoodBlast()
        {
            // Variants
            var standard = CreateWoodBlastVariant_base();
            //var blade = CreateWoodBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Wood", "Wood", isComposite: false,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.SunBeam00,
                Helper.CreateSprite(Main.ModPath + "/Icons/woodBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/woodBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing);
            var extended = CreateWoodBlastVariant_extended();
            var spindle = CreateWoodBlastVariant_spindle();
            var wall = CreateWoodBlastVariant_wall();
            // Ability
            CreateWoodBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateWoodBlastFeature();
            // Progression
            CreateWoodBlastProgression();

            //entangling substance
            //pushing substance
        }

        #region Wood Variants

        private static BlueprintAbility CreateWoodBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/woodBlast.png");

            var ability = Helper.CreateBlueprintAbility("WoodBlastAbility", LocalizationTool.GetString("Wood.Wood.Name"),
                LocalizationTool.GetString("Wood.Wood.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWoodBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeWoodBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Wood.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWoodBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleWoodBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.Vinetrap00_Projectile_1.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.Vinetrap00_Projectile_1.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Wood.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWoodBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Wood", "1f26aacd3ad314e4c820d4fe2ac3fd46", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallWoodBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Wood.BaseAbility;

            return ability;
        }
        
        //impale DarkCodex

        #endregion

        public static void CreateWoodBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/woodBlast.png");

            var ability = Helper.CreateBlueprintAbility("WoodBlastBase", LocalizationTool.GetString("Wood.Wood.Name"),
                LocalizationTool.GetString("Wood.Wood.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateWoodBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("WoodBlastFeature", LocalizationTool.GetString("Wood.Wood.Name"),
                LocalizationTool.GetString("Wood.Wood.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Wood.BaseAbility))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        public static void CreateWoodBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("WoodBlastProgression", LocalizationTool.GetString("Wood.Wood.Name"),
                LocalizationTool.GetString("Wood.Wood.Description"), null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Wood.Blade.Feature)),
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Wood.BlastFeature))
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.ToAny(Tree.Wood.BlastFeature));
            Helper.AddEntries(progression, entry);        }

        #endregion

        #region Composite Blasts

        private static void CreateCompositeBlasts()
        {
            CreateVerdantBlast();
            CreateAutumnBlast();
            CreateSpringBlast();
            CreateSummerBlast();
            CreateWinterBlast();
            CreatePositiveAdmixture();
        }

        #region Verdant

        public static void CreateVerdantBlast()
        {
            // Variants
            var standard = CreateVerdantBlastVariant_base();
            //var blade = CreateVerdantBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Verdant", "Verdant", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.SunBeam00,
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                e: DamageEnergyType.PositiveEnergy);
            var extended = CreateVerdantBlastVariant_extended();
            var spindle = CreateVerdantBlastVariant_spindle();
            var wall = CreateVerdantBlastVariant_wall();
            // Ability
            CreateVerdantBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateVerdantBlastFeature();

            //Dazzling
            //Entangling
            //Pushing
        }

        #region Verdant Variants

        private static BlueprintAbility CreateVerdantBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("VerdantBlastAbility", LocalizationTool.GetString("Wood.Verdant.Name"),
                LocalizationTool.GetString("Wood.Verdant.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateVerdantBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeVerdantBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Verdant.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateVerdantBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleVerdantBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.Vinetrap00_Projectile_1.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.Vinetrap00_Projectile_1.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Verdant.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateVerdantBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.DamageBonus
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Verdant", "098a29fefbbc4564281afa5a6887cd2c", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallVerdantBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Verdant.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateVerdantBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("VerdantBlastBase", LocalizationTool.GetString("Wood.Verdant.Name"),
                LocalizationTool.GetString("Wood.Verdant.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateVerdantBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("VerdantBlastFeature", LocalizationTool.GetString("Wood.Verdant.Name"),
                LocalizationTool.GetString("Wood.Verdant.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Verdant.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Composite_Verdant.Blade.Feature))
                );
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Autumn

        public static void CreateAutumnBlast()
        {
            // Variants
            var standard = CreateAutumnBlastVariant_base();
            //var blade = CreateAutumnBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Autumn", "Autumn", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.Kinetic_EarthBlast00_Projectile,
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing);
            var extended = CreateAutumnBlastVariant_extended();
            var spindle = CreateAutumnBlastVariant_spindle();
            var wall = CreateAutumnBlastVariant_wall();
            // Ability
            CreateAutumnBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateAutumnBlastFeature();

            //Entangling
            //Pushing
        }

        #region Autumn Variants

        private static BlueprintAbility CreateAutumnBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("AutumnBlastAbility", LocalizationTool.GetString("Wood.Autumn.Name"),
                LocalizationTool.GetString("Wood.Autumn.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateAutumnBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeAutumnBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Autumn.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateAutumnBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleAutumnBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.Kinetic_EarthBlast00_Projectile.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.Kinetic_EarthBlast00_Projectile.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Autumn.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateAutumnBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Autumn", "1f26aacd3ad314e4c820d4fe2ac3fd46", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, twice: true),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallAutumnBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Autumn.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateAutumnBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("AutumnBlastBase", LocalizationTool.GetString("Wood.Autumn.Name"),
                LocalizationTool.GetString("Wood.Autumn.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateAutumnBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("AutumnBlastFeature", LocalizationTool.GetString("Wood.Autumn.Name"),
                LocalizationTool.GetString("Wood.Autumn.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Autumn.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Composite_Autumn.Blade.Feature))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Spring
        public static void CreateSpringBlast()
        {
            // Variants
            var standard = CreateSpringBlastVariant_base();
            //var blade = CreateSpringBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Spring", "Spring", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.WindProjectile00,
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing);
            var extended = CreateSpringBlastVariant_extended();
            var spindle = CreateSpringBlastVariant_spindle();
            var wall = CreateSpringBlastVariant_wall();
            // Ability
            CreateSpringBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateSpringBlastFeature();

            //Entangling
            //Pushing
        }

        #region Spring Variants

        private static BlueprintAbility CreateSpringBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SpringBlastAbility", LocalizationTool.GetString("Wood.Spring.Name"),
                LocalizationTool.GetString("Wood.Spring.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSpringBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeSpringBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Spring.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSpringBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleSpringBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.WindProjectile00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.WindProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Spring.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSpringBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Spring", "098a29fefbbc4564281afa5a6887cd2c", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, twice: true),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallSpringBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Spring.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateSpringBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SpringBlastBase", LocalizationTool.GetString("Wood.Spring.Name"),
                LocalizationTool.GetString("Wood.Spring.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateSpringBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("SpringBlastFeature", LocalizationTool.GetString("Wood.Spring.Name"),
                LocalizationTool.GetString("Wood.Spring.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Spring.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Composite_Spring.Blade.Feature))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Summer
        public static void CreateSummerBlast()
        {
            // Variants
            var standard = CreateSummerBlastVariant_base();
            //var blade = CreateSummerBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Summer", "Summer", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.FireCommonProjectile00,
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                e: DamageEnergyType.Fire);
            // Ability
            var extended = CreateSummerBlastVariant_extended();
            var spindle = CreateSummerBlastVariant_spindle();
            var wall = CreateSummerBlastVariant_wall();
            // Ability
            CreateSummerBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateSummerBlastFeature();

            //Entangling
            //Pushing
        }

        #region Summer Variants

        private static BlueprintAbility CreateSummerBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SummerBlastAbility", LocalizationTool.GetString("Wood.Summer.Name"),
                LocalizationTool.GetString("Wood.Summer.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.FireCommonProjectile00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSummerBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeSummerBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.FireCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Summer.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSummerBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleSummerBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.FireCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.FireCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Summer.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSummerBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Summer", "098a29fefbbc4564281afa5a6887cd2c", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallSummerBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Summer.BaseAbility;

            return ability;
        }


        #endregion

        public static void CreateSummerBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SummerBlastBase", LocalizationTool.GetString("Wood.Summer.Name"),
                LocalizationTool.GetString("Wood.Summer.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateSummerBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("SummerBlastFeature", LocalizationTool.GetString("Wood.Summer.Name"),
                LocalizationTool.GetString("Wood.Summer.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Summer.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Composite_Summer.Blade.Feature))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Winter
        public static void CreateWinterBlast()
        {
            // Variants
            var standard = CreateWinterBlastVariant_base();
            //var blade = CreateWinterBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Winter", "Winter", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.Kinetic_Ice00_Projectile,
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/verdantBlast.png"),
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                e: DamageEnergyType.Cold);

            var extended = CreateWinterBlastVariant_extended();
            var spindle = CreateWinterBlastVariant_spindle();
            var wall = CreateWinterBlastVariant_wall();
            // Ability
            CreateWinterBlastAbility(standard, blade, extended, spindle, wall);
            // Feature
            CreateWinterBlastFeature();

            //Entangling
            //Pushing
        }

        #region Winter Variants

        private static BlueprintAbility CreateWinterBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("WinterBlastAbility", LocalizationTool.GetString("Wood.Winter.Name"),
                LocalizationTool.GetString("Wood.Winter.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.ColdCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWinterBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeWinterBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.ColdCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Winter.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWinterBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleWinterBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.ColdCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.ColdCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Winter.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWinterBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Winter", "9ca5f7aa098e29a4ea17171cba6c8a43", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallWinterBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Winter.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateWinterBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("WinterBlastBase", LocalizationTool.GetString("Wood.Winter.Name"),
                LocalizationTool.GetString("Wood.Winter.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateWinterBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("WinterBlastFeature", LocalizationTool.GetString("Wood.Winter.Name"),
                LocalizationTool.GetString("Wood.Winter.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Winter.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Composite_Winter.Blade.Feature))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Positive Admixture

        public static void CreatePositiveAdmixture()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/positiveBlast.png");

            var ability = Helper.CreateBlueprintActivatableAbility("PositiveAdmixtureAbility", out var buff, LocalizationTool.GetString("Wood.Admixture.Positive.Name"),
                LocalizationTool.GetString("Wood.Admixture.Positive.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniquePositiveAdmixture
                {
                    m_AbilityList = Tree.GetAll(basic: true, onlyEnergy: true, archetype: true).Select(s => s.BaseAbility).ToArray()
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 2
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("PositiveAdmixtureFeature", LocalizationTool.GetString("Wood.Admixture.Positive.Name"),
                LocalizationTool.GetString("Wood.Admixture.Positive.Description"), icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(ability))
                );

            PositiveAdmixture.Feature = feature.ToRef();
            PositiveAdmixture.Buff = buff.ToRef();

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, onlyEnergy: true).ToList().ToArray());
        }


        #endregion

        #endregion

        #region Infusions
        public static void CreateInfusions()
        {
            CreateSporeInfusion();
            CreateToxicInfusion();
            CreateGreaterToxicInfusion();

            Kineticist.TryDarkCodexAddExtraWildTalent
                (
                Tree.Spore.Feature,
                Tree.Toxic.Feature,
                Tree.ToxicGreater.Feature
                );
            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures,
                Tree.Spore.Feature,
                Tree.Toxic.Feature,
                Tree.ToxicGreater.Feature
                );

        }

        public static void CreateSporeInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("d797007a142a6c0409a74b064065a15e"); // Poison

            var poison_damage = new ContextDiceValue
            {
                DiceType = DiceType.D6,
                DiceCountValue = new ContextValue { Value = 1, ValueType = ContextValueType.Simple },
                BonusValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple }
            };
            var poison_duration = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple },
                BonusValue = new ContextValue { Value = 10, ValueType = ContextValueType.Simple }
            };

            var deal_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Acid, poison_damage);
            deal_damage.Duration = poison_duration;
            deal_damage.DamageType.Type = Kingmaker.RuleSystem.Rules.Damage.DamageType.Direct;

            var poison_buff = Helper.CreateBlueprintBuff("SporeInfusionPoison", LocalizationTool.GetString("Wood.Spore.Poison.Name"),
                LocalizationTool.GetString("Wood.Spore.Poison.Description"), icon);
            poison_buff.SetComponents
                (
                Helper.CreateAddFactContextActions(on: null, off: null, round: new GameAction[] { deal_damage })
                );

            var disease_buff = Helper.CreateBlueprintBuff("SporeInfusionDisease", LocalizationTool.GetString("Wood.Spore.Disease.Name"),
                LocalizationTool.GetString("Wood.Spore.Disease.Description"), icon);
            disease_buff.SetComponents
                (
                new BuffPoisonStatDamage
                {
                    Descriptor = ModifierDescriptor.None,
                    Stat = StatType.Dexterity,
                    Value = new DiceFormula { m_Dice = DiceType.D6, m_Rolls = 1 },
                    Ticks = 1000,
                    SaveType = SavingThrowType.Fortitude
                }
                );
            disease_buff.Frequency = DurationRate.Days;


            var ability = Helper.CreateBlueprintActivatableAbility("SporeInfusionAbility", out var buff, LocalizationTool.GetString("Wood.Spore.Name"),
                LocalizationTool.GetString("Wood.Spore.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(new ContextActionSavingThrow
                    {
                        Type = SavingThrowType.Fortitude,
                        Actions = Helper.CreateActionList(new ContextActionConditionalSaved { Failed = Helper.CreateActionList(poison_buff.CreateContextActionApplyBuff(permanent: true), disease_buff.CreateContextActionApplyBuff(permanent: true)) })
                    }),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 2
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("SporeInfusionFeature", LocalizationTool.GetString("Wood.Spore.Name"),
                LocalizationTool.GetString("Wood.Spore.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreateAddFacts(AnyRef.ToAny(ability))
                );
        }

        public static void CreateToxicInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("4e42460798665fd4cb9173ffa7ada323"); // Sickened
            var sickened = Helper.ToRef<BlueprintBuffReference>("4e42460798665fd4cb9173ffa7ada323"); // Sickened

            var ability = Helper.CreateBlueprintActivatableAbility("ToxicInfusionAbility", out var buff, LocalizationTool.GetString("Wood.Toxic.Name"),
                LocalizationTool.GetString("Wood.Toxic.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(new ContextActionSavingThrow
                    {
                        Type = SavingThrowType.Fortitude,
                        Actions = Helper.CreateActionList(new ContextActionConditionalSaved { Failed = Helper.CreateActionList(sickened.Get().CreateContextActionApplyBuff(1, DurationRate.Rounds)) })
                    }),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 3
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("ToxicInfusionFeature", LocalizationTool.GetString("Wood.Toxic.Name"),
                LocalizationTool.GetString("Wood.Toxic.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(AnyRef.ToAny(ability))
                );
        }

        public static void CreateGreaterToxicInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("4e42460798665fd4cb9173ffa7ada323"); // Sickened Icon

            var poison_buff = Helper.CreateBlueprintBuff("GreaterToxicInfusionDisease", "Toxic", "Taking 1d2 Con damage for 6 rounds, Cure: 2 consecutive saves",
                icon);
            poison_buff.SetComponents
                (
                new BuffPoisonStatDamage
                {
                    Descriptor = ModifierDescriptor.None,
                    Stat = StatType.Constitution,
                    Value = new DiceFormula { m_Dice = DiceType.D2, m_Rolls = 1 },
                    Ticks = 6,
                    SaveType = SavingThrowType.Fortitude
                }
                );

            var ability = Helper.CreateBlueprintActivatableAbility("GreaterToxicInfusionAbility", out var buff, LocalizationTool.GetString("Wood.Toxic.Greater.Name"),
                LocalizationTool.GetString("Wood.Toxic.Greater.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(new ContextActionSavingThrow
                    {
                        Type = SavingThrowType.Fortitude,
                        Actions = Helper.CreateActionList(new ContextActionConditionalSaved { Failed = Helper.CreateActionList(poison_buff.CreateContextActionApplyBuff(permanent: true)) })
                    }),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 3
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("GreaterToxicInfusionFeature", LocalizationTool.GetString("Wood.Toxic.Greater.Name"),
                LocalizationTool.GetString("Wood.Toxic.Greater.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 14),
                Helper.CreateAddFacts(AnyRef.ToAny(ability)),
                new RemoveFeatureOnApply
                {
                        m_Feature = AnyRef.ToAny(Tree.Toxic.Feature)
                }
                );
        }

        #endregion

        #region Wild Talents

        private static void CreateWoodWildTalents(BlueprintFeature elemental_defense)
        {
            AddToSkilledKineticist();
            AddToExpandedDefense(elemental_defense);

            (var wild_0, var wild_1, var wild_2, var wild_3) = CreateWildTalentBonusFeatWood();
            BlueprintFeatureReference woodHealer = CreateWoodHealer();
            BlueprintFeatureReference woodlandStep = CreateWoodlandStep();
            BlueprintFeatureReference thornFlesh = CreateThornFlesh(elemental_defense);
            BlueprintFeatureReference herbalAntivenom = CreateHerbalAntivenom();
            BlueprintFeatureReference wildGrowth = CreateWildGrowth();
            BlueprintFeatureReference forestSiege = CreateForestSiege();
            BlueprintFeatureReference woodSoldiers = CreateWoodSoldiers();

            Kineticist.TryDarkCodexAddExtraWildTalent(wild_0, wild_1, wild_2, wild_3, woodHealer, woodlandStep, thornFlesh, herbalAntivenom, wildGrowth, forestSiege, woodSoldiers);
            Kineticist.AddToWildTalents(wild_0, wild_1, wild_2, wild_3, woodHealer, woodlandStep, thornFlesh, herbalAntivenom, wildGrowth, forestSiege, woodSoldiers);
        }

        private static void AddToSkilledKineticist()
        {
            var buff = Helper.CreateBlueprintBuff("SkilledKineticistWoodBuff", LocalizationTool.GetString("SkilledKineticist"));
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                    ContextRankProgression.Div2, max: 20, classes: new BlueprintCharacterClassReference[1] { Tree.Class }),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillLoreNature)
                );

            var condition = Helper.CreateContextConditionHasFact(AnyRef.ToAny(Tree.FocusWood.First));
            var conditional = Helper.CreateConditional(condition,
                ifTrue: buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, false, false, true, true));

            var factContextAction = Kineticist.ref_skilled_kineticist.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref factContextAction.Activated.Actions, conditional);
        }

        private static void AddToExpandedDefense(BlueprintFeature elemental_defense)
        {
            var selection = AnyRef.ToRef<BlueprintFeatureSelectionReference>(Kineticist.ref_expanded_defense).Get();
            Helper.AppendAndReplace(ref selection.m_AllFeatures, elemental_defense.ToRef());
        }

        private static BlueprintFeatureReference CreateWoodHealer()
        {
            var KineticRevivification = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0377fcf4c10871f4187809d273af7f5d"); // KineticRevivificationFeature
            var HealingBurst = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c73b37aaa2b82b44686c56db8ce14e7f"); // HealingBUrstFeature

            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/woodHealer.png");
            var negativeAffinity = Helper.ToRef<BlueprintUnitFactReference>("d5ee498e19722854198439629c1841a5"); // NegativeEnergyAffinity
            var constructType = Helper.ToRef<BlueprintUnitFactReference>("fd389783027d63343b4a5634bd81645f"); // ConstructType

            var heal = new ContextActionHealTarget
            {
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.Zero,
                    DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple },
                    BonusValue = new ContextValue { ValueType = ContextValueType.Shared, ValueShared = AbilitySharedValue.Duration }
                }
            };

            var fx = new ContextActionSpawnFx
            {
                PrefabLink = new PrefabLink { AssetId = "e9399b6d57369ab4a9c3d88798d92f33" }
            };

            var calc_shared_duration = Kineticist.Blast.CalculateSharedValue(type: AbilitySharedValue.Duration, dice: DiceType.D6);
            calc_shared_duration.Value.BonusValue.ValueType = ContextValueType.Shared;
            calc_shared_duration.Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            var ability = Helper.CreateBlueprintAbility("WoodHealerAbility", LocalizationTool.GetString("Wood.WoodHealer.Name"),
                LocalizationTool.GetString("Wood.WoodHealer.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Touch, null, null).TargetAlly(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, heal, fx),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.CalculateSharedValue(),
                calc_shared_duration,
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1),
                Helper.CreateAbilityTargetHasFact(true, negativeAffinity, constructType),
                new SpellComponent { School = SpellSchool.Universalist }
                );

            var feature = Helper.CreateBlueprintFeature("WoodHealerFeature", LocalizationTool.GetString("Wood.WoodHealer.Name"),
                LocalizationTool.GetString("Wood.WoodHealer.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteFeature(Tree.Positive.BlastFeature),
                Helper.CreateAddFacts(ability.ToRef2()),
                Helper.CreatePrerequisiteNoFeature(feature.ToRef())
                );

            var prereqList = KineticRevivification.GetComponent<PrerequisiteFeaturesFromList>();
            prereqList.Group = Prerequisite.GroupType.Any;
            Helper.AppendAndReplace(ref prereqList.m_Features, feature.ToRef());

            prereqList = HealingBurst.GetComponent<PrerequisiteFeaturesFromList>();
            prereqList.Group = Prerequisite.GroupType.Any;
            Helper.AppendAndReplace(ref prereqList.m_Features, feature.ToRef());


            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateWoodlandStep()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("11f4072ea766a5840a46e6660894527d"); //BloodlineFeyWoodlandStride

            var feature = Helper.CreateBlueprintFeature("WoodlandStepFeature", LocalizationTool.GetString("Wood.Step.Name"),
                LocalizationTool.GetString("Wood.Step.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteFeature(Tree.Positive.BlastFeature),
                Helper.CreatePrerequisiteNoFeature(feature.ToRef()),
                new AddConditionImmunity
                {
                    Condition = UnitCondition.DifficultTerrain
                }
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateThornFlesh(BlueprintFeature elemental_defense)
        {
            UnityEngine.Sprite icon = Helper.StealIcon("94064ed53b1020247941ac70313b439d"); // JaggedFleshFeature
            PrefabLink prefab = new PrefabLink { AssetId = "352469f228a3b1f4cb269c7ab0409b8e" }; // JaggedFleshAbility
            PrefabLink onStart = new PrefabLink { AssetId = "c8782bace2956d641892f8a6a523bdfa" }; // JaggedFleshBuff

            var buff = Helper.CreateBlueprintBuff("ThornFleshBuff", LocalizationTool.GetString("Wood.ThornFlesh.Name"),
                LocalizationTool.GetString("Wood.ThornFlesh.Description"), icon, onStart);
            buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;
            buff.FxOnRemove = onStart;
            buff.SetComponents
                (
                new AddTargetAttackWithWeaponTrigger
                {
                    OnlyHit = true, OnlyMelee = true, CheckCategory = true,
                    Categories = new WeaponCategory[] { WeaponCategory.UnarmedStrike, WeaponCategory.Bite, WeaponCategory.Claw, WeaponCategory.OtherNaturalWeapons, WeaponCategory.Gore },
                    ActionsOnAttacker = 
                        Helper.CreateActionList
                        (
                            Helper.CreateContextActionDealDamage
                            (
                                PhysicalDamageForm.Piercing, 
                                new ContextDiceValue { DiceType = DiceType.D6, BonusValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple}, DiceCountValue = new ContextValue { Value = 1, ValueType = ContextValueType.Simple } }
                            ))
                }
                );

            var ability = Helper.CreateBlueprintAbility("ThornFleshAbility", LocalizationTool.GetString("Wood.ThornFlesh.Name"),
                LocalizationTool.GetString("Wood.ThornFlesh.Description"), icon, AbilityType.Supernatural, UnitCommand.CommandType.Standard,
                AbilityRange.Personal, null, null).TargetSelf(CastAnimationStyle.Self);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(permanent: true, fromSpell: true)),
                new AbilitySpawnFx 
                    {
                    Time = AbilitySpawnFxTime.OnApplyEffect,
                    Anchor = AbilitySpawnFxAnchor.SelectedTarget,
                    OrientationAnchor = AbilitySpawnFxAnchor.None,
                    PositionAnchor = AbilitySpawnFxAnchor.None,
                    OrientationMode = AbilitySpawnFxOrientation.Copy,
                    PrefabLink = prefab
                    },
                new SpellDescriptorComponent
                {
                    Descriptor = (SpellDescriptor)1
                },
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("ThornFleshFeature", LocalizationTool.GetString("Wood.ThornFlesh.Name"),
                LocalizationTool.GetString("Wood.ThornFlesh.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeature(elemental_defense.ToRef()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateHerbalAntivenom()
        {
            var buff = Helper.CreateBlueprintBuff("HerbalAntivenomBuff", LocalizationTool.GetString("Wood.Herbal.Name"),
                LocalizationTool.GetString("Wood.Herbal.Description"));
            buff.Flags(hidden: true, stayOnDeath: true);
            buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;
            buff.SetComponents
                (
                new SavingThrowBonusAgainstDescriptor
                {
                    SpellDescriptor = SpellDescriptor.Poison,
                    ModifierDescriptor = ModifierDescriptor.Alchemical,
                    Value = 5
                },
                new AddStatBonus
                {
                    Descriptor = ModifierDescriptor.UntypedStackable,
                    Stat = StatType.SkillLoreReligion,
                    Value = 5
                }
                );

            var feature = Helper.CreateBlueprintFeature("HerbalAntivenomFeature", LocalizationTool.GetString("Wood.Herbal.Name"),
                LocalizationTool.GetString("Wood.Herbal.Description"), null, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(buff.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateWildGrowth()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"); // Entangle

            var ability = Helper.CreateBlueprintAbility("WildGrowthAbility", LocalizationTool.GetString("Wood.WildGrowth.Name"),
                LocalizationTool.GetString("Wood.WildGrowth.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, 
                AbilityRange.Long, null, null).TargetPoint(CastAnimationStyle.Omni);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, new ContextActionSpawnAreaEffect
                    {
                      m_AreaEffect = Helper.ToRef<BlueprintAbilityAreaEffectReference>("bcb6329cefc66da41b011299a43cc681"), // EntangleArea
                      DurationValue = new ContextDurationValue
                      {
                          Rate = DurationRate.Minutes,
                          DiceType = DiceType.Zero,
                          DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple },
                          BonusValue = new ContextValue { ValueRank = AbilityRankType.Default, ValueType = ContextValueType.Rank }
                      }
                    }),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClassReference[] {Tree.Class}),
                new AbilityAoERadius { m_TargetType = TargetType.Any, m_Radius = new Feet { m_Value = 40 } },
                Kineticist.Blast.BurnCost(null)
                );

            var feature = Helper.CreateBlueprintFeature("WildGrowthFeature", LocalizationTool.GetString("Wood.WildGrowth.Name"),
                LocalizationTool.GetString("Wood.WildGrowth.Description"), null, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateForestSiege()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("01300baad090d634cb1a1b2defe068d6"); // ClashingRocks

            var ability_target = Helper.CreateBlueprintAbility("ForestSiegeAbilityTarget", LocalizationTool.GetString("Wood.Siege.Ability.Name"),
                LocalizationTool.GetString("Wood.Siege.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Long).TargetEnemy(CastAnimationStyle.Kineticist);
            ability_target.SetComponents
                (
                Helper.CreateAbilityEffectRunAction
                    (
                    SavingThrowType.Reflex,
                    Helper.CreateContextActionDealDamage(PhysicalDamageForm.Bludgeoning, new ContextDiceValue { DiceType = DiceType.D6, BonusValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple }, DiceCountValue = new ContextValue { Value = 6, ValueType = ContextValueType.Simple } }, halfIfSaved: true)
                    ),
                new AbilityDeliverProjectileClustered
                {
                    m_Projectile = Resource.Projectile.Kinetic_EarthSphere00_Projectile.ToRef<BlueprintProjectileReference>(),
                    m_ProjectileCount = new ContextValue { ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.ProjectilesCount }
                },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.DivStep, AbilityRankType.ProjectilesCount, stepLevel: 3, classes: new BlueprintCharacterClassReference[] { Tree.Class }),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 0)
                );

            var buff = Helper.CreateBlueprintBuff("ForestSiegeBuff", "Forest Siege",
                "You may command plant-life to hurl rocks at a target", icon);
            buff.Flags(stayOnDeath: true);
            buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;

            var ability_buff = Helper.CreateBlueprintAbility("ForestSiegeAbilityBuff", LocalizationTool.GetString("Wood.Siege.Action.Name"),
                LocalizationTool.GetString("Wood.Siege.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Kineticist);
            ability_buff.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(permanent: true)),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var ability_base = Helper.CreateBlueprintAbility("ForestSiegeAbilityBase", LocalizationTool.GetString("Wood.Siege.Name"),
                LocalizationTool.GetString("Wood.Siege.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Personal);

            Helper.AddToAbilityVariants(ability_base, ability_buff, ability_target);

            var feature = Helper.CreateBlueprintFeature("ForestSiegeFeature", LocalizationTool.GetString("Wood.Siege.Name"),
                LocalizationTool.GetString("Wood.Siege.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 18),
                Helper.CreateAddFacts(ability_base.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateWoodSoldiers()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("4b76d32feb089ad4499c3a1ce8e1ac27"); // AnimateDead
            var unit = CreateWoodSoldierUnit();
            var context_four = new ContextDiceValue { DiceType = DiceType.Zero, BonusValue = new ContextValue { Value = 4, ValueType = ContextValueType.Simple }, DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple } };
            var context_duration_hours = new ContextDurationValue
            {
                Rate = DurationRate.Hours,
                DiceType = DiceType.Zero,
                DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple },
                BonusValue = new ContextValue { Value = 8, ValueType = ContextValueType.Simple }
            };

            string pool_guid = Helper.GetGuid("WoodSoldiersSummonPool");
            var pool = new BlueprintSummonPool
            {
                name = "WoodSoldiersSummonPool",
                Limit = 4,
                DoNotRemoveDeadUnits = false
            };
            Helper.AddAsset(pool, pool_guid);
            var pool_ref = AnyRef.ToRef<BlueprintSummonPoolReference>(pool);

            #region Golem Buffs

            var buff_autumn = Helper.CreateBlueprintBuff("WoodSoldiersAutumnBuff", "Autumn Soldier", LocalizationTool.GetString("Wood.Soldiers.Description"), icon);
            buff_autumn.SetComponents // Burrow Speed, Resistance to Physical
                (
                new AddDamageResistancePhysical { }
                );
            var buff_spring = Helper.CreateBlueprintBuff("WoodSoldiersSpringBuff", "Spring Soldier", LocalizationTool.GetString("Wood.Soldiers.Description"), icon);
            buff_spring.SetComponents // Flight, Difficult terrain and AC bonus
                (
                new AddConditionImmunity
                {
                    Condition = UnitCondition.DifficultTerrain
                },
                new AddStatBonus
                {
                    Descriptor = ModifierDescriptor.UntypedStackable,
                    Stat = StatType.AC,
                    Value = 3
                }
                );
            var buff_summer = Helper.CreateBlueprintBuff("WoodSoldiersSummerBuff", "Summer Soldier", LocalizationTool.GetString("Wood.Soldiers.Description"), icon);
            buff_summer.SetComponents // Fire Resistance, Fire damage on attacks
                (
                new AddDamageResistanceEnergy {  Type = DamageEnergyType.Fire },
                new AdditionalDamageOnHit
                {
                    OnlyMelee = false,
                    OnlyNaturalAndUnarmed = false,
                    SpecificWeapon = false,
                    Element = DamageEnergyType.Fire,
                    EnergyDamageDice = new DiceFormula { m_Dice = DiceType.D6, m_Rolls = 1 }
                }
                );
            var buff_winter = Helper.CreateBlueprintBuff("WoodSoldiersWinterBuff", "Winter Soldier", LocalizationTool.GetString("Wood.Soldiers.Description"), icon);
            buff_winter.SetComponents // Cold Resistance, Cold damage on attacks
                (
                new AddDamageResistanceEnergy { Type = DamageEnergyType.Cold },
                new AdditionalDamageOnHit
                {
                    OnlyMelee = false,
                    OnlyNaturalAndUnarmed = false,
                    SpecificWeapon = false,
                    Element = DamageEnergyType.Cold,
                    EnergyDamageDice = new DiceFormula { m_Dice = DiceType.D6, m_Rolls = 1 }
                }
                );

            #endregion

            var summon = new ContextActionSpawnMonster
            {
                m_Blueprint = unit,
                m_SummonPool = pool_ref,
                CountValue = context_four,
                DurationValue = context_duration_hours,
                UseLimitFromSummonPool = true,
                AfterSpawn = Helper.CreateActionList(
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.ToAny(Tree.Composite_Autumn.BlastFeature)),
                        ifFalse: null,
                        ifTrue: buff_autumn.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.ToAny(Tree.Composite_Spring.BlastFeature)),
                        ifFalse: null,
                        ifTrue: buff_spring.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.ToAny(Tree.Composite_Summer.BlastFeature)),
                        ifFalse: null,
                        ifTrue: buff_summer.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.ToAny(Tree.Composite_Winter.BlastFeature)),
                        ifFalse: null,
                        ifTrue: buff_winter.CreateContextActionApplyBuff(permanent: true))
                    )
            };

            var buff = Helper.CreateBlueprintBuff("WoodSoldiersBuff", LocalizationTool.GetString("Wood.Soldiers.Buff.Name"),
                LocalizationTool.GetString("Wood.Soldiers.Description"), icon);
            buff.Flags(hidden: true, stayOnDeath: true);
            buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;

            var ability = Helper.CreateBlueprintAbility("WoodSoldiersAbility", LocalizationTool.GetString("Wood.Soldiers.Name"),
                LocalizationTool.GetString("Wood.Soldiers.Description"), icon, AbilityType.Supernatural, UnitCommand.CommandType.Free,
                AbilityRange.Close, null, null).TargetPoint(CastAnimationStyle.Kineticist);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, summon, new ContextActionOnContextCaster { Actions = Helper.CreateActionList(buff.CreateContextActionApplyBuff(permanent: true)) }),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 0)
                );

            buff.SetComponents
                (
                new AddKineticistBurnModifier { BurnType = KineticistBurnType.WildTalent, Value = 1, m_AppliableTo = new BlueprintAbilityReference[] { ability.ToRef() } }
                );

            var feature = Helper.CreateBlueprintFeature("WoodSoldiersFeature", LocalizationTool.GetString("Wood.Soldiers.Name"),
                LocalizationTool.GetString("Wood.Soldiers.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusWood.First),
                    AnyRef.ToAny(Tree.FocusWood.Second),
                    AnyRef.ToAny(Tree.FocusWood.Third),
                    AnyRef.ToAny(Tree.FocusWood.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 16),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        // Change Bonus Feats
        private static (BlueprintFeatureReference wild_0, BlueprintFeatureReference wild_1, BlueprintFeatureReference wild_2, BlueprintFeatureReference wild_3) CreateWildTalentBonusFeatWood()
        {
            var spell_pen = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459"); // SpellPenetration
            var spell_pen_greater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec"); // GreaterSpellPenetration
            var precise_shot = Helper.ToRef<BlueprintFeatureReference>("8f3d1e6b4be006f4d896081f2f889665"); // PreciseShot
            var trip = Helper.ToRef<BlueprintFeatureReference>("0f15c6f70d8fb2b49aa6cc24239cc5fa"); // ImprovedTrip
            var trip_greater = Helper.ToRef<BlueprintFeatureReference>("4cc71ae82bdd85b40b3cfe6697bb7949"); // SpellPenetration

            var wild_0 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood", LocalizationTool.GetString("Wood.Skills.Name"),
                LocalizationTool.GetString("Wood.Skills.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_0.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Third), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Knight), true)
                );
            wild_0.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_0.m_AllFeatures, spell_pen, precise_shot, trip);

            var wild_1 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood1", LocalizationTool.GetString("Wood.Skills.Name"),
                LocalizationTool.GetString("Wood.Skills.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_1.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Third), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Knight), true)
                );
            wild_1.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_1.m_AllFeatures, spell_pen_greater, precise_shot, trip);

            var wild_2 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood2", LocalizationTool.GetString("Wood.Skills.Name"),
                LocalizationTool.GetString("Wood.Skills.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_2.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Third), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Knight), true)
                );
            wild_2.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_2.m_AllFeatures, spell_pen, precise_shot, trip_greater);

            var wild_3 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood3", LocalizationTool.GetString("Wood.Skills.Name"),
                LocalizationTool.GetString("Wood.Skills.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_3.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Third), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                new PrerequisiteSelectionPossible
                {
                    m_ThisFeature = AnyRef.ToAny(wild_3)
                },
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusWood.Knight), true)
                );
            wild_3.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_3.m_AllFeatures, spell_pen_greater, precise_shot, trip_greater);

            return (wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef());
        }


        #endregion

        #region Units

        private static BlueprintUnitReference CreateWoodSoldierUnit()
        {
            var brain = Helper.ToRef<BlueprintBrainReference>("e3fc8ed2e88da4145b217c316751cc7f"); // WoodGolemBrain
            var type = Helper.ToRef<BlueprintUnitTypeReference>("35974c5b156e5824f94379ce60a95150"); // GolemStone
            var portrait = Helper.ToRef<BlueprintPortraitReference>("2a1e6b8c31e245f7899825149ce4d113"); // StoneGolem
            var faction = Helper.ToRef<BlueprintFactionReference>("1b08d9ed04518ec46a9b3e4e23cb5105"); // Summoned
            var add_facts = new BlueprintUnitFactReference[]
            {
                Helper.ToRef<BlueprintUnitFactReference>("987ba44303e88054c9504cb3083ba0c9"), // NaturalArmor6
                Helper.ToRef<BlueprintUnitFactReference>("fb88b018013dc8e419150f86540c07f2"), // DRAdamantine5
                Helper.ToRef<BlueprintUnitFactReference>("be8dcb83f4bd3e24185ceb0cea084a06"), // GolemWoodImmunity
                Helper.ToRef<BlueprintUnitFactReference>("eba7737aef48d304fb6788d748a2df69"), // GolemWoodSplintering
                Helper.ToRef<BlueprintUnitFactReference>("40aaecaebce743e48a1d35957583fcc6") // GolemAutumnVisual
            };
            var limbs = new BlueprintItemWeaponReference[]
            {
                Helper.ToRef<BlueprintItemWeaponReference>("c6d3cd958772be148952c011b3a15452"), // Slam2d6
                Helper.ToRef<BlueprintItemWeaponReference>("c6d3cd958772be148952c011b3a15452") // Slam2d6
            };
            var barks = Helper.ToRef<BlueprintUnitAsksListReference>("34ca66289f4899347a7ab9bef34a13f0"); // StoneGolem_barks

            string guid_Unit = Helper.GetGuid("WoodSoldierUnit");
            string guid_Template = Helper.GetGuid("WoodSoldierAdvancedTemplate");

            var unit = new BlueprintUnit
            {
                name = "WoodSoldierUnit",
                m_DisplayName = LocalizationTool.GetString("Wood.Soldiers.Unit.Name"),
                m_Description = LocalizationTool.GetString("Wood.Soldiers.Unit.Description"),
                m_Icon = null
            };
            Helper.AddAsset(unit, guid_Unit);

            unit.SetComponents
                (
                new AddClassLevels
                {
                    m_CharacterClass = Helper.ToRef<BlueprintCharacterClassReference>("fd66bdea5c33e5f458e929022322e6bf"), // ConstructClass
                    Levels = 8,
                    RaceStat = StatType.Constitution,
                    LevelsStat = StatType.Unknown
                },
                new BuffOnEntityCreated
                {
                    m_Buff = Helper.ToRef<BlueprintBuffReference>("0f775c7d5d8b6494197e1ce937754482") // Unlootable
                },
                new BuffOnEntityCreated
                {
                    m_Buff = Helper.ToRef<BlueprintBuffReference>("50d51854cf6a3434d96a87d050e1d09a") // SummonedCreatureSpawnMonsterIV-VI
                },
                new UnitUpgraderComponent // RemoveBrokenSummonOnLoad
                {
                    m_Upgraders = new BlueprintUnitUpgrader.Reference[] { Helper.ToRef<BlueprintUnitUpgrader.Reference>("2fc8c3f9bc904d8a82daa72d844dbed2") }
                }
                );
            unit.m_Type = type;
            unit.LocalizedName = (Kingmaker.Localization.SharedStringAsset)ScriptableObject.CreateInstance("SharedStringAsset");
            unit.LocalizedName.String = LocalizationTool.GetString("Wood.Soldiers.Unit.Name");
            unit.Gender = Gender.Male;
            unit.Size = Size.Medium;
            unit.IsLeftHanded = false;
            unit.Color = new UnityEngine.Color { a = 1f, r = .15f, g = .15f, b = .15f };
            unit.Alignment = Alignment.TrueNeutral;
            unit.m_Portrait = portrait;
            unit.Prefab = new UnitViewLink { AssetId = "f59e51021e055b1459b0260a76cc4e54" };
            unit.Visual = new UnitVisualParams 
                {
                BloodType = Kingmaker.Visual.HitSystem.BloodType.Dust,
                FootprintType = FootprintType.Humanoid,
                FootprintScale = 1.0f,
                BloodPuddleFx = new PrefabLink { AssetId = "999702969e8c9c1418e3d97a4b776710" },
                DismemberFx = new PrefabLink { AssetId = "999702969e8c9c1418e3d97a4b776710" },
                m_Barks = barks,
                DefaultArmorSoundType = Kingmaker.Visual.Sound.ArmorSoundType.Wood,
                FootstepSoundSizeType = Kingmaker.Visual.Sound.FootstepSoundSizeType.BootLarge,
                FootSoundType = Kingmaker.Visual.Sound.FootSoundType.None,
                FootSoundSize = Size.Large,
                BodySoundType = Kingmaker.Visual.Sound.BodySoundType.Wood,
                BodySoundSize = Size.Large,
                SilentCaster = true
                };
            unit.m_Faction = faction;
            unit.FactionOverrides = new FactionOverrides
            {
                m_AttackFactionsToAdd = new BlueprintFactionReference[0],
                m_AttackFactionsToRemove = new BlueprintFactionReference[0]
            };
            unit.m_Brain = brain;
            unit.Body = new BlueprintUnit.UnitBody 
                {
                DisableHands = true,
                m_AdditionalLimbs = limbs
                };
            unit.Strength = 18;
            unit.Dexterity = 17;
            unit.Constitution = 10;
            unit.Intelligence = 0;
            unit.Wisdom = 17;
            unit.Charisma = 1;
            unit.Speed = new Feet { m_Value = 30 };
            unit.Skills = new BlueprintUnit.UnitSkills
            {
                Acrobatics = 0,
                Physique = 0,
                Diplomacy = 0,
                Thievery = 0,
                LoreNature = 0,
                Perception = 0,
                UseMagicDevice = 0,
                LoreReligion = 0,
                KnowledgeWorld = 0,
                KnowledgeArcana = 0
            };
            unit.m_AddFacts = add_facts;

            StatAdjustment[] adjustments = new StatAdjustment[] 
            {
                new StatAdjustment { Stat = StatType.Strength, Adjustment = 4 },
                new StatAdjustment { Stat = StatType.Dexterity, Adjustment = 4 },
                new StatAdjustment { Stat = StatType.Wisdom, Adjustment = 4 },
                new StatAdjustment { Stat = StatType.Charisma, Adjustment = 4 }
            };

            var advanced = new BlueprintUnitTemplate
            {
                name = "WoodenSoldierAdvancedTemplate",
                m_RemoveFacts = Array.Empty<BlueprintUnitFactReference>(),
                m_AddFacts = Array.Empty<BlueprintUnitFactReference>(),
                StatAdjustments = adjustments
            };
            Helper.AddAsset(advanced, guid_Template);

            unit.m_AdditionalTemplates = new BlueprintUnitTemplateReference[] { AnyRef.ToAny(advanced) };

            return AnyRef.ToAny(unit);
        }

        #endregion
    }
}
