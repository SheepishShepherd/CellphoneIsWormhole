using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CellphoneIsWormhole
{
	[Label("Configs")]
	public class CiW_Configs : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		
		[Header("[i:3625] [c/ffeb6e:Debugging]")]

		[DefaultValue(false)]
		[Label("Draw Arrows")]
		[Tooltip("If issues occur while hovering over other players. Try enabling this config.\nReplaces the playerhead drawing with an arrow.")]
		public bool drawArrow;

		[BackgroundColor(189, 183, 107)]
		[DefaultValue(false)]
		[Label("Enable Developer Mode")]
		public bool inDebugMode;
	}
}
