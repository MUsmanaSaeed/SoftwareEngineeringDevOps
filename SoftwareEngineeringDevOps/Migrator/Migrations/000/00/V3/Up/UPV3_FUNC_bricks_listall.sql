create or replace function public.bricks_listall()
returns setof bricks
as $$
	select * from bricks;
$$ language sql;
