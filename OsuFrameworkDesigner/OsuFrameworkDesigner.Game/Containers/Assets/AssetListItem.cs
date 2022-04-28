using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components.Interfaces;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Assets;

public class AssetListItem : CompositeDrawable {
	DesignerSpriteText name;
	Box icon;

	public AssetListItem () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		AddInternal( new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		AddInternal( new FillFlowContainer { Spacing = new( 4 ), AutoSizeAxes = Axes.Y, Direction = FillDirection.Horizontal }.WithChildren(
			icon = new Box {
				Size = new( 12 ),
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			},
			name = new() {
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Colour = Colour4.Black,
				Alpha = 0.8f
			}
		) );
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
		name.Text = component.Name;
		Component = component;
		ComponentColour = IHasFillColour.From( component );

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
		if ( string.IsNullOrWhiteSpace( Component.Name ) )
			name.Text = Component.GetType().ReadableName();
		else
			name.Text = Component.Name;
	}

	Bindable<Colour4> selectionColour = new( ColourConfiguration.SelectionDefault );

	[BackgroundDependencyLoader]
	private void load ( ColourConfiguration colours ) {
		selectionColour.BindTo( colours.Selection );
		selectionColour.BindValueChanged( v => BorderColour = v.NewValue, true );
	}

	protected override bool OnClick ( ClickEvent e ) {
		Clicked?.Invoke( Component, e );
		return true;
	}

	public event Action<IComponent, ClickEvent>? Clicked;
}
