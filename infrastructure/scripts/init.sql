-- Taller2NET - Initial Database Setup
-- This script runs on first PostgreSQL startup
-- EF Core migrations handle the actual schema creation

-- Ensure the database exists (created by POSTGRES_DB env var)
-- Create additional indexes for performance (EF handles base schema)

\c academic_db;

-- Extension for UUID support (optional, using int PKs in this project)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- The application uses EF Core migrations for schema creation
-- This file can be extended with stored procedures, views, etc.

-- Example view for reporting (will work once EF migrations run)
-- CREATE OR REPLACE VIEW v_student_summary AS ...
-- (Tables are created by EF Core migrations at startup)

SELECT 'Taller2NET database initialized' AS status;
