#pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
#pragma warning disable CS8622 // パラメーターの型における参照型の NULL 値の許容が、ターゲット デリゲートと一致しません。おそらく、NULL 値の許容の属性が原因です。

using HarmonyLib;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley;

// TODO:
// NPCが自分のベッドに向かうスケジュールを設定
// NPC用ベッドの経路探索の調整

// テクスチャの話
// テクスチャなんですが、自分で用意したassetsフォルダしか参照できないのかな…？他のModで主人公用のベッドのテクスチャを差し替えたりしてるので、そっちのテクスチャをつかったりできないかな…大変か
// あわよくば、主人公用のベッドと同じようにAlternative Texuturesでテクスチャを変更できるようにしたいところだけど。。

namespace LiveInNPCBeds
{
    public class ModEntry : Mod
    {

        public static IMonitor ModMonitor = null!;
        private static ModConfig Config;
        public static ModEntry Instance;

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Instance = this;
            var harmony = new Harmony(ModManifest.UniqueID);

            // ✅ config.json を読み込む！
            Config = helper.ReadConfig<ModConfig>();

            // ✅ loadObjects() の後にベッドを配置
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "loadObjects"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(LoadObjects_Prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "resetForNewDay"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ResetForNewDay_Postfix))
            );
        }

        public static bool LoadObjects_Prefix(GameLocation __instance)
        {
            if (!Context.IsMainPlayer) 
            {
                return true; // マルチプレイではホストのみが処理
            }

            Instance.ApplyInitialBeds(__instance);
            return true;
        }

        public void ApplyInitialBeds(GameLocation __instance)
        {
            foreach (var entry in Config.InitialBeds)
            {
                if (__instance == null || entry.Location != __instance.NameOrUniqueName)
                {
                    continue;
                }

                if (entry.Location != __instance.NameOrUniqueName)
                {
                    return;
                }

                Vector2 bedTile = new Vector2(entry.TileX, entry.TileY);
                BedFurniture bed = new BedFurniture("Custom_NPCBed", bedTile);

                // ✅ NPCをアサイン
                AssignBedToNPC(bed, entry.NPC);

                __instance.furniture.Add(bed);
                ModMonitor.Log($"[DEBUG] Placed NPC {entry.NPC}'s bed at {entry.Location} ({entry.TileX}, {entry.TileY})", LogLevel.Debug);
            }
        }


        public static void ResetForNewDay_Postfix(NPC __instance, int dayOfMonth)
        {
            if (__instance.Name != "Seiris")
            {
                return;
            }

            if (__instance == null || __instance.currentLocation == null)
            {
                return;
            }

            // ✅ NPC専用ベッドを探す
            GameLocation location = __instance.currentLocation;

            // ✅ `modData` に `AssignedNPC` を持つ `BedFurniture` を検索
            var npcBed = location.furniture
                .OfType<BedFurniture>()
                .FirstOrDefault(bed => GetAssignedNPC(bed) == __instance.Name);

            if (npcBed != null)
            {
                // ✅ NPCをベッドの位置に移動
                __instance.setTilePosition((int)npcBed.TileLocation.X, (int)npcBed.TileLocation.Y + 1);
                ModMonitor.Log($"[DEBUG] Placed NPC {__instance.Name} at {location.NameOrUniqueName} ({(int)npcBed.TileLocation.X}, {(int)npcBed.TileLocation.Y})", LogLevel.Debug);
            }
        }

        // public static void BehaviorAtGameTick_Postfix(NPC __instance)
        // {
        //     if (__instance == null || __instance.currentLocation == null)
        //     {
        //         return;
        //     }

        //     // ✅ 夜10時以降ならベッドに帰る
        //     if (Game1.timeOfDay >= 2200)
        //     {
        //         GameLocation location = __instance.currentLocation;
        //         var npcBed = location.furniture.OfType<NPCBedFurniture>()
        //             .FirstOrDefault(bed => bed.GetAssignedNPC() == __instance.Name);

        //         if (npcBed != null)
        //         {
        //             // ✅ NPCがベッドへ移動
        //             __instance.controller = new PathFindController(
        //                 __instance, location, 
        //                 new Point((int)npcBed.TileLocation.X, (int)npcBed.TileLocation.Y), 
        //                 -1, 
        //                 () => __instance.Halt()
        //             );
        //         }
        //     }
        // }

        public static void AssignBedToNPC(BedFurniture bed, string npcName)
        {
            if (bed == null || string.IsNullOrEmpty(npcName))
            {
                return;
            }
            // ✅ `modData` に `AssignedNPC` を設定
            bed.modData["AssignedNPC"] = npcName;
        }

        public static string GetAssignedNPC(BedFurniture bed)
        {
            if (bed == null || !bed.modData.ContainsKey("AssignedNPC"))
            {
                return null;
            }
            return bed.modData["AssignedNPC"];
        }


    }

    public class ModConfig
    {
        public bool EnableInitialBeds { get; set; } = false;
        public List<InitialBedEntry> InitialBeds { get; set; } = new List<InitialBedEntry>();
    }

    public class InitialBedEntry
    {
        public string Location { get; set; } = "FarmHouse";
        public int TileX { get; set; } = 0;
        public int TileY { get; set; } = 0;
        public string NPC { get; set; } = "";
    }
    
}