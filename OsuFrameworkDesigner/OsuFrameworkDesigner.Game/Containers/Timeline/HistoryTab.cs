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

	void addChange ( IChange change ) {
		var item = new HistoryListItem( change );
		Add( item );
		itemByChange.Add( change, item );
		item.Clicked += ( c, e ) => {
			History.NavigateTo( c );
		};
	}

	void removeChange ( IChange change ) {
		itemByChange.Remove( change, out var item );
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
