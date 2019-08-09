workflow "New workflow" {
  on = "push"
  resolves = ["Setup Dotnet for use with actions"]
}

action "Setup Dotnet for use with actions" {
  uses = "actions/setup-dotnet@d6004ce18bdb4641fec8d84c683b2adb850c3dd5"
}
