using osu.Framework.Screens;
using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game;

public class DesignerScreen : Screen {
	public DesignerScreen () {
		AddInternal( new DesignerTopBar() );
	}
}