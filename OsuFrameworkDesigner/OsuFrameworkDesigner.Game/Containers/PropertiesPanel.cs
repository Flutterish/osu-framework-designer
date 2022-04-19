using osu.Framework.Caching;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.UserInterface;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
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
		AddInternal( items = new FillFlowContainer().Vertical() );

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
			foreach ( var category in Components.SelectMany( Extensions.GetNestedProperties ).GroupBy( x => x.Category ) ) {
				items.Add( new DesignerSpriteText { Text = category.Key, Colour = Colour4.Black } );

				foreach ( var prop in category.GroupBy( x => (x.Name, x.Type) ) ) {
					items.Add( new DesignerSpriteText { Text = prop.Key.Name, Colour = Colour4.Black } );

					var ungroupable = prop.Where( x => !x.Groupable );
					if ( ungroupable.Any() ) {
						var distinct = ungroupable.Select( x => x.Value ).Distinct();
						items.Add( createEditField( prop.Key.Type, ungroupable, distinctValues: distinct.Count() != 1 ) );
					}

					foreach ( var i in prop.Where( x => x.Groupable ).GroupBy( x => x.Value ) ) {
						items.Add( createEditField( prop.Key.Type, i, distinctValues: false ) );
					}
				}
			}
		}
	}

	Drawable createEditField ( Type type, IEnumerable<IProp> props, bool distinctValues ) {
		if ( type == typeof( float ) ) {
			return new BasicTextBox {
				Text = distinctValues ? "Mixed" : $"{props.First().Value}",
				Height = 40,
				RelativeSizeAxes = Axes.X
			};
		}

		throw new InvalidOperationException( $"No edit field for type {type.ReadableName()} exists." );
	}
}