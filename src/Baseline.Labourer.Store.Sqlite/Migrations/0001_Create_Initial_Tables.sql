CREATE TABLE bl_lb_scheduled_jobs (
    name            TEXT NOT NULL,
    cron_expression TEXT NOT NULL,
    last_completed  TEXT,
    last_run        TEXT,
    next_run        TEXT NOT NULL,
    type            TEXT NOT NULL,
    parameters_type TEXT NOT NULL,
    parameters      TEXT NOT NULL,
    created_at      TEXT NOT NULL,
    updated_at      TEXT NOT NULL
)