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

CREATE TABLE bl_lb_locks (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    resource    TEXT NOT NULL,
    until       TEXT NOT NULL,
    released_at TEXT,
    created_at  TEXT NOT NULL,
    updated_at  TEXT NOT NULL
);