CREATE TABLE IF NOT EXISTS notifications (
    id UUID PRIMARY KEY,
    channel VARCHAR(16) NOT NULL,
    recipient TEXT NOT NULL,
    subject TEXT,
    message TEXT NOT NULL,
    createdat TIMESTAMP NOT NULL,
    status VARCHAR(16) NOT NULL
);
