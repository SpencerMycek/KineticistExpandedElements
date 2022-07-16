using System;
using UnityModManagerNet;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using BlueprintCore.Utils;
using KineticistElementsExpanded.ElementAether;
using KineticistElementsExpanded.ElementVoid;
using Void = KineticistElementsExpanded.ElementVoid.Void;
using Kingmaker;
using System.Linq;


namespace KineticistElementsExpanded
{
    public class Main
    {

        public static Harmony harmony;
        public static bool IsInGame => Game.Instance.Player?.Party?.Any() ?? false; // RootUIContext.Instance?.IsInGame ?? false; //

        /// <summary>True if mod is enabled. Doesn't do anything right now.</summary>
        public static bool Enabled { get; set; } = true;
        /// <summary>Path of current mod.</summary>
        public static string ModPath;

        internal static UnityModManager.ModEntry.ModLogger logger;

        //        #region GUI

        /// <summary>Called when the mod is turned to on/off.
        /// With this function you control an operation of the mod and inform users whether it is enabled or not.</summary>
        /// <param name="value">true = mod to be turned on; false = mod to be turned off</param>
        /// <returns>Returns true, if state can be changed.</returns>
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.Enabled = value;
            return true;
        }

        /// <summary>Loads on game start.</summary>
        /// <param name="modEntry.Info">Contains all fields from the 'Info.json' file.</param>
        /// <param name="modEntry.Path">The path to the mod folder e.g. '\Steam\steamapps\common\YourGame\Mods\TestMod\'.</param>
        /// <param name="modEntry.Active">Active or inactive.</param>
        /// <param name="modEntry.Logger">Writes logs to the 'Log.txt' file.</param>
        /// <param name="modEntry.OnToggle">The presence of this function will let the mod manager know that the mod can be safely disabled during the game.</param>
        /// <param name="modEntry.OnGUI">Called to draw UI.</param>
        /// <param name="modEntry.OnSaveGUI">Called while saving.</param>
        /// <param name="modEntry.OnUpdate">Called by MonoBehaviour.Update.</param>
        /// <param name="modEntry.OnLateUpdate">Called by MonoBehaviour.LateUpdate.</param>
        /// <param name="modEntry.OnFixedUpdate">Called by MonoBehaviour.FixedUpdate.</param>
        /// <returns>Returns true, if no error occurred.</returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModPath = modEntry.Path;
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            //modEntry.OnGUI = OnGUI;
            //modEntry.OnSaveGUI = OnSaveGUI;
            //modEntry.OnHideGUI = OnHideGUI;
            //modEntry.OnUnload = Unload;

            try
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll();
                //Helper
            } catch (Exception ex)
            {
                Helper.PrintException(ex);
                return false;
            }
            return true;
        }

        public static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmony?.UnpatchAll(modEntry.Info.Id);
            return true;
        }


    }

    [HarmonyPatch(typeof(StartGameLoader), "LoadAllJson")]
    static class StartGameLoader_LoadAllJson
    {
        private static bool Run = false;

        static void Postfix()
        {
            if (Run) return; Run = true;

            Helper.Print("Loading Kineticist Elements Expanded");

            LoadSafe(Aether.Configure);
            LoadSafe(Void.Configure);

            Helper.Print("Finished loading Kineticist Elements Expanded");

        }


        public static bool LoadSafe(Action action)
        {
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            try
            {
                Helper.Print($"Loading {name}");
                action();

                return true;
            }
            catch (Exception ex)
            {
                Helper.PrintException(ex);
                return false;
            }
        }
    }
}
