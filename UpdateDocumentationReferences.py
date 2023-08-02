import re
from pathlib import Path
from typing import Generator

COMMENT_SLASHES = "/// "


def get_relevant_cs_files(path: Path) -> Generator[Path, None, None]:
    all_cs_files = path.rglob("*.cs")
    for file_path in all_cs_files:
        # exclude visual-studio's internal cs files
        if ("Debug" not in file_path.parts) and ("Release" not in file_path.parts):
            yield file_path


def overwrite_file(original_file: Path, content: str) -> None:
    print("Updating file:" + original_file.as_posix())
    new_file = original_file.with_suffix(".tmp")
    with new_file.open("w") as f:
        f.write(content)
    original_file.unlink()
    new_file.rename(original_file)


def fix_file(file: Path, new_path: str, pattern: re.Pattern) -> (str, int):
    with file.open("r") as f:
        code = f.read()
    full_documentation_string = COMMENT_SLASHES + new_path
    return pattern.subn(
        full_documentation_string, code
    )  # <- fails here if regex is corrupted


def main():
    working_directory = Path.cwd()
    print(f"Working dir: {working_directory}")
    if not (working_directory / ".git").exists():
        raise NotADirectoryError("Not a valid git repository.")
    project_name = working_directory.parts[-1]
    print("Project name: " + project_name)

    project_name_re_escaped = re.escape(project_name)
    pattern = re.compile(r"///.*?file:///.*?/" + project_name_re_escaped)
    # re doesn't understand named groups -> https://docs.python.org/3/howto/regex.html
    new_path = working_directory.as_uri()
    for code_file in get_relevant_cs_files(working_directory):
        new_code, changes = fix_file(code_file, new_path, pattern)
        if changes:
            overwrite_file(code_file, new_code)


if __name__ == "__main__":
    try:
        main()
        print("done!")
    except NotADirectoryError:
        print("Not a git repository. Exiting.")
    input("Enter to close")
