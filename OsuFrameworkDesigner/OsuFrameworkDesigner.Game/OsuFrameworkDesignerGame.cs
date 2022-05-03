using osu.Framework.Screens;

namespace OsuFrameworkDesigner.Game;

public class OsuFrameworkDesignerGame : OsuFrameworkDesignerGameBase {
	private ScreenStack screenStack;

	public OsuFrameworkDesignerGame () {
		Child = screenStack = new ScreenStack().Fill();
		screenStack.Push( new DesignerScreen() );
	}
}