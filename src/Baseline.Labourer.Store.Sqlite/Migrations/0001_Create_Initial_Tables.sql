CREATE TABLE bl_lb_scheduled_jobs (
    name              TEXT PRIMARY KEY NOT NULL,
    cron_expression   TEXT NOT NULL,
    last_completed_at TEXT,
    last_run_at       TEXT,
    next_run_at       TEXT NOT NULL,
    type              TEXT NOT NULL,
    parameters_type   TEXT NOT NULL,
    parameters        TEXT NOT NULL,
    created_at        TEXT NOT NULL,
    updated_at        TEXT NOT NULL
);
CREATE INDEX idx_bl_lb_scheduled_jobs_next_run_at ON bl_lb_scheduled_jobs(next_run_at);

CREATE TABLE bl_lb_locks (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    resource    TEXT NOT NULL,
    until       TEXT NOT NULL,
    released_at TEXT,
    created_at  TEXT NOT NULL,
    updated_at  TEXT NOT NULL
);
CREATE INDEX idx_bl_lb_locks_resource ON bl_lb_locks(resource);

CREATE TABLE bl_lb_job_logs (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    job_id     TEXT NOT NULL,
    log_level  TEXT NOT NULL,
    message    TEXT,
    exception  TEXT,
    created_at TEXT NOT NULL
);
CREATE INDEX bl_lb_job_logs_job_id ON bl_lb_job_logs (job_id);