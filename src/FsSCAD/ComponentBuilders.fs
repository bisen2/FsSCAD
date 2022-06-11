namespace FsSCAD

/// Helper functons for component creation
module ComponentBuilders =
  open Components

  /// Helper functions for creating base components.
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
    /// A sphere with the given radius
    let sphere radius = Sphere(radius = radius, diameter = 2. * radius, fragmentAngle = 12, fragmentSize = 2, resolution = 0)
    /// A cone with the given base radius and height
    let cone radius height = Cylinder(height = height, radius1 = 0, radius2 = radius, center = false)
    /// A cylinder with the given base radius and height centered on the origin
    let centerCone radius height = Cylinder(height = height, radius1 = 0, radius2 = radius, center = true)
    /// A cylinder with the given radius and height
    let cylinder radius height = Cylinder(height = height, radius1 = radius, radius2 = radius, center = false)
    /// A cylinder with the given radius and height centered on the origin
    let centerCylinder radius height = Cylinder(height = height, radius1 = radius, radius2 = radius, center = true)
    /// A polyhedron built from the given points and faces
    let polyhedron points faces convexity = Polyhedron(points = points, faces = faces, convexity = convexity)

  /// Helper functions for applying transformations to components.
  [<AutoOpen>]
  module Transfomations =
    /// scales based on the given vector
    let scale vector target = Scale(vector = vector, target = target)
    /// scales by the given factor along the X axis
    let scaleX factor target = Scale(vector = (factor, 1, 1), target = target)
    /// scales by the given factor along the Y axis
    let scaleY factor target = Scale(vector = (1, factor, 1), target = target)
    /// scales by the given factor along the Z axis
    let scaleZ factor target = Scale(vector = (1, 1, factor), target = target)
    /// resizes to the given dimensions
    let resize newSize target = Resize(newSize = newSize, target = target)
    /// rotates the given angle around the given axis
    let rotate angle vector target = Rotate(angle = angle, vector = vector, target = target)
    /// rotates the given angle around the X axis
    let rotateX angle target = Rotate(angle = angle, vector = (1, 0, 0), target = target)
    /// rotates the given angle around the Y axis
    let rotateY angle target = Rotate(angle = angle, vector = (0, 1, 0), target = target)
    /// rotates the given angle around the Z axis
    let rotateZ angle target = Rotate(angle = angle, vector = (0, 0, 1), target = target)
    /// translates by the given vector
    let translate vector target = Translate(vector = vector, target = target)
    /// translates by the given amount along the X axis
    let translateX distance target = Translate(vector = (distance, 0, 0), target = target)
    /// translates by the given amount along the Y axis
    let translateY distance target = Translate(vector = (0, distance, 0), target = target)
    /// translates by the given amount along the Z axis
    let translateZ distance target = Translate(vector = (0, 0, distance), target = target)
    /// Mirrors across the given vector
    let mirror vector target = Mirror(vector = vector, target = target)
    let multiMatrix mmatrix target = MultiMatrix(mmatrix = mmatrix, target = target)
    /// sets the color based on the given RGBA values
    let colorRGBA color target = ColorRGBA(color = color, target = target)
    /// sets the color based on the given hex value
    let colorHex color target = ColorHex(color = color, target = target)
    /// sets the color based on the given color name \
    /// for allowed color names, see https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Transformations#color
    let colorName color target = ColorName(color = color, target = target)
    let offset r delta chamfer target = Offset(r = r, delta = delta, chamfer = chamfer, target = target)
