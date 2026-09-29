"""Extract candidate fold axes from a Rhino-exported STEP file.

The script uses STEP topology (CLOSED_SHELL -> EDGE_CURVE -> VERTEX_POINT)
instead of relying on display mesh data.  It writes a reviewable JSON file
with the best matching boundary edge for each requested pair of Adams parts.
"""

from __future__ import annotations

import argparse
import json
import math
import re
from pathlib import Path


PART_RING = [16, 17, 14, 15, 12, 13, 10, 11, 8, 9, 6, 7, 4, 5, 2, 3]
REF = re.compile(r"#(\d+)")
ENTITY = re.compile(r"#(\d+)\s*=\s*(.*)", re.DOTALL)
COORDS = re.compile(r"CARTESIAN_POINT\s*\(\s*''\s*,\s*\(([^)]*)\)", re.I)


def read_entities(path: Path) -> dict[int, str]:
    text = path.read_text(encoding="utf-8", errors="replace")
    data = text[text.find("DATA;") + 5 : text.find("ENDSEC;", text.find("DATA;"))]
    entities: dict[int, str] = {}
    for statement in data.split(";"):
        match = ENTITY.match(statement.strip())
        if match:
            entities[int(match.group(1))] = match.group(2).strip()
    return entities


def refs(entity: str) -> list[int]:
    return [int(value) for value in REF.findall(entity)]


def point_for_vertex(entities: dict[int, str], vertex_id: int) -> tuple[float, float, float]:
    vertex = entities[vertex_id]
    point_id = refs(vertex)[0]
    point = entities[point_id]
    match = COORDS.search(point)
    if not match:
        raise ValueError(f"Vertex #{vertex_id} does not refer to a Cartesian point")
    return tuple(float(value) for value in match.group(1).split(","))  # type: ignore[return-value]


def edge_data_for_shell(entities: dict[int, str], shell_id: int) -> list[tuple[tuple[float, float, float], tuple[float, float, float]]]:
    """Walk a shell's face/loop topology and collect unique topological edges."""
    pending = [shell_id]
    seen: set[int] = set()
    edge_ids: set[int] = set()
    while pending:
        entity_id = pending.pop()
        if entity_id in seen:
            continue
        seen.add(entity_id)
        entity = entities.get(entity_id, "")
        if entity.startswith("EDGE_CURVE"):
            edge_ids.add(entity_id)
            continue
        # Do not descend through a geometric curve/surface: topology has
        # already given us the endpoint vertices we need.
        if any(entity.startswith(kind) for kind in ("B_SPLINE", "LINE", "CIRCLE", "PLANE", "DIRECTION", "CARTESIAN_POINT")):
            continue
        pending.extend(refs(entity))

    edges = []
    for edge_id in sorted(edge_ids):
        edge = refs(entities[edge_id])
        if len(edge) < 2:
            continue
        edges.append((point_for_vertex(entities, edge[0]), point_for_vertex(entities, edge[1])))
    return edges


def subtract(a: tuple[float, float, float], b: tuple[float, float, float]) -> tuple[float, float, float]:
    return tuple(x - y for x, y in zip(a, b))  # type: ignore[return-value]


def add(a: tuple[float, float, float], b: tuple[float, float, float]) -> tuple[float, float, float]:
    return tuple(x + y for x, y in zip(a, b))  # type: ignore[return-value]


def scale(a: tuple[float, float, float], factor: float) -> tuple[float, float, float]:
    return tuple(value * factor for value in a)  # type: ignore[return-value]


def dot(a: tuple[float, float, float], b: tuple[float, float, float]) -> float:
    return sum(x * y for x, y in zip(a, b))


def length(a: tuple[float, float, float]) -> float:
    return math.sqrt(dot(a, a))


def distance(a: tuple[float, float, float], b: tuple[float, float, float]) -> float:
    return length(subtract(a, b))


def normalized(a: tuple[float, float, float]) -> tuple[float, float, float]:
    magnitude = length(a)
    if magnitude < 1e-12:
        raise ValueError("Zero-length edge cannot define a hinge axis")
    return scale(a, 1 / magnitude)


def best_pair(a_edges, b_edges):
    """Find closest, equally long, parallel edge pair; return its centre axis."""
    candidates = []
    for a0, a1 in a_edges:
        va = subtract(a1, a0)
        la = length(va)
        if la < 1e-9:
            continue
        ua = normalized(va)
        for b0, b1 in b_edges:
            vb = subtract(b1, b0)
            lb = length(vb)
            if lb < 1e-9:
                continue
            ub = normalized(vb)
            parallel_error = 1 - abs(dot(ua, ub))
            same = (distance(a0, b0) + distance(a1, b1)) / 2
            reversed_ = (distance(a0, b1) + distance(a1, b0)) / 2
            endpoint_error = min(same, reversed_)
            length_error = abs(la - lb)
            # The first component favours common/near-coincident edges.
            score = endpoint_error + 2 * length_error + 100 * parallel_error
            candidates.append((score, endpoint_error, length_error, parallel_error, a0, a1, b0, b1))
    if not candidates:
        raise ValueError("No usable boundary edges")
    score, endpoint_error, length_error, parallel_error, a0, a1, b0, b1 = min(candidates, key=lambda item: item[0])
    # Pair endpoints consistently before averaging the two physical edges.
    if distance(a0, b1) + distance(a1, b0) < distance(a0, b0) + distance(a1, b1):
        b0, b1 = b1, b0
    start = scale(add(a0, b0), 0.5)
    end = scale(add(a1, b1), 0.5)
    axis = normalized(subtract(end, start))
    centre = scale(add(start, end), 0.5)
    return {
        "edge_on_first_part": [a0, a1],
        "edge_on_second_part": [b0, b1],
        "axis_start": start,
        "axis_end": end,
        "marker_location": centre,
        "marker_z_axis": axis,
        "edge_match_error": {
            "endpoint_distance": endpoint_error,
            "length_difference": length_error,
            "non_parallel_factor": parallel_error,
        },
    }


def euler_313_for_z_axis(z_axis: tuple[float, float, float]) -> tuple[float, float, float]:
    """Return Adams' body-fixed 3-1-3 angles (degrees), choosing gamma = 0.

    Adams/View marker orientation is conventionally expressed as body-fixed
    3-1-3 Euler angles.  With the third angle zero, the resulting marker Z
    axis is exactly the requested hinge direction; X/Y are chosen consistently
    but otherwise have no effect on a revolute joint.
    """
    x, y, z = normalized(z_axis)
    beta = math.acos(max(-1.0, min(1.0, z)))
    if abs(math.sin(beta)) < 1e-12:
        alpha = 0.0
    else:
        alpha = math.atan2(x, -y)
    return tuple(math.degrees(angle) for angle in (alpha, beta, 0.0))  # type: ignore[return-value]


def command_marker(name: str, owner: str, location, orientation) -> str:
    loc = ", ".join(f"{value:.12g}" for value in location)
    ori = ", ".join(f"{value:.12g}" for value in orientation)
    return (
        f"marker create marker_name = .MODEL_9299.{owner}.{name} &\n"
        f"    location = {loc} &\n"
        f"    orientation = {ori} &\n"
        "    relative_to = .MODEL_9299.GROUND\n"
    )


def write_adams_command(path: Path, hinges: list[dict]) -> None:
    """Write a command file for a clean Adams model created from this STEP."""
    lines = [
        "! Generated from 929.stp by extract_adams_hinges.py.",
        "! Import into a CLEAN .MODEL_9299 after importing the STEP geometry.",
        "! Existing manually created joints must be removed first, otherwise they",
        "! will duplicate these constraints and produce an overconstrained model.",
        "! Marker orientations use Adams body-fixed 3-1-3 Euler angles in degrees.",
        "",
    ]
    for hinge in hinges:
        first = hinge["first_part"]
        second = hinge["second_part"]
        suffix = f"{first[4:]}_{second[4:]}"
        orientation = euler_313_for_z_axis(tuple(hinge["marker_z_axis"]))
        location = hinge["marker_location"]
        lines.append(command_marker(f"MK_{suffix}_I", first, location, orientation))
        lines.append(command_marker(f"MK_{suffix}_J", second, location, orientation))
        lines.extend(
            [
                f"constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_{suffix} &",
                f"    i_marker_name = .MODEL_9299.{first}.MK_{suffix}_I &",
                f"    j_marker_name = .MODEL_9299.{second}.MK_{suffix}_J",
                "",
            ]
        )

    # Fix the first part at the centre of its first generated hinge.  This is
    # a valid body-fixed reference marker because it is owned by PART16.
    first_hinge = hinges[0]
    orientation = euler_313_for_z_axis(tuple(first_hinge["marker_z_axis"]))
    location = first_hinge["marker_location"]
    lines.append(command_marker("MK_FIX_16_I", "PART16", location, orientation))
    lines.append(command_marker("MK_FIX_16_J", "GROUND", location, orientation))
    lines.extend(
        [
            "constraint create joint fixed joint_name = .MODEL_9299.AUTO_FIX_16 &",
            "    i_marker_name = .MODEL_9299.PART16.MK_FIX_16_I &",
            "    j_marker_name = .MODEL_9299.GROUND.MK_FIX_16_J",
            "",
        ]
    )
    path.write_text("\n".join(lines), encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("step_file", type=Path)
    parser.add_argument("--output", type=Path, default=Path("hinge_candidates.json"))
    args = parser.parse_args()

    entities = read_entities(args.step_file)
    shells = sorted(entity_id for entity_id, entity in entities.items() if entity.startswith("CLOSED_SHELL"))
    if len(shells) != 16:
        raise SystemExit(f"Expected 16 CLOSED_SHELL entities, found {len(shells)}")

    # Adams imports brep_1 through brep_16 as PART2 through PART17.
    part_edges = {part: edge_data_for_shell(entities, shells[part - 2]) for part in range(2, 18)}
    pairs = list(zip(PART_RING, PART_RING[1:] + PART_RING[:1]))
    hinges = []
    for first, second in pairs:
        hinge = best_pair(part_edges[first], part_edges[second])
        hinge["name"] = f"HINGE_{first}_{second}"
        hinge["first_part"] = f"PART{first}"
        hinge["second_part"] = f"PART{second}"
        hinges.append(hinge)

    result = {
        "source": str(args.step_file),
        "part_order": [f"PART{part}" for part in PART_RING],
        "fixed_part": "PART16",
        "hinges": hinges,
        "review_note": "Inspect edge_match_error before creating Adams joints. Low endpoint_distance and non_parallel_factor indicate a reliable shared fold edge.",
    }
    args.output.write_text(json.dumps(result, indent=2), encoding="utf-8")
    command_path = args.output.with_name("create_hinges_9299.cmd")
    write_adams_command(command_path, hinges)
    print(f"Wrote {args.output} with {len(hinges)} candidate fold axes.")
    print(f"Wrote {command_path} for Adams/View import.")


if __name__ == "__main__":
    main()
