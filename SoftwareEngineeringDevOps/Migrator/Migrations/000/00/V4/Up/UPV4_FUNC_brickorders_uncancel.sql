create or replace function public.brickorders_uncancel(
	"Id" bigint
)
returns setof brickorders
as $$
	update brickorders
	set cancelleddate = null
	where id = "Id"
	returning *;
$$ language sql;
