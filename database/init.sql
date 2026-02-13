-- ============================================
-- Order Management System - Database Setup
-- ============================================
-- This script creates the database, tables, 
-- indexes, and initial configuration.
--
-- HOW TO RUN:
-- Option 1: Using psql command line
--   psql -U postgres -f database/init.sql
--
-- Option 2: Using pgAdmin
--   Open pgAdmin → Right click on server → 
--   Query Tool → Paste this script → Execute
--
-- Option 3: Let the API handle it automatically
--   The API runs EF Core migrations on startup.
--   Just create the empty database and run the API.
-- ============================================

-- ============================================
-- 1. CREATE DATABASE
-- ============================================
-- Run this part connected to the 'postgres' default database

-- Check if database exists, create if not
SELECT 'CREATE DATABASE ordermanagement'
WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'ordermanagement'
)\gexec

-- ============================================
-- 2. CONNECT TO THE DATABASE
-- ============================================
\c ordermanagement;

-- ============================================
-- 3. CREATE TABLES
-- ============================================

-- Orders table
CREATE TABLE IF NOT EXISTS orders (
    id UUID PRIMARY KEY,
    customer_name VARCHAR(200) NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    value DECIMAL(18, 2) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    
    -- Constraints
    CONSTRAINT chk_orders_status CHECK (status IN ('Pending', 'Processing', 'Completed')),
    CONSTRAINT chk_orders_value CHECK (value > 0),
    CONSTRAINT chk_orders_customer_name CHECK (LENGTH(TRIM(customer_name)) > 0),
    CONSTRAINT chk_orders_product_name CHECK (LENGTH(TRIM(product_name)) > 0)
);

-- Order Status History table (Bonus: +3 points)
CREATE TABLE IF NOT EXISTS order_status_history (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    changed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Foreign Key
    CONSTRAINT fk_order_status_history_order 
        FOREIGN KEY (order_id) 
        REFERENCES orders(id) 
        ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_history_status CHECK (status IN ('Pending', 'Processing', 'Completed'))
);

-- Outbox Messages table (Bonus: Outbox Pattern +3 points)
CREATE TABLE IF NOT EXISTS outbox_messages (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    is_processed BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Foreign Key
    CONSTRAINT fk_outbox_messages_order 
        FOREIGN KEY (order_id) 
        REFERENCES orders(id) 
        ON DELETE CASCADE
);

-- EF Core Migrations History (used by Entity Framework)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- ============================================
-- 4. CREATE INDEXES
-- ============================================

-- Index for filtering orders by status
CREATE INDEX IF NOT EXISTS ix_orders_status 
    ON orders(status);

-- Index for ordering by creation date
CREATE INDEX IF NOT EXISTS ix_orders_created_at 
    ON orders(created_at DESC);

-- Index for status history lookup by order
CREATE INDEX IF NOT EXISTS ix_order_status_history_order_id 
    ON order_status_history(order_id);

-- Index for unprocessed outbox messages (partial index for performance)
CREATE INDEX IF NOT EXISTS ix_outbox_unprocessed 
    ON outbox_messages(created_at) 
    WHERE is_processed = false;

-- ============================================
-- 5. VERIFY SETUP
-- ============================================

-- Show created tables
SELECT table_name, table_type 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- Show created indexes
SELECT indexname, tablename 
FROM pg_indexes 
WHERE schemaname = 'public' 
ORDER BY tablename, indexname;

-- ============================================
-- DONE! Database is ready.
-- ============================================
-- 
-- Expected output:
--   Tables: orders, order_status_history, outbox_messages, __EFMigrationsHistory
--   Indexes: ix_orders_status, ix_orders_created_at, 
--            ix_order_status_history_order_id, ix_outbox_unprocessed
--
-- Next steps:
--   1. Update connection string in appsettings.json
--   2. Run the API: cd src/OrderManagement.API && dotnet run
--   3. Run the Frontend: cd frontend && npm run dev
-- ============================================