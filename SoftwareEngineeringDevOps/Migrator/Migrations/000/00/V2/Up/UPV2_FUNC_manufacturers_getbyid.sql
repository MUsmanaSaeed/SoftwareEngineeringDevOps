create or replace function public.manufacturers_getbyid(
	"Id" bigint
)
returns setof manufacturers
as $$
	select * from manufacturers
	where id = "Id";
$$ language sql;
