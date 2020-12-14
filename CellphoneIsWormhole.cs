using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

// TODO: Currently doesn't support: Dyes (fairly certain it supports modded hairs and vanity, but havent tested)

namespace CellphoneIsWormhole
{
    class CellphoneIsWormhole : Mod
    {
        public CellphoneIsWormhole() {

		}

		public bool CheckWormholeConditions(Player player)
		{
			if (ModContent.GetInstance<CiW_Configs>().inDebugMode) {
				// If in Debug Mode, only a Cell Phone is required
				return player.active && player.HasItem(ItemID.CellPhone);
			}
			else {
				// Otherwise, we must also be on a team and have PVP disabled
				return player.active && player.team > 0 && !player.hostile && player.HasItem(ItemID.CellPhone);
			}
		}

		public bool IsPlayerInvalid(int plr)
		{
			if (ModContent.GetInstance<CiW_Configs>().inDebugMode) {
				// If in Debug Mode, any player works (for drawing purposes)
				return false;
			}
			else {
				//Invalid players include: Inactive players, Dead players, Players not on the same team, or Players that are hostile (in PVP)
				Player myPlayer = Main.player[Main.myPlayer];
				Player player = Main.player[plr];
				return player.whoAmI == myPlayer.whoAmI || !player.active || player.dead || myPlayer.team != player.team || player.hostile;
			}
		}

		// Plays a sound when hovering over a valid player to teleport to
		internal static bool HoverSoundPlayed = false;

		public override void PostDrawFullscreenMap(ref string mouseText)
		{
			// Define the player; if they do not have a cellphone, do not run any of this code
			Player player = Main.LocalPlayer;
			if (!player.active || !player.HasItem(ItemID.CellPhone)) {
				HoverSoundPlayed = false;
				return;
			}

			// Necessary for the wormhole potion's visuals and functionailty
			bool foundTarget = false;
			string text = "";

			// Setup Map to screen sizes
			float mapWorldScale = Main.mapFullscreenScale / 16f;
			float offsetX = Main.screenWidth / 2 - Main.mapFullscreenPos.X * Main.mapFullscreenScale;
			float offsetY = Main.screenHeight / 2 - Main.mapFullscreenPos.Y * Main.mapFullscreenScale;
			
			// We must be on a team, not in pvp, and must have a cellphone to teleport
			if (CheckWormholeConditions(player)) {
				for (int i = 0; i < Main.maxPlayers; i++) {
					// If the chosen player is self, nonexistent, or an invalid player, continue to the next player
					if (IsPlayerInvalid(i)) {
						continue;
					}

					// A player that can be drawn on the map
					Player drawnPlayer = Main.player[i];

					// Get the map position of the selected player
					float playerHeadCenterX = offsetX + mapWorldScale * (drawnPlayer.position.X + drawnPlayer.width / 2);
					float playerHeadCenterY = offsetY + mapWorldScale * (drawnPlayer.position.Y + drawnPlayer.gfxOffY + drawnPlayer.height / 2);
					playerHeadCenterX -= 2f;
					playerHeadCenterY -= 2f - Main.mapFullscreenScale / 5f * 2f;

					// Calculate the bounds of their "head hitbox"
					float minX = playerHeadCenterX - 14f * Main.UIScale;
					float minY = playerHeadCenterY - 14f * Main.UIScale;
					float maxX = minX + 28f * Main.UIScale;
					float maxY = minY + 28f * Main.UIScale;

					// If we are in a "head hitbox" of a valid player, draw their head.
					if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY) {
						if (ModContent.GetInstance<CiW_Configs>().drawArrow) {
							Texture2D arrow = ModContent.GetTexture("Terraria/UI/VK_Shift");
							Main.spriteBatch.Draw(
								arrow,
								new Vector2(playerHeadCenterX - (arrow.Bounds.Width / 2) - 5, playerHeadCenterY - (arrow.Bounds.Height / 2) - 30),
								arrow.Bounds,
								Main.DiscoColor,
								0f, Vector2.Zero, 1.5f, SpriteEffects.FlipVertically, 0f);
						}
						else {
							// Flip the drawing if the player is facing the other direction
							SpriteEffects effect = drawnPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
							// Offset for drawing
							playerHeadCenterX -= 30;
							playerHeadCenterY -= 26;

							// Draw the player's head based on their playerDrawData
							// Cycling through the drawdata, we check if the texture of the layer and any of the players textures match, then draw
							// Doing this one layer at a time with the for loop means we will always get the correct order no matter how big the layer sizes are
							// Order of these conditions do not matter, as the will get checked in order by the drawlayers in the for loop
							// The spritebatch draws the appropriate texture as well as scaling it up to cover the original map head

							//TODO: BUG changing cosmetics while looking at map breaks the layers orders.
							//TODO: TEST do players with similar cosmetics break order?
							// Main.playerDrawData draws ALL players? So if they wear similar cosmetics will it draw ones persons clothing over anothers?

							for (int j = 0; j < Main.playerDrawData.Count; j++) {
								Texture2D texture = Main.playerDrawData[j].texture;

								// Face
								if (texture == Main.playerTextures[drawnPlayer.skinVariant, 0]) {
									Main.spriteBatch.Draw(
										Main.playerTextures[drawnPlayer.skinVariant, 0],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 30),
										drawnPlayer.skinColor,
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Eye Sclera
								else if (texture == Main.playerTextures[0, 1]) {
									Main.spriteBatch.Draw(
										Main.playerTextures[0, 1],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 30),
										Color.White,
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Eye Color
								else if (texture == Main.playerTextures[0, 2]) {
									Main.spriteBatch.Draw(
										Main.playerTextures[0, 2],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 30),
										drawnPlayer.eyeColor,
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Hair
								else if (texture == Main.playerHairTexture[drawnPlayer.hair]) {
									Main.spriteBatch.Draw(
										Main.playerHairTexture[drawnPlayer.hair],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 56),
										drawnPlayer.GetHairColor(false),
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Alt Hair (due to certain hats)
								else if (texture == Main.playerHairAltTexture[drawnPlayer.hair]) {
									Main.spriteBatch.Draw(
										Main.playerHairAltTexture[drawnPlayer.hair],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 56),
										drawnPlayer.GetHairColor(false),
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Helmet
								else if (!drawnPlayer.armor[10].IsAir && texture == Main.armorHeadTexture[drawnPlayer.armor[10].headSlot]) {
									Main.spriteBatch.Draw(
										Main.armorHeadTexture[drawnPlayer.armor[10].headSlot],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 56),
										Color.White,
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Vanity Helmet
								else if (!drawnPlayer.armor[0].IsAir && texture == Main.armorHeadTexture[drawnPlayer.armor[0].headSlot]) {
									Main.spriteBatch.Draw(
										Main.armorHeadTexture[drawnPlayer.armor[0].headSlot],
										new Vector2(playerHeadCenterX, playerHeadCenterY),
										new Rectangle(0, 0, 40, 56),
										Color.White,
										0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
								}
								// Accessories that take up the face slot
								else {
									for (int k = 3; k < drawnPlayer.armor.Length; k++) {
										if (!drawnPlayer.armor[k].IsAir && drawnPlayer.armor[k].faceSlot > -1 && texture == Main.accFaceTexture[drawnPlayer.armor[k].faceSlot]) {
											Main.spriteBatch.Draw(
												Main.accFaceTexture[drawnPlayer.armor[k].faceSlot],
												new Vector2(playerHeadCenterX, playerHeadCenterY),
												new Rectangle(0, 0, 40, 56),
												Color.White,
												0f, Vector2.Zero, Main.UIScale + 0.5f, effect, 0f);
										}
									}
								}
							}
						}

						// If a sound hasn't player after hovering over a valid player, play a sound
						if (!HoverSoundPlayed) {
							Main.PlaySound(SoundID.MenuTick);
						}

						// A valid target is being hovered over and the hover sound check has occured, set both to true 
						foundTarget = true;
						HoverSoundPlayed = true;

						// If a player clicks while hovering over a valid target, close the fullscreen map and perform a wormhole teleport
						if (Main.mouseLeft && Main.mouseLeftRelease) {
							Main.mouseLeftRelease = false;
							Main.mapFullscreen = false;
							player.UnityTeleport(drawnPlayer.position);
						}
						else if (text == "") {
							// If no click has occured, set the text for drawing later outside of the player loop
							text = Language.GetTextValue("Game.TeleportTo", drawnPlayer.name);
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
                TooltipLine line = new TooltipLine(mod, "CiW_WormholeTooltip1", Lang.GetTooltip(ItemID.WormholePotion).GetLine(0).ToString());
				TooltipLine line2 = new TooltipLine(mod, "CiW_WormholeTooltip2", Lang.GetTooltip(ItemID.WormholePotion).GetLine(1).ToString());
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