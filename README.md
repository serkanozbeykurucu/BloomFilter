# BloomFilter API - Suspicious Domain & Email Detection System

A high-performance security API that leverages **Bloom Filter algorithm** to detect suspicious domains and email addresses in real-time. Built with .NET 9.0 and designed for scalability and speed.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Response Structure](#response-structure)
- [Controllers Overview](#controllers-overview)
- [Usage Examples](#usage-examples)
- [Performance Notes](#performance-notes)
- [License](#license)

## Overview

BloomFilter API is a sophisticated threat detection system designed to identify malicious domains and email addresses with minimal latency. It combines the probabilistic efficiency of Bloom Filters with the accuracy of database verification, making it ideal for:

- Phishing detection systems
- Email spam filters
- Browser security extensions
- Threat intelligence platforms
- User-generated content moderation

### How It Works

1. **Bloom Filter Layer (Fast)**: First-level check using probabilistic data structure (~0.1ms)
2. **Database Layer (Accurate)**: Second-level verification for exact matches (~5-10ms)
3. **User Reports**: Community-driven threat intelligence
4. **Admin Dashboard**: Manage and review reported threats

## Features

- **High Performance**: Sub-millisecond checks using Bloom Filter algorithm
- **Two-Tier Verification**: Fast probabilistic checks + accurate database validation
- **Automatic Data Seeding**: Pre-load suspicious domains/emails on startup
- **User Reporting System**: Allow users to report suspicious content
- **Bulk Operations**: Efficiently add multiple entries at once
- **Admin Dashboard**: Statistics and report management
- **RESTful API**: Clean, well-documented endpoints
- **Swagger UI**: Interactive API documentation
- **Multi-language Support**: EN, TR, PL localization
- **Health Checks**: Monitor system status

## Technology Stack

- **.NET 9.0** - Modern C# runtime
- **Entity Framework Core** - ORM for PostgreSQL
- **PostgreSQL** - Relational database
- **Bloom Filter** - Probabilistic data structure
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation

## Architecture

```
BloomFilter/
├── src/
│   ├── BloomFilter.Business/          # Business Logic Layer
│   │   ├── Abstract/                  # Service Interfaces
│   │   └── Concrete/                  # Service Implementations
│   ├── BloomFilter.DataAccess/        # Data Access Layer
│   │   ├── Abstract/                  # Repository Interfaces
│   │   └── Concrete/                  # EF Core Repositories
│   ├── BloomFilter.Entity/            # Domain Entities
│   ├── BloomFilter.Dto/               # Data Transfer Objects
│   ├── BloomFilter.Shared/            # Shared Components
│   │   └── Responses/                 # Response Wrappers
│   ├── BloomFilter.HttpApi/           # API Controllers
│   └── BloomFilter.HttpApi.Host/      # Application Host
│       └── SeedData/                  # Seed Data Files
└── test/
    └── BloomFilter.Tests/             # Unit & Integration Tests
```

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- Visual Studio 2022, VS Code, or Rider

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/bloomfilter-api.git
cd bloomfilter-api
```

2. **Configure database connection**

Edit `src/BloomFilter.HttpApi.Host/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BloomFilterDb;Username=postgres;Password=yourpassword"
  }
}
```

3. **Run database migrations**
```bash
cd src/BloomFilter.HttpApi.Host
dotnet ef database update
```

4. **Prepare seed data (optional)**

Add your own data to seed files:
- `SeedData/suspicious.txt` - List of suspicious domains (one per line)
- `SeedData/emails.txt` - List of suspicious emails (one per line)

5. **Run the application**
```bash
dotnet run
```

The application will automatically:
- Run database migrations
- Initialize Bloom Filters
- Load seed data (if files exist)

### Access Points

- **API Base URL**: `https://localhost:7038`
- **Swagger UI**: `https://localhost:7038/swagger`
- **Health Check**: `https://localhost:7038/health`

## Response Structure

All API responses follow a consistent structure using the generic `Response<T>` wrapper from `BloomFilter.Shared`:

### Success Response
```json
{
  "responseCode": 200,
  "message": "Success",
  "data": { ... }
}
```

### Error Response
```json
{
  "responseCode": 400,
  "message": "Validation error message",
  "data": null
}
```

### Response Codes
- `200` - Success
- `204` - NoContent
- `400` - BadRequest
- `401` - Unauthorized
- `403` - Forbidden
- `404` - NotFound
- `500` - InternalServerError

## Controllers Overview

### 1. BloomFilterController (`/api/BloomFilter`)

**Purpose**: Low-level Bloom Filter management and direct filter operations

**Endpoints**:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/Initialize` | Initialize Bloom Filters |
| POST | `/Rebuild` | Rebuild filters from database |
| GET | `/GetFilterStats?filterName={name}` | Get filter statistics |
| POST | `/AddDomain` | Add domain to filter |
| POST | `/AddEmail` | Add email to filter |
| POST | `/CheckDomain` | Check if domain exists in filter |
| POST | `/CheckEmail` | Check if email exists in filter |

**Example Responses**:

**Check Domain (Suspicious)**:
```json
{
  "responseCode": 200,
  "data": true,
  "message": "Domain 'phishing-site.com' found in Bloom Filter. This domain may be suspicious (Note: false positives possible)"
}
```

**Check Domain (Clean)**:
```json
{
  "responseCode": 200,
  "data": false,
  "message": "Domain 'google.com' not found in Bloom Filter. This domain appears to be safe"
}
```

---

### 2. SuspiciousDomainController (`/api/SuspiciousDomain`)

**Purpose**: Manage suspicious domains with database persistence and Bloom Filter integration

**Endpoints**:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/Check` | Check if domain is suspicious (main endpoint) |
| GET | `/GetDomains` | Get paginated list of domains |
| GET | `/GetMostReportedDomains?count={n}` | Get top reported domains |
| GET | `/SearchDomains?searchTerm={term}` | Search domains by term |
| POST | `/bulk` | Add multiple domains at once |

**Example Request - Check Domain**:
```json
POST /api/SuspiciousDomain/Check
{
  "domainName": "account-verify-secure.com"
}
```

**Example Response**:
```json
{
  "responseCode": 200,
  "data": {
    "checkedValue": "account-verify-secure.com",
    "isSuspicious": true,
    "checkType": "Domain",
    "isExactMatch": true,
    "checkedDate": "2024-01-15T10:30:00Z",
    "additionalInfo": "Domain found in database with 12 user reports"
  },
  "message": "Success"
}
```

**Example Request - Bulk Add**:
```json
POST /api/SuspiciousDomain/bulk
{
  "items": [
    "malware-download.xyz",
    "fake-banking.com",
    "phishing-scam.net"
  ],
  "description": "Added from threat intelligence feed"
}
```

**Example Response**:
```json
{
  "responseCode": 200,
  "data": {
    "totalCount": 3,
    "successCount": 3,
    "failedCount": 0,
    "failedItems": [],
    "successItems": [
      "malware-download.xyz",
      "fake-banking.com",
      "phishing-scam.net"
    ],
    "processedDate": "2024-01-15T10:30:00Z"
  },
  "message": "Bulk operation completed successfully"
}
```

**Example Response - Get Domains (Paginated)**:
```json
{
  "responseCode": 200,
  "data": {
    "data": [
      {
        "id": 1,
        "domainName": "phishing-site.com",
        "description": "Reported phishing site",
        "reportCount": 15,
        "lastReportedDate": "2024-01-15T08:20:00Z",
        "createdDate": "2024-01-10T12:00:00Z",
        "isActive": true
      }
    ],
    "totalCount": 1250,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 125,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "Success"
}
```

---

### 3. SuspiciousEmailController (`/api/SuspiciousEmail`)

**Purpose**: Manage suspicious email addresses with filtering and search capabilities

**Endpoints**:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/Check` | Check if email is suspicious |
| GET | `/GetSuspiciousEmails` | Get paginated list of emails |
| GET | `/GetMostReportedEmails?count={n}` | Get top reported emails |
| GET | `/SearchEmails?searchTerm={term}` | Search emails by term |
| GET | `/GetEmailsByDomain?domainName={domain}` | Get emails from specific domain |
| POST | `/AddBulkEmails` | Add multiple emails at once |

**Example Request - Check Email**:
```json
POST /api/SuspiciousEmail/Check
{
  "value": "spam-account@protonmail.net"
}
```

**Example Response**:
```json
{
  "responseCode": 200,
  "data": {
    "checkedValue": "spam-account@protonmail.net",
    "isSuspicious": true,
    "checkType": "Email",
    "isExactMatch": true,
    "checkedDate": "2024-01-15T10:30:00Z",
    "additionalInfo": "Email found with 8 reports"
  },
  "message": "Success"
}
```

**Example Response - Get Emails by Domain**:
```json
{
  "responseCode": 200,
  "data": [
    {
      "id": 45,
      "emailAddress": "spam1@tempmail.com",
      "domainName": "tempmail.com",
      "description": "Spam account",
      "reportCount": 5,
      "lastReportedDate": "2024-01-14T15:30:00Z",
      "createdDate": "2024-01-10T09:00:00Z",
      "isActive": true
    },
    {
      "id": 78,
      "emailAddress": "phishing@tempmail.com",
      "domainName": "tempmail.com",
      "description": "Phishing attempt",
      "reportCount": 12,
      "lastReportedDate": "2024-01-15T08:20:00Z",
      "createdDate": "2024-01-12T14:30:00Z",
      "isActive": true
    }
  ],
  "message": "Success"
}
```

---

### 4. UserReportController (`/api/UserReport`)

**Purpose**: Handle user-submitted reports of suspicious domains and emails

**Endpoints**:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/Create` | Submit a new report |
| GET | `/GetReportsByStatus?status={n}` | Get reports by status |
| GET | `/GetRecentReports?count={n}` | Get recent reports |
| PUT | `/UpdateReportStatus` | Update report status (Admin) |
| GET | `/GetDashboardStats` | Get dashboard statistics |

**Report Status Enum**:
- `0` - Pending (awaiting review)
- `1` - Approved (verified and added to blocklist)
- `2` - Rejected (false positive)

**Report Type Enum**:
- `0` - Email
- `1` - Domain

**Example Request - Create Report**:
```json
POST /api/UserReport/Create
{
  "reportType": 1,
  "reportedValue": "suspicious-banking-login.com",
  "description": "This site attempted to steal my banking credentials. It looks identical to my bank's website.",
  "reporterEmail": "user@example.com"
}
```

**Example Response**:
```json
{
  "responseCode": 200,
  "data": {
    "id": 234,
    "reportType": 1,
    "reportedValue": "suspicious-banking-login.com",
    "description": "This site attempted to steal my banking credentials...",
    "reporterEmail": "user@example.com",
    "status": 0,
    "reportDate": "2024-01-15T10:30:00Z"
  },
  "message": "Report created successfully"
}
```

**Example Request - Update Report Status (Admin)**:
```json
PUT /api/UserReport/UpdateReportStatus
{
  "reportId": 234,
  "newStatus": 1,
  "adminNotes": "Verified as phishing site. Added to blocklist."
}
```

**Example Response - Dashboard Stats**:
```json
{
  "responseCode": 200,
  "data": {
    "totalSuspiciousDomains": 1250,
    "totalSuspiciousEmails": 3400,
    "totalUserReports": 856,
    "pendingReports": 23,
    "todayReports": 7,
    "bloomFilterStats": [
      {
        "filterName": "DomainFilter",
        "itemCount": 1250,
        "estimatedFalsePositiveRate": 0.01,
        "capacity": 10000
      },
      {
        "filterName": "EmailFilter",
        "itemCount": 3400,
        "estimatedFalsePositiveRate": 0.01,
        "capacity": 10000
      }
    ],
    "generatedDate": "2024-01-15T10:30:00Z"
  },
  "message": "Success"
}
```

## Usage Examples

### 1. E-commerce Platform - Checkout Validation

```javascript
async function validateCheckout(email, websiteUrl) {
  // Check email safety
  const emailResponse = await fetch('/api/SuspiciousEmail/Check', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ value: email })
  });

  const emailResult = await emailResponse.json();

  if (emailResult.data.isSuspicious) {
    showWarning('This email address has been flagged as suspicious');
    return false;
  }

  // Check external link safety
  if (websiteUrl) {
    const domain = new URL(websiteUrl).hostname;
    const domainResponse = await fetch('/api/SuspiciousDomain/Check', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ domainName: domain })
    });

    const domainResult = await domainResponse.json();

    if (domainResult.data.isSuspicious) {
      showWarning(`This website has been reported ${domainResult.data.additionalInfo}`);
      return false;
    }
  }

  return true;
}
```

### 2. Admin Panel - Process User Reports

```javascript
// Fetch pending reports
async function loadPendingReports() {
  const response = await fetch('/api/UserReport/GetReportsByStatus?status=0&pageNumber=1&pageSize=20');
  const result = await response.json();

  displayReports(result.data.data);
}

// Approve report and add to blocklist
async function approveReport(reportId, reportedDomain) {
  // Add to suspicious domains
  await fetch('/api/SuspiciousDomain/bulk', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      items: [reportedDomain],
      description: `Verified from user report #${reportId}`
    })
  });

  // Update report status
  await fetch('/api/UserReport/UpdateReportStatus', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      reportId: reportId,
      newStatus: 1,
      adminNotes: 'Verified and added to blocklist'
    })
  });

  showSuccess('Report approved and domain added to blocklist');
}
```

### 3. Browser Extension - Real-time Protection

```javascript
// Check website when user navigates
chrome.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {
  if (changeInfo.status === 'complete' && tab.url) {
    const domain = new URL(tab.url).hostname;

    const response = await fetch('https://api.yourservice.com/api/SuspiciousDomain/Check', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ domainName: domain })
    });

    const result = await response.json();

    if (result.data.isSuspicious) {
      // Show warning badge
      chrome.action.setBadgeText({ text: '!', tabId: tabId });
      chrome.action.setBadgeBackgroundColor({ color: '#FF0000' });

      // Show notification
      chrome.notifications.create({
        type: 'basic',
        iconUrl: 'warning.png',
        title: 'Suspicious Website Detected',
        message: `This website has been flagged: ${result.data.additionalInfo}`
      });
    }
  }
});

// Allow users to report suspicious sites
function reportCurrentSite() {
  chrome.tabs.query({ active: true, currentWindow: true }, async (tabs) => {
    const domain = new URL(tabs[0].url).hostname;
    const description = prompt('Please describe the suspicious activity:');

    await fetch('https://api.yourservice.com/api/UserReport/Create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        reportType: 1,
        reportedValue: domain,
        description: description,
        reporterEmail: 'extension-user@anonymous.com'
      })
    });

    alert('Thank you for reporting. Our team will review it shortly.');
  });
}
```

### 4. Threat Intelligence Integration

```javascript
// Import threats from external feeds daily
async function importThreatFeed() {
  const feedUrl = 'https://threatfeed.example.com/daily-malicious-domains.json';

  const feedResponse = await fetch(feedUrl);
  const threats = await feedResponse.json();

  // Filter and prepare domains
  const domains = threats
    .filter(t => t.category === 'phishing' || t.category === 'malware')
    .map(t => t.domain);

  // Bulk add to system
  const response = await fetch('/api/SuspiciousDomain/bulk', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      items: domains,
      description: `Imported from threat feed at ${new Date().toISOString()}`
    })
  });

  const result = await response.json();

  console.log(`Successfully added ${result.data.successCount} domains`);
  console.log(`Duplicates skipped: ${result.data.totalCount - result.data.successCount}`);
}

// Schedule daily import
setInterval(importThreatFeed, 24 * 60 * 60 * 1000);
```

### 5. Two-Tier Check Strategy (Recommended)

```javascript
// Optimal performance: Fast Bloom Filter + Accurate DB verification
async function checkDomainSmart(domain) {
  // Step 1: Fast Bloom Filter check (~0.1ms)
  const bloomResponse = await fetch('/api/BloomFilter/CheckDomain', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(domain)
  });

  const bloomResult = await bloomResponse.json();

  // If not in Bloom Filter, definitely safe
  if (!bloomResult.data) {
    return {
      safe: true,
      confidence: 'high',
      checkTime: '~0.1ms'
    };
  }

  // Step 2: Database verification for accuracy (~5-10ms)
  const dbResponse = await fetch('/api/SuspiciousDomain/Check', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ domainName: domain })
  });

  const dbResult = await dbResponse.json();

  return {
    safe: !dbResult.data.isSuspicious,
    confidence: 'high',
    details: dbResult.data,
    checkTime: '~5-10ms'
  };
}
```

## Performance Notes

### Bloom Filter vs Database Comparison

| Operation | Bloom Filter | Database | Recommendation |
|-----------|--------------|----------|----------------|
| Check Speed | ~0.1ms | ~5-10ms | Use Bloom Filter first |
| Accuracy | 99% (1% false positive) | 100% | Verify positives in DB |
| Memory | ~10KB | ~100MB | Bloom Filter is efficient |
| Scalability | Excellent | Good | Combine both approaches |

### Best Practices

1. **First Check**: Use Bloom Filter for ultra-fast initial screening
2. **Verification**: If Bloom Filter returns positive, verify with database
3. **False Positives**: Bloom Filter may have ~1% false positive rate (configurable)
4. **Bulk Operations**: Process ~100 items per request for optimal performance
5. **Caching**: Consider caching frequently checked domains

### Performance Metrics

- **Bloom Filter Check**: ~0.1ms per domain/email
- **Database Check**: ~5-10ms per domain/email
- **Bulk Insert (100 items)**: ~50ms total
- **Filter Rebuild**: ~500ms for 10,000 items

## Security Considerations

1. **Rate Limiting**: Implement API rate limiting in production
2. **Authentication**: Secure admin endpoints (`/UpdateReportStatus`, etc.) with auth middleware
3. **Input Validation**: All inputs are validated at DTO level
4. **HTTPS**: Always use HTTPS in production
5. **CORS**: Configure CORS policy in `appsettings.json` for your domains
6. **SQL Injection**: Protected via EF Core parameterized queries
7. **XSS Prevention**: API returns JSON only (frontend responsibility for sanitization)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

For questions, issues, or feature requests, please open an issue on GitHub.

---

**Built with .NET 9.0 | Powered by Bloom Filter Algorithm**
