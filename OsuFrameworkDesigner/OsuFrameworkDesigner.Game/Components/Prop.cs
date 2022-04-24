using System.Runtime.CompilerServices;

namespace OsuFrameworkDesigner.Game.Components;

public interface IProp : IBindable {
	/// <summary>
	/// The name of the property. Properties with the same name and type, in the same category are merged
	/// </summary>
	string Name { get; }
	/// <summary>
	/// The category of the properrty. Properties are grouped by category
	/// </summary>
	string Category { get; }
	/// <summary>
	/// Whether this property should be grouped by same value (<see langword="true"/>) or be just one field (<see langword="false"/>)
	/// </summary>
	bool Groupable { get; }
	Type Type { get; }
	object? Value { get; }
}

public class Prop<T> : Bindable<T>, IProp {
	public Prop ( [CallerMemberName] string name = "" ) { Name = name; }
	public Prop ( T @default, [CallerMemberName] string name = "" ) : base( @default ) { Name = name; }

	/// <summary>
	/// The name of this property. It is automatically set to the caller member name.
	/// <code>
	/// public readonly Prop&lt;T&gt; MyValue = new(); // this will assign the name "MyValue"
	/// </code>
	/// </summary>
	public string Name {
		get => Description;
		init => Description = value;
	}
	public string Category { get; init; } = "Ungrouped";
	public bool Groupable { get; init; } = false;
	public Type Type => typeof( T );
	object? IProp.Value => Value;

	public static implicit operator T ( Prop<T> prop )
		=> prop.Value;
}
