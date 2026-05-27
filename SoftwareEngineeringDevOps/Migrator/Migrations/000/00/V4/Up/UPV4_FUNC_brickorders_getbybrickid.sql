create or replace function public.brickorders_getbybrickid(
	"BrickId" bigint
)
returns setof brickorders
as $$
	select * from brickorders
	where brickid = "BrickId";
$$ language sql;
