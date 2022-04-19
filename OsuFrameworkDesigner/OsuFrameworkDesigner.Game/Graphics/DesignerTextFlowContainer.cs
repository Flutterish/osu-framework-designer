using osu.Framework.Graphics.Sprites;

namespace OsuFrameworkDesigner.Game.Graphics;

public class DesignerTextFlowContainer : TextFlowContainer {
	protected override SpriteText CreateSpriteText ()
		=> new DesignerSpriteText();
}
