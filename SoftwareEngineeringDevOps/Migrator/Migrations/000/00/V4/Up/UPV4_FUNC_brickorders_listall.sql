create or replace function public.brickorders_listall()
returns setof brickorders
as $$
	select * from brickorders;
$$ language sql;
