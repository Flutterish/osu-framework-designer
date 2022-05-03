using osu.Framework.Testing;

namespace OsuFrameworkDesigner.Game.Tests.Visual;

public abstract class OsuFrameworkDesignerTestScene : TestScene {
	protected override ITestSceneTestRunner CreateRunner () => new OsuFrameworkDesignerTestSceneTestRunner();

	private class OsuFrameworkDesignerTestSceneTestRunner : OsuFrameworkDesignerGameBase, ITestSceneTestRunner {
		private TestSceneTestRunner.TestRunner runner;

		protected override void LoadAsyncComplete () {
			base.LoadAsyncComplete();
			Add( runner = new TestSceneTestRunner.TestRunner() );
		}

		public void RunTestBlocking ( TestScene test ) => runner.RunTestBlocking( test );
	}
}