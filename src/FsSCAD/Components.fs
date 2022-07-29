namespace FsSCAD

module Components =

  type MultiMatrix_Row = float * float * float
  type MultiMatrix = MultiMatrix_Row * MultiMatrix_Row * MultiMatrix_Row

  /// Type describing a valid hexadecimal color definition.
  type HexColor = HexColor of int
  /// Validated constructor for the `HexColor` type.
  /// If the value is not within the range of valid hexadecimal color definitions, `ArgumentOutOfRange` exception is raised.
  let HexColor (value: int) =
    if value <= 0xffffff && value >= 0 then HexColor value
    else System.ArgumentOutOfRangeException(nameof(value)) |> raise

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
    | ColorName of color:string * target:Component
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
      List.map toSCADFunc >> List.toArray >> fun strs -> System.String.Join(';', strs) + ";"

    /// Converts a component object to a string containing the OpenSCAD code to generate the component.
    let rec toSCAD: Component -> string = function
      // Base components
      | Cube ((x, y, z), center) -> $"cube(size = [{x}, {y}, {z}], center = {boolToString center})"
      | Sphere (r, d, fa, fs, fn) -> $"sphere(r = {r}, d = {d}, $fa = {fa}, $fs = {fs}, $fn = {fn}"
      | Cylinder (h, r1, r2, center) -> $"cylinder(h = {h}, r1 = {r1}, r2 = {r2}, center = {boolToString center})"
      | Polyhedron (points, faces, convexity) -> $"polyhedron(points = {ptsToString points}, faces = {ptsToString faces}, convexity = {convexity})"
      // Transformations of components
      | Scale ((x, y, z), target) -> $"scale(v = [{x}, {y}, {z}]) {toSCAD target}"
      | Resize ((x, y, z), target) -> $"resize(newSize = [{x}, {y}, {z}]) {toSCAD target}"
      | Rotate (a, (x, y, z), target) -> $"rotate(a = {a}, v = [{x}, {y}, {z}]) {toSCAD target}"
      | Translate ((x, y, z), target) -> $"translate(v = [{x}, {y}, {z}]) {toSCAD target}"
      | Mirror ((x, y, z), target) -> $"mirror(v = [{x}, {y}, {z}]) {toSCAD target}"
      | MultiMatrix (m, target) -> $"multimatrix(m = {multiMatrixToString m}) {toSCAD target}"
      | ColorRGBA ((r, g, b, a), target) -> $"color(c = [{r}, {g}, {b}, {a}]) {toSCAD target}"
      | ColorHex (c, target) -> $"color(c = {c}) {toSCAD target}"
      | ColorName (c, target) -> $"color(c = {c}) {toSCAD target}"
      | Offset (r, delta, chamfer, target) -> $"offset(r = {r}, delta = {delta}, chamfer = {boolToString chamfer}) {toSCAD target}"
      | Minkowski (target) -> $"minkowski() {toSCAD target}"
      | Hull (target) -> $"hull() {toSCAD target}"
      // Combinations of components
      | Union targets -> $"union() {{ {targets |> componentsToString toSCAD} }}"
      | Intersection targets -> $"intersection() {{ {targets |> componentsToString toSCAD} }}"
      | Difference (baseComponent, diffComponents) -> $"difference() {{ {baseComponent :: diffComponents |> componentsToString toSCAD} }}"
