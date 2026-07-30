# Work Around

## Applied

- All table headers and cells are centered in `TeacherGroupsManager.WebUI/wwwroot/css/site.css`.
- Long table cell content is trimmed with an ellipsis.
- Full cell text is available on hover through the `title` attribute.
- Table cell titles are applied to static tables and refreshed after DataTables redraws in `TeacherGroupsManager.WebUI/wwwroot/js/datatables-helper.js`.
- Filter controls are localized for Arabic, English, and French:
  - Filters / Filter
  - Search by
  - Apply
  - Reset
  - Reload
  - Close
- DataTables loading, pagination, empty-state, and search text already use the selected language.

## Still missing or recommended

- Browser-level visual verification is still needed for every table page, especially on mobile widths.
- Automated UI tests for table alignment, truncation, hover titles, and filter localization are not present.
- French and Arabic strings should be reviewed by a native speaker because the current project contains existing translation text that may need refinement.

## Main files

- `TeacherGroupsManager.WebUI/wwwroot/css/site.css`
- `TeacherGroupsManager.WebUI/wwwroot/js/datatables-helper.js`

## Feature handling

### Centered table content

The shared CSS targets every `table th` and `table td`, so normal Razor tables and DataTables tables use the same centered alignment. The rule uses `!important` to win over Bootstrap or DataTables alignment utilities.

### Trimmed cell content

Table headers and cells have a maximum width, hidden overflow, and `text-overflow: ellipsis`. Long values remain on one line and display an ellipsis instead of expanding the table.

### Full value on hover

`setTableCellTitles` reads the complete cell text and assigns it to the native HTML `title` attribute. It also reads values from input, select, and textarea controls when a cell has no direct text. The function runs on page load, when DataTables creates a row, and after every DataTables redraw.

### Localized filters

The shared DataTables helper selects Arabic, English, or French from the document language. Filter drawer labels, search placeholders, action buttons, tooltips, loading messages, pagination, and empty-state messages are generated from that language dictionary.

### Responsive behavior

The existing responsive table wrapper keeps wide tables horizontally scrollable on small screens. The mobile media rule reduces cell padding while preserving the centered and trimmed behavior.

## Class Session Attendance and Check-Out

### Applied

- Group session management through `GroupSessionService`
- Session statuses and lifecycle: `Scheduled`, `Started`, `Completed`, and `Cancelled` enum support
- Starting sessions and storing actual start time
- Loading currently active students in the group when a session starts
- Idempotent attendance roster creation
- Student attendance registration
- `Present`, `Late`, `Absent`, `Excused`, and `NotRecorded` statuses
- Check-in time and late-minute calculation with a reusable service grace-period constant of 10 minutes
- Student check-out registration
- `Normal` and `LeftEarly` departure statuses with a 10-minute early-departure threshold
- Manual check-in and check-out methods, with QR and student-code enum values reserved for future support
- Bulk attendance operations through `MarkAsync`
- Monthly payment status lookup in `SessionAttendanceDto`
- Completion converts remaining `NotRecorded` students to `Absent`
- Unique database constraints for sessions and student attendance rows
- EF Core migration `20260730120000_AddGroupSessionsAttendance`
- Automated service tests for session start, idempotency, late attendance, early departure, and completion

### Business rules implemented

- A session requires an existing group.
- Planned end time must be later than planned start time.
- A session can only be started while `Scheduled`.
- A session can only be completed while `Started`.
- Starting a session records `ActualStartTime` and creates one roster row per active group student.
- Repeated start attempts do not create duplicate attendance rows.
- Check-in is allowed only while the session is `Started`.
- Check-in uses `Manual`, calculates actual late minutes, and marks the student `Present` within 10 minutes or `Late` after that.
- Check-out requires a check-in and cannot occur before check-in time.
- Check-out uses `Manual` and marks early departure when it is more than 10 minutes before planned end.
- Attendance and departure statuses remain independent.
- Excused attendance requires an excuse reason.
- Completion marks every remaining `NotRecorded` row as `Absent` and stores `ActualEndTime`.
- A student/session attendance pair is unique at the database level.
- Payment status is read for the session month and year and does not block attendance or check-out.

### Main files

- `TeacherGroupsManager.Core/Entities/GroupSession.cs`
- `TeacherGroupsManager.Core/Entities/StudentSessionAttendance.cs`
- `TeacherGroupsManager.Core/Enums/SessionEnums.cs`
- `TeacherGroupsManager.Data/Context/TeacherGroupsDbContext.cs`
- `TeacherGroupsManager.Services/Services/GroupSessionService.cs`
- `TeacherGroupsManager.Services/Interfaces/IAppServices.cs`
- `TeacherGroupsManager.Dtos/SessionDtos.cs`
- `TeacherGroupsManager.Services.Tests/GroupSessionServiceTests.cs`
- `TeacherGroupsManager.Data/Migrations/20260730120000_AddGroupSessionsAttendance.cs`
- `work around.md`

### Database changes

- New table: `GroupSessions`.
- New table: `StudentSessionAttendances`.
- `GroupSessions` columns include group, session date, planned/actual times, status, topic, notes, cancellation reason, and audit fields.
- `StudentSessionAttendances` columns include session, student, attendance/departure statuses, check-in/check-out data, late minutes, excuse reason, notes, and audit fields.
- Relationships: group to sessions, session to attendance rows, and student to attendance rows.
- Unique index: `GroupSessions(GroupId, SessionDate, PlannedStartTime)`.
- Unique index: `StudentSessionAttendances(GroupSessionId, StudentId)`.
- Migration name: `AddGroupSessionsAttendance`.
- No new seeded statuses or permissions were added; statuses are enums and the permission/UI layer remains incomplete.

### Tests and verification

- Added `GroupSessionServiceTests` covering roster creation/idempotency, late check-in, early departure, and completion-to-absent behavior.
- Total test result: 83 passed, 0 failed, 0 skipped.
- Build result: `dotnet build TeacherGroupsManager.sln --no-restore` succeeded with 0 warnings and 0 errors.
- Browser verification was not performed.
- SQL Server migration application was not performed; migration source was reviewed and the test suite used EF Core InMemory.

### Still missing or recommended

- Sessions controller, Razor pages, group-details Sessions action, attendance/check-out UI, and DataTables initialization are not implemented yet.
- Dedicated session and attendance permissions, role seeding, and backend authorization by assigned teacher/assistant group are not implemented yet.
- Enrollment history with join/leave dates is not present in the existing model; session start currently uses `Student.IsActive` and `GroupId`.
- Cancel-session action and mandatory cancellation reason are not implemented yet.
- Completed-session correction flow, mandatory correction reason, and audit record of old/new values are not implemented yet.
- Concurrency token and explicit transactional session start/completion are not implemented yet.
- Migration application against SQL Server and browser verification remain outstanding.
- Existing localization resources have not yet been extended with session-specific labels and messages.

## Lesson Attendance

### Applied

- Reused the existing `Lesson`, `LessonStudent`, `LessonService`, `LessonsController`, and `Lessons/Attendance.cshtml` workflow.
- Reused the existing `AttendanceStatus` enum and composite lesson/student attendance relationship.
- Existing attendance saves update all submitted lesson students in one service operation and reject student IDs outside the lesson roster.
- Added lesson-month payment status lookup to the attendance view model.
- Added paid, partially paid, and unpaid status display beside each attendance row.
- Added localized Select All and Clear All controls that set attendance to Present or Absent.
- Preserved the existing shared table styling and localization behavior.

### Business rules implemented

- Attendance is recorded against an existing lesson and its assigned `LessonStudent` roster.
- Each student appears once because the existing `(LessonId, StudentId)` composite key is preserved.
- Attendance updates reject incomplete or unrelated student submissions.
- Attendance notes are trimmed and empty notes are stored as null.
- Payment status is read for the lesson group, academic year, month, and year.
- Unpaid students remain eligible for attendance; saving attendance does not create or modify payment records.
- Select All marks all displayed students Present; Clear All marks all displayed students Absent.

### Main files

- `TeacherGroupsManager.Services/Services/LessonService.cs`
- `TeacherGroupsManager.Dtos/LessonDtos.cs`
- `TeacherGroupsManager.WebUI/Views/Lessons/Attendance.cshtml`
- `TeacherGroupsManager.Shared/Localization/SharedResource.resx`
- `TeacherGroupsManager.Shared/Localization/SharedResource.ar.resx`
- `TeacherGroupsManager.Shared/Localization/SharedResource.fr.resx`
- `TeacherGroupsManager.Services.Tests/LessonServiceTests.cs`
- `work around.md`

### Database changes

- No new lesson-attendance table was required.
- Existing `LessonStudents` table and its composite primary key `(LessonId, StudentId)` were reused.
- Existing monthly payment table and uniqueness rules were reused.
- No new migration was created for this lesson-attendance enhancement.

### Tests and verification

- Existing `LessonServiceTests` continue to cover group roster creation, duplicate lesson protection, attendance loading, invalid student rejection, status updates, and note trimming.
- Total test result: 83 passed, 0 failed, 0 skipped.
- Build result: `dotnet build TeacherGroupsManager.sln --no-restore` succeeded with 0 warnings and 0 errors.
- Browser-level attendance verification was not performed.
- Migration application against SQL Server was not performed for this enhancement because no database migration was needed.

### Still missing or recommended

- A student-code column/display is not present in the existing student model; the attendance page currently displays the existing mobile field.
- Fine-grained teacher-to-group ownership enforcement for lessons was not added; the existing `LessonsManage` policy remains the authorization boundary.
- Explicit database transaction and concurrency-token handling for attendance saves were not added.
- Browser verification at desktop/mobile widths and RTL rendering remains outstanding.
