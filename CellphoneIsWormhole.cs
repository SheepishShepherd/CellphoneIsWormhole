using System.Collections.Generic;
using System.Linq;
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
			On_Player.HasUnityPotion += Player_HasUnityPotion;
			On_Player.TakeUnityPotion += Player_TakeUnityPotion;
		}

		public static int[] IsConsideredUnityPotion = new int[] {
			ItemID.CellPhone,
			ItemID.Shellphone,
			ItemID.ShellphoneDummy,
			ItemID.ShellphoneHell,
			ItemID.ShellphoneOcean,
			ItemID.ShellphoneSpawn,
		};

		// Normally, this method would only check to see if a player has a wormhole potion
		// Now the method will be also return true if the player has a cell phone
		private static bool Player_HasUnityPotion(On_Player.orig_HasUnityPotion orig, Player player) {
			foreach (int item in IsConsideredUnityPotion) {
				if (player.HasItemInInventoryOrOpenVoidBag(item))
					return true;
			}
			return orig(player);
		}

		// Normally, this method would consume 1 wormhole potion from a player's inventory to teleport
		// Now the method will prevent consuming wormhole potions if the player has cellphone to teleport with
		private static void Player_TakeUnityPotion(On_Player.orig_TakeUnityPotion orig, Player player) {
			foreach (int item in IsConsideredUnityPotion) {
				if (player.HasItemInInventoryOrOpenVoidBag(item))
					return;
			}
			orig(player);
		}
	}

	class CellphoneTweak : GlobalItem
	{
		// Add the wormhole potion's tooltips to the cellphone as they now function identically
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (CellphoneIsWormhole.IsConsideredUnityPotion.Contains(item.type)) {
				ItemTooltip WormholeTooltips = Lang.GetTooltip(ItemID.WormholePotion);
				TooltipLine[] modToolTip = new TooltipLine[] {
					new TooltipLine(Mod, "CiW_1", WormholeTooltips.GetLine(0)),
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
