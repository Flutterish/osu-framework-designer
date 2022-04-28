using OsuFrameworkDesigner.Game.Components.Blueprints;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IComponent {
	string Name { get; }
	IEnumerable<IProp> Properties { get; }

	Blueprint<IComponent> CreateBlueprint ();

	public Prop<T>? GetProperty<T> ( string name, string category )
		=> Properties.FirstOrDefault( x => x.Prototype.Name == name && x.Prototype.Category == category ) as Prop<T>;

	public Prop<T>? GetProperty<T> ( PropDescription description )
		=> Properties.FirstOrDefault( x => x.Prototype == description ) as Prop<T>;

	public IEnumerable<IProp> GetNestedProperties ()
		=> this is IEnumerable<IComponent> container
			? Properties.Concat( container.SelectMany( c => c.GetNestedProperties() ) )
			: Properties;
}
