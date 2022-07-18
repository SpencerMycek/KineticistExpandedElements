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
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace KineticistElementsExpanded.ElementWood
{
    class Wood : Statics
    {

        // Fixed a mixup between Aether and Void Class skills
        
        public static KineticistTree Tree = new();

        public static KineticistTree.Element Positive = new();
        public static KineticistTree.Element WoodBlast = new();
        public static KineticistTree.Element Verdant = new();
        public static KineticistTree.Element Autumn = new();
        public static KineticistTree.Element Spring = new();
        public static KineticistTree.Element Summer = new();
        public static KineticistTree.Element Winter = new();

        public static KineticistTree.Focus WoodFocus = new();

        public static KineticistTree.Infusion PhotoKinetic = new();
        public static KineticistTree.Infusion Spore = new();
        public static KineticistTree.Infusion Toxic = new();
        public static KineticistTree.Infusion GreaterToxic = new();

        public static void Configure()
        {
            // Void for reference

            BlueprintFeatureBase wood_class_skills = CreateWoodClassSkills();

            // Form Infusions
            // Blasts
            // Substance Infusions

            // Elemental Defense

            BlueprintFeature flesh_of_wood_feature = null;

            // Misc Blast Functions

            // Elemental Focuses
            CreateWoodElementalFocus(wood_class_skills, flesh_of_wood_feature);
            CreateKineticKnightWoodFocus(wood_class_skills, flesh_of_wood_feature);
            CreateSecondElementWood();
            CreateThirdElementWood();

            // Wild Talents
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateWoodClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("WoodClassSkills", "Wood Class Skills",
                WoodClassSkillsDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddClassSkill(StatType.SkillLoreNature)
                );
            return feature;
        }

        private static void CreateWoodBlastsSelection()
        {

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

            // Can be any void basic: Gravity or Negative
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
        // TODO
        //  Natural Armor bonus
        #endregion

        #region Positive Blast
        #endregion

        #region Wood Blast
        #endregion

        #region Composite Blasts
        // TODO
        //  Positive Admixture
        //  Verdant
        //  Autumn
        //  Spring
        //  Summer
        //  Winter
        #endregion

        #region Infusions
        // TODO
        //  Dazzling
        //  Photokinetic <-- New
        //  pushing
        //  entangling
        //  foxfire
        //  impale <-- Dark Codex
        //  toxic <-- New? Codex?
        //  spore <-- New?
        //  deadly earth
        //  toxic greater New+?
        #endregion

        #region Wild Talents
        // TODO
        //  Bonus Wild Talent Feats
        //  Roots
        //  Wood Healer
        //  Woodland Step
        //  Merciful foliage
        //  Brachiation
        //  thorn flesh
        //  warp wood
        //  greensight
        //  healing burst
        //  herbal antivenom
        //  plant disguise
        //  shape wood
        //  plant puppet
        //  wild growth
        //  woodland step (greater)
        //  kinetic revivification
        //  green tongue
        //  ^^ greater
        //  tree step
        //  wood soldiers
        //  forest siege
        #endregion

        #region Area Effects
        // TODO
        //  Wood
        //  Positive
        //  Verdant
        //  Autumn
        //  Spring
        //  Summer
        //  Winter
        #endregion
    }
}
