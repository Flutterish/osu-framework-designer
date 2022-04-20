using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;

namespace OsuFrameworkDesigner.Game.Cursor;

public class DesignerCursorContainer : CursorEffectContainer<DesignerCursorContainer, IUsesCursorStyle> {
	protected override Container<Drawable> Content => cursorContainer;
	ShapedCursorContainer cursorContainer;
	public DesignerCursorContainer () {
		AddInternal( cursorContainer = new ShapedCursorContainer().Fill() );
	}

	InputManager? inputManager;
	InputManager InputManager => inputManager ??= GetContainingInputManager();
	protected override void Update () {
		base.Update();

		var target = ( InputManager.DraggedDrawable as IUsesCursorStyle )
			?? FindTargets().FirstOrDefault();

		if ( target is IUsesCursorStyle cursorStyle ) {
			cursorContainer.Style = cursorStyle.CursorStyle;
			cursorContainer.CursorRotation = ( cursorStyle as IUsesCursorRotation )?.CursorRotation ?? 0;
		}
		else {
			cursorContainer.Style = CursorStyle.Default;
			cursorContainer.CursorRotation = 0;
		}
	}

	private class ShapedCursorContainer : CursorContainer {
		SpriteIcon cursor = null!;
		private Dictionary<CursorStyle, (IconUsage icon, Vector2 offset, float rotation, Anchor origin, Vector2 scale)> cursors = new() {
			[CursorStyle.Default] = (FontAwesome.Solid.MousePointer, new( -0.2f, 0 ), 0, Anchor.TopLeft, new( 1 )),
			[CursorStyle.Pointer] = (FontAwesome.Solid.HandPointer, new(), 0, Anchor.TopLeft, new( 1 )),
			[CursorStyle.ResizeNW] = (FontAwesome.Solid.ArrowsAltH, new(), 45, Anchor.Centre, new( 1.2f )),
			[CursorStyle.ResizeSW] = (FontAwesome.Solid.ArrowsAltH, new(), -45, Anchor.Centre, new( 1.2f )),
			[CursorStyle.ResizeDiagonal] = (FontAwesome.Solid.ExpandArrowsAlt, new(), 0, Anchor.TopLeft, new( 1 )),
			[CursorStyle.ResizeOrthogonal] = (FontAwesome.Solid.ArrowsAlt, new(), 0, Anchor.TopLeft, new( 1 )),
			[CursorStyle.ResizeHorizontal] = (FontAwesome.Solid.ArrowsAltH, new(), 0, Anchor.Centre, new( 1.2f )),
			[CursorStyle.ResizeVertical] = (FontAwesome.Solid.ArrowsAltV, new(), 0, Anchor.Centre, new( 1.2f )),
			[CursorStyle.Rotate] = (FontAwesome.Solid.RedoAlt, new(), 0, Anchor.Centre, new( 1 ))
		};

		public float CursorRotation {
			set => ActiveCursor.Rotation = value;
		}

		CursorStyle style;
		public CursorStyle Style {
			get => style;
			set {
				var (icon, offset, rotation, origin, scale) = cursors[style = value];
				cursor.Icon = icon;
				cursor.Rotation = rotation;
				cursor.Position = cursor.Size * offset;
				cursor.Origin = origin;
				cursor.Scale = scale;
			}
		}

		protected override Drawable CreateCursor ()
			=> new Container {
				Origin = Anchor.Centre,
				Size = new( 50 ),
				Child = cursor = new SpriteIcon {
					Icon = FontAwesome.Solid.MousePointer,
					Size = new( 20 ),
					Colour = Colour4.Black,
					Anchor = Anchor.Centre
				}
			};
	}
}
