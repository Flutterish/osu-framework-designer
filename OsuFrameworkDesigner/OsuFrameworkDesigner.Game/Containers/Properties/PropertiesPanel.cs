using osu.Framework.Caching;
using osu.Framework.Extensions.TypeExtensions;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Containers.Assets;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class PropertiesPanel : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SidePanelDefault );
	FillFlowContainer items;

	public PropertiesPanel () {
		RelativeSizeAxes = Axes.Y;
		Width = 300;
		AddInternal( background = new Box().Fill() );
		AddInternal( new BasicScrollContainer {
			Child = items = new FillFlowContainer {
				Direction = FillDirection.Full,
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Padding = new( 10 )
			}
		}.Fill() );

		Components.BindCollectionChanged( ( _, _ ) => componentCache.Invalidate() );
	}

	public readonly BindableList<IComponent> Components = new();

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		backgroundColor.BindTo( colours.SidePanel );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}

	Cached componentCache = new();
	protected override void Update () {
		base.Update();

		if ( !componentCache.IsValid ) {
			componentCache.Validate();

			foreach ( var (field, free, pool) in rentedFields ) {
				free( field );
				pool.Push( field );

				items.Remove( field );
			}
			rentedFields.Clear();

			items.Clear();
			foreach ( var category in Components.SelectMany( c => c.GetNestedProperties() ).GroupBy( x => x.Category ) ) {
				items.Add( new DesignerSpriteText { Text = category.Key, Font = DesignerFont.Bold( 18 ), Colour = Colour4.Black, Alpha = 0.5f, RelativeSizeAxes = Axes.X } );

				foreach ( var prop in category.GroupBy( x => (x.Name, x.Type) ) ) {
					var ungroupable = prop.Where( x => !x.Groupable );
					if ( ungroupable.Any() ) items.Add( createEditField( prop.Key.Type, ungroupable ) );

					foreach ( var i in prop.Where( x => x.Groupable ).GroupBy( x => x.Value ) ) {
						items.Add( createEditField( prop.Key.Type, i ) );
					}
				}
			}
		}
	}

	static (Func<Drawable> create, Action<Drawable, IEnumerable<IProp>, string> apply, Action<Drawable> free) createField<TField, T>
		( Action<TField, IEnumerable<IProp<T>>, string> apply, Action<TField> free ) where TField : Drawable, new() {
		return (
			() => new TField(),
			( f, p, t ) => apply( (TField)f, p.OfType<IProp<T>>(), t ),
			f => free( (TField)f )
		);
	}

	List<(Drawable field, Action<Drawable> free, Stack<Drawable> pool)> rentedFields = new();
	Dictionary<Type, Stack<Drawable>> editFieldPool = new();
	static readonly Dictionary<Type, (Func<Drawable> create, Action<Drawable, IEnumerable<IProp>, string> apply, Action<Drawable> free)> editFieldFactory = new() {
		[typeof( float )] = createField<FloatEditField, float>(
			( f, p, t ) => { f.Title = t; f.Apply( p ); },
			f => f.Free()
		),
		[typeof( int )] = createField<IntEditField, int>(
			( f, p, t ) => { f.Title = t; f.Apply( p ); },
			f => f.Free()
		),
		[typeof( Colour4 )] = createField<ColourEditField, Colour4>(
			( f, p, t ) => f.Apply( p ),
			f => f.Free()
		),
	};
	Drawable createEditField ( Type type, IEnumerable<IProp> props ) {
		if ( !editFieldPool.TryGetValue( type, out var stack ) ) editFieldPool.Add( type, stack = new() );

		if ( !editFieldFactory.TryGetValue( type, out var factory ) ) throw new InvalidOperationException( $"No edit field for type {type.ReadableName()} exists." );

		if ( !stack.TryPop( out var field ) ) field = factory.create();

		factory.apply( field, props, props.First().Name );
		rentedFields.Add( (field, factory.free, stack) );
		return field;
	}
}