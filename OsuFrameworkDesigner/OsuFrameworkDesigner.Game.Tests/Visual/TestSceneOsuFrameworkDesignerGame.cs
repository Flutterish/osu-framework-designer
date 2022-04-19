using osu.Framework.Allocation;
using osu.Framework.Platform;

namespace OsuFrameworkDesigner.Game.Tests.Visual;

public class TestSceneOsuFrameworkDesignerGame : OsuFrameworkDesignerTestScene {
	// Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
	// You can make changes to classes associated with the tests and they will recompile and update immediately.

	private OsuFrameworkDesignerGame game;

	[BackgroundDependencyLoader]
	private void load (GameHost host) {
		game = new OsuFrameworkDesignerGame();
		game.SetHost( host );

		AddGame( game );
	}
}