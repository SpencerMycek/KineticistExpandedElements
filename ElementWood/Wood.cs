using BlueprintCore.Utils;
using KineticistElementsExpanded.Components;
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

namespace KineticistElementsExpanded.ElementWood
{
    class Wood : Statics
    {
        // Idea to bring Ghoran race as a dryad or something
        
        public static KineticistTree Tree = new();

        public static KineticistTree.Focus WoodFocus = new();

        public static KineticistTree.Element Positive = new();
        public static KineticistTree.Element WoodBlast = new();
        public static KineticistTree.Element Verdant = new();
        public static KineticistTree.Element Autumn = new();
        public static KineticistTree.Element Spring = new();
        public static KineticistTree.Element Summer = new();
        public static KineticistTree.Element Winter = new();

        public static KineticistTree.Infusion PositiveAdmixture = new();

        public static KineticistTree.Infusion Spore = new();
        public static KineticistTree.Infusion Toxic = new();
        public static KineticistTree.Infusion GreaterToxic = new();

        public static void Configure()  
        {
            BlueprintFeatureBase wood_class_skills = CreateWoodClassSkills();

            CreateInfusions();

            CreateWoodBlastsSelection();
            CreateCompositeBlasts();

            Kineticist.AddElementsToInfusion(Spore, WoodBlast, Verdant, Autumn, Spring, Summer, Winter);
            Kineticist.AddElementsToInfusion(Toxic, WoodBlast, Verdant, Autumn, Spring, Summer, Winter);
            Kineticist.AddElementsToInfusion(GreaterToxic, WoodBlast, Verdant, Autumn, Spring, Summer, Winter);

            BlueprintFeature flesh_of_wood_feature = CreateFleshofWood();

            Kineticist.AddElementalDefenseIsPrereqFor(Positive.BlastFeature, Positive.BladeFeature, flesh_of_wood_feature);
            Kineticist.AddElementalDefenseIsPrereqFor(WoodBlast.BlastFeature, WoodBlast.BladeFeature, flesh_of_wood_feature);

            Kineticist.ElementsBlastSetup(Positive, WoodBlast, Verdant, Autumn, Spring, Summer, Winter);

            EntanglePushInfusions(WoodBlast, Verdant, Autumn, Spring, Summer, Winter);
            DazzleInfusion(Positive, Verdant);
            FoxfireInfusions(Positive);

            Kineticist.AddCompositeToBuff(Tree, Autumn, WoodBlast, Tree.Earth);
            Kineticist.AddCompositeToBuff(Tree, Spring, WoodBlast, Tree.Air);
            Kineticist.AddCompositeToBuff(Tree, Summer, WoodBlast, Tree.Fire);
            Kineticist.AddCompositeToBuff(Tree, Winter, WoodBlast, Tree.Cold);

            Kineticist.AddAdmixtureToBuff(Tree, PositiveAdmixture, Positive, true, true, false);
            Kineticist.AddBladesToKineticWhirlwind(WoodBlast, Positive, Verdant, Autumn, Spring, Summer, Winter);


            CreateWoodElementalFocus(wood_class_skills, flesh_of_wood_feature);
            CreateKineticKnightWoodFocus(wood_class_skills, flesh_of_wood_feature);
            CreateSecondElementWood();
            CreateThirdElementWood();

            CreateWoodWildTalents(flesh_of_wood_feature);
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateWoodClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("WoodClassSkills", "Wood Class Skills",
                WoodClassSkillsDescription, null, null, 0)
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

            var selection = Helper.CreateBlueprintFeatureSelection("WoodBlastSelection", "Wood Blast",
                WoodBlastDescription, null, null, FeatureGroup.None, SelectionMode.Default);
            selection.IsClassFeature = true;

            Helper.AppendAndReplace<BlueprintFeatureReference>(ref selection.m_AllFeatures,
                AnyRef.Get(Positive.Progession).To<BlueprintFeatureReference>(),
                AnyRef.Get(WoodBlast.Progession).To<BlueprintFeatureReference>());

            Positive.Selection = selection.ToRef3();
            WoodBlast.Selection = Positive.Selection;
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
            var progression = Helper.CreateBlueprintProgression("ElementalFocusWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Tree.BloodKineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any Wood basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(2, flesh_of_wood);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.First = progression.ToRef3();
        }

        private static void CreateKineticKnightWoodFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            var progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(4, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Knight = progression.ToRef3();
        }

        private static void CreateSecondElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("SecondaryElementWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Positive.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(WoodBlast.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Verdant.BlastFeature).To<BlueprintUnitFactReference>())
                        )
                    ),
                Helper.CreateAddFacts(AnyRef.Get(Tree.CompositeBuff).To<BlueprintUnitFactReference>())
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(7, Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Second = progression.ToRef3();
        }

        private static void CreateThirdElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Verdant.BlastFeature).To<BlueprintUnitFactReference>())
                        )
                    ),
                Helper.CreateAddFacts(AnyRef.Get(Tree.CompositeBuff).To<BlueprintUnitFactReference>()),
                Helper.CreatePrerequisiteNoFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>())
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(15, Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Third = progression.ToRef3();
        }

        #endregion

        #region Flesh of Wood

        public static BlueprintFeature CreateFleshofWood()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("5b77d7cc65b8ab74688e74a37fc2f553"); // Barkskin

            #region Resource

            var resource = Helper.CreateBlueprintAbilityResource("FleshofWoodResource", null,
                FleshofWoodDescription, null, false, 6, 0, 1, 0, 3, 1, 2, true, 0, false, 0,
                classRef: Tree.Class);

            #endregion
            #region Effect Feature

            var effect_feature = Helper.CreateBlueprintFeature("FlashofWoodEffectFeature", null,
                null, null, icon, FeatureGroup.None);
            effect_feature.Ranks = 7;
            effect_feature.HideInUI = true;
            effect_feature.HideInCharacterSheetAndLevelUp = true;
            effect_feature.IsClassFeature = true;
            effect_feature.SetComponents
                (
                Helper.CreateAddFacts()
                );

            #endregion
            #region Effect Buff

            var effect_buff = Helper.CreateBlueprintBuff("FleshofWoodEffectBuff", null,
                null, null, icon);
            effect_buff.Flags(hidden: true, stayOnDeath: true, removeOnRest: true);
            effect_buff.Stacking = StackingType.Stack;
            effect_buff.IsClassFeature = true;
            effect_buff.SetComponents
                (
                Helper.CreateAddFacts(effect_feature.ToRef2())
                );

            #endregion
            #region Buff

            var buff = Helper.CreateBlueprintBuff("FleshofWoodBuff", null,
                null, null, icon);
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

            var ability = Helper.CreateBlueprintAbility("FleshofWoodAbility", "Flesh of Wood -- Increase AC",
                FleshofWoodDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityResourceLogic(resource.ToRef(), true, false, 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var feature = Helper.CreateBlueprintFeature("FleshofWoodFeature", "Flesh of Wood",
                FleshofWoodDescription, null, icon, FeatureGroup.None);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreateAddFacts(buff.ToRef2(), ability.ToRef2()),
                // Prereqs Positive/Wood Feature, Respective Blade features
                Helper.CreatePrerequisiteFeature(Positive.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Positive.BladeFeature, any: true),
                Helper.CreatePrerequisiteFeature(WoodBlast.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(WoodBlast.BladeFeature, any: true),
                Helper.CreateAddAbilityResources(false, 0, true, false, resource.ToRef())
                );

            return feature;
        }

        #endregion

        #region Positive Blast

        public static void CreatePositiveBlast()
        {
            // Variants
            var standard = CreatePositiveBlastVariant_base();
            var blade = CreatePositiveBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("positiveBlast.png");

            var ability = Helper.CreateBlueprintAbility("PositiveBlastAbility", "Positive Blast",
                PositiveBlastDescription, null, icon, AbilityType.Special,
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
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreatePositiveBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangePositiveBlastAbility",
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.SunBeam00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Positive.BaseAbility;

            return ability;
        }
        private static BlueprintAbility CreatePositiveBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindlePositiveBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Positive.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Positive.BaseAbility;

            return ability;
        }


        #region Kinetic Blade: Positive

        private static BlueprintAbility CreatePositiveBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreatePositiveBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladePositiveBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladePositiveBlastAbility", "Positive Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladePositiveBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("PositiveBlastKineticBladeDamage", "Positive Blast",
                PositiveBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.Mythic4lvlAngel_BladeOfTheSun00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("PositiveKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Positive.BladeFeature = blade_feat.ToRef();
            Positive.BladeDamageAbility = blade_damage_ability.ToRef();
            Positive.BladeBuff = buff.ToRef();

            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreatePositiveBlastBlade_weapon()
        {
            var weapon = Helper.CreateBlueprintItemWeapon("PositiveKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_energy_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreatePositiveBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreatePositiveBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("PositiveKineticBladeEnchantment", "Positive Blast — Kinetic Blade",
                null, "Positive Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "d739a9e236ba6164ab854b356bfb6ed5" };

            return enchant;
        }

        #endregion

        #endregion

        public static void CreatePositiveBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("positiveBlast.png");

            var ability = Helper.CreateBlueprintAbility("PositiveBlastBase", "Positive Blast",
                PositiveBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Positive.BaseAbility = ability.ToRef();
        }

        public static void CreatePositiveBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("PositiveBlastFeature", "Positive Blast",
                PositiveBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Positive.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Positive.BlastFeature = feature.ToRef();
        }

        public static void CreatePositiveBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("PositiveBlastProgression", "Positive Blast",
                PositiveBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Positive.BladeFeature).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Positive.BlastFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, Positive.BlastFeature);
            Helper.AddEntries(progression, entry);

            Positive.Progession = progression.ToRef3();
        }

        #endregion

        #region Wood Blast

        public static void CreateWoodBlast()
        {
            // Variants
            var standard = CreateWoodBlastVariant_base();
            var blade = CreateWoodBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("woodBlast.png");

            var ability = Helper.CreateBlueprintAbility("WoodBlastAbility", "Wood Blast",
                WoodBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = WoodBlast.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWoodBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleWoodBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = WoodBlast.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = WoodBlast.BaseAbility;

            return ability;
        }
        
        //impale DarkCodex

        #region Kinetic Blade: Wood

        private static BlueprintAbility CreateWoodBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateWoodBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeWoodBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeWoodBlastAbility", "Wood Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeWoodBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("WoodBlastKineticBladeDamage", "Wood Blast",
                WoodBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("WoodKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            WoodBlast.BladeFeature = blade_feat.ToRef();
            WoodBlast.BladeDamageAbility = blade_damage_ability.ToRef();
            WoodBlast.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateWoodBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("WoodKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateWoodBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateWoodBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("WoodKineticBladeEnchantment", "Wood Blast — Kinetic Blade",
                null, "Wood Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" }; // EarthKineticBladeEnchantment

            return enchant;
        }


        #endregion

        #endregion

        public static void CreateWoodBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("woodBlast.png");

            var ability = Helper.CreateBlueprintAbility("WoodBlastBase", "Wood Blast",
                WoodBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            WoodBlast.BaseAbility = ability.ToRef();
        }

        public static void CreateWoodBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("WoodBlastFeature", "Wood Blast",
                WoodBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(WoodBlast.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            WoodBlast.BlastFeature = feature.ToRef();
        }

        public static void CreateWoodBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("WoodBlastProgression", "Wood Blast",
                WoodBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(WoodBlast.BladeFeature).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(WoodBlast.BlastFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(WoodBlast.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            WoodBlast.Progession = progression.ToRef3();
        }

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
            var blade = CreateVerdantBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("VerdantBlastAbility", "Verdant Blast",
                VerdantBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.Vinetrap00_Projectile_1, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Verdant.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateVerdantBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleVerdantBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Verdant.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Verdant.BaseAbility;

            return ability;
        }


        #region Kinetic Blade: Verdant

        private static BlueprintAbility CreateVerdantBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateVerdantBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeVerdantBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeVerdantBlastAbility", "Verdant Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeVerdantBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("VerdantBlastKineticBladeDamage", "Verdant Blast",
                VerdantBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.PositiveEnergy),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.Mythic4lvlAngel_BladeOfTheSun00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("VerdantKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Verdant.BladeFeature = blade_feat.ToRef();
            Verdant.BladeDamageAbility = blade_damage_ability.ToRef();
            Verdant.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateVerdantBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("VerdantKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateVerdantBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateVerdantBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("VerdantKineticBladeEnchantment", "Verdant Blast — Kinetic Blade",
                null, "Verdant Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" }; // EarthKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateVerdantBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("VerdantBlastBase", "Verdant Blast",
                VerdantBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Verdant.BaseAbility = ability.ToRef();
        }

        public static void CreateVerdantBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("VerdantBlastFeature", "Verdant Blast",
                VerdantBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(Verdant.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Verdant.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Verdant.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Autumn

        public static void CreateAutumnBlast()
        {
            // Variants
            var standard = CreateAutumnBlastVariant_base();
            var blade = CreateAutumnBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("AutumnBlastAbility", "Autumn Blast",
                AutumnBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Autumn.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateAutumnBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleAutumnBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Autumn.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Autumn.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Autumn

        private static BlueprintAbility CreateAutumnBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateAutumnBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeAutumnBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeAutumnBlastAbility", "Autumn Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeAutumnBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("AutumnBlastKineticBladeDamage", "Autumn Blast",
                AutumnBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                Kineticist.Blast.RankConfigDice(true, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("AutumnKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Autumn.BladeFeature = blade_feat.ToRef();
            Autumn.BladeDamageAbility = blade_damage_ability.ToRef();
            Autumn.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateAutumnBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("AutumnKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateAutumnBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateAutumnBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("AutumnKineticBladeEnchantment", "Autumn Blast — Kinetic Blade",
                null, "Autumn Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" }; // EarthKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateAutumnBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("AutumnBlastBase", "Autumn Blast",
                AutumnBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Autumn.BaseAbility = ability.ToRef();
        }

        public static void CreateAutumnBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("AutumnBlastFeature", "Autumn Blast",
                AutumnBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(Autumn.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Autumn.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Autumn.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Spring
        public static void CreateSpringBlast()
        {
            // Variants
            var standard = CreateSpringBlastVariant_base();
            var blade = CreateSpringBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SpringBlastAbility", "Spring Blast",
                SpringBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Spring.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSpringBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleSpringBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Spring.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Spring.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Spring

        private static BlueprintAbility CreateSpringBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateSpringBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeSpringBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeSpringBlastAbility", "Spring Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeSpringBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("SpringBlastKineticBladeDamage", "Spring Blast",
                VerdantBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                Kineticist.Blast.RankConfigDice(true, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("SpringKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Spring.BladeFeature = blade_feat.ToRef();
            Spring.BladeDamageAbility = blade_damage_ability.ToRef();
            Spring.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateSpringBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("SpringKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateSpringBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateSpringBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("SpringKineticBladeEnchantment", "Spring Blast — Kinetic Blade",
                null, "Spring Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "19d9b36b62efe1448b00630ec53db58c" }; // AirKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateSpringBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SpringBlastBase", "Spring Blast",
                SpringBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Spring.BaseAbility = ability.ToRef();
        }

        public static void CreateSpringBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("SpringBlastFeature", "Spring Blast",
                SpringBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(Spring.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Spring.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Spring.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Summer
        public static void CreateSummerBlast()
        {
            // Variants
            var standard = CreateSummerBlastVariant_base();
            var blade = CreateSummerBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SummerBlastAbility", "Summer Blast",
                SummerBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.FireCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Summer.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateSummerBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleSummerBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Summer.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Summer.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Summer

        private static BlueprintAbility CreateSummerBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateSummerBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeSummerBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeSummerBlastAbility", "Summer Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeSummerBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("SummerBlastKineticBladeDamage", "Summer Blast",
                SummerBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Fire),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.FireCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("SummerKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Summer.BladeFeature = blade_feat.ToRef();
            Summer.BladeDamageAbility = blade_damage_ability.ToRef();
            Summer.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateSummerBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("SummerKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateSummerBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateSummerBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("SummerKineticBladeEnchantment", "Summer Blast — Kinetic Blade",
                null, "Summer Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "98080b9ed84b7d845bc9549e99652048" }; // FireKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateSummerBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("SummerBlastBase", "Summer Blast",
                SummerBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Summer.BaseAbility = ability.ToRef();
        }

        public static void CreateSummerBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("SummerBlastFeature", "Summer Blast",
                SummerBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(Summer.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Summer.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Summer.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Winter
        public static void CreateWinterBlast()
        {
            // Variants
            var standard = CreateWinterBlastVariant_base();
            var blade = CreateWinterBlastVariant_blade();
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
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("WinterBlastAbility", "Winter Blast",
                WinterBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description,
                null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.ColdCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Winter.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateWinterBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleWinterBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Winter.BaseAbility;

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
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Winter.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Winter

        private static BlueprintAbility CreateWinterBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateWinterBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeWinterBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeWinterBlastAbility", "Winter Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeWinterBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("WinterBlastKineticBladeDamage", "Winter Blast",
                WinterBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, e: DamageEnergyType.Cold),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.ColdCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("WinterKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Winter.BladeFeature = blade_feat.ToRef();
            Winter.BladeDamageAbility = blade_damage_ability.ToRef();
            Winter.BladeBuff = buff.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateWinterBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("WinterKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateWinterBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateWinterBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("WinterKineticBladeEnchantment", "Winter Blast — Kinetic Blade",
                null, "Winter Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "61303c067a5f1a541843bdb2ecb8a9c0" }; // ColdKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateWinterBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("verdantBlast.png");

            var ability = Helper.CreateBlueprintAbility("WinterBlastBase", "Winter Blast",
                WinterBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Winter.BaseAbility = ability.ToRef();
        }

        public static void CreateWinterBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("WinterBlastFeature", "Winter Blast",
                WinterBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(Winter.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Winter.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Winter.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Positive Admixture

        public static void CreatePositiveAdmixture()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("positiveBlast.png");

            var ability = Helper.CreateBlueprintActivatableAbility("PositiveAdmixtureAbility", "Positive Admixture",
                PositiveAdmixtureDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniquePositiveAdmixture
                {
                    m_AbilityList = Tree.BaseBasicEnergy
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

            var feature = Helper.CreateBlueprintFeature("PositiveAdmixtureFeature", "Positive Admixture",
                PositiveAdmixtureDescription, null, icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef2())
                );

            PositiveAdmixture.InfusionFeature = feature.ToRef();
            PositiveAdmixture.InfusionBuff = buff.ToRef();

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, composites: false, basicPhysical: false).ToList().ToArray());
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
                Spore.InfusionFeature,
                Toxic.InfusionFeature,
                GreaterToxic.InfusionFeature
                );
            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures,
                Spore.InfusionFeature,
                Toxic.InfusionFeature,
                GreaterToxic.InfusionFeature
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
            var poison_buff = Helper.CreateBlueprintBuff("SporeInfusionPoison", "Spores", "Taking 1d6 poison damage per round for 10 rounds.",
                null, icon, null, null);
            poison_buff.SetComponents
                (
                Helper.CreateAddFactContextActions(on: null, off: null, round: new GameAction[] { Helper.CreateContextActionDealDamageDirect(poison_damage, poison_duration) })
                );

            var disease_buff = Helper.CreateBlueprintBuff("SporeInfusionDisease", "Poison Puff", "Taking 1d6 Dex damage daily, Cure: 2 consecutive saves",
                null, icon, null, null);
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


            var ability = Helper.CreateBlueprintActivatableAbility("SporeInfusionAbility", "Spore Infusion",
                SporeInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("SporeInfusionFeature", "Spore Infusion",
                SporeInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Spore.InfusionFeature = feature.ToRef();
            Spore.InfusionBuff = buff.ToRef();
        }

        public static void CreateToxicInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("4e42460798665fd4cb9173ffa7ada323"); // Sickened
            var sickened = Helper.ToRef<BlueprintBuffReference>("4e42460798665fd4cb9173ffa7ada323"); // Sickened

            var ability = Helper.CreateBlueprintActivatableAbility("ToxicInfusionAbility", "Toxic Infusion",
                ToxicInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("ToxicInfusionFeature", "Toxic Infusion",
                ToxicInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Toxic.InfusionFeature = feature.ToRef();
            Toxic.InfusionBuff = buff.ToRef();
        }

        public static void CreateGreaterToxicInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("4e42460798665fd4cb9173ffa7ada323"); // Sickened Icon

            var poison_buff = Helper.CreateBlueprintBuff("GreaterToxicInfusionDisease", "Toxic", "Taking 1d2 Con damage for 6 rounds, Cure: 2 consecutive saves",
                null, icon, null, null);
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

            var ability = Helper.CreateBlueprintActivatableAbility("GreaterToxicInfusionAbility", "GreaterToxic Infusion",
                GreaterToxicInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("GreaterToxicInfusionFeature", "GreaterToxic Infusion",
                GreaterToxicInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 14),
                Helper.CreateAddFacts(ability.ToRef2()),
                new RemoveFeatureOnApply
                {
                    m_Feature = AnyRef.Get(Toxic.InfusionFeature).To<BlueprintUnitFactReference>()
                }
                );

            GreaterToxic.InfusionFeature = feature.ToRef();
            GreaterToxic.InfusionBuff = buff.ToRef();
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
            var buff = Helper.CreateBlueprintBuff("SkilledKineticistWoodBuff", "Skilled Kineticist", null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                    ContextRankProgression.Div2, max: 20, classes: new BlueprintCharacterClassReference[1] { Tree.Class }),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillLoreNature)
                );

            var condition = Helper.CreateContextConditionHasFact(AnyRef.Get(WoodFocus.First).To<BlueprintUnitFactReference>());
            var conditional = Helper.CreateConditional(condition,
                ifTrue: buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, false, false, true, true));

            var factContextAction = Kineticist.ref_skilled_kineticist.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref factContextAction.Activated.Actions, conditional);
        }

        private static void AddToExpandedDefense(BlueprintFeature elemental_defense)
        {
            var selection = AnyRef.Get(Kineticist.ref_expanded_defense).To<BlueprintFeatureSelectionReference>().Get();
            Helper.AppendAndReplace(ref selection.m_AllFeatures, elemental_defense.ToRef());
        }

        private static BlueprintFeatureReference CreateWoodHealer()
        {
            var KineticRevivification = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0377fcf4c10871f4187809d273af7f5d"); // KineticRevivificationFeature
            var HealingBurst = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c73b37aaa2b82b44686c56db8ce14e7f"); // HealingBUrstFeature

            UnityEngine.Sprite icon = Helper.CreateSprite("woodHealer.png");
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

            var ability = Helper.CreateBlueprintAbility("WoodHealerAbility", "Wood Healer",
                WoodHealerDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
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

            var feature = Helper.CreateBlueprintFeature("WoodHealerFeature", "Wood Healer",
                WoodHealerDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteFeature(Positive.BlastFeature),
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

            var feature = Helper.CreateBlueprintFeature("WoodlandStepFeature", "Woodland Step",
                WoodlandStepDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteFeature(Positive.BlastFeature),
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

            var buff = Helper.CreateBlueprintBuff("ThornFleshBuff", "Thorn Flesh", 
                ThornFleshDescription, null, icon, onStart);
            buff.Flags(removeOnRest: true);
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

            var ability = Helper.CreateBlueprintAbility("ThornFleshAbility", "Thorn Flesh",
                ThornFleshDescription, null, icon, AbilityType.Supernatural, UnitCommand.CommandType.Standard,
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

            var feature = Helper.CreateBlueprintFeature("ThornFleshFeature", "Thorn Flesh",
                ThornFleshDescription, null, icon, FeatureGroup.KineticWildTalent);
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
            var buff = Helper.CreateBlueprintBuff("HerbalAntivenomBuff", "Herbal Antivenom",
                HerbalAntivenomDescription, null, null);
            buff.Flags(hidden: true, stayOnDeath: true, removeOnRest: false);
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

            var feature = Helper.CreateBlueprintFeature("HerbalAntivenomFeature", "Herbal Antivenom",
                HerbalAntivenomDescription, null, null, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(buff.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateWildGrowth()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"); // Entangle

            var ability = Helper.CreateBlueprintAbility("WildGrowthAbility", "Wild Growth",
                WildGrowthDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, 
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

            var feature = Helper.CreateBlueprintFeature("WildGrowthFeature", "Wild Growth",
                WildGrowthDescription, null, null, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateForestSiege()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("01300baad090d634cb1a1b2defe068d6"); // ClashingRocks

            var ability_target = Helper.CreateBlueprintAbility("ForestSiegeAbilityTarget", "Command Plants",
                ForestSiegeDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
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
                "You may command plant-life to hurl rocks at a target", null, icon);
            buff.Flags(removeOnRest: true, stayOnDeath: true);

            var ability_buff = Helper.CreateBlueprintAbility("ForestSiegeAbilityBuff", "Empower Plant-life",
                ForestSiegeDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Kineticist);
            ability_buff.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(permanent: true)),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var ability_base = Helper.CreateBlueprintAbility("ForestSiegeAbilityBase", "Forest Siege",
                ForestSiegeDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Personal);

            Helper.AddToAbilityVariants(ability_base, ability_buff, ability_target);

            var feature = Helper.CreateBlueprintFeature("ForestSiegeFeature", "Forest Siege",
                ForestSiegeDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
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

            var pool = AnyRef.Get(Helper.CreateBlueprintSummonPool("WoodSoldiersSummonPool", null, 4, true).ToRef()).To<BlueprintSummonPoolReference>();

            #region Golem Buffs

            var buff_autumn = Helper.CreateBlueprintBuff("WoodSoldiersAutumnBuff", "Autumn Soldier", WoodSoldiersDescription, null, icon);
            buff_autumn.SetComponents // Burrow Speed, Resistance to Physical
                (
                new AddDamageResistancePhysical { }
                );
            var buff_spring = Helper.CreateBlueprintBuff("WoodSoldiersSpringBuff", "Spring Soldier", WoodSoldiersDescription, null, icon);
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
            var buff_summer = Helper.CreateBlueprintBuff("WoodSoldiersSummerBuff", "Summer Soldier", WoodSoldiersDescription, null, icon);
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
            var buff_winter = Helper.CreateBlueprintBuff("WoodSoldiersWinterBuff", "Winter Soldier", WoodSoldiersDescription, null, icon);
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
                m_SummonPool = pool,
                CountValue = context_four,
                DurationValue = context_duration_hours,
                UseLimitFromSummonPool = true,
                AfterSpawn = Helper.CreateActionList(
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.Get(Autumn.BlastFeature).To<BlueprintUnitFactReference>()),
                        ifFalse: null,
                        ifTrue: buff_autumn.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.Get(Spring.BlastFeature).To<BlueprintUnitFactReference>()),
                        ifFalse: null,
                        ifTrue: buff_spring.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.Get(Summer.BlastFeature).To<BlueprintUnitFactReference>()),
                        ifFalse: null,
                        ifTrue: buff_summer.CreateContextActionApplyBuff(permanent: true)),
                    Helper.CreateConditional(
                        Helper.CreateContextConditionCasterHasFact(AnyRef.Get(Winter.BlastFeature).To<BlueprintUnitFactReference>()),
                        ifFalse: null,
                        ifTrue: buff_winter.CreateContextActionApplyBuff(permanent: true))
                    )
            };

            var buff = Helper.CreateBlueprintBuff("WoodSoldiersBuff", "Wood Soldier",
                WoodSoldiersDescription, null, icon, null, null);
            buff.Flags(hidden: true, removeOnRest: true, stayOnDeath: true);

            var ability = Helper.CreateBlueprintAbility("WoodSoldiersAbility", "Wood Soldiers",
                WoodSoldiersDescription, null, icon, AbilityType.Supernatural, UnitCommand.CommandType.Free,
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

            var feature = Helper.CreateBlueprintFeature("WoodSoldiersFeature", "Wood Soldiers",
                WoodSoldiersDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>()),
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

            var wild_0 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood", wood_wild_talent_name,
                wood_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_0.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_0.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_0.m_AllFeatures, spell_pen, precise_shot, trip);

            var wild_1 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood1", wood_wild_talent_name,
                wood_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_1.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_1.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_1.m_AllFeatures, spell_pen_greater, precise_shot, trip);

            var wild_2 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood2", wood_wild_talent_name,
                wood_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_2.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_2.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_2.m_AllFeatures, spell_pen, precise_shot, trip_greater);

            var wild_3 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatWood3", wood_wild_talent_name,
                wood_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_3.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                new PrerequisiteSelectionPossible
                {
                    m_ThisFeature = wild_3.ToRef3()
                },
                Helper.CreatePrerequisiteFeature(AnyRef.Get(WoodFocus.Knight).To<BlueprintFeatureReference>(), true)
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

            var unit = Helper.CreateBlueprintUnit("WoodSoldierUnit", "Wood Soldier", null, null, null);
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
            unit.LocalizedName.String = Helper.CreateString("Wood Soldier");
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

            var advanced = Helper.CreateBlueprintUnitTemplate("WoodSoldierAdvancedTemplate", null, adjustments: adjustments);

            unit.m_AdditionalTemplates = new BlueprintUnitTemplateReference[] { AnyRef.Get(advanced.ToRef()).To<BlueprintUnitTemplateReference>() };

            return unit.ToRef2();
        }

        #endregion
    }
}
