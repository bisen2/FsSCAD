module TestUtils

open FsCheck
open FsCheck.Xunit

// defines an Arbitrary<float> that does not include NaN, infinity, etc
type NormalFloat =
  static member Double() =
    Arb.Default.Float()
    |> Arb.filter System.Double.IsNormal

// tells FsCheck.Xunit to override the default Arbitrary<float> with ours
[<assembly: Properties(Arbitrary = [| typeof<NormalFloat> |])>]
do ()
