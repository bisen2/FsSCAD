# FsSCAD
An F# library for generating [OpenSCAD](https://github.com/openscad/openscad/) designs.

**Note: This project is in the very early experimental phases. It currently only supports a small subset of OpenSCAD and is likely to change a lot as I decide how I want it to work in different situations.**

```fs
// defining a type to describe a post with a slot cut in both the top and bottom
type Post =
    {   Width: float
        Height: float
        SlotDepth: float
        SlotAngle: float }
    // our type implements the `IComponent` interface, meaning that other objects can expect to find the `Component` property which exposes the internal structure of the component.
    interface IComponent with
        member this.Component =
        Difference(
            baseComponent = rect (this.Width, this.Width, this.Height),
            diffComponents = [
                // top slot
                centerRect (this.Width/3., this.Width*2., this.SlotDepth)
                |> rotateZ this.SlotAngle
                |> translate (this.Width/2., this.Width/2., this.Height-this.SlotDepth/2.)
                // bottom slot
                centerRect (this.Width/3., this.Width*2., this.SlotDepth)
                |> rotateZ this.SlotAngle
                |> translate (this.Width/2., this.Width/2., this.SlotDepth/2.)
            ]
        )

// defining an instance of the `Post` type
let myPost = {
    Post.Width = 3.
    Height = 24.
    SlotDepth = 2.5
    SlotAngle = 45.
}

// printing the OpenSCAD code to generate the post
(myPost :> IComponent).Component
|> Component.toSCAD
|> printfn "%s"
```

This library allows you to define and manipulate a `Component` object in F# code, then generate the OpenSCAD code that defines the same object. This allows me to circumvent some aspects of the OpenSCAD language that I find clunky. Below are some of the issues I would like to address with this wrapper.

## Provide strong typing of components
By providing types to all components, we can make illogical states un-compilable. For example, the following is valid OpenSCAD code and will compile.
```scad
rotate(a = 20, v = [45, 45, 45])
    translate(v = [10, 20, 30]);
```
Conceptually, this code makes no sense. We have a rotation and a translation, but there is no object that they are being applied to. By providing strong typing via F#'s type system, we can explicitly make this sort of design illegal.
```fs
// definition of a simplified `Component` type
type Component =
    | Cube of size:(float*float*float) * center:bool
    | Rotate of a:float * v:(float*float*float) * target:Component
    | Translate of v:(float*float*float) * target:Component
// throws a compiler error due to `Translate` not having a target component
let invalid =
    Rotate(20, (45,45,45),
        Translate(10, 20, 30)
    )
// does not throw a compiler error
let valid =
    Rotate(20, (45, 45, 45),
        Translate((10, 20, 30),
            Cube(size = (10, 10, 10), center = false)
        )
    )
```
Obviously this syntax is not the most convenient - we will talk more about that in the next section.


## Allow pipeline composition of components and transformations
By providing helper functions with curried arguments we can make the application and composition of components and transformations explicit.
```fs
let cube side = Cube(size = [side, side, side], center = false) // float -> Component
let rotate angle vector target = Rotate(angle, vector, target) // float -> float * float * float -> Component -> Component
let translate vector target = Translate(vector, target) // float * float * float -> Component -> Component

// we can pipe a component through various transformations
let myCube = // Component
    cube 10
    |> rotate 20 (45, 45, 45)
    |> translate (10, 20, 30)

// or define a custom transformation as a composition of base transformations
let myTranslation = // Component -> Component
    rotate 20 (45, 45, 45)
    >> translate (10, 20, 30)

// or even still use the cascade style similar to OpenSCAD
let myCube1 = // Component
    translate (10, 20, 30) (rotate 20 (45, 45, 45) (cube 10))
```


## Avoid the implicit collections in boolean operations (`union`, `difference`, and `intersection`).
All of the boolean operations in OpenSCAD take in an arbitrary number of components and perform an operation over all of them. This collection is implicit and simply appears as multiple semi-colon seperated components in the `{}` block of the operation. The `difference` operation is particularly interesting in this regard as the first element of the collection is treated as the base object that all of the other elements are subtracted from. By working these collections into the `Component` type, we can make these collections explicit, as well as making the typing reflect that the first component of the `difference` operation is different from the others.
```fs
// partial definition of the `Component` type
type Component =
    // ... Other cases of the `Component` type
    | Union of targets:list<Component>
    | Intersection of targets:list<Component>
    | Difference of baseComponent:Component * diffComponents:list<Component>

let myHollowCube = // Component
    Difference(
        baseComponent = centerCube 10
        diffComponents = [
            centerCube 5
            centerCube 5 |> translate (0, 0, 5)
            centerCube 5 |> translate (0, 5, 0)
            centerCube 5 |> translate (5, 0, 0)
            centerCube 5 |> translate (0, 0, -5)
            centerCube 5 |> translate (0, -5, 0)
            centerCube 5 |> translate (-5, 0, 0)
        ]
    )
```

As a nice consequence of using F#'s list, we can also use sequence expressions to programmatically generate the contents of these collections.
```fs
// a series of cubes every 10 units along the x axis
let myCubes = // Component
    Union [
        for i in 0 .. 10 .. 100 ->
            centerCube 5 |> translate (i, 0, 0)
    ]
// The same using the `List` module functions
let myCubes1 = // Component
    [ 0 .. 10 .. 100 ] // list<int>
    |> List.map (fun i -> translate (i, 0, 0)) // list<(Component -> Component)>
    |> List.map (fun f -> centerCube 5 |> f) // list<Component>
    |> Union // Component
```

## Allow F# to handle the modularity of components
Although OpenSCAD provides a module system to handle parameterization of components, there are several advantages that we can realize by expressing that modularity on the F# side. Consider this example of a beam with a square through mortise cut into it.
```fs
// defining a record type to hold the parameters of the `MortisedBeam` object
type MortisedBeam =
    {   Width: float
        Height: float
        Length: float
        MortiseSize: float
        MortisePosition: float }
    // defining a default set of parameters
    static member Default = {
        Width = 2
        Height = 4
        Length = 10
        MortiseSize = 1
        MortisePosition = 1
    }
    // implementing the `IComponent` interface so that the OpenSCAD definition can be generated
    interface IComponent with
        member this.Component =
            Difference(
                baseComponent = centerRect (this.Length, this.Width, this.Height)
                diffComponents = [
                    centerRect (this.MortiseSize, this.Width, this.MortiseSize)
                    |> translate (this.MortisePosition, 0, 0)
                ]
            )
// as a consequence of building this on top of a record type, we can use copy and update semantics to create a new version of the beam based on the defaults
let mortisedBeam_20ft = { MortisedBeam.Default with Length = 20 } // MortisedBeam
// or we can define a function that produces new beams based on inputs
let moveMortise position beam = { beam with MortisePosition = position } // float -> MortisedBeam -> MortisedBeam
// and by combining the read and update semantics with the collection types from F#, we can efficiently perform changes on a collection of components
let moveMortises position beams = // float -> list<MortisedBeam> -> list<MortisedBeam>
    beams // list<MortisedBeam>
    |> List.map (moveMortise position) // list<MortisedBeam>
```
