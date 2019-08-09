workflow "New workflow" {
  on = "push"
  resolves = ["Setup Dotnet for use with actions"]
}

action "Setup Dotnet for use with actions" {
  uses = "actions/setup-dotnet@v1"
  runs = "dotnet build"
}
