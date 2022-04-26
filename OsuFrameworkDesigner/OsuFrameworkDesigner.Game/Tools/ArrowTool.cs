using OsuFrameworkDesigner.Game.Components;

namespace OsuFrameworkDesigner.Game.Tools;

public class ArrowTool : LineTool {
	protected override LineComponent CreateShape ()
		=> new ArrowComponent();
}
