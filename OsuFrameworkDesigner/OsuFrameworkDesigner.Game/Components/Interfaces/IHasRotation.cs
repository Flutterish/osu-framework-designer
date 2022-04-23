namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasRotation {
	Prop<float> Rotation { get; }

	public static IHasRotation? From ( IComponent component ) {
		if ( component is IHasRotation iface )
			return iface;

		if ( component.GetProperty<float>( "Rotation", "Basic" ) is Prop<float> rot ) {
			return new Impl {
				Rotation = rot
			};
		}

		return null;
	}

	private struct Impl : IHasRotation {
		public Prop<float> Rotation { get; set; }
	}
}
