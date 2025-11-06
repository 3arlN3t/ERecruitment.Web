# QA Validation Runbook

This runbook captures the targeted security, usability, and performance checks required for the final readiness sign‑off. Follow the steps in each section, record evidence in the indicated locations, and attach artefacts (screenshots, logs, exports) to the project documentation repository once complete.

---

## 1. Security Checks

| Check | Procedure | Evidence |
| --- | --- | --- |
| **1.1 Anti‑forgery tokens** | Inspect HTML forms in the browser (e.g., registration, profile update, admin status update). Confirm presence of `__RequestVerificationToken`. Attempt replaying a POST request without the token using `curl`/Postman – expect `400` or redirect. | Screenshot + intercepted request log |
| **1.2 Authentication & authorization** | 1) Access `/Admin/*` endpoints while unauthenticated – expect redirect to `/Account/Login`. 2) Authenticate as an applicant and attempt `/Admin/*` – expect `403`/redirect. 3) Admin login should require confirmed email/admin role. | Browser screenshots of redirects |
| **1.3 Password storage** | Query Identity database (SQL Server): `SELECT TOP 5 Id, Email, PasswordHash FROM AspNetUsers;` Confirm hashes use Identity v3 format (`AQAAAA...`). | Screenshot/redacted SQL output |
| **1.4 SQL injection resilience** | Submit payloads such as `' OR 1=1--` or `"; DROP TABLE Applicants; --` into search fields (job browsing, admin application filter). Monitor logs for exceptions; verify results remain filtered or empty. | Console/log snippet |
| **1.5 XSS resilience** | Populate profile fields with encoded payloads: `<script>alert('x')</script>` and `<img src=x onerror=alert(1)>`. Save and view dashboard/admin listings – expect escaped output (`&lt;script&gt;`). | Screenshot showing encoded output |
| **1.6 CSRF guard** | Using browser dev tools, copy a POST request (e.g., profile save). Reissue without the anti-forgery header/token – expect request blocked. | Network log |
| **1.7 File upload scanning** | Attempt to upload `test.exe` and `test.docx` with embedded scripts for CV upload. Service should reject unsupported extensions/unsafe content (check logs for `File failed safety scan`). | Upload response screenshot |
| **1.8 Session timeout** | Configure `IdleTimeout` to 1 minute locally (web.config override), login, remain idle past timeout, then navigate – expect forced re-login. | Screen recording or sequential screenshots |
| **1.9 Transport security** | Launch site via HTTPS (`https://localhost:5051`). Use `openssl s_client -connect localhost:5051` to confirm TLS handshake and certificate chain (dev cert acceptable). | Terminal capture |
| **1.10 Email verification** | Register new account, ensure confirmation email is sent. Attempt login prior to confirmation – expect block. Confirm via link and re-test login. | Email client screenshot + login outcome |

---

## 2. Performance Spot Checks

> **Pre-requisites:** Ensure build configuration is `Release`, seed representative data (≥100 applicants, ≥50 job posts, ≥500 applications) via existing seeding utilities or scripts.

| Check | Procedure | Evidence |
| --- | --- | --- |
| **2.1 API latency sampling** | Use `wrk`, `ab`, or `k6` to hit `/Applicant/Dashboard`, `/Jobs/Index`, `/AdminApplications/Index` with 50 virtual users for 1 minute. Record mean/95th percentile latency and error rate. | Tool report |
| **2.2 Database profiling** | Enable EF Core logging (`appsettings.Development.json`: `"Microsoft.EntityFrameworkCore.Database.Command": "Information"`). Load admin dashboard and application list; confirm no N+1 query spikes (>20 queries/request). | Log excerpt |
| **2.3 File upload throughput** | Upload 5 MB PDF. Measure response time and confirm file stored under `App_Data/cvs`. | Stopwatch reading & storage screenshot |
| **2.4 Concurrent admin workflow** | In two browser sessions, edit the same application status simultaneously. Confirm last writer wins, audit entries capture both actions, no concurrency exceptions. | Audit log screenshot |
| **2.5 Memory footprint** | Run site with `dotnet-counters monitor --process-id <pid> System.Runtime` during load test; ensure GC heap remains stable (<300 MB) and no sustained LOH growth. | `dotnet-counters` output |

---

## 3. Usability & Accessibility Review

| Check | Procedure | Evidence |
| --- | --- | --- |
| **3.1 Responsive design** | Test key screens (Registration, Applicant dashboard, Admin manage application) at breakpoints 320px, 768px, 1024px using browser dev tools. | Screenshots for each breakpoint |
| **3.2 Keyboard navigation** | Navigate forms using keyboard only (Tab/Shift+Tab). Confirm logical focus order and visibility of focus outlines. | Short screen recording or notes |
| **3.3 Screen reader cues** | Use NVDA or VoiceOver on registration and admin manage pages. Confirm labels announced correctly, combo boxes read current value, validation errors announced. | Audio transcript or tester notes |
| **3.4 Error messaging clarity** | Trigger validation errors (missing required fields, invalid ID/passport). Confirm error summaries and inline messages are human-readable and guide corrections. | Screenshot |
| **3.5 Color contrast** | Run Lighthouse or axe DevTools. Ensure no contrast violations for primary UI elements/badges. | Report excerpt |
| **3.6 Admin workflow intuitiveness** | Have a peer walk through closing/reopening a job and updating an application status with email. Collect qualitative feedback (time to complete, confusion points). | Feedback notes |

---

## 4. Evidence Consolidation Checklist

- [ ] Store raw outputs (logs, reports, screenshots) under `docs/evidence/<YYYYMMDD>/`.
- [ ] Update `docs/audit/TEST_EXECUTION_SUMMARY.md` with pass/fail per check.
- [ ] Log any defects in the issue tracker (include reproduction steps and references to evidence artefacts).
- [ ] Update runbook completion date and tester initials below.

| Run Date | Tester | Notes |
| --- | --- | --- |
| (YYYY-MM-DD) |  |  |

---

### Appendix A – Suggested Tools

- **Load Testing:** `k6`, `wrk`, `ApacheBench (ab)`
- **Security Probing:** Postman, `curl`, Burp Suite, OWASP ZAP (baseline scan)
- **Accessibility:** Chrome Lighthouse, axe DevTools, NVDA/VoiceOver
- **System Counters:** `dotnet-counters`, `dotnet-trace`, SQL Server Profiler

---

> After executing the checks, update this document with findings (PASSED / FAILED, defect IDs), and archive associated evidence in the project repository for audit readiness.

