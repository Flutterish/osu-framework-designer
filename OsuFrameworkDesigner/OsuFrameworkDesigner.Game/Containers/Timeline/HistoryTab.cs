using OsuFrameworkDesigner.Game.Persistence;

namespace OsuFrameworkDesigner.Game.Containers.Timeline;

public class HistoryTab : FillFlowContainer {
	public readonly History History;

	Dictionary<IChange, HistoryListItem> itemByChange = new();
	HistoryListItem? selectedItem;
	public HistoryTab ( History history ) {
		AutoSizeAxes = Axes.Y;
		RelativeSizeAxes = Axes.X;
		Direction = FillDirection.Vertical;
		Spacing = new( 5 );
		History = history;

		history.ChangeAdded += addChange;
		history.ChangeRemoved += removeChange;
		foreach ( var i in history.Changes ) {
			addChange( i );
		}

		history.NavigatedBack += onHistoryNavigated;
		history.NavigatedForward += onHistoryNavigated;
		if ( history.LatestChange != null )
			onHistoryNavigated( history.LatestChange );
	}

	void onHistoryNavigated ( IChange change ) {
		if ( selectedItem != null )
			selectedItem.Selected = false;

		if ( History.LatestChange != null )
			( selectedItem = itemByChange[History.LatestChange] ).Selected = true;
		else
			selectedItem = null;
	}

	Stack<HistoryListItem> itemPool = new();
	void addChange ( IChange change ) {
		if ( !itemPool.TryPop( out var item ) ) {
			item = new HistoryListItem();
			item.Clicked += ( c, e ) => {
				History.NavigateTo( c );
			};
		}

		item.Apply( change );
		Add( item );
		itemByChange.Add( change, item );
	}

	void removeChange ( IChange change ) {
		itemByChange.Remove( change, out var item );
		item!.Free();
		itemPool.Push( item );
		Remove( item );
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );
		History.ChangeAdded -= addChange;
		History.ChangeRemoved -= removeChange;
		History.NavigatedBack -= onHistoryNavigated;
		History.NavigatedForward -= onHistoryNavigated;
	}
}
