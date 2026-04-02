-- Creates app role + database for CLTI.
-- This script runs automatically on first container init (empty volume).

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'clti_app') THEN
    CREATE ROLE clti_app LOGIN PASSWORD 'clti_app_password';
  END IF;
END
$$;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'clti') THEN
    CREATE DATABASE clti OWNER clti_app;
  END IF;
END
$$;
