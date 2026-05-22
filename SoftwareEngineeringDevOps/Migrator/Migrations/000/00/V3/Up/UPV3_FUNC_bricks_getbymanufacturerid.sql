create or replace function public.bricks_getbymanufacturerid(
	"ManufacturerId" bigint
)
returns setof bricks
as $$
	select * from bricks
	where manufacturerid = "ManufacturerId";
$$ language sql;
