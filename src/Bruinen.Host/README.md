# Bruinen.Host

Use LibMan to manage client-side libraries in `wwwroot`.

Example: install Bootstrap into `wwwroot/lib` with specific files:

```sh
dotnet libman install bootstrap@5.3.3 -p cdnjs -d wwwroot/lib/bootstrap --files css/bootstrap.min.css --files js/bootstrap.bundle.min.js
```