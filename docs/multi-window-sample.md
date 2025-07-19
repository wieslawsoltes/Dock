# MultiWindowSample

This sample demonstrates how to open multiple top‑level windows using Dock. It follows the steps from the complex layouts guide to float dockables into separate windows and persist the resulting layout.

Run the application with:

```bash
dotnet run --project samples/MultiWindowSample
```

On first start the sample creates two windows. Moving or closing them then exiting writes `layout.json`. The file is loaded on the next run so the previous window arrangement reappears.
