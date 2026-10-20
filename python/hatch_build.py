import shutil
from pathlib import Path
from hatchling.builders.hooks.plugin.interface import BuildHookInterface
from hatchling.metadata.plugin.interface import MetadataHookInterface


class CustomMetadataHook(MetadataHookInterface):
    # Recent hatchling rejects `readme = "../README.md"` (outside the project dir), so the
    # README is read here instead: the copy in python/ (present in an sdist) or the repo one.
    def update(self, metadata):
        root = Path(self.root)
        for candidate in (root / "README.md", root.parent / "README.md"):
            if candidate.exists():
                metadata["readme"] = {
                    "content-type": "text/markdown",
                    "text": candidate.read_text(encoding="utf-8"),
                }
                return


class CustomBuildHook(BuildHookInterface):
    _FILES = ("README.md", "LICENSE")

    def initialize(self, version, build_data):
        root = Path(self.root)
        for name in self._FILES:
            local = root / name
            upstream = root.parent / name
            if upstream.exists() and not local.exists():
                shutil.copy2(upstream, local)
            if local.exists():
                build_data["force_include"][str(local)] = name
