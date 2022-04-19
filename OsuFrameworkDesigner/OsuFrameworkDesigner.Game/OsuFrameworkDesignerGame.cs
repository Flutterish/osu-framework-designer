using osu.Framework.Screens;

namespace OsuFrameworkDesigner.Game;

public class OsuFrameworkDesignerGame : OsuFrameworkDesignerGameBase {
	private ScreenStack screenStack;

	public OsuFrameworkDesignerGame () {
		Child = screenStack = new ScreenStack { RelativeSizeAxes = Axes.Both };
		screenStack.Push( new DesignerScreen() );
	}
}