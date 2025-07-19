# Layout generator

`LayoutGenerator` converts a simple configuration file into a Dock layout at runtime. The configuration can be written in JSON or YAML and follows the schema defined in `layout-schema.json`.

## Configuration format

A file describes the orientation of the main dock, a list of documents and optional tools. Each tool specifies an alignment and the proportion it occupies.

Example `layout.json`:

```json
{
  "orientation": "Horizontal",
  "documents": [
    { "id": "Doc1", "title": "Document 1" }
  ],
  "tools": [
    { "id": "Output", "title": "Output", "alignment": "Bottom", "proportion": 0.25 }
  ]
}
```

## Using the generator

1. Build the `LayoutGeneratorTool` project.
2. Run the tool and pass a path to a JSON or YAML file:

```bash
dotnet run --project samples/LayoutGeneratorTool layout.json
```

The application loads the file, builds the layout and displays a demo window.
