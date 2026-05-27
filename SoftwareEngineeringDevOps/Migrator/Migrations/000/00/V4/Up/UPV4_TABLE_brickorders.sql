CREATE TABLE brickorders(
  id BIGSERIAL not null primary key,
  orderno text not null,
  brickid bigint not null references bricks(id),
  bricksordered int not null default 0,
  ordereddate timestamptz not null,
  expecteddate timestamptz not null,
  cancelleddate timestamptz null default null,
  createdbyid bigint not null references users(id)
);
