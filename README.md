# PremierVenue

PremierVenue is a venue discovery and event-booking platform. It provides a public venue catalogue, authenticated client portal, staff operations portal, booking and quote workflow, venue administration, document handling, payment/refund tracking, reporting, and interactive venue maps.

The system is built with an ASP.NET Core 8 Web API, Entity Framework Core, SQL Server, and a responsive vanilla JavaScript/Bootstrap PWA frontend.

## System capabilities

### Public venue discovery

- Browse active venues in a responsive catalogue.
- Search and filter venues by name, city, capacity, budget, event type, and sorting option.
- View venue cards with pricing, capacity, location, gallery images, event types, and amenities.
- Switch between list and interactive map views.
- Select a venue marker to open venue details.
- View a venue detail page with:
  - Image gallery and primary image
  - Venue description
  - Capacity and daily pricing
  - Event types
  - Amenity and feature chips
  - Exact map location
- Contact the business through the public contact page.

### Authentication and account management

- Client registration and login.
- JWT access-token authentication.
- Role-based authorization for `Client`, `Staff`, and `Admin` users.
- Password reset request and password reset flows.
- Staff invitation acceptance flow.
- Central profile management for authenticated users.
- Staff and admin user administration.
- Secure role-protected staff portal pages and API endpoints.

### Client portal

- Client dashboard with booking activity and account information.
- Browse and search available venues.
- Save and remove favourite venues.
- View saved venues.
- Submit a booking request containing:
  - Venue
  - Event type
  - Start and end dates
  - Guest count
  - Budget
  - Additional services
  - Special requirements
- View personal bookings through the authenticated `/api/bookings/my` endpoint.
- View booking status, dates, venue, quote, payment information, and documents.
- Accept or reject a staff quote.
- Download quotes, invoices, proof-of-payment documents, and other booking PDFs.
- View bookings in a calendar.
- View venue availability and unavailable booked dates.
- Update the client profile.

### Staff and admin portal

- Staff dashboard with operational metrics and pending work.
- Manage venues and venue availability.
- Manage venue event types and amenities.
- Configure per-venue service options that appear in the client request flow.
- Manage clients and users.
- Review incoming booking requests.
- Prepare and send quotes.
- Upload quote, invoice, proof-of-payment, and other booking documents.
- Track booking status transitions.
- Record deposit-paid and fully-paid transactions through booking status changes.
- Process cancellations and calculate refunds according to the booking's stored policy.
- Track cancellation fees when the client has not paid enough to cover a cancellation.
- Lock cancelled, rejected, and quote-rejected bookings so no further status changes or uploads are allowed.
- Auto-complete confirmed, deposit-paid, and fully-paid bookings after the event date passes.
- View financial reports with gross collected, refunds, and net collected revenue.
- View bookings on a staff calendar.
- Manage staff profile information.

## Venue management

### Venue details

Venues support:

- Name and description
- Address, city, province, and postal code
- Latitude and longitude
- Interactive map-selected location
- Capacity
- Base daily price
- Active/inactive status
- Event types
- Standard amenities
- Custom amenities
- Supported service options for client requests
- Multiple gallery images
- Primary/thumbnail image

### Venue images

Staff and admins can select multiple images from their device when creating or editing a venue.

Supported functionality includes:

- Multi-file image selection
- Local image previews before saving
- Primary image selection
- Existing image gallery during edit
- Change the primary image later
- Delete existing images
- Database-backed image binary storage for the current implementation
- Image content served through an authenticated-aware API route
- File type and file-size validation

The current image storage is deliberately abstracted around venue photo records so it can later be replaced with Azure Blob Storage or another object-storage provider without changing the venue management workflow.

### Venue amenities and features

Standard amenities are selected with tickboxes in the staff venue form. Current standard options include:

- Parking
- WiFi
- Catering
- AV System
- Security
- Stage
- Outdoor Space
- Bar
- Decor
- Air Conditioning

Admins and staff can also add any number of custom amenities, such as:

- Swimming pool
- Generator
- Bridal suite
- Private entrance
- Fireworks area

Custom amenities are stored with the venue and displayed alongside standard amenities. Standard amenities use mapped Bootstrap icons. Custom amenities use a blue checkmark icon. All amenity icons are rendered in blue on venue cards and detail pages.

### Venue maps and geocoding

- Leaflet map integration using OpenStreetMap tiles.
- Admins can click the map to place a venue marker.
- The marker can be dragged for precise positioning.
- Latitude and longitude fields are updated from the marker.
- Reverse geocoding attempts to populate address, city, province, and postal code from a selected pin.
- Forward geocoding attempts to locate coordinates from the entered address fields.
- Clients can browse all located venues on a map.
- Venue detail pages show the selected venue's exact location.

### Venue service options

Staff can configure the service options that appear as checkboxes when a client submits a booking request. Default options include:

- Catering
- Staffing & security
- Setup & cleanup

Admins and staff can also add custom service options (up to 30, each up to 100 characters). Supported services are stored with the venue and returned in the venue details used by the client request form.

### Venue availability and booked dates

Each venue can have explicit availability records with a date, availability flag, and notes. Staff can manually mark dates as unavailable. The availability calendar also considers active bookings; any date covered by a confirmed, deposit-paid, fully-paid, or pending booking is automatically marked as unavailable with a "Booked" note. Cancelled, rejected, and quote-rejected bookings are ignored when calculating availability.

### Venue search and sorting

The public venue catalogue and search API support filtering by search term (name, city, description), minimum capacity, minimum and maximum daily price, event type, and city. Sort options include `price-asc`, `price-desc`, `capacity`, `newest`, and `name` (default). Date-range filtering fields exist in the search contract for future availability-based search.

## Booking and quote workflow

1. A client submits a booking request.
2. The API validates the request and checks for date conflicts.
3. Staff reviews the request and prepares a quote.
4. Staff selects a cancellation policy and can attach quote documents.
5. The client accepts or rejects the quote.
6. Staff records deposit and full-payment progress through booking statuses.
7. Documents can be attached and downloaded throughout the workflow.
8. Staff can cancel the booking, triggering the configured refund calculation.
9. Reports calculate net collected revenue after refunds.

### Booking status lifecycle

Bookings move through the following statuses:

- **Pending** — Request received, awaiting staff review.
- **Quoted** — Staff has prepared and sent a quote.
- **QuoteAccepted** — Client accepted the quote.
- **QuoteRejected** — Client rejected the quote.
- **Confirmed** — Booking is confirmed by staff.
- **DepositPaid** — Deposit has been received.
- **FullyPaid** — Full amount has been received.
- **Completed** — Event has ended; set automatically.
- **Cancelled** — Booking cancelled by staff, refund calculated.
- **Rejected** — Staff rejected the initial request.

Terminal statuses are `Cancelled`, `Rejected`, and `QuoteRejected`. Bookings in a terminal status cannot be modified, quoted, uploaded to, or have their status changed. `Completed` is also terminal from a mutation standpoint and is set automatically by a background service (every 30 minutes) and on every booking retrieval; it moves confirmed, deposit-paid, and fully-paid bookings to completed once the event end date has passed. Staff cannot manually set a booking to `Completed`.

The staff request page enforces a logical status dropdown so only the current status and the next likely statuses can be selected:

- **Pending** → Quoted, Rejected, Cancelled
- **Quoted** → QuoteAccepted, QuoteRejected, Cancelled
- **QuoteAccepted** → Confirmed, Cancelled
- **Confirmed** → DepositPaid, Cancelled
- **DepositPaid** → FullyPaid, Cancelled
- **FullyPaid** → Cancelled

The API itself accepts any non-terminal status and rejects `Completed` directly.

### Booking conflict rules

A new or updated booking is rejected with HTTP `409 Conflict` if its dates overlap an existing booking that is not cancelled, rejected, or quote-rejected.

Venue availability calendars also mark dates from active booking records as unavailable.

### Cancellation policies and refunds

Each quoted booking stores its cancellation policy code and policy text. The current policies include:

- **Standard**: refund percentage depends on the number of days remaining before the event.
- **Full deposit refund**: refundable deposit according to the policy.
- **Non-refundable deposit**: deposit is not refunded.

Cancellation tracking stores:

- Cancellation policy code
- Cancellation timestamp
- Refund amount
- Refund status
- Cancellation fee amount
- Cancellation fee status
- Cancellation fee due date, when applicable

Payment records are created for deposit and full-payment status transitions. Refund records are created when a cancellation refund is calculated.

If a cancellation happens before enough money has been collected, the system keeps the refund and the cancellation-fee obligation separate. That means a booking can be cancelled with no deposit paid, and the record will still show that no refund is due while preserving the fee balance for client follow-up or deactivation workflows.

The current lifecycle is:

1. Staff changes the booking to cancelled.
2. The system totals completed non-refund payments.
3. Previous refunds are excluded from the refundable base.
4. The stored cancellation policy is applied using the days remaining before the event.
5. Refund amount and cancellation-fee amount are stored independently.
6. Cancelled and rejected bookings become terminal and cannot be edited further.

> The system records the refund decision and amount. Any external banking or payment-provider transfer must still be completed through the configured payment process.

### Refund calculation details

When a booking is cancelled, the refund is calculated as follows:

1. The refundable base is the total of completed non-refund payments minus any previous refunds.
2. The cancellation policy is resolved from the quote:
   - **FullRefund**: 100% of the refundable base.
   - **NoRefund**: 0% of the refundable base.
   - **Standard**:
     - 100% if the event is more than 30 days away.
     - 50% if 15–30 days away.
     - 25% if 7–14 days away.
     - 0% if less than 7 days away.
3. The refund amount is `refundable base × percentage`, rounded to two decimals.
4. The cancellation fee percentage is the remainder (`100% − refund percentage`). The cancellation fee is calculated against the deposit amount and tracked separately. If the client has not paid enough to cover the fee, the outstanding balance is recorded with a due date seven days from cancellation.
5. A completed refund payment record is created with a `REFUND-{ReferenceNumber}` transaction reference.

## Documents and payments

- Booking documents are stored and served through the document API.
- Supported workflows include quote, invoice, proof of payment, and other PDF documents.
- Staff can upload documents to a booking.
- Clients and authorized staff can download attached documents.
- Docker uses a persistent `premiervenue-uploads` volume for uploaded booking files.
- Venue image binary data is currently stored in SQL Server.
- Payment records are associated with bookings and payment status.
- Reports subtract completed refunds from collected payment totals.

### Document types and permissions

Supported document types are `Contract`, `Invoice`, `Quote`, `Receipt`, `Insurance`, `ProofOfPayment`, and `Other`. Staff can upload `Quote`, `Invoice`, `Contract`, `Receipt`, `Insurance`, and `Other` documents. Clients can only upload `ProofOfPayment`. Document uploads are blocked once a booking is in a terminal status (`Cancelled`, `Rejected`, `QuoteRejected`). Only PDF files up to 10 MB are accepted.

### Payment records

Status transitions create payment records automatically:

- `DepositPaid` adds a completed `Deposit` payment for the quoted deposit amount, with reference `POP-{ReferenceNumber}-DEPOSIT`.
- `FullyPaid` adds a completed `FullPayment` payment for the outstanding balance after completed non-refund payments, with reference `POP-{ReferenceNumber}-BALANCE`.
- Cancellations add a completed `Refund` payment for the calculated refund amount, with reference `REFUND-{ReferenceNumber}`.

Payment types are `Deposit`, `FullPayment`, `PartialPayment`, and `Refund`. Payment statuses are `Pending`, `Processing`, `Completed`, `Failed`, and `Refunded`.

## Reporting

Staff reports include:

- Booking totals
- Booking status breakdowns
- Quote totals
- Payment totals
- Refund totals
- Net collected revenue
- Date-range and status filtering where supported

Net collected revenue is calculated from completed payment records minus completed refunds. Legacy bookings without payment records use booking totals as a compatibility fallback.

Cancellation-fee balances are tracked separately from refunds so staff can distinguish money owed back to the client from money still owed by the client.

### Report details

The staff reporting dashboard includes a six-month revenue chart with a month-by-month breakdown, a booking mix visualization showing completed, cancelled, and open percentages, and a detailed history table with reference numbers, venue, client, date range, collected amount, and status.

## Architecture

### Backend layers

- **PremierVenue.API**: HTTP controllers, authentication, authorization, middleware, and application startup.
- **PremierVenue.Core**: DTOs, validators, business services, and application logic.
- **PremierVenue.Domain**: Entities, enums, interfaces, and domain models.
- **PremierVenue.Infrastructure**: EF Core `DbContext`, repositories, the initial SQL schema, data seeding, and infrastructure services.

### Frontend

- Vanilla JavaScript and semantic HTML.
- Bootstrap 5.3.8 for layout and components.
- Bootstrap Icons for interface and amenity icons.
- Leaflet 1.9.4 for interactive maps.
- SweetAlert2 for confirmation and feedback dialogs.
- Responsive, mobile-first page layouts.
- PWA manifest and service worker support.
- Nginx container for frontend serving.

## Technology stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server 2022 / Azure SQL
- JWT authentication
- FluentValidation
- Repository and unit-of-work patterns
- SQL Server initial schema script (`src/PremierVenue.Infrastructure/Data/initial.sql`)

### Frontend

- HTML5
- CSS3
- Vanilla JavaScript
- Bootstrap 5.3.8
- Bootstrap Icons
- Leaflet
- PWA manifest and service worker

### Container and cloud targets

- Docker and Docker Compose
- Nginx
- Azure Container Apps or App Service
- Azure SQL Database
- Azure Blob Storage as the future venue/document storage target

### Planned or external integrations

The solution includes configuration points for integrations that may be enabled by deployment:

- Ozow or another payment provider
- SMTP, SendGrid, or Azure Communication Services
- Gmail API for email testing
- Stream Chat or another messaging provider
- Hangfire/background processing


## API overview

The API is rooted at `/api`.

### Authentication

- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/refresh`
- `GET /api/auth/invitation`
- `POST /api/auth/accept-invitation`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

### Users

- `GET /api/users/me`
- `PUT /api/users/me`
- Staff/admin user administration endpoints under `/api/users`

### Venues

- `GET /api/venues`
- `GET /api/venues/{id}`
- `POST /api/venues/search`
- `POST /api/venues`
- `PUT /api/venues/{id}`
- `PATCH /api/venues/{id}/toggle-status`
- `DELETE /api/venues/{id}`
- `POST /api/venues/{id}/photos`
- `GET /api/venues/{venueId}/photos/{photoId}/content`
- `PATCH /api/venues/{venueId}/photos/{photoId}/primary`
- `DELETE /api/venues/{venueId}/photos/{photoId}`

### Saved venues

- `GET /api/saved-venues`
- `GET /api/saved-venues/{venueId}/exists`
- `POST /api/saved-venues/{venueId}`
- `DELETE /api/saved-venues/{venueId}`

### Bookings

- `POST /api/bookings`
- `GET /api/bookings/my`
- `GET /api/bookings/{id}`
- `GET /api/bookings/reference/{referenceNumber}`
- Staff booking list and pending-request endpoints under `/api/bookings`
- `PATCH /api/bookings/{id}/status`
- `POST /api/bookings/quote`
- `POST /api/bookings/{id}/quote-decision`

### Documents

Booking document upload and download endpoints are available under `/api/documents` and are protected according to booking ownership and staff authorization.

## Project structure

```text
PremierVenue/
├── src/
│   ├── PremierVenue.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   ├── PremierVenue.Core/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Utilities/
│   ├── PremierVenue.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── ValueObjects/
│   └── PremierVenue.Infrastructure/
│       ├── Data/
│       │   └── initial.sql
│       ├── Repositories/
│       └── Services/
├── frontend/
│   ├── pages/
│   │   ├── client/           # Authenticated client portal pages
│   │   ├── staff/             # Staff and admin portal pages
│   │   └── public/            # Public pages (login, register, venues, contact, etc.)
│   ├── assets/
│   │   ├── css/
│   │   ├── js/
│   │   │   ├── client/        # Client portal page logic
│   │   │   ├── public/        # Public pages and authentication flows
│   │   │   ├── shared/        # Common utilities, API client, and shared components
│   │   │   └── staff/         # Staff and admin portal page logic
│   │   └── images/
│   ├── index.html
│   ├── manifest.json
│   ├── sw.js
│   ├── nginx.conf
│   └── Dockerfile
├── tests/
│   └── PremierVenue.Tests/   # Domain, API controller, and database tests
├── .github/
│   └── workflows/ci.yml      # GitHub Actions build and test workflow
├── docker-compose.yml
├── .env
├── .gitignore
└── README.md
```

## Getting started

### Prerequisites

- .NET 8 SDK
- SQL Server 2022, SQL Server Express, LocalDB, or Docker SQL Server
- Docker Desktop for the containerized setup
- A modern browser
- Node.js is optional; the frontend does not require a Node build step

## Security

- JWT authentication for protected API operations.
- Role-based authorization for client, staff, and admin capabilities.
- Password hashing through the authentication implementation.
- FluentValidation for request validation.
- EF Core parameterized data access.
- CORS configuration.
- File type and file-size validation for uploads.
- Booking ownership checks for client resources.
- No secrets should be stored in source control.

## Current project status

- [x] Layered ASP.NET Core API structure
- [x] EF Core and SQL Server persistence
- [x] JWT authentication and role authorization
- [x] Client registration, login, password reset, and invitations
- [x] Client and staff profile persistence
- [x] Public venue discovery and search
- [x] Saved venues
- [x] Venue management and active/inactive status
- [x] Event-type tickbox management
- [x] Standard amenity tickbox management
- [x] Repeatable custom venue amenities
- [x] Amenity icons on public and client venue pages
- [x] Multiple venue images and primary image selection
- [x] Database-backed venue image storage
- [x] Interactive admin venue map selection
- [x] Client/public venue browsing maps
- [x] Address-to-map and map-to-address geocoding
- [x] Booking request workflow
- [x] Booking date conflict enforcement
- [x] Quote creation and client quote decisions
- [x] Booking PDF upload/download workflow
- [x] Payment status records
- [x] Cancellation policies and refund calculation
- [x] Net collected revenue reporting
- [x] Docker Compose local environment
- [x] Initial domain, API controller, and database automated tests
- [ ] Expand automated API, authorization, and SQL Server integration coverage
- [ ] Replace current database image storage with production blob storage
- [ ] Production deployment hardening and monitoring

## Demo accounts

Development seed accounts may be available depending on the active database seed configuration. Never use development credentials in production.

```text
Admin: admin@premiervenue.com / Password123!
Staff: staff@premiervenue.com / Password123!
Client: client@premiervenue.com / Password123!
```

## Support

For internal support, follow the project's support and issue-reporting process.

## License

This project is proprietary software. All rights reserved.
