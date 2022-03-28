CREATE TABLE bl_lb_scheduled_jobs
(
    id                TEXT PRIMARY KEY NOT NULL,
    name              TEXT             NOT NULL,
    cron_expression   TEXT             NOT NULL,
    type              TEXT             NOT NULL,
    parameters_type   TEXT             NOT NULL,
    parameters        TEXT             NOT NULL,
    next_run_at       TEXT,
    last_run_at       TEXT,
    last_completed_at TEXT,
    created_at        TEXT             NOT NULL,
    updated_at        TEXT             NOT NULL
);
CREATE INDEX idx_bl_lb_scheduled_jobs_next_run_at ON bl_lb_scheduled_jobs (next_run_at);

CREATE TABLE bl_lb_locks
(
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    resource    TEXT NOT NULL,
    until       TEXT NOT NULL,
    released_at TEXT,
    created_at  TEXT NOT NULL,
    updated_at  TEXT NOT NULL
);
CREATE INDEX idx_bl_lb_locks_resource ON bl_lb_locks (resource);

CREATE TABLE bl_lb_job_logs
(
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id     TEXT NOT NULL,
    log_level  TEXT NOT NULL,
    message    TEXT,
    exception  TEXT,
    created_at TEXT NOT NULL
);
CREATE INDEX bl_lb_job_logs_job_id ON bl_lb_job_logs (job_id);

CREATE TABLE bl_lb_servers
(
    id         TEXT PRIMARY KEY,
    hostname   TEXT NOT NULL,
    key        TEXT NOT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE bl_lb_server_heartbeats
(
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    server_id  TEXT NOT NULL,
    created_at TEXT NOT NULL
);
CREATE INDEX bl_lb_server_heartbeats_server_id ON bl_lb_server_heartbeats (server_id);

CREATE TABLE bl_lb_workers
(
    id         TEXT PRIMARY KEY,
    server_id  TEXT NOT NULL,
    created_at TEXT NOT NULL
);
CREATE INDEX bl_lb_workers_server_id ON bl_lb_workers (server_id);

CREATE TABLE bl_lb_dispatched_jobs
(
    id              TEXT PRIMARY KEY NOT NULL,
    retries         INT              NOT NULL DEFAULT (0),
    status          INT              NOT NULL,
    type            TEXT             NOT NULL,
    parameters_type TEXT,
    parameters      TEXT,
    finished_at     TEXT,
    created_at      TEXT             NOT NULL,
    updated_at      TEXT             NOT NULL
);