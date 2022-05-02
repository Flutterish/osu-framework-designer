﻿using OsuFrameworkDesigner.Game.Containers.Properties;
using System.Runtime.CompilerServices;

namespace OsuFrameworkDesigner.Game.Components;

public interface IProp {
	PropDescription Prototype { get; }

	object? Value { get; }
}

public interface IProp<T> : IProp, IBindable<T> {
	new T Value { get; set; }
}

public class Prop<T> : Bindable<T>, IProp<T> {
	public Prop ( PropDescription proto ) { Prototype = proto; }
	public Prop ( T @default, PropDescription proto ) : base( @default ) { Prototype = proto; }
	public PropDescription Prototype { get; }

	public override T Value {
		get => base.Value;
		set => base.Value = Prototype.Normalize( value, base.Value );
	}

	object? IProp.Value => Value;

	public static implicit operator T ( Prop<T> prop )
		=> prop.Value;
}

public class ClampedProp<T> : BindableNumber<T>, IProp<T> where T : struct, IComparable<T>, IConvertible, IEquatable<T> {
	public ClampedProp ( PropDescription proto ) { Prototype = proto; }
	public ClampedProp ( T @default, PropDescription proto ) : base( @default ) { Prototype = proto; }
	public PropDescription Prototype { get; }

	public override T Value {
		get => base.Value;
		set => base.Value = Prototype.Normalize( value, base.Value );
	}

	object? IProp.Value => Value;

	public static implicit operator T ( ClampedProp<T> prop )
		=> prop.Value;
}

public sealed record PropDescription {
	public PropDescription ( [CallerMemberName] string name = "" ) {
		Name = name;
	}

	/// <summary>
	/// The name of the property.
	/// </summary>
	public string Name { get; init; }
	/// <summary>
	/// The category of the property. Properties are grouped by category
	/// </summary>
	public string Category { get; init; } = "Ungrouped";
	/// <summary>
	/// Whether this property should be grouped by same value (<see langword="true"/>) or be just one field (<see langword="false"/>)
	/// </summary>
	public bool Groupable { get; init; }

	public Func<PropDescription, Drawable> CreateEditField { get; init; } = null!;
	public Action<Drawable, IEnumerable<IProp>> ApplyEditField { get; init; } = null!;
	public Action<Drawable> FreeEditField { get; init; } = null!;

	Func<object?, object?> normalizeValue = x => x;
	Func<object?, object?, object?> normalizeValueWihDefault = ( x, d ) => x;
	public PropDescription WithNormalizeFunction<T> ( Func<T, T> func, Func<T, T, T> funcWithDefault ) => this with {
		normalizeValue = obj => func( (T)obj! ),
		normalizeValueWihDefault = ( obj, def ) => funcWithDefault( (T)obj!, (T)def! )
	};
	public T Normalize<T> ( T value )
		=> (T)normalizeValue( value )!;

	public T Normalize<T> ( T value, T @default )
		=> (T)normalizeValueWihDefault( value, Normalize( @default ) )!;

	public string UnqualifiedName => Name.StartsWith( Category ) ? Name[Category.Length..] : Name;
}

public static class PropDescriptions {
	public static readonly PropDescription UnboundFloatProp = new() {
		CreateEditField = self => new FloatEditField { Title = self.UnqualifiedName },
		ApplyEditField = ( f, props ) => ( (FloatEditField)f ).Apply( props.OfType<IProp<float>>() ),
		FreeEditField = f => ( (FloatEditField)f ).Free()
	};
	public static readonly PropDescription FloatProp = UnboundFloatProp.WithNormalizeFunction<float>(
		f => float.IsNaN( f ) || float.IsInfinity( f ) ? 0 : f,
		( f, def ) => float.IsNaN( f ) || float.IsInfinity( f ) ? def : f
	);
	public static readonly PropDescription IntProp = new() {
		CreateEditField = self => new IntEditField { Title = self.UnqualifiedName },
		ApplyEditField = ( f, props ) => ( (IntEditField)f ).Apply( props.OfType<IProp<int>>() ),
		FreeEditField = f => ( (IntEditField)f ).Free()
	};
	public static readonly PropDescription ColourProp = new() {
		CreateEditField = self => new ColourEditField(),
		ApplyEditField = ( f, props ) => ( (ColourEditField)f ).Apply( props.OfType<IProp<Colour4>>() ),
		FreeEditField = f => ( (ColourEditField)f ).Free()
	};

	public static readonly PropDescription X = FloatProp with { Name = "X", Category = "Basic" };
	public static readonly PropDescription Y = FloatProp with { Name = "Y", Category = "Basic" };
	public static readonly PropDescription Width = FloatProp with { Name = "Width", Category = "Basic" };
	public static readonly PropDescription Height = FloatProp with { Name = "Height", Category = "Basic" };
	public static readonly PropDescription Rotation = FloatProp with { Name = "Rotation", Category = "Basic" };
	public static readonly PropDescription ScaleX = FloatProp with { Name = "X", Category = "Scale" };
	public static readonly PropDescription ScaleY = FloatProp with { Name = "Y", Category = "Scale" };
	public static readonly PropDescription ShearX = FloatProp with { Name = "X", Category = "Shear" };
	public static readonly PropDescription ShearY = FloatProp with { Name = "Y", Category = "Shear" };
	public static readonly PropDescription OriginX = FloatProp with { Name = "X", Category = "Origin" };
	public static readonly PropDescription OriginY = FloatProp with { Name = "Y", Category = "Origin" };
	public static readonly PropDescription FillColour = ColourProp with { Name = "Colour", Category = "Fill", Groupable = true };
	public static readonly PropDescription CornerRadius = FloatProp with { Name = "Radius", Category = "Corners" };
}