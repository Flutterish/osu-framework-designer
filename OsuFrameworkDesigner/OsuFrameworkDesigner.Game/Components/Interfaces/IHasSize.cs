namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IHasSize {
	Prop<float> Width { get; }
	Prop<float> Height { get; }

	public static IHasSize? From ( IComponent component ) {
		if ( component is IHasSize iface )
			return iface;

		if ( component.GetProperty<float>( PropDescriptions.Width ) is Prop<float> w && component.GetProperty<float>( PropDescriptions.Height ) is Prop<float> h ) {
			return new Impl {
				Width = w,
				Height = h
			};
		}

		return null;
	}

	private struct Impl : IHasSize {
		public Prop<float> Width { get; set; }
		public Prop<float> Height { get; set; }
	}
}
