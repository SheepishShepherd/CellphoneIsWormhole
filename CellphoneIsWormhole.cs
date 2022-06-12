using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace CellphoneIsWormhole
{
	class CellphoneIsWormhole : Mod
	{
		public CellphoneIsWormhole() {

		}

		public override void Load() {
			On.Terraria.Player.HasUnityPotion += Player_HasUnityPotion;
			On.Terraria.Player.TakeUnityPotion += Player_TakeUnityPotion;
		}

		// Normally, this method would only check to see if a player has a wormhole potion
		// Now the method will be also return true if the player has a cell phone
		private static bool Player_HasUnityPotion(On.Terraria.Player.orig_HasUnityPotion orig, Player player) {
			for (int i = 0; i < Main.InventorySlotsTotal; i++) {
				if (player.inventory[i].type == ItemID.CellPhone && player.inventory[i].stack > 0) {
					return true;
				}
			}
			return orig(player);
		}

		// Normally, this method would consume 1 wormhole potion from a player's inventory to teleport
		// Now the method will prevent consuming wormhole potions if the player has cellphone to teleport with
		private static void Player_TakeUnityPotion(On.Terraria.Player.orig_TakeUnityPotion orig, Player player) {
			for (int i = 0; i < Main.InventorySlotsTotal; i++) {
				if (player.inventory[i].type == ItemID.CellPhone && player.inventory[i].stack > 0) {
					return;
				}
			}
			orig(player);
		}
	}

	class CellphoneTweak : GlobalItem
	{
		// Add the wormhole potion's tooltips to the cellphone as they now function identically
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (item.type == ItemID.CellPhone) {
				ItemTooltip WormholeTooltips = Lang.GetTooltip(ItemID.WormholePotion);
				TooltipLine[] modToolTip = new TooltipLine[] {
					new TooltipLine(Mod, "CiW_1", WormholeTooltips.GetLine(0) + " also holding a cell phone"),
					new TooltipLine(Mod, "CiW_2", WormholeTooltips.GetLine(1)),
				};

				// Find the spots for the last tooltips. Before the price tooltip but after everything else.
				// Expert/Master tooltips don't need to be checked since the item is not expert/master exclusive
				int index = tooltips.FindIndex(tt => tt.Mod == "Terraria" && tt.Name.EndsWith("Price"));
				for (int i = 0; i < modToolTip.Length; i++) {
					if (index != -1)
						tooltips.Insert(index + i, modToolTip[i]);
					else
						tooltips.Add(modToolTip[i]);
				}
			}
		}
	}
}
