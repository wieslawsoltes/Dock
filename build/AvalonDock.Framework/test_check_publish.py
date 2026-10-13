import io
import json
import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import Mock, patch
from urllib.error import HTTPError
from urllib.request import Request

import check_publish


class Response(io.BytesIO):
    status = 200


class PublishGuardTests(unittest.TestCase):
    feed = "https://feed.example/v3/index.json"
    version = "12.1.4-avalondock.2"

    def opener(self, content_host="feed.example"):
        service = {"resources": [{"@type": "PackageBaseAddress/3.0.0",
                                  "@id": f"https://{content_host}/content/"}]}
        return Mock(open=Mock(side_effect=[Response(json.dumps(service).encode()), Response(), Response()]))

    def test_requires_both_exact_versions_using_read_only_requests(self):
        opener = self.opener()
        self.assertTrue(check_publish.packages_available(self.feed, self.version, opener=opener))
        requests = [call.args[0] for call in opener.open.call_args_list]
        self.assertEqual(["GET", "HEAD", "HEAD"], [request.method for request in requests])
        self.assertEqual(f"https://feed.example/content/avalonia/{self.version}/avalonia.{self.version}.nupkg", requests[1].full_url)
        self.assertEqual(f"https://feed.example/content/avalonia.remote.protocol/{self.version}/avalonia.remote.protocol.{self.version}.nupkg", requests[2].full_url)

    def test_credentials_are_sent_only_to_the_destination_origin(self):
        opener = self.opener("cdn.example")
        check_publish.packages_available(self.feed, self.version, "user", "token", opener)
        requests = [call.args[0] for call in opener.open.call_args_list]
        self.assertIsNotNone(requests[0].get_header("Authorization"))
        self.assertIsNone(requests[1].get_header("Authorization"))
        self.assertIsNone(requests[2].get_header("Authorization"))

    def test_redirects_strip_credentials_on_host_or_scheme_change(self):
        original = Request(self.feed, headers={"Authorization": "Basic secret"})
        handler = check_publish.SameOriginCredentials()
        for destination in ("https://cdn.example/file", "http://feed.example/file"):
            redirected = handler.redirect_request(original, None, 302, "Found", {}, destination)
            self.assertIsNone(redirected.get_header("Authorization"))

    def test_missing_either_package_is_not_accepted(self):
        for failure_index in (1, 2):
            opener = self.opener()
            results = list(opener.open.side_effect)
            error = HTTPError(self.feed, 404, "Missing", {}, None)
            self.addCleanup(error.close)
            results[failure_index] = error
            opener.open.side_effect = results
            with self.assertRaises(HTTPError):
                check_publish.packages_available(self.feed, self.version, opener=opener)

    def test_unverified_feed_skips_publish_and_reports_bundle(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "output"
            summary = Path(directory) / "summary"
            with patch.dict(os.environ, {"GITHUB_OUTPUT": str(output), "GITHUB_STEP_SUMMARY": str(summary)}), \
                 patch("sys.argv", ["check_publish.py", "--feed", self.feed]), \
                 patch.object(check_publish, "packages_available", side_effect=OSError("unavailable")), \
                 patch("sys.stdout", new_callable=io.StringIO):
                check_publish.main()
            self.assertEqual("available=false\n", output.read_text())
            self.assertIn("publishing skipped", summary.read_text())
            self.assertIn("package bundle", summary.read_text())

    def test_plain_http_is_rejected(self):
        with self.assertRaises(ValueError):
            check_publish.packages_available("http://feed.example/index.json", self.version)


if __name__ == "__main__":
    unittest.main()
