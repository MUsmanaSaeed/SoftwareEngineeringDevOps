DROP FUNCTION IF EXISTS public.brickordersreceived_insert(bigint, int, timestamp, bigint);

create or replace function public.brickordersreceived_insert(
	"BrickOrderId" bigint,
	"BricksReceived" int,
	"ReceivedDate" timestamp,
	"ReceivedById" bigint
)
returns setof brickordersreceived
as $$
	insert into brickordersreceived(brickorderid, bricksreceived, receiveddate, receivedbyid)
	values ("BrickOrderId", "BricksReceived", "ReceivedDate", "ReceivedById")
	returning *;
$$ language sql;
