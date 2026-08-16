-- SQL Schema Script for CommunityVoting Application

CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(150) NOT NULL UNIQUE,
    "PhoneNumber" VARCHAR(50),
    "PasswordHash" TEXT NOT NULL,
    "Role" INT NOT NULL DEFAULT 2 -- 0: GlobalAdmin, 1: CommunityAdmin, 2: CommunityMember
);

CREATE TABLE "Communities" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Address" VARCHAR(300) NOT NULL,
    "Cif" VARCHAR(50),
    "CreatedByUserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT
);

CREATE TABLE "CommunityMembers" (
    "Id" UUID PRIMARY KEY,
    "CommunityId" UUID NOT NULL REFERENCES "Communities"("Id") ON DELETE CASCADE,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "MemberRole" INT NOT NULL DEFAULT 2,
    "JoinedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (now() at time zone 'utc'),
    CONSTRAINT "UQ_CommunityMember" UNIQUE ("CommunityId", "UserId")
);

CREATE TABLE "Meetings" (
    "Id" UUID PRIMARY KEY,
    "CommunityId" UUID NOT NULL REFERENCES "Communities"("Id") ON DELETE CASCADE,
    "Title" VARCHAR(250) NOT NULL,
    "Type" INT NOT NULL DEFAULT 0, -- 0: Ordinary, 1: Extraordinary
    "Location" VARCHAR(250) NOT NULL,
    "ScheduledAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "SecondCallAt" TIMESTAMP WITH TIME ZONE,
    "VotingStart" TIMESTAMP WITH TIME ZONE NOT NULL,
    "VotingEnd" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsTransparent" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE "AgendaItems" (
    "Id" UUID PRIMARY KEY,
    "MeetingId" UUID NOT NULL REFERENCES "Meetings"("Id") ON DELETE CASCADE,
    "Title" VARCHAR(300) NOT NULL,
    "Description" TEXT,
    "Order" INT NOT NULL DEFAULT 1
);

CREATE TABLE "Proposals" (
    "Id" UUID PRIMARY KEY,
    "MeetingId" UUID NOT NULL REFERENCES "Meetings"("Id") ON DELETE CASCADE,
    "AgendaItemId" UUID NOT NULL REFERENCES "AgendaItems"("Id") ON DELETE CASCADE,
    "Title" VARCHAR(300) NOT NULL,
    "Description" TEXT,
    "Order" INT NOT NULL DEFAULT 1
);

CREATE TABLE "ProposalOptions" (
    "Id" UUID PRIMARY KEY,
    "ProposalId" UUID NOT NULL REFERENCES "Proposals"("Id") ON DELETE CASCADE,
    "Label" VARCHAR(200) NOT NULL
);

CREATE TABLE "Documents" (
    "Id" UUID PRIMARY KEY,
    "ProposalId" UUID NOT NULL REFERENCES "Proposals"("Id") ON DELETE CASCADE,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "FileName" VARCHAR(255) NOT NULL,
    "StoragePath" TEXT NOT NULL,
    "ContentType" VARCHAR(100) NOT NULL,
    "FileSize" BIGINT NOT NULL,
    "UploadedByUserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE RESTRICT,
    "UploadedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (now() at time zone 'utc')
);
