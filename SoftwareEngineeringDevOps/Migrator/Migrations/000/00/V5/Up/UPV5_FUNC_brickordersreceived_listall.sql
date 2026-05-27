create or replace function public.brickordersreceived_listall()
returns setof brickordersreceived
as $$
	select * from brickordersreceived;
$$ language sql;
