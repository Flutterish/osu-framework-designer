using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace OsuFrameworkDesigner.Game.Tools;
public class ToolButton : Button {
	Box background;
	Bindable<Colour4> backgroundColor = new( Theme.TopbarButtonDefault );
	Bindable<Colour4> hoverColor = new( Theme.TopbarButtonHoverDefault );
	Bindable<Colour4> activeColor = new( Theme.TopbarButtonActiveDefault );

	SpriteIcon icon;
	Bindable<Colour4> iconColor = new( Theme.TopbarButtonIconDefault );
	public IconUsage Icon {
		get => icon.Icon;
		set => icon.Icon = value;
	}

	public readonly BindableBool Selected = new();
	public readonly Tool Tool;

	public ToolButton ( Tool tool ) {
		Tool = tool;
		RelativeSizeAxes = Axes.Y;
		Width = 60;
		AddInternal( background = new Box().Fill() );
		AddInternal( icon = new SpriteIcon {
			Icon = FontAwesome.Solid.QuestionCircle,
			Size = new( 0.6f )
		}.Fill().Center() );

		Selected.BindValueChanged( _ => updateColour() );
	}

	[BackgroundDependencyLoader]
	private void load ( Theme colours ) {
		iconColor.BindTo( colours.TopbarButtonIcon );
		backgroundColor.BindTo( colours.TopbarButton );
		hoverColor.BindTo( colours.TopbarButtonHover );
		activeColor.BindTo( colours.TopbarButtonActive );

		icon.FadeColour( iconColor );
		updateColour();
		FinishTransforms( true );
	}

	private bool isPressed;
	private void updateColour () {
		backgroundColor.UnbindEvents();
		hoverColor.UnbindEvents();
		activeColor.UnbindEvents();

		background.FadeColour( ( isPressed || Selected.Value ) ? activeColor : IsHovered ? hoverColor : backgroundColor );
	}

	protected override bool OnHover ( HoverEvent e ) {
		updateColour();
		return base.OnHover( e );
	}

	protected override void OnHoverLost ( HoverLostEvent e ) {
		updateColour();
		base.OnHoverLost( e );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		isPressed = true;
		updateColour();
		return base.OnMouseDown( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		isPressed = false;
		updateColour();
		base.OnMouseUp( e );
	}

	protected override bool OnClick ( ClickEvent e ) {
		Selected.Value = true;
		return base.OnClick( e );
	}
}
