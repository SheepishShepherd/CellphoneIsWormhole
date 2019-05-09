using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

// Teleport to beds/npcs?
// Currently doesn't support: Dyes, Accessories, Modded Hats/Hairs?

namespace CellphoneIsWormhole
{
    class CellphoneIsWormhole : Mod
    {
        internal static CellphoneIsWormhole instance;
        internal static bool unityMouseOver = false;

        public CellphoneIsWormhole()
        {

        }

        public override void Load()
        {
            instance = this;
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            Player player = Main.player[Main.myPlayer];
            if (!player.HasItem(ItemID.CellPhone))
            {
                unityMouseOver = false;
                return;
            }

            bool foundTarget = false;
            string text = "";

            float mapWorldScale = Main.mapFullscreenScale / 16f;
            float offsetX = Main.screenWidth / 2 - Main.mapFullscreenPos.X * Main.mapFullscreenScale;
            float offsetY = Main.screenHeight / 2 - Main.mapFullscreenPos.Y * Main.mapFullscreenScale;

            if (Main.netMode == NetmodeID.MultiplayerClient && player.team > 0 && !player.hostile && player.HasItem(ItemID.WormholePotion))
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    if (i == Main.myPlayer || NotValidPlayer(Main.player[i]))
                    {
                        continue;
                    }

                    Player drawPlayer = Main.player[i];

                    float playerHeadCenterX = offsetX + mapWorldScale * (drawPlayer.position.X + drawPlayer.width / 2);
                    float playerHeadCenterY = offsetY + mapWorldScale * (drawPlayer.position.Y + drawPlayer.gfxOffY + drawPlayer.height / 2);
                    playerHeadCenterX -= 2f;
                    playerHeadCenterY -= 2f - Main.mapFullscreenScale / 5f * 2f;

                    float minX = playerHeadCenterX - 14f * Main.UIScale;
                    float minY = playerHeadCenterY - 14f * Main.UIScale;
                    float maxX = minX + 28f * Main.UIScale;
                    float maxY = minY + 28f * Main.UIScale;

                    if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY)
                    {
                        SpriteEffects effect = SpriteEffects.None;
                        if (drawPlayer.direction == -1) effect = SpriteEffects.FlipHorizontally;

                        // Head with skin color
                        Main.spriteBatch.Draw(
                            Main.playerTextures[Main.LocalPlayer.skinVariant, 0],
                            new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                            new Rectangle(0, 0, 40, 30),
                            ModifyColor(drawPlayer.skinColor),
                            0f, Vector2.Zero, 1.5f, effect, 0f);

                        // White part of eyes
                        Main.spriteBatch.Draw(
                            Main.playerTextures[0, 1],
                            new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                            new Rectangle(0, 0, 40, 30),
                            ModifyColor(Color.White),
                            0f, Vector2.Zero, 1.5f, effect, 0f);

                        // Eye color
                        Main.spriteBatch.Draw(
                            Main.playerTextures[0, 2],
                            new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                            new Rectangle(0, 0, 40, 30),
                            ModifyColor(drawPlayer.eyeColor),
                            0f, Vector2.Zero, 1.5f, effect, 0f);

                        // Draws Hair if no hats/armor equipped of if the hair draws behind the vanity/armor
                        if (drawPlayer.armor[0].type == 0 && drawPlayer.armor[10].type == 0)
                        {
                            Main.spriteBatch.Draw(
                                Main.playerHairTexture[drawPlayer.hair],
                                new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                                new Rectangle(0, 0, 40, 56),
                                ModifyColor(drawPlayer.GetHairColor(false)),
                                0f, Vector2.Zero, 1.5f, effect, 0f);
                        }
                        else if (GetDrawPosition(drawPlayer.head) == -1)
                        {
                            if (drawPlayer.GetModPlayer<CellphonePlayers>().hairDrawing)
                            {
                                Main.spriteBatch.Draw(
                                   Main.playerHairTexture[drawPlayer.hair],
                                   new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                                   new Rectangle(0, 0, 40, 56),
                                   ModifyColor(drawPlayer.GetHairColor(false)),
                                   0f, Vector2.Zero, 1.5f, effect, 0f);
                            }
                            else if (drawPlayer.GetModPlayer<CellphonePlayers>().hatDrawing)
                            {
                                Main.spriteBatch.Draw(
                                   Main.playerHairAltTexture[drawPlayer.hair],
                                   new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                                   new Rectangle(0, 0, 40, 56),
                                   ModifyColor(drawPlayer.GetHairColor(false)),
                                   0f, Vector2.Zero, 1.5f, effect, 0f);
                            }
                        }
                        
                        // Draws vanity/armor if any
                        if (drawPlayer.armor[10].type > 0)
                        {
                            Main.spriteBatch.Draw(
                               Main.armorHeadTexture[drawPlayer.armor[10].headSlot],
                               new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                               new Rectangle(0, 0, 40, 56),
                               ModifyColor(Color.White),
                               0f, Vector2.Zero, 1.5f, effect, 0f);
                        }
                        else if (drawPlayer.armor[0].type > 0)
                        {
                            Main.spriteBatch.Draw(
                               Main.armorHeadTexture[drawPlayer.armor[0].headSlot],
                               new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                               new Rectangle(0, 0, 40, 56),
                               ModifyColor(Color.White),
                               0f, Vector2.Zero, 1.5f, effect, 0f);
                        }

                        // Draws Hair after certain vanity/armor
                        if (GetDrawPosition(drawPlayer.head) == 1)
                        {
                            if (drawPlayer.GetModPlayer<CellphonePlayers>().hairDrawing)
                            {
                                Main.spriteBatch.Draw(
                                   Main.playerHairTexture[drawPlayer.hair],
                                   new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                                   new Rectangle(0, 0, 40, 56),
                                   ModifyColor(drawPlayer.GetHairColor(false)),
                                   0f, Vector2.Zero, 1.5f, effect, 0f);
                            }
                            else if (drawPlayer.GetModPlayer<CellphonePlayers>().hatDrawing)
                            {
                                Main.spriteBatch.Draw(
                                   Main.playerHairAltTexture[drawPlayer.hair],
                                   new Vector2(playerHeadCenterX - 30, playerHeadCenterY - 25),
                                   new Rectangle(0, 0, 40, 56),
                                   ModifyColor(drawPlayer.GetHairColor(false)),
                                   0f, Vector2.Zero, 1.5f, effect, 0f);
                            }
                        }

                        if (!unityMouseOver) Main.PlaySound(SoundID.MenuTick);

                        foundTarget = true;
                        unityMouseOver = true;

                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            Main.mouseLeftRelease = false;
                            Main.mapFullscreen = false;
                            player.UnityTeleport(drawPlayer.position);
                        }
                        else if (text == "") text = Language.GetTextValue("Game.TeleportTo", drawPlayer.name);
                        break;
                    }
                }
            }

            if (!foundTarget && unityMouseOver) unityMouseOver = false;
            if (text != "") mouseText = text;
        }

        public int GetDrawPosition(int head)
        {
            int[] InFront = new int[]
            {
                10, 12, 28, 62, 97, 106, 113, 116, 119, 133, 138, 139, 163, 178, 181, 191, 198
            };

            int[] Behind = new int[]
            {
                14, 15, 16, 18, 21, 24, 25, 26, 40, 44, 51, 56, 59, 60, 67, 68, 69, 114, 121, 126, 130, 136, 140, 145, 158, 159, 161, 184, 190, 195
            };

            for (int i = 0; i < InFront.Length; i++)
            {
                if (InFront[i] == head) return 1;
            }

            for (int i = 0; i < Behind.Length; i++)
            {
                if (Behind[i] == head) return -1;
            }

            return 0;
        }

        public bool NotValidPlayer(Player player)
        {
            if (!player.active || player.dead || Main.player[Main.myPlayer].team != player.team || player.hostile) return true;
            return false;
        }

        public Color ModifyColor(Color oldColor)
        {
            Color result = oldColor;
            float modifier = 1f;
            result.R = (byte)((float)(int)result.R * modifier);
            result.B = (byte)((float)(int)result.B * modifier);
            result.G = (byte)((float)(int)result.G * modifier);
            return result;
        }
    }

    class CellphoneTweak : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == ItemID.CellPhone)
            {
                TooltipLine line = new TooltipLine(mod, "CIW_PlayerTeleport", "Teleports you to a party member\nClick their head on the fullscreen map");
                int index = tooltips.FindIndex(tt => tt.mod == "Terraria" && (tt.Name == "Expert" || tt.Name.EndsWith("Price")));
                if (index != -1) { tooltips.Insert(index, line); }
                else { tooltips.Add(line); }
            }
        }

        public override void DrawHair(int head, ref bool drawHair, ref bool drawAltHair)
        {
            if (!Main.gameMenu)
            {
                Main.LocalPlayer.GetModPlayer<CellphonePlayers>().hairDrawing = drawHair;
                Main.LocalPlayer.GetModPlayer<CellphonePlayers>().hatDrawing = drawAltHair;
            }
        }

        public override bool ConsumeItem(Item item, Player player)
        {
            // To prevent the Wormhole potions from being used while having a cellphone
            if (Main.LocalPlayer.HasItem(ItemID.CellPhone) && item.type == ItemID.WormholePotion) return false; 
            return base.ConsumeItem(item, player);
        }
    }

    class CellphonePlayers : ModPlayer
    {
        public bool hairDrawing;
        public bool hatDrawing;

        public override void ResetEffects()
        {
            hairDrawing = false;
            hatDrawing = false;
        }
    }
}