create or replace function public.brickordersreceived_getbyid(
	"Id" bigint
)
returns setof brickordersreceived
as $$
	select * from brickordersreceived
	where id = "Id";
$$ language sql;
