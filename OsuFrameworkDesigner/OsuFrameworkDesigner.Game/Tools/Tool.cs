using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class Tool : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public Tool () {
		this.Fill();
	}
}
