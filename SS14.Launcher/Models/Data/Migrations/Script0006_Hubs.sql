CREATE TABLE Hub (
    Address TEXT NOT NULL UNIQUE PRIMARY KEY,
    Priority INTEGER NOT NULL UNIQUE, -- 0 is highest priority

    -- Address can't be empty
    CONSTRAINT AddressNotEmpty CHECK (Address <> ''),
    -- Ensure priority is >= 0
    CONSTRAINT PriorityNotNegative CHECK (Priority >= 0)
);

-- (In multiverse, hubs are reset via c# so they only have to be managed in one location.
-- So the following upstream default is not necessary here as it's included in ConfigDefaults)
-- INSERT INTO Hub (Address, Priority) VALUES ('https://central.spacestation14.io/hub/', 0);
