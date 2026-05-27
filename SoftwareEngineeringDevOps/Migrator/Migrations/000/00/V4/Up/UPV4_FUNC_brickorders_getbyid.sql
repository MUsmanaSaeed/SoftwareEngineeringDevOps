create or replace function public.brickorders_getbyid(
	"Id" bigint
)
returns setof brickorders
as $$
	select * from brickorders
	where id = "Id";
$$ language sql;
