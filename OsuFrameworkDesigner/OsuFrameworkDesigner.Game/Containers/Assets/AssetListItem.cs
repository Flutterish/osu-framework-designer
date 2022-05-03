using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Containers.Assets;

public class AssetListItem : CompositeDrawable {
	BasicTextBox name;
	Box icon;

	public AssetListItem () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		AddInternal( new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		AddInternal( new FillFlowContainer { Spacing = new( 4 ), AutoSizeAxes = Axes.Y, Direction = FillDirection.Horizontal }.WithChildren(
			icon = new Box {
				Size = new( 16 ),
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			},
			name = new() {
				Height = 26,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			}
		) );

		name.CommitOnFocusLost = true;
		name.OnCommit += ( t, v ) => {
			Component.Name = t.Text;
		};
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

	public IComponent Component { get; private set; } = null!;
	protected IHasFillColour? ComponentColour { get; private set; }

	protected virtual void OnApply ( IComponent component ) { }
	public void Apply ( IComponent component ) {
		Component = component;
		ComponentColour = IHasFillColour.From( component );
		onNameUpdated();

		OnApply( component );
	}

	protected virtual void OnFree () { }
	public void Free () {
		Component = null!;
		ComponentColour = null;
		Selected = false;
		OnFree();
	}

	protected override void Update () {
		base.Update();

		icon.Colour = ComponentColour?.FillColour.Value ?? Colour4.Black;
		name.Width = DrawWidth - 20;
	}

	void onNameUpdated () {
		if ( string.IsNullOrWhiteSpace( Component.Name ) )
			name.Text = Component.GetType().ReadableName();
		else
			name.Text = Component.Name;
	}

	Bindable<Colour4> selectionColour = new( Theme.SelectionDefault );

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		selectionColour.BindTo( colours.Selection );
		selectionColour.BindValueChanged( v => BorderColour = v.NewValue, true );
	}

	protected override bool OnClick ( ClickEvent e ) {
		Clicked?.Invoke( Component, e );
		return true;
	}

	public event Action<IComponent, ClickEvent>? Clicked;
}
