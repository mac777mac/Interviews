# Solution conventions

## EF
- Prefer LINQ extension methods over raw SQL if possible.
- Keep EF configurations separate from entity classes. Place them in EntityConfiguration folder
- EF configurations should not contain collation if not specified explicitly

# coding conventions
- if operator always has branch body wrapped in curly brakets 