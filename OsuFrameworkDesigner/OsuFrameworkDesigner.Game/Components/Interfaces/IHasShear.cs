namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasShear {
	Prop<float> ShearX { get; }
	Prop<float> ShearY { get; }

	public static IHasShear? From ( IComponent component ) {
		if ( component is IHasShear iface )
			return iface;

		if ( component.GetProperty<float>( PropDescriptions.ShearX ) is Prop<float> x && component.GetProperty<float>( PropDescriptions.ShearY ) is Prop<float> y ) {
			return new Impl {
				ShearX = x,
				ShearY = y
			};
		}

		return null;
	}

	private struct Impl : IHasShear {
		public Prop<float> ShearX { get; set; }
		public Prop<float> ShearY { get; set; }
	}
}
