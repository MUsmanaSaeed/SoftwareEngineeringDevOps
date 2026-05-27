create or replace function public.users_delete(
	"Id" bigint
)
returns void
as $$
	delete from users
	where id = "Id";
$$ language sql;