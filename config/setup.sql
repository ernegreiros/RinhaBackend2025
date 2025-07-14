CREATE UNLOGGED TABLE payment (
    correlationId UUID NOT NULL,
    amount DECIMAL NOT NULL,
    service VARCHAR(50) NOT NULL,
    createdOn TIMESTAMP NOT NULL
);

CREATE INDEX payment_createdOn ON payment (createdOn);