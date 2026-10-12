"""Audit CBM content hashes without changing its cache or repository files."""

import argparse
from contextlib import closing
import hashlib
import json
from pathlib import Path
import sqlite3
import sys


def audit(repo, database, project, paths):
    with closing(sqlite3.connect(database.resolve().as_uri() + "?mode=ro", uri=True)) as connection:
        connection.execute("PRAGMA query_only = ON")
        rows = connection.execute(
            "SELECT rel_path, sha256, size, mtime_ns FROM file_hashes WHERE project = ?",
            (project,),
        ).fetchall()
    if not rows:
        raise ValueError(f"No hash records for project: {project}")

    records = {row[0]: row[1:] for row in rows}
    selected = sorted(set(paths) if paths else records)
    current = []
    stale = []
    missing = []
    unindexed = []
    virtual = []
    for relative in selected:
        if relative.startswith(".codebase-memory/.semantic-input/"):
            virtual.append(relative)
            continue
        source = (repo / relative).resolve()
        if not source.is_relative_to(repo.resolve()):
            raise ValueError(f"Path is outside repository: {relative}")
        if not source.is_file():
            missing.append(relative)
            continue
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        stat = source.stat()
        entry = {"path": relative, "sha256": digest, "size": stat.st_size}
        if relative not in records:
            unindexed.append(entry)
            continue
        expected_hash, expected_size, expected_mtime = records[relative]
        entry.update({
            "hash_matches": digest == expected_hash,
            "size_matches": stat.st_size == expected_size,
            "mtime_ns_matches": stat.st_mtime_ns == expected_mtime,
            "windows_mtime_matches": int(stat.st_mtime) * 1_000_000_000 == expected_mtime,
        })
        (current if entry["hash_matches"] and entry["size_matches"] else stale).append(entry)

    return {
        "project": project,
        "repository": str(repo.resolve()),
        "database": str(database.resolve()),
        "selected": len(selected),
        "content_current_count": len(current),
        "stale_count": len(stale),
        "missing_count": len(missing),
        "not_indexed_count": len(unindexed),
        "virtual_count": len(virtual),
        "metadata_only_count": sum(not row["mtime_ns_matches"] for row in current),
        "windows_timestamp_difference_count": sum(not row["windows_mtime_matches"] for row in current),
        "content_current": current,
        "stale": stale,
        "missing": missing,
        "not_indexed": unindexed,
        "virtual": virtual,
        "note": "Matching hashes establish content freshness only. Unindexed files need direct reads; parsing and graph completeness remain separate checks.",
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, default=Path.cwd())
    parser.add_argument("--project", required=True)
    parser.add_argument("--database", type=Path)
    parser.add_argument("--path", action="append", default=[])
    args = parser.parse_args()
    database = args.database or (
        Path.home() / ".cache/codebase-memory-mcp" / f"{args.project}.db"
    )
    try:
        result = audit(args.repo, database, args.project, args.path)
    except (OSError, sqlite3.Error, ValueError) as error:
        print(f"CBM audit failed: {error}", file=sys.stderr)
        return 2
    print(json.dumps(result, indent=2))
    return 1 if result["stale_count"] or result["missing_count"] else 0


if __name__ == "__main__":
    sys.exit(main())
