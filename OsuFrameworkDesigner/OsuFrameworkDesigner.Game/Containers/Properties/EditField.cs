using osu.Framework.Caching;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Memory;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public abstract class EditField<T> : CompositeDrawable {
	protected RentedArray<IProp<T>> Props { get; private set; }

	protected virtual void OnFree () { }
	public void Free () {
		foreach ( var i in Props ) {
			i.ValueChanged -= onValueChanged;
		}
		Props.Dispose();
		Props = default;

		OnFree();
	}

	protected virtual void OnApply () { }
	public void Apply ( IEnumerable<IProp<T>> props ) {
		Props = MemoryPool<IProp<T>>.Shared.Rent( props );
		foreach ( var i in Props ) {
			i.ValueChanged += onValueChanged;
		}
		InvalidateDisplay();

		OnApply();
	}

	private void onValueChanged ( ValueChangedEvent<T> e ) {
		InvalidateDisplay();
	}

	protected void SetValue ( T value ) {
		foreach ( var i in Props ) {
			i.Value = value;
		}
	}

	Cached displayCache = new();
	protected void InvalidateDisplay () {
		displayCache.Invalidate();
	}

	protected abstract void UpdateDisplay ();

	protected override void Update () {
		base.Update();

		if ( !displayCache.IsValid ) {
			UpdateDisplay();

			displayCache.Validate();
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		Free();
	}
}
