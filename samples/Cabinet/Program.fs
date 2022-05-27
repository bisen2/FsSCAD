open System.IO
open FsSCAD.Components
open Post

[<EntryPoint>]
let main _ =
  // make a `tmp` directory if it doesn't exist
  Directory.CreateDirectory("./tmp") |> ignore
  // create our design text
  let scad =
    { Width = 3.
      Height = 24.
      SlotDepth = 2.5
      Angle1 = 25.
      Angle2 = 10. }
    |> fun post -> (post :> IComponent).Component
    |> Component.toSCAD
  // write the design text to a file
  File.WriteAllText("./tmp/output.scad", scad)
  // return success error code
  0
