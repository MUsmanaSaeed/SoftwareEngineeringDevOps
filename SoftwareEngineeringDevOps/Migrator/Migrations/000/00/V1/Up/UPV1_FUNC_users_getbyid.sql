create or replace function public.users_getbyid(
	"Id" bigint
)
returns setof users
as $$
	select * from users 
	where id = "Id";
$$ language sql;