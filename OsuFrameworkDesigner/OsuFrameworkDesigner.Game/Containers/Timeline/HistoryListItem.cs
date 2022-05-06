using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Persistence;

namespace OsuFrameworkDesigner.Game.Containers.Timeline;

public class HistoryListItem : CompositeDrawable {
	protected TextFlowContainer Text;

	public HistoryListItem () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		AddInternal( new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		AddInternal( Text = new TextFlowContainer( s => {
			s.Font = DesignerFont.Default( 16 );
			s.Colour = Colour4.Black;
		} ) {
			AutoSizeAxes = Axes.Y,
			RelativeSizeAxes = Axes.X
		} );
	}

	protected IChange Change { get; private set; } = null!;

	protected virtual void OnApply () {
		if ( Change is ComponentChange componentSingle ) {
			Text.Text = $"{componentSingle.Type} {componentSingle.Target.NameOrDefault()}";
		}
		else if ( Change is ComponentsChange componentMultiple ) {
			if ( componentMultiple.Target.Length == 1 ) {
				Text.Text = $"{componentMultiple.Type} {componentMultiple.Target[0].NameOrDefault()}";
			}
			else {
				Text.Text = $"{componentMultiple.Type} {componentMultiple.Target.Length} Components";
			}
		}
		else if ( Change is PropChange propSingle ) {
			var target = propSingle.Target;
			Text.Text = $"Changed {target.Prototype.Category}/{target.Prototype.UnqualifiedName} from {propSingle.PreviousValue} to {propSingle.NextValue}";
		}
		else if ( Change is PropsChange propMultiple ) {
			if ( propMultiple.Target.Length == 1 ) {
				var c = propMultiple.Target[0];
				var target = c.Target;
				Text.Text = $"Changed {target.Prototype.Category}/{target.Prototype.UnqualifiedName} from {c.PreviousValue} to {c.NextValue}";
			}
			else {
				Text.Text = $"Changed {propMultiple.Target.Length} Props";
			}
		}
		else {
			Text.Text = Change.ToString();
		}
	}
	public void Apply ( IChange change ) {
		Change = change;
		OnApply();
	}

	protected virtual void OnFree () { }
	public void Free () {
		OnFree();
		Change = null!;
	}

	bool selected;
	public bool Selected {
		get => selected;
		set {
			selected = value;

			if ( value ) {
				Masking = true;
				BorderThickness = 4;
			}
			else {
				Masking = false;
				BorderThickness = 0;
			}
		}
	}

	Bindable<Colour4> selectionColour = new( Theme.SelectionDefault );

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		selectionColour.BindTo( colours.Selection );
		selectionColour.BindValueChanged( v => BorderColour = v.NewValue, true );
	}

	protected override bool OnClick ( ClickEvent e ) {
		Clicked?.Invoke( Change, e );
		return true;
	}

	public event Action<IChange, ClickEvent>? Clicked;
}
