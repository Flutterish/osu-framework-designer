namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasFillColour {
	Prop<Colour4> FillColour { get; }

	public static IHasFillColour? From ( IComponent component ) {
		if ( component is IHasFillColour iface )
			return iface;

		if ( component.GetProperty<Colour4>( "Colour", "Fill" ) is Prop<Colour4> c ) {
			return new Impl {
				FillColour = c
			};
		}

		return null;
	}

	private struct Impl : IHasFillColour {
		public Prop<Colour4> FillColour { get; set; }
	}
}
