using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Containers.Assets;

public class AssetsPanel : CompositeDrawable {
	Box background;
	FillFlowContainer items;
	Bindable<Colour4> backgroundColor = new( Theme.SidePanelDefault );
	Stack<AssetListItem> listItemPool = new();
	Dictionary<IComponent, AssetListItem> itemsByComponent = new();

	public AssetsPanel () {
		RelativeSizeAxes = Axes.Y;
		Width = 300;
		AddInternal( background = new Box().Fill() );
		AddInternal( new BasicScrollContainer {
			Child = items = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Padding = new( 10 )
			}
		}.Fill() );

		Selection.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( IComponent i in e.OldItems ) {
					if ( itemsByComponent.TryGetValue( i, out var item ) )
						item.Selected = false;
				}
			}
			if ( e.NewItems != null ) {
				foreach ( IComponent i in e.NewItems ) {
					if ( itemsByComponent.TryGetValue( i, out var item ) )
						item.Selected = true;
				}
			}
		} );
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		backgroundColor.BindTo( colours.SidePanel );
		background.FadeColour( backgroundColor );
		FinishTransforms( true );
	}

	public void AddComponent ( IComponent component, IComponent? parent ) {
		if ( !listItemPool.TryPop( out var listItem ) ) {
			listItem = new();
			listItem.Clicked += onListItemClicked;
		}

		listItem.Apply( component );
		items.Add( listItem );
		itemsByComponent.Add( component, listItem );
	}

	void onListItemClicked ( IComponent comp, ClickEvent e ) {
		if ( e.ControlPressed ) {
			if ( !Selection.Contains( comp ) ) {
				Selection.Add( comp );
				SelectionChanged?.Invoke( Selection );
			}
			else {
				Selection.Remove( comp );
				SelectionChanged?.Invoke( Selection );
			}
		}
		else if ( e.ShiftPressed ) {
			if ( Selection.Count == 0 ) {
				Selection.Add( comp );
				SelectionChanged?.Invoke( Selection );
				return;
			}

			if ( !itemsByComponent.TryGetValue( comp, out var clickedItem ) )
				return;

			if ( !itemsByComponent.TryGetValue( Selection.Last(), out var lastItem ) )
				return;

			var start = items.IndexOf( lastItem );
			var end = items.IndexOf( clickedItem );
			if ( start > end )
				(start, end) = (end, start);

			for ( int i = start; i <= end; i++ ) {
				comp = ( (AssetListItem)items[i] ).Component;
				if ( !Selection.Contains( comp ) ) {
					Selection.Add( comp );
				}
			}

			SelectionChanged?.Invoke( Selection );
		}
		else {
			if ( Selection.Count != 1 || Selection.Single() != comp ) {
				Selection.Clear();
				Selection.Add( comp );
				SelectionChanged?.Invoke( Selection );
			}
		}
	}

	public void RemoveComponent ( IComponent component, IComponent? parent ) {
		if ( !itemsByComponent.Remove( component, out var listItem ) )
			return;

		items.Remove( listItem );
		listItem.Free();
		listItemPool.Push( listItem );
	}

	public readonly BindableList<IComponent> Selection = new();
	public event Action<IBindableList<IComponent>>? SelectionChanged;
}