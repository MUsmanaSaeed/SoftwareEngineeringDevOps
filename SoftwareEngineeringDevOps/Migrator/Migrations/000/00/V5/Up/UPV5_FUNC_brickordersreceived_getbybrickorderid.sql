create or replace function public.brickordersreceived_getbybrickorderid(
	"BrickOrderId" bigint
)
returns setof brickordersreceived
as $$
	select * from brickordersreceived
	where brickorderid = "BrickOrderId";
$$ language sql;
