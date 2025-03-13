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

// TODO:
// NPC用ベッドの家具定義
// ロビンの店で買えるようにする
// NPCの専用ベッドを設定できる
// NPCが自分のベッドに向かうスケジュールを設定
// NPC用ベッドの経路探索の調整
// FarmHouseに初期ベッドを配置する機能

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