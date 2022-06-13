module Post

open FsSCAD.Components
open FsSCAD.ComponentBuilders

type Post =
  { Width: float
    Height: float
    SlotDepth: float
    Angle1: float
    Angle2: float }

  interface IComponent with
    member this.Component =
      Difference(
        baseComponent = rect (this.Width, this.Width, this.Height),
        diffComponents = [
          // Top slots
          centerRect (this.Width/3., this.Width*2., this.SlotDepth+0.01)
          |> rotateZ this.Angle1
          |> translate (this.Width/2., this.Width/2., this.Height-this.SlotDepth/2.)

          centerRect (this.Width/3., this.Width*2., this.SlotDepth+0.01)
          |> rotateZ (90. - this.Angle2)
          |> translate (this.Width/2., this.Width/2., this.Height - this.SlotDepth / 2.)

          // Bottom slots
          centerRect (this.Width/3., this.Width*2., this.SlotDepth-0.01)
          |> rotateZ this.Angle1
          |> translate (this.Width/2., this.Width/2., this.SlotDepth/2. - 0.01)

          centerRect (this.Width/3., this.Width*2., this.SlotDepth-0.01)
          |> rotateZ (90. - this.Angle2)
          |> translate (this.Width/2., this.Width/2., this.SlotDepth/2. - 0.01)
        ]
      )
