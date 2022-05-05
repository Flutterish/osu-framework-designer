using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Tools;

public abstract class Tool : CompositeDrawable {
	[Resolved]
	protected Composer Composer { get; private set; } = null!;

	public Tool () {
		this.Fill();
	}

	public virtual void BeginUsing () { }
	public virtual void StopUsing () { }

	/// <summary>
	/// Whether this tool is in the middle of editing prop values.
	/// If this is true, props will not be commited to history until the batch is done.
	/// </summary>
	public virtual bool IsEditingProps => true;
}
