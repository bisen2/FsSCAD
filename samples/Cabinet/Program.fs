open FsSCAD.Components
open Post

open FsSCAD.ComponentBuilders

type MortisedBeam =
  { Width: float
    Height: float
    Length: float
    MortiseSize: float
    MortisePosition: float }
  static member Default = {
    Width = 2
    Height = 4
    Length = 10
    MortiseSize = 1
    MortisePosition = 1
  }
  interface IComponent with
    member this.Component =
      Difference(
        baseComponent = centerRect (this.Length, this.Width, this.Height),
        diffComponents = [
          centerRect (this.MortiseSize, this.Width, this.MortiseSize)
          |> translate (this.MortisePosition, 0, 0)
        ]
      )
      
let moveMortise position beam = { beam with MortisePosition = position}
let myBeam = { MortisedBeam.Default with Length = 20 }

{ Post.Width = 3.
  Height = 24.
  SlotDepth = 2.5
  Angle1 = 25.
  Angle2 = 10. }
|> fun post -> (post :> IComponent).Component
|> Component.toSCAD
|> printfn "%s"
