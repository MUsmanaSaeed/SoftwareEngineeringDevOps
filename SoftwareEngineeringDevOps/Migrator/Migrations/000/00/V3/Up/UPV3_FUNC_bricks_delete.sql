create or replace function public.bricks_delete(
	"Id" bigint
)
returns void
as $$
	delete from bricks
	where id = "Id";
$$ language sql;
