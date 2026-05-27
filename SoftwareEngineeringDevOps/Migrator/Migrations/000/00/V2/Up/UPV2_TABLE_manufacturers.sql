CREATE TABLE manufacturers(
  id BIGSERIAL not null primary key,
  name text not null UNIQUE,
  address1 text not null,
  address2 text null,
  postcode text not null,
  phoneno text not null,
  email text not null
);
