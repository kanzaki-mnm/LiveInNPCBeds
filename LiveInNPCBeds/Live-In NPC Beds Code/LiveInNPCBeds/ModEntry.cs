#pragma warning disable CS8603 // Null 参照戻り値である可能性があります。
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
#pragma warning disable CS8622 // パラメーターの型における参照型の NULL 値の許容が、ターゲット デリゲートと一致しません。おそらく、NULL 値の許容の属性が原因です。

using HarmonyLib;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using StardewValley.Pathfinding;

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
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            var harmony = new Harmony(ModManifest.UniqueID);

            // ✅ config.json を読み込む！
            Config = helper.ReadConfig<ModConfig>();

            // ✅ ゲーム開始時にベッドを配置！
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "resetForNewDay"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ResetForNewDay_Postfix))
            );

            // harmony.Patch(
            //     original: AccessTools.Method(typeof(NPC), "behaviorAtGameTick"),
            //     postfix: new HarmonyMethod(typeof(ModEntry), nameof(BehaviorAtGameTick_Postfix))
            // );
        }

        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (Config.EnableInitialBeds)
            {
                ApplyInitialBeds();
            }
        }

        private void ApplyInitialBeds()
        {
            foreach (var entry in Config.InitialBeds)
            {
                GameLocation location = Game1.getLocationFromName(entry.Location);
                if (location == null)
                {
                    Monitor.Log($"Error: Location '{entry.Location}' not found.", LogLevel.Warn);
                    continue;
                }

                Vector2 bedTile = new Vector2(entry.TileX, entry.TileY);
                NPCBedFurniture bed = new NPCBedFurniture("Custom_NPCBed", bedTile);
                bed.AssignToNPC(entry.NPC);

                // ✅ ベッドを追加
                location.furniture.Add(bed);

                Monitor.Log($"Placed NPC {entry.NPC}'s bed at {entry.Location} ({entry.TileX}, {entry.TileY})", LogLevel.Info);
            }
        }

        public static void ResetForNewDay_Postfix(NPC __instance)
        {
            if (__instance == null || __instance.currentLocation == null)
            {
                return;
            }

            // ✅ NPC専用ベッドを探す
            GameLocation location = __instance.currentLocation;
            var npcBed = location.furniture.OfType<NPCBedFurniture>()
                .FirstOrDefault(bed => bed.GetAssignedNPC() == __instance.Name);

            if (npcBed != null)
            {
                // ✅ NPCをベッドの位置に移動
                __instance.setTilePosition((int)npcBed.TileLocation.X, (int)npcBed.TileLocation.Y);
                ModMonitor.Log($"Placed NPC {__instance.Name} at {location.NameOrUniqueName} ({(int)npcBed.TileLocation.X}, {(int)npcBed.TileLocation.Y})", LogLevel.Info);
            }
            else
            {
                ModMonitor.Log($"NPC {__instance.Name}'s bed is NOT found", LogLevel.Info);
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

    public class NPCBedFurniture : BedFurniture
    {
        public NPCBedFurniture(string itemId, Vector2 tile)
            : base(itemId, tile)
        {
        }

        // ✅ 明示的に Location プロパティを定義
        public override GameLocation Location
        {
            get { return base.Location; }
            set { base.Location = value; }
        }

        // ✅ プレイヤーが寝れないようにする
        // public override bool CanModifyBed(Farmer who)
        // {
        //     return true; // プレイヤーが移動・削除できない
        //     // return false; // プレイヤーが移動・削除できない
        // }

        // ✅ NPCをアサインするための `modData`
        public void AssignToNPC(string npcName)
        {
            modData["AssignedNPC"] = npcName;
        }

        public string GetAssignedNPC()
        {
            return modData.ContainsKey("AssignedNPC") ? modData["AssignedNPC"] : null;
        }

        // ✅ NPCのためにベッドを予約する
        public override void ReserveForNPC()
        {
            if (!mutex.IsLocked()) mutex.RequestLock();
        }

        // ✅ NPCがこのベッドを使っているかチェック
        public bool IsBeingUsedByNPC()
        {
            if (mutex.IsLocked()) 
            {
                return true;
            }

            if (Location == null) 
            {
                return false;
            }

            Rectangle bedBounds = GetBoundingBox();
            foreach (NPC npc in Location.characters)
            {
                if (npc.GetBoundingBox().Intersects(bedBounds))
                {
                    return true;
                }
            }
            return false;
        }

        // ✅ NPCがベッドに向かう位置を決める
        public override Point GetBedSpot()
        {
            return new Point((int)tileLocation.X, (int)tileLocation.Y);
        }
        
    }
}