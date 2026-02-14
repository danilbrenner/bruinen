# Bruinen.Host

Use LibMan to manage client-side libraries in `wwwroot`.

Install dotnet global tools:

In root of the repo:

```sh
dotnet tool restore
```

Initialize LibMan for this project:

```sh
cd src/Bruinen.Host

dotnet libman init
```

Example: install Bootstrap into `wwwroot/lib`:

```sh
dotnet libman install bootstrap@5.3.3 -p cdnjs -d wwwroot/lib/bootstrap
```

Restore libraries from `libman.json`:

```sh
dotnet libman restore
```

Example: Update libraries from `libman.json`:

```sh
dotnet libman update bootstrap
```