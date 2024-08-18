-- Removes wizden data / blocked wizden servers by default so as to not cause errors

-- Default onto MV servers
INSERT INTO ServerFilter (Category, Data) VALUES (100, 'multiverse_engine');

-- No reason to keep old wizden logins since they just throw errors at this point
DELETE FROM Login;

-- Remove wizden hubs
DELETE FROM Hub WHERE Address="https://hub.spacestation14.com/";
DELETE FROM Hub WHERE Address="https://cdn.spacestationmultiverse.com/wizden-hub-mirror/";
