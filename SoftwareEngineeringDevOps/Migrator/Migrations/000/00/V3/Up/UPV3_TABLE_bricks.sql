CREATE TABLE bricks(
  id BIGSERIAL not null primary key,
  name text not null UNIQUE,
  manufacturerid bigint not null references manufacturers(id),
  price numeric(10, 2) not null default 0,
  colour text not null,
  material text not null,
  strength numeric(8,4) not null default 0,
  width numeric(6,1) not null default 0,
  height numeric(6,1) not null default 0,
  depth numeric(6,1) not null default 0,
  type text not null,
  voids numeric(5,4) not null default 0
);
