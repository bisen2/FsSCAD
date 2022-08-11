namespace FsSCAD

module Components =
  open System

  type MultiMatrix_Row = float * float * float
  type MultiMatrix = MultiMatrix_Row * MultiMatrix_Row * MultiMatrix_Row

  /// Type describing a valid hexadecimal color definition.
  type HexColor = HexColor of int
  /// Validated constructor for the `HexColor` type.
  /// If the value is not within the range of valid hexadecimal color definitions, `ArgumentOutOfRange` exception is raised.
  let HexColor (value: int) =
    if value <= 0xffffff && value >= 0 then HexColor value
    else ArgumentOutOfRangeException(nameof(value)) |> raise

  /// Type describing an arbitrary OpenSCAD component.
  /// A component can be a base component, a transformation of a target component, or a combination of multiple components.
  type Component =
    // Base components
    | Cube of size:(float*float*float) * center:bool
    | Sphere of radius:float * diameter:float * fragmentAngle:float * fragmentSize:float * resolution:float
    | Cylinder of height:float * radius1:float * radius2:float * center:bool
    | Polyhedron of points:list<int*int*int> * faces:list<int*int*int> * convexity:int
    // Transformations of components
    | Scale of vector:(float*float*float) * target:Component
    | Resize of newSize:(float*float*float) * target:Component
    | Rotate of angle:float * vector:(float*float*float) * target:Component
    | Translate of vector:(float*float*float) * target:Component
    | Mirror of vector:(float*float*float) * target:Component
    | MultiMatrix of mmatrix:MultiMatrix * target:Component
    | ColorRGBA of color:(float*float*float*float) * target:Component
    | ColorHex of color:HexColor * target:Component
    | ColorName of color:ColorName * target:Component
    | Offset of r:float * delta:float * chamfer:bool * target:Component
    | Minkowski of target:Component
    | Hull of target:Component
    // Combinations of components
    | Union of targets:list<Component>
    | Intersection of targets:list<Component>
    | Difference of baseComponent:Component * diffComponents:list<Component>

  /// The interface that all objects describing a components must use to expose their internal structure.
  type IComponent =
    /// The internal representation of the component.
    abstract member Component: Component

  module Component =

    let boolToString: bool -> string = function
      | true -> "true"
      | false -> "false"

    let ptsToString: list<int*int*int> -> string =
      List.map (fun (x, y, z) -> $"[{x}, {y}, {z}]")
      >> fun strs -> System.String.Join(',', strs)
      >> sprintf "[ %s ]"

    let multiMatrixToString (((x1,y1,z1), (x2,y2,z2), (x3,y3,z3)): MultiMatrix) : string =
      $"[ [{x1}, {y1}, {z1} ], [ {x2}, {y2}, {z2} ], [ {x3}, {y3}, {z3} ] ]"

    let componentsToString (toSCADFunc: Component -> string) =
      List.map toSCADFunc >> fun strs -> System.String.Join(';', strs)

    let rec toFormattedSCAD (indentLevel: uint) (comp: Component) : string =
      let indent = String(' ', indentLevel*4u |> int)
      let subComponent comp = $"{Environment.NewLine}{toFormattedSCAD (indentLevel+1u) comp}"
      let collection: list<Component> -> string =
        List.map (subComponent >> fun str -> $"{str};")
        >> System.String.Concat
      match comp with
      // Base components
      | Cube ((x, y, z), center) -> $"{indent}cube(size = [{x}, {y}, {z}], center = {boolToString center})"
      | Sphere (r, d, fa, fs, fn) -> $"{indent}sphere(r = {r}, d = {d}, $fa = {fa}, $fs = {fs}, $fn = {fn}"
      | Cylinder (h, r1, r2, center) -> $"{indent}cylinder(h = {h}, r1 = {r1}, r2 = {r2}, center = {boolToString center})"
      | Polyhedron (points, faces, convexity) -> $"{indent}polyhedron(points = {ptsToString points}, faces = {ptsToString faces}, convexity = {convexity})"
      // Transformations of components
      | Scale ((x, y, z), target) -> $"{indent}scale(v = [{x}, {y}, {z}]){subComponent target}"
      | Resize ((x, y, z), target) -> $"{indent}resize(newSize = [{x}, {y}, {z}]){subComponent target}"
      | Rotate (a, (x, y, z), target) -> $"{indent}rotate(a = {a}, v = [{x}, {y}, {z}]){subComponent target}"
      | Translate ((x, y, z), target) -> $"{indent}translate(v = [{x}, {y}, {z}]){subComponent target}"
      | Mirror ((x, y, z), target) -> $"{indent}mirror(v = [{x}, {y}, {z}]){subComponent target}"
      | MultiMatrix (m, target) -> $"{indent}multimatrix(m = {multiMatrixToString m}){subComponent target}"
      | ColorRGBA ((r, g, b, a), target) -> $"{indent}color(c = [{r}, {g}, {b}, {a}]){subComponent target}"
      | ColorHex (c, target) -> $"{indent}color(c = {c}){subComponent target}"
      | ColorName (c, target) -> $"{indent}color(c = {c}){subComponent target}"
      | Offset (r, delta, chamfer, target) -> $"{indent}offset(r = {r}, delta = {delta}, chamfer = {boolToString chamfer}){subComponent target}"
      | Minkowski (target) -> $"{indent}minkowski(){subComponent target}"
      | Hull (target) -> $"{indent}hull(){subComponent target}"
      // Combinations of components
      | Union targets -> $"{indent}union() {{{collection targets}{indent}{Environment.NewLine}}}"
      | Intersection targets -> $"{indent}intersection() {{{collection targets}{indent}{Environment.NewLine}}}"
      | Difference (baseComponent, diffComponents) -> $"{indent}difference() {{{collection (baseComponent :: diffComponents)}{indent}{Environment.NewLine}}}"

    let toSCAD comp = toFormattedSCAD 0u comp
