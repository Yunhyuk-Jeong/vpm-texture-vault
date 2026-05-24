#!/usr/bin/env python3
import argparse
import json
import re
from copy import deepcopy
from pathlib import Path
from typing import Optional


PACKAGE_NAME = "com.studio-iyan.texture-vault"


def read_json(path: Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(data, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def validate_semver(version: str):
    if not re.match(r"^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$", version):
        raise ValueError(f"Version is not a semantic version string: {version}")


def normalize_pages_url(value: str) -> str:
    value = value.rstrip("/")
    if value.endswith("/index.json"):
        value = value[: -len("/index.json")]
    return value


def build_listing(package_json: dict, owner: str, repo: str, pages_url: str, zip_sha256: str, existing: Optional[dict]):
    version = package_json["version"]
    validate_semver(version)

    pages_url = normalize_pages_url(pages_url)
    release_zip_url = (
        f"https://github.com/{owner}/{repo}/releases/download/"
        f"v{version}/{PACKAGE_NAME}-{version}.zip"
    )

    listing = deepcopy(existing) if existing else {}
    listing.setdefault("name", "Studio Iyan VPM Repository")
    listing.setdefault("id", f"{owner}.{repo}")
    listing.setdefault("author", package_json.get("author", {}).get("name", "Studio Iyan"))
    listing["url"] = f"{pages_url}/index.json"
    listing.setdefault("packages", {})

    packages = listing["packages"]
    package_entry = packages.setdefault(PACKAGE_NAME, {})
    package_entry.setdefault("versions", {})

    manifest = deepcopy(package_json)
    manifest["url"] = release_zip_url
    manifest["zipSHA256"] = zip_sha256

    package_entry["versions"][version] = manifest
    return listing


def main():
    parser = argparse.ArgumentParser(description="Generate or update a VPM repository listing index.json.")
    parser.add_argument("--owner", required=True, help="GitHub owner or organization.")
    parser.add_argument("--repo", required=True, help="GitHub repository name.")
    parser.add_argument("--pages-url", required=True, help="GitHub Pages base URL or index.json URL.")
    parser.add_argument("--zip-sha256", required=True, help="SHA256 checksum for the release zip.")
    parser.add_argument("--output", required=True, help="Output index.json path.")
    args = parser.parse_args()

    script_dir = Path(__file__).resolve().parent
    repo_dir = script_dir.parent
    root_package_json = repo_dir / "package.json"
    nested_package_json = repo_dir / PACKAGE_NAME / "package.json"
    package_json_path = root_package_json if root_package_json.exists() else nested_package_json
    output_path = Path(args.output).resolve()

    if not package_json_path.exists():
        raise FileNotFoundError(f"package.json not found at {root_package_json} or {nested_package_json}")

    package_json = read_json(package_json_path)
    if package_json.get("name") != PACKAGE_NAME:
        raise ValueError(f"Unexpected package name: {package_json.get('name')}")

    existing = read_json(output_path) if output_path.exists() else None
    listing = build_listing(package_json, args.owner, args.repo, args.pages_url, args.zip_sha256, existing)
    write_json(output_path, listing)


if __name__ == "__main__":
    main()
