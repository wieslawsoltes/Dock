#!/usr/bin/env python3
"""Build the pinned AvalonDock framework dependency with Avalonia's packaging tools."""
import argparse
import hashlib
import json
import shutil
import subprocess
import sys
import xml.etree.ElementTree as xml
from pathlib import Path


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-tree", type=Path, help="Reuse a clean checkout at the pinned commit.")
    parser.add_argument("--output", type=Path, help="Package feed output directory.")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    specification = Path(__file__).with_name("framework.json")
    config = json.loads(specification.read_text())
    output = (args.output or root / "artifacts/avalondock-framework/nuget").resolve()
    manifest = output / "manifest.json"
    fingerprint = hashlib.sha256(specification.read_bytes() + Path(__file__).read_bytes()).hexdigest()
    if manifest.exists():
        previous = json.loads(manifest.read_text())
        if previous.get("specification") == fingerprint and all(
            (output / name).is_file() and sha256(output / name) == digest
            for name, digest in previous.get("packages", {}).items()
        ) and len(previous.get("packages", {})) == 4:
            print(f"Pinned framework packages verified: {output}")
            return
    checkout = (args.source_tree or output.parent / "source").resolve()

    def run(*command):
        print("Running: " + " ".join(map(str, command)), flush=True)
        subprocess.run([str(value) for value in command], cwd=checkout, check=True)

    if not (checkout / ".git").exists():
        checkout.mkdir(parents=True, exist_ok=True)
        run("git", "init")
        run("git", "remote", "add", "origin", config["repository"])
    current = subprocess.run(["git", "rev-parse", "HEAD"], cwd=checkout, text=True,
                             capture_output=True, check=False).stdout.strip()
    if not current.startswith(config["commit"]):
        if args.source_tree:
            raise RuntimeError(f"The supplied checkout is {current}, expected {config['commit']}")
        run("git", "fetch", "--depth=1", "origin", config["commit"])
        run("git", "checkout", "--detach", "FETCH_HEAD")
    changes = subprocess.check_output(["git", "status", "--porcelain", "--untracked-files=normal"], cwd=checkout, text=True).strip()
    if changes:
        raise RuntimeError("The framework checkout has uncommitted files; commit or remove them and update framework.json before packaging.")
    run("git", "submodule", "update", "--init", "--recursive", "--depth=1")
    version = config["packageVersion"]
    project = checkout / "avalondock-compatibility-pack.proj"
    merge_config = checkout / "nukebuild/numerge.json"
    original_merge = merge_config.read_bytes()
    original_project = project.read_bytes() if project.exists() else None
    try:
        intermediate = checkout / "build-intermediate/nuget"
        if intermediate.exists():
            shutil.rmtree(intermediate)
        traversal = xml.Element("Project", Sdk="Microsoft.Build.Traversal")
        references = xml.SubElement(traversal, "ItemGroup")
        projects = ["src/Avalonia.Build.Tasks/Avalonia.Build.Tasks.csproj",
                    "src/Avalonia.Remote.Protocol/Avalonia.Remote.Protocol.csproj",
                    "packages/Avalonia/Avalonia.csproj"]
        for reference in xml.parse(checkout / "build/CoreLibraries.props").findall(".//ProjectReference"):
            projects.append(reference.attrib["Include"].replace("$(MSBuildThisFileDirectory)/../", ""))
        for name in ["Avalonia.Generators", "Avalonia.Analyzers.CSharp", "Avalonia.Analyzers.VisualBasic", "Avalonia.Analyzers.CodeFixes.CSharp"]:
            projects.append(f"src/tools/{name}/{name}.csproj")
        for name in projects:
            xml.SubElement(references, "ProjectReference", Include=name)
        project.write_text(xml.tostring(traversal, encoding="unicode"))
        run("dotnet", "restore", "src/tools/Avalonia.Designer.HostApp/Avalonia.Designer.HostApp.csproj", "--nologo")
        run("dotnet", "pack", project, "-c", "Release", f"-p:PackageVersion={version}", "--nologo")
        config_core = json.loads(original_merge)
        config_core["Packages"] = [item for item in config_core["Packages"] if item["Id"] == "Avalonia"]
        merge_config.write_text(json.dumps(config_core))
        # Use upstream's IL repacking, NuGet dependency merge, signing, and reference-assembly generation.
        run("dotnet", "run", "--project", "nukebuild/_build.csproj", "--", "--target", "CreateNugetPackages",
            "--skip", "--force-nuget-version", version)
        built_commit = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=checkout, text=True).strip()
        changed_files = set(subprocess.check_output(["git", "diff", "HEAD", "--name-only"], cwd=checkout, text=True).splitlines())
        changed_files.update(subprocess.check_output(["git", "ls-files", "--others", "--exclude-standard"], cwd=checkout, text=True).splitlines())
        changed_files.difference_update({"nukebuild/numerge.json", project.name})
        if built_commit != config["commit"] or changed_files:
            raise RuntimeError("The framework source changed during packaging; discard this build and rebuild from a clean pinned revision.")
        output.mkdir(parents=True, exist_ok=True)
        packages = {}
        for name in ["Avalonia", "Avalonia.Remote.Protocol"]:
            for extension in ["nupkg", "snupkg"]:
                filename = f"{name}.{version}.{extension}"
                source = checkout / "artifacts/nuget" / filename
                if source.resolve() != (output / filename).resolve():
                    shutil.copy2(source, output / filename)
                packages[filename] = sha256(output / filename)
        manifest.write_text(json.dumps({"specification": fingerprint,
                                       "commit": built_commit,
                                       "packages": packages}, indent=2) + "\n")
        print(f"Pinned framework packages built: {output}")
    finally:
        merge_config.write_bytes(original_merge)
        if original_project is None:
            project.unlink(missing_ok=True)
        else:
            project.write_bytes(original_project)


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"Framework bootstrap failed: {error}", file=sys.stderr)
        raise SystemExit(1)
