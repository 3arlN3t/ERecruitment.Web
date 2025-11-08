# Test Data Guide
## Pre-Seeded Data for ERecruitment.Web

**Status:** ✅ Data seeding is ENABLED and AUTOMATIC  
**When:** Seeding runs automatically on first application startup  
**Location:** `/Data/IdentitySeeder.cs` and `/Data/DomainSeeder.cs`

---

## 🔐 Admin Account

### Credentials
| Field | Value |
|-------|-------|
| **Email** | `admin@example.com` |
| **Password** | `Admin1234!` |
| **Role** | Admin |
| **Status** | Pre-seeded at startup |

### How to Use
1. Start the application: `dotnet run`
2. Go to: `http://localhost:5000/account/login`
3. Login with admin credentials above
4. Access admin features at: `http://localhost:5000/admin`

### Admin Features Available
- View all applications
- Export applications (CSV)
- Update application status
- Bulk reject applications
- View analytics and reports

---

## 👥 Test Applicant Accounts

### Test Applicant 1
| Field | Value |
|-------|-------|
| **Email** | `john.smith@test.com` |
| **Password** | `Test1234!` |
| **Name** | John Smith |
| **SA ID** | 9001015009087 |
| **Phone** | +27 82 123 4567 |
| **Location** | Pretoria, Gauteng |

### Test Applicant 2
| Field | Value |
|-------|-------|
| **Email** | `sarah.jones@test.com` |
| **Password** | `Test1234!` |
| **Name** | Sarah Jones |
| **SA ID** | 8505205009088 |
| **Phone** | +27 82 123 4567 |
| **Location** | Pretoria, Gauteng |

### Test Applicant 3
| Field | Value |
|-------|-------|
| **Email** | `david.williams@test.com` |
| **Password** | `Test1234!` |
| **Name** | David Williams |
| **SA ID** | 7803105009089 |
| **Phone** | +27 82 123 4567 |
| **Location** | Pretoria, Gauteng |

---

## 💼 Job Postings (5 Pre-Seeded Jobs)

All jobs are pre-seeded and ready to apply for!

### Job 1: Digital Transformation Lead
| Field | Value |
|-------|-------|
| **Post Number** | 14/001 |
| **Reference** | DT 01/2025 |
| **Department** | Strategy & Innovation |
| **Location** | Pretoria |
| **Province** | Gauteng |
| **Salary** | R1,022,226 – R1,205,233 per annum (Level 13) |
| **Killer Question** | "Provide an example of a transformation programme you led end-to-end." |

### Job 2: Senior Tourism Economist
| Field | Value |
|-------|-------|
| **Post Number** | 14/002 |
| **Reference** | DT 02/2025 |
| **Department** | Policy & Research |
| **Location** | Cape Town |
| **Province** | Western Cape |
| **Salary** | R908,502 per annum (Level 12) |
| **Killer Question** | "Describe a tourism impact model you have built or contributed to." |

### Job 3: HR Business Partner
| Field | Value |
|-------|-------|
| **Post Number** | 14/003 |
| **Reference** | DT 03/2025 |
| **Department** | Corporate Services |
| **Location** | Durban |
| **Province** | KwaZulu-Natal |
| **Salary** | R811,560 per annum (Level 11) |
| **Killer Question** | "How have you used analytics to drive HR decisions?" |

### Job 4: Infrastructure Project Manager
| Field | Value |
|-------|-------|
| **Post Number** | 14/004 |
| **Reference** | DT 04/2025 |
| **Department** | Tourism Infrastructure |
| **Location** | Polokwane |
| **Province** | Limpopo |
| **Salary** | R744,255 per annum (Level 10) |
| **Killer Question** | "Share an example of delivering a complex capital project on time." |

### Job 5: Grants Compliance Officer
| Field | Value |
|-------|-------|
| **Post Number** | 14/005 |
| **Reference** | DT 05/2025 |
| **Department** | Finance & Governance |
| **Location** | Kimberley |
| **Province** | Northern Cape |
| **Salary** | R557,478 per annum (Level 8) |
| **Killer Question** | "Describe a time you improved compliance controls in a grants environment." |

---

## 📋 Application Flow - Step by Step

### Step 1: Login as Test Applicant
```
URL: http://localhost:5000/account/login
Email: john.smith@test.com
Password: Test1234!
```

### Step 2: Complete Profile (if needed)
```
URL: http://localhost:5000/applicant/profile
- First Name: John
- Last Name: Smith
- SA ID: 9001015009087
- Phone: +27 82 123 4567
- Address: Pretoria, Gauteng
```

### Step 3: Browse Available Jobs
```
URL: http://localhost:5000/applicant/dashboard
- See all 5 job postings
- Click on any job to view details
```

### Step 4: Start Application
```
Click "Apply Now" on any job
- Read job description
- Start application (creates draft)
```

### Step 5: Answer Killer Question
```
Answer the mandatory screening question
Two options:
  a) Save as Draft
  b) Submit answer
```

### Step 6: Submit Application
```
Submit answer to complete the application
- Status changes to "Submitted"
- Applicant receives confirmation
```

---

## 🎯 Testing Scenarios

### Scenario 1: Complete Application Success
1. Login: `john.smith@test.com` / `Test1234!`
2. Apply for: "Digital Transformation Lead"
3. Answer killer question: Any answer works
4. Submit application
5. **Expected:** Success message, application marked as submitted

### Scenario 2: Test Error Handling
1. Login as admin: `admin@example.com` / `Admin1234!`
2. Go to: Admin → Applications
3. Try to reject without reason
4. **Expected:** Validation error from Phase 2 exception handling

### Scenario 3: Bulk Operations
1. Login as admin
2. Select multiple applications
3. Bulk reject them
4. **Expected:** All selected applications rejected with message

### Scenario 4: Export Data
1. Login as admin
2. Go to: Admin → Applications
3. Click "Export CSV"
4. **Expected:** CSV file downloads with application data

### Scenario 5: Application Withdrawal
1. Login as applicant
2. Go to: Dashboard
3. Find submitted application
4. Click "Withdraw"
5. **Expected:** Application status changes to "Withdrawn"

---

## 🔍 Verify Seeding Worked

### Check in Database
```bash
# Connect to SQL Server
docker exec erecruitment-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrongPassword123!'

# Run these queries:
SELECT COUNT(*) AS JobPostings FROM JobPostings;
SELECT COUNT(*) AS Applicants FROM Applicants;
SELECT COUNT(*) AS Users FROM AspNetUsers;

# Expected results:
# JobPostings: 5
# Applicants: 3+
# Users: 4 (1 admin + 3 test accounts)
```

### Check in Application Logs
When you run `dotnet run`, look for:
```
info: ERecruitment.Web.Data.IdentitySeeder[0]
      Seeding identity users...
      
info: ERecruitment.Web.Data.DomainSeeder[0]
      Seeding domain data...
```

---

## 🛠️ Troubleshooting

### Issue: No Jobs Appearing
**Solution:** 
- Verify database migration ran
- Check seeding logs in console
- Restart application to re-seed

### Issue: Admin Login Fails
**Solution:**
- Check credentials: `admin@example.com` / `Admin1234!`
- Verify SQL Server is running
- Check database connection string

### Issue: Test Accounts Not Available
**Solution:**
- Restart application to re-seed
- Clear browser cookies and session
- Check applicant profile is complete

### Issue: Can't Answer Killer Question
**Solution:**
- Complete profile first
- Ensure profile has SA ID or Passport
- Check job has questions defined

---

## 📊 Default Configuration

### Passwords (Development Only)
| User Type | Password | Note |
|-----------|----------|------|
| Admin | `Admin1234!` | Development credential |
| Test Applicants | `Test1234!` | Development credential |

**⚠️ WARNING:** These are development-only credentials. In production, use secure password generation and management.

### Contact Information
All test applicants have the same contact info for simplicity:
- **Phone:** +27 82 123 4567
- **Location:** Pretoria, Gauteng
- **Email:** As per test email

---

## 🚀 Starting Fresh

If you want to reset the database and re-seed:

```bash
# Stop the application
Ctrl+C

# Delete the database (optional)
docker exec erecruitment-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "DROP DATABASE ERecruitment;"

# Start the application again
dotnet run

# The database will be automatically:
# 1. Created
# 2. Migrations applied
# 3. Data seeded
```

---

## 📝 Seeding Code Locations

| Component | File | Lines |
|-----------|------|-------|
| Identity Seeding | `/Data/IdentitySeeder.cs` | 1-105 |
| Domain Seeding | `/Data/DomainSeeder.cs` | 1-343 |
| Seeding Initialization | `/Program.cs` | 139, 146 |
| Test Credentials | `/Constants.cs` | 160-178 |

---

## ✅ Verification Checklist

- [ ] Application starts successfully
- [ ] Admin can login with provided credentials
- [ ] Test applicants can login
- [ ] 5 job postings visible on dashboard
- [ ] Can apply for a job
- [ ] Can answer killer question
- [ ] Can submit application
- [ ] Admin can view all applications
- [ ] Admin can update application status
- [ ] Error handling displays properly

---

## 🎓 Learning with Test Data

### Learn Authentication
- Use admin account to understand role-based access
- Use test accounts to understand applicant flow

### Learn Workflows
- Complete full application from start to finish
- Test error scenarios (withdrawal, status updates)

### Learn Error Handling (Phase 2)
- Trigger validation errors intentionally
- Test exception handling display
- Verify error messages are user-friendly

### Learn Admin Features
- Manage applications as admin
- Perform bulk operations
- Export data for reporting

---

## Summary

✅ **5 Job Postings** pre-seeded  
✅ **3 Test Applicant Accounts** pre-seeded  
✅ **1 Admin Account** pre-seeded  
✅ **Automatic seeding** on first run  
✅ **Ready to test** immediately after startup  

**Start the application and begin testing!**

---

**Last Updated:** November 7, 2025  
**Status:** ✅ Production Ready


