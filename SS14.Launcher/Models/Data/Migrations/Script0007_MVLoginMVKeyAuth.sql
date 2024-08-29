-- Adds key auth support

CREATE TABLE "LoginMVKey" (
	"UserName"	TEXT NOT NULL,
	"PublicKey"	TEXT NOT NULL,
	"PrivateKey"	TEXT NOT NULL,
	PRIMARY KEY("PublicKey")
);
