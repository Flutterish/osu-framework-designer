using osu.Framework.Caching;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class PropertiesPanel : CompositeDrawable {
	Box background;
	Bindable<Colour4> backgroundColor = new( Theme.SidePanelDefault );
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
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.SidePanel );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}

	Cached componentCache = new();
	protected override void Update () {
		base.Update();

		if ( !componentCache.IsValid ) {
			componentCache.Validate();

			foreach ( var (field, description, pool) in rentedFields ) {
				description.FreeEditField( field );
				pool.Push( field );

				items.Remove( field );
			}
			rentedFields.Clear();

			items.Clear();
			if ( Components.Count == 1 ) {
				var comp = Components.Single();
				var name = new DesignerSpriteText { 
					Font = DesignerFont.Bold( 24 ), 
					Colour = Colour4.Black, 
					RelativeSizeAxes = Axes.X, 
					AlwaysPresent = true 
				};
				items.Add( name );
				name.OnUpdate += _ => {
					name.Text = comp.NameOrDefault();
				};
			}
			else if ( Components.Any() ) {
				var name = new DesignerSpriteText { Text = $"{Components.Count} Selected", Font = DesignerFont.Bold( 24 ), Colour = Colour4.Black, RelativeSizeAxes = Axes.X };
				items.Add( name );
			}

			foreach ( var category in Components.SelectMany( c => c.GetNestedProperties() ).GroupBy( x => x.Prototype.Category ) ) {
				items.Add( new DesignerSpriteText { Text = category.Key, Font = DesignerFont.Bold( 18 ), Colour = Colour4.Black, Alpha = 0.5f, RelativeSizeAxes = Axes.X } );

				foreach ( var prop in category.GroupBy( x => x.Prototype ) ) {
					if ( prop.Key.Groupable ) {
						foreach ( var i in prop.GroupBy( x => x.Value ) ) {
							items.Add( createEditField( prop.Key, i ) );
						}
					}
					else {
						items.Add( createEditField( prop.Key, prop ) );
					}
				}
			}
		}
	}

	List<(Drawable field, PropDescription description, Stack<Drawable> pool)> rentedFields = new();
	Dictionary<PropDescription, Stack<Drawable>> editFieldPool = new();
	Drawable createEditField ( PropDescription description, IEnumerable<IProp> props ) {
		if ( !editFieldPool.TryGetValue( description, out var pool ) )
			editFieldPool.Add( description, pool = new() );

		if ( !pool.TryPop( out var field ) )
			field = description.CreateEditField( description );

		description.ApplyEditField( field, props );
		rentedFields.Add( (field, description, pool) );
		return field;
	}
}