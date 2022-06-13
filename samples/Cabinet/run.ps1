# run the design project once to ensure the output is present
dotnet run
# start VSCode to edit the design project
code .
# start OpenSCAD to view the design render
Start-Process -FilePath "openscad" -ArgumentList "tmp/output.scad"
# re-run the design project any time a change is detected
dotnet watch
