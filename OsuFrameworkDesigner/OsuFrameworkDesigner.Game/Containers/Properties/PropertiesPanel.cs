using osu.Framework.Caching;
using osu.Framework.Extensions.TypeExtensions;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Containers.Properties;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers;

public class PropertiesPanel : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( ColourConfiguration.SidePanelDefault );
	FillFlowContainer items;

	public PropertiesPanel () {
		RelativeSizeAxes = Axes.Y;
		Width = 300;
		AddInternal( background = new Box().Fill() );
		AddInternal( items = new FillFlowContainer {
			Direction = FillDirection.Full,
			AutoSizeAxes = Axes.Y,
			RelativeSizeAxes = Axes.X,
			Padding = new( 10 )
		} );

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

			items.Clear();
			foreach ( var category in Components.SelectMany( c => c.GetNestedProperties() ).GroupBy( x => x.Category ) ) {
				items.Add( new DesignerSpriteText { Text = category.Key, Font = DesignerFont.Bold( 18 ), Colour = Colour4.Black, Alpha = 0.5f, RelativeSizeAxes = Axes.X } );

				foreach ( var prop in category.GroupBy( x => (x.Name, x.Type) ) ) {
					var ungroupable = prop.Where( x => !x.Groupable );
					if ( ungroupable.Any() ) {
						items.Add( createEditField( prop.Key.Type, ungroupable ) );
					}

					foreach ( var i in prop.Where( x => x.Groupable ).GroupBy( x => x.Value ) ) {
						items.Add( createEditField( prop.Key.Type, i ) );
					}
				}
			}
		}
	}

	Drawable createEditField ( Type type, IEnumerable<IProp> props ) {
		if ( type == typeof( float ) ) {
			var field = new FloatEditField { Title = props.First().Name };
			field.Apply( props.OfType<IProp<float>>() );
			return field;
		}
		else if ( type == typeof( int ) ) {
			var field = new IntEditField { Title = props.First().Name };
			field.Apply( props.OfType<IProp<int>>() );
			return field;
		}
		else if ( type == typeof( Colour4 ) ) {
			var field = new ColourEditField();
			field.Apply( props.OfType<IProp<Colour4>>() );
			return field;
		}

		throw new InvalidOperationException( $"No edit field for type {type.ReadableName()} exists." );
	}
}