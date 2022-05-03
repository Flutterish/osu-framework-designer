using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Graphics;
using OsuFrameworkDesigner.Game.Memory;
using osuTK.Input;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class TextureEditField : EditField<Texture>, IFileDropHandler {
	Sprite box;

	public TextureEditField () {
		RelativeSizeAxes = Axes.X;
		Height = 40;
		Margin = new( 2 );

		AddInternal( new FillFlowContainer { Spacing = new( 10 ) }.FilledHorizontal().WithChildren(
			box = new Sprite {
				Width = 60,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				RelativeSizeAxes = Axes.Y,
				FillMode = FillMode.Fit
			},
			new DesignerSpriteText {
				Colour = Colour4.Black,
				Alpha = 0.5f,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Font = DesignerFont.Monospace( 20 ),
				Text = "Texture"
			}
		) );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		if ( e.Button is MouseButton.Right ) {
			SetValue( Texture.WhitePixel );
			return true;
		}

		return base.OnMouseDown( e );
	}

	protected override void UpdateDisplay () {
		box.Texture = Props[0].Value;
	}

	[Resolved]
	FileTextureCache textureCache { get; set; } = null!;

	public override bool HandlePositionalInput => true;
	public bool OnFileDrop ( FileDropArgs args ) {
		textureCache.GetTextureAsync( args.File ).ContinueWith( v => {
			SetValue( v.Result );
		} );

		return true;
	}
}
