using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CellphoneIsWormhole
{
	class CellphoneIsWormhole : Mod
	{
		public CellphoneIsWormhole()
		{

		}

		public override void Load()
		{
			On.Terraria.Player.HasUnityPotion += Player_HasUnityPotion;
			On.Terraria.Player.TakeUnityPotion += Player_TakeUnityPotion;
		}

		private static bool Player_HasUnityPotion(On.Terraria.Player.orig_HasUnityPotion orig, Player player)
		{
			for (int i = 0; i < Main.InventorySlotsTotal; i++) {
				if (player.inventory[i].type == ItemID.CellPhone && player.inventory[i].stack > 0) {
					return true;
				}
			}
			return orig(player);
		}

		private static void Player_TakeUnityPotion(On.Terraria.Player.orig_TakeUnityPotion orig, Player player)
		{
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
