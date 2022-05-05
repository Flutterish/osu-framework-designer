using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Components;
using OsuFrameworkDesigner.Game.Components.Interfaces;

namespace OsuFrameworkDesigner.Game.Containers.Assets;

public class AssetListItem : CompositeDrawable {
	BasicTextBox name;
	Sprite icon;

	public AssetListItem () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		AddInternal( new Box { Alpha = 0, AlwaysPresent = true }.Fill() );
		AddInternal( new FillFlowContainer { Spacing = new( 4 ), AutoSizeAxes = Axes.Y, Direction = FillDirection.Horizontal }.WithChildren(
			new Container {
				Size = new( 16 ),
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Child = icon = new Sprite {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fit
				}
			},
			name = new() {
				Height = 26,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			}
		) );

		name.CommitOnFocusLost = true;
		name.OnCommit += ( t, v ) => {
			Component.Name.Value = t.Text;
			onNameUpdated();
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
	protected IProp<Colour4>? ComponentColour { get; private set; }
	protected IProp<Texture>? ComponentTexture { get; private set; }
	protected IProp<string> ComponentName { get; private set; } = null!;

	protected virtual void OnApply ( IComponent component ) { }
	public void Apply ( IComponent component ) {
		Component = component;
		ComponentName = component.Name;
		ComponentColour = component.GetProperty<Colour4>( PropDescriptions.FillColour );
		ComponentTexture = component.GetProperty<Texture>( PropDescriptions.Texture );
		onNameUpdated();

		ComponentName.ValueChanged += onComponentNameChanged;

		OnApply( component );
	}

	void onComponentNameChanged ( ValueChangedEvent<string> e ) {
		onNameUpdated();
	}

	protected virtual void OnFree () { }
	public void Free () {
		ComponentName.ValueChanged -= onComponentNameChanged;

		Component = null!;
		ComponentColour = null;
		Selected = false;
		ComponentName = null!;
		OnFree();
	}

	protected override void Update () {
		base.Update();

		icon.Colour = ComponentColour?.Value ?? Colour4.White;
		icon.Texture = ComponentTexture?.Value ?? Texture.WhitePixel;
		name.Width = DrawWidth - 20;
	}

	void onNameUpdated () {
		name.Text = Component.NameOrDefault();
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
