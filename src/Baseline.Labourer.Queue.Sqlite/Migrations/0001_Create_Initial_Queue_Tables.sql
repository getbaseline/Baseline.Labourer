CREATE TABLE bl_lb_queue
(
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    message      TEXT NOT NULL,
    hidden_until TEXT,
    created_at   TEXT NOT NULL
);
CREATE INDEX idx_bl_lb_queue_hidden_until ON bl_lb_queue (hidden_until);