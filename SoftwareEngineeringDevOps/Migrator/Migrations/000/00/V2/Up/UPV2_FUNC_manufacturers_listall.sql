create or replace function public.manufacturers_listall()
returns setof manufacturers
as $$
	select * from manufacturers;
$$ language sql;
