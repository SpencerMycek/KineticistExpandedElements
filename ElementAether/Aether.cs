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
using System;
using System.Linq;
using static Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature;
using static Kingmaker.UnitLogic.Mechanics.Properties.BlueprintUnitProperty;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace KineticistElementsExpanded.ElementAether
{
    class Aether : Statics
    {
        private static KineticistTree Tree = KineticistTree.Instance;

        public static void Configure()
        {
            BlueprintFeatureBase aether_class_skills = CreateAetherClassSkills();

            CreateInfusions();

            CreateTelekineticBlast();
            CreateCompositeBlasts();

            Kineticist.AddElementsToInfusion(Tree.Disintegrating, Tree.Composite_Force);
            Kineticist.AddElementsToInfusion(Tree.ForceHook, Tree.Composite_Force);
            Kineticist.AddElementsToInfusion(Tree.FoeThrow, Tree.Telekinetic);
            Kineticist.AddElementsToInfusion(Tree.ManyThrow, Tree.Telekinetic);

            BlueprintFeature force_ward = CreateForceWard();

            Kineticist.AddElementalDefenseIsPrereqFor(Tree.Telekinetic.BlastFeature, Tree.Telekinetic.Blade.Feature, force_ward);

            Kineticist.ElementsBlastSetup(Tree.Telekinetic, Tree.Composite_Force);

            BowlingPushInfusions(Tree.Telekinetic);

            Kineticist.AddBladesToKineticWhirlwind(Tree.Telekinetic, Tree.Composite_Force);

            CreateAetherElementalFocus(aether_class_skills, force_ward);
            CreateKineticKnightAetherFocus(aether_class_skills, force_ward);
            CreateSecondElementAether();
            CreateThirdElementAether();

            CreateAetherWildTalents(force_ward);
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateAetherClassSkills()
        {
            var test = new AddClassSkill();
            var feature = Helper.CreateBlueprintFeature("AetherClassSkills", "Aether Class Skills",
                AetherClassSkillsDescription, null, 0)
                .SetComponents(
                new AddClassSkill  { Skill = StatType.SkillThievery },
                new AddClassSkill { Skill = StatType.SkillKnowledgeWorld }
                );

            return feature;
        }

        private static void BowlingPushInfusions(params KineticistTree.Element[] elements)
        {
            var bowlingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("b3bd080e-ed83-a994-0abd-97e4aa2a7341"); // BowlingInfusionFeature
            var bowlingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("918b2524-af5c-3f64-7b5d-aa4f4e985411"); // BowlingInfusionBuff
            var pushingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("fbb97f35-a41b-71c4-cbc3-6c5f3995b892"); // PushingInfusionFeature
            var pushingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f795bede-8bae-faf4-d9d7-f404ede960ba"); // PushingInfusionBuff

            foreach (var element in elements)
            {
                var prereq = pushingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);
                prereq = bowlingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);

                var applicable = pushingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);
                applicable = bowlingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);

                var trigger = pushingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
                trigger = bowlingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
            }
        }

        #endregion

        #region Elemental Focus Selection

        private static void CreateAetherElementalFocus(BlueprintFeatureBase aether_class_skills, BlueprintFeatureBase force_ward_feature)
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("ElementalFocusAether", "Aether",
                ElementalFocusAetherDescription, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Kineticist.ref_blood_kineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(1, Tree.Telekinetic.Progession, aether_class_skills);
            var entry2 = Helper.CreateLevelEntry(2, force_ward_feature);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, AnyRef.ToAny(Tree.FocusAether.First));
        }

        private static void CreateKineticKnightAetherFocus(BlueprintFeatureBase aether_class_skills, BlueprintFeatureBase force_ward_feature)
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusAether", "Aether",
                ElementalFocusAetherDescription, null,
                FeatureGroup.KineticElementalFocus);

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(1, Tree.Telekinetic.Progession, aether_class_skills);
            var entry2 = Helper.CreateLevelEntry(4, force_ward_feature);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, AnyRef.ToAny(Tree.FocusAether.Knight));
        }

        private static void CreateSecondElementAether() 
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("SecondaryElementAether", "Aether",
                ElementalFocusAetherDescription, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusAether.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusAether.Knight))
                    ), 
                    new GameAction[] { Helper.CreateAddFact(new FactOwner(), AnyRef.ToAny(Tree.Composite_Force.BlastFeature)) }

                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(7, Tree.Telekinetic.Progession);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, AnyRef.ToAny(Tree.FocusAether.Second));
        }

        private static void CreateThirdElementAether() 
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementAether", "Aether",
                ElementalFocusAetherDescription, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusAether.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusAether.Knight))
                        ),
                    new GameAction[] { Helper.CreateAddFact(new FactOwner(), AnyRef.ToAny(Tree.Composite_Force.BlastFeature)) }
                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff)),
                Helper.CreatePrerequisiteNoFeature(AnyRef.ToAny(Tree.FocusAether.Second))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(15, Tree.Telekinetic.Progession);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, AnyRef.ToAny(Tree.FocusAether.Third));
        }

        #endregion

        #region Force Ward

        private static BlueprintFeature CreateForceWard()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/forceWard.png");

            #region Effect Feature

            var effect_feature = Helper.CreateBlueprintFeature("ForceWardEffectFeature", icon: icon, group: FeatureGroup.None);
            effect_feature.Ranks = 20;
            effect_feature.HideInUI = true;
            effect_feature.HideInCharacterSheetAndLevelUp = true;
            effect_feature.IsClassFeature = true;
            effect_feature.SetComponents
                (
                Helper.CreateAddFacts(new BlueprintUnitFactReference[] { })
                );

            #endregion
            #region Effect Buff

            var effect_buff = Helper.CreateBlueprintBuff("ForceWardEffectBuff", icon: icon);
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

            var feature_value_getter = new FeatureRankPlusBonusGetter()
            {
                Feature = effect_feature.ToRef(),
                bonus = 4
            };
            var classlvl_value_getter = new ClassLevelGetter()
            {
                ClassRef = Tree.Class
            };
            var temp_hp_progression = Helper.CreateBlueprintUnitProperty("ForceWardHPProperty")
                .SetComponents
                (
                feature_value_getter,
                classlvl_value_getter
                );
            temp_hp_progression.OperationOnComponents = MathOperation.Multiply;
            temp_hp_progression.BaseValue = 1;

            var calculateShared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 0.5,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.CasterCustomProperty,
                        m_CustomProperty = temp_hp_progression.ToRef()
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0
                    }
                }
            };
            var temp_hp = new TemporaryHitPointsUnique
            {
                Value = new ContextValue
                {
                    Value = 1,
                    ValueType = ContextValueType.Shared,
                    ValueShared = AbilitySharedValue.Damage
                },
                RemoveWhenHitPointsEnd = false,
                Descriptor = ModifierDescriptor.UntypedStackable
            };
            var regen = new RegenTempHpPerMinute(Tree.Class, effect_feature);



            var buff = Helper.CreateBlueprintBuff("ForceWardBuff", icon: icon);
            buff.Flags(hidden: true, stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.SetComponents
                (
                temp_hp,
                regen,
                calculateShared,
                Helper.CreateRecalculateOnFactsChange(effect_feature.ToRef2())
                );

            // TEMP TODO REMOVE
            var fw_buff_combat_refresh = Helper.CreateBlueprintBuff("ForceWardBuffCombatRefresh", "FW Buff Refresh", icon: icon);
            fw_buff_combat_refresh.Flags(true, true, null, null)
                .SetComponents
                (
                Helper.CreateAddFacts(effect_feature.ToRef2())
                );
            fw_buff_combat_refresh.Stacking = StackingType.Prolong;
            var fw_resource = Helper.CreateBlueprintAbilityResource("ForceWardResource", "Force Ward", ForceWardDescription);
            // TEMP TODO REMOVE

            #endregion
            #region Ability

            var ability = Helper.CreateBlueprintAbility("ForceWardAbility", "Force Ward", ForceWardDescription,
                icon, AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            ability.AvailableMetamagic = Metamagic.Heighten;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var feature = Helper.CreateBlueprintFeature("ForceWardFeature", "Force Ward",
                ForceWardDescription, icon, FeatureGroup.None);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreateAddFacts(buff.ToRef2(), ability.ToRef2()),
                Helper.CreatePrerequisiteFeature(Tree.Telekinetic.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Telekinetic.Blade.Feature, any: true)
                );

            return feature;

        }

        #endregion

        #region Telekinetic Blast

        private static void CreateTelekineticBlast()
        {
            // Variants
            var standard = CreateTelekineticBlastVariant_base();
            var extended = CreateTelekineticBlastVariant_extended();
            var spindle = CreateTelekineticBlastVariant_spindle();
            var wall = CreateTelekineticBlastVariant_wall();
            var blade = CreateTelekineticBlastVariant_blade();
            var foeThrow = CreateTelekineticBlastVariant_throw(); // Output not used due to UI reqs
            var many = CreateTelekineticBlastVariant_many();
            // Ability
            CreateTelekineticBlastAbility(standard, many, extended, spindle, wall, blade);
            // Feature
            CreateTelekineticBlastFeature();
            // Progression
            CreateTelekineticBlastProgression();

        }
        
        #region Blast Variants

        private static BlueprintAbility CreateTelekineticBlastVariant_base()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/telekineticBlast.png");

            var ability = Helper.CreateBlueprintAbility("TelekineticBlastAbility", "Telekinetic Blast", TelekineticBlastDescription, 
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions,
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(), 
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.BatteringBlast00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            // Bandaids
            ((ContextActionDealDamage)actions.Actions[0]).UseWeaponDamageModifiers = true;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;

            return ability;
        }
        private static BlueprintAbility CreateTelekineticBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeTelekineticBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, 
                    p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, 
                    isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.BatteringBlast00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Telekinetic.BaseAbility;
            
            // Bandaid
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;

            return ability;
        }
        private static BlueprintAbility CreateTelekineticBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleTelekineticBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, 
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, 
                    p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, 
                    isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.BatteringBlast00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.BatteringBlast00.ToRef<BlueprintProjectileReference>(),
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
            ability.m_Parent = Tree.Telekinetic.BaseAbility;

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage));
            var action_damage = Helper.CreateContextActionDealDamage(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var context_conditional_saved = Helper.CreateContextActionConditionalSaved(null, action_damage);
            actions.Actions = new GameAction[] { context_conditional_saved };

            return ability;
        }
        private static BlueprintAbility CreateTelekineticBlastVariant_wall()
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
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Telekinetic", "4ffc8d2162a215e44a1a728752b762eb", p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallTelekineticBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
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
            ability.m_Parent = Tree.Telekinetic.BaseAbility;

            return ability;
        }
        private static BlueprintAbility CreateTelekineticBlastVariant_many()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/manyThrow.png");

            var ability = Helper.CreateBlueprintAbility("ManyThrowTelekineticBlast", "Many Throw", ManyThrowInfusionDescription, 
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions,
                    p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                    isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 4, blast: 0, talent: 0),
                Kineticist.Blast.RequiredFeat(Tree.ManyThrow.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverMultiAttack
                {
                    Condition = null,
                    Weapon = "65951e1195848844b8ab8f46d942f6e8".ToRef<BlueprintItemWeaponReference>(),
                    Projectiles = new BlueprintProjectileReference[]
                        {
                            Resource.Projectile.MagicMissile00.ToRef<BlueprintProjectileReference>(),
                            Resource.Projectile.MagicMissile01.ToRef<BlueprintProjectileReference>(),
                            Resource.Projectile.MagicMissile02.ToRef<BlueprintProjectileReference>(),
                            Resource.Projectile.MagicMissile03.ToRef<BlueprintProjectileReference>(),
                            Resource.Projectile.MagicMissile04.ToRef<BlueprintProjectileReference>(),
                        },
                    TargetType = TargetType.Enemy,
                    DelayBetweenChain = 0f,
                    radius = new Feet { m_Value = 30 },
                    TargetsCount = Helper.CreateContextValue(AbilityRankType.ProjectilesCount)
                },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.AsIs, type: AbilityRankType.ProjectilesCount,
                    classes: new BlueprintCharacterClassReference[] { Tree.Class })
                ).TargetPoint(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Telekinetic.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;

            return ability;
        }
        private static BlueprintAbility CreateTelekineticBlastVariant_throw()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/foeThrow.png");

            var foeThrowBuff = CreateFoeThrowTargetBuff();
            var ft_targetAbility = CreateFoeThrowTargetAbility(foeThrowBuff, Tree.FoeThrow.Feature);
            var ft_throwAbility = CreateFoeThrowThrowAbility(foeThrowBuff, Tree.FoeThrow.Feature);

            var ability = Helper.CreateBlueprintAbility("FoeThrowTelekineticBlast", "Foe Throw", FoeThrowInfusionDescription, 
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, infusion: 2, blast: 0, talent: 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Telekinetic.BaseAbility;

            Helper.AddToAbilityVariants(ability, ft_targetAbility);
            Helper.AddToAbilityVariants(ability, ft_throwAbility);

            Tree.FoeThrow.Feature.Get().AddComponents(Helper.CreateAddFacts(ability.ToRef2()));

            return ability;
        }

        #region Kinetic Blade: Telekinetic

        private static BlueprintAbility CreateTelekineticBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea31-3b9a-9cb4-d86b-bbca01b90346"); // KineticBladeAirBlastAbility
            var damage_icon = Helper.StealIcon("89cc522f-2e14-44b4-0ba1-757320c58530"); // AirBlastKineticBladeDamage

            var weapon = CreateTelekineticBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeTelekineticBlastBuff");
            buff.Flags(true, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = AnyRef.ToAny(weapon) }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeTelekineticBlastAbility", out var unused, "Telekinetic Blast — Kinetic Blade",
                KineticBladeDescription, icon, group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeTelekineticBlastBurnAbility", null, null, icon,
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

            var blade_damage_ability = Helper.CreateBlueprintAbility("TelekineticBlastKineticBladeDamage", "Telekinetic Blast",
                TelekineticBlastDescription, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, 
                    p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                Kineticist.Blast.RankConfigDice(false, false),
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

            var blade_feat = Helper.CreateBlueprintFeature("TelekineticKineticBladeFeature", null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateTelekineticBlastBlade_weapon()
        {
            var weapon = Helper.CreateBlueprintItemWeapon("AetherKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero }, price: 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateTelekineticBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateTelekineticBlastBlade_enchantment()
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
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.KineticBlast, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.KineticistMainStatProperty, min: 0, max: 20);
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

            var enchant = Helper.CreateBlueprintWeaponEnchantment("AetherKineticBladeEnchantment", "Telekinetic Blast — Kinetic Blade", null, "Telekinetic Blast");
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "19d9b36b62efe1448b00630ec53db58c" };

            return enchant;
        }

        #endregion

        #endregion

        private static void CreateTelekineticBlastAbility(params BlueprintAbility[] variants)
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/telekineticBlast.png");

            var ability = Helper.CreateBlueprintAbility("TelekineticBlastBase", "Telekinetic Blast", TelekineticBlastDescription,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
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

        private static void CreateTelekineticBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("TelekineticBlastFeature", "Telekinetic Blast",
                TelekineticBlastDescription, null, FeatureGroup.KineticBlast)
                .SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Telekinetic.BaseAbility))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        private static void CreateTelekineticBlastProgression()
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("TelekineticBlastProgression", "Telekinetic Blast",
                TelekineticBlastDescription, null, 0);
            progression.SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature), 
                    AnyRef.ToAny(Tree.Telekinetic.Blade.Feature)),
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Telekinetic.BlastFeature))
                );

            var entry = Helper.CreateLevelEntry(1, Tree.Telekinetic.BlastFeature);
            Helper.AddEntries(progression, entry);
        }

        #endregion

        #region Composite Blast

        private static void CreateCompositeBlasts()
        {
            CreateForceBlast();
        }

        private static void CreateForceBlast()
        {
            // Variants
            var standard = CreateForceBlastVariant_base();
            var extended = CreateForceBlastVariant_extended();
            var spindle = CreateForceBlastVariant_spindle();
            var wall = CreateForceBlastVariant_wall();
            var blade = CreateForceBlastVariant_blade();
            var hook = CreateForceBlastVariant_hook();
            // Ability
            CreateForceBlastAbility(standard, hook, extended, spindle, wall, blade);
            // Feature
            CreateForceBlastFeature();

            // TODO
            //CreateAethericBoost(out lesserAethericBoost, out greaterAethericBoost, out lesserAethericBuff, out greaterAethericBuff);
        }      

        #region Force variants

        public static AbilityEffectRunAction CreateForceBlastRunAction()
        {
            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealForceDamage(dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

            return runaction;
        }

        public static BlueprintAbility CreateForceBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/forceBlast.png");

            var ability = Helper.CreateBlueprintAbility("ForceBlastAbility", "Force Blast", ForceBlastDescription, 
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                CreateForceBlastRunAction(), // Force Damage (Force with fire, same as battering blast)
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 2),
                Kineticist.Blast.Projectile(Resource.Projectile.Disintegrate00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.SpellDescriptor(SpellDescriptor.Force),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            return ability;
        }
        public static BlueprintAbility CreateForceBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeForceBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description,
                icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.SetComponents
                (
                CreateForceBlastRunAction(), // Force Damage (Force with fire, same as battering blast)
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 1, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.Disintegrate00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.SpellDescriptor(SpellDescriptor.Force),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Force.BaseAbility;

            return ability;
        }
        public static BlueprintAbility CreateForceBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var force_runAction = CreateForceBlastRunAction();
            var context_dealDamage = force_runAction.Actions.Actions[0];
            var context_conditional_saved = Helper.CreateContextActionConditionalSaved(null, context_dealDamage);
            force_runAction.Actions.Actions = new GameAction[] { context_conditional_saved };

            var ability = Helper.CreateBlueprintAbility(
                "SplindleForceBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                force_runAction,
                Kineticist.Blast.RankConfigDice(twice: true),Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
                Kineticist.Blast.SpellDescriptor(SpellDescriptor.Force),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.Disintegrate00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.Disintegrate00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                        ValueRank = AbilityRankType.ProjectilesCount,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Force.BaseAbility;

            return ability;
        }
        public static BlueprintAbility CreateForceBlastVariant_wall()
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
                m_AreaEffect = AnyRef.ToAny(CreateForceWallEffect()),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallForceBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 1, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.SpellDescriptor(SpellDescriptor.Force),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Force.BaseAbility;

            return ability;
        }
        public static BlueprintAbility CreateForceBlastVariant_hook()
        {
            var ability = Helper.CreateBlueprintAbility("ForceHookForceBlastAbility", "Force Hook", ForceHookInfusionDescription,
                null, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            ability.SetComponents
                (
                CreateForceBlastRunAction(), // Force Damage (Force with fire, same as battering blast)
                Kineticist.Blast.RankConfigDice(twice: true),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.ForceHook.Feature),
                Kineticist.Blast.SpellDescriptor(SpellDescriptor.Force),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityCustomMoveToTarget
                {
                    m_Projectile = Resource.Projectile.Disintegrate00.ToRef<BlueprintProjectileReference>(),
                    DisappearFx = new PrefabLink { AssetId = "5caa897344a18ea4e9f7e3368eb2f19b" },
                    DisappearDuration = 0.1f,
                    AppearFx = new PrefabLink { AssetId = "4fa8c88064e270a4594f534c2a65198d" },
                    AppearDuration = 0.1f
                    }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Force.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Force

        private static BlueprintAbility CreateForceBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateForceBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeForceBlastBuff");
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = AnyRef.ToAny(weapon) }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeForceBlastAbility",out var unused, "Force Blast — Kinetic Blade",
                KineticBladeDescription, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeForceBlastBurnAbility", null, null, icon,
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

            var blade_damage_ability = Helper.CreateBlueprintAbility("ForceBlastKineticBladeDamage", "Force Blast",
                ForceBlastDescription, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
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

            var blade_feat = Helper.CreateBlueprintFeature("ForceKineticBladeFeature", null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateForceBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("ForceKineticBladeWeapon", weaponType: Kineticist.ref_kinetic_blast_energy_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero }, price: 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateForceBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateForceBlastBlade_enchantment()
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
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.KineticBlast, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.KineticistMainStatProperty, min: 0, max: 20);
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

            var enchant = Helper.CreateBlueprintWeaponEnchantment("ForceKineticBladeEnchantment", "Force Blast — Kinetic Blade",
                "Force Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "fafefd27475150f499b5c7275a851f2f" }; // EarthKineticBladeEnchantment

            return enchant;
        }

        #endregion

        #endregion

        private static void CreateForceBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/forceBlast.png");

            var ability = Helper.CreateBlueprintAbility("ForceBlastBase", "Force Blast", ForceBlastDescription,
                icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
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

        private static void CreateForceBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("ForceBlastFeature", "Force Blast", ForceBlastDescription, 
                null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Force.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    Tree.KineticBlade.Feature,
                    Tree.Composite_Force.Blade.Feature)
                );
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }


        // Aetheric Boost (Buff, maybe?)
        //  Provide a buff/toggle with the same scaling as blast dice as bonus damage
        #region Aetheric Boost
        /*
        private static void CreateAethericBoost(out BlueprintFeature lesserAethericBoost, out BlueprintFeature greaterAethericBoost, out BlueprintBuff lesserAethericBuff, out BlueprintBuff greaterAethericBuff)
        {
            CreateLesserAethericBoost(out lesserAethericBoost, out lesserAethericBuff);
            CreateGreaterAethericBoost(out greaterAethericBoost, out greaterAethericBuff);
        }

        private static void CreateLesserAethericBoost(out BlueprintFeature lesserAethericBoost, out BlueprintBuff lesserAethericBuff)
        {
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2764b5504e98e6824cab3d27c"); // KineticBlastFeature

            var dice = Helper.CreateContextDiceValue(DiceType.Zero, null, Helper.CreateContextValue(AbilityRankType.DamageBonus));
            var dealDamage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice);

            var trigger = new AddKineticistInfusionDamageTrigger
            {
                Actions = new ActionList { Actions = new GameAction[] { dealDamage } },
                m_WeaponType = null,
                CheckSpellParent = true,
                TriggerOnDirectDamage = true
            };

            var contextRank = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageBonus, max: 20, feature: kinetic_blast_feature.ToRef());

            var recalc = new RecalculateOnStatChange
            {
                UseKineticistMainStat = true,
                Stat = StatType.Unknown
            };

            lesserAethericBuff = Helper.CreateBlueprintBuff("AethericBoostLesserBuff", "Aetheric Boost",
                AethericBoostLesserDescription, null, null, null);
            lesserAethericBuff.Stacking = StackingType.Replace;
            lesserAethericBuff.Flags(false, true);
            lesserAethericBuff.SetComponents
                (
                trigger, 
                contextRank,
                recalc
                );

            lesserAethericBoost = Helper.CreateBlueprintFeature("AethericBoostLesser", "Aetheric Boost",
                AethericBoostLesserDescription, null, null, FeatureGroup.None);
            lesserAethericBoost.SetComponents
                (
                Helper.CreateAddFactContextActions
                    (
                        new GameAction[] { lesserAethericBuff.CreateContextActionApplyBuff(asChild: true, permanent: true) }
                    )
                );
        }

        private static void CreateGreaterAethericBoost(out BlueprintFeature greaterAethericBoost, out BlueprintBuff GreaterAethericBuff)
        {
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2764b5504e98e6824cab3d27c"); // KineticBlastFeature

            var dice = Helper.CreateContextDiceValue(DiceType.Zero, null, Helper.CreateContextValue(AbilityRankType.DamageBonus));
            var dealDamage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice);

            var trigger = new AddKineticistInfusionDamageTrigger
            {
                Actions = new ActionList { Actions = new GameAction[] { dealDamage } },
                m_WeaponType = null,
                CheckSpellParent = true,
                TriggerOnDirectDamage = true,
            };

            var contextRank = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, progression: ContextRankProgression.MultiplyByModifier, type: AbilityRankType.DamageBonus, stepLevel: 2, feature: kinetic_blast_feature.ToRef());

            var recalc = new RecalculateOnStatChange
            {
                UseKineticistMainStat = true,
                Stat = StatType.Unknown
            };

            GreaterAethericBuff = Helper.CreateBlueprintBuff("AethericBoostGreaterBuff", "Aetheric Boost",
                AethericBoostGreaterDescription, null, null);
            GreaterAethericBuff.Stacking = StackingType.Replace;
            GreaterAethericBuff.Flags(false, true);
            GreaterAethericBuff.SetComponents
                (
                trigger,
                contextRank,
                recalc
                );

            greaterAethericBoost = Helper.CreateBlueprintFeature("AethericBoostGreater", "Aetheric Boost",
                AethericBoostGreaterDescription, null, FeatureGroup.None);
            greaterAethericBoost.SetComponents
                (
                Helper.CreateAddFactContextActions
                    (
                        new GameAction[] { GreaterAethericBuff.CreateContextActionApplyBuff(asChild: true, permanent: true) }
                    )
                );
        }


        private static void limitAethericBoosts(BlueprintBuff lab, BlueprintBuff gab, BlueprintAbilityReference[] custom_simple, BlueprintAbilityReference[] custom_composite)
        {
            try
            {
                var simple_fire = Helper.ToRef<BlueprintAbilityReference>("83d5873f306ac954cad95b6aeeeb2d8c"); // FireBlastBase
                var simple_earth = Helper.ToRef<BlueprintAbilityReference>("e53f34fb268a7964caf1566afb82dadd"); // EarthBlastBase
                var simple_air = Helper.ToRef<BlueprintAbilityReference>("0ab1552e2ebdacf44bb7b20f5393366d"); // AirBlastBase
                var simple_elec = Helper.ToRef<BlueprintAbilityReference>("45eb571be891c4c4581b6fcddda72bcd"); // ElectricBlastBase
                var simple_water = Helper.ToRef<BlueprintAbilityReference>("d663a8d40be1e57478f34d6477a67270"); // WaterBlastBase
                var simple_cold = Helper.ToRef<BlueprintAbilityReference>("7980e876b0749fc47ac49b9552e259c1"); // ColdBlastBase

                var composite_sand = Helper.ToRef<BlueprintAbilityReference>("b93e1f0540a4fa3478a6b47ae3816f32"); // SandstormBlastBase
                var composite_plasma = Helper.ToRef<BlueprintAbilityReference>("9afdc3eeca49c594aa7bf00e8e9803ac"); // PlasmaBlastBase
                var composite_blizzard = Helper.ToRef<BlueprintAbilityReference>("16617b8c20688e4438a803effeeee8a6"); // BlizzardBlastBase
                var composite_chargeWater = Helper.ToRef<BlueprintAbilityReference>("4e2e066dd4dc8de4d8281ed5b3f4acb6"); // ChargedWaterBlastBase
                var composite_magma = Helper.ToRef<BlueprintAbilityReference>("8c25f52fce5113a4491229fd1265fc3c"); // MagmaBlastBase
                var composite_mud = Helper.ToRef<BlueprintAbilityReference>("e2610c88664e07343b4f3fb6336f210c"); // MudBlastBase
                var composite_steam = Helper.ToRef<BlueprintAbilityReference>("3baf01649a92ae640927b0f633db7c11"); // SteamBlastBase

                custom_simple = custom_simple.Append(simple_air, simple_cold, simple_earth, simple_elec, simple_fire, simple_water);
                custom_composite = custom_composite.Append(composite_blizzard, composite_chargeWater, composite_magma, composite_mud, composite_plasma, composite_sand, composite_steam);

                var lab_trigger = lab.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref lab_trigger.m_AbilityList, custom_simple);
                var gab_trigger = gab.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref gab_trigger.m_AbilityList, custom_composite);
            } catch (Exception ex)
            {
                Helper.PrintNotification($"Exception: {ex.Message}");
            }
        }
        */
        #endregion

        #endregion

        #region Infusions

        public static void CreateInfusions()
        {
            CreateDisintegratingInfusion();
            CreateManyThrowInfusion();
            CreateFoeThrowInfusion();
            CreateForceHookInfusion();

            Kineticist.TryDarkCodexAddExtraWildTalent(Tree.Disintegrating.Feature, Tree.ManyThrow.Feature, Tree.FoeThrow.Feature, Tree.ForceHook.Feature);
            Helper.AppendAndReplace(ref Tree.SelectionInfusion.Get().m_AllFeatures, Tree.Disintegrating.Feature, Tree.ManyThrow.Feature, Tree.FoeThrow.Feature, Tree.ForceHook.Feature);
        }

        public static void CreateDisintegratingInfusion()
        {
            var disintegrate_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f7a6a7d2cfeb36643915aece45349827"); // DisintegrateBuff
            var icon = Helper.StealIcon("4aa7942c3e62a164387a73184bca3fc1"); // Disintegrate Icon

            #region ability

            var ability = Helper.CreateBlueprintActivatableAbility("DisintegratingInfusionAbility", out var buff, "Disintegrating Infusion",
                DisintegratingInfusionDescription, icon, activationType: Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                deactivateImmediately: true, group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, onByDefault: true);
            ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;

            #endregion

            #region Custom Damage

            ContextRankConfig config_dice = Helper.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, progression: ContextRankProgression.MultiplyByModifier, stepLevel: 4, feature: Tree.KineticBlast);
            ContextRankConfig config_bonus = Helper.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, progression: ContextRankProgression.Div2, stat: StatType.Constitution, customProperty: Tree.KineticistMainStatProperty);


            ContextDiceValue value = Helper.CreateContextDiceValue(DiceType.D6, diceCount: Helper.CreateContextValue(AbilityRankType.DamageDice), bonus: Helper.CreateContextValue(AbilityRankType.DamageBonus));

            #endregion

            #region Disintegration

            var apply_buff = disintegrate_buff.CreateContextActionApplyBuff(permanent: true);

            var check_health_less_zero = new ContextConditionCompareTargetHP
            {
                Not = false,
                m_CompareType = ContextConditionCompareTargetHP.CompareType.Less,
                Value = new ContextValue
                {
                    ValueType = ContextValueType.Simple,
                    Value = 0
                }
            };

            var disintegrate_conditional = Helper.CreateConditional(check_health_less_zero, apply_buff);


            #endregion

            #region Buff Components

            var disintegrateNullifyDamage = new AbilityUniqueDisintegrateInfusion(Tree.Composite_Force.BaseAbility)
            {
                Actions = new ActionList {  Actions = new GameAction[] { disintegrate_conditional } },
                Value = value
            };

            var burn_modifier = new AddKineticistBurnModifier
            {
                BurnType = KineticistBurnType.Infusion,
                Value = 4
            };
            var calc_abilityParams = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = Tree.Class
            };
            var recalc_stat_change = new RecalculateOnStatChange
            {
                Stat = StatType.Unknown,
                UseKineticistMainStat = true
            };

            #endregion

            #region Buff

            buff.Flags(stayOnDeath: true);
            buff.SetComponents
                (
                disintegrateNullifyDamage,
                config_dice,
                config_bonus,
                calc_abilityParams,
                burn_modifier,
                recalc_stat_change
                );

            #endregion

            var feature = Helper.CreateBlueprintFeature("DisintegratingInfusionFeature", "Disintegrating Infusion",
                DisintegratingInfusionDescription, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(ability)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 12, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusFirst))
                );
        }

        public static void CreateManyThrowInfusion()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/manyThrow.png");

            var feature = Helper.CreateBlueprintFeature("ManyThrowInfusion", "Many Throw", 
                ManyThrowInfusionDescription, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 16),
                Helper.CreatePrerequisiteFeaturesFromList(false, Kineticist.ref_infusion_extendedRange),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusFirst))
                );
        }

        public static void CreateForceHookInfusion()
        {
            UnityEngine.Sprite icon = null;

            var feature = Helper.CreateBlueprintFeature("ForceHookInfusion", "Force Hook",
                ForceHookInfusionDescription, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusFirst))
                );
        }

        #region Foe Throw

        public static void CreateFoeThrowInfusion()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/foeThrow.png");

            var feature = Helper.CreateBlueprintFeature("FoeThrowInfusion", "Foe Throw",
                FoeThrowInfusionDescription, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusFirst))
                );
        }

        public static BlueprintBuff CreateFoeThrowTargetBuff()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/foeThrow.png");

            var buff = Helper.CreateBlueprintBuff("FoeThrowInfusionTargetBuff", "Lifted",
                FoeThrowTargetBuffDescription, icon, null);
            buff.Flags();
            buff.Stacking = StackingType.Replace;
            return buff;
        }

        public static BlueprintAbility CreateFoeThrowTargetAbility(BlueprintBuff foeThrowBuff, BlueprintFeature requirement)
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/foeThrow.png");

            var ability = Helper.CreateBlueprintAbility("FoeThrowInfusionTargetAbility", "Lift Target",
                FoeThrowInfusionTargetAbilityDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Free,
                AbilityRange.Close, null, null);
            ability.SetComponents
                (
                Kineticist.Blast.RequiredFeat(requirement),
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, new ContextActionRemoveBuffAll() { m_Buff = foeThrowBuff }, foeThrowBuff.CreateContextActionApplyBuff(1, DurationRate.Rounds))
                ).TargetEnemy(CastAnimationStyle.Kineticist);
                
            return ability;
        }

        public static BlueprintAbility CreateFoeThrowThrowAbility(BlueprintBuff foeThrowBuff, BlueprintFeature requirement)
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/foeThrow.png");

            var ability = Helper.CreateBlueprintAbility("FoeThrowInfusionThrowAbility", "Throw Target",
                FoeThrowInfusionThrowAbilityDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Close, null, null);
            ability.SetComponents
                (
                new AbilityCustomFoeThrowUnique
                {
                    m_Projectile = Resource.Projectile.BatteringBlast00.ToRef<BlueprintProjectileReference>(),
                    DisappearFx = new PrefabLink { AssetId = "5caa897344a18ea4e9f7e3368eb2f19b" },
                    DisappearDuration = 0f,
                    AppearFx = new PrefabLink { AssetId = "4fa8c88064e270a4594f534c2a65198d" },
                    AppearDuration = 0f,
                    m_Buff = foeThrowBuff,
                    Value = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage))
                },
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(requirement),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, new ContextActionRemoveBuffAll { m_Buff = foeThrowBuff })
                ).TargetEnemy(CastAnimationStyle.Kineticist);

            return ability;
        }

        #endregion

        #endregion

        #region Wild Talents

        private static void CreateAetherWildTalents(BlueprintFeature elemental_defense)
        {
            AddToSkilledKineticist();
            AddToKineticHealer();
            AddToExpandedDefense(elemental_defense);

            (var wild_0, var wild_1, var wild_2, var wild_3) = CreateWildTalentBonusFeatAether();
            (var selfTK_lesser, var selfTK_greater) = CreateSelfTelekinesis();
            BlueprintFeatureReference invis = CreateTelekineticInvisibility();
            BlueprintFeatureReference tf_feat = CreateTelekineticFinesse();
            BlueprintFeatureReference maneuvers = CreateTelekineticManeuvers(tf_feat);
            BlueprintFeatureReference touchsite = CreateTouchsiteReactive();
            BlueprintFeatureReference spell_deflection = CreateSpellDeflection();

            Kineticist.TryDarkCodexAddExtraWildTalent(wild_0, wild_1, wild_2, wild_3, selfTK_lesser, selfTK_greater, invis, tf_feat, maneuvers, touchsite, spell_deflection);
            Kineticist.AddToWildTalents(wild_0, wild_1, wild_2, wild_3, selfTK_lesser, selfTK_greater, invis, tf_feat, maneuvers, touchsite, spell_deflection);
        }

        private static void AddToSkilledKineticist()
        {
            var skilled_kineticist_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("56b70109d78b0444cb3ad04be3b1ee9e"); // SkilledKineticistBuff

            var buff = Helper.CreateBlueprintBuff("SkilledKineticistAetherBuff", "Skilled Kineticist");
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                    ContextRankProgression.Div2, max: 20, classes: new BlueprintCharacterClassReference[1] { Tree.Class }),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillThievery),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillKnowledgeWorld)
                );

            var condition = Helper.CreateContextConditionHasFact(AnyRef.ToAny(Tree.FocusAether.First));
            var conditional = Helper.CreateConditional(condition,
                ifTrue: buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, false, false, true, true));

            var factContextAction = skilled_kineticist_buff.GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref factContextAction.Activated.Actions, conditional);
        }

        private static void AddToKineticHealer()
        {
            var feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3ef66697-3adf-a8f4-0af6-c0679bd98ba5"); // Kinetic Healer Feature
            var feat_preq = feat.GetComponent<PrerequisiteFeaturesFromList>();
            Helper.AppendAndReplace(ref feat_preq.m_Features,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature));
        }

        private static void AddToExpandedDefense(BlueprintFeature fw_feature)
        {
            var selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("d741f298-dfae-8fc4-0b46-15aaf83b6548"); // Kineticist Expanded Defense Selection
            Helper.AppendAndReplace(ref selection.m_AllFeatures, fw_feature.ToRef());
        }

        private static BlueprintFeatureReference CreateTelekineticInvisibility()
        {
            var invis_buff_icon = Helper.StealIcon("525f980c-b29b-c224-0b93-e953974cb325"); // Invisibility Effect Buff Icon
            var invis_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("525f980c-b29b-c224-0b93-e953974cb325"); // Invisibility Effect Buff

            var ti_ability = Helper.CreateBlueprintAbility("TelekineticInvisibiltyAbility", "Telekinetic Invisibility",
                TelekineticInvisibilityDescription, invis_buff_icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Personal);
            ti_ability.TargetSelf();
            ti_ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction
                    (
                    SavingThrowType.Unknown,
                    invis_buff.CreateContextActionApplyBuff(1, DurationRate.Hours, false, false, false, true)
                    ),
                Helper.CreateAbilityAcceptBurnOnCast(0)
                );

            var ti_feat = Helper.CreateBlueprintFeature("TelekineticInvisibilityFeature", "Telekinetic Invisibility",
                TelekineticInvisibilityDescription, invis_buff_icon, FeatureGroup.KineticWildTalent);
            ti_feat.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ti_ability.ToRef2())
                );

            return ti_feat.ToRef();
        }
        
        private static BlueprintFeatureReference CreateTelekineticFinesse()
        {
            var icon = Helper.StealIcon("d6d68c99-6016-e1c4-e85e-cd0ee0067c29"); // Ranged Legerdemain

            var tf_ability = Helper.CreateBlueprintActivatableAbility("TelekineticFinesseToggleAbility", out var tf_buff, "Telekinetic Finesse",
                TelekineticFinesseDescription, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, false, false, false, false, false, false, false, false, false, 1);

            var mech_feature = new AddMechanicsFeature { m_Feature = MechanicsFeatureType.RangedLegerdemain };

            tf_buff.Flags(null, true, null, null);
            tf_buff.Stacking = StackingType.Replace;
            tf_buff.SetComponents
                (
                mech_feature,
                Helper.CreateAddContextStatBonus(Helper.CreateContextValue(AbilityRankType.StatBonus),StatType.SkillThievery),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Div2, AbilityRankType.StatBonus, classes: new BlueprintCharacterClassReference[] {Tree.Class})
                );

            var tf_feat = Helper.CreateBlueprintFeature("TelekineticFinesseFeature", "Telekinetic Finesse",
                TelekineticFinesseDescription, icon, FeatureGroup.KineticWildTalent);
            tf_feat.HideInCharacterSheetAndLevelUp = true;
            tf_feat.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 1),
                Helper.CreateAddFacts(AnyRef.ToAny(tf_ability))
                );

            return tf_feat.ToRef();
        }
        
        #region Telekinetic Maneuvers
        private static BlueprintFeatureReference CreateTelekineticManeuvers(BlueprintFeature finesse_feat)
        {
            var variant_trip = CreateVariant_TM_Trip();
            var variant_disarm = CreateVariant_TM_Disarm();
            var variant_bullRush = CreateVariant_TM_BullRush();
            var variant_dt_blind = CreateVariant_TM_DirtyTrick_Blind(finesse_feat);
            var variant_dt_entangle = CreateVariant_TM_DirtyTrick_Entangle(finesse_feat);
            var variant_dt_sickened = CreateVariant_TM_DirtyTrick_Sickened(finesse_feat);
            var variant_pull = CreateVariant_TM_Pull(); ;
            var feature = CreateTeleKineticManeuversFeature(variant_trip, variant_disarm, variant_bullRush, variant_dt_blind, variant_dt_entangle, variant_dt_sickened, variant_pull);
            return feature;
        }

        #region tm_variants
        private static BlueprintAbility CreateVariant_TM_Trip()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6fd05c4e-cfeb-d6f4-d873-325de442fc17");
            var icon = Helper.StealIcon("6fd05c4e-cfeb-d6f4-d873-325de442fc17");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Trip,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversTripAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_Disarm()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("45d94c6d-b453-cfc4-a9b9-9b72d6afe6f6");
            var icon = Helper.StealIcon("45d94c6d-b453-cfc4-a9b9-9b72d6afe6f6");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Disarm,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDisarmAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_BullRush()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("7ab6f70c-996f-e9b4-597b-8332f0a3af5f");
            var icon = Helper.StealIcon("7ab6f70c-996f-e9b4-597b-8332f0a3af5f");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.BullRush,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversBullRushAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Blind(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("8b736419-3036-a8d4-a803-08fbe16c8187");
            var icon = Helper.StealIcon("8b736419-3036-a8d4-a803-08fbe16c8187");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickBlind,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickBlindAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Entangle(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("5f22daa9-460c-5844-992b-f751e1e8eb78");
            var icon = Helper.StealIcon("5f22daa9-460c-5844-992b-f751e1e8eb78");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickEntangle,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickEntangleAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Sickened(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("4921b86e-e42c-0b54-e87a-2f9b20521ab9");
            var icon = Helper.StealIcon("4921b86e-e42c-0b54-e87a-2f9b20521ab9");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickSickened,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickSickenedAction", parent.m_DisplayName,
                parent.Description, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_Pull()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("d131394d-9d8e-f384-2984-06cfa45bb7b7");
            var icon = Helper.StealIcon("d131394d-9d8e-f384-2984-06cfa45bb7b7");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Pull,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversPullAction", "Pull",
                TelekineticManeuversPullDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        #endregion

        private static BlueprintFeatureReference CreateTeleKineticManeuversFeature(params BlueprintAbility[] variants)
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/telekineticManeuvers.png");

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversAbility", "Telekinetic Maneuvers", 
                TelekineticManeuversDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Long, null, null);

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            var feature = Helper.CreateBlueprintFeature("TelekineticManeuversFeature", "Telekinetic Maneuvers",
                TelekineticManeuversDescription, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }
        
        #endregion

        private static BlueprintFeatureReference CreateTouchsiteReactive()
        {
            var icon = Helper.CreateSprite(Main.ModPath+"/Icons/touchsite.png");

            var ignore_flatFoot = new FlatFootedIgnore
            {
                Type = FlatFootedIgnoreType.UncannyDodge
            };
            var condition = new AddCondition
            {
                Condition = UnitCondition.AttackOfOpportunityBeforeInitiative
            };

            var feature = Helper.CreateBlueprintFeature("TouchsiteReactive", "Touchsite, Reactive",
                TouchsiteReactiveDescription, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                ignore_flatFoot,
                condition
                );

            return feature.ToRef();
        }
        
        #region Self Telekinesis

        private static (BlueprintFeatureReference lesser, BlueprintFeatureReference greater) CreateSelfTelekinesis()
        {
            var lesser_feat = CreateSelfTelekinesisLesser();
            var greater_feat = CreateSelfTelekinesisGreater(lesser_feat);

            return (lesser_feat, greater_feat);
        }

        private static BlueprintFeatureReference CreateSelfTelekinesisLesser()
        {
            var icon = Helper.StealIcon("e4979934-bdb3-9d84-2b28-bee614606823"); // Buff Wings Mutagen

            var ac_bonus = new ACBonusAgainstAttacks
            {
                AgainstMeleeOnly = true,
                AgainstRangedOnly = false,
                OnlySneakAttack = false,
                NotTouch = false,
                IsTouch = false,
                OnlyAttacksOfOpportunity = false,
                Value = Helper.CreateContextValue(0),
                ArmorClassBonus = 3,
                Descriptor = ModifierDescriptor.Dodge,
                CheckArmorCategory = false,
                NotArmorCategory = null,
                NoShield = false
            };
            var no_difficultTerrain = new AddConditionImmunity
            {
                Condition = UnitCondition.DifficultTerrain
            };

            var buff = Helper.CreateBlueprintBuff("SelfTelekinesisBuff", "Self Telekinesis",
                SelfTelekinesisDescription, icon);
            buff.Flags(null, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var ability = Helper.CreateBlueprintAbility("SelfTelekinesisAbility", "Self Telekinesis",
                SelfTelekinesisDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Move, AbilityRange.Personal);
            ability.TargetSelf(CastAnimationStyle.Kineticist);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false))
                );

            var feature = Helper.CreateBlueprintFeature("SelfTelekinesisFeature", "Self Telekinesis", 
                SelfTelekinesisDescription, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateSelfTelekinesisGreater(BlueprintFeature st_lesser_feat)
        {
            var icon = Helper.StealIcon("e4979934-bdb3-9d84-2b28-bee614606823"); // Buff Wings Mutagen

            var ac_bonus = new ACBonusAgainstAttacks
            {
                AgainstMeleeOnly = true,
                AgainstRangedOnly = false,
                OnlySneakAttack = false,
                NotTouch = false,
                IsTouch = false,
                OnlyAttacksOfOpportunity = false,
                Value = Helper.CreateContextValue(0),
                ArmorClassBonus = 3,
                Descriptor = ModifierDescriptor.Dodge,
                CheckArmorCategory = false,
                NotArmorCategory = null,
                NoShield = false
            };
            var no_difficultTerrain = new AddConditionImmunity
            {
                Condition = UnitCondition.DifficultTerrain
            };

            var ability = Helper.CreateBlueprintActivatableAbility("SelfTelekinesisGreaterAbility", out var buff, "Self Telekinesis, Greater",
                SelfTelekinesisGreaterDescription, icon, UnitCommand.CommandType.Move, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, false, false, false, false, false, false, true, false, false, 1);

            buff.Flags(false, false, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var remove_lesser = new RemoveFeatureOnApply
            {
                m_Feature = st_lesser_feat.ToRef2()
            };
            var feature = Helper.CreateBlueprintFeature("SelfTelekinesisGreaterFeature", "Self Telekinesis, Greater",
                SelfTelekinesisGreaterDescription, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeature(st_lesser_feat.ToRef()),
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)), 
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                remove_lesser,
                Helper.CreateAddFacts(AnyRef.ToAny(ability))
                );

            st_lesser_feat.AddComponents(Helper.CreatePrerequisiteNoFeature(feature.ToRef()));

            return feature.ToRef();
        }
        
        #endregion

        private static BlueprintFeatureReference CreateSpellDeflection()
        {
            var icon = Helper.StealIcon("50a77710-a7c4-9144-99d0-254e76a808e5"); // Spell Resistance Buff

            var add_sr = new AddSpellResistance
            {
                AddCR = false,
                AllSpellResistancePenaltyDoNotUse = false,
                Value = new ContextValue
                {
                    ValueType = ContextValueType.Shared,
                    ValueRank = AbilityRankType.Default,
                    ValueShared = AbilitySharedValue.Damage,
                    Value = 1
                }
            };

            var classlvl_value_getter = new ClassLevelGetter()
            {
                ClassRef = Tree.Class
            };
            var property = Helper.CreateBlueprintUnitProperty("SpellDeflectionProperty")
                .SetComponents
                (
                classlvl_value_getter
                );
            property.BaseValue = 1;
            property.OperationOnComponents = MathOperation.Sum;

            var value = new ContextCalculateSharedValue 
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.CasterCustomProperty,
                        m_CustomProperty = property.ToRef()
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 9
                    }
                }
            };

            var buff = Helper.CreateBlueprintBuff("SpellDeflectionBuff", "Spell Deflection",
                SpellDeflectionDescription, icon, null);
            buff.Stacking = StackingType.Replace;
            buff.Flags(null, false, null, null);
            buff.SetComponents
                (
                add_sr, value
                );

            var variant_instant = Helper.CreateBlueprintAbility("SpellDeflectionAbilityInstant", "Spell Deflection",
                SpellDeflectionDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);
            variant_instant.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, 
                    buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false)),
                Helper.CreateAbilityAcceptBurnOnCast(0)
                );

            var duration_value = new ContextDurationValue
            {
                Rate = DurationRate.TenMinutes,
                DiceType = DiceType.One,
                DiceCountValue = Helper.CreateContextValue(0),
                BonusValue = new ContextValue
                {
                    m_CustomProperty = property.ToRef(),
                    Value = 1,
                    ValueRank = AbilityRankType.Default,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.CasterCustomProperty
                }
            };

            var variant_prolonged = Helper.CreateBlueprintAbility("SpellDeflectionAbilityProlonged", "Spell Deflection Extended",
                SpellDeflectionDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);
            variant_prolonged.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, 
                    buff.CreateContextActionApplyBuff(duration_value, false, true, false, false)),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            var ability = Helper.CreateBlueprintAbility("SpellDeflectionAbility", "Spell Deflection",
                SpellDeflectionDescription, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);

            ability.AddToAbilityVariants(variant_instant, variant_prolonged);

            var feature = Helper.CreateBlueprintFeature("SpellDeflectionFeature", "Spell Deflection",
                SpellDeflectionDescription, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.Telekinetic.BlastFeature)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 14),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature.ToRef();
        }

        private static (BlueprintFeatureReference wild_0, BlueprintFeatureReference wild_1, BlueprintFeatureReference wild_2, BlueprintFeatureReference wild_3) CreateWildTalentBonusFeatAether()
        {
            var spell_pen = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459"); // SpellPenetration
            var spell_pen_greater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec"); // GreaterSpellPenetration
            var precise_shot = Helper.ToRef<BlueprintFeatureReference>("8f3d1e6b4be006f4d896081f2f889665"); // PreciseShot
            var trip = Helper.ToRef<BlueprintFeatureReference>("0f15c6f70d8fb2b49aa6cc24239cc5fa"); // ImprovedTrip
            var trip_greater = Helper.ToRef<BlueprintFeatureReference>("4cc71ae82bdd85b40b3cfe6697bb7949"); // SpellPenetration

            var wild_0 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatAether", aether_wild_talent_name,
                aether_wild_talent_description, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_0.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.Telekinetic.BlastFeature), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusAether.Knight), true)
                );
            wild_0.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_0.m_AllFeatures, spell_pen, precise_shot, trip);

            var wild_1 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatAether1", aether_wild_talent_name,
                aether_wild_talent_description, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_1.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.Telekinetic.BlastFeature), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusAether.Knight), true)
                );
            wild_1.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_1.m_AllFeatures, spell_pen_greater, precise_shot, trip);

            var wild_2 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatAether2", aether_wild_talent_name,
                aether_wild_talent_description, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_2.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.Telekinetic.BlastFeature), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusAether.Knight), true)
                );
            wild_2.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_2.m_AllFeatures, spell_pen, precise_shot, trip_greater);

            var wild_3 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatAether3", aether_wild_talent_name,
                aether_wild_talent_description, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_3.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.Telekinetic.BlastFeature), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                new PrerequisiteSelectionPossible
                {
                    m_ThisFeature = AnyRef.ToAny(wild_3)
                },
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusAether.Knight), true)
                );
            wild_3.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_3.m_AllFeatures, spell_pen_greater, precise_shot, trip_greater);

            return (wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef());
        }

        #endregion

        #region Area Effects

        public static BlueprintAbilityAreaEffect CreateTelekineticWallEffect()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391"); // KineticistMainClass
            var kineticist_main_stat_property = "f897845bbbc008d4f9c1c4a03e22357a".ToRef<BlueprintUnitPropertyReference>(); // KineticistMainStatProperty
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2764b5504e98e6824cab3d27c"); // KineticBlastFeature
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = "4ffc8d2162a215e44a1a728752b762eb" }; // AirBlastWallEffect PrefabLink

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage));

            var context_dealDamage = Helper.CreateContextActionDealDamage(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                dice, false, false, false, true, false, AbilitySharedValue.Damage);
            ActionList action_list = new() { Actions = new GameAction[1] { context_dealDamage } };

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("WallTelekineticBlastArea", true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: action_list);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = false;

            var context1 = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, stat: StatType.Constitution, 
                type: AbilityRankType.DamageBonus, customProperty: kineticist_main_stat_property, min: 0, max: 20);
            var context2 = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, stat: StatType.Constitution,
                type: AbilityRankType.DamageDice, customProperty: kineticist_main_stat_property, min: 0, max: 20,
                feature: kinetic_blast_feature.ToRef());

            var calc_shared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
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

            var calc_ability_params = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = kineticist_class.ToRef()
            };

            area_effect.AddComponents(unique, context1, context2, calc_shared, calc_ability_params);

            return area_effect;
        }

        public static BlueprintAbilityAreaEffect CreateForceWallEffect()
        {
            var kineticist_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391"); // KineticistMainClass
            var kineticist_main_stat_property = "f897845bbbc008d4f9c1c4a03e22357a".ToRef<BlueprintUnitPropertyReference>(); // KineticistMainStatProperty
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2764b5504e98e6824cab3d27c"); // KineticBlastFeature
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = "4ffc8d2162a215e44a1a728752b762eb" }; // AirBlastWallEffect PrefabLink

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilityRankType.DamageBonus));

            var context_dealDamage = Helper.CreateContextActionDealForceDamage(dice, false, false, false, false, false, AbilitySharedValue.Damage);
            ActionList action_list = new() { Actions = new GameAction[1] { context_dealDamage } };

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("WallForceBlastArea", true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: action_list);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = true;

            var context1 = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, stat: StatType.Constitution,
                type: AbilityRankType.DamageBonus, customProperty: kineticist_main_stat_property, min: 0, max: 20);
            var context2 = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, stat: StatType.Constitution,
                type: AbilityRankType.DamageDice, min: 0, max: 20, feature: kinetic_blast_feature.ToRef());

            var calc_shared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
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

            var calc_ability_params = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = kineticist_class.ToRef()
            };

            area_effect.AddComponents(unique, context1, context2, calc_shared, calc_ability_params);

            return area_effect;
        }

        #endregion
    }
}
