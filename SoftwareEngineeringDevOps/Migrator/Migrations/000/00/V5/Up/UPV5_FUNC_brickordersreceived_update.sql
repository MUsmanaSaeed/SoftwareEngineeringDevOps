DROP FUNCTION IF EXISTS public.brickordersreceived_update(bigint, int, timestamptz);

create or replace function public.brickordersreceived_update(
	"Id" bigint,
	"BricksReceived" int,
	"ReceivedDate" timestamptz
)
returns setof brickordersreceived
as $$
	update brickordersreceived
	set bricksreceived = "BricksReceived",
		receiveddate = "ReceivedDate"
	where id = "Id"
	returning *;
$$ language sql;
