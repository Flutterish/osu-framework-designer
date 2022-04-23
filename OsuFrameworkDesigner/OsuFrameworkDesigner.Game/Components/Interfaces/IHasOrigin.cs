namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasOrigin {
	Prop<float> OriginX { get; }
	Prop<float> OriginY { get; }

	public static IHasOrigin? From ( IComponent component ) {
		if ( component is IHasOrigin iface )
			return iface;

		if ( component.GetProperty<float>( "X", "Origin" ) is Prop<float> x && component.GetProperty<float>( "Y", "Origin" ) is Prop<float> y ) {
			return new Impl {
				OriginX = x,
				OriginY = y
			};
		}

		return null;
	}

	private struct Impl : IHasOrigin {
		public Prop<float> OriginX { get; set; }
		public Prop<float> OriginY { get; set; }
	}
}
