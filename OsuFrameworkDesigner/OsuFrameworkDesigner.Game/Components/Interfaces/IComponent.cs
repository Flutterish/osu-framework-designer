using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.TypeExtensions;
using OsuFrameworkDesigner.Game.Components.Blueprints;
using OsuFrameworkDesigner.Game.Containers;

namespace OsuFrameworkDesigner.Game.Components.Interfaces;

public interface IComponent {
	string Name { get; set; }
	IEnumerable<IProp> Properties { get; }

	Blueprint<IComponent> CreateBlueprint ();

	public IProp<T>? GetProperty<T> ( string name, string category )
		=> Properties.FirstOrDefault( x => x.Prototype.Name == name && x.Prototype.Category == category ) as IProp<T>;

	public IProp<T>? GetProperty<T> ( PropDescription description )
		=> Properties.FirstOrDefault( x => x.Prototype == description ) as IProp<T>;

	public IEnumerable<IProp> GetNestedProperties ()
		=> this is IEnumerable<IComponent> container
			? Properties.Concat( container.SelectMany( c => c.GetNestedProperties() ) )
			: Properties;

	public IEnumerable<IComponent> GetNestedComponents ()
		=> this is IEnumerable<IComponent> container
			? this.Yield().Concat( container.SelectMany( c => c.GetNestedComponents() ) )
			: this.Yield();

	public string NameOrDefault () {
		var name = Name;
		if ( string.IsNullOrEmpty( name ) )
			name = GetType().ReadableName();
		else
			return name;

		if ( name.StartsWith( "Component" ) )
			name = name["Component".Length..];
		if ( name.EndsWith( "Component" ) )
			name = name[..^"Component".Length];

		return name;
	}

	public string NameOrDefault ( Composer composer ) {
		var name = Name;
		if ( string.IsNullOrEmpty( name ) )
			name = GetType().ReadableName();
		else
			return name;

		if ( name.StartsWith( "Component" ) )
			name = name["Component".Length..];
		if ( name.EndsWith( "Component" ) )
			name = name[..^"Component".Length];

		int i = 2;
		var testName = name;
		while ( composer.Components.Any( x => x.Name == testName ) )
			testName = $"{name} {i++}";

		return testName;
	}
}
