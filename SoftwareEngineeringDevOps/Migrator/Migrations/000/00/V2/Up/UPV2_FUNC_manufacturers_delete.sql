create or replace function public.manufacturers_delete(
	"Id" bigint
)
returns void
as $$
	delete from manufacturers
	where id = "Id";
$$ language sql;
