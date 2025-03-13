using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using xTile.Tiles;
using xTile.Layers;
using StardewValley.GameData.Buildings;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using System.Reflection;

namespace LiveInNPCBeds
{
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor = null!;

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;

            Harmony.DEBUG = true;
            var harmony = new Harmony(ModManifest.UniqueID);

            // NPCBarrier属性を削除
            // harmony.Patch(
            //     original: AccessTools.Method(typeof(GameLocation), "loadObjects"),
            //     postfix: new HarmonyMethod(typeof(ModEntry), nameof(RemoveNPCBarrier))
            // );
        }

    }
}