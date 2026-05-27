create or replace function public.brickordersreceived_delete(
	"Id" bigint
)
returns void
as $$
	delete from brickordersreceived
	where id = "Id";
$$ language sql;
