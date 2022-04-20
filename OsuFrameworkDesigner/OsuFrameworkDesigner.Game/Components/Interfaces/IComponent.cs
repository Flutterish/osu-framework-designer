using OsuFrameworkDesigner.Game.Components.Blueprints;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IComponent {
	string Name { get; }
	IEnumerable<IProp> Properties { get; }

	Blueprint<IComponent> CreateBlueprint ();

	public Prop<T>? GetProperty<T> ( string name, string category )
		=> Properties.FirstOrDefault( x => x.Name == name && x.Category == category && x.Type == typeof( T ) ) as Prop<T>;
}
