using osu.Framework.Caching;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class PropertiesTab : FillFlowContainer {
	public PropertiesTab () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		Direction = FillDirection.Full;

		Components.BindCollectionChanged( ( _, _ ) => componentCache.Invalidate() );
	}

	public readonly BindableList<IComponent> Components = new();

	Cached componentCache = new();
	protected override void Update () {
		base.Update();

		if ( !componentCache.IsValid ) {
			componentCache.Validate();

			foreach ( var (field, description, pool) in rentedFields ) {
				description.FreeEditField( field );
				pool.Push( field );

				Remove( field );
			}
			rentedFields.Clear();

			Clear();
			if ( Components.Count == 1 ) {
				var comp = Components.Single();
				var name = new DesignerSpriteText {
					Font = DesignerFont.Bold( 24 ),
					Colour = Colour4.Black,
					RelativeSizeAxes = Axes.X,
					AlwaysPresent = true
				};
				Add( name );
				name.OnUpdate += _ => {
					name.Text = comp.NameOrDefault();
				};
			}
			else if ( Components.Any() ) {
				var name = new DesignerSpriteText { Text = $"{Components.Count} Selected", Font = DesignerFont.Bold( 24 ), Colour = Colour4.Black, RelativeSizeAxes = Axes.X };
				Add( name );
			}

			foreach ( var category in Components.SelectMany( c => c.GetNestedProperties() ).GroupBy( x => x.Prototype.Category ) ) {
				Add( new DesignerSpriteText { Text = category.Key, Font = DesignerFont.Bold( 18 ), Colour = Colour4.Black, Alpha = 0.5f, RelativeSizeAxes = Axes.X } );

				foreach ( var prop in category.GroupBy( x => x.Prototype ) ) {
					if ( prop.Key.Groupable ) {
						foreach ( var i in prop.GroupBy( x => x.Value ) ) {
							Add( createEditField( prop.Key, i ) );
						}
					}
					else {
						Add( createEditField( prop.Key, prop ) );
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