using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BedFix
{
    internal class BedSystem : ModSystem
    {
        internal static Configuration Config;

        public override void Load() {
            Terraria.IL_Main.DoUpdateInWorld += Main_DoUpdateInWorld;
            Terraria.GameContent.On_PlayerEyeHelper.SetStateByPlayerInfo += PlayerEyeHelper_SetStateByPlayerInfo; ;
            Terraria.GameContent.On_PlayerSittingHelper.SitDown += PlayerSittingHelper_SitDown;
            Terraria.GameContent.On_PlayerSittingHelper.SitUp += PlayerSittingHelper_SitUp;
            Terraria.GameContent.On_PlayerSleepingHelper.DoesPlayerHaveReasonToActUpInBed += PlayerSleepingHelper_DoesPlayerHaveReasonToActUpInBed;
            Terraria.GameContent.On_PlayerSleepingHelper.UpdateState += PlayerSleepingHelper_UpdateState;
            Terraria.On_Player.CheckSpawn += Player_CheckSpawn;
            Terraria.DataStructures.On_AnchoredEntitiesCollection.GetNextPlayerStackIndexInCoords += AnchoredEntitiesCollection_GetNextPlayerStackIndexInCoords;
        }

        //IL_0061: ldsfld class Terraria.Player[] Terraria.Main::player
        //IL_0066: ldloc.3
        //IL_0067: ldelem.ref
        //IL_0068: ldflda valuetype Terraria.GameContent.PlayerSleepingHelper Terraria.Player::sleeping
        //IL_006D: call instance bool Terraria.GameContent.PlayerSleepingHelper::get_FullyFallenAsleep() (cursor)
        //IL_0072: brfalse.s IL_0078
        private void Main_DoUpdateInWorld(ILContext il) {
            try {
                var c = new ILCursor(il);

                c.GotoNext(
                    MoveType.After,
                    i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
                    i => i.Match(OpCodes.Ldloc_3),
                    i => i.Match(OpCodes.Ldelem_Ref),
                    i => i.MatchLdflda(typeof(Player), nameof(Player.sleeping)),
                    i => i.MatchCall(typeof(PlayerSleepingHelper), "get_FullyFallenAsleep")
                );

                c.Emit(OpCodes.Ldloc_3); // emit i
                c.EmitDelegate<Func<bool, int, bool>>((fallAsleep, i) => {
                    var player = Main.player[i];
                    if (Config.SleepOnChair && !fallAsleep && player.sitting.isSitting && player.sleeping.timeSleeping >= 120)
                        return true; // fall asleep on chair
                    return fallAsleep; // returns normal
                });

            }
            catch {
                throw new Exception("Error happened with BedFix mod Main_DoUpdateInWorld IL editing: Hook location not found, if (player[i].sleeping.FullyFallenAsleep)");
            }
        }

        private void PlayerEyeHelper_SetStateByPlayerInfo(Terraria.GameContent.On_PlayerEyeHelper.orig_SetStateByPlayerInfo orig, ref PlayerEyeHelper self, Player player) {
            if (Config.SleepOnChair && !player.sleeping.isSleeping && player.sitting.isSitting) {
                player.sleeping.isSleeping = true;
                orig.Invoke(ref self, player);
                player.sleeping.isSleeping = false;
                return;
            }
            orig.Invoke(ref self, player);
        }

        private void PlayerSittingHelper_SitDown(Terraria.GameContent.On_PlayerSittingHelper.orig_SitDown orig, ref PlayerSittingHelper self, Player player, int x, int y) {
            if (Config.SleepOnChair) {
                player.sleeping.timeSleeping = 0;
            }
            orig.Invoke(ref self, player, x, y);
        }

        private void PlayerSittingHelper_SitUp(Terraria.GameContent.On_PlayerSittingHelper.orig_SitUp orig, ref PlayerSittingHelper self, Player player, bool multiplayerBroadcast) {
            if (Config.SleepOnChair && self.isSitting) {
                player.sleeping.timeSleeping = 0;
            }
            orig.Invoke(ref self, player, multiplayerBroadcast);
        }

        // it seems that reflecetion can't set the value of EyeFrameToShow
        //private void PlayerEyeHelper_UpdateEyeFrameToShow(On.Terraria.GameContent.PlayerEyeHelper.orig_UpdateEyeFrameToShow orig, ref PlayerEyeHelper self, Player player) {
        //    orig.Invoke(ref self, player);

        //    // use reflection to set eye frame when sitting on a chair.
        //    var targetMethod = player.eyeHelper.GetType().GetMethod("DoesPlayerCountAsModeratelyDamaged", BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { typeof(Player) });
        //    var moderatelyDamaged = (bool)targetMethod.Invoke(player.eyeHelper, new object[] { player });

        //    EyeFrame eyeFrame = moderatelyDamaged ? EyeFrame.EyeHalfClosed : EyeFrame.EyeOpen;
        //    int _timeInState = player.sleeping.timeSleeping;

        //    var targetValue = player.eyeHelper.GetType().GetProperty(nameof(player.eyeHelper.EyeFrameToShow), BindingFlags.Instance | BindingFlags.Public);
        //    if (targetValue is not null)
        //        targetValue.SetValue(player.eyeHelper, (int)((_timeInState >= 60) ? ((_timeInState < 120) ? EyeFrame.EyeHalfClosed : EyeFrame.EyeClosed) : eyeFrame));
        //}

        private bool PlayerSleepingHelper_DoesPlayerHaveReasonToActUpInBed(Terraria.GameContent.On_PlayerSleepingHelper.orig_DoesPlayerHaveReasonToActUpInBed orig, ref PlayerSleepingHelper self, Player player) {
            return Config.FallAsleepAlways ? false : orig.Invoke(ref self, player); // never open the player's eyes.
        }

        private void PlayerSleepingHelper_UpdateState(Terraria.GameContent.On_PlayerSleepingHelper.orig_UpdateState orig, ref PlayerSleepingHelper self, Player player) {
            if (Config.FallAsleepImmediately && self.timeSleeping < 120) {
                self.timeSleeping = 120; // falling asleep immediately.
            }

            // enabled sleeping on a chair and is currently on a chair.
            if (Config.SleepOnChair && !self.isSleeping && player.sitting.isSitting) {
                self.timeSleeping++;
                return; // don't run vanilla code.
            }

            if (Config.UsingItemStopsSleeping) {
                int i = player.itemAnimation;
                player.itemAnimation = 0; // so the game thinks the player never used any item.
                orig.Invoke(ref self, player);
                player.itemAnimation = i;
                return;
            }

            orig.Invoke(ref self, player);
        }

        private int AnchoredEntitiesCollection_GetNextPlayerStackIndexInCoords(Terraria.DataStructures.On_AnchoredEntitiesCollection.orig_GetNextPlayerStackIndexInCoords orig, AnchoredEntitiesCollection self, Point coords) {
            return Config.SleepTogether ? 1 : orig.Invoke(self, coords); // so the game thinks there is only one player.
        }

        private bool Player_CheckSpawn(Terraria.On_Player.orig_CheckSpawn orig, int x, int y) {
            return Config.ValidHouseRequirement ? orig.Invoke(x, y) : true;
        }
    }
}
