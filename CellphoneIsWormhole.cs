using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

		// Normally, this method would only check to see if a player has a wormhole potion
		// Now the method will be also return true if the player has a cellphone
		private static bool Player_HasUnityPotion(On_Player.orig_HasUnityPotion orig, Player player) {
			foreach (int item in CellphoneSystem.IsConsideredUnityPotion) {
				if (player.HasItemInInventoryOrOpenVoidBag(item))
					return true;
			}
			return orig(player);
		}

		// Normally, this method would consume 1 wormhole potion from a player's inventory to teleport
		// Now the method will prevent consuming wormhole potions if the player has cellphone to teleport with
		private static void Player_TakeUnityPotion(On_Player.orig_TakeUnityPotion orig, Player player) {
			foreach (int item in CellphoneSystem.IsConsideredUnityPotion) {
				if (player.HasItemInInventoryOrOpenVoidBag(item))
					return;
			}
			orig(player);
		}
	}

	class CellphoneSystem : ModSystem {
		public override void PostAddRecipes() {
			List<int> items = new List<int>();
			foreach (int itemType in IsConsideredUnityPotion) {
				var addItemResults = Main.recipe
				.Take(Recipe.numRecipes)
				.Where(r => r.HasIngredient(itemType) || r.AcceptedByItemGroups(itemType, ItemID.ShellphoneDummy));

				foreach (Recipe recipe in addItemResults) {
					if (!items.Contains(recipe.createItem.type))
						items.Add(recipe.createItem.type);
				}
			}

			foreach (int type in items) {
				if (!IsConsideredUnityPotion.Contains(type))
					IsConsideredUnityPotion.Add(type);
			}
		}

		internal readonly static List<int> IsConsideredUnityPotion = new List<int> {
			ItemID.CellPhone,
			ItemID.Shellphone,
			ItemID.ShellphoneDummy,
			ItemID.ShellphoneHell,
			ItemID.ShellphoneOcean,
			ItemID.ShellphoneSpawn
		};
	}

	class CellphoneTweak : GlobalItem
	{
		// Add the wormhole potion's tooltips to the cellphone as they now function identically
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (CellphoneSystem.IsConsideredUnityPotion.Contains(item.type)) {
				TooltipLine[] modToolTip = new TooltipLine[] {
					new TooltipLine(Mod, "WormholePotion_Tooltip1", Lang.GetTooltip(ItemID.WormholePotion).GetLine(0)),
					new TooltipLine(Mod, "WormholePotion_Tooltip2", Lang.GetTooltip(ItemID.WormholePotion).GetLine(1)),
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
