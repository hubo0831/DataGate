 CREATE TABLESPACE datagate
         LOGGING
        DATAFILE 'c:\sqldata\DataGate_db.DBF'
         SIZE 32M
         AUTOEXTEND ON
         NEXT 32M MAXSIZE UNLIMITED
         EXTENT MANAGEMENT LOCAL;
CREATE USER datagate IDENTIFIED BY datagate 
DEFAULT TABLESPACE datagate
TEMPORARY TABLESPACE temp;

grant dba to datagate;
GRANT CONNECT,RESOURCE TO datagate;
