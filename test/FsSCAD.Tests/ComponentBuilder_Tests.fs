/// These properties mostly just mimic the ComponentBuilders implementations.
/// Their main purpose is detecting breaking changes.
module ComponentBuilder_Tests

open System
open FsCheck
open FsCheck.Xunit
open FsSCAD.Components
open FsSCAD.ComponentBuilders

module Utils =

  // The default integer generator does not regularly create large enough numbers to fall outside of the
  // hex color range, so we need to add a custom generator for this test to verify we handle out of ranges

  type BigInts =
    static member Int() =
      Gen.choose(0, Int32.MaxValue)
      |> Arb.fromGen

  [<Property(Arbitrary=[| typeof<BigInts> |])>]
  let HexColor value =
    if value > 0xffffff || value < 0 then
      Prop.throws<ArgumentOutOfRangeException, _>(lazy (HexColor value))
    else
      Prop.ofTestable (HexColor value |> function | HexColor v -> v = value)

module BaseComponents =

  [<Property>]
  let rect sides =
    rect sides = Cube(size = sides, center = false)

  [<Property>]
  let centerRect sides =
    centerRect sides = Cube(size = sides, center = true)

  [<Property>]
  let cube size =
    cube size = Cube(size = (size, size, size), center = false)

  [<Property>]
  let centerCube size =
    centerCube size = Cube(size = (size, size, size), center = true)

  [<Property>]
  let sphere radius =
    sphere radius = Sphere(radius = radius, diameter = 2. * radius, fragmentAngle = 12, fragmentSize = 2, resolution = 0)

  [<Property>]
  let cone radius height =
    cone radius height = Cylinder(height = height, radius1 = 0, radius2 = radius, center = false)

  [<Property>]
  let centerCone radius height =
    centerCone radius height = Cylinder(height = height, radius1 = 0, radius2 = radius, center = true)

  [<Property>]
  let cylinder radius height =
    cylinder radius height = Cylinder(height = height, radius1 = radius, radius2 = radius, center = false)

  [<Property>]
  let centerCylinder radius height =
    centerCylinder radius height = Cylinder(height = height, radius1 = radius, radius2 = radius, center = true)

  [<Property>]
  let polyhedron points faces convexity =
    polyhedron points faces convexity = Polyhedron(points = points, faces = faces, convexity = convexity)

module Transfomations =

      [<Property>]
      let scale vector target =
        scale vector target = Scale(vector = vector, target = target)

      [<Property>]
      let scaleX factor target =
        scaleX factor target = Scale(vector = (factor, 1, 1), target = target)

      [<Property>]
      let scaleY factor target =
        scaleY factor target = Scale(vector = (1, factor, 1), target = target)

      [<Property>]
      let scaleZ factor target =
        scaleZ factor target = Scale(vector = (1, 1, factor), target = target)

      [<Property>]
      let resize newSize target =
        resize newSize target = Resize(newSize = newSize, target = target)

      [<Property>]
      let rotate angle vector target =
        rotate angle vector target = Rotate(angle = angle, vector = vector, target =
        target)
      [<Property>]
      let rotateX angle target =
        rotateX angle target = Rotate(angle = angle, vector = (1, 0, 0), target = target)

      [<Property>]
      let rotateY angle target =
        rotateY angle target = Rotate(angle = angle, vector = (0, 1, 0), target = target)

      [<Property>]
      let rotateZ angle target =
        rotateZ angle target = Rotate(angle = angle, vector = (0, 0, 1), target = target)

      [<Property>]
      let translate vector target =
        translate vector target = Translate(vector = vector, target = target)

      [<Property>]
      let translateX distance target =
        translateX distance target = Translate(vector = (distance, 0, 0), target =
        target)
      [<Property>]
      let translateY distance target =
        translateY distance target = Translate(vector = (0, distance, 0), target =
        target)
      [<Property>]
      let translateZ distance target =
        translateZ distance target = Translate(vector = (0, 0, distance), target =
        target)
      [<Property>]
      let mirror vector target =
        mirror vector target = Mirror(vector = vector, target = target)

      [<Property>]
      let multiMatrix mmatrix target =
        multiMatrix mmatrix target = MultiMatrix(mmatrix = mmatrix, target = target)

      [<Property>]
      let colorRGBA color target =
        colorRGBA color target = ColorRGBA(color = color, target = target)

      [<Property>]
      let colorHex color target =
        colorHex color target = ColorHex(color = color, target = target)

      [<Property>]
      let colorName color target =
        colorName color target = ColorName(color = color, target = target)

      [<Property>]
      let offset r delta chamfer target =
        offset r delta chamfer target = Offset(r = r, delta = delta, chamfer = chamfer,
        target = target)
