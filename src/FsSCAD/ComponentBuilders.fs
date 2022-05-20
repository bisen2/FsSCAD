namespace FsSCAD

/// helper functon for component creation
module ComponentBuilders =
  open Components

  [<AutoOpen>]
  module BaseComponents =
    /// A cube with the given side lengths with the (0,0,0) corner at the origin
    let rect sides = Cube(size = sides, center = false)
    /// A cube with the given side lengths centered at the origin
    let centerRect sides = Cube(size = sides, center = true)
    /// A cube with the given side length with the (0,0,0) corner at the origin
    let cube size = Cube(size = (size, size, size), center = false)
    /// A cube with the given side length centered at the origin
    let centerCube size = Cube(size = (size, size, size), center = true)
    // TODO: set up functions here for all other base components
    // TODO: provide functions for default values of shapes (i.e. Cube() -> Cube(size = (1,1,1), center = true))

  [<AutoOpen>]
  module Transfomations =
    let scale vector target = Scale(vector = vector, target = target)
    let resize newSize target = Resize(newSize = newSize, target = target)
    /// rotates the given angle around the given axis
    let rotate angle vector target = Rotate(angle = angle, vector = vector, target = target)
    /// rotates the given angle around the X axis
    let rotateX angle target = Rotate(angle = angle, vector = (1, 0, 0), target = target)
    /// rotates the given angle around the Y axis
    let rotateY angle target = Rotate(angle = angle, vector = (0, 1, 0), target = target)
    /// rotates the given angle around the Z axis
    let rotateZ angle target = Rotate(angle = angle, vector = (0, 0, 1), target = target)
    let translate vector target = Translate(vector = vector, target = target)
    let mirror vector target = Mirror(vector = vector, target = target)
    let multiMatrix mmatrix target = MultiMatrix(mmatrix = mmatrix, target = target)
    let colorRGBA color target = ColorRGBA(color = color, target = target)
    let colorHex color target = ColorHex(color = color, target = target)
    let colorName color target = ColorName(color = color, target = target)
    let offset r delta chamfer target = Offset(r = r, delta = delta, chamfer = chamfer, target = target)
