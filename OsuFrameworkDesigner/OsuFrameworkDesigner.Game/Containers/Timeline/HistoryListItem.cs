using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Persistence;

namespace OsuFrameworkDesigner.Game.Containers.Timeline;

public class HistoryListItem : CompositeDrawable {
	TextFlowContainer text;
	IChange change;

	public HistoryListItem ( IChange change ) {
		this.change = change;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		AddInternal( new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		AddInternal( text = new TextFlowContainer( s => {
			s.Font = DesignerFont.Default( 16 );
			s.Colour = Colour4.Black;
		} ) {
			AutoSizeAxes = Axes.Y,
			RelativeSizeAxes = Axes.X
		} );

		if ( change is ComponentChange componentSingle ) {
			text.Text = $"{componentSingle.Type} {componentSingle.Target.NameOrDefault()}";
		}
		else if ( change is ComponentsChange componentMultiple ) {
			if ( componentMultiple.Target.Length == 1 ) {
				text.Text = $"{componentMultiple.Type} {componentMultiple.Target[0].NameOrDefault()}";
			}
			else {
				text.Text = $"{componentMultiple.Type} {componentMultiple.Target.Length} Components";
			}
		}
		else if ( change is PropChange propSingle ) {
			var target = propSingle.Target;
			text.Text = $"Changed {target.Prototype.Category}/{target.Prototype.UnqualifiedName} from {propSingle.PreviousValue} to {propSingle.NextValue}";
		}
		else if ( change is PropsChange propMultiple ) {
			if ( propMultiple.Target.Length == 1 ) {
				var c = propMultiple.Target[0];
				var target = c.Target;
				text.Text = $"Changed {target.Prototype.Category}/{target.Prototype.UnqualifiedName} from {c.PreviousValue} to {c.NextValue}";
			}
			else {
				text.Text = $"Changed {propMultiple.Target.Length} Props";
			}
		}
		else {
			text.Text = change.ToString();
		}
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
		Clicked?.Invoke( change, e );
		return true;
	}

	public event Action<IChange, ClickEvent>? Clicked;
}
