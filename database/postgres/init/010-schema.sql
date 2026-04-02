-- Core schema for CLTI (PostgreSQL).
-- Derived from EF Core migrations in CLTI.Diagnosis/Migrations.

\connect clti

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- =========================
-- sys_* tables
-- =========================

CREATE TABLE IF NOT EXISTS sys_enum (
  "Id"                integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Name"              varchar(255) NOT NULL,
  "OrderingType"      varchar(64)  NOT NULL,
  "Guid"              uuid         NOT NULL DEFAULT gen_random_uuid(),
  "OrderingTypeEditor" varchar(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS sys_log (
  "Id"                integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Date"              timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "Thread"            text NULL,
  "Level"             text NULL,
  "Logger"            text NULL,
  "Message"           text NULL,
  "Exception"         text NULL,
  "UserId"            integer NULL,
  "ProcessId"         integer NULL,
  "Logger_namespace"  text NULL
);

CREATE TABLE IF NOT EXISTS sys_rights (
  "Id"           integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Name"         varchar(50) NOT NULL,
  "Description"  text NULL,
  "Guid"         uuid NOT NULL DEFAULT gen_random_uuid()
);

CREATE TABLE IF NOT EXISTS sys_role (
  "Id"           integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Name"         varchar(50) NOT NULL,
  "Description"  text NULL,
  "Guid"         uuid NOT NULL DEFAULT gen_random_uuid()
);

CREATE TABLE IF NOT EXISTS sys_enum_item (
  "Id"        integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "SysEnumId" integer NOT NULL,
  "Name"      varchar(255) NOT NULL,
  "Value"     varchar(255) NULL,
  "Icon"      varchar(64)  NULL,
  "OrderIndex" integer NOT NULL,
  "Guid"      uuid NOT NULL DEFAULT gen_random_uuid(),
  "Color"     varchar(10) NULL,
  CONSTRAINT "FK_sys_enum_item_sys_enum_SysEnumId"
    FOREIGN KEY ("SysEnumId") REFERENCES sys_enum ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_api_key (
  "Id"            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "ApiKey"        varchar(255) NOT NULL,
  "Guid"          uuid NOT NULL DEFAULT gen_random_uuid(),
  "Description"   text NULL,
  "CreatedAt"     timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "ExpiresAt"     timestamptz NULL,
  "StatusEnumItemId" integer NOT NULL,
  CONSTRAINT "FK_sys_api_key_sys_enum_item_StatusEnumItemId"
    FOREIGN KEY ("StatusEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_licence (
  "Id"                 integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Guid"               uuid NOT NULL DEFAULT gen_random_uuid(),
  "LicenceKey"         varchar(255) NOT NULL,
  "LicenceTypeEnumItemId" integer NOT NULL,
  "StartDate"          timestamptz NOT NULL,
  "EndDate"            timestamptz NOT NULL,
  "StatusEnumItemId"   integer NOT NULL,
  "CreatedAt"          timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "UpdatedAt"          timestamptz NULL,
  CONSTRAINT "FK_sys_licence_sys_enum_item_LicenceTypeEnumItemId"
    FOREIGN KEY ("LicenceTypeEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_sys_licence_sys_enum_item_StatusEnumItemId"
    FOREIGN KEY ("StatusEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_user (
  "Id"              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "TitleBeforeName" varchar(50) NULL,
  "FirstName"       varchar(50) NULL,
  "LastName"        varchar(50) NOT NULL,
  "TitleAfterName"  varchar(50) NULL,
  "Password"        varchar(255) NOT NULL,
  "Email"           varchar(100) NOT NULL,
  "CreatedAt"       timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "StatusEnumItemId" integer NOT NULL,
  "PasswordHashType" varchar(20) NULL,
  "Guid"            uuid NOT NULL DEFAULT gen_random_uuid(),
  CONSTRAINT "FK_sys_user_sys_enum_item_StatusEnumItemId"
    FOREIGN KEY ("StatusEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_refresh_token (
  "Id"            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "UserId"        integer NOT NULL,
  "Token"         varchar(500) NOT NULL,
  "CreatedAt"     timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "ExpiresAt"     timestamptz NOT NULL,
  "IsRevoked"     boolean NOT NULL,
  "IsUsed"        boolean NOT NULL,
  "ReplacedByToken" varchar(500) NULL,
  "Guid"          uuid NOT NULL DEFAULT gen_random_uuid(),
  CONSTRAINT "FK_sys_refresh_token_sys_user_UserId"
    FOREIGN KEY ("UserId") REFERENCES sys_user ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_user_role (
  "Id"        integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "SysUserId" integer NOT NULL,
  "SysRoleId" integer NOT NULL,
  CONSTRAINT "FK_sys_user_role_sys_user_SysUserId"
    FOREIGN KEY ("SysUserId") REFERENCES sys_user ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_sys_user_role_sys_role_SysRoleId"
    FOREIGN KEY ("SysRoleId") REFERENCES sys_role ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_role_rights (
  "Id"        integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "SysRoleId" integer NOT NULL,
  "SysRightId" integer NOT NULL,
  CONSTRAINT "FK_sys_role_rights_sys_role_SysRoleId"
    FOREIGN KEY ("SysRoleId") REFERENCES sys_role ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_sys_role_rights_sys_rights_SysRightId"
    FOREIGN KEY ("SysRightId") REFERENCES sys_rights ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS sys_user_licence (
  "Id"             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "Guid"           uuid NOT NULL DEFAULT gen_random_uuid(),
  "UserId"         integer NOT NULL,
  "LicenceId"      integer NOT NULL,
  "AssignedDate"   timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "ExpiryDate"     timestamptz NULL,
  "StatusEnumItemId" integer NOT NULL,
  CONSTRAINT "FK_sys_user_licence_sys_user_UserId"
    FOREIGN KEY ("UserId") REFERENCES sys_user ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_sys_user_licence_sys_licence_LicenceId"
    FOREIGN KEY ("LicenceId") REFERENCES sys_licence ("Id") ON DELETE CASCADE,
  CONSTRAINT "FK_sys_user_licence_sys_enum_item_StatusEnumItemId"
    FOREIGN KEY ("StatusEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE CASCADE
);

-- =========================
-- u_* tables (CLTI domain)
-- =========================

CREATE TABLE IF NOT EXISTS u_clti (
  "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

  "AbiKpi" double precision NOT NULL,
  "FbiPpi" double precision NULL,

  "WifiCriteria_W1" boolean NOT NULL,
  "WifiCriteria_W2" boolean NOT NULL,
  "WifiCriteria_W3" boolean NOT NULL,
  "WifiCriteria_I0" boolean NOT NULL,
  "WifiCriteria_I1" boolean NOT NULL,
  "WifiCriteria_I2" boolean NOT NULL,
  "WifiCriteria_I3" boolean NOT NULL,
  "WifiCriteria_FI0" boolean NOT NULL,
  "WifiCriteria_FI1" boolean NOT NULL,
  "WifiCriteria_FI2" boolean NOT NULL,
  "WifiCriteria_FI3" boolean NOT NULL,

  "ClinicalStageWIfIEnumItemId" integer NOT NULL,
  "CrabPoints" integer NOT NULL,
  "TwoYLE" double precision NOT NULL,

  "GlassCriteria_AidI" boolean NOT NULL,
  "GlassCriteria_AidII" boolean NOT NULL,
  "GlassCriteria_AidA" boolean NOT NULL,
  "GlassCriteria_AidB" boolean NOT NULL,
  "GlassCriteria_Fps" integer NOT NULL,
  "GlassCriteria_Ips" integer NOT NULL,
  "GlassCriteria_Iid" boolean NOT NULL,
  "GlassCriteria_IidI" boolean NOT NULL,
  "GlassCriteria_IidII" boolean NOT NULL,
  "GlassCriteria_IidIII" boolean NOT NULL,
  "GlassCriteria_ImdP0" boolean NOT NULL,
  "GlassCriteria_ImdP1" boolean NOT NULL,
  "GlassCriteria_ImdP2" boolean NOT NULL,

  "Guid" uuid NOT NULL DEFAULT gen_random_uuid(),
  "CreatedAt" timestamptz NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
  "ModifiedAt" timestamptz NULL,
  "IsDeleted" boolean NOT NULL,

  CONSTRAINT "FK_u_clti_sys_enum_item_ClinicalStageWIfIEnumItemId"
    FOREIGN KEY ("ClinicalStageWIfIEnumItemId") REFERENCES sys_enum_item ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS u_clti_photos (
  "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  "CltiCaseId" integer NOT NULL,
  "CltiCaseGuid" uuid NOT NULL,
  "Guid" uuid NOT NULL DEFAULT gen_random_uuid(),
  "CTA" bytea NULL,
  "DSA" bytea NULL,
  "MRA" bytea NULL,
  "US_of_lower_extremity_arteries" bytea NULL,
  CONSTRAINT "FK_u_clti_photos_u_clti_CltiCaseId"
    FOREIGN KEY ("CltiCaseId") REFERENCES u_clti ("Id") ON DELETE CASCADE
);

-- =========================
-- Indexes (including AddPerformanceIndexes)
-- =========================

CREATE INDEX IF NOT EXISTS "IX_sys_api_key_StatusEnumItemId" ON sys_api_key ("StatusEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_sys_enum_item_SysEnumId" ON sys_enum_item ("SysEnumId");
CREATE INDEX IF NOT EXISTS "IX_sys_licence_LicenceTypeEnumItemId" ON sys_licence ("LicenceTypeEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_sys_licence_StatusEnumItemId" ON sys_licence ("StatusEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_sys_refresh_token_UserId" ON sys_refresh_token ("UserId");
CREATE INDEX IF NOT EXISTS "IX_sys_role_rights_SysRightId" ON sys_role_rights ("SysRightId");
CREATE INDEX IF NOT EXISTS "IX_sys_role_rights_SysRoleId" ON sys_role_rights ("SysRoleId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_StatusEnumItemId" ON sys_user ("StatusEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_licence_LicenceId" ON sys_user_licence ("LicenceId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_licence_StatusEnumItemId" ON sys_user_licence ("StatusEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_licence_UserId" ON sys_user_licence ("UserId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_role_SysRoleId" ON sys_user_role ("SysRoleId");
CREATE INDEX IF NOT EXISTS "IX_sys_user_role_SysUserId" ON sys_user_role ("SysUserId");
CREATE INDEX IF NOT EXISTS "IX_u_clti_ClinicalStageWIfIEnumItemId" ON u_clti ("ClinicalStageWIfIEnumItemId");
CREATE INDEX IF NOT EXISTS "IX_u_clti_photos_CltiCaseId" ON u_clti_photos ("CltiCaseId");

-- Performance indexes migration
CREATE UNIQUE INDEX IF NOT EXISTS "IX_sys_user_Email" ON sys_user ("Email");
CREATE INDEX IF NOT EXISTS "IX_sys_user_role_UserId_RoleId" ON sys_user_role ("SysUserId", "SysRoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_sys_refresh_token_Token" ON sys_refresh_token ("Token");
CREATE INDEX IF NOT EXISTS "IX_sys_refresh_token_UserId_Active" ON sys_refresh_token ("UserId", "IsUsed", "IsRevoked");

-- Grant minimal rights to app role
GRANT CONNECT ON DATABASE clti TO clti_app;
GRANT USAGE ON SCHEMA public TO clti_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO clti_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO clti_app;
