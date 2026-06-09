# AGENTS.md

# ==================================================
# IMPORTANT
# ==================================================

Always read this file before making any code changes.

This file contains project-specific rules.

Preserving existing functionality is the highest priority.

If any request conflicts with these rules, ask for confirmation first.

# ==================================================
# PROJECT OVERVIEW
# ==================================================

This is an ASP.NET MVC project.

The project is currently under development.

Many features already work correctly.

The goal is to continue development safely without breaking existing functionality.

Do not assume that the project structure needs improvement.

Do not assume that existing naming is wrong.

Follow the project's existing style.

# ==================================================
# LANGUAGE RULES
# ==================================================

This project intentionally uses Vietnamese names without accents.

Examples:

- NhanVien
- KhachHang
- SanPham
- DonHang
- NhaCungCap
- KhuyenMai

These names are intentional.

DO NOT automatically translate them into English.

DO NOT rename existing entities.

Keep the current naming style used by the project.

# ==================================================
# DATABASE RULES
# ==================================================

Database structures may use Vietnamese names without accents.

This is intentional.

DO NOT rename:

- Tables
- Columns
- Models
- Entity names
- Property names

unless explicitly requested.

Examples:

KEEP:

- NhanVien
- HoTen
- ChucVu
- DienThoai
- NgayVaoLam

DO NOT CHANGE TO:

- Employee
- FullName
- Position
- PhoneNumber
- HireDate

# ==================================================
# CODE SAFETY RULES
# ==================================================

Before modifying any code:

1. Read related files.
2. Understand the current implementation.
3. Check dependencies.
4. Check references.

Never modify code you do not understand.

Never assume unused code is safe to remove.

Never remove functionality unless explicitly requested.

# ==================================================
# CHANGE LIMITS
# ==================================================

Always make the smallest possible change.

Prefer fixing the specific problem.

Avoid large refactoring.

Avoid architecture changes.

Avoid mass renaming.

Avoid unnecessary optimization.

If more than 3 files need modification:

STOP.

Explain:

- Why.
- Which files.
- What will change.

Ask for confirmation before continuing.

# ==================================================
# EXISTING FEATURES PROTECTION
# ==================================================

Existing working features are more important than code style.

Never break an existing feature to improve naming.

Never break an existing feature to improve architecture.

Never break an existing feature to improve formatting.

Functionality comes first.

# ==================================================
# FILE MODIFICATION RULES
# ==================================================

Do not rename files unless requested.

Do not move files unless requested.

Do not create duplicate implementations.

Reuse existing code whenever possible.

Do not create new services, repositories, helpers,
or abstractions unless required.

# ==================================================
# RESPONSE RULES
# ==================================================

Before making changes:

Explain:

1. What will be changed.
2. Why it is needed.
3. Which files will be modified.

After making changes:

Explain:

1. Which files were changed.
2. What was changed.
3. Why it was changed.
4. Possible side effects.

# ==================================================
# ERROR HANDLING
# ==================================================

If project context is unclear:

DO NOT GUESS.

Ask questions first.

If multiple solutions exist:

Explain options.

Wait for confirmation.

# ==================================================
# PRIORITY ORDER
# ==================================================

Priority 1:
Preserve existing functionality.

Priority 2:
Read AGENTS.md first.

Priority 3:
Keep Vietnamese naming.

Priority 4:
Make minimal changes.

Priority 5:
Ask before large modifications.

Priority 6:
Avoid unnecessary refactoring.