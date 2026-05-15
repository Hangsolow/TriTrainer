# QA Bug Report: Sprint 2 Integration Blocker

## Component
TriTrainer API Service (`/activities` POST/DELETE endpoints)

## Severity
**blocker** — All POST and DELETE operations on `/activities` endpoint return 500 Internal Server Error

## Steps to Reproduce

1. Start Aspire: `aspire start`
2. Wait for API service to be healthy
3. POST a new activity:
   ```bash
   curl -X POST http://localhost:5398/activities \
     -H "Content-Type: application/json" \
     -d '{
       "date": "2026-04-30",
       "type": "Run",
       "durationMinutes": 45,
       "notes": "Test"
     }'
   ```

## Expected
HTTP 201 Created with activity ID in response

## Actual
HTTP 500 Internal Server Error:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "..."
}
```

## Root Cause
The `CreateActivityRequest` record has a `Type` field (enum `ActivityType`), but the JSON deserializer is not configured to accept string-based enum values. The error log shows:
```
Microsoft.AspNetCore.Http.BadHttpRequestException: Failed to read parameter "CreateActivityRequest request" from the request body as JSON.
---> System.Text.Json.JsonException: The JSON value could not be converted to CreateActivityRequest. Path: $.type
---> System.Text.Json.Serialization.Converters.EnumConverter`1.Read(...)
```

## Why This Passed Dev Tests
The unit tests (`ActivityTests.cs`) do not test HTTP serialization — they test the EF Core model directly in C# code, which passes fine. The integration tests (`IntegrationTest1.cs`) do test HTTP endpoints and return **3 failures** on POST/DELETE operations, but the dev lane progress report claims "All tests pass, 0 skipped."

## Fix Required
Add `JsonStringEnumConverter` to `Program.cs` HTTP JSON options:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

## Impact
- Sprint 1 legacy activities workflow is completely broken (no activities can be created or deleted)
- Integration tests fail (3/5 failing)
- E2E smoke tests will fail on any activity creation attempt
- Regression gate cannot pass

## Evidence
- Integration test failures: DeleteActivity, PostActivity, PostActivity_ThenAppearInGetActivities
- API logs show JsonException during enum deserialization
- Manual curl test reproduces 500 on POST

---
**Filed by:** Ivy (QA)  
**Date:** 2026-05-15  
**Status:** RESOLVED  
**Resolved by:** Sage (dev lane) — `JsonStringEnumConverter` added to `Program.cs` `ConfigureHttpJsonOptions`  
**Resolved date:** 2026-05-15  
**Verified by:** Ivy (QA) — integration suite passes 5/5  
**Assigned to:** Closed
