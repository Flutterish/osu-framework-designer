namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasScale {
	Prop<float> ScaleX { get; }
	Prop<float> ScaleY { get; }

	public static IHasScale? From ( IComponent component ) {
		if ( component is IHasScale iface )
			return iface;

		if ( component.GetProperty<float>( PropDescriptions.ScaleX ) is Prop<float> x && component.GetProperty<float>( PropDescriptions.ScaleY ) is Prop<float> y ) {
			return new Impl {
				ScaleX = x,
				ScaleY = y
			};
		}

		return null;
	}

	private struct Impl : IHasScale {
		public Prop<float> ScaleX { get; set; }
		public Prop<float> ScaleY { get; set; }
	}
}
