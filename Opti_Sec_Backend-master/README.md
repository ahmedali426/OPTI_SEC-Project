# Opti-Sec: Biometric Access Control System

**Graduation Project | .NET 9 Web API**

Developed a comprehensive security management backend API that enables organizations to control physical access through biometric authentication. The system manages clients, registered members (with fingerprint and facial recognition data), and gates/entry points while maintaining detailed access logs for security auditing.

## Key Technical Achievements

- **Architecture**: Built a scalable REST API using **.NET 9** with Clean Architecture principles, implementing repository pattern via Entity Framework Core and SQL Server
- **Security**: Implemented JWT authentication with refresh tokens, ASP.NET Identity for role-based access control (RBAC), and secure password policies with account lockout protection
- **Biometric Integration**: Designed data models supporting fingerprint templates and facial recognition image storage for multi-factor authentication workflows
- **Background Processing**: Integrated **Hangfire** for scheduled jobs (e.g., automated reports, notifications) with dedicated job queues and dashboard monitoring
- **Validation & Mapping**: Configured FluentValidation for request validation and Mapster for high-performance object-to-DTO mapping
- **Observability**: Implemented structured logging with **Serilog** and request logging middleware for production monitoring
- **Documentation**: Configured Swagger/OpenAPI with JWT bearer token authentication for seamless API consumer integration
- **File Management**: Built secure file upload service handling member photos and access event images with static file serving

## Core Domain Features

- Multi-tenant client management with user account linking
- Member registration with biometric data capture (fingerprint + face)
- Gate/entry point configuration and assignment
- Real-time access logging with authorization status and image capture
- Email notification service integration via SMTP

## Tech Stack

`.NET 9` `Entity Framework Core` `SQL Server` `JWT Authentication` `ASP.NET Identity` `Hangfire` `Serilog` `FluentValidation` `Mapster` `Swagger/OpenAPI` `MailKit`