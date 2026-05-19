CREATE TABLE users(
  id BIGSERIAL not null primary key,
  username text not null UNIQUE,
  password text not null,
  firstname text not null,
  lastname text not null,
  isadmin BOOLEAN not null DEFAULT FALSE,
  iseditor BOOLEAN not null DEFAULT FALSE
);