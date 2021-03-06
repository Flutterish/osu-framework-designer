# osu!framework Designer
A figma-like visual editor for osu!framework with an ability to export designs as code.

## Roadmap:
* [X] Visual editor for all basic osu!framework shapes
  * [X] Basic Transforms ( Position, Size, Scale, Shear, Rotation, Origin, Matrix operations )
  * [X] Common Properties ( Name, Fill colour )
  * [X] Box/Sprite ( Corner radius, Texture )
  * [X] Circle/Circular Progress/Arc ( Fill, Sweep, Texture )
  * [X] Triangle ( 3 vertice positions, Texture )
* [X] Additional Shapes
  * [X] Line/Arrow ( start/end point, width )
  * [X] Convex Polygon ( vertice count, Corner radius, Texture )
* [ ] Text
  * [ ] Basic text editing
  * [ ] Font uploading
  * [ ] Rich text formatting
* [ ] Addditinal properties
  * [ ] Border colour and thickness
  * [ ] Gradients
    * [ ] ColourInfo Gradient
    * [ ] Proper Gradient
  * [ ] Texture coordinates
  * [ ] Automatic Texture UV modes for applicable shapes ( some shapes have more than one "valid" way to use a texture )
* [X] Asset panel ( working, will be redesigned ) 
* [X] Property panel ( working, will be redesigned ) 
* [X] History ( undo/redo for all actions )
* [X] History Panel ( list of all recorded changes - working, will be redesigned )
* [ ] Copy-paste
* [X] Snap Guides ( point, line and line intersection guides )
  * [ ] TODO Snap when shearing
* [ ] Keyboard shortcuts for visual actions
  * [X] Multiple placement with shape tools ( currently shift )
  * [ ] Proportional scaling
  * [ ] Duplicate
  * [X] Dont use snap guides ( currently control )
* [ ] Grouping
* [ ] Effects ( glow and such. Since they use a buffered contaiener, they will be marked as bad for performance )
* [ ] Unions
  * [ ] Union ( A∪B )
  * [ ] Substract ( A/B )
  * [ ] Intersect ( A∩B )
  * [ ] Exclude ( (A∪B)/(A∩B) )
* [ ] Auto layout using osu!famework containers
  * [ ] FillFlowContainer
  * [ ] GridContainer
  * [ ] TableContainer
  * [ ] FlexBox
* [ ] Interactive containers
  * [ ] ScrollContainer
  * [ ] RearrangeableListContainer
  * [ ] SearchContainer
  * [ ] TabbableContainer
* [ ] Components
* [ ] Ability to export designs as code
* [ ] Feature parity with Figma
* [ ] Ability to load figma files
* [ ] Shared online sessions
* [ ] Support for plugins
* [ ] Live code (with external editors)
