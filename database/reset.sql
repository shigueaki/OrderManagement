-- ============================================
-- RESET DATABASE - USE WITH CAUTION!
-- This will DELETE ALL DATA and recreate tables.
-- ============================================

\c ordermanagement;

-- Drop all tables
DROP TABLE IF EXISTS outbox_messages CASCADE;
DROP TABLE IF EXISTS order_status_history CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;

-- Confirm
SELECT 'All tables dropped successfully.' AS result;

-- Now run init.sql to recreate:
-- psql -U postgres -f database/init.sql