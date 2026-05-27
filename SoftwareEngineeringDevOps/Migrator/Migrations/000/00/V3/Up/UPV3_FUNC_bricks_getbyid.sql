create or replace function public.bricks_getbyid(
	"Id" bigint
)
returns setof bricks
as $$
	select * from bricks
	where id = "Id";
$$ language sql;
