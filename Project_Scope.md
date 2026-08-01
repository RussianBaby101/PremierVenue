Dougs Hiring
Project Scope:

Event Booking System Name: PremierVenue

Core Idea: 
Clients can browse venues and submit detailed booking requests. Staff review, refine, confirm, and handle payments/contracts.
 
User Flows
Client Side (Public + Portal)
1.	Browse Venues (No login required)
	Search & filter venues (date, capacity, location, type of event, budget, amenities)
	View photos, pricing, availability calendar, rules
	Venue detail page

2.	Submit Booking Request
Fill form with:
	Event type (Wedding, Corporate, Birthday, Conference, etc.)
	Preferred dates + duration (multi-day support)
	Number of guests
	Special requirements (catering, bar, spaces)
	Budget range
	Contact details + notes
	Submit → Gets a Request Reference Number

3.	Client Portal (After request is submitted – Link in navbar)
	Login (via email and password)
	Will use emails + OTPs for forget password
	View all their requests/bookings
	Track status (Pending → Under Review → Confirmed → Paid → Completed)
	View quotes & proposals from staff
	Approve/reject changes
	Make payments (deposit & balance)
	Download contracts & invoices
	Communicate with staff (messages/notes)

Staff Side (Main Admin App)
1.	Dashboard
	New booking requests (with alerts)
	Today’s events & tasks
	Pending payments & confirmations

2.	Review & Convert Requests
	See all incoming requests
	Check real availability
	Edit/refine details (add fees, suggest alternatives, upsell)
	Add internal notes
	Convert to Official Booking
	Send quote/proposal to client

3.	Full Booking Management
	Calendar
	Venue & resource scheduling and Management (CRUD)
	Task checklists
	Document management
	Status updates



 
Full Module Structure 

Public / Client-Facing
	Venue Directory + Search
	Availability Calendar (read-only)
	Booking Request Form
	Client Portal

Staff / Internal System
	Dashboard
	Booking Requests Inbox
	Calendar & Scheduling
	Bookings Management
	Clients & CRM
	Venues & Resources
	Finance & Payments
	Tasks & Operations
	Reports

Shared / System
	Notifications (Email + Push)
	Authentication (Staff full access, Clients limited)
	Audit Log

Tech Stack
Backend
	ASP.NET Core 8 Web API (C#)
	Entity Framework Core
	SQL Server (Azure SQL Database)
	JWT Authentication (with separate Staff & Client roles)
	Hangfire (for background jobs & reminders)
	Fluent Validation (for input validation)
	Stream Chat (Messaging)

Frontend (PWA)
	HTML + JavaScript for mobile support
	Bootstrap 5.3.8 only (no extra component libraries)
	Custom CSS only where Bootstrap is not enough (kept minimal)
	Service Worker + manifest.json for PWA features
    Sweetaelrt2 For Actions

Hosting & DevOps
	Azure for everything:
o	Azure Web App (or Azure Container Apps)
o	Azure SQL Database
o	Azure Blob Storage (for photos, contracts, documents)
	Docker containers (Dockerfile for backend + frontend)
	Azure Container Registry (optional)

Other Tools
	Visual Studio Code/Visual Studio
	Ozow for payments (Instant EFT)
	SendGrid or Azure Communication Services for email/SMS (Gmail App for testing)








The Approach:
	Clients fill out requirements
	Staff manage requests (Acts as the middleman between client, venue and catering)
	Modern and convenient look and feel

Hybrid Strategy:
1.	Main System = PWA
	This is the primary interface (desktop + mobile web).
	Works offline, installable, push notifications, fast loading.
2.	Native Mobile App (Secondary / Wrapper)
	Use Capacitor 
	Take the existing HTML + JavaScript + Bootstrap PWA.
	Add Capacitor to turn it into a native app.

