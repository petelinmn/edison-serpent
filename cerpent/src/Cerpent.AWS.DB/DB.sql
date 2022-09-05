--drop table aggregationrules
create sequence aggregationrules_id_seq as integer;
create table aggregationrules
(
    id            integer       default nextval('aggregationrules_id_seq'::regclass)    not null primary key,
    name          text unique                                                           not null,
    atomics       json,
    contextfields text[],
    condition     text,
    timespan      real
);

create sequence stereotypedescriptions_id_seq as integer;
create table stereotypedescriptions
(
    id            integer       default nextval('stereotypedescriptions_id_seq'::regclass)    not null primary key,
    name          text unique                                                                 not null,
    triggerevent  text unique                                                                 not null,
    metrics       jsonb,
    upperbounds   jsonb,
    lowerbounds   jsonb,
    accuracy      jsonb      
);

create sequence events_id_seq as integer;
create table events
(
    id            integer       default nextval('events_id_seq'::regclass)    not null primary key,
    name          text                                                        not null,
    datetime      timestamp                                                   not null,
    data          jsonb                                                       not null
);

create sequence stereotypecheckresults_id_seq as integer;
create table stereotypecheckresults
(
    id                       integer   default nextval('stereotypecheckresults_id_seq'::regclass)   not null primary key,
    stereotypedescriptionid  integer                                                        		not null,
	triggereventid           integer                                                                not null,
    chartresults             jsonb                                                                  not null,
	datetime                 timestamp                                                              not null
);	

ALTER TABLE stereotypecheckresults
ADD CONSTRAINT FK_stereotypecheckresults_stereotypedescriptionid_stereotype_id FOREIGN KEY (stereotypedescriptionid) REFERENCES stereotypedescriptions (id);

ALTER TABLE stereotypecheckresults
ADD CONSTRAINT FK_stereotypecheckresults_triggereventid_event_id FOREIGN KEY (triggereventid) REFERENCES events (id);
