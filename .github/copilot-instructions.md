# Copilot Review Instructions

## IMPORTANT: Review Comment Format (Educational Approach)

**YOU MUST structure ALL comments to follow the template**

### Standard Comment Template:

🔴/🟠/🟡 **[Severity]: [Issue Title]**
[Issue description]

Severity are:
🔴 Critical Issues
🟠 Hight Priority Issues (Warn)
🟡 Our Standards (non-blocking issue)

## Project Context
ASP.NET Core 9 Web API for order management

## Critical Issues (Block PR)
- SQL injection vulnerabilities
- Missing [Authorize] attributes
- Storing passwords/secrets in code

## High Priority Issues (Warn)
- Missing async/await on I/O operations
- No input validation
- Missing error handling

## Our Standards
- Use Entity Framework Core (not raw SQL)
- All endpoints require authorization
- Return DTOs, not entities

