/// These tests compare the Component.toSCAD output to the expected strings.
/// Their main purpose is detecting breaking changes.
module SCAD_Generation

open FsCheck.Xunit
open FsSCAD.Components

let boolToString = function | true -> "true" | false -> "false"
let ptsToString =
  List.map (fun (x, y, z) -> $"[{x}, {y}, {z}]")
  >> fun strs -> System.String.Join(',', strs)
  >> sprintf "[ %s ]"
let multiMatrixToString (((x1,y1,z1),(x2,y2,z2),(x3,y3,z3)): MultiMatrix) =
  $"[ [{x1}, {y1}, {z1} ], [ {x2}, {y2}, {z2} ], [ {x3}, {y3}, {z3} ] ]"
let componentsToString (toSCADFunc: Component -> string) =
  List.map toSCADFunc >> List.toArray >> fun strs -> System.String.Join(';', strs) + ";"

module BaseComponents =

  [<Property>]
  let cube (x, y, z) c =
    Cube(size = (x, y, z), center = c)
    |> Component.toSCAD
    |> (=) $"cube(size = [{x}, {y}, {z}], center = {boolToString c})"

  [<Property>]
  let sphere r d fa fs fn =
    Sphere(radius = r, diameter = d, fragmentAngle = fa, fragmentSize = fs, resolution = fn)
    |> Component.toSCAD
    |> (=) $"sphere(r = {r}, d = {d}, $fa = {fa}, $fs = {fs}, $fn = {fn}"

  [<Property>]
  let cylinder h r1 r2 c =
    Cylinder(height = h, radius1 = r1, radius2 = r2, center = c)
    |> Component.toSCAD
    |> (=) $"cylinder(h = {h}, r1 = {r1}, r2 = {r2}, center = {boolToString c})"

  [<Property>]
  let polyhedron pts faces convexity =
    Polyhedron(points = pts, faces = faces, convexity = convexity)
    |> Component.toSCAD
    |> (=) $"polyhedron(points = {ptsToString pts}, faces = {ptsToString faces}, convexity = {convexity})"

module Transformations =

  [<Property>]
  let scale (x, y, z) t =
    Scale(vector = (x, y, z), target = t)
    |> Component.toSCAD
    |> (=) $"scale(v = [{x}, {y}, {z}]) {Component.toSCAD t}"

  [<Property>]
  let resize (x, y, z) t =
    Resize(newSize = (x, y, z), target = t)
    |> Component.toSCAD
    |> (=) $"resize(newSize = [{x}, {y}, {z}]) {Component.toSCAD t}"

  [<Property>]
  let rotate a (x, y, z) t =
    Rotate(angle = a, vector = (x, y, z), target = t)
    |> Component.toSCAD
    |> (=) $"rotate(a = {a}, v = [{x}, {y}, {z}]) {Component.toSCAD t}"

  [<Property>]
  let translate (x, y, z) t =
    Translate(vector = (x, y, z), target = t)
    |> Component.toSCAD
    |> (=) $"translate(v = [{x}, {y}, {z}]) {Component.toSCAD t}"

  [<Property>]
  let mirror (x, y, z) t =
    Mirror(vector = (x, y, z), target = t)
    |> Component.toSCAD
    |> (=) $"mirror(v = [{x}, {y}, {z}]) {Component.toSCAD t}"

  [<Property>]
  let multiMatrix m t =
    MultiMatrix(mmatrix = m, target = t)
    |> Component.toSCAD
    |> (=) $"multimatrix(m = {multiMatrixToString m}) {Component.toSCAD t}"

  [<Property>]
  let colorRGBA (r, g, b, a) t =
    ColorRGBA(color = (r, g, b, a), target = t)
    |> Component.toSCAD
    |> (=) $"color(c = [{r}, {g}, {b}, {a}]) {Component.toSCAD t}"

  [<Property>]
  let colorHex c t =
    ColorHex(color = c, target = t)
    |> Component.toSCAD
    |> (=) $"color(c = {c}) {Component.toSCAD t}"

  [<Property>]
  let colorName c t =
    ColorName(color = c, target = t)
    |> Component.toSCAD
    |> (=) $"color(c = {c}) {Component.toSCAD t}"

  [<Property>]
  let offset r d c t =
    Offset(r = r, delta = d, chamfer = c, target = t)
    |> Component.toSCAD
    |> (=) $"offset(r = {r}, delta = {d}, chamfer = {boolToString c}) {Component.toSCAD t}"

  [<Property>]
  let minkowski t =
    Minkowski(target = t)
    |> Component.toSCAD
    |> (=) $"minkowski() {Component.toSCAD t}"

  [<Property>]
  let hull t =
    Hull(target = t)
    |> Component.toSCAD
    |> (=) $"hull() {Component.toSCAD t}"

module Combinations =

  [<Property>]
  let union ts =
    Union(targets = ts)
    |> Component.toSCAD
    |> (=) $"union() {{ {ts |> componentsToString Component.toSCAD} }}"

  [<Property>]
  let intersection ts =
    Intersection(targets = ts)
    |> Component.toSCAD
    |> (=) $"intersection() {{ {ts |> componentsToString Component.toSCAD} }}"

  [<Property>]
  let difference bc dcs =
    Difference(baseComponent = bc, diffComponents = dcs)
    |> Component.toSCAD
    |> (=) $"difference() {{ {bc :: dcs |> componentsToString Component.toSCAD} }}"
