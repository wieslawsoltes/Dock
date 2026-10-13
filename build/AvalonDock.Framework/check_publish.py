#!/usr/bin/env python3
"""Allow Dock feed publishing only when its pinned framework packages already exist."""
import argparse
import base64
import json
import os
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener


class SameOriginCredentials(HTTPRedirectHandler):
    """Keep feed credentials off redirects to a different package download host."""

    def redirect_request(self, request, fp, code, message, headers, new_url):
        redirected = super().redirect_request(request, fp, code, message, headers, new_url)
        origin = urlsplit(request.full_url)
        destination = urlsplit(new_url)
        if redirected and (origin.scheme, origin.netloc) != (destination.scheme, destination.netloc):
            redirected.remove_header("Authorization")
        return redirected


def packages_available(feed, version, username="", token="", opener=None):
    if urlsplit(feed).scheme != "https":
        raise ValueError("The publishing feed must use HTTPS.")
    opener = opener or build_opener(SameOriginCredentials())

    def request(url, method="GET"):
        headers = {}
        if token and urlsplit(url).netloc == urlsplit(feed).netloc:
            credentials = base64.b64encode(f"{username}:{token}".encode()).decode()
            headers["Authorization"] = "Basic " + credentials
        return opener.open(Request(url, headers=headers, method=method), timeout=30)

    with request(feed) as response:
        index = json.load(response)
    base = next((resource["@id"] for resource in index["resources"]
                 if resource.get("@type") == "PackageBaseAddress/3.0.0"), None)
    if not base or urlsplit(base).scheme != "https":
        raise ValueError("The feed has no HTTPS package-content endpoint.")
    version = version.lower()
    for package in ("avalonia", "avalonia.remote.protocol"):
        url = f"{base.rstrip('/')}/{package}/{version}/{package}.{version}.nupkg"
        with request(url, "HEAD") as response:
            if response.status != 200:
                return False
    return True


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--feed", required=True, help="Destination NuGet v3 service index.")
    args = parser.parse_args()
    specification = json.loads(Path(__file__).with_name("framework.json").read_text())
    version = specification["packageVersion"]
    try:
        available = packages_available(args.feed, version, os.environ.get("NUGET_FEED_USERNAME", ""),
                                       os.environ.get("NUGET_FEED_TOKEN", ""))
    except (OSError, URLError, ValueError, KeyError) as error:
        available = False
        if isinstance(error, HTTPError):
            error.close()
        print(f"Framework availability could not be verified ({type(error).__name__}).")
    message = (f"Pinned framework {version} is available; Dock feed publishing is enabled." if available else
               f"Dock feed publishing skipped: Avalonia and Avalonia.Remote.Protocol {version} "
               "must already exist at the destination. Download the package bundle containing both feeds.")
    print(message if available else "::warning::" + message)
    if output := os.environ.get("GITHUB_OUTPUT"):
        with open(output, "a", encoding="utf-8") as stream:
            stream.write(f"available={str(available).lower()}\n")
    if summary := os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(summary, "a", encoding="utf-8") as stream:
            stream.write(message + "\n")


if __name__ == "__main__":
    main()
