CREATE TABLE brickordersreceived(
  id BIGSERIAL not null primary key,
  brickorderid bigint not null references brickorders(id),
  bricksreceived int not null,
  receiveddate timestamp not null,
  receivedbyid bigint not null references users(id)
);
