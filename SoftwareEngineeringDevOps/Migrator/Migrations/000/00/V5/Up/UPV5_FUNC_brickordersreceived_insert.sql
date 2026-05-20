DROP FUNCTION IF EXISTS public.brickordersreceived_insert(bigint, int, timestamp);

create or replace function public.brickordersreceived_insert(
	"BrickOrderId" bigint,
	"BricksReceived" int,
	"ReceivedDate" timestamp
)
returns setof brickordersreceived
as $$
	insert into brickordersreceived(brickorderid, bricksreceived, receiveddate)
	values ("BrickOrderId", "BricksReceived", "ReceivedDate")
	returning *;
$$ language sql;
