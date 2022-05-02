namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasPosition {
	Prop<float> X { get; }
	Prop<float> Y { get; }

	public static IHasPosition? From ( IComponent component ) {
		if ( component is IHasPosition iface )
			return iface;

		if ( component.GetProperty<float>( PropDescriptions.X ) is Prop<float> x && component.GetProperty<float>( PropDescriptions.Y ) is Prop<float> y ) {
			return new Impl {
				X = x,
				Y = y
			};
		}

		return null;
	}

	private struct Impl : IHasPosition {
		public Prop<float> X { get; set; }
		public Prop<float> Y { get; set; }
	}
}
