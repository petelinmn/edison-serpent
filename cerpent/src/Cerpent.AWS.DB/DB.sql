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
    upperbounds   json,
    lowerbounds   json,
    accuracy      text      
);