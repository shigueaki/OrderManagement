# 📦 Order Management System

A full-stack order management system with real-time updates, built as a technical challenge showcasing modern software architecture patterns.

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3-blue)
![Azure Service Bus](https://img.shields.io/badge/Azure-Service%20Bus-blue)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Running the Project](#-running-the-project)
- [API Documentation](#-api-documentation)
- [Testing](#-testing)
- [Project Structure](#-project-structure)
- [Bonus Features](#-bonus-features)
- [Environment Variables](#-environment-variables)
- [Troubleshooting](#-troubleshooting)

---

## 🎯 Overview

This system allows users to **create**, **list**, and **view orders**. When an order is created, a message is published to **Azure Service Bus** using the **Outbox Pattern** for transactional reliability. A background **Worker** consumes messages, processes orders, and updates their status in real-time via **SignalR WebSockets**.

### Order Status Flow
┌─────────┐     ┌────────────┐     ┌───────────┐ ┌──────────┐ ┌──────────┐ ┌───────────┐
│ Pending │ ──→ │ Processing │ ──→ │ Completed │ │ (create) │ │ (worker) │ │(worker+5s)│ 
└─────────┘     └────────────┘     └───────────┘ └──────────┘ └──────────┘ └───────────┘

---

## 🛠 Tech Stack

<img width="356" height="259" alt="image" src="https://github.com/user-attachments/assets/f5327110-e490-490c-9f46-026e9240d6f2" />

---

### Core Features

    ✅ Create Orders — Form with validation
    ✅ List Orders — Responsive table with status badges
    ✅ View Order Details — Full details with status history timeline
    ✅ Real-time Updates — SignalR WebSocket notifications
    ✅ Async Processing — Worker processes orders via Service Bus



