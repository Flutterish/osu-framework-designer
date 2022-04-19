using System.Runtime.CompilerServices;

namespace OsuFrameworkDesigner.Game.Components;

public interface IProp : IBindable {
	string Name { get; }
	string Category { get; }
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
	public string Category { get; init; }
	public bool Groupable { get; init; } = false;
	public Type Type => typeof( T );
	object? IProp.Value => Value;
}
