using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CellphoneIsWormhole
{
    class CellphoneIsWormhole : Mod
    {
        public CellphoneIsWormhole() {

		}
	}

	class CellphoneSystem : ModSystem
	{
		public bool CheckWormholeConditions(Player player)
		{
			return player.team > 0 && !player.hostile;
		}

		public bool IsPlayerInvalid(Player plr)
		{
			CWConfigs config = ModContent.GetInstance<CWConfigs>();
			Player myPlayer = Main.LocalPlayer;
			if (config.requiresCellphone && !plr.HasItem(ItemID.CellPhone)) {
				return true;
            }
			return plr.whoAmI == myPlayer.whoAmI || plr.dead || myPlayer.team != plr.team || plr.hostile;
		}

		public static float GetFullMapScale() => Main.mapFullscreenScale / Main.UIScale;

		// Plays a sound when hovering over a valid player to teleport to
		internal static bool HoverSoundPlayed = false;
		public override void PostDrawFullscreenMap(ref string mouseText)
		{
			// Define the player; if they do not have a cellphone, do not run any of this code
			Player localPlayer = Main.LocalPlayer;
			if (!localPlayer.active || !localPlayer.HasItem(ItemID.CellPhone)) {
				HoverSoundPlayed = false;
				return;
			}

			// Necessary for the wormhole potion's visuals and functionailty
			bool foundTarget = false;
			string text = "";

			// We must be on a team, not in pvp, and must have a cellphone to teleport
			if (CheckWormholeConditions(localPlayer)) {
				foreach (Player player in Main.player) {
					// If the chosen player is self, nonexistent, or an invalid player, continue to the next player
					if (!player.active || IsPlayerInvalid(player)) {
						continue;
					}

					// The following code is directly from source... so why is it offset???

					float mapPosScaledX = Main.mapFullscreenPos.X * GetFullMapScale();
					float mapPosScaledY = Main.mapFullscreenPos.Y * GetFullMapScale();

					float playerMapPoxOffsetX = (0f - mapPosScaledX + (float)(Main.screenWidth / 2)) + (10f * GetFullMapScale());
					float playerMapPosOffsetY = (0f - mapPosScaledY + (float)(Main.screenHeight / 2)) + (10f * GetFullMapScale());

					float playerMapPosX = ((player.position.X + (float)(player.width / 2)) / 16f) * GetFullMapScale();
					float playerMapPosY = ((player.position.Y + player.gfxOffY + (float)(player.height / 2)) / 16f) * GetFullMapScale();

					playerMapPosX += playerMapPoxOffsetX;
					playerMapPosY += playerMapPosOffsetY;
					playerMapPosX -= 6f;
					playerMapPosY -= 2f;
					playerMapPosY -= 2f - GetFullMapScale() / 5f * 2f;
					playerMapPosX -= 10f * GetFullMapScale();
					playerMapPosY -= 10f * GetFullMapScale();

					float minX = playerMapPosX + 4f - 14f * Main.UIScale;
					float minY = playerMapPosY + 2f - 14f * Main.UIScale;
					float maxX = minX + 28f * Main.UIScale;
					float maxY = minY + 28f * Main.UIScale;

					// If we are in a "head hitbox" of a valid player, draw their head.
					if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY) {
						Vector2 headPos = new Vector2(playerMapPosX, playerMapPosY);
						Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, headPos, 1f, Main.UIScale + 0.5f, Main.OurFavoriteColor);

						// If a sound hasn't player after hovering over a valid player, play a sound
						if (!HoverSoundPlayed) {
							SoundEngine.PlaySound(SoundID.MenuTick);
						}

						// A valid target is being hovered over and the hover sound check has occured, set both to true 
						foundTarget = true;
						HoverSoundPlayed = true;
						// If a player clicks while hovering over a valid target, close the fullscreen map and perform a wormhole teleport
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							Main.mouseLeftRelease = false;
							Main.mapFullscreen = false;
							localPlayer.UnityTeleport(player.position);
						}
						else if (text == "") {
							// If no click has occured, set the text for drawing later outside of the player loop
							text = Language.GetTextValue("Game.TeleportTo", player.name);
						}
						break;
					}
				}
			}

			// foundTarget is reset to false every tick, but if hovering over a player it becomes true
			// When not hovering a player, the check never becomes true which resets the HoverSoundPlayed bool back to false.
			// This allows the hover sound to play again when hovering over a new valid target
			if (!foundTarget && HoverSoundPlayed) {
				HoverSoundPlayed = false;
			}

			// If text is not empty, make mouseText have that text when hovering over a player
			if (text != "") {
				mouseText = text;
			}
		}
	}

    class CellphoneTweak : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
			// Add the wormhole potion's tooltips to the cellphone as they now function identically
            if (item.type == ItemID.CellPhone) {
                TooltipLine line = new TooltipLine(Mod, "CiW_WormholeTooltip1", Lang.GetTooltip(ItemID.WormholePotion).GetLine(0).ToString());
				TooltipLine line2 = new TooltipLine(Mod, "CiW_WormholeTooltip2", Lang.GetTooltip(ItemID.WormholePotion).GetLine(1).ToString());
				int index = tooltips.FindIndex(tt => tt.mod == "Terraria" && (tt.Name == "Expert" || tt.Name.EndsWith("Price")));
				if (index != -1) {
					tooltips.Insert(index, line);
					tooltips.Insert(index + 1, line2);
				}
				else {
					tooltips.Add(line);
					tooltips.Add(line2);
				}
            }
        }

		public override bool ConsumeItem(Item item, Player player)
        {
			// Prevent wormhole potions from being consumed while holding a cellphone
			if (Main.LocalPlayer.HasItem(ItemID.CellPhone) && item.type == ItemID.WormholePotion) {
				return false;
			}
            return base.ConsumeItem(item, player);
        }
    }
}
