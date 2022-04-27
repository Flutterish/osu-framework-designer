using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using OsuFrameworkDesigner.Game.Graphics;

namespace OsuFrameworkDesigner.Game.Containers.Properties;

public class ColourEditField : EditField<Colour4> {
	ColourSelectorBox box;
	DesignerSpriteText hex;

	public ColourEditField () {
		RelativeSizeAxes = Axes.X;
		Height = 30;
		Margin = new( 2 );

		AddInternal( new FillFlowContainer { Spacing = new( 10 ) }.FilledHorizontal().WithChildren(
			box = new ColourSelectorBox {
				Width = 60,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				RelativeSizeAxes = Axes.Y
			},
			hex = new DesignerSpriteText {
				Colour = Colour4.Black,
				Alpha = 0.5f,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Font = DesignerFont.Monospace( 20 )
			}
		) );

		box.Current.ValueChanged += v => SetValue( v.NewValue );
	}

	protected override void UpdateDisplay () {
		hex.Text = Props[0].Value.ToHex();
		box.Colour = Props[0].Value;
		box.Current.Value = Props[0].Value;
	}

	private class ColourSelectorBox : Box, IHasPopover {
		public Popover? GetPopover () {
			var popover = new PopoverColourSelector();
			popover.ColourPicker.Current.Value = Colour;
			popover.ColourPicker.Current.BindTo( Current );
			return popover;
		}

		protected override bool OnClick ( ClickEvent e ) {
			this.ShowPopover();
			return base.OnClick( e );
		}

		public readonly Bindable<Colour4> Current = new();
	}

	private class PopoverColourSelector : Popover {
		public readonly BasicColourPicker ColourPicker;

		public PopoverColourSelector () {
			Child = ColourPicker = new BasicColourPicker();
			Content.Padding = new MarginPadding( 10 );
			Background.Alpha = 0;
		}

		protected override Drawable CreateArrow ()
			=> Empty();

		private const float fade_duration = 250;
		private const double scale_duration = 500;
		protected override void PopIn () {
			this.ScaleTo( 1, scale_duration, Easing.OutElasticHalf );
			this.FadeIn( fade_duration, Easing.OutQuint );
		}

		protected override void PopOut () {
			this.ScaleTo( 0.7f, scale_duration, Easing.OutQuint );
			this.FadeOut( fade_duration, Easing.OutQuint );
		}
	}
}
