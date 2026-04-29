import shutil
from pathlib import Path
from hatchling.builders.hooks.plugin.interface import BuildHookInterface


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
