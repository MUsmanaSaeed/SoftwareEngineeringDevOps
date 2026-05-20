create or replace function public.brickorders_delete(
	"Id" bigint
)
returns void
as $$
	delete from brickorders
	where id = "Id";
$$ language sql;
