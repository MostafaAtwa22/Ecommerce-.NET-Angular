# Full-Stack Ecommerce Platform

A comprehensive, scalable, and modern E-commerce platform built with **.NET 8** and **Angular 20**. This application provides an end-to-end shopping experience, featuring a dynamic product catalog, real-time chat, advanced rate-limiting, role-based authentication, and a sleek user interface.

## 🚀 Features

*   **Product & Catalog Management:** Browse products, categories, and brands with comprehensive filtering, sorting, and pagination.
*   **Shopping Cart & Orders:** Complete checkout flow with robust cart management.
*   **Coupons & Discounts:** Apply validation-backed discount codes directly during checkout.
*   **Real-time Customer Support:** Built-in live chat feature connecting customers with support using SignalR.
*   **Authentication & Security:** JWT-based authentication, Google Social Login, and Role-Based Access Control (RBAC) (Admin vs. Customer).
*   **Rate Limiting:** Granular, API-level rate limiting configured for different modules (Authentication, Products, Orders, etc.) to ensure high availability.
*   **Admin Dashboard:** Dedicated administration portal for managing users, roles, products, orders, and coupons, complete with dark mode themes.

## 🏗️ Architecture & Services

The application follows a **Clean Architecture** approach with separated layers, ensuring decoupled, maintainable, and testable code. It integrates multiple background and external services to handle complex workflows:

*   **SQL Server & EF Core:** Relational data persistence layer.
*   **Redis / Valkey:** Distributed caching and transient data storage.
*   **Kafka (Aiven):** Event streaming and message processing for asynchronous event-driven communications.
*   **Hangfire:** Background job processing and scheduled tasks.
*   **Stripe:** Secure payment gateway integration.

## 📂 Repository Structure

*   **/Ecommerce.API**: The robust backend system built in ASP.NET Core 8. Includes the Core, Infrastructure, and API presentation layers.
*   **/Ecommerce.Client**: The modern frontend Single Page Application (SPA) built using Angular 20.

---
*For detailed setup instructions, libraries, and design patterns, check out the independent README files in the API and Client directories.*
